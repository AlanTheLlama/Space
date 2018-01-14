using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space {
    public class World {
        private int MIN_DIST_FOR_ASTEROIDS = 200;
        private int MIN_DIST_FOR_PLANETS = 800;
        private int MIN_DIST_FOR_GALAXIES = 2000;
        private int GALAXY_RADIUS = 1400;

        private int SizeX { get; set; }
        private int SizeY { get; set; }

        private List<SpaceObject> spaceObjects;
        private Vector2[] galaxyList;

        public World(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;
        }

        public void Generate(int densityMin, int densityMax) {
            Random r = new Random();
            int rInt = r.Next(densityMin, densityMax);
            int galaxies = rInt / 25;

            spaceObjects = new List<SpaceObject>();
            galaxyList = new Vector2[galaxies];

            for (int i = 0; i < galaxies; i++) {
                int x = r.Next(0, SizeX);
                int y = r.Next(0, SizeY);

                while (tooCloseGal(new Vector2(x, y), i)) {
                    x = r.Next(0, SizeX);
                    y = r.Next(0, SizeY);
                }

                galaxyList[i] = new Vector2(x, y);
                spaceObjects.Add(new Star(x, y, 400));
            }

            for(int i = 0; i < rInt; i++) {
                int type = r.Next(1, 9);
                if (type > 1) {
                    float radius = 100;
                    int x = r.Next(0, SizeX);
                    int y = r.Next(0, SizeY);

                    while (tooCloseAst(new Vector2(x, y), radius)) {
                        x = r.Next(0, SizeX);
                        y = r.Next(0, SizeY);
                    }

                    spaceObjects.Add(new Asteroid(x, y, radius));
                } else {
                    int gal = r.Next(0, galaxies);
                    Vector2 center = galaxyList[gal];

                    float radius = 30;
                    int x = r.Next((int)center.X - GALAXY_RADIUS, (int)center.X + GALAXY_RADIUS);
                    int y = r.Next((int)center.Y - GALAXY_RADIUS, (int)center.Y + GALAXY_RADIUS);

                    while (tooCloseAst(new Vector2(x, y), radius)) {
                        x = r.Next((int)center.X - GALAXY_RADIUS, (int)center.X + GALAXY_RADIUS);
                        y = r.Next((int)center.Y - GALAXY_RADIUS, (int)center.Y + GALAXY_RADIUS);
                    }

                    spaceObjects.Add(new Planet(x, y, radius));
                }
            }
        }

        private bool tooCloseAst(Vector2 newSpaceObject, float rad) {
            foreach (SpaceObject so in spaceObjects) {
                if (Math2.inRadius(newSpaceObject, so.getPos(), rad + so.getRadius() + MIN_DIST_FOR_ASTEROIDS)) {
                    return true;
                }
            }
            return false;
        }

        private bool tooClosePla(Vector2 newSpaceObject, float rad) {
            foreach (SpaceObject so in spaceObjects) {
                if (Math2.inRadius(newSpaceObject, so.getPos(), rad + so.getRadius() + MIN_DIST_FOR_PLANETS)) {
                    return true;
                }
            }
            return false;
        }

        private bool tooCloseGal(Vector2 newGal, int i) {
            for (int index = 0; index < i; index++) {
                if (Math2.inRadius(newGal, galaxyList[index], MIN_DIST_FOR_GALAXIES)) {
                    return true;
                }
            }
            return false;
        }

        public List<SpaceObject> getSpaceObjects() {
            return this.spaceObjects;
        }

        public int getSizeX() {
            return this.SizeX;
        }

        public int getSizeY() {
            return this.SizeY;
        }
    }
}
