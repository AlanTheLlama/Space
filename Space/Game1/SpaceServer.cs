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
using MenuLib;

namespace SpaceServer {

    public class StateObject {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
        public TcpClient newClient = new TcpClient();
    }

    public class SpaceServer : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        List<PlayerData> playerLocations;
        List<int> IDS;
        Button startButton;

        Socket listener;
        Texture2D startButtonTex;

        Rectangle sBRect;

        string[] splitter;
        char[] deliminators = { ',', ' ', '/' };
        char terminator = ';';

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

            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

            System.Diagnostics.Debug.WriteLine("Started Listener Successfully");

            base.Initialize();
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            System.Diagnostics.Debug.WriteLine("SpriteBatch started");
            font = Content.Load<SpriteFont>("File");

            startButton = new Button(Content.Load<Texture2D>("startButton"), "Start", 10, GraphicsDevice.PresentationParameters.Bounds.Height - 30, 50, 20);

            playerLocations = new List<PlayerData>();
            IDS = new List<int>() { 0 };
        }

        protected override void UnloadContent() {

        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (startButton.Clicked()) System.Diagnostics.Debug.WriteLine("Clicked!");

            base.Update(gameTime);
        }

        public void AcceptCallback(IAsyncResult ar) {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

            StateObject state = new StateObject();
            state.workSocket = handler;
            try {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(TalkToTheHand), state);
            } catch (SocketException se) {
                System.Diagnostics.Debug.WriteLine("Client connected but did not ping!\nERROR: " + se);
            }
        }

        public void TalkToTheHand(IAsyncResult ar) {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            ASCIIEncoding encoder = new ASCIIEncoding();

            int bytesRead = handler.EndReceive(ar);
            String content = String.Empty;

            System.Diagnostics.Debug.WriteLine("SERVER-RECIEVED: bytes: " + bytesRead);
            if (bytesRead > 0) {                
                if (bytesRead > 48) bytesRead = 48;
                state.sb.Append(encoder.GetString(state.buffer, 0, bytesRead));

               // if(!state.sb.ToString().Contains(";"))
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(TalkToTheHand), state);
            }

            content = state.sb.ToString();
            state.sb.Clear();

            String rec = content;
            int pos = 0;
            splitter = new string[4] { "0", "1", "2", "3" };  //failsafe before anything even happens

            System.Diagnostics.Debug.WriteLine("REC: " + rec);
            rec = rec.Split(terminator)[0];
            if (rec.Length > 4) splitter = rec.Split(deliminators); //failsafe #1
            if (splitter.Length == 4) { //failsafe #2
                List<string> splitterList = new List<string> { splitter[0], splitter[1], splitter[2], splitter[3] }; //failsafe #3
                try {
                    if (rec.Length > 2) {
                        bool exists = false;
                        for (int i = 0; i < playerLocations.Count; i++) {
                            if (playerLocations[i].getID() == int.Parse(splitterList[3])) {
                                exists = true;
                                pos = i;
                            }
                        }
                        if (exists) {
                            playerLocations[pos].setCoords(float.Parse(splitterList[0]), float.Parse(splitterList[1]), float.Parse(splitterList[2]));
                            System.Diagnostics.Debug.WriteLine("SERVER: UPDATED DATA OF PLAYER " + splitterList[3]);
                        }

                        if (!exists && splitterList[3] != null && !IDS.Contains(int.Parse(splitterList[3]))) {
                            IDS.Add(int.Parse(splitterList[3]));
                            try {
                                System.Diagnostics.Debug.WriteLine("SERVER: ADDING MOVINGOBJECT TO REGISTRY: " + (playerLocations.Count + 1) + "\nID:" + splitterList[3]);
                                playerLocations.Add(new PlayerData(
                                    float.Parse(splitterList[0]),
                                    float.Parse(splitterList[1]),
                                    float.Parse(splitterList[2]),
                                    int.Parse(splitterList[3])));
                            } catch (System.IndexOutOfRangeException exc) {
                                System.Diagnostics.Debug.WriteLine("SERVER: FAILED TO ADD TO REGISTRY" + "\n    REC:" + rec + "\nSERVER-EXCEPTION: " + exc);
                            }
                        }
                    }
                } catch (System.FormatException ex) {
                    System.Diagnostics.Debug.WriteLine("SERVER: Could not process due to FormatException.");
                }
                //}
            }
            System.Diagnostics.Debug.WriteLine("SERVER: past while with list size " + playerLocations.Count);
            //Send all player information back to clients
            /*for (int i = 0; i < playerLocations.Count; i++) {
                byte[] b = encoder.GetBytes(playerLocations[i].dataString());
                System.Diagnostics.Debug.WriteLine("SERVER: Sending PlayerData no. " + i + "\n        " + playerLocations[i].dataString());

                handler.BeginSend(b, 0, b.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }*/

            byte[] b = encoder.GetBytes(combineInfo());

            handler.BeginSend(b, 0, b.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        public string combineInfo() {
            string s = "";
            for (int i = 0; i < playerLocations.Count; i++) s = s + playerLocations[i].dataString();
            return s;
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            int i = 0;
            try {
                foreach (PlayerData pd in playerLocations) {
                    spriteBatch.DrawString(font, pd.outputAsString(), new Vector2(10, (i * 20)), Color.Black);
                    i++;
                }
            } catch (InvalidOperationException e) {
                System.Diagnostics.Debug.WriteLine(e);
            }

            //text
            spriteBatch.DrawString(font, "Status: " + playerLocations.Count, new Vector2(10, 10), Color.White);

            //buttons
            spriteBatch.Draw(startButton.tex, startButton.bounds, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
