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
        PROJECTILE,
        ASTEROID,
        MINING_PLANET,
        STAR
    };

    public interface Object {
        //Rectangle getCircle { get; set; }
        Circle getCircle();
        bool isHit(Object o);
        Vector2 getPos();
        void getHit(float power);
        bool isAlive();
        void update(World w);
        ObjectType getType();
        Texture2D getTexture();
        int getID();
        float getAngle();
        String getOwner();
        void setOwner(string newOwner);
        String getTask();
    }
}
