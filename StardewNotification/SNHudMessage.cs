using System;
using global::StardewValley.Menus;
using global::StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewNotification
{
    public class SNHudMessage: HUDMessage
    {

        /// <summary>Construct an instance with the default time and an empty icon.</summary>
        /// <param name="message">The message text to show.</param>
        public SNHudMessage(string message) : base(message, 3500f)
        {
        }

        /// <summary>Construct an instance with a specified icon type, and a duration 1.5× default.</summary>
        /// <param name="message">The message text to show.</param>
        /// <param name="whatType">The icon to show, matching a constant like <see cref="F:StardewValley.HUDMessage.error_type" />.</param>
        public SNHudMessage(string message, int whatType) : base(message,whatType)
        {
        }

        /// <summary>Construct an instance with the given values.</summary>
        /// <param name="message">The message text to show.</param>
        /// <param name="timeLeft">The duration in milliseconds for which to show the message.</param>
        /// <param name="fadeIn">Whether the message should start transparent and fade in.</param>
        public SNHudMessage(string message, float timeLeft, bool fadeIn = false) : base(message, timeLeft, fadeIn)
        {
            
        }

        public override void draw(SpriteBatch b, int i, ref int heightUsed)
        {
            Rectangle tsarea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
            if (noIcon)
            {
                int overrideX = tsarea.Left + 16;
                int height2 = (int)Game1.smallFont.MeasureString(message).Y + 64;
                int overrideY = ((Game1.uiViewport.Width < 1400) ? (-64) : 0) + tsarea.Bottom - height2 - heightUsed - 64;
                heightUsed += height2;
                IClickableMenu.drawHoverText(b: b, text: message, font: Game1.smallFont, overrideX: overrideX, overrideY: overrideY, alpha: transparency);
                return;
            }
            else
            {
                base.draw(b, i, ref heightUsed);    
            } 

        }
    }
}