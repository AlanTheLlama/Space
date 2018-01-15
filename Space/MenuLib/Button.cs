using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MenuLib {
    public class Button {
        public Texture2D tex { get; set; }
        string text;
        int X, Y, width, height;
        public Rectangle bounds;

        public Button(Texture2D tex, string text, int X, int Y, int width, int height) {
            this.X = X;
            this.Y = Y;
            this.width = width;
            this.height = height;
            this.tex = tex;
            this.bounds = new Rectangle(X, Y, width, height);
        }

        public bool Clicked() {
            var mState = Mouse.GetState();
            var mPos = new Point(mState.X, mState.Y);

            if (new Rectangle(X, Y, width, height).Contains(mPos) && mState.LeftButton == ButtonState.Pressed) return true;
            return false;
        }
    }
}
