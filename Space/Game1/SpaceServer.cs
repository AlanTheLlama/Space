using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceServer {
    public class SpaceServer : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        NetPeerConfiguration config;
        NetServer server;
        NetIncomingMessage message;
        NetOutgoingMessage mOut;
        SpriteFont font;
        List<PlayerData> playerLocations;

        TcpListener tcpListen;
        TcpListener listener;
        Thread listenThread;
        
        Texture2D startButton;

        Rectangle sBRect;

        string[] splitter;
        char[] deliminators = { ',', ' ', '/' };
        string serverStatus;
        bool running;

        public SpaceServer() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            this.IsMouseVisible = true;
            /*config = new NetPeerConfiguration("Squad");
            config.EnableUPnP = true;
            config.LocalAddress = IPAddress.Parse("192.168.1.244");
            config.Port = 31579;
            //server = new NetServer(config);

            tcpListen = new TcpListener(IPAddress.Parse("192.168.1.244"), 31579);
            listenThread = new Thread(new ThreadStart(PutEarToDoor));
            listenThread.Start();

            System.Diagnostics.Debug.WriteLine("sys: " + listenThread.IsAlive);
            */
            base.Initialize();

            IPAddress IP = IPAddress.Parse("192.168.1.244");

            listener = new TcpListener(IP, 31579);

            System.Diagnostics.Debug.WriteLine("Starting listener");
            listener.Start();
            running = true;
            System.Diagnostics.Debug.WriteLine("Started listener, looping through clients");

            Thread listenerThread = new Thread(new ThreadStart(() => LoopClients()));
            listenerThread.Start();
            System.Diagnostics.Debug.WriteLine("Started thread");
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            System.Diagnostics.Debug.WriteLine("SpriteBatch started");
            font = Content.Load<SpriteFont>("File");

            startButton = Content.Load<Texture2D>("startButton");

            sBRect = new Rectangle(10, GraphicsDevice.PresentationParameters.Bounds.Height - 30, 50, 20);

            playerLocations = new List<PlayerData>();
        }
        
        protected override void UnloadContent() {
            
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            var mState = Mouse.GetState();
            var mPos = new Point(mState.X, mState.Y);

            /*if (sBRect.Contains(mPos) && mState.LeftButton == ButtonState.Pressed && server.Status != NetPeerStatus.Starting && server.Status != NetPeerStatus.Running) {
                //server.Start();
            }*/

            /*System.Diagnostics.Debug.WriteLine("Listening for client");
            TcpClient newClient = listener.AcceptTcpClient();
            System.Diagnostics.Debug.WriteLine("Accepted client at " + newClient.Client.ToString());

            Thread t = new Thread(new ParameterizedThreadStart(TalkToTheHand));
            t.Start(newClient);
            Socket s = listener.AcceptSocket();

            System.Diagnostics.Debug.WriteLine("Socket accepted from: " + s.RemoteEndPoint);

            byte[] b = new byte[512];
            int k = s.Receive(b);

            for (int i = 0; i < k; i++) {
                System.Diagnostics.Debug.Write(Convert.ToChar(b[i]));
            }

            s.Close();
            */
            //serverStatus = "Server Status: " + server.ConnectionsCount;
            //System.Diagnostics.Debug.WriteLine(serverStatus);

            /*while ((message = server.ReadMessage()) != null) {           //Reciever
                switch (message.MessageType) {
                    case NetIncomingMessageType.Data:
                        splitter = new string[4] { "0", "1", "2", "3"};
                        splitter = message.ReadString().ToString().Split(deliminators);

                        //System.Diagnostics.Debug.WriteLine("ID REPORTED: " + splitter[3]);

                        bool exists = false;
                        for(int i = 0; i < playerLocations.Count; i++) {
                            if(playerLocations[i].getID().ToString().Equals(splitter[3])) {
                                playerLocations[i].setCoords(float.Parse(splitter[0]), float.Parse(splitter[1]), float.Parse(splitter[2]));
                                exists = true;
                            }
                        }
                        if (!exists) {
                            System.Diagnostics.Debug.WriteLine("PLOC SIZE: " + playerLocations.Count);
                            playerLocations.Add(new PlayerData(
                                float.Parse(splitter[0]),
                                float.Parse(splitter[1]),
                                float.Parse(splitter[2]),
                                int.Parse(splitter[3])));
                        }
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        
                        break;

                    case NetIncomingMessageType.DebugMessage:
                        System.Diagnostics.Debug.WriteLine(message.ReadString());
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine("unhandled message with type: "
                            + message.MessageType);
                        break;
                }
            }
            */
            /*foreach(PlayerData pd in playerLocations) {          //Sender
                mOut = server.CreateMessage();
                mOut.Write(pd.dataString());
                server.SendToAll(mOut, NetDeliveryMethod.ReliableOrdered);
            }*/

            //PutEarToDoor();
            base.Update(gameTime);
        }

        public void PutEarToDoor() {
            this.tcpListen.Start();
            while (true) {
                System.Diagnostics.Debug.WriteLine("Listening");
                TcpClient client = this.tcpListen.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(TalkToTheHand));
                clientThread.Start(client);
            }
        }

        public void LoopClients() {
            System.Diagnostics.Debug.WriteLine("LoopClients");
            while (running) {
                System.Diagnostics.Debug.WriteLine("Waiting for client..");
                
                TcpClient newClient = listener.AcceptTcpClient();
                System.Diagnostics.Debug.WriteLine("Found client");
                
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }

        public void HandleClient(object obj) {
            TcpClient client = (TcpClient)obj;
            System.Diagnostics.Debug.WriteLine("Handling client");

            StreamWriter sWriter = new StreamWriter(client.GetStream(), Encoding.ASCII);
            StreamReader sReader = new StreamReader(client.GetStream(), Encoding.ASCII);

            Boolean bClientConnected = true;
            String sData = null;

            while (bClientConnected) {
                System.Diagnostics.Debug.WriteLine("Hi there!");
                sData = null;

                while (sData == null) {
                    System.Diagnostics.Debug.WriteLine("Loooooping");
                    sData = sReader.ReadLine();
                    System.Diagnostics.Debug.WriteLine(sData);
                }

                System.Diagnostics.Debug.WriteLine("sData: " + sData);
                splitter = new string[4] { "0", "1", "2", "3" };
                splitter = sData.Split(deliminators);

                System.Diagnostics.Debug.WriteLine("ID REPORTED: " + splitter[3]);

                bool exists = false;
                System.Diagnostics.Debug.WriteLine("Splitter length: " + splitter.Length);
                System.Diagnostics.Debug.WriteLine("PLAYER LIST SIZE: " + playerLocations.Count);
                for (int i = 0; i < playerLocations.Count; i++) {
                    if (playerLocations[i].getID().ToString().Equals(splitter[3])) {
                        playerLocations[i].setCoords(float.Parse(splitter[0]), float.Parse(splitter[1]), float.Parse(splitter[2]));
                        exists = true;
                    }
                }
                if (!exists) {
                    System.Diagnostics.Debug.WriteLine("PLAYER LIST SIZE: " + playerLocations.Count);
                    playerLocations.Add(new PlayerData(
                        float.Parse(splitter[0]),
                        float.Parse(splitter[1]),
                        float.Parse(splitter[2]),
                        int.Parse(splitter[3])));
                }

                foreach (PlayerData pd in playerLocations) {
                    sWriter.WriteLine(pd.dataString());
                    sWriter.Flush();
                }
            }
        }

        public void TalkToTheHand(object client) {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();

            byte[] buffer = encoder.GetBytes("Server sends message to client!");
            byte[] message = new byte[4096];
            int bytesRead;

            while (true) {
                bytesRead = 0;
                try {
                    bytesRead = clientStream.Read(message, 0, 4096);
                } catch {
                    System.Diagnostics.Debug.WriteLine("Neighbour was an asshole. ");
                    break;
                }

                if (bytesRead == 0) {
                    System.Diagnostics.Debug.WriteLine("Neighbour ditched us. ");
                    break;
                }

                //message has successfully been received
                System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
            }

            tcpClient.Close();
        }
        
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            int i = 0;
            foreach(PlayerData pd in playerLocations) {
                spriteBatch.DrawString(font, pd.outputAsString(), new Vector2(10, (i * 20)), Color.Black);
                i++;
            }

            //text
            spriteBatch.DrawString(font, "Status: " + playerLocations.Count, new Vector2(10, 10), Color.White);
            
            //buttons
            spriteBatch.Draw(startButton, sBRect, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
