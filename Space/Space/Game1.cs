﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Space {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        Viewport viewport;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Camera cam;

        NetPeerConfiguration config;
        NetIncomingMessage mail;
        NetOutgoingMessage msg;
        NetClient client;
        //TCP
        TcpClient clientTCP;
        DnsEndPoint serverEP;
        IPHostEntry ipHostInfo;

        AI bob;

        public float MAX_SPEED = 9;

        public World world;

        static public List<MovingObject> movingObjects;
        char[] deliminators = { ',', ' ', '/' };
        string[] splitter;
        bool found;

        public static Texture2D ship;
        public static Texture2D testTile;
        public static Texture2D asteroid;
        public static Texture2D enemy;

        public static PlayerShip player;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            world = new World(15000, 15000);
            world.Generate(600, 1000, 50);

            config = new NetPeerConfiguration("Squad");
            client = new NetClient(config);
            config.EnableUPnP = true;

            //TCP
            clientTCP = new TcpClient();
            serverEP = new DnsEndPoint("frankensquad.zapto.org", 31579);
            ipHostInfo = new IPHostEntry();
            var res = Dns.GetHostEntry("frankensquad.zapto.org").AddressList;

            System.Diagnostics.Debug.WriteLine(res);

            System.Diagnostics.Debug.WriteLine(ipHostInfo.AddressList);
            
            //END TCP

            client.Start();
            client.Connect(host: "207.216.252.138", port: 31579);

            base.Initialize();
        }

        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            movingObjects = new List<MovingObject>();

            //textures
            ship = Content.Load<Texture2D>("Images/ship");
            testTile = Content.Load<Texture2D>("Images/tile");
            asteroid = Content.Load<Texture2D>("Images/asteroid");
            enemy = Content.Load<Texture2D>("Images/enemy");
            font = Content.Load<SpriteFont>("File");

            player = new PlayerShip(new Vector2(world.SizeX / 2, world.SizeY / 2));
            movingObjects.Add(player);

            bob = new AI(7500, 7500);

            viewport = GraphicsDevice.Viewport;
            cam = new Camera(viewport, player);
        }

        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        //apologies for mild overcomplication on movementlol
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            bool keyW = false;   //these are necessary for angling the sprites when two keys are pressed,
            bool keyA = false;   //because the Keyboard.GetState() function can only handle one key
            bool keyS = false;   //at a time
            bool keyD = false;
            bool keyQ = false;
            bool keyE = false;
            bool click = false;

            if (Keyboard.GetState().IsKeyDown(Keys.W)) {
                keyW = true;
            } else keyW = false;

            if (Keyboard.GetState().IsKeyDown(Keys.S)) {
                keyS = true;
            } else keyS = false;

            if (Keyboard.GetState().IsKeyDown(Keys.A)) {
                keyA = true;
            } else keyA = false;

            if (Keyboard.GetState().IsKeyDown(Keys.D)) {
                keyD = true;
            } else keyD = false;

            if (Keyboard.GetState().IsKeyDown(Keys.Q)) {
                keyQ = true;
            } else keyQ = false;

            if (Keyboard.GetState().IsKeyDown(Keys.E)) {
                keyE = true;
            } else keyE = false;

            if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
                click = true;
            } else click = false;

            if (keyW == true) {
                player.thrust();
            }

            if (keyS == true) {
                player.brake();
            }

            if (keyD == true) {
                player.rotateRight();
            }

            if (keyA == true) {
                player.rotateLeft();
            }

            if (keyQ == true) {
                player.leftThrust();
            }

            if (keyE == true) {
                player.rightThrust();
            }
            
            if (click == true) {
                movingObjects.Add(player.fireWeapon(new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y)));
            }

            foreach (MovingObject mo in movingObjects) {
                mo.update(world);
            }

            bob.nearby();
            //System.Diagnostics.Debug.WriteLine("Bob's Location: " + bob.pos.X.ToString() + ", " + bob.pos.Y.ToString());
            sendToServer(player);
            checkMail();
            //push!
            if (Keyboard.GetState().IsKeyDown(Keys.Enter)) client.Disconnect("Disconnected");

            cam.UpdateCamera(viewport);
            base.Update(gameTime);
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Viewport = viewport;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, null, null, null, null, cam.Transform);

            foreach (SpaceObject obj in World.spaceObjects) {
                spriteBatch.Draw(obj.getImage(),
                    new Rectangle(obj.getXpos(), obj.getYpos(), 50, 50),
                    Color.White);
                //System.Diagnostics.Debug.WriteLine("Asteroid co-ord: " + obj.getXpos() + ", " + obj.getYpos() + ". ");
            }

            int i = 0;
            foreach (MovingObject mo in movingObjects) {
                if (mo.getType() == 0) {
                    spriteBatch.Draw(ship,
                        new Rectangle((int)mo.getPos().X, (int)mo.getPos().Y, ship.Width, ship.Height),
                        null,
                        Color.White,
                        mo.getAngle() + (float)0.5 * (float)Math.PI,
                        new Vector2(ship.Width / 2, ship.Height / 2),
                        SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, movingObjects.Count.ToString() + ", " + movingObjects[i].getID().ToString(), new Vector2(-50, i * 20), Color.Black);
                    i++;
                }
            }

            //drawing some tiles to represent camera/ship movement against something that stays still
            spriteBatch.Draw(testTile, new Rectangle(60, 60, 10, 10), Color.White);
            spriteBatch.Draw(testTile, new Rectangle(15, -50, 10, 10), Color.White);
            spriteBatch.Draw(testTile, new Rectangle(200, 0, 10, 10), Color.White);
            spriteBatch.Draw(testTile, new Rectangle(100, -300, 10, 10), Color.White);
            spriteBatch.Draw(asteroid, new Rectangle(50, 50, 50, 50), Color.White);
            spriteBatch.Draw(asteroid, new Rectangle(-50, 50, 100, 110), Color.White);

            spriteBatch.Draw(enemy, bob.pos, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
        
        public void sendToServer(PlayerShip ps) {
            msg = client.CreateMessage();
            msg.Write(ps.dataString());
            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public void checkMail() {
            while ((mail = client.ReadMessage()) != null) {
                switch (mail.MessageType) {
                    case NetIncomingMessageType.Data:
                        splitter = new string[4] { "0", "1", "2", "3" };
                        splitter = mail.ReadString().ToString().Split(deliminators);

                        found = false;
                        for (int i = 0; i < movingObjects.Count; i++) {
                            if (movingObjects[i].getType() == 0) {
                                if (movingObjects[i].getID().ToString().Equals(splitter[3]) && splitter[3].Equals(player.getID().ToString()) == false) {
                                    movingObjects[i].setCoords(float.Parse(splitter[0]), float.Parse(splitter[1]), float.Parse(splitter[2]));
                                    found = true;
                                }
                            }
                        }

                        if (!found && splitter[3].Equals(player.getID().ToString()) == false) movingObjects.Add(new PlayerShip(
                              new Vector2(float.Parse(splitter[0]), float.Parse(splitter[1])), float.Parse(splitter[2]),
                              int.Parse(splitter[3])));

                        break;
                    case NetIncomingMessageType.StatusChanged:
                        break;
                    case NetIncomingMessageType.DebugMessage:
                        System.Diagnostics.Debug.WriteLine(mail.ReadString());
                        break;
                    default:
                        //System.Diagnostics.Debug.WriteLine("SPAM MAIL: "
                        //    + mail.MessageType);
                        break;
                }
            }
        }
    }
}
