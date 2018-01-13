using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {
    public class Laser : MovingObject {
        // TODO

        public Vector2 pos;
        public Vector2 angle;
        public float power;
        public float lifetime;
        public int identifier;
        

        private ObjectType type;

        public Laser(Vector2 pos, float power, Vector2 angle) {
            this.pos = pos;
            this.power = power;
            this.angle = angle;
            this.lifetime = 1000;

            type = ObjectType.PROJECTILE;
        }

        public void updatePosition(World w) {
            float x = this.pos.X;
            float y = this.pos.Y;
            x += power * angle.X;
            y += power * angle.Y;
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
            if (angle.X == 0 && angle.Y == 0) {
                return 0;
            }
            return (float)Math.Sin(angle.Y / Math2.getQuadSum(angle.X, angle.Y));
        }

        public int getID() {
            return this.identifier;
        }

        public void setCoords(float x, float y, float rot) {
            this.pos = new Vector2(x, y);
            this.angle = new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot));
        }

        public Texture2D GetTexture() {
            return MainClient.laserTex;
        }
    }
}
