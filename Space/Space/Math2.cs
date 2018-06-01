using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {
    class Math2 {
        public static float QUARTER_CIRCLE = (float)(0.5 * Math.PI);
        public static float HALF_CIRCLE = (float)(Math.PI);
        public static float THREE_QUARTER_CIRCLE = (float)(1.5 * Math.PI);

        public static Vector2 getUnitVector(float x, float y) {
            float ret = getQuadSum(x, y);
            Vector2 uv;
            if (ret != 0) {
                uv = new Vector2(x / ret, y / ret);
            } else {
                uv = new Vector2(0, 0);
            }
            return uv;
        }

        public static float getQuadSum(float x, float y) {
            return (float)Math.Sqrt(x * x + y * y);
        }

        public static bool inRadius(Vector2 pos1, Vector2 pos2, float radius) {
            
            return ((float)Math2.getQuadSum(pos2.X - pos1.X, pos2.Y - pos1.Y)) < radius;
        }


        public static float toDegrees(float rad) {
            float deg = rad * (180f / (float)Math.PI);
            if (deg < 0f) deg += 360f;

            return deg;
        }
    }
}
