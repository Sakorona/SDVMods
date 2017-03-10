// Thanks to Pathoschild's Lookup Anything, which provided the pattern code for this menu!

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

//3P
using Pathoschild.Stardew.UIF;

namespace ClimateOfFerngill
{
    /// <summary>A UI which shows information about an item.</summary>
    internal class WeatherMenu : IClickableMenu
    {
        /*********
        ** Properties
        *********/

        /// <summary>Encapsulates logging and monitoring.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The aspect ratio of the page background.</summary>
        private readonly Vector2 AspectRatio = new Vector2(Sprites.Letter.Sprite.Width, Sprites.Letter.Sprite.Height);

        /// <summary>The spacing around the scroll buttons.</summary>
        private readonly int ScrollButtonGutter = 15;

        /// <summary>Whether the game's draw mode has been validated for compatibility.</summary>
        private bool ValidatedDrawMode;

        /*********
        ** Public methods
        *********/
        /****
        ** Constructors
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates logging and monitoring.</param>
        public WeatherMenu(IMonitor monitor)
        {
            // save data
            this.Monitor = monitor;

            // update layout
            this.UpdateLayout();
        }

        /****
        ** Events
        ****/
        /// <summary>The method invoked when the player left-clicks on the lookup UI.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            this.HandleLeftClick(x, y);
        }

