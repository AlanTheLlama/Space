﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Space {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>

    public class StateObject {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        Viewport viewport;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Camera cam;

        NetPeerConfiguration config;
        NetIncomingMessage mail;
        NetOutgoingMessage msg;
        //NetClient client;
        //TCP
        TcpClient clientTCP;
        TcpClient client;
        IPEndPoint serverEP;
        IPHostEntry ipHostInfo;

        StreamReader _sReader;
        StreamWriter _sWriter;

        AI bob;

        public float MAX_SPEED = 9;

        public World world;

        public static List<MovingObject> movingObjects;
        char[] deliminators = { ',', ' ', '/', ';'};
        string[] splitter;
        bool found;

        public static Texture2D ship;
        public static Texture2D testTile;
        public static Texture2D asteroid;
        public static Texture2D enemy;
        public static Texture2D laserTex;

        bool connected = false;

        public static PlayerShip player;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            world = new World(15000, 15000);
            world.Generate(600, 1000, 50);

            this.IsMouseVisible = true;

            config = new NetPeerConfiguration("Squad");
            //client = new NetClient(config);
            config.EnableUPnP = true;

            //TCP
            /*clientTCP = new TcpClient();
            serverEP = new IPEndPoint(IPAddress.Parse("207.216.252.138"), 31579);
            ipHostInfo = new IPHostEntry();*/
            //var res = Dns.GetHostEntry("frankensquad.zapto.org").AddressList;
            //System.Diagnostics.Debug.WriteLine("HOST ENTRY:" + res);

            //System.Diagnostics.Debug.WriteLine(ipHostInfo.AddressList);
            
            //END TCP

            //client.Start();
            //client.Connect(host: "207.216.252.138", port: 31579);

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
            laserTex = Content.Load<Texture2D>("Images/laser");

            player = new PlayerShip(new Vector2(world.SizeX / 2, world.SizeY / 2));
            movingObjects.Add(player);

            bob = new AI(7500, 7500);

            viewport = GraphicsDevice.Viewport;
            cam = new Camera(viewport, player);
        }

        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            if(Keyboard.GetState().IsKeyDown(Keys.Enter) && connected == false) {
                connectToServer();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W) == true) {
                player.thrust();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S) == true) {
                player.reverse();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D) == true) {
                player.rotateRight();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A) == true) {
                player.rotateLeft();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Q) == true) {
                player.leftThrust();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E) == true) {
                player.rightThrust();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space) == true) {
                player.brake();
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
                Laser l = player.fireWeapon(new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y));
                if (l != null) {
                    movingObjects.Add(l);
                }
            }

            bob.nearby();
            bob.updatePosition(world);

            foreach (MovingObject mo in movingObjects) {
                mo.update(world);
            }
            //sendToServer(player);
            //checkMail();
            //push!
            //if (Keyboard.GetState().IsKeyDown(Keys.Enter)) client.Disconnect("Disconnected");

            if (connected) updateLocation(player);
            cam.UpdateCamera(viewport);
            base.Update(gameTime);

            /*if (!clientTCP.Connected) {
                System.Diagnostics.Debug.WriteLine("Attempting to connect to 207.216.252.138...");
                try {
                    clientTCP.Connect(serverEP);
                } catch (System.Net.Sockets.SocketException ex) {
                    System.Diagnostics.Debug.WriteLine("Failed to connect: " + ex);
                }
            } else if (clientTCP.Connected) System.Diagnostics.Debug.WriteLine("Connected!");*/
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Viewport = viewport;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, null, null, null, null, cam.Transform);

            foreach (SpaceObject obj in World.spaceObjects) {        //static objects
                spriteBatch.Draw(obj.getImage(),
                    new Rectangle(obj.getXpos(), obj.getYpos(), 50, 50),
                    Color.White);
            }

            int i = 0;
            foreach (MovingObject mo in movingObjects) {             //moving objects (duh)
                spriteBatch.Draw(mo.GetTexture(),
                    new Rectangle((int)mo.getPos().X, (int)mo.getPos().Y, mo.GetTexture().Width, mo.GetTexture().Height),
                    null,
                    Color.White,
                    mo.getAngle() + (float)0.5 * (float)Math.PI,
                    new Vector2(mo.GetTexture().Width / 2, mo.GetTexture().Height / 2),
                    SpriteEffects.None, 0);
                spriteBatch.DrawString(font, movingObjects.Count.ToString() + ", " + movingObjects[i].getID().ToString(), new Vector2(-50, i * 20), Color.Black);
                i++;
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
        
        public void connectToServer() {
            try {
                client = new TcpClient();
                ASCIIEncoding encoder = new ASCIIEncoding();

                client.Connect("192.168.1.244", 31579);
                connected = true;
                //System.Diagnostics.Debug.WriteLine("Connected");
            } catch {
                System.Diagnostics.Debug.WriteLine("Connection Failed");
                connected = false;
            }
        }

        public void updateLocation(MovingObject mo) {
            ASCIIEncoding encoder = new ASCIIEncoding();
            Stream writer = client.GetStream();
            NetworkStream clientStream = client.GetStream();

            byte[] buffer = encoder.GetBytes(player.dataString());
            byte[] message = new byte[4096];
            int bytesRead;

            //System.Diagnostics.Debug.WriteLine("CLIENT: Writing buffer ");
            writer.Write(buffer, 0, buffer.Length);

            //System.Diagnostics.Debug.WriteLine("CLIENT: Entering while ");
            bytesRead = 0;
            if (clientStream.DataAvailable) {
                while (true || encoder.GetString(message, 0, bytesRead).Split(deliminators).Length <= 4) {
                    bytesRead = 0;
                    try {
                        //System.Diagnostics.Debug.WriteLine("CLIENT: Attempting to find paper ");
                        bytesRead = clientStream.Read(message, 0, 4096);
                        //System.Diagnostics.Debug.WriteLine("CLIENT: Read the morning paper, page #" + bytesRead);
                        String rec = encoder.GetString(message, 0, bytesRead);
                        System.Diagnostics.Debug.WriteLine("CLIENT-RECIEVED-PRELIM: " + rec);
                    } catch {
                        System.Diagnostics.Debug.WriteLine("CLIENT: Neighbour was an asshole. ");
                        break;
                    }
                    if (bytesRead == 0) {
                        System.Diagnostics.Debug.WriteLine("CLIENT: Neighbour ditched us. ");
                        break;
                    }
                }
            }
            if (bytesRead > 0) {
                String rec = encoder.GetString(message, 0, bytesRead);
                System.Diagnostics.Debug.WriteLine("CLIENT-RECIEVED: " + rec);
            }
            writer.Flush();
        }

        public void HandleCommunication() {
            _sReader = new StreamReader(client.GetStream(), Encoding.ASCII);
            _sWriter = new StreamWriter(client.GetStream(), Encoding.ASCII);

            //connected = true;
            String sData = null;
            while (connected) {
                String sDataIncoming = _sReader.ReadLine(); //recieving
                splitter = new string[4] { "0", "1", "2", "3" };
                splitter = sDataIncoming.Split(deliminators);

                System.Diagnostics.Debug.WriteLine("CLIENT-RECIEVE: " + sDataIncoming);

                found = false;
                for (int i = 0; i < movingObjects.Count; i++) {
                    if (movingObjects[i].getID().ToString().Equals(splitter[3]) && splitter[3].Equals(player.getID().ToString()) == false) {
                        movingObjects[i].setCoords(float.Parse(splitter[0]), float.Parse(splitter[1]), float.Parse(splitter[2]));
                        found = true;
                    }
                }

                if (!found && splitter[3].Equals(player.getID().ToString()) == false) movingObjects.Add(new PlayerShip(
                      new Vector2(float.Parse(splitter[0]), float.Parse(splitter[1])), float.Parse(splitter[2]),
                      int.Parse(splitter[3])));
                System.Diagnostics.Debug.WriteLine("CLIENT: Added player");
            }
        } //dead

       /* public void checkMail() {
            while ((mail = client.ReadMessage()) != null) {
                switch (mail.MessageType) {
                    case NetIncomingMessageType.Data:
                        splitter = new string[4] { "0", "1", "2", "3" };
                        splitter = mail.ReadString().ToString().Split(deliminators);

                        found = false;
                        for (int i = 0; i < playerList.Count; i++) {
                            if (playerList[i].getID().ToString().Equals(splitter[3]) && splitter[3].Equals(player.getID().ToString()) == false) {
                                playerList[i].setCoords(float.Parse(splitter[0]), float.Parse(splitter[1]), float.Parse(splitter[2]));
                                found = true;
                            }
                        }

                        if (!found && splitter[3].Equals(player.getID().ToString()) == false) playerList.Add(new PlayerShip(
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
        }*/
    }
}
