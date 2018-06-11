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
using Microsoft.Xna.Framework.Audio;
using System.Linq;

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
        List<SoundEffect> soundEffects;
        Camera cam;
        ClientDataHandler handler;
        Timer dataTimer;
        AI bob;
        Button startButton, exitButton, optionsButton;
        public static Random r;

        public static float MAX_SPEED = 9;
        public static float RENDER_RADIUS = 7000;
        public static int MAP_WIDTH = 300000;
        public static int MAP_HEIGHT = 180000;
        public static int SCREEN_WIDTH = 1920;
        public static int SCREEN_HEIGHT = 1080;


        public static List<String> names;

        public static World world;

        public static List<Object> objects;
        public static List<Asteroid> asteroids;
        public static List<AI> ships;
        public static List<Projectile> projectiles;
        public List<Object> toBeDestroyed;
        public List<Button> buttonList;
        public static List<Object> players;
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
            ships = new List<AI>();
            toBeDestroyed = new List<Object>();
            buttonList = new List<Button>();
            names = new List<String>();
            players = new List<Object>();
            soundEffects = new List<SoundEffect>();
            projectiles = new List<Projectile>();

            r = new Random();

            ps = PlayState.PLAYING;

            world = new World(MAP_WIDTH, MAP_HEIGHT); //regular world
            world.Generate(10000, 12000);

            //world = new World(15000, 15000);            //test world, uncomment these and comment the two world lines above for test world and vice versa
            //world.generateTestWorld();

            foreach (SpaceObject so in world.getSpaceObjects()) {
                objects.Add(so);
            }
            asteroids = world.asteroidJail;
            for (int i = 0; i < world.factions.Count(); i++) {
                foreach (AI s in world.factions[i].controlledShips) {
                    ships.Add(s);
                }
            }

            handler = new ClientDataHandler();

            dataTimer = new Timer();
            dataTimer.Elapsed += new ElapsedEventHandler(exchangeData);
            dataTimer.Interval = 25;
            dataTimer.Enabled = true;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            viewport = GraphicsDevice.Viewport;
            viewport.Width = 1920;
            viewport.Height = 1080;

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

            soundEffects.Add(Content.Load<SoundEffect>("Sounds/music"));

            startButton = new Button(Content.Load<Texture2D>("Images/startButton"), null, 20, viewport.Height - 40, 50, 20);
            exitButton = new Button(Content.Load<Texture2D>("Images/exitButton"), null, 90, viewport.Height - 40, 50, 20);
            optionsButton = new Button(Content.Load<Texture2D>("Images/optionsButton"), null, 160, viewport.Height - 40, 50, 20);

            buttonList.Add(startButton);
            buttonList.Add(exitButton);
            buttonList.Add(optionsButton);

            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();

            var instance = soundEffects[0].CreateInstance();
            instance.IsLooped = true;
            instance.Play();

            player = new PlayerShip(new Vector2(world.getSizeX() / 2, world.getSizeY() / 2));
            //bob = new AI(MAP_WIDTH / 2, MAP_HEIGHT / 2 + 100);
            objects.Add(player);
            //objects.Add(bob);
            players.Add(player);
        }

        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            //bob.travelToTarget(player.getPos(), 1, 4);
            //bob.rotateToTarget(player.getPos());
            player.update(world);

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
                        if (/*!player.isBoosting() && !player.isCooling()*/ 1 == 1) {
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
                                        projectiles.Add(l);
                                    }
                                    //Console.WriteLine(Mouse.GetState().Position.X.ToString() + ", " + Mouse.GetState().Position.Y.ToString());
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

            toBeDestroyed.Clear();

            foreach (AI o in ships.ToList()) {
                if (!o.isAlive()) {
                    ships.Remove(o);
                } else {
                    o.update(world);
                    /*foreach(Object obj in objects) {
                        if (obj.getType() == ObjectType.PROJECTILE) {
                            Projectile p = (Projectile)obj;
                            //p.update(world);
                            if (o.isHit(p) && p.getID() != o.getID()) {
                                p.getHit(0);
                                o.getHit(p.getPower());
                                //Console.WriteLine("Projectile " + o2.getID() + " hit " + o.getID());
                            }
                        }
                    }*/
                }
            }

            foreach(Projectile p in projectiles.ToList()) {
                if (!p.isAlive()) {
                    projectiles.Remove(p);
                }else p.update(world);
            }

            foreach (Faction f in world.factions) {
                f.update();
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


        //MAPFUNC
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

                        Vector2 mousePos = new Vector2(topLeft.X - 18 + Mouse.GetState().Position.X * (float)2.851,
                                    topLeft.Y - 18 + Mouse.GetState().Position.Y * (float)2.851);

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

                                if (Math2.inRadius(mousePos, pos, 16)) {
                                    spriteBatch.DrawString(bigFont, "ID: " + o.getID().ToString(),
                                        new Vector2(topLeft.X + SCREEN_WIDTH * (float)1.4, topLeft.Y + 16), Color.White);
                                    spriteBatch.DrawString(bigFont, "OWNER: \"" + o.getOwner() + "\"",
                                        new Vector2(topLeft.X + SCREEN_WIDTH * (float)1.4, topLeft.Y + 48), Color.White);
                                }
                            }
                            
                        }
                        foreach(AI o in ships) {
                                Vector2 pos = new Vector2(topLeft.X + o.getPos().X * SCREEN_WIDTH * SCALE / MAP_WIDTH, topLeft.Y + o.getPos().Y * SCREEN_HEIGHT * SCALE / MAP_HEIGHT);
                                spriteBatch.Draw(minimap_star,
                                    new Rectangle((int)pos.X, (int)pos.Y, 5, 5),
                                    null,
                                    Color.White,
                                    0,
                                    new Vector2(minimap_star.Width / 2, minimap_star.Height / 2),
                                    SpriteEffects.None, 0);
                        }
                        Vector2 shipPos = new Vector2(topLeft.X + player.getPos().X * SCREEN_WIDTH * SCALE / MAP_WIDTH, topLeft.Y + player.getPos().Y * SCREEN_HEIGHT * SCALE / MAP_HEIGHT);
                        spriteBatch.Draw(minimap_ship,
                            new Rectangle((int)shipPos.X, (int)shipPos.Y, minimap_ship.Width, minimap_ship.Height),
                            null,
                            Color.White,
                            0,
                            new Vector2(minimap_ship.Width / 2, minimap_ship.Height / 2),
                            SpriteEffects.None, 0);
                        spriteBatch.Draw(minimap_ship,
                            new Rectangle((int)mousePos.X, (int)mousePos.Y, minimap_ship.Width, minimap_ship.Height),
                            null,
                            Color.White,
                            0,
                            new Vector2(minimap_ship.Width / 2, minimap_ship.Height / 2),
                            SpriteEffects.None, 0);
                    } else {
                        int i = 0;
                        foreach (Object o in objects) {
                            bool draw = Math2.inRadius(player.getPos(), o.getPos(), RENDER_RADIUS);
                            ObjectType type = o.getType();
                            if ((type == ObjectType.STAR ||
                                type == ObjectType.MINING_PLANET ||
                                type == ObjectType.ASTEROID) &&
                                draw) {
                                spriteBatch.Draw(o.getTexture(),
                                    new Rectangle((int)o.getPos().X, (int)o.getPos().Y, o.getTexture().Width, o.getTexture().Height),
                                    null,
                                    Color.White,
                                    o.getAngle() + Math2.QUARTER_CIRCLE,
                                    new Vector2(o.getTexture().Width / 2, o.getTexture().Height / 2),
                                    SpriteEffects.None, 0);
                                if (o.getType() == ObjectType.STAR) {
                                    spriteBatch.DrawString(font, "Star ID: " + ((Star)o).getID().ToString(), new Vector2(o.getPos().X, o.getPos().Y + 200), Color.White);
                                }
                            }
                        }

                        foreach(PlayerShip o in players) {
                            Console.Out.WriteLine("Player Location: " + o.getPos().X + ", " + o.getPos().Y);
                            spriteBatch.Draw(o.getTexture(),
                                    new Rectangle((int)o.getPos().X, (int)o.getPos().Y, o.getTexture().Width, o.getTexture().Height),
                                    null,
                                    Color.White,
                                    o.getAngle() + Math2.QUARTER_CIRCLE,
                                    new Vector2(o.getTexture().Width / 2, o.getTexture().Height / 2),
                                    SpriteEffects.None, 0);
                        }

                        foreach(Asteroid o in asteroids) {
                            if (Math2.inRadius(player.getPos(), o.getPos(), RENDER_RADIUS)) {
                                spriteBatch.Draw(o.getTexture(),
                                    new Rectangle((int)o.getPos().X, (int)o.getPos().Y, o.getTexture().Width, o.getTexture().Height),
                                    null,
                                    Color.White,
                                    o.getAngle() + Math2.QUARTER_CIRCLE,
                                    new Vector2(o.getTexture().Width / 2, o.getTexture().Height / 2),
                                    SpriteEffects.None, 0);
                            }
                        }

                        foreach(AI o in ships) {
                            if (Math2.inRadius(player.getPos(), o.getPos(), RENDER_RADIUS)) {
                                spriteBatch.Draw(o.getTexture(),
                                    new Rectangle((int)o.getPos().X, (int)o.getPos().Y, o.getTexture().Width, o.getTexture().Height),
                                    null,
                                    Color.White,
                                    o.getAngle() + Math2.QUARTER_CIRCLE,
                                    new Vector2(o.getTexture().Width / 2, o.getTexture().Height / 2),
                                    SpriteEffects.None, 0);

                                spriteBatch.DrawString(font, o.getTask() + "\n" + o.getOwner() + "\n" + o.getID(), new Vector2(o.getPos().X, o.getPos().Y + 200), Color.White);
                            }
                        }
                        foreach(Projectile o in projectiles) {
                            if (Math2.inRadius(player.getPos(), o.getPos(), RENDER_RADIUS)) {
                                spriteBatch.Draw(o.getTexture(),
                                    new Rectangle((int)o.getPos().X, (int)o.getPos().Y, o.getTexture().Width, o.getTexture().Height),
                                    null,
                                    Color.White,
                                    o.getAngle() + Math2.QUARTER_CIRCLE,
                                    new Vector2(o.getTexture().Width / 2, o.getTexture().Height / 2),
                                    SpriteEffects.None, 0);
                            }
                        }

                        spriteBatch.DrawString(font, "Speed: " + Math.Round((decimal)player.getSpeed()).ToString(), new Vector2(player.getPos().X, player.getPos().Y + 80), Color.White);
                        spriteBatch.DrawString(font, "Iron:  " + Math.Round((decimal)player.getIron()).ToString(), new Vector2(player.getPos().X, player.getPos().Y + 96), Color.White);
                        spriteBatch.DrawString(font, "Gems:  " + Math.Round((decimal)player.getGems()).ToString(), new Vector2(player.getPos().X, player.getPos().Y + 112), Color.White);
                        spriteBatch.DrawString(font, "Cap:   " + Math.Round((decimal)player.getCapacity()).ToString(), new Vector2(player.getPos().X, player.getPos().Y + 128), Color.White);
                    }
                    break;

                case PlayState.MENU:
                    
                    spriteBatch.DrawString(font, "yolo", new Vector2(player.getPos().X - (SCREEN_WIDTH / 2) + 20, player.getPos().Y), Color.White);
                    foreach (Button b in buttonList) {
                        spriteBatch.Draw(b.tex, b.bounds, Color.White);
                        System.Diagnostics.Debug.WriteLine("Yolo");
                    }
                    break;

                default:

                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}