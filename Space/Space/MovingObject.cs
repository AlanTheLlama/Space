using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {
    public interface MovingObject {

        void updatePosition(World w);

        void update(World w);

        Vector2 getPos();

        int getType();

        float getAngle();

        int getID();

        void setCoords(float x, float y, float rot);
    }
}
