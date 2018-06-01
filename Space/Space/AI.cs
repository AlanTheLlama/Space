using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space {
    public class AI : MovingObject {

        private float MAX_SPEED = 4;

        //MOVEMENT 
        private MovingObject dangerObject;
        public List<Vector2> patrolTargets;
        public List<Object> attackTargets;
        public List<AI> nearbyShips;
        public List<AI> nearbyCombatants;
        public List<string> publicEnemies;   //kek
        public SpaceObject miningTarget;
        private Vector2 pos;
        private Vector2 velocity;
        public Vector2 home;
        public SpaceObject destination;
        public SpaceObject homeStar;
        private float aimRotation;
        private float rotSpeed;
        private float baseRotSpeed;
        private float forwardForce;
        private float backwardForce;
        private float sideForce;
        private float mass;
        private float targetSpeed;
        private bool finishedRotating;
        private bool arrived;
        private bool minedStuff;
        private int patrollingTo;
        private bool returning;
        public enum Roles { MILITARY, ECONOMY }
        public Roles role;
        string owner;

        //NON MOVEMENT
        private Vector2 change;
        private float dist;

        //SERVER
        private int identifier;
        private ObjectType type;
        Object playerRef;

        //GAMEPLAY
        public enum State { IDLE, TRAVELLING, PATROLLING, COMBAT, DEFENDING, FLEEING, MINING } //not sure how this is going to work lol I only need like two of these rn
        public State currentState;
        private bool boosting;
        private bool cooling;
        private float radius;
        private float shield;
        private bool alive;
        private bool runningAway;
        private float weaponCooldown;
        private float weapons;

        public AI (float x, float y) { 
            this.pos = new Vector2(x, y);
            this.aimRotation = (float)0.5 * (float)Math.PI;           //don't press enter.
            this.velocity = new Vector2(0, 0);
            this.rotSpeed = (float)0.08;
            this.baseRotSpeed = rotSpeed;
            this.forwardForce = (float)2; 
            this.backwardForce = (float)1; 
            this.sideForce = (float)0.5;
            this.mass = 30;
            this.runningAway = false;
            this.arrived = false;
            this.returning = false;
            this.patrollingTo = 0;
            this.patrolTargets = new List<Vector2>();
            this.publicEnemies = new List<string>();
            this.nearbyCombatants = new List<AI>();
            this.nearbyShips = new List<AI>();
            //this.miningTarget = new List<Object>();

            this.type = ObjectType.AI;

            this.radius = 32;
            this.shield = 200;
            this.alive = true;
            this.cooling = false;
            this.boosting = false;
            this.weaponCooldown = 0;
            this.weapons = 24;
            this.currentState = State.IDLE;
            this.attackTargets = new List<Object>();
            this.setIdentifier();
            Console.WriteLine("New AI created with ID " + this.identifier);
        }

        //GETTERS/SETTERS

        public void setRole(Roles r) {
            this.role = r;
        }

        public Roles getRole() {
            return role;
        }

        public void setIdentifier() {
            this.identifier = MainClient.r.Next(0, 100000);
        }
        
        public void setHome(Vector2 h, SpaceObject hS) {
            this.home = h;
            this.homeStar = hS;
            this.destination = homeStar;
        }
        public void setState(State s) {
            this.currentState = s;
        }

        public State getState() {
            return currentState;
        }

        public bool isBoosting()
        {
            return this.boosting;
        }

        public bool isCooling()
        {
            return this.cooling;
        }
        public float getRot()
        {
            return this.aimRotation;
        }

        public int getID() {
            return this.identifier;
        }

        public Vector2 getPos() {
            return this.pos;
        }

        public ObjectType getType() {
            return this.type;
        }

        public float getAngle() {
            return this.aimRotation;
        }

        public Texture2D getTexture() {
            return MainClient.enemy;
        }

        public void setCoords(float x, float y, float rot) {
            this.pos = new Vector2(x, y);
            this.aimRotation = rot;
        }

        public bool isAlive() {
            return this.alive;
        }

        //MOVEMENT

        public void rotateRight() {
            this.aimRotation = this.aimRotation + this.rotSpeed;
            if (this.aimRotation > 2 * ((float)Math.PI)) {
                this.aimRotation = this.aimRotation - (2 * ((float)Math.PI));
            }

        }

        public void rotateLeft() {
            this.aimRotation = this.aimRotation - this.rotSpeed;
            if (this.aimRotation < 0) {
                this.aimRotation = this.aimRotation + (2 * ((float)Math.PI));
            }
        }
        public void thrust() {
            if (this.getSpeed() == 0) {
                this.velocity.X = (float)Math.Cos(this.aimRotation) * this.forwardForce / this.mass;
                this.velocity.Y = (float)Math.Sin(this.aimRotation) * this.forwardForce / this.mass;
            } else {
                this.velocity.X = this.velocity.X + (float)Math.Cos(this.aimRotation) * this.forwardForce / this.mass;
                this.velocity.Y = this.velocity.Y + (float)Math.Sin(this.aimRotation) * this.forwardForce / this.mass;
                if (this.getSpeed() > MAX_SPEED) {
                    this.scaleSpeed(MAX_SPEED);
                }
            }
        }

        public void leftThrust() {
            float left = this.aimRotation - (float)0.5 * (float)Math.PI;
            if (left > 2 * Math.PI) {
                left -= 2 * (float)Math.PI;
            }
            this.velocity.X = this.velocity.X + (float)Math.Cos(left) * this.sideForce / this.mass;
            this.velocity.Y = this.velocity.Y + (float)Math.Sin(left) * this.sideForce / this.mass;
            if (this.getSpeed() > MAX_SPEED) {
                this.scaleSpeed(MAX_SPEED);
            }
        }

        public void rightThrust() {
            float right = this.aimRotation + (float)0.5 * (float)Math.PI;
            if (right < 0) {
                right += 2 * (float)Math.PI;
            }
            this.velocity.X = this.velocity.X + (float)Math.Cos(right) * this.sideForce / this.mass;
            this.velocity.Y = this.velocity.Y + (float)Math.Sin(right) * this.sideForce / this.mass;
            if (this.getSpeed() > MAX_SPEED) {
                this.scaleSpeed(MAX_SPEED);
            }
        }

        public void scaleSpeed(float maxSpeed) {
            float scale = maxSpeed / this.getSpeed();
            this.velocity.X = this.velocity.X * scale;
            this.velocity.Y = this.velocity.Y * scale;
        }

        public void reverse() {
            this.velocity.X = this.velocity.X - (float)Math.Cos(this.aimRotation) * this.sideForce / this.mass;
            this.velocity.Y = this.velocity.Y - (float)Math.Sin(this.aimRotation) * this.sideForce / this.mass;
            if (this.getSpeed() < 0.1) {
                this.velocity.X = 0;
                this.velocity.Y = 0;
            }
        }

        public void brake() {
            if (this.getSpeed() != 0) {
                if (this.getSpeed() < 0.101) {
                    this.velocity = new Vector2(0, 0);
                } else {
                    this.velocity.X = this.velocity.X - this.velocity.X * this.backwardForce / (this.mass * this.getSpeed());
                    this.velocity.Y = this.velocity.Y - this.velocity.Y * this.backwardForce / (this.mass * this.getSpeed());
                }
            }
        }

        public void boost()
        {
            if (!boosting)
            {
                MAX_SPEED = 4 * MAX_SPEED;
                boosting = true;
            }
        }

        public void noBoost()
        {
            if (boosting)
            {
                MAX_SPEED = MAX_SPEED / 4;
                boosting = false;
                this.cooling = true;
            }
            if (this.getSpeed() > MAX_SPEED)
            {
                this.brake();
                this.brake();
            }
            else if (this.cooling)
            {
                this.cooling = false;
            }
        }

        //MISC

        public void updatePosition(World w)
        {
            pollSpeed();

            Vector2 vec = new Vector2(this.pos.X + this.velocity.X, this.pos.Y + this.velocity.Y);
            float x = this.pos.X;
            float y = this.pos.Y;
            if (vec.X >= 0 || vec.X <= w.getSizeX())
            {
                x = vec.X;
            }
            if (vec.Y >= 0 || vec.Y <= w.getSizeX())
            {
                y = vec.Y;
            }
            this.pos = new Vector2(x, y);
            //System.Diagnostics.Debug.Print(danger().ToString());
        }

        public void update(World w)
        {
            //if(this.getSpeed() > 0)
                //Console.WriteLine(this.getSpeed() + "location: " + this.getPos().X + ", " + this.getPos().Y);   //cpu death
            //BEHAVIOUR
            switch (this.currentState) {
                case State.IDLE:
                    //chill with bob ross
                    break;

                case State.COMBAT:
                    nearbyShips = obtainSurroundings();
                    this.leftThrust();
                    if (!nearbyCombatants.Any() && attackTargets.Any()) this.currentState = State.TRAVELLING;
                    if (!nearbyCombatants.Any() && !attackTargets.Any()) this.currentState = State.PATROLLING;
                    if (nearbyCombatants.Any()) {
                        rotateToTarget(nearbyCombatants[0].getPos());
                        maintainDistance(nearbyCombatants[0].getPos());
                        fireWeapon(nearbyCombatants[0]);
                    }
                    break;

                case State.DEFENDING:
                    nearbyShips = obtainSurroundings();
                    if (nearbyCombatants.Any()) this.currentState = State.COMBAT;
                    break;

                case State.FLEEING:
                    //TODO
                    break;

                case State.TRAVELLING:
                    if (this.attackTargets.Any()) {
                        travelToTarget(this.attackTargets[0].getPos(), 4, 7);
                    }
                    nearbyShips = obtainSurroundings();
                    if (nearbyCombatants.Any()) this.currentState = State.COMBAT;
                    if (arrived) this.currentState = State.DEFENDING;
                    break;

                case State.PATROLLING:
                    patrol();
                    break;

                case State.MINING:
                    miningLoop();
                    break;

                default:
                    //CHILL
                    break;
            }
            if (weaponCooldown > 0)
            {
                weaponCooldown--;
            }
            this.updatePosition(w);
        }

        public float getSpeed()
        {
            return (float)Math.Sqrt(this.velocity.X * this.velocity.X + this.velocity.Y * this.velocity.Y);
        }

        public void distanceTo(MovingObject mo)
        {
            this.change.X = mo.getPos().X - this.pos.X;
            this.change.Y = mo.getPos().Y - this.pos.Y;
            dist = (float)Math.Sqrt(this.change.X * this.change.X + this.change.Y * this.change.Y);
        }
        public float distanceTo(Vector2 vc) {
            this.change.X = vc.X - this.pos.X;
            this.change.Y = vc.Y - this.pos.Y;
            return (float)Math.Sqrt(this.change.X * this.change.X + this.change.Y * this.change.Y);
        }

        public void fireWeapon(MovingObject mo)
        {
            if (weaponCooldown == 0)
            {
                distanceTo(mo);
                float x = this.change.X;
                float y = this.change.Y;
                Vector2 angle = Math2.getUnitVector(x, y);
                Projectile laser = new Projectile(this.pos, this.weapons, angle, this.getID());
                MainClient.objects.Add(laser);
                Console.WriteLine("Laser " + this.getID() + " firing");
                this.weaponCooldown = 20;
                return;
            }
            return;
        }


        //GAMEPLAY

        public int danger() { //Changed this around a bit
            foreach (Object o in MainClient.players) {
                if (o.getType() == ObjectType.PLAYER) {
                    distanceTo((MovingObject) o);
                    this.dangerObject = (MovingObject) o;
                    if (dist <= 200) {
                        playerRef = o;
                        runningAway = true;
                        return 1;
                    }
                    else if (dist <= 400)
                    {
                        playerRef = o;
                        runningAway = true;
                        return 2;
                    }
                    else if (dist <= 600)
                    {
                        playerRef = o;
                        runningAway = true;
                        return 3;
                    }else if (dist > 600) {
                        playerRef = o;
                        runningAway = false;
                        return 0;
                    }
                }
            }
            runningAway = false;
            return 0; //200,400,600
        }

        public void decide() { //Solely combat scenario
            int temp = danger();

            if (temp < 1000) {
                Vector2 pLoc = playerRef.getPos();
                Vector2 AiLoc = this.getPos();
                double slope = (AiLoc.Y - pLoc.Y) / (AiLoc.X - pLoc.X);
                float deg = (float)Math.Atan(slope);
            }

            if (temp == 1) //immediate danger
            {
                targetSpeed = 5;
            }
            else if (temp == 2) //close
            {
                targetSpeed = 3;
            }
            else if (temp == 3) //far
            {
                targetSpeed = 1;
            }
            else if(temp == 0) {
                targetSpeed = 0;
            }
        }

        public bool isHit(Object o) {
            return Math2.inRadius(this.getPos(), o.getPos(), this.radius);
        }

        public void getHit(float power) {
            this.shield -= power;
            if (this.shield <= 0) {
                this.alive = false;
            }
        }

        public void pollSpeed() {
            if (this.getSpeed() < targetSpeed) this.thrust();
            if (this.getSpeed() > targetSpeed) this.brake();
            if (this.getSpeed() > targetSpeed && targetSpeed < 0) this.reverse();
        }

        public void addAttackTarget(SpaceObject target) {
            attackTargets.Add(target);
        }
        public void addAttackTarget(MovingObject target) {
            attackTargets.Add(target);
        }
        public void addAttackTarget(Object target) {
            attackTargets.Add(target);
        }
        public void addAttackTarget(AI target) {
            attackTargets.Add(target);
        }

        public void addPatrolTarget(SpaceObject target) {
            patrolTargets.Add(target.getPos());
        }
        public void addPatrolTarget(MovingObject target) {
            patrolTargets.Add(target.getPos());
        }
        public void addPatrolTarget(List<SpaceObject> target) {
            foreach(Object o in target) {
                patrolTargets.Add(o.getPos());
            }
        }
        public void addPatrolTarget(AI target) {
            patrolTargets.Add(target.getPos());
        }
        public void addMiningTarget(SpaceObject target) {
            miningTarget = target;
        }

        public List<AI> obtainSurroundings() {
            List<AI> surroundingShips = new List<AI>();

            for (int i = 0; i < MainClient.world.factions.Count(); i++) {
                foreach (AI ship in MainClient.world.factions[i].controlledShips) {
                    if (ship.getOwner() != this.getOwner() && !surroundingShips.Contains(ship) && ship.isAlive()) {
                        if (distanceTo(ship.getPos()) < 1500) {
                            surroundingShips.Add(ship);
                            if (ship.attackTargets.Contains(this.attackTargets[0]) && ship.getOwner() != this.getOwner()) {
                                alertFaction(ship.getOwner());
                            }
                            if (publicEnemies.Any() && publicEnemies.Contains(ship.getOwner()) && !nearbyCombatants.Contains(ship) && ship.getRole() == Roles.MILITARY) {
                                nearbyCombatants.Add(ship);
                            }
                        }
                    }
                }
            }

            checkDead();

            Console.WriteLine("Ship " + this.getID() + " found " + nearbyCombatants.Count() + " nearby combatants");

            return surroundingShips;
        }

        public void rotateToTarget(Vector2 target) {
            float deltaX = 0f, deltaY = 0f, angleRad = 0f;
            deltaX = target.X - this.getPos().X;
            deltaY = target.Y - this.getPos().Y;
            angleRad = (float)Math.Atan2(deltaY, deltaX);

            float currentRotation = Math2.toDegrees(this.getRot());
            float angleBetween = Math2.toDegrees(angleRad);

            this.rotSpeed = 0.06f;
            //Console.WriteLine("bob:" + currentRotation + " - world dif: " + angleBetween);

            if (currentRotation < angleBetween - 5) {
                this.rotateRight();
                finishedRotating = false;
            }
            if (currentRotation > angleBetween + 5) {
                this.rotateLeft();
                finishedRotating = false;
            }
            if (currentRotation > angleBetween - 5 && currentRotation < angleBetween + 5) finishedRotating = true;
        }

        public void checkDead() {
            foreach(AI ship in this.nearbyShips.ToList()) {
                if(!ship.isAlive()) {
                    try {
                        this.nearbyShips.Remove(ship);
                        this.nearbyCombatants.Remove(ship);
                    }catch {
                        Console.WriteLine("Couldn't find ship to delete");
                    }
                }
            }
        }

        public void alertFaction(string badguys) {
            foreach (Faction f in MainClient.world.factions) {
                if (f.name == this.getOwner()) f.recieveBaddieAlert(badguys);
            }
        }

        public void miningLoop() {
            if(destination == miningTarget) {
                travelToTarget(homeStar, 1, 5);
            }
            if(destination == homeStar) {
                travelToTarget(miningTarget, 1, 5);
            }
        }

        public void returnHome() {
            if(distanceTo(home) <= 100) {
                returning = false;
                arrived = false;
                return;
            }else returning = true;

            travelToTarget(home, 1, 5);
        }

        public void travelToTarget(Vector2 target, int turnSpeed, int travelSpeed) {
            rotateToTarget(target);
            if(!finishedRotating)targetSpeed = turnSpeed;
            float distanceToTarget = distanceTo(target);
            arrived = false;

            if (finishedRotating && (distanceToTarget > 400)) {
                this.targetSpeed = travelSpeed;
            }
            if (distanceToTarget <= 400) {
                targetSpeed = (distanceToTarget / 400);
                if (targetSpeed < 1.5) targetSpeed = 1.5f;
            }
            if (distanceToTarget <= 100) {
                targetSpeed = 0;
                arrived = true;
            }
        }

        public void maintainDistance(Vector2 target, int distance) {
            float dist = distanceTo(target);
            int desiredGap = distance;
            if (dist > desiredGap) targetSpeed = 3;
            if (dist < desiredGap) targetSpeed = -2;
        }

        public void maintainDistance(Vector2 target) {
            float dist = distanceTo(target);
            int desiredGap = MainClient.r.Next(100, 600);
            if (dist > desiredGap) targetSpeed = 3;
            if (dist < desiredGap) targetSpeed = -2;
        }

        public void travelToTarget(SpaceObject target, int turnSpeed, int travelSpeed) {
            rotateToTarget(target.getPos());
            if (!finishedRotating) targetSpeed = turnSpeed;
            float distanceToTarget = distanceTo(target.getPos());
            arrived = false;

            if (finishedRotating && (distanceToTarget > 400)) {
                this.targetSpeed = travelSpeed;
            }
            if (distanceToTarget <= 400) {
                targetSpeed = (distanceToTarget / 400);
                if (targetSpeed < 1.5) targetSpeed = 1.5f;
            }
            if (distanceToTarget <= 100) {
                targetSpeed = 0;
                arrived = true;
                destination = target;
            }
        }

        public void runFromTarget(Vector2 target) {
            float deltaX = target.X - this.getPos().X;
            float deltaY = target.Y - this.getPos().Y;
            float angleRad = (float)Math.Atan2(deltaY, deltaX);

            float currentRotation = Math2.toDegrees(this.getRot());
            float angleBetween = Math2.toDegrees(angleRad);
            //System.Diagnostics.Debug.Print("ANGLE BETWEEN:     " + angleBetween);
            //System.Diagnostics.Debug.Print("AI ROT:     " + (currentRotation + 180));  //might need some tweaking as bob occasionally pulls a random 360

            float targetRotation = currentRotation + 180;
            if (targetRotation > 360) targetRotation -= 360;
            if (targetRotation < (angleBetween - 10)) rotateRight();
            if (targetRotation > (angleBetween + 10)) rotateLeft();
        }

        public void patrol() {
            if (this.patrolTargets.Any()) {                          //if there are any locations in the patrol targets vector, start patrolling.
                if (!this.arrived) {                                //if it hasn't arrived yet, go there.
                    travelToTarget(this.patrolTargets[this.patrollingTo], 2, 5);
                } else {
                    if (this.patrollingTo == this.patrolTargets.Count) {
                        this.patrollingTo = 0;
                    } else this.patrollingTo += 1;                   //if it's arrived, move to the next location - if at the end of the list, go to the start
                }
            }
        }

        public string getOwner() {
            return this.owner;
        }

        public void setOwner(string newOwner) {
            this.owner = newOwner;
        }

        public string getTask() {
            return currentState.ToString();
        }
    }
}
