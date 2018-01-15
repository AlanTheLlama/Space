using System;
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
        //TCP v
        //Sockets/DDNS v
        IPHostEntry ipHostInfo;
        IPAddress ipAddress;
        IPEndPoint remoteEP;
        Socket client;

        StreamReader _sReader;
        StreamWriter _sWriter;

        AI bob;

        public float MAX_SPEED = 9;

        public World world;

        public static List<MovingObject> movingObjects;
        char[] deliminators = { ',', ' ', '/', ';'};
        string[] splitter;
        bool found;
        bool talking = false;

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

            //ipHostInfo = Dns.GetHostEntry("24.108.12.19");
            //ipAddress = ipHostInfo.AddressList[0];
            //remoteEP = new IPEndPoint(ipAddress, 31579);
            remoteEP = new IPEndPoint(IPAddress.Parse("192.168.0.13"), 31579);
            client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

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

            if (!player.isLanding()) {
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

                if (Keyboard.GetState().IsKeyDown(Keys.H) == true && player.getSpeed() < player.MAX_SPEED_TO_LAND) {
                    List<SpaceObject> spaceObjects = world.getSpaceObjects();
                    for (int index = 0; index < spaceObjects.Count; index++) {
                        if (Math2.inRadius(player.getPos().X, player.getPos().Y,
                            spaceObjects[index].getXpos(), spaceObjects[index].getYpos(), spaceObjects[index].getRadius())) {
                            player.land(spaceObjects[index]);
                            index = spaceObjects.Count;
                        }
                    }
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.G) == true) {
                player.takeOff();
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

            connectToServer();
            cam.UpdateCamera(viewport);
            base.Update(gameTime);
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Viewport = viewport;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, null, null, null, null, cam.Transform);

            foreach (SpaceObject obj in world.getSpaceObjects()) {        //static objects
                spriteBatch.Draw(obj.getImage(),
                    new Rectangle((int)obj.getXpos(), (int)obj.getYpos(), 50, 50),
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
                ASCIIEncoding encoder = new ASCIIEncoding();

                client.BeginConnect(remoteEP, new AsyncCallback(AcceptCallback), client);
                connected = true;
                //System.Diagnostics.Debug.WriteLine("Connected");
            } catch {
                System.Diagnostics.Debug.WriteLine("Connection Failed");
                connected = false;
            }
        }

        public void AcceptCallback(IAsyncResult ar) {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(updateLocation), state);

            try {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(RecieveStuff), state);
            } catch (SocketException se) {
                System.Diagnostics.Debug.WriteLine("Server connected but did not ping back!\nERROR: " + se);
            }
        }

        public void SendStuff(IAsyncResult ar) {
            try {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);

            }catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        public void RecieveStuff(IAsyncResult ar) {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            ASCIIEncoding encoder = new ASCIIEncoding();

            int bytesRead = handler.EndReceive(ar);
            String content = String.Empty;

            if(bytesRead > 0) {
                state.sb.Append(encoder.GetString(state.buffer, 0, bytesRead));

                content = state.sb.ToString();

                if (content.IndexOf("<EOF>") > -1) {
                    System.Diagnostics.Debug.WriteLine("SERVER: Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);

                    //System.Diagnostics.Debug.WriteLine("CLIENT-RECIEVED: " + content);
                } else {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(RecieveStuff), state);
                }

                state.sb.Clear();

                String rec = content;
                System.Diagnostics.Debug.WriteLine("CLIENT-RECIEVED: " + rec);
            }
        }

        public void updateLocation(IAsyncResult ar) {
            ASCIIEncoding encoder = new ASCIIEncoding();
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            byte[] buffer = encoder.GetBytes(player.dataString());
            byte[] message = new byte[4096];
            int bytesRead;

            //Sending
            client.BeginSend(buffer, 0, buffer.Length, 0,
                new AsyncCallback(SendStuff), client);

            //Listening

            state = new StateObject();
            state.workSocket = client;

            try {
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(RecieveStuff), state);
            }catch (Exception e) {
                System.Diagnostics.Debug.WriteLine("ERROR: " + e.ToString());
            }
            bytesRead = 0;
            /*if (true) {
                talking = true;
                while (true || encoder.GetString(message, 0, bytesRead).Split(deliminators).Length <= 4) {
                    bytesRead = 0;
                    try {
                        //System.Diagnostics.Debug.WriteLine("CLIENT: Attempting to find paper ");
                        bytesRead = clientStream.Read(message, 0, 4096);
                        //System.Diagnostics.Debug.WriteLine("CLIENT: Read the morning paper, page #" + bytesRead);
                        String rec = encoder.GetString(message, 0, bytesRead);
                        System.Diagnostics.Debug.WriteLine("CLIENT-RECIEVED-PRELIM: " + rec);
                    } catch (System.IndexOutOfRangeException exc) {
                        System.Diagnostics.Debug.WriteLine("CLIENT: Failed to recieve data: " + exc);
                        break;
                    }
                    if (bytesRead == 0) {
                        System.Diagnostics.Debug.WriteLine("CLIENT: Neighbour ditched us. ");
                        break;
                    }
                }
            }*/
            if (bytesRead > 0) {
                String rec = encoder.GetString(message, 0, bytesRead);
                System.Diagnostics.Debug.WriteLine("CLIENT-RECIEVED: " + rec);
            }
            //writer.Flush();
        }

        /*public void HandleCommunication() {
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
        }*/ //dead

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
