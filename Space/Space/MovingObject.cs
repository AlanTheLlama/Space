using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {

    public interface MovingObject : Object {

        void updatePosition(World w);

        void setCoords(float x, float y, float rot);
    }
}
