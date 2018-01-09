using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Space {
    class Asteroid : SpaceObject {
        public int x;
        public int y;

        public Texture2D getImage() {
            return Space.Game1.asteroid;
        }

        public int getXpos() {
            return x;
        }

        public int getYpos() {
            return y;
        }

        public Asteroid(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }
}
