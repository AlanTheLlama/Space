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

    public class StateObject {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public TcpClient newClient = new TcpClient();
    }

    public class SpaceServer : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        NetPeerConfiguration config;
        NetServer server;
        NetIncomingMessage message;
        NetOutgoingMessage mOut;
        SpriteFont font;
        List<PlayerData> playerLocations;
        List<int> IDS;

        TcpListener tcpListen;
        Socket listener;
        Thread listenThread;
        
        Texture2D startButton;

        Rectangle sBRect;

        string[] splitter;
        char[] deliminators = { ',', ' ', '/', ';' };
        string serverStatus;
        bool running;

        public SpaceServer() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            this.IsMouseVisible = true;

            IPAddress IP = IPAddress.Parse("192.168.1.244");

            listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(new IPEndPoint(IP, 31579));
            listener.Listen(100);

            //System.Diagnostics.Debug.WriteLine("Starting listener");
            //running = true;
            //Thread listenerThread = new Thread(new ThreadStart(() => LoopClients()));
            System.Diagnostics.Debug.WriteLine("Started Listener Successfully");

            base.Initialize();
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            System.Diagnostics.Debug.WriteLine("SpriteBatch started");
            font = Content.Load<SpriteFont>("File");

            startButton = Content.Load<Texture2D>("startButton");

            sBRect = new Rectangle(10, GraphicsDevice.PresentationParameters.Bounds.Height - 30, 50, 20);

            playerLocations = new List<PlayerData>();
            IDS = new List<int>() { 0 };
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

            TcpClient newClient;
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

            base.Update(gameTime);
        }

        public void PutEarToDoor() {
            this.tcpListen.Start();
            while (true) {
                System.Diagnostics.Debug.WriteLine("Listening");
                TcpClient client = this.tcpListen.AcceptTcpClient();

                //Thread clientThread = new Thread(new ParameterizedThreadStart(TalkToTheHand));
                //clientThread.Start(client);
            }
        }

        /*public void LoopClients() {
            System.Diagnostics.Debug.WriteLine("LoopClients");
            while (running) {
                System.Diagnostics.Debug.WriteLine("Waiting for client..");
                
                TcpClient newClient = listener.AcceptTcpClient();
                System.Diagnostics.Debug.WriteLine("Found client");
                
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }*/

        /*public void HandleClient(object obj) {
            TcpClient client = (TcpClient)obj;
            System.Diagnostics.Debug.WriteLine("Handling client");

            StreamWriter sWriter = new StreamWriter(client.GetStream(), Encoding.ASCII);
            StreamReader sReader = new StreamReader(client.GetStream(), Encoding.ASCII);

            Boolean bClientConnected = true;
            String sData = null;

            while (bClientConnected) {
                System.Diagnostics.Debug.WriteLine("Hi there!");
                sData = null;

                while (client.Available == 0) {
                    System.Diagnostics.Debug.WriteLine("Loooooping");
                    sData = sReader.ReadLine();
                    System.Diagnostics.Debug.WriteLine("WHILE sData: " + sData);
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

            sWriter.Close();
            sReader.Close();
        }*/

        public void AcceptCallback(IAsyncResult ar) {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;
            try {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(TalkToTheHand), state);
            }catch (SocketException se) {
                System.Diagnostics.Debug.WriteLine("Client connected but did not ping!\nERROR: " + se);
            }
        }

        private void DeleteImposters() {
            for (int i = 0; i < playerLocations.Count - 1; i++) {
                int checker = playerLocations[i].getID();
                if (playerLocations.Count > 1) {
                    for (int k = i + 1; k < playerLocations.Count - 1; i++) {
                        if (playerLocations[k].getID() == checker) {
                            playerLocations.RemoveAt(k);
                            System.Diagnostics.Debug.WriteLine("IMPOSTER NULLIFIED");
                        }
                    }
                }
            }
        }

        private void Truncate(string[] str, int pos) {
            for(int i = pos; i < str.Length; i++) {
                str[i] = null;
            }
        }

        public void TalkToTheHand(IAsyncResult ar) {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            ASCIIEncoding encoder = new ASCIIEncoding();

            System.Diagnostics.Debug.WriteLine("SERVER: Count of " + playerLocations.Count);
            byte[] buffer = encoder.GetBytes("Server recieves message!");
            byte[] message = new byte[4096];
            int bytesRead = handler.EndReceive(ar);
            String content = String.Empty;

            if(bytesRead > 0) {
                state.sb.Append(encoder.GetString(state.buffer, 0, bytesRead));

                content = state.sb.ToString();

                if (content.IndexOf("<EOF>") > -1) {
                    System.Diagnostics.Debug.WriteLine("SERVER: Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    
                    System.Diagnostics.Debug.WriteLine("SERVER-RECIEVED: RECIEVED: " + Content);
                } else {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(TalkToTheHand), state);
                }

                state.sb.Clear();

                String rec = content;
                if (rec.Length > 1) System.Diagnostics.Debug.WriteLine("REC READ AS: " + rec);
                splitter = new string[4] { "0", "1", "2", "3" };
                if(rec.Length > 4)splitter = rec.Split(deliminators);
                List<string> splitterList = new List<string>{ splitter[0], splitter[1], splitter[2], splitter[3] };

                try {
                    if (rec.Length > 2) {
                        bool exists = false;

                        for (int i = 0; i < playerLocations.Count - 1; i++) {
                            if (playerLocations[i].getID().ToString().Equals(splitterList[3])) {
                                playerLocations[i].setCoords(float.Parse(splitterList[0]), float.Parse(splitterList[1]), float.Parse(splitterList[2]));
                                System.Diagnostics.Debug.WriteLine("SERVER: UPDATED DATA OF PLAYER " + splitterList[3]);
                                exists = true;
                            }
                        }

                        for (int i = 0; i < playerLocations.Count - 1; i++) {
                            if (playerLocations[i].getID() == int.Parse(splitterList[3])) {
                                exists = true;
                                //System.Diagnostics.Debug.WriteLine("MATCH FOUND VIA INT CHECK");
                            }
                        }

                        if (!exists && splitterList[3] != null && !IDS.Contains(int.Parse(splitterList[3]))) {
                            IDS.Add(int.Parse(splitterList[3]));
                            try {
                                System.Diagnostics.Debug.WriteLine("SERVER: ADDING MOVINGOBJECT TO REGISTRY: " + (playerLocations.Count + 1) + "\n" + splitterList[3]);
                                playerLocations.Add(new PlayerData(
                                    float.Parse(splitterList[0]),
                                    float.Parse(splitterList[1]),
                                    float.Parse(splitterList[2]),
                                    int.Parse(splitterList[3])));
                            } catch (System.IndexOutOfRangeException exc) {
                                System.Diagnostics.Debug.WriteLine("SERVER: FAILED TO ADD TO REGISTRY" + "\n    REC:" + rec + "\nSERVER-EXCEPTION: " + exc);
                            }
                        }

                        DeleteImposters();
                    }
                }catch (System.FormatException ex) {
                    System.Diagnostics.Debug.WriteLine("SERVER: Could not process due to FormatException.");
                }
                    //Send all player information back to clients
                for (int i = 0; i < playerLocations.Count - 1; i++) {
                    byte[] b = encoder.GetBytes(playerLocations[i].dataString());
                    //clientStream.Write(b, 0, b.Length);
                    System.Diagnostics.Debug.WriteLine("SERVER: Sending PlayerData no. " + i + "\n        " + playerLocations[i].dataString());
                }
            }
            
            //System.Diagnostics.Debug.WriteLine("Past while loop ");

            //tcpClient.Close();
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
