﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {

    public enum ObjectType {
        PLAYER,
        AI,
        PROJECTILE
    };

    public interface MovingObject {

        void updatePosition(World w);

        void update(World w);

        Vector2 getPos();

        ObjectType getType();

        Texture2D GetTexture();

        float getAngle();

        int getID();

        void setCoords(float x, float y, float rot);
    }
}
