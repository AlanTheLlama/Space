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
using MenuLib;

namespace Space {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>

    public class MainClient : Game {
        GraphicsDeviceManager graphics;
        Viewport viewport;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont bigFont;
        Camera cam;
        ClientDataHandler handler;
        Timer dataTimer;
        AI bob;
        Button startButton, exitButton, optionsButton;

        public static float MAX_SPEED = 9;
        public static float RENDER_RADIUS = 2000;
        public static int MAP_WIDTH = 300000;
        public static int MAP_HEIGHT = 180000;
        public static int SCREEN_WIDTH = 800;
        public static int SCREEN_HEIGHT = 480;

        public World world;

        public static List<Object> objects;
        public List<Button> buttonList;
        char[] deliminators = { ',', ' ', '/', ';' };
        public enum PlayState { MENU, PLAYING };
        PlayState ps;

        public static Texture2D ship;
        public static Texture2D testTile;
        public static Texture2D asteroid;
        public static Texture2D enemy;
        public static Texture2D laserTex;
        public static Texture2D planet;
        public static Texture2D star;
        public static Texture2D minimap_star;
        public static Texture2D minimap_ship;

        bool connected = false;

        public static PlayerShip player;

        public MainClient() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            objects = new List<Object>();
            buttonList = new List<Button>();

            startButton = new Button(Content.Load<Texture2D>("startButton"), null, 20, viewport.Height - 40, 50, 20);
            exitButton = new Button(Content.Load<Texture2D>("exitButton"), null, 90, viewport.Height - 40, 50, 20);
            optionsButton = new Button(Content.Load<Texture2D>("optionsButton"), null, 160, viewport.Height - 40, 50, 20);

            ps = PlayState.MENU;

            world = new World(MAP_WIDTH, MAP_HEIGHT);
            world.Generate(10000, 12000);
            foreach (SpaceObject so in world.getSpaceObjects()) {
                objects.Add(so);
            }

            handler = new ClientDataHandler();

            dataTimer = new Timer();
            dataTimer.Elapsed += new ElapsedEventHandler(exchangeData);
            dataTimer.Interval = 25;
            dataTimer.Enabled = true;

            spriteBatch = new SpriteBatch(GraphicsDevice);

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
            laserTex = Content.Load<Texture2D>("Images/laser");
            planet = Content.Load<Texture2D>("Images/planet");
            star = Content.Load<Texture2D>("Images/star");
            minimap_ship = Content.Load<Texture2D>("Images/minimap_ship");
            minimap_star = Content.Load<Texture2D>("Images/minimap_star");
            font = Content.Load<SpriteFont>("File");
            bigFont = Content.Load<SpriteFont>("BiggerFont");

