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
        public int id;
        public int influenceRadius;
        public int influencers;
        public Circle rad;

        //Resources
        private float iron;
        private float gems;

        public Rectangle getCircle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Planet(float x, float y, float radius, int id) {
            this.pos.X = x;
            this.pos.Y = y;
            this.radius = radius;
            this.health = 10000;
            this.alive = true;
            this.type = ObjectType.MINING_PLANET;
            this.id = id;
            owner = "Independent";
            this.influenceRadius = 7500;

            this.iron = MainClient.r.Next(100, 1000);
            this.gems = MainClient.r.Next(100, 1000);

            this.rad = new Circle((int)this.pos.X + (int)this.radius, (int)this.pos.Y + (int)this.radius, (int)this.radius);
        }

        public Texture2D getTexture() {
            return Space.MainClient.planet; //down with firefox
        }

        public Vector2 getPos() {
            return pos;
        }

        public int getID() {
            return id;
        }

        public int getInfluencers() {
            return influencers;
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
            
            return false;
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
            ret[(int)MineReturn.IRON] = (float)0.01* MainClient.r.Next(1*(int)power, 5*(int)power);
            ret[(int)MineReturn.GEMS] = (float)0.01* MainClient.r.Next(1 * (int)power, 2 * (int)power);
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

        public void findFactionStars() {
            foreach(Star star in MainClient.world.starList) {
                if (distanceTo(star.getPos(), this.getPos()) < influenceRadius
                    && !star.getOwner().Equals("Independent")) {
                    influencers += 1;
                    if(influencers > 1)
                        Console.WriteLine("Found star #" + star.getID() + " " + distanceTo(star.getPos(), this.getPos()).ToString() + " units away, owned by " + star.getOwner() + "\nNearby Occupied Stars: " + influencers);
                }
            }
        }

        public float distanceTo(Vector2 v1, Vector2 v2) {
            Vector2 change;
            change.X = v1.X - v2.X;
            change.Y = v1.Y - v2.Y;
            return (float)Math.Sqrt(change.X * change.X + change.Y * change.Y);
        }

        public int getInfluenceRadius() {
            return influenceRadius;
        }

        public string getTask() {
            throw new NotImplementedException();
        }

        Circle Object.getCircle() {
            throw new NotImplementedException();
        }
    }
}

