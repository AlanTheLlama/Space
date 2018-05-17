using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space {
    public interface SpaceObject : Object {
        float getRot();
        float getRadius();
        int getInfluencers();
        int getInfluenceRadius();
        void findFactionStars();
    }
}
