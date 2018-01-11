using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace Space {
    public class AI {

        public float MAX_SPEED = 4;

        //MOVEMENT 
        public Vector2 pos;
        public Vector2 velocity;
        public float aimRotation;
        public float rotSpeed;
        public float curveSpeed;
        public float forwardForce;
        public float backwardForce;
        public float sideForce;
        public float mass;


        //NON MOVEMENT
        public Vector2 change;
        public float dist;

        public AI (int x, int y) { 
            this.pos = new Vector2(x, y);
            this.aimRotation = (float)0.5 * (float)Math.PI;           //don't press enter.
            this.velocity = new Vector2(0, 0);
            this.rotSpeed = (float)0.15;
            this.curveSpeed = (float)0.05;
            this.forwardForce = (float)2; 
            this.backwardForce = (float)1; 
            this.sideForce = (float)0.5;
            this.mass = 50;
        }
        public Vector2 thisPos() {
            return pos;
        }
        //ALL THE NEW MOVEMENT SYSTEM
        public float getRot()
        {
            return this.aimRotation;
        }
        public void rotateRight()
        {
            this.aimRotation = this.aimRotation + this.rotSpeed;
            if (this.aimRotation > 2 * ((float)Math.PI))
            {
                this.aimRotation = this.aimRotation - (2 * ((float)Math.PI));
            }

        }

        public void rotateLeft()
        {
            this.aimRotation = this.aimRotation - this.rotSpeed;
            if (this.aimRotation < 0)
            {
                this.aimRotation = this.aimRotation + (2 * ((float)Math.PI));
            }
        }

        public void thrust()
        {
            this.velocity.X = this.velocity.X + (float)Math.Cos(this.aimRotation) * this.forwardForce / this.mass;
            this.velocity.Y = this.velocity.Y + (float)Math.Sin(this.aimRotation) * this.forwardForce / this.mass;
            if (this.getSpeed() > MAX_SPEED)
            {
                this.scaleSpeed();
            }
        }

        public void leftThrust()
        {
            float left = this.aimRotation - (float)0.5 * (float)Math.PI;
            if (left > 2 * Math.PI)
            {
                left -= 2 * (float)Math.PI;
            }
            this.velocity.X = this.velocity.X + (float)Math.Cos(left) * this.sideForce / this.mass;
            this.velocity.Y = this.velocity.Y + (float)Math.Sin(left) * this.sideForce / this.mass;
            if (this.getSpeed() > MAX_SPEED)
            {
                this.scaleSpeed();
            }
        }

        public void rightThrust()
        {
            float right = this.aimRotation + (float)0.5 * (float)Math.PI;
            if (right < 0)
            {
                right += 2 * (float)Math.PI;
            }
            this.velocity.X = this.velocity.X + (float)Math.Cos(right) * this.sideForce / this.mass;
            this.velocity.Y = this.velocity.Y + (float)Math.Sin(right) * this.sideForce / this.mass;
            if (this.getSpeed() > MAX_SPEED)
            {
                this.scaleSpeed();
            }
        }

        public void scaleSpeed()
        {
            float scale = MAX_SPEED / this.getSpeed();
            this.velocity.X = this.velocity.X * scale;
            this.velocity.Y = this.velocity.Y * scale;
        }

        public void reverse()
        {
            this.velocity.X = this.velocity.X - (float)Math.Cos(this.aimRotation) * this.sideForce / this.mass;
            this.velocity.Y = this.velocity.Y - (float)Math.Sin(this.aimRotation) * this.sideForce / this.mass;
            if (this.getSpeed() < 0.1)
            {
                this.velocity.X = 0;
                this.velocity.Y = 0;
            }
        }

        public void brake()
        {
            this.velocity.X = this.velocity.X - this.velocity.X * this.backwardForce / (this.mass * this.getSpeed());
            this.velocity.Y = this.velocity.Y - this.velocity.Y * this.backwardForce / (this.mass * this.getSpeed());
            if (Math.Abs(this.velocity.X) < 0)
            {
                this.velocity.X = 0;
            }
            if (Math.Abs(this.velocity.Y) < 0)
            {
                this.velocity.Y = 0;
            }
        }
        public void updatePosition(World w)
        {
            Vector2 vec = new Vector2(this.pos.X + this.velocity.X, this.pos.Y + this.velocity.Y);
            float x = this.pos.X;
            float y = this.pos.Y;
            if (vec.X >= 0 || vec.X <= w.SizeX)
            {
                x = vec.X;
            }
            if (vec.Y >= 0 || vec.Y <= w.SizeX)
            {
                y = vec.Y;
            }
            this.pos = new Vector2(x, y);
        }
        public void update(World w)
        {
            updatePosition(w);
        }

        public float getSpeed()
        {
            return (float)Math.Sqrt(this.velocity.X * this.velocity.X + this.velocity.Y * this.velocity.Y);
        }
        public void distToo(MovingObject mo)
        {
            change.X = mo.getPos().X - this.pos.X;
            change.Y = mo.getPos().Y - this.pos.Y;
            dist = (float)Math.Sqrt(change.X * change.X + change.Y * change.Y);
        }
        public bool danger() {
            foreach (MovingObject mo in Game1.movingObjects) {
                if (mo.getType() == ObjectType.PLAYER) {
                    changeX = Math.Abs(mo.getPos().X - this.pos.X);
                    changeY = Math.Abs(mo.getPos().Y - this.pos.Y);
                    dist = (float)Math.Sqrt(((changeX * changeX) + (changeY * changeY)));
                    if (dist <= 200) {
                        return true;
                    }
                }
            }
            return false;
        }

        public void nearby() { //What to do when a player is detected
            if (danger()) {
                this.thrust();
                System.Diagnostics.Debug.WriteLine("DANGER!");
            }
            else if (!danger() && this.getSpeed() > 0) {
                brake();
            }
        }
    }
}
