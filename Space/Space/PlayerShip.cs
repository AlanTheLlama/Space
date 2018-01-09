using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space
{
    public class PlayerShip
    {
        public Vector2 pos { get; set; }
        public float rotation { get; set; }
        public float speed;
        public float rotSpeed;
        public float acceleration;
        public bool initialized;
        public int identifier;
        Random r = new Random();

        public PlayerShip(Vector2 vec, float rot, float speed, float rotSpeed, float acceleration)
        {
            pos = vec;
            rotation = rot;
            this.speed = speed;
            this.rotSpeed = rotSpeed;
            this.acceleration = acceleration;
            this.initialized = true;
            this.identifier = r.Next(0, 1000000);
        }

        public PlayerShip(Vector2 vec, float rot, int id) {            //for other players
            pos = vec;
            rotation = rot;
            identifier = id;
        }

        public int getID() {
            return identifier;
        }
        public float getRot() {
            return this.rotation;
        }
        public string dataString() {
            return pos.X.ToString() + "/" + pos.Y.ToString() + "/" + getRot().ToString() + "/" + getID().ToString();
        }
        public void setCoords(float x, float y, float rot) {
            this.pos = new Vector2(x, y);
            this.rotation = rot;
        }
    }
}
