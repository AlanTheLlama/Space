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

        public PlayerShip(Vector2 vec, float rot, float speed, float rotSpeed, float acceleration)
        {
            pos = vec;
            rotation = rot;
            this.speed = speed;
            this.rotSpeed = rotSpeed;
            this.acceleration = acceleration;
            this.initialized = true;
        }
    }
}
