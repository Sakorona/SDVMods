// Thanks to Pathoschild's Lookup Anything, which provided the pattern code for this menu!

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Pathoschild.Stardew.UIF;
using System.Linq;
using StardewModdingAPI.Utilities;
using TwilightShards.Stardew.Common;
using TwilightShards.Common;

namespace ClimatesOfFerngillRebuild
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

        /// <summary> Configuration Options </summary>
        private WeatherConfig OurConfig;
        
        /// <summary>The aspect ratio of the page background.</summary>
        private readonly Vector2 AspectRatio = new Vector2(Sprites.Letter.Sprite.Width, Sprites.Letter.Sprite.Height);

        /// <summary>The spacing around the scroll buttons.</summary>
        private readonly int ScrollButtonGutter = 15;

        /// <summary> To da Moon, Princess!  </summary>
        private SDVMoon OurMoon;

        /// <summary>The amount to scroll long content on each up/down scroll.</summary>
        private readonly int ScrollAmount;

        /// <summary>The clickable 'scroll up' icon.</summary>
        private readonly ClickableTextureComponent ScrollUpButton;

        /// <summary>The clickable 'scroll down' icon.</summary>
        private readonly ClickableTextureComponent ScrollDownButton;

        /// <summary>The maximum pixels to scroll.</summary>
        private int MaxScroll;

        /// <summary>The number of pixels to scroll.</summary>
        private int CurrentScroll;

        /// <summary> The dice object.  </summary>
        private MersenneTwister Dice;

        /// <summary> The current weather status </summary>
        private WeatherConditions CurrentWeather;

        /// <summary> Our Icons </summary>
        private Sprites.Icons IconSheet;

        ///<summary>This contains the text for various things</summary>
        private ITranslationHelper Helper;

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
        public WeatherMenu(IMonitor monitor, IReflectionHelper reflectionHelper, Sprites.Icons Icon, ITranslationHelper Helper, WeatherConditions weat, SDVMoon Termina, 
               WeatherConfig ModCon, int scroll, MersenneTwister Dice)
        {
            // save data
            this.Monitor = monitor;
            this.Reflection = reflectionHelper;
            this.Helper = Helper;
            this.CurrentWeather = weat;
            this.ScrollAmount = scroll;
            this.IconSheet = Icon;
            this.OurMoon = Termina;
            this.OurConfig = ModCon;
            this.Dice = Dice;

            // add scroll buttons
            this.ScrollUpButton = new ClickableTextureComponent(Rectangle.Empty, Sprites.Icons.source2, Sprites.Icons.UpArrow, 1);
            this.ScrollDownButton = new ClickableTextureComponent(Rectangle.Empty, Sprites.Icons.source2, Sprites.Icons.DownArrow, 1);

            // update layout
            this.UpdateLayout();
        }

        /****
        ** Events
        ****/

        /// <summary>The method invoked when the player scrolls the mouse wheel on the lookup UI.</summary>
        /// <param name="direction">The scroll direction.</param>
        public override void receiveScrollWheelAction(int direction)
        {
            if (direction > 0)    // positive number scrolls content up
                this.ScrollUp();
            else
                this.ScrollDown();
        }

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

        /// <summary>Scroll up the menu content by the specified amount (if possible).</summary>
        public void ScrollUp()
        {
            this.CurrentScroll -= this.ScrollAmount;
        }

        /// <summary>Scroll down the menu content by the specified amount (if possible).</summary>
        public void ScrollDown()
        {
            this.CurrentScroll += this.ScrollAmount;
        }

        /// <summary>Handle a left-click from the player's mouse or controller.</summary>
        /// <param name="x">The x-position of the cursor.</param>
        /// <param name="y">The y-position of the cursor.</param>
        public void HandleLeftClick(int x, int y)
        {
            // close menu when clicked outside
            if (!this.isWithinBounds(x, y))
                this.exitThisMenu();
        }

        public string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        /// <summary>Render the UI.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public override void draw(SpriteBatch spriteBatch)
        {
            // disable when game is using immediate sprite sorting
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
            //int tableBorderWidth = 1;

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

                // draw weather icon
                contentBatch.Draw(IconSheet.source, new Vector2(x + leftOffset, y + topOffset), 
                    IconSheet.GetWeatherSprite(CurrentWeather.TodayWeather), Color.White);
                leftOffset += 72;
                string weatherString = "";

                // draw fields
                float wrapWidth = this.width - leftOffset - gutter;
                {
                    // draw high and low
                    {
                        Vector2 descSize = contentBatch.DrawTextBlock(font, Helper.Get("weather-menu.opening", new { season = Game1.currentSeason, day = Game1.dayOfMonth }),
                            new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                        topOffset += descSize.Y;
                        if (CurrentWeather.IsFogVisible())
                        {
                            //fog display
                            string fogString = "";

                            switch (Game1.currentSeason)
                            {
                                case "spring":
                                    if (CurrentWeather.IsDarkFog())
                                        fogString = Helper.Get("weather-desc.spring_dfog", new { time = CurrentWeather.GetFogEndTime().ToString() });
                                    else
                                        fogString = Helper.Get("weather-desc.spring_fog", new { time = CurrentWeather.GetFogEndTime().ToString() });
                                    break;
                                case "summer":
                                    if (CurrentWeather.IsDarkFog())
                                        fogString = Helper.Get("weather-desc.summer_dfog", new { time = CurrentWeather.GetFogEndTime().ToString() });
                                    else
                                        fogString = Helper.Get("weather-desc.summer_fog", new { time = CurrentWeather.GetFogEndTime().ToString() });
                                    break;
                                case "fall":
                                    if (CurrentWeather.IsDarkFog())
                                        fogString = Helper.Get("weather-desc.fall_dfog", new { time = CurrentWeather.GetFogEndTime().ToString() });
                                    else
                                        fogString = Helper.Get("weather-desc.fall_fog", new { time = CurrentWeather.GetFogEndTime().ToString() });
                                    break;
                                case "winter":
                                    if (CurrentWeather.IsDarkFog())
                                        fogString = Helper.Get("weather-desc.winter_dfog", new { time = CurrentWeather.GetFogEndTime().ToString() });
                                    else
                                        fogString = Helper.Get("weather-desc.winter_fog", new { time = CurrentWeather.GetFogEndTime().ToString() });
                                    break;
                                default:
                                    fogString = "ERROR";
                                    break;
                            }

                            Vector2 fogSize = contentBatch.DrawTextBlock(font, fogString, new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                            topOffset += fogSize.Y;
                        }

                        topOffset += lineHeight;
                        //build the temperature display
                        string Temperature = CurrentWeather.GetTemperatureString(OurConfig.ShowBothScales, Helper);

                        //Output today's weather

                        if (CurrentWeather.TodayWeather != Game1.weather_festival)
                        {
                            weatherString = Helper.Get("weather-menu.fore_today_notFest", 
                                new { currentConditions = CurrentWeather.GetDescText(CurrentWeather.TodayWeather, SDate.Now(), Dice, Helper), 
                                Temperature = Temperature });
                        }
                        else
                        {
                            weatherString = Helper.Get("weather-menu.fore_today_festival", 
                            new { festival = SDVUtilities.GetFestivalName(), Temperature = Temperature });
                        }

                        Vector2 nameSize = contentBatch.DrawTextBlock(font, weatherString, new Vector2(x + leftOffset, y + topOffset), wrapWidth);
                        topOffset += nameSize.Y;
                        topOffset += lineHeight;


                        //Output Tomorrow's weather
                        if (!(Utility.isFestivalDay(Game1.dayOfMonth +1, Game1.currentSeason)))
                        {
                            weatherString = Helper.Get("weather-menu.fore_tomorrow_notFest", new
                            {
                                currentConditions = CurrentWeather.GetDescText(CurrentWeather.TomorrowWeather, SDate.Now().AddDays(1), Dice, Helper),
                                Temperature = Temperature
                            });
                        }
                        else
                        {
                            weatherString = Helper.Get("weather-menu.fore_tomorrow_festival",
                                new { festival = SDVUtilities.GetTomorrowFestivalName(), Temperature = Temperature });
                        }

                        Vector2 tomSize = contentBatch.DrawTextBlock(font, weatherString, new Vector2(x + leftOffset, y + topOffset), wrapWidth);

                        topOffset += tomSize.Y;
                    }

                    // draw spacer
                    topOffset += lineHeight;
                    
                    if (CurrentWeather.IsDangerousWeather())
                    {
                        weatherString = Helper.Get("weather-menu.dangerous_condition", 
                            new { condition = CurrentWeather.GetHazardousText(Helper,SDate.Now(), Dice) });
                        Vector2 statusSize = contentBatch.DrawTextBlock(font, weatherString, new Vector2(x + leftOffset, y + topOffset), wrapWidth, 
                            bold: true);
                        topOffset += statusSize.Y;
                        topOffset += lineHeight;
                    }

                }

                //draw moon info
                contentBatch.Draw(IconSheet.source, new Vector2(x + 15, y + topOffset), 
                    IconSheet.GetMoonSprite(OurMoon.CurrPhase), Color.White);

                weatherString = Helper.Get("moon-desc.desc_moonphase", 
                    new { moonPhase = SDVMoon.DescribeMoonPhase(OurMoon.CurrPhase, Helper)});

                Vector2 moonText = contentBatch.DrawTextBlock(font, 
                    weatherString, new Vector2(x + leftOffset, y + topOffset), wrapWidth);

                topOffset += lineHeight; //stop moon from being cut off.

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
            this.drawMouse(Game1.spriteBatch);
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
