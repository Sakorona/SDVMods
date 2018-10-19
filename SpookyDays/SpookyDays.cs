using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Monsters;
using TwilightShards.Common;

namespace SpookyDays
{
    public class SpookyDays : Mod
    {
        private MersenneTwister Dice;
        private bool SetDarkness = false;
        private int EffectTimer = 0;
        private int EffectTimer2 = 0;
        private bool MonstersSummoned = false;
        private GameLocation MonsterLocation;
        private List<NPC> MonstersToKill = new List<NPC>();
        private Color darkColor = new Color(255, 255, 185);

        public override void Entry(IModHelper helper)
        {
            Dice = new MersenneTwister();
            GameEvents.UpdateTick += GameEvents_UpdateTick;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
           if (Dice.NextDouble() < .08)
           { 
               //trick or treat!
               Game1.addHUDMessage(new HUDMessage("You hear a disembodied voice... trick or treat!"));
               int rNum = Dice.Next(0,6);
               if (Dice.NextDouble() < .5)
               {
                 //trick
                 switch (rNum)
                    {
                        case 0:
                            //darkness!!!!
                            SetDarkness = true;
                            EffectTimer = 9000;
                            break;
                        case 1:
                            //bats!!!!!
                            Vector2 zero = Vector2.Zero;
                           
                            switch (Game1.random.Next(4))
                            {
                                case 0:
                                    zero.X = Dice.Next(Game1.currentLocation.map.Layers[0].LayerWidth);
                                    break;
                                case 1:
                                    zero.X = (Game1.currentLocation.map.Layers[0].LayerWidth - 1);
                                    zero.Y = Dice.Next(Game1.currentLocation.map.Layers[0].LayerHeight);
                                    break;
                                case 2:
                                    zero.Y = (Game1.currentLocation.map.Layers[0].LayerHeight - 1);
                                    zero.X = Dice.Next(Game1.currentLocation.map.Layers[0].LayerWidth);
                                    break;
                                case 3:
                                    zero.Y = Game1.random.Next(Game1.currentLocation.map.Layers[0].LayerHeight);
                                    break;
                            }

                            if (Utility.isOnScreen(zero * Game1.tileSize, Game1.tileSize))
                                zero.X -= Game1.viewport.Width;                           
                            
                            Bat bat = new Bat(zero * Game1.tileSize)
                            {
                                focusedOnFarmers = true,
                                wildernessFarmMonster = true
                            };
                            bat.reloadSprite();
                            MonstersToKill.Add(bat);
                            MonstersSummoned = true;
                            EffectTimer2 = 9000;
                            Game1.currentLocation.characters.Add(bat);
                            break;
                        default:
                            return;
                    }
                    
               }
               else { 
               //treat
                }
           }
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
           if (SetDarkness)
           {
                Game1.ambientLight = darkColor;
                EffectTimer--;
           }

           if (MonstersSummoned)
            {
                EffectTimer2--;

                if (EffectTimer2 == 0)
                {
                    
                }
            }

        }
    }
}
