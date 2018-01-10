using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space
{
    public class PlayerShip
    {
        public float MAX_SPEED = 9;

        public Vector2 pos;
        public float aimRotation;
        public float movRotation;
        public float speed;
        public float rotSpeed;
        public float curveSpeed;
        public float acceleration;
        public bool initialized;
        public int identifier;
        Random r = new Random();

        public PlayerShip(Vector2 vec)
        {
            this.pos = vec;
            this.aimRotation = (float)0.5 * (float)Math.PI;
            this.movRotation = -this.aimRotation;
            this.speed = 0;
            this.rotSpeed = (float)0.15;
            this.curveSpeed = (float)0.05;
            this.acceleration = (float)0.4;
            this.initialized = true;
            this.identifier = r.Next(0, 1000000);
        }

        public PlayerShip(Vector2 vec, float rot, int id) {            //for other players
            pos = vec;
            aimRotation = rot;
            identifier = id;
        }

        public int getID() {
            return identifier;
        }
        public float getRot() {
            return this.aimRotation;
        }
        public string dataString() {
            return pos.X.ToString() + "/" + pos.Y.ToString() + "/" + getRot().ToString() + "/" + getID().ToString();
        }
        public void setCoords(float x, float y, float rot) {
            this.pos = new Vector2(x, y);
            this.aimRotation = rot;
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
            this.accelerate();
            if (this.aimRotation != this.movRotation)
            {
                this.turnBody();
            }
        }

        public void accelerate()
        {
            this.speed += this.acceleration;
            if (this.speed > MAX_SPEED)
            {
                this.speed = MAX_SPEED;
            }
        }

        public void turnBody()
        {
            float rot = this.movRotation;
            rot = rot - this.aimRotation;
            if (Math.Abs(rot) > this.curveSpeed)
            {
                if (rot < 0)
                {
                    rot += 2 * (float)Math.PI;
                }
                if (rot < Math.PI)
                {
                    this.movRotation += this.curveSpeed;
                }
                else
                {
                    this.movRotation -= this.curveSpeed;
                }
            }
        }

        public void brake()
        {
            this.speed -= this.acceleration;
            if (this.speed < 0)
            {
                this.speed = 0;
            }
        }

        public void updatePosition()
        {
            this.pos = new Vector2(this.pos.X - (this.speed * (float)Math.Cos(this.movRotation)), this.pos.Y - (this.speed * (float)Math.Sin(this.movRotation)));
        }
    }
}