            player = new PlayerShip(new Vector2(MAP_WIDTH / 2, MAP_HEIGHT / 2));
            bob = new AI(MAP_WIDTH / 2, MAP_HEIGHT / 2);
            objects.Add(player);
            objects.Add(bob);
        }

        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && connected == false) {
                try {
                    handler.connectToServer();
                    connected = true;
                } catch (Exception e) {
                    connected = false;
                    System.Diagnostics.Debug.WriteLine("Could not connect!");
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.T) == true) {
                player.onMap();
            } else {
                player.offMap();
                if (player.isAlive()) {
                    if (!player.isLanding()) {
                        if (!player.isCooling()) {
                            if (Keyboard.GetState().IsKeyDown(Keys.W) == true) {
                                player.thrust();
                            }
                        }
                        if (!player.isBoosting() && !player.isCooling()) {
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
                                for (int index = 0; index < objects.Count; index++) {
                                    if (objects[index].getType() == ObjectType.ASTEROID || objects[index].getType() == ObjectType.MINING_PLANET) {
                                        if (Math2.inRadius(player.getPos(), objects[index].getPos(), ((SpaceObject)objects[index]).getRadius())) {
                                            player.land((SpaceObject)objects[index]);
                                            index = objects.Count;
                                        }
                                    }
                                }
                            }

                            if (!player.isCooling()) {
                                if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
                                    Laser l = player.fireWeapon(new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y));
                                    if (l != null) {
                                        objects.Add(l);
                                    }
                                    Console.WriteLine(Mouse.GetState().Position.X.ToString() + ", " + Mouse.GetState().Position.Y.ToString());
                                }
                            }
                        }
                        if (!player.isCooling()) {
                            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) == true) {
                                player.boost();
                            }
                        }

                        if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) == false || player.isCooling()) {
                            player.noBoost();
                        }

                    } else {

                        if (Keyboard.GetState().IsKeyDown(Keys.M) == true) {
                            if (player.getCapacity() > 0) {
                                player.mine();
                            }
                        }

                        if (Keyboard.GetState().IsKeyDown(Keys.G) == true) {
                            player.takeOff();
                        }
                    }
                }
            }

            List<Object> toBeDestroyed = new List<Object>();

            foreach (Object o in objects) {
                if (!o.isAlive()) {
                    toBeDestroyed.Add(o);
                } else {
                    o.update(world);
                    if (o.getType() == ObjectType.PROJECTILE) {
                        foreach (Object o2 in objects) {
                            Projectile p = (Projectile)o;
                            if (o2.isHit(o)) {
                                o.getHit(0);
                                o2.getHit(p.getPower());
                            }
                        }
                    }
                }
            }

            foreach (Object o in toBeDestroyed) {
                objects.Remove(o);
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
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, cam.Transform);

            switch (ps) {
                case PlayState.PLAYING:
                    if (player.isInMap()) {
                        float SCALE = (float)2.8;
                        Vector2 topLeft = new Vector2(player.getPos().X - SCREEN_WIDTH * (float)1.5 + SCREEN_WIDTH * (3 - SCALE) / 2,
                            player.getPos().Y - SCREEN_HEIGHT * (float)1.5 + SCREEN_HEIGHT * (3 - SCALE) / 2);
                        foreach (Object o in objects) {
                            if (o.getType() == ObjectType.STAR) {
                                Vector2 pos = new Vector2(topLeft.X + o.getPos().X * SCREEN_WIDTH * SCALE / MAP_WIDTH, topLeft.Y + o.getPos().Y * SCREEN_HEIGHT * SCALE / MAP_HEIGHT);
                                spriteBatch.Draw(minimap_star,
                                    new Rectangle((int)pos.X, (int)pos.Y, minimap_star.Width, minimap_star.Height),
                                    null,
                                    Color.White,
                                    0,
                                    new Vector2(minimap_star.Width / 2, minimap_star.Height / 2),
                                    SpriteEffects.None, 0);

                                if (Math2.inRadius(new Vector2(topLeft.X + Mouse.GetState().Position.X * SCALE,
                                    topLeft.Y + Mouse.GetState().Position.Y * SCALE), pos, 16)) {

                                    spriteBatch.DrawString(bigFont, "ID: " + o.getID().ToString(),
                                    new Vector2(topLeft.X + SCREEN_WIDTH * (float)1.4, topLeft.Y + 16), Color.White);
                                }
                            }
                        }
                        Vector2 shipPos = new Vector2(topLeft.X + player.getPos().X * SCREEN_WIDTH * SCALE / MAP_WIDTH, topLeft.Y + player.getPos().Y * SCREEN_HEIGHT * SCALE / MAP_HEIGHT);
                        spriteBatch.Draw(minimap_ship,
                            new Rectangle((int)shipPos.X, (int)shipPos.Y, minimap_ship.Width, minimap_ship.Height),
                            null,
                            Color.White,
                            0,
                            new Vector2(minimap_ship.Width / 2, minimap_ship.Height / 2),
                            SpriteEffects.None, 0);
                    } else {
                        int i = 0;
                        foreach (Object o in objects) {
                            ObjectType type = o.getType();
                            if ((type == ObjectType.STAR ||
                                type == ObjectType.MINING_PLANET ||
                                type == ObjectType.ASTEROID) &&
                                Math2.inRadius(player.getPos(), o.getPos(), RENDER_RADIUS)) {
                                spriteBatch.Draw(o.getTexture(),
                                    new Rectangle((int)o.getPos().X, (int)o.getPos().Y, o.getTexture().Width, o.getTexture().Height),
                                    null,
                                    Color.White,
                                    o.getAngle() + Math2.QUARTER_CIRCLE,
                                    new Vector2(o.getTexture().Width / 2, o.getTexture().Height / 2),
                                    SpriteEffects.None, 0);
                                i++;
                                if (o.getType() == ObjectType.STAR) {
                                    spriteBatch.DrawString(font, "Star ID: " + ((Star)o).getID().ToString(), new Vector2(o.getPos().X, o.getPos().Y + 200), Color.White);
                                }
                            }
                        }
                        foreach (Object o in objects) {
                            ObjectType type = o.getType();
                            if ((type == ObjectType.PLAYER || type == ObjectType.AI) && Math2.inRadius(player.getPos(), o.getPos(), RENDER_RADIUS)) {
                                spriteBatch.Draw(o.getTexture(),
                                    new Rectangle((int)o.getPos().X, (int)o.getPos().Y, o.getTexture().Width, o.getTexture().Height),
                                    null,
                                    Color.White,
                                    o.getAngle() + Math2.QUARTER_CIRCLE,
                                    new Vector2(o.getTexture().Width / 2, o.getTexture().Height / 2),
                                    SpriteEffects.None, 0);
                                i++;
                                if (o.getType() == ObjectType.STAR) {
                                    spriteBatch.DrawString(font, "Star ID: " + ((Star)o).getID().ToString(), new Vector2(o.getPos().X, o.getPos().Y + 200), Color.White);
                                }
                            }
                        }
                        foreach (Object o in objects) {
                            ObjectType type = o.getType();
                            if (type == ObjectType.PROJECTILE && Math2.inRadius(player.getPos(), o.getPos(), RENDER_RADIUS)) {
                                spriteBatch.Draw(o.getTexture(),
                                    new Rectangle((int)o.getPos().X, (int)o.getPos().Y, o.getTexture().Width, o.getTexture().Height),
                                    null,
                                    Color.White,
                                    o.getAngle() + Math2.QUARTER_CIRCLE,
                                    new Vector2(o.getTexture().Width / 2, o.getTexture().Height / 2),
                                    SpriteEffects.None, 0);
                                i++;
                                if (o.getType() == ObjectType.STAR) {
                                    spriteBatch.DrawString(font, "Star ID: " + ((Star)o).getID().ToString(), new Vector2(o.getPos().X, o.getPos().Y + 200), Color.White);
                                }
                            }
                        }

                        spriteBatch.DrawString(font, "Speed: " + Math.Round((decimal)player.getSpeed()).ToString(), new Vector2(player.getPos().X, player.getPos().Y + 80), Color.White);
                        spriteBatch.DrawString(font, "Iron:  " + Math.Round((decimal)player.getIron()).ToString(), new Vector2(player.getPos().X, player.getPos().Y + 96), Color.White);
                        spriteBatch.DrawString(font, "Gems:  " + Math.Round((decimal)player.getGems()).ToString(), new Vector2(player.getPos().X, player.getPos().Y + 112), Color.White);
                        spriteBatch.DrawString(font, "Cap:   " + Math.Round((decimal)player.getCapacity()).ToString(), new Vector2(player.getPos().X, player.getPos().Y + 128), Color.White);
                    }
                    break;

                case PlayState.MENU:
                    
                    break;

                default:

                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}