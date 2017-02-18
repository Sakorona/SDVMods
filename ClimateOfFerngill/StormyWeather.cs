using System;
using StardewModdingAPI;
using StardewValley.Objects;
using StardewModdingAPI.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using StardewValley;

/// <summary>
/// This file handles storm penalties.
/// </summary>
namespace ClimateOfFerngill
{
    public static class StormyWeather
    {
        private static double PercentOutside { get; set; }
        public static double TickPerSpan { get; set; }
        public static double TicksOutside { get; set; }
        private static double PenaltyThres { get; set; }
        private static int PenaltyAmt { get; set; }
        private static bool Enabled { get; set; }
        private static bool Initiated { get; set; } = false;

        public static void InitiateVariables(ClimateConfig c)
        {
            if (!Initiated)
            {
                PenaltyThres = .65;
                PenaltyAmt = c.StaminaPenalty;
                Enabled = c.StormyPenalty;
                Initiated = true;
            }
        }

        public static void CheckForStaminaPenalty(Action<string, bool> log, bool debugEnabled)
        {
            //safety check
            if (TickPerSpan == 0 || !Initiated)
                return;

            PercentOutside = TickPerSpan / TickPerSpan;
            if (debugEnabled) log("Ticks Outside was: " + PercentOutside + " with " + TickPerSpan + " ticks per span and " + TicksOutside + " ticks outside.", false);

            if (PercentOutside > PenaltyThres && Game1.isLightning)
            {
                Game1.player.Stamina -= PenaltyAmt; //make it negative //bugfix that was stupid self
                log("Current Stamina is " + Game1.player.Stamina, false);
                if (Game1.player.Stamina <= 20f)
                {
                    Game1.staminaShakeTimer = 1000;
                    for (int i = 0; i < 4; i++)
                    {
                        Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2((float)(Game1.random.Next(Game1.tileSize / 2) + Game1.viewport.Width - (48 + Game1.tileSize / 8)), (float)(Game1.viewport.Height - 224 - Game1.tileSize / 4 - (int)((double)(Game1.player.MaxStamina - 270) * 0.715))), false, 0.012f, Color.SkyBlue)
                        {
                            motion = new Vector2(-2f, -10f),
                            acceleration = new Vector2(0f, 0.5f),
                            local = true,
                            scale = (float)(Game1.pixelZoom + Game1.random.Next(-1, 0)),
                            delayBeforeAnimationStart = i * 30
                        });
                    }
                }
            }

            //reset the counters
            TickPerSpan = 0;
            TicksOutside = 0;
        }

    }
}
