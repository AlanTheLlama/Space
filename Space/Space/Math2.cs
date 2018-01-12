using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space {
    class Math2 {
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
    }
}
