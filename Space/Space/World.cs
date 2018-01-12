using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space {
    public class World {
        //world generation??
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        public List<SpaceObject> spaceObjects;

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
                int type = r.Next(1, 5);
                //System.Diagnostics.Debug.WriteLine("Type: " + type);

                int x = r.Next(0, SizeX);
                int y = r.Next(0, SizeY);

                if (type == 1) spaceObjects.Add(new Asteroid(x, y, 100));
                if (type == 2) spaceObjects.Add(new Asteroid(x, y, 100)); //only one type of SpaceObject rn lol
                if (type == 3) spaceObjects.Add(new Asteroid(x, y, 100));
                if (type == 4) spaceObjects.Add(new Asteroid(x, y, 100));
            }
        }

        public List<SpaceObject> getSpaceObjects() {
            return this.spaceObjects;
        }
    }
}
