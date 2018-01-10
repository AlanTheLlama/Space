using System;
using System.Collections.Generic;
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
            config.Port = 31579;
            server = new NetServer(config);

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
