using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Space
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        Viewport viewport;
        SpriteBatch spriteBatch;
        Camera cam;

        public List<PlayerShip> playerList;

        public Texture2D ship;
        public Texture2D testTile;

        public PlayerShip player;

        Boolean keyW = false;   //these are necessary for angling the sprites when two keys are pressed,
        Boolean keyA = false;   //because the Keyboard.GetState() function can only handle one key
        Boolean keyS = false;   //at a time
        Boolean keyD = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            viewport = GraphicsDevice.Viewport;
            cam = new Camera(viewport);

            playerList = new List<PlayerShip>();

            //textures
            ship = Content.Load<Texture2D>("Images/ship");
            testTile = Content.Load<Texture2D>("Images/tile");

            player = new PlayerShip(new Vector2(-ship.Width / 2, -ship.Height / 2), 0);
            playerList.Add(player);
        }

        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        //apologies for mild overcomplication on movementlol
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W)) {
                keyW = true;
            } else keyW = false;

            if (Keyboard.GetState().IsKeyDown(Keys.S)) {
                keyS = true;
            }
            else keyS = false;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) {
                keyA = true;
            }
            else keyA = false;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) {
                keyD = true;
            }
            else keyD = false;

            if(keyW == true && keyS == false) {
                if (keyW && keyA) {
                    player.rotation = (float)Math.PI * 1.75F;
                    player.pos = new Vector2(player.pos.X - 1, player.pos.Y - 1);
                } else if (keyW && keyD) {
                    player.rotation = (float)Math.PI * 0.25F;
                    player.pos = new Vector2(player.pos.X + 1, player.pos.Y - 1);
                } else {
                    player.rotation = 0;
                    player.pos = new Vector2(player.pos.X, player.pos.Y - 1);
                }
            }
            if(keyS == true && keyW == false) {
                if (keyS && keyA) {
                    player.rotation = (float)Math.PI * 1.25F;
                    player.pos = new Vector2(player.pos.X - 1, player.pos.Y + 1);
                } else if (keyS && keyD) {
                    player.rotation = (float)Math.PI * 0.75F;
                    player.pos = new Vector2(player.pos.X + 1, player.pos.Y + 1);
                }
                else {
                    player.rotation = (float)Math.PI;
                    player.pos = new Vector2(player.pos.X, player.pos.Y + 1);
                }
            }

            if(keyA == true && keyW == false && keyS == false) {
                player.rotation = (float)Math.PI * 1.5F;
                player.pos = new Vector2(player.pos.X - 1, player.pos.Y);
            }

            if (keyD == true && keyW == false && keyS == false) {
                player.rotation = (float)Math.PI * 0.5F;
                player.pos = new Vector2(player.pos.X + 1, player.pos.Y);
            }

            cam.UpdateCamera(viewport);
            base.Update(gameTime);
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Viewport = viewport;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, null, null, null, null, cam.Transform);

            foreach (PlayerShip ships in playerList)
            {
                spriteBatch.Draw(ship, 
                    new Rectangle((int)ships.pos.X, (int)ships.pos.Y, ship.Width, ship.Height),
                    null, 
                    Color.White,
                    ships.rotation,
                    new Vector2(ship.Width / 2, ship.Height / 2),
                    SpriteEffects.None, 0);
                
            }

            //drawing some tiles to represent camera/ship movement against something that stays still
            spriteBatch.Draw(testTile, new Rectangle(60, 60, 10, 10), Color.White);
            spriteBatch.Draw(testTile, new Rectangle(15, -50, 10, 10), Color.White);
            spriteBatch.Draw(testTile, new Rectangle(200, 0, 10, 10), Color.White);
            spriteBatch.Draw(testTile, new Rectangle(100, -300, 10, 10), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
