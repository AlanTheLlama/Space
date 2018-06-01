using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {
    public class Star : SpaceObject {
        private Vector2 pos;
        private float radius;
        private float health;
        private bool alive;
        private ObjectType type;
        private int identifier;
        private string owner = "Independent";

        public Rectangle getCircle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Star(float x, float y, float radius, int identifier) {
            this.pos.X = x;
            this.pos.Y = y;
            this.radius = radius;
            this.health = 100000;
            this.alive = true;
            this.type = ObjectType.STAR;
            this.identifier = identifier;
        }

        public Texture2D getTexture() {
            return MainClient.star;
        }

        public Vector2 getPos() {
            return pos;
        }

        public int getID() {
            return this.identifier;
        }

        public float getRot() {
            return 0;
        }

        public float getRadius() {
            return this.radius;
        }

        public bool isAlive() {
            return this.alive;
        }

        public float getAngle() {
            return 0;
        }

        public ObjectType getType() {
            return this.type;
        }

        public bool isHit(Object o) {
            /*if (o.getType() == ObjectType.PROJECTILE) {
                return Math2.inRadius(this.getPos(), o.getPos(), this.radius);
            }*/
            return false;
        }

        public void getHit(float power) {
            this.health -= power;
            if (this.health <= 0) {
                this.alive = false;
            }
        }

        public void update(World w) {; }

        public string getOwner() {
            return owner;
        }

        public void setOwner(string newOwner) {
            owner = newOwner;
        }

        public int getInfluencers() {
            throw new NotImplementedException();
        }

        public void findFactionStars() {
            throw new NotImplementedException();
        }

        public int getInfluenceRadius() {
            throw new NotImplementedException();
        }

        public string getTask() {
            throw new NotImplementedException();
        }

        Circle Object.getCircle() {
            throw new NotImplementedException();
        }
    }
}
