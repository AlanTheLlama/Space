using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace Space {
    public class AI {

        public float MAX_SPEED = 4;

        public Vector2 pos; //Don't know if I need any of these?
        public float aimRotation;
        public float movRotation;
        public float speed;
        public float rotSpeed;
        public float curveSpeed;
        public float acceleration;
        public bool initialized;




        public void genNew() { //Generates a new AI spawn
            

        }      
        public bool danger() {
            //If the player is within a 50x50 area return true
            //Game1.playerList;
            

            return true;
        }
    }
}