        /// <summary>The method invoked when the player right-clicks on the lookup UI.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        /// <summary>The method called when the game window changes size.</summary>
        /// <param name="oldBounds">The former viewport.</param>
        /// <param name="newBounds">The new viewport.</param>
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.UpdateLayout();
        }

        /// <summary>The method called when the player presses a controller button.</summary>
        /// <param name="button">The controller button pressed.</param>
        public override void receiveGamePadButton(Buttons button)
        {
            switch (button)
            {
                // left click
                case Buttons.A:
                    Point p = Game1.getMousePosition();
                    this.HandleLeftClick(p.X, p.Y);
                    break;

                // exit
                case Buttons.B:
                    this.exitThisMenu();
                    break;
            }
        }

        /****
        ** Methods
        ****/

            /// <summary>Handle a left-click from the player's mouse or controller.</summary>
        /// <param name="x">The x-position of the cursor.</param>
        /// <param name="y">The y-position of the cursor.</param>
        public void HandleLeftClick(int x, int y)
        {
            // close menu when clicked outside
            if (!this.isWithinBounds(x, y))
                this.exitThisMenu();
        }

        /// <summary>Render the UI.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public override void draw(SpriteBatch spriteBatch)
        {
            // disable when game is using immediate sprite sorting
            // (This prevents Lookup Anything from creating new sprite batches, which breaks its core rendering logic.
            // Fortunately this very rarely happens; the only known case is the Stardew Valley Fair, when the only thing
            // you can look up anyway is the farmer.)
            if (!this.ValidatedDrawMode)
            {
                IPrivateField<SpriteSortMode> sortModeField =
                    this.Reflection.GetPrivateField<SpriteSortMode>(Game1.spriteBatch, "spriteSortMode", required: false) // XNA
                    ?? this.Reflection.GetPrivateField<SpriteSortMode>(Game1.spriteBatch, "_sortMode"); // MonoGame
                if (sortModeField.GetValue() == SpriteSortMode.Immediate)
                {
                    this.Monitor.Log("Aborted the weather draw because the game's current rendering mode isn't compatible with the mod's UI. This only happens in rare cases (e.g. the Stardew Valley Fair).", LogLevel.Warn);
                    this.exitThisMenu(playSound: false);
                    return;
                }
                this.ValidatedDrawMode = true;
            }

            // calculate dimensions
            int x = this.xPositionOnScreen;
            int y = this.yPositionOnScreen;
            const int gutter = 15;
            float leftOffset = gutter;
            float topOffset = gutter;
            float contentWidth = this.width - gutter * 2;
            float contentHeight = this.height - gutter * 2;
            int tableBorderWidth = 1;

            // get font
            SpriteFont font = Game1.smallFont;
            float lineHeight = font.MeasureString("ABC").Y;

            //at this point I'm going to manually put this in as I don't need in many places,
            // and I kinda want to have this where I can understand what it's for 
            float spaceWidth = DrawHelper.GetSpaceWidth(font);

            // draw background
            // (This uses a separate sprite batch because it needs to be drawn before the
            // foreground batch, and we can't use the foreground batch because the background is
            // outside the clipping area.)
            using (SpriteBatch backgroundBatch = new SpriteBatch(Game1.graphics.GraphicsDevice))
            {
                backgroundBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
                backgroundBatch.DrawSprite(Sprites.Letter.Sheet, Sprites.Letter.Sprite, x, y, scale: width / (float)Sprites.Letter.Sprite.Width);
                backgroundBatch.End();
            }

            /*
            this.Monitor.InterceptErrors("drawing the lookup info", () =>
            {
 
                   // draw background
                // (This uses a separate sprite batch because it needs to be drawn before the
                // foreground batch, and we can't use the foreground batch because the background is
                // outside the clipping area.)
                using (SpriteBatch backgroundBatch = new SpriteBatch(Game1.graphics.GraphicsDevice))
                {
                    backgroundBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
                    backgroundBatch.DrawSprite(Sprites.Letter.Sheet, Sprites.Letter.Sprite, x, y, scale: this.width / (float)Sprites.Letter.Sprite.Width);
                    backgroundBatch.End();
                }

                // draw foreground
                // (This uses a separate sprite batch to set a clipping area for scrolling.)
                using (SpriteBatch contentBatch = new SpriteBatch(Game1.graphics.GraphicsDevice))
                {
                    // begin draw
                    GraphicsDevice device = Game1.graphics.GraphicsDevice;
                    device.ScissorRectangle = new Rectangle(x + gutter, y + gutter, (int)contentWidth, (int)contentHeight);
                    contentBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, new RasterizerState { ScissorTestEnable = true });

                    // scroll view
                    this.CurrentScroll = Math.Max(0, this.CurrentScroll); // don't scroll past top
                    this.CurrentScroll = Math.Min(this.MaxScroll, this.CurrentScroll); // don't scroll past bottom
                    topOffset -= this.CurrentScroll; // scrolled down == move text up

                    // draw portrait
                    if (subject.DrawPortrait(contentBatch, new Vector2(x + leftOffset, y + topOffset), new Vector2(70, 70)))
                        leftOffset += 72;

                    // draw fields
                    float wrapWidth = this.width - leftOffset - gutter;
                    {
                        // draw name & item type
                        {
                            Vector2 nameSize = contentBatch.DrawTextBlock(font, $"{subject.Name}.", new Vector2(x + leftOffset, y + topOffset), wrapWidth, bold: true);
                            Vector2 typeSize = contentBatch.DrawTextBlock(font, $"{subject.Type}.", new Vector2(x + leftOffset + nameSize.X + spaceWidth, y + topOffset), wrapWidth);
                            topOffset += Math.Max(nameSize.Y, typeSize.Y);
                        }

                        // draw description
                        if (subject.Description != null)
                        {
                            Vector2 size = contentBatch.DrawTextBlock(font, subject.Description?.Replace(Environment.NewLine, " "), new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                            topOffset += size.Y;
                        }

                        // draw spacer
                        topOffset += lineHeight;

                        // draw custom fields
                        if (this.Fields.Any())
                        {
                            ICustomField[] fields = this.Fields;
                            float cellPadding = 3;
                            float labelWidth = fields.Where(p => p.HasValue).Max(p => font.MeasureString(p.Label).X);
                            float valueWidth = wrapWidth - labelWidth - cellPadding * 4 - tableBorderWidth;
                            foreach (ICustomField field in fields)
                            {
                                if (!field.HasValue)
                                    continue;

                                // draw label & value
                                Vector2 labelSize = contentBatch.DrawTextBlock(font, field.Label, new Vector2(x + leftOffset + cellPadding, y + topOffset + cellPadding), wrapWidth);
                                Vector2 valuePosition = new Vector2(x + leftOffset + labelWidth + cellPadding * 3, y + topOffset + cellPadding);
                                Vector2 valueSize =
                                    field.DrawValue(contentBatch, font, valuePosition, valueWidth)
                                    ?? contentBatch.DrawTextBlock(font, field.Value, valuePosition, valueWidth);
                                Vector2 rowSize = new Vector2(labelWidth + valueWidth + cellPadding * 4, Math.Max(labelSize.Y, valueSize.Y));

                                // draw table row
                                Color lineColor = Color.Gray;
                                contentBatch.DrawLine(x + leftOffset, y + topOffset, new Vector2(rowSize.X, tableBorderWidth), lineColor); // top
                                contentBatch.DrawLine(x + leftOffset, y + topOffset + rowSize.Y, new Vector2(rowSize.X, tableBorderWidth), lineColor); // bottom
                                contentBatch.DrawLine(x + leftOffset, y + topOffset, new Vector2(tableBorderWidth, rowSize.Y), lineColor); // left
                                contentBatch.DrawLine(x + leftOffset + labelWidth + cellPadding * 2, y + topOffset, new Vector2(tableBorderWidth, rowSize.Y), lineColor); // middle
                                contentBatch.DrawLine(x + leftOffset + rowSize.X, y + topOffset, new Vector2(tableBorderWidth, rowSize.Y), lineColor); // right

                                // update offset
                                topOffset += Math.Max(labelSize.Y, valueSize.Y);
                            }
                        }
                    }

                    // update max scroll
                    this.MaxScroll = Math.Max(0, (int)(topOffset - contentHeight + this.CurrentScroll));

                    // draw scroll icons
                    if (this.MaxScroll > 0 && this.CurrentScroll > 0)
                        this.ScrollUpButton.draw(contentBatch);
                    if (this.MaxScroll > 0 && this.CurrentScroll < this.MaxScroll)
                        this.ScrollDownButton.draw(spriteBatch);

                    // end draw
                    contentBatch.End();
                }

                // draw cursor
                base.drawMouse(Game1.spriteBatch);
            }, this.OnDrawError);
            */
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Update the layout dimensions based on the current game scale.</summary>
        private void UpdateLayout()
        {
            // update size
            this.width = Math.Min(Game1.tileSize * 14, Game1.viewport.Width);
            this.height = Math.Min((int)(this.AspectRatio.Y / this.AspectRatio.X * this.width), Game1.viewport.Height);

            // update position
            Vector2 origin = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
            this.xPositionOnScreen = (int)origin.X;
            this.yPositionOnScreen = (int)origin.Y;

            // update up/down buttons
            int x = this.xPositionOnScreen;
            int y = this.yPositionOnScreen;
            int gutter = this.ScrollButtonGutter;
            float contentHeight = this.height - gutter * 2;
        }

        /// <summary>The method invoked when an unhandled exception is intercepted.</summary>
        /// <param name="ex">The intercepted exception.</param>
        private void OnDrawError(Exception ex)
        {
            Monitor.Log($"handling an error in the draw code {ex}", LogLevel.Error);
        }
    }
}
