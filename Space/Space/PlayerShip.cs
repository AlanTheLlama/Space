using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space
{
    public class PlayerShip : MovingObject {
        public float MAX_SPEED = 7;
        public float COOLDOWN = 20;
        public float MAX_SPEED_TO_LAND = 2;

        // MOVEMENT
        private Vector2 pos;
        private Vector2 velocity;
        private float aimRotation;
        private float rotSpeed;
        private float forwardForce;
        private float backwardForce;
        private float sideForce;
        private float mass;

        // STATUS
        private bool initialized;
        private bool landing;
        private SpaceObject landedOn;
        private bool alive;

        private float iron;
        private float gems;

        // SERVER
        private int identifier;
        Random r = new Random();

        // STATS (not implemented yet)
        private float shield;
        private float weapons;
        private float weaponCooldown;
        private float radius;
        private float miningEquipment;

        //DEFINITION
        private ObjectType type;

        public PlayerShip(Vector2 vec)
        {
            this.pos = vec;
            this.aimRotation = (float)0.5 * (float)Math.PI;           //don't press enter.
            this.velocity = new Vector2(0, 0);
            this.rotSpeed = (float)0.15;
            this.forwardForce = (float)2;
            this.backwardForce = (float)1;
            this.sideForce = (float)0.5;
            this.mass = 5;

            this.initialized = true;
            this.landing = false;
            this.alive = true;

            this.iron = 0;
            this.gems = 0;

            this.identifier = r.Next(0, 1000000);
            
            this.shield = 100;
            this.weapons = 12;
            this.weaponCooldown = 0;
            this.radius = 20;
            this.miningEquipment = 10;

            type = ObjectType.PLAYER;
        }

        public PlayerShip(Vector2 vec, float rot, int id) {            //for other players
            pos = vec;
            aimRotation = rot;
            identifier = id;
        }

        //GETTERS/SETTERS

        public int getID() {
            return identifier;
        }

        public float getRot() {
            return this.aimRotation;
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

        public Texture2D getTexture() {
            return MainClient.ship;
        }

        public bool isInitialized() {
            return this.initialized;
        }

        public string dataString() {
            return pos.X.ToString() + "/" + pos.Y.ToString() + "/" + getRot().ToString() + "/" + getID().ToString() + ";";
        }

        public bool isLanding() {
            return this.landing;
        }

        public void setCoords(float x, float y, float rot) {
            this.pos = new Vector2(x, y);
            this.aimRotation = rot;
        }

        public bool isAlive() {
            return this.alive;
        }

        //MOVEMENT

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
        }

        public float getSpeed() {
            return Math2.getQuadSum(this.velocity.X, this.velocity.Y);
        }

        public void updatePosition(World w)
        {
            Vector2 vec = new Vector2(this.pos.X + this.velocity.X, this.pos.Y + this.velocity.Y);
            float x = this.pos.X;
            float y = this.pos.Y;
            if (vec.X >= 0 || vec.X <= w.getSizeX()) {
                x = vec.X;
            }
            if (vec.Y >= 0 || vec.Y <= w.getSizeX()) {
                y = vec.Y;
            }
            this.pos = new Vector2(x, y);
        }

        //GAMEPLAY

        public Laser fireWeapon(Vector2 mouse) {
            if (weaponCooldown == 0) {
                float x = mouse.X - 400;
                float y = mouse.Y - 240;
                Vector2 angle = Math2.getUnitVector(x, y);
                Laser laser = new Laser(this.pos, this.weapons, angle, this.identifier);
                this.weaponCooldown = COOLDOWN;
                return laser;
            }
            return null;
        }

        public void update(World w) {
            if (this.landing && this.getSpeed() > 0) {
                this.brake();
            }
            if (landedOn != null) {
                if (!Math2.inRadius(this.pos, landedOn.getPos(), landedOn.getRadius()) || !landedOn.isAlive()) {
                    this.takeOff();
                }
            }

            updatePosition(w);
            if (weaponCooldown > 0) {
                this.weaponCooldown--;
            }
        }

        public void land(SpaceObject so) {
            if (!landing) {
                landing = true;
            }
            if (this.getSpeed() > 0) {
                this.brake();
            }
            this.landedOn = so;
        }

        public void takeOff() {
            if (landing) {
                landing = false;
            }
            this.landedOn = null;
        }

        public bool isHit(Object o) {
            Projectile p = (Projectile)o;
            if (p.getOriginID() == this.identifier) {
                return false;
            }
            return Math2.inRadius(this.getPos(), o.getPos(), this.radius);
        }

        public void getHit(float power) {
            this.shield -= power;
            if (this.shield <= 0) {
                this.alive = false;
            }
        }

        public void mine() {
            if (landedOn.getType() == ObjectType.MINING_PLANET) {
                Planet planet = (Planet)landedOn;
                float[] mine = planet.mine(miningEquipment);
                this.iron += mine[(int)MineReturn.IRON];
                this.gems += mine[(int)MineReturn.GEMS];
                Console.WriteLine(this.iron.ToString() + ", " + this.gems.ToString());
            }
        }
    }
}