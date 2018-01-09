using System;
using System.Collections.Generic;
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
        Camera cam;

        public float MAX_SPEED = 6;

        World world;

        public List<PlayerShip> playerList;

        public static Texture2D ship;
        public static Texture2D testTile;
        public static Texture2D asteroid;

        public static PlayerShip player;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            // TODO: Add your initialization logic here
            world = new World(15000, 15000);
            world.Generate(600, 1000, 50);

            base.Initialize();
        }

        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            playerList = new List<PlayerShip>();

            //textures
            ship = Content.Load<Texture2D>("Images/ship");
            testTile = Content.Load<Texture2D>("Images/tile");
            asteroid = Content.Load<Texture2D>("Images/asteroid");

            player = new PlayerShip(new Vector2(-ship.Width / 2, -ship.Height / 2), -((float)0.5 * ((float)Math.PI)), 1, (float)0.1, (float)0.4);
            playerList.Add(player);

            viewport = GraphicsDevice.Viewport;
            cam = new Camera(viewport, player);
        }

        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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

            if (keyW == true && keyS == false) {
                
            }
            if (keyW == true)
            {
                accelerate(player);
            }

            if (keyS == true)
            {
                brake(player);
            }

            if (keyD == true)
            {
                rotateRight(player);
            }

            if (keyA == true)
            {
                rotateLeft(player);
            }

            updatePosition(player);

            cam.UpdateCamera(viewport);
            base.Update(gameTime);
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Viewport = viewport;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, null, null, null, null, cam.Transform);

            foreach (SpaceObject obj in World.spaceObjects) {
                spriteBatch.Draw(obj.getImage(),
                    new Rectangle(obj.getXpos(), obj.getYpos(), 50, 50),
                    Color.White);
                //System.Diagnostics.Debug.WriteLine("Asteroid co-ord: " + obj.getXpos() + ", " + obj.getYpos() + ". ");
            }

            foreach (PlayerShip ships in playerList) {
                spriteBatch.Draw(ship,
                    new Rectangle((int)ships.pos.X, (int)ships.pos.Y, ship.Width, ship.Height),
                    null,
                    Color.White,
                    ships.rotation + (float) 0.5 * (float) Math.PI,
                    new Vector2(ship.Width / 2, ship.Height / 2),
                    SpriteEffects.None, 0);

            }

            //drawing some tiles to represent camera/ship movement against something that stays still
            spriteBatch.Draw(testTile, new Rectangle(60, 60, 10, 10), Color.White);
            spriteBatch.Draw(testTile, new Rectangle(15, -50, 10, 10), Color.White);
            spriteBatch.Draw(testTile, new Rectangle(200, 0, 10, 10), Color.White);
            spriteBatch.Draw(testTile, new Rectangle(100, -300, 10, 10), Color.White);
            spriteBatch.Draw(asteroid, new Rectangle(50, 50, 50, 50), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void rotateRight(PlayerShip ps)
        {
            ps.rotation = ps.rotation + ps.rotSpeed;
            if (ps.rotation > 2 * ((float)Math.PI))
            {
                ps.rotation = ps.rotation - (2 * ((float)Math.PI));
            }

        }

        public void rotateLeft(PlayerShip ps)
        {
            ps.rotation = ps.rotation - ps.rotSpeed;
            if (ps.rotation < 0)
            {
                ps.rotation = ps.rotation + (2 * ((float)Math.PI));
            }
        }

        public void accelerate(PlayerShip ps)
        {
            if (ps.speed < MAX_SPEED)
            {
                ps.speed += ps.acceleration;
            }

        }

        public void brake(PlayerShip ps)
        {
            if (ps.speed > 0)
            {
                ps.speed -= ps.acceleration;
            }
        }

        public void updatePosition(PlayerShip ps)
        {
            float x = ps.pos.X;
            float y = ps.pos.Y;
            x += ps.speed * (float) Math.Cos(ps.rotation);
            y += ps.speed * (float) Math.Sin(ps.rotation);
            ps.pos = new Vector2(x, y);
        }
    }
}
