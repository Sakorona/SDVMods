﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ClimatesOfFerngillRebuild.Patches
{
    public static class WeatherDebrisPatches
    {
        static void CtorPostfix(WeatherDebris __instance)
        {
            Rectangle sourceRect = ClimatesOfFerngill.Reflection.GetField<Rectangle>(__instance,"sourceRect").GetValue();
            double prob = ClimatesOfFerngill.Dice.NextDouble();
            int which;
            if (prob < .6)
                which = 0;
            else if (prob >= .6 && prob < .8)
                which = 1;
            else if (prob >= .8 && prob < .9)
                which = 2;
            else
                which = 3;


            switch (which)
            {
                case 0:
                    sourceRect = new Rectangle(0,160,16,16);
                    break;
                case 1:
                    sourceRect = new Rectangle(0,176,16,16);
                    break;
                case 2:
                    sourceRect = new Rectangle(0,192,16,16);
                    break;
                case 3:
                    sourceRect = new Rectangle(0,208,16,16);
                    break;
            }

            ClimatesOfFerngill.Reflection.GetField<Rectangle>(__instance, "sourceRect").SetValue(sourceRect);
        }

        static bool UpdatePrefix(WeatherDebris __instance, bool slow, ref Rectangle ___sourceRect, ref bool ___blowing)
        {
            __instance.position.X += __instance.dx + (slow ? 0.0f : WeatherDebris.globalWind);
            __instance.position.Y += __instance.dy - (slow ? 0.0f : -0.5f);
            if (__instance.dy < 0.0 && !___blowing)
                __instance.dy += 0.01f;
            if (!Game1.fadeToBlack && Game1.fadeToBlackAlpha <= 0.0)
            {
                if (__instance.position.X < -80.0)
                {
                    __instance.position.X = Game1.viewport.Width;
                    __instance.position.Y = Game1.random.Next(0, Game1.viewport.Height - 64);
                }
                if ((double)__instance.position.Y > (Game1.viewport.Height + 16))
                {
                    __instance.position.X = Game1.random.Next(0, Game1.viewport.Width);
                    __instance.position.Y = -64f;
                    __instance.dy = Game1.random.Next(-15, 10) / (slow ? (Game1.random.NextDouble() < 0.1 ? 5f : 200f) : 50f);
                    __instance.dx = Game1.random.Next(-10, 0) / (slow ? 200f : 50f);
                }
                else if (__instance.position.Y < -64.0)
                {
                    __instance.position.Y = Game1.viewport.Height;
                    __instance.position.X = Game1.random.Next(0, Game1.viewport.Width);
                }
            }
            if (___blowing)
            {
                __instance.dy -= 0.01f;
                if (Game1.random.NextDouble() < 0.006 || __instance.dy < -2.0)
                    ___blowing = false;
                
            }
            else if (!slow && Game1.random.NextDouble() < 0.001 && Game1.currentSeason != null && (Game1.currentSeason.Equals("spring") || Game1.currentSeason.Equals("summer")))
               ___blowing = true;
            switch (__instance.which)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    __instance.animationTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                    if (__instance.animationTimer > 0)
                        break;
                    __instance.animationTimer = 100 + __instance.animationIntervalOffset;
                    __instance.animationIndex += __instance.animationDirection;
                    if (__instance.animationDirection == 0)
                        __instance.animationDirection = __instance.animationIndex < 9 ? 1 : -1;
                    if (__instance.animationIndex > 10)
                    {
                        if (Game1.random.NextDouble() < 0.82)
                        {
                            --__instance.animationIndex;
                            __instance.animationDirection = 0;
                            __instance.dx += 0.1f;
                            __instance.dy -= 0.2f;
                        }
                        else
                            __instance.animationIndex = 0;
                    }
                    else if (__instance.animationIndex == 4 && __instance.animationDirection == -1)
                    {
                        ++__instance.animationIndex;
                        __instance.animationDirection = 0;
                        __instance.dx -= 0.1f;
                        __instance.dy -= 0.1f;
                    }
                    if (__instance.animationIndex == 7 && __instance.animationDirection == -1)
                        __instance.dy -= 0.2f;
                    if (__instance.which == 3)
                        break;
                    ___sourceRect.X = 0 + __instance.animationIndex * 16;
                    break;
            }

            return false;
        }

        static bool DrawPrefix(SpriteBatch b, WeatherDebris __instance, Rectangle ___sourceRect)
        {
            b.Draw(ClimatesOfFerngill.OurIcons.LeafSprites, __instance.position, new Rectangle?(___sourceRect), Color.White, 0.0f, Vector2.Zero, 3f, SpriteEffects.None, 1E-06f);
            return false;
        }
    }
}
