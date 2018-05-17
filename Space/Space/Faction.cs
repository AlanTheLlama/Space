using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space {
    public class Faction {

        public List<AI> controlledShips;
        public List<AI> military;
        public List<AI> workers;
        public List<SpaceObject> controlledPlanets;
        public List<SpaceObject> contestedPlanets;
        public List<SpaceObject> controlledStars;
        private enum Tasks { PATROL, ATTACK, DEFEND, MINE, SCOUT}

        private String name;

        private int militaristic, economical, aggression;          //characteristics that determine ship composition etc.
        private int patrolDist = 50;

        private int startingShips;

        private bool bandits;

        public Faction() {
            this.controlledShips = new List<AI>();
            this.military = new List<AI>();
            this.workers = new List<AI>();
            this.controlledPlanets = new List<SpaceObject>();
            this.contestedPlanets = new List<SpaceObject>();
            this.controlledStars = new List<SpaceObject>();

            decideName();
            isBanditFaction();
            if (!bandits) {
                findHomeStar();
                findLocalPlanets();
            }
            generateStartingShips();

            Random r = new Random();

            this.militaristic = r.Next(0, 9);
            this.economical = 10 - this.militaristic;
            this.aggression = r.Next(-5, 5);

            assignShipRoles();
        }

        public void update() {
            think();
        }

        private void think() {          //that's scary
            if (this.contestedPlanets.Count > 0) {
                contestPlanets();                    //also sends a couple military units to patrol (possibly)
            } else patrolControlled();
        }

        private void assignMilitarySingleTarget(Object target) {

        }

        private void assignMilitaryMultiTarget(Object[] targets, bool randomTarget) {}

        private void contestPlanets(Object[] targets, bool randomTarget) {}
        
        private void assignMilitaryMultiTarget(Object[] targets) {

        }

        private void contestPlanets() {
            Random r = new Random();
            int ranTarget;
            int ranDist;
            foreach (AI ship in military) {
                ranDist = r.Next(0, 100);
                if (ranDist > 50) {
                    ranTarget = r.Next(contestedPlanets.Count);
                    ship.addAttackTarget(contestedPlanets[ranTarget]);
                    ship.setState(AI.State.COMBAT);
                } else { patrolControlled(); }
            }
        }

        private void patrolControlled() {
            Random r = new Random();
            int roll;

            foreach(AI ship in military) {
                roll = r.Next(0, 100);
                if (roll >= distribute(Tasks.PATROL)) {
                    //Console.WriteLine(this.name + " set a ship to patrol");
                    ship.setState(AI.State.PATROLLING);
                }
            }
        }

        private void patrolControlled(AI ship) {
            ship.setState(AI.State.PATROLLING);
            ship.addPatrolTarget((SpaceObject) controlledPlanets);
        }

        private int distribute(Tasks task) {   //Faction keeps track of when tasks are successful and unsuccessful to alter the distribution for the future
            Random r = new Random();
            int roll;
            switch(task) {
                case Tasks.PATROL:
                    roll = r.Next(-10, 10);
                    return patrolDist + roll;

                case Tasks.ATTACK:

                    return 80;
            }

            return 50;
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
                    this.name = this.name + namePieces[picker];
                }
                if(!MainClient.names.Contains(this.name)) {
                    MainClient.names.Add(this.name);
                    break;
                }
            } while (0 == 0);
            //use sentience to name self
            //not rng
            //trust me
        }

        public void displayInfo() {
            Console.WriteLine("Faction: " + name + ", Star ID: " + controlledStars[0].getID().ToString() + "\nControlled Stars: " + controlledStars.Count.ToString()
                + "\nControlled Planets: " + this.controlledPlanets.Count.ToString() + "\nContested Planets: " +
                this.contestedPlanets.Count.ToString() + "\nControlled Ships: " + controlledShips.Count.ToString());
            foreach (SpaceObject planet in this.controlledPlanets) Console.WriteLine("Planet: " + planet.getID());
            Console.WriteLine("----------------------------------------");
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
                this.controlledShips.Add(new AI(x + (i * 75), y));
                MainClient.objects.Add(this.controlledShips[i]);
            }
        }

        private void assignShipRoles() {
            int shipNumber = this.controlledShips.Count;

            Random r = new Random();
            int rand;
            foreach (AI ship in this.controlledShips) {
                ship.setOwner(this.name);
                ship.setHome(controlledStars[0].getPos());
                rand = r.Next(0, 10);
                if (!bandits) {
                    if (rand <= militaristic) {
                        ship.setRole(AI.Roles.MILITARY);       //say you roll a 7 for militaristic in the generation - you have a 70% chance of each ship being military
                        military.Add(ship);
                    } else {
                        ship.setRole(AI.Roles.ECONOMY);
                        workers.Add(ship);
                        if (controlledPlanets.Any()) {
                            ship.addMiningTarget(controlledPlanets[r.Next(0, controlledPlanets.Count)]);
                            ship.setState(AI.State.MINING);
                        }
                    }
                }
            }
        }

        private void findHomeStar() {
            Random r = new Random();
            int rInt;

            while (0 == 0) {
                rInt = r.Next(0, MainClient.world.getSpaceObjects().Count);

                if (MainClient.world.getSpaceObjects()[rInt].getType() == ObjectType.STAR &&
                        MainClient.world.getSpaceObjects()[rInt].getOwner().Equals("Independent")) {
                    controlledStars.Add(MainClient.world.getSpaceObjects()[rInt]);
                    MainClient.world.getSpaceObjects()[rInt].setOwner(this.name);
                    break;
                }
            }
        }

        private void findLocalPlanets() {
            /*foreach (SpaceObject planet in MainClient.world.getSpaceObjects()) {
                if(planet.getType() == ObjectType.MINING_PLANET) {
                    if (distanceTo(planet.getPos()) < planet.getInfluenceRadius()) controlledPlanets.Add(planet);
                }
            }*/
            foreach(SpaceObject planet in MainClient.world.getSpaceObjects()) {
                if(planet.getType() == ObjectType.MINING_PLANET) {
                    if (distanceTo(controlledStars[0].getPos(), planet.getPos()) < planet.getInfluenceRadius()) {
                        this.controlledPlanets.Add(planet);
                        Console.WriteLine("Planet at: " + planet.getPos().X + ", " + planet.getPos().Y + " -- distance: " + distanceTo(controlledStars[0].getPos(), planet.getPos()));
                    }
                }
            }
        }

        public void findContestedPlanets() {
            foreach(SpaceObject planet in this.controlledPlanets.ToList()) {
                if(planet.getInfluencers() > 1) {
                    Console.WriteLine("Found Contested Planet");
                    this.controlledPlanets.Remove(planet);
                    this.contestedPlanets.Add(planet);
                }
            }
        }

        public List<SpaceObject> getControlledPlanets() {
            return this.controlledPlanets;
        }

        public float distanceTo(Vector2 vc) {
            Vector2 change;
            change.X = vc.X - controlledStars[0].getPos().X;
            change.Y = vc.Y - controlledStars[0].getPos().Y;
            return (float)Math.Sqrt(change.X * change.X + change.Y * change.Y);
        }

        public float distanceTo(Vector2 v1, Vector2 v2) {
            Vector2 change;
            change.X = v1.X - v2.X;
            change.Y = v1.Y - v2.Y;
            return (float)Math.Sqrt(change.X * change.X + change.Y * change.Y);
        }

        private bool isBanditFaction() {
            Random r = new Random();
            int rInt = r.Next(0, 9);

            if (rInt == 7) return true;

            return false;
        }
    }
}
