using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Space {
    class Asteroid : SpaceObject {
        private float x;
        private float y;
        private float radius;

        public Texture2D getImage() {
            return Space.MainClient.asteroid;
        }

        public float getXpos() {
            return x;
        }

        public float getYpos() {
            return y;
        }

        public float getRot() {
            return 0;
        }

        public Asteroid(float x, float y, float radius) {
            this.x = x;
            this.y = y;
            this.radius = radius;
        }

        public float getRadius() {
            return this.radius;
        }
    }
}
