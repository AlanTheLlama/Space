using System;
using System.Collections.Generic;
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
        Thread listenThread;

        Texture2D startButton;

        Rectangle sBRect;

        string[] splitter;
        char[] deliminators = { ',', ' ', '/' };
        string serverStatus;

        public SpaceServer() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            this.IsMouseVisible = true;
            config = new NetPeerConfiguration("Squad");
            config.EnableUPnP = true;
            config.LocalAddress = IPAddress.Parse("192.168.1.244");
            config.Port = 31579;
            server = new NetServer(config);

            tcpListen = new TcpListener(IPAddress.Any, 31579);
            listenThread = new Thread(new ThreadStart(PutEarToDoor));
            listenThread.Start();

            System.Diagnostics.Debug.WriteLine("sys: " + config.LocalAddress + "\next: " + config.BroadcastAddress);

            base.Initialize();
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
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

            if(sBRect.Contains(mPos) && mState.LeftButton == ButtonState.Pressed && server.Status != NetPeerStatus.Starting && server.Status != NetPeerStatus.Running) {
                server.Start();
            }

            if(server.Status == NetPeerStatus.Running) {

            }

            serverStatus = "Server Status: " + server.ConnectionsCount;
            //System.Diagnostics.Debug.WriteLine(serverStatus);
            
            while ((message = server.ReadMessage()) != null) {           //Reciever
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

            foreach(PlayerData pd in playerLocations) {          //Sender
                mOut = server.CreateMessage();
                mOut.Write(pd.dataString());
                server.SendToAll(mOut, NetDeliveryMethod.ReliableOrdered);
            }

            base.Update(gameTime);
        }

        public void PutEarToDoor() {
            this.tcpListen.Start();
            while (true) {
                TcpClient client = this.tcpListen.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(TalkToTheHand));
                clientThread.Start(client);
            }
        }

        public void TalkToTheHand(object client) {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

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
                ASCIIEncoding encoder = new ASCIIEncoding();
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
            spriteBatch.DrawString(font, "Status: " + server.Status.ToString(), new Vector2(10, 10), Color.White);
            
            //buttons
            spriteBatch.Draw(startButton, sBRect, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
