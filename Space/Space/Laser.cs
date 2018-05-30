using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {
    public class Laser : Projectile {
        // BASIC PROJECTILE HAS NO EXTRA FEATURES FOR NOW

        public Laser(Vector2 pos, float power, Vector2 angle, int origin) : base(pos, power, angle, origin) {
        }

        public new ObjectType getType() {
            return ObjectType.PROJECTILE;
        }

        public new Texture2D getTexture() {
            return MainClient.laserTex;
        }
    }
}
