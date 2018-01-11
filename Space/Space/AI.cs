using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace Space {
    public class AI {

        public float MAX_SPEED = 4;

        public Vector2 pos; //Don't know if I need any of these?
        public float speed = 1;
        public float acceleration = 1;

        public AI (int x, int y) { //Creates a new AI at pos x, y
            this.pos = new Vector2(x, y);
            this.acceleration = (float)0.3;
        }

        public void accelerate() {
            this.speed += this.acceleration;
            if (this.speed > MAX_SPEED)
            {
                this.speed = MAX_SPEED;
            }
        }
        
        public void moveYo() {
            this.pos = new Vector2(pos.X, pos.Y - 1);
        }

        public void brake() {
            this.speed -= this.acceleration;
            if (this.speed < 0)
            {
                this.speed = 0;
            }
        }

        public bool danger() {
            //If the player is within a 50x50 area return true
            //Game1.playerList;
            foreach (PlayerShip ships in Game1.playerList) {
                /*if ((ships.pos.X <= (this.pos.X + 50) && ships.pos.X >= (this.pos.X - 50)) && (ships.pos.Y <= (this.pos.Y + 50) && ships.pos.Y >= (this.pos.Y - 50))) {
                    return true;
                }*/
                if (new Rectangle((int)ships.pos.X - 171, (int)ships.pos.Y - 191, 171, 191).Contains(
                    new Point((int)this.pos.X, (int)this.pos.Y))) return true;
            }
            return false;
        }
        public void nearby() {
            if (danger()) {
                this.moveYo();
                //System.Diagnostics.Debug.WriteLine("DANGER!");
            }
            else if (!danger() && this.speed > 0) {
                brake();
            }
        }
    }
}
