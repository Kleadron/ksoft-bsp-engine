using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSoft.Game.Graphics
{
    public class DebugFontRenderer
    {
        const int MinChar = 32;
        const int MaxChar = 128;
        const int CharWidth = 8;
        const int CharHeight = 8;

        SpriteBatch sb;
        Texture2D texture;

        public DebugFontRenderer(SpriteBatch sb, Texture2D texture)
        {
            this.sb = sb;
            this.texture = texture;
        }

        // basic function to calculate the size of the text on the screen
        public void GetSize(string text, out int width, out int height)
        {
            int highestWidth = 0;
            int highestHeight = CharHeight;

            int curWidth = 0;

            for(int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\n')
                {
                    curWidth = 0;
                    highestHeight += CharHeight;
                }
                else
                {
                    curWidth += CharWidth;
                    if (highestWidth < curWidth)
                        highestWidth = curWidth;
                }
            }

            width = highestWidth;
            height = highestHeight;
        }

        public void Submit(string text, int x, int y, Color color)
        {
            int destX = x;
            int destY = y;

            for(int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // newlines
                if (c == '\n')
                {
                    destX = x;
                    destY += CharHeight;
                    continue;
                }

                // check for the specific char you want before defaulting it!
                if (c < MinChar || c > MaxChar)
                    c = '?';

                int srcX = (c - MinChar) * CharWidth;
                //int destX = x + (i * CharWidth);

                if (c != ' ')
                    sb.Draw(texture, new Rectangle(destX, destY, CharWidth, CharHeight), new Rectangle(srcX, 0, CharWidth, CharHeight), color);

                destX += CharWidth;
            }
        }

        public void SubmitShadowed(string text, int x, int y, Color color, Color shadowColor)
        {
            // shadow
            Submit(text, x + 1, y + 1, shadowColor);
            // foreground
            Submit(text, x, y, color);
        }
    }
}
