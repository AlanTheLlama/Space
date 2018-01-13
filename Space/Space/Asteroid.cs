using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space {
    class Asteroid : SpaceObject {
        private Vector2 pos;
        private float radius;
        private float health;
        private bool alive;
        private ObjectType type;

        public Asteroid(float x, float y, float radius) {
            this.pos.X = x;
            this.pos.Y = y;
            this.radius = radius;
            this.health = 100;
            this.alive = true;
            this.type = ObjectType.NO_INTERACTION_OBJECT;
        }

        public Texture2D getTexture() {
            return Space.MainClient.asteroid;
        }

        public Vector2 getPos() {
            return pos;
        }

        public int getID() {
            return 0;
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
            return Math2.inRadius(this.getPos(), o.getPos(), this.radius);
        }

        public void getHit(float power) {
            this.health -= power;
            if (this.health <= 0) {
                this.alive = false;
            }
        }

        public void update(World w) {; }
    }
}
