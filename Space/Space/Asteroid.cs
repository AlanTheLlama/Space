﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space {
    public class Asteroid : SpaceObject {
        private Vector2 pos;
        private float radius;
        private float health;
        private bool alive;
        private ObjectType type;

        public Rectangle getCircle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Asteroid(float x, float y, float radius) {
            this.pos.X = x;
            this.pos.Y = y;
            this.radius = radius;
            this.health = 100;
            this.alive = true;
            this.type = ObjectType.ASTEROID;
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
            return false;
            //return Math2.inRadius(this.getPos(), o.getPos(), this.radius);
        }

        public void getHit(float power) {
            this.health -= power;
            if (this.health <= 0) {
                this.alive = false;
            }
        }

        public void update(World w) {; }

        public string getOwner() {
            return "this is an asteroid dummy";
        }

        public void setOwner(string newOwner) {
            throw new NotImplementedException();
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
