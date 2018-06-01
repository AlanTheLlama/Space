using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space {
    public class Projectile : MovingObject {

        private Vector2 pos;
        private Vector2 angle;
        private float power;
        private float lifetime;
        private int identifier;
        private int radius;

        private ObjectType type;
        Random r = new Random();
        private int originID;

        private bool alive;

        public Projectile(Vector2 pos, float power, Vector2 angle, int origin) {
            this.pos = pos;
            this.power = power;
            this.angle = angle;
            this.lifetime = 100;
            this.identifier = r.Next(0, 1000000);
            this.radius = 2;

            this.type = ObjectType.PROJECTILE;
            this.alive = true;
            this.originID = origin;
        }

        public float getOriginID() {
            return this.originID;
        }

        public float getAngle() {
            if (this.angle.X == 0) {
                if (this.angle.Y > 0) {
                    return Math2.QUARTER_CIRCLE;
                } else {
                    return Math2.THREE_QUARTER_CIRCLE;
                }
            }
            return (float)Math.Sin(this.angle.Y / Math2.getQuadSum(this.angle.X, this.angle.Y));
        }

        public int getID() {
            return this.originID;
        }

        public Vector2 getPos() {
            return this.pos;
        }

        public Texture2D getTexture() {
            return MainClient.laserTex;
        }

        public ObjectType getType() {
            return this.type;
        }

        public float getPower() {
            return this.power;
        }

        public bool isAlive() {
            return this.alive;
        }

        public bool isHit(Object o) {
            if (o.getID() == this.getID()) {
                //Console.WriteLine("Not hitting self");
                return false;
            }
            //Console.WriteLine("Projectile from " + this.originID + " hit " + o.getID());
            return false;//Math2.inRadius(this.getPos(), o.getPos(), this.radius);
        }

        public void setCoords(float x, float y, float rot) {
            this.pos = new Vector2(x, y);
            this.angle = new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot));
        }

        public void update(World w) {
            this.updatePosition(w);
            this.lifetime--;
            if (this.lifetime < 0) {
                this.alive = false;
            }
        }

        public void updatePosition(World w) {
            float x = this.pos.X;
            float y = this.pos.Y;
            x += power * angle.X;
            y += power * angle.Y;
            this.pos = new Vector2(x, y);
        }

        public void getHit(float power) {
            this.alive = false;
            //Console.WriteLine("getHit called on " + this.getID());
        }

        public string getOwner() {
            throw new NotImplementedException();
        }

        public void setOwner(string newOwner) {
            throw new NotImplementedException();
        }

        public string getTask() {
            throw new NotImplementedException();
        }
    }
}
