﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space {
    public class World {
        private int MIN_DIST_FOR_ASTEROIDS = 300;
        private int MIN_DIST_FOR_PLANETS = 1400;
        private int MIN_DIST_FOR_SOL_SYS = 3000;
        private int SOL_SYS_RADIUS = 1800;

        private int SizeX { get; set; }
        private int SizeY { get; set; }
        private Vector2 uniCenter;

        public List<SpaceObject> spaceObjects;
        public List<Asteroid> asteroidJail;
        public List<Faction> factions;
        public List<Star> starList;
        private Vector2[] solarSystemList;

        public World(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;

            uniCenter = new Vector2(getSizeX() / 2, getSizeY() / 2);
            asteroidJail = new List<Asteroid>();
        }

        public void Generate(int densityMin, int densityMax) {
            Random r = new Random();
            int rInt = r.Next(densityMin, densityMax);
            int solarSystems = rInt / 25;
            int factionNumber = rInt / 100;

            System.Diagnostics.Debug.Print(factionNumber.ToString());

            factions = new List<Faction>();
            spaceObjects = new List<SpaceObject>();
            solarSystemList = new Vector2[solarSystems];
            starList = new List<Star>();

            for (int i = 0; i < solarSystems; i++) {
                int x = r.Next(0, SizeX);
                int y = r.Next(0, SizeY);

                while (tooCloseSolSys(new Vector2(x, y), i)) {
                    x = r.Next(0, SizeX);
                    y = r.Next(0, SizeY);
                }

                solarSystemList[i] = new Vector2(x, y);
                spaceObjects.Add(new Star(x, y, 200, i));
                starList.Add((Star) spaceObjects[i]);
            }

            for(int i = 0; i < rInt; i++) {
                int type = r.Next(1, 9);
                if (type > 1) {
                    float radius = 30;
                    int x = r.Next(0, SizeX);
                    int y = r.Next(0, SizeY);

                    while (tooCloseAst(new Vector2(x, y), radius)) {
                        x = r.Next(0, SizeX);
                        y = r.Next(0, SizeY);
                    }

                    asteroidJail.Add(new Asteroid(x, y, radius));
                } else {
                    int solSys = r.Next(0, solarSystems);
                    Vector2 center = solarSystemList[solSys];

                    float radius = 100;
                    int x = r.Next((int)center.X - SOL_SYS_RADIUS, (int)center.X + SOL_SYS_RADIUS);
                    int y = r.Next((int)center.Y - SOL_SYS_RADIUS, (int)center.Y + SOL_SYS_RADIUS);

                    while (tooCloseAst(new Vector2(x, y), radius)) {
                        x = r.Next((int)center.X - SOL_SYS_RADIUS, (int)center.X + SOL_SYS_RADIUS);
                        y = r.Next((int)center.Y - SOL_SYS_RADIUS, (int)center.Y + SOL_SYS_RADIUS);
                    }

                    spaceObjects.Add(new Planet(x, y, radius, i));
                }
            }

            for(int i = 0; i < factionNumber; i++) {
                factions.Add(new Faction());
            }

            foreach(SpaceObject so in spaceObjects) {           //must run AFTER factions and planets are finished creating
                if(so.getType() == ObjectType.MINING_PLANET) {
                    so.findFactionStars();
                }
            }

            foreach (Faction f in factions) {
                f.findContestedPlanets();
            }

            foreach (Faction f in factions) {
                f.displayInfo();
            }
        }

        public void generateTestWorld() {                            //test world
            int factionNumber = 2;
            int solarSystems = 15;

            factions = new List<Faction>();
            spaceObjects = new List<SpaceObject>();
            solarSystemList = new Vector2[solarSystems];
            starList = new List<Star>();
            
            addStar(uniCenter.X - 5000, uniCenter.Y, 0);
            addPlanet(solarSystemList[0].X + 1000, (int)solarSystemList[0].Y + 100, 1);
            addPlanet(solarSystemList[0].X - 2000, (int)solarSystemList[0].Y + 500, 2);
            addPlanet(solarSystemList[0].X + 700, (int)solarSystemList[0].Y - 300, 3);

            addStar(uniCenter.X + 5000, uniCenter.Y, 4);
            addPlanet(solarSystemList[4].X + 1000, (int)solarSystemList[4].Y + 100, 5);
            addPlanet(solarSystemList[4].X - 2000, (int)solarSystemList[4].Y + 500, 6);
            addPlanet(solarSystemList[4].X + 700, (int)solarSystemList[4].Y - 300, 7);

            addStar(uniCenter.X, uniCenter.Y - 5000, 8);
            addPlanet(solarSystemList[8].X + 1000, (int)solarSystemList[8].Y + 100, 9);
            addPlanet(solarSystemList[8].X - 2000, (int)solarSystemList[8].Y + 500, 10);
            addPlanet(solarSystemList[8].X + 700, (int)solarSystemList[8].Y - 300, 11);

            addStar(uniCenter.X, uniCenter.Y + 5000, 12);
            addPlanet(solarSystemList[12].X + 1000, (int)solarSystemList[12].Y + 100, 13);
            addPlanet(solarSystemList[12].X - 2000, (int)solarSystemList[12].Y + 500, 14);
            addPlanet(solarSystemList[12].X + 700, (int)solarSystemList[12].Y - 300, 15);

            addPlanet(MainClient.world.getSizeX() / 2, MainClient.world.getSizeY() / 2, 16);
            asteroidJail.Add(new Asteroid(30, 30, 10));

            //Faction Initialization
            for (int i = 0; i < factionNumber; i++) {
                factions.Add(new Faction());
            }

            foreach (SpaceObject so in spaceObjects) {           //must run AFTER factions and planets are finished creating
                if (so.getType() == ObjectType.MINING_PLANET) {
                    so.findFactionStars();
                }
            }

            foreach (Faction f in factions) {
                f.findContestedPlanets();
            }

            foreach (Faction f in factions) {
                f.displayInfo();
            }
        }

        private void addStar(float x, float y, int i) {
            solarSystemList[i] = new Vector2(x, y);
            spaceObjects.Add(new Star(x, y, 200, i));
            starList.Add((Star)spaceObjects[i]);
        }

        private void addPlanet(float x, float y, int i) {
            spaceObjects.Add(new Planet(x, y, 100, i));
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

        private bool tooCloseSolSys(Vector2 newSolSys, int i) {
            for (int index = 0; index < i; index++) {
                if (Math2.inRadius(newSolSys, solarSystemList[index], MIN_DIST_FOR_SOL_SYS)) {
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
