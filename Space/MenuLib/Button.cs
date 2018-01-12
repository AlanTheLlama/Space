using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MenuLib {
    class Button {
        Texture2D tex;
        string text;
        int X, Y, width, height;

        public Button(Texture2D tex, string text, int X, int Y, int width, int height) {
            this.X = X;
            this.Y = Y;
            this.width = width;
            this.height = height;
            this.tex = tex;
        }
    }
}
