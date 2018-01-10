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
        public float speed;
        public float acceleration;

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
                if ((ships.pos.X <= (this.pos.X + 50) && ships.pos.X >= (this.pos.X - 50)) && (ships.pos.Y <= (this.pos.Y + 50) && ships.pos.Y >= (this.pos.Y - 50))) {
                    return true;
                }
            }
            return false;
        }
        public void nearby() {
            if (danger()) {
                this.accelerate();
            }
            else if (!danger() && this.speed > 0) {
                this.brake();
            }
        }
    }
}
