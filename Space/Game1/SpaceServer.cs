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
        SpriteFont font;
        List<PlayerData> playerLocations;

        string[] splitter;
        char[] deliminators = { ',', ' ', '/' };
        string serverStatus;

        public SpaceServer() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            config = new NetPeerConfiguration("Squad");
            config.Port = 31579;
            server = new NetServer(config);

            server.Start();

            playerLocations = new List<PlayerData>();

            base.Initialize();
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("File");
        }
        
        protected override void UnloadContent() {
            
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            serverStatus = "Server Status: " + server.ConnectionsCount;
            //System.Diagnostics.Debug.WriteLine(serverStatus);
            
            while ((message = server.ReadMessage()) != null) {
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
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
