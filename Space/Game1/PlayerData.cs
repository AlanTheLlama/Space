using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceServer
{
    class PlayerData
    {
        float x, y;
        float rot;
        int id;

		public PlayerData(float x, float y, float rot, int id) {
            this.x = x;
            this.y = y;
            this.rot = rot;
			this.id = id;
        }
        public float getX() {
			return x;
        }
        public float getY() {
            return y;
        }
        public float getRot() {
            return rot;
        }
        public int getID() {
            return id;
        }
        public void setCoords(float x, float y, float rot) {
            this.x = x;
            this.y = y;
            this.rot = rot;
        }
		public string outputAsString() {
            return "X: " + this.x + ", Y: " + this.y + ", rot: " + this.rot + "ID: " + this.id;
        }

        public string dataString() {
            return x.ToString() + "/" + y.ToString() + "/" + getRot().ToString() + "/" + getID().ToString() + ";";
        }
    }
}
