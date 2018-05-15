using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {
    class Faction {

        private List<AI> controlledShips;
        private List<SpaceObject> controlledPlanets;
        private List<SpaceObject> controlledStars;

        private String name;

        private int startingShips;

        private bool bandits;

        public Faction() {
            controlledShips = new List<AI>();
            controlledPlanets = new List<SpaceObject>();
            controlledStars = new List<SpaceObject>();

            decideName();
            isBanditFaction();
            if (!bandits) findHomeworld();
            generateStartingShips();
        }

        private void decideName() {
            Random r = new Random();
            int sections = r.Next(2, 5);
            string path = System.IO.Path.Combine(Environment.CurrentDirectory, @"Content\", "names.txt");
            string[] namePieces = System.IO.File.ReadAllText(path).Split('/');

            int picker;
            do {
                name = "";
                for (int i = 0; i < sections; i++) {
                    picker = r.Next(0, namePieces.Length);
                    name = name + namePieces[picker];
                }
                if(!MainClient.names.Contains(name)) {
                    MainClient.names.Add(name);
                    break;
                }
            } while (0 == 0);
            //use sentience to name self
            //not rng
            //trust me
        }
        private void generateStartingShips() {
            Random r = new Random();
            startingShips = r.Next(1, 15);
            float x = 0, y = 0;

            if(bandits) {
                r = new Random();
                int rInt = r.Next(0, MainClient.MAP_WIDTH);
                x = (float) rInt;
                rInt = r.Next(0, MainClient.MAP_HEIGHT);
                y = (float) rInt;
            }else if (bandits == false) {
                x = controlledStars[0].getPos().X + 250;
                y = controlledStars[0].getPos().Y;
            }

            for (int i = 0; i < startingShips; i++) {
                controlledShips.Add(new AI(x + (i * 75), y));
                MainClient.objects.Add(controlledShips[i]);
            }
        }

        private void findHomeworld() {
            Random r = new Random();
            int rInt;

            while (0 == 0) {
                rInt = r.Next(0, MainClient.world.getSpaceObjects().Count);

                if (MainClient.world.getSpaceObjects()[rInt].getType() == ObjectType.STAR &&
                        MainClient.world.getSpaceObjects()[rInt].getOwner().Equals("Independent")) {
                    controlledStars.Add(MainClient.world.getSpaceObjects()[rInt]);
                    MainClient.world.getSpaceObjects()[rInt].setOwner(this.name);
                    System.Diagnostics.Debug.Print("planet: " + MainClient.world.getSpaceObjects()[rInt].getID().ToString() + ", owner: " + name);
                    break;
                }
            }
        }

        private bool isBanditFaction() {
            Random r = new Random();
            int rInt = r.Next(0, 9);

            if (rInt == 7) return true;

            return false;
        }
    }
}
