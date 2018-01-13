using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Space {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>

    public class MainClient : Game {
        GraphicsDeviceManager graphics;
        Viewport viewport;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Camera cam;
        ClientDataHandler handler;
        Timer dataTimer;
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

        public MainClient() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            world = new World(15000, 15000);
            world.Generate(600, 1000, 50);

            handler = new ClientDataHandler();

            dataTimer = new Timer();
            dataTimer.Elapsed += new ElapsedEventHandler(exchangeData);
            dataTimer.Interval = 25;
            dataTimer.Enabled = true;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            movingObjects = new List<MovingObject>();

            viewport = GraphicsDevice.Viewport;

            this.IsMouseVisible = true;

            base.Initialize();

            cam = new Camera(viewport, player);
        }

        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        protected override void LoadContent() {
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
        }

        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            if(Keyboard.GetState().IsKeyDown(Keys.Enter) && connected == false) {
                try {
                    handler.connectToServer();
                    connected = true;
                }catch (Exception e) {
                    connected = false;
                    System.Diagnostics.Debug.WriteLine("Could not connect!");
                }
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

            //connectToServer();
            cam.UpdateCamera(viewport);
            base.Update(gameTime);
        }

        public void exchangeData(object source, ElapsedEventArgs e) {
            if (handler.isConnected()) {
                handler.Send(player.dataString());  //Look how tiny I made all the networking stuff!!! #beproud
                handler.Receive();
            }
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
    }
}
