using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {
    public enum MineReturn {
        IRON = 0,
        GEMS = 1
    };

    public class Planet : SpaceObject {
        private Vector2 pos;
        private float radius;
        private float health;
        private bool alive;
        private ObjectType type;
        public static String owner;

        private Random r = new Random();

        //Resources
        private float iron;
        private float gems;

        public Planet(float x, float y, float radius) {
            this.pos.X = x;
            this.pos.Y = y;
            this.radius = radius;
            this.health = 10000;
            this.alive = true;
            this.type = ObjectType.MINING_PLANET;
            owner = "Independent";

            this.iron = r.Next(100, 1000);
            this.gems = r.Next(100, 1000);

        }

        public Texture2D getTexture() {
            return Space.MainClient.planet; //down with firefox
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

        public String getOwner() {
            return owner;
        }

        public void setOwner(String newOwner) {
            owner = newOwner;
        }

        public void update(World w) {; }

        public float[] mine(float power) {
            float[] ret = { 0, 0 };
            ret[(int)MineReturn.IRON] = (float)0.01*r.Next(1*(int)power, 5*(int)power);
            ret[(int)MineReturn.GEMS] = (float)0.01*r.Next(1 * (int)power, 2 * (int)power);
            if (this.iron < ret[(int)MineReturn.IRON]) {
                ret[(int)MineReturn.IRON] = this.iron;
            }
            if (this.gems < ret[(int)MineReturn.GEMS]) {
                ret[(int)MineReturn.GEMS] = this.gems;
            }
            this.iron -= ret[(int)MineReturn.IRON];
            this.gems -= ret[(int)MineReturn.GEMS];
            return ret;
        }
    }
}

