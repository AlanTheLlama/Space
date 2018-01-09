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

        public PlayerShip(Vector2 vec, float rot, float speed)
        {
            pos = vec;
            rotation = rot;
            this.speed = speed;
        }
    }
}
