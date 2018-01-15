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
        public Texture2D tex;
        public Rectangle sourceRect;

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
            this.sourceRect = new Rectangle(0, 0, 50, 20);
        }

        public void SetClicking() {
            sourceRect.X = (tex.Width / 3) * 2;
        }

        public void SetHover() {
            sourceRect.X = (tex.Width / 3) * 1;
        }

        public void SetStandard() {
            sourceRect.X = (tex.Width / 3) * 0;
        }

        public bool Hovering() {
            var mState = Mouse.GetState();
            var mPos = new Point(mState.X, mState.Y);

            if (bounds.Contains(mPos) && mState.LeftButton == ButtonState.Released) return true;
            return false;
        }

        public bool Clicked() {
            var mState = Mouse.GetState();
            var mPos = new Point(mState.X, mState.Y);

            sourceRect.X = (tex.Width / 3) * 3;

            if (bounds.Contains(mPos) && mState.LeftButton == ButtonState.Pressed) return true;
            return false;
        }
        
        public static void CheckStates(List<Button> buttonList) {
            foreach (Button b in buttonList) {
                if (b.Hovering()) {
                    b.SetHover();
                } else if (b.Clicked()) {
                    b.SetClicking();
                } else b.SetStandard();
            }
        }
    }
}
