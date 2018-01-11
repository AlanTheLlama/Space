using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space
{
    public class PlayerShip : MovingObject {
        public float MAX_SPEED = 9;

        // MOVEMENT
        public Vector2 pos;
        public Vector2 velocity;
        public float aimRotation;
        public float rotSpeed;
        public float curveSpeed;
        public float forwardForce;
        public float backwardForce;
        public float sideForce;
        public float mass;

        // SERVER
        public bool initialized;
        public int identifier;
        Random r = new Random();

        // STATS (not implemented yet)
        public float power;
        public float shield;
        public float weapons;
        public float weaponCooldown;

        //DEFINITION
        private ObjectType type;

        public PlayerShip(Vector2 vec)
        {
            this.pos = vec;
            this.aimRotation = (float)0.5 * (float)Math.PI;           //don't press enter.
            this.velocity = new Vector2(0, 0);
            this.rotSpeed = (float)0.15;
            this.curveSpeed = (float)0.05;
            this.forwardForce = (float)2;
            this.backwardForce = (float)1;
            this.sideForce = (float)0.5;
            this.mass = 5;

            this.initialized = true;
            this.identifier = r.Next(0, 1000000);

            this.power = 10;
            this.shield = 2;
            this.weapons = 8;
            this.weaponCooldown = 300;

            type = ObjectType.PLAYER;
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
            return pos.X.ToString() + "/" + pos.Y.ToString() + "/" + getRot().ToString() + "/" + getID().ToString() + "/";
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
            this.velocity.X = this.velocity.X + (float)Math.Cos(this.aimRotation) * this.forwardForce / this.mass;
            this.velocity.Y = this.velocity.Y + (float)Math.Sin(this.aimRotation) * this.forwardForce / this.mass;
            if (this.getSpeed() > MAX_SPEED) {
                this.scaleSpeed();
            }
        }

        public void leftThrust() {
            float left = this.aimRotation - (float)0.5 * (float)Math.PI;
            if (left > 2 * Math.PI) {
                left -= 2 * (float)Math.PI;
            }
            this.velocity.X = this.velocity.X + (float)Math.Cos(left) * this.sideForce / this.mass;
            this.velocity.Y = this.velocity.Y + (float)Math.Sin(left) * this.sideForce / this.mass;
            if (this.getSpeed() > MAX_SPEED) {
                this.scaleSpeed();
            }
        }

        public void rightThrust() {
            float right = this.aimRotation + (float)0.5 * (float)Math.PI;
            if (right < 0) {
                right += 2 * (float)Math.PI;
            }
            this.velocity.X = this.velocity.X + (float)Math.Cos(right) * this.sideForce / this.mass;
            this.velocity.Y = this.velocity.Y + (float)Math.Sin(right) * this.sideForce / this.mass;
            if (this.getSpeed() > MAX_SPEED) {
                this.scaleSpeed();
            }
        }

        public void scaleSpeed() {
            float scale = MAX_SPEED / this.getSpeed();
            this.velocity.X = this.velocity.X * scale;
            this.velocity.Y = this.velocity.Y * scale;
        }

        public void reverse() {
            this.velocity.X = this.velocity.X - (float)Math.Cos(this.aimRotation) * this.sideForce / this.mass;
            this.velocity.Y = this.velocity.Y - (float)Math.Sin(this.aimRotation) * this.sideForce / this.mass;
            if (this.getSpeed() < 0.1) {
                this.velocity.X = 0;
                this.velocity.Y = 0;
            }
        }

        public void brake() {
            this.velocity.X = this.velocity.X - this.velocity.X * this.backwardForce / (this.mass * this.getSpeed());
            this.velocity.Y = this.velocity.Y - this.velocity.Y * this.backwardForce / (this.mass * this.getSpeed());
            if (Math.Abs(this.velocity.X) < 0) {
                this.velocity.X = 0;
            }
            if (Math.Abs(this.velocity.Y) < 0) {
                this.velocity.Y = 0;
            }
        }

        public float getSpeed() {
            return (float)Math.Sqrt(this.velocity.X * this.velocity.X + this.velocity.Y * this.velocity.Y);
        }

        public void updatePosition(World w)
        {
            Vector2 vec = new Vector2(this.pos.X + this.velocity.X, this.pos.Y + this.velocity.Y);
            float x = this.pos.X;
            float y = this.pos.Y;
            if (vec.X >= 0 || vec.X <= w.SizeX) {
                x = vec.X;
            }
            if (vec.Y >= 0 || vec.Y <= w.SizeX) {
                y = vec.Y;
            }
            this.pos = new Vector2(x, y);
        }

        public Laser fireWeapon(Vector2 mouse) {
            Laser laser = new Laser(this.pos, this.weapons, (float)Math.Tan((mouse.X - this.pos.X) / (mouse.Y - this.pos.Y)));
            // TODO - fix mouse position
            //System.Diagnostics.Debug.WriteLine(laser.angle.ToString());
            return laser;
        }

        public void update(World w) {
            updatePosition(w);
            this.weaponCooldown--;
        }

        public Vector2 getPos() {
            return this.pos;
        }

        public ObjectType getType() {
            return type;
        }

        public float getAngle() {
            return this.aimRotation;
        }
    }
}