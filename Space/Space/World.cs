﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space {
    class World {
        //world generation??
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        public static List<SpaceObject> spaceObjects;

        public World(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;
        }

        public void Generate(int densityMin, int densityMax, int minDistance) {
            Random r = new Random();
            int rInt = r.Next(densityMin, densityMax);
            System.Diagnostics.Debug.WriteLine("Points: " + rInt);

            spaceObjects = new List<SpaceObject>();

            for(int i = 0; i < rInt; i++) {
                rInt = r.Next(1, 4);
                int x = r.Next(0, SizeX);
                int y = r.Next(0, SizeY);

                if (rInt == 1) spaceObjects.Add(new Asteroid(x, y));
                if (rInt == 2) spaceObjects.Add(new Asteroid(x, y)); //only one type of SpaceObject rn lol
                if (rInt == 3) spaceObjects.Add(new Asteroid(x, y));
                if (rInt == 4) spaceObjects.Add(new Asteroid(x, y));
            }
        }
    }
}
