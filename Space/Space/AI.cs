﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space {
    public class AI : MovingObject {

        private float MAX_SPEED = 4;

        //MOVEMENT 
        private Vector2 pos;
        private Vector2 velocity;
        private float aimRotation;
        private float rotSpeed;
        private float forwardForce;
        private float backwardForce;
        private float sideForce;
        private float mass;

        //NON MOVEMENT
        private Vector2 change;
        private float dist;

        //SERVER
        private int identifier;
        Random r = new Random();
        private ObjectType type;

        //GAMEPLAY
        private bool boosting;
        private bool cooling;
        private float radius;
        private float shield;
        private bool alive;
        private float weaponCooldown;
        private float weapons;

        public AI (int x, int y) { 
            this.pos = new Vector2(x, y);
            this.aimRotation = (float)0.5 * (float)Math.PI;           //don't press enter.
            this.velocity = new Vector2(0, 0);
            this.rotSpeed = (float)0.15;
            this.forwardForce = (float)2; 
            this.backwardForce = (float)1; 
            this.sideForce = (float)0.5;
            this.mass = 50;

            this.identifier = r.Next(0, 1000000);
            this.type = ObjectType.AI;

            this.radius = 32;
            this.shield = 100;
            this.alive = true;
            this.cooling = false;
            this.boosting = false;
            this.weaponCooldown = 0;
            this.weapons = 24;
    }

        //GETTERS/SETTERS
        public bool isBoosting()
        {
            return this.boosting;
        }

        public bool isCooling()
        {
            return this.cooling;
        }
        public float getRot()
        {
            return this.aimRotation;
        }

        public int getID() {
            return identifier;
        }

        public Vector2 getPos() {
            return this.pos;
        }

        public ObjectType getType() {
            return this.type;
        }

        public float getAngle() {
            return this.aimRotation;
        }

        public Texture2D getTexture() {
            return MainClient.enemy;
        }

        public void setCoords(float x, float y, float rot) {
            this.pos = new Vector2(x, y);
            this.aimRotation = rot;
        }

        public bool isAlive() {
            return this.alive;
        }

        //MOVEMENT

        public void rotateRight() {
            this.aimRotation = this.aimRotation + this.rotSpeed;
            if (this.aimRotation > 2 * ((float)Math.PI)) {
                this.aimRotation = this.aimRotation - (2 * ((float)Math.PI));
            }

        }

        public void rotateLeft() {
            this.aimRotation = this.aimRotation - this.rotSpeed;
            if (this.aimRotation < 0) {
                this.aimRotation = this.aimRotation + (2 * ((float)Math.PI));
            }
        }
        public void thrust() {
            if (this.getSpeed() == 0) {
                this.velocity.X = (float)Math.Cos(this.aimRotation) * this.forwardForce / this.mass;
                this.velocity.Y = (float)Math.Sin(this.aimRotation) * this.forwardForce / this.mass;
            } else {
                this.velocity.X = this.velocity.X + (float)Math.Cos(this.aimRotation) * this.forwardForce / this.mass;
                this.velocity.Y = this.velocity.Y + (float)Math.Sin(this.aimRotation) * this.forwardForce / this.mass;
                if (this.getSpeed() > MAX_SPEED) {
                    this.scaleSpeed(MAX_SPEED);
                }
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
                this.scaleSpeed(MAX_SPEED);
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
                this.scaleSpeed(MAX_SPEED);
            }
        }

        public void scaleSpeed(float maxSpeed) {
            float scale = maxSpeed / this.getSpeed();
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
            if (this.getSpeed() != 0) {
                if (this.getSpeed() < 0.101) {
                    this.velocity = new Vector2(0, 0);
                } else {
                    this.velocity.X = this.velocity.X - this.velocity.X * this.backwardForce / (this.mass * this.getSpeed());
                    this.velocity.Y = this.velocity.Y - this.velocity.Y * this.backwardForce / (this.mass * this.getSpeed());
                }
            }
        }

        public void boost()
        {
            if (!boosting)
            {
                MAX_SPEED = 4 * MAX_SPEED;
                boosting = true;
            }
        }

        public void noBoost()
        {
            if (boosting)
            {
                MAX_SPEED = MAX_SPEED / 4;
                boosting = false;
                this.cooling = true;
            }
            if (this.getSpeed() > MAX_SPEED)
            {
                this.brake();
                this.brake();
            }
            else if (this.cooling)
            {
                this.cooling = false;
            }
        }

        //MISC

        public void updatePosition(World w)
        {
            Vector2 vec = new Vector2(this.pos.X + this.velocity.X, this.pos.Y + this.velocity.Y);
            float x = this.pos.X;
            float y = this.pos.Y;
            if (vec.X >= 0 || vec.X <= w.getSizeX())
            {
                x = vec.X;
            }
            if (vec.Y >= 0 || vec.Y <= w.getSizeX())
            {
                y = vec.Y;
            }
            this.pos = new Vector2(x, y);
        }

        public void update(World w)
        {
            decide();
            if (weaponCooldown > 0)
            {
                weaponCooldown--;
            }
            this.updatePosition(w);
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

        public Laser fireWeapon(MovingObject mo)
        {
            if (weaponCooldown == 0)
            {
                distToo(mo);
                float x = change.X;
                float y = change.Y;
                Vector2 angle = Math2.getUnitVector(x, y);
                Laser laser = new Laser(this.pos, this.weapons, angle, this.identifier);
                this.weaponCooldown = 4;
                return laser;
            }
            return null;
        }


        //GAMEPLAY

        public int danger() { //Changed this around a bit
            foreach (Object o in MainClient.objects) {
                if (o.getType() == ObjectType.PLAYER) {
                    distToo((MovingObject)o);
                    if (dist <= 50) {
                        return 1;
                    }
                    else if (dist <= 100)
                    {
                        return 2;
                    }
                    else if (dist <= 200)
                    {
                        return 3;
                    }
                }
            }
            return 0;
        }

        public void decide() { //Solely combat scenario
            int temp = danger();

            if (temp == 2) //Most danger
            {
                this.thrust();
            }
            else if (temp == 3)
            {
                this.rotateRight();
                //do rotating stuff
            }
            else if (temp == 1)
            {
                this.boost();
            }

            else if (this.getSpeed() > 0)
            {
                this.brake();
            }
            else 
            {
                this.rotateLeft();
                //Will make you accelerate l8ter
            }
        }

        public bool isHit(Object o) {
            return Math2.inRadius(this.getPos(), o.getPos(), this.radius);
        }

        public void getHit(float power) {
            this.shield -= power;
            if (this.shield <= 0) {
                this.alive = false;
            }
        }
    }
}
