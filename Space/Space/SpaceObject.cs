using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Space {
    public interface SpaceObject {
        Texture2D getImage();         //not worth much now but setting this up for future
        float getXpos();
        float getYpos();
        float getRot();
        float getRadius();
    }
}
