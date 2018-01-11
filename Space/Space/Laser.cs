using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {
    public class Laser : MovingObject {
        // TODO

        public Vector2 pos;
        public float power;
        public float angle;
        public float lifetime;
        public int identifier;

        private ObjectType type;

        public Laser(Vector2 pos, float power, float angle) {
            this.pos = pos;
            this.power = power;
            this.angle = angle;
            this.lifetime = 1000;

            type = ObjectType.PROJECTILE;
        }

        public void updatePosition(World w) {
            float x = this.pos.X;
            float y = this.pos.Y;
            x += power * (float)Math.Cos(angle);
            y += power * (float)Math.Sin(angle);
            this.pos = new Vector2(x, y);
        }
        
        public void update(World w) {
            updatePosition(w);
        }

        public Vector2 getPos() {
            return this.pos;
        }

        public ObjectType getType() {
            return type;
        }

        public float getAngle() {
            return this.angle;
        }

        public int getID() {
            return this.identifier;
        }

        public void setCoords(float x, float y, float rot) {
            this.pos = new Vector2(x, y);
            this.angle = rot;
        }
    }
}
