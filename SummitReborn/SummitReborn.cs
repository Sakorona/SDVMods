using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
<<<<<<< HEAD
using System.Reflection;
=======
>>>>>>> master

namespace SummitReborn
{
    public class SummitConfig
    {
        public bool Clouds = true;
    }

<<<<<<< HEAD
    public class SummitReborn : Mod, IAssetLoader
=======
    public class SummitReborn : Mod
>>>>>>> master
    {
        private float weatherX;
        private SummitConfig ModConfig;

<<<<<<< HEAD
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Maps\Railroad") || asset.AssetNameEquals(@"Maps\Summit");
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals(@"Maps\Railroad"))
                return (T)(object)this.Helper.Content.Load<xTile.Map>(@"Assets\Railroad_alt.tbin");
            if (asset.AssetNameEquals(@"Maps\Summit"))
                return (T)(object)this.Helper.Content.Load<xTile.Map>(@"Assets\Summit_alt.tbin");
            throw new NotSupportedException($"Unexpected asset name: {asset.AssetName}");
        }

=======
>>>>>>> master
        public override void Entry(IModHelper helper)
        {
            ModConfig = Helper.ReadConfig<SummitConfig>();
            GameEvents.UpdateTick += GameEvents_UpdateTick;
            GraphicsEvents.OnPreRenderHudEvent += GraphicsEvents_OnPreRenderHudEvent;
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.currentGameTime != null)
            {
                this.weatherX = this.weatherX + (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds * 0.03f;
            }            
        }

<<<<<<< HEAD
        private static int GetPixelZoom()
        {
            FieldInfo field = typeof(Game1).GetField(nameof(Game1.pixelZoom), BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                throw new InvalidOperationException($"The {nameof(Game1)}.{nameof(Game1.pixelZoom)} field could not be found.");
            return (int)field.GetValue(null);
        }

=======
>>>>>>> master
        private void GraphicsEvents_OnPreRenderHudEvent(object sender, EventArgs e)
        {
            //draw weather in the summit map
            if (Game1.isRaining && Game1.currentLocation.IsOutdoors && (Game1.currentLocation is Summit) && (!Game1.eventUp || Game1.currentLocation.isTileOnMap(new Vector2((float)(Game1.viewport.X / Game1.tileSize), (float)(Game1.viewport.Y / Game1.tileSize)))))
            {
                for (int index = 0; index < Game1.rainDrops.Length; ++index)
                {
                    Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[index].position, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[index].frame, -1, -1)), Color.White);
                }

                if (ModConfig.Clouds)
                {
<<<<<<< HEAD
                    int num2 = -61 * GetPixelZoom();
                    while (num2 < Game1.viewport.Width + 61 * GetPixelZoom())
                    {
                        Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2((float)num2 + this.weatherX % (float)(61 * GetPixelZoom()), (float)(-Game1.tileSize / 2)), new Rectangle?(new Rectangle(643, 1142, 61, 53)), Color.DarkSlateGray * 1f, 0.0f, Vector2.Zero, (float)GetPixelZoom(), SpriteEffects.None, 1f);
                        num2 += 61 * GetPixelZoom();
=======
                    int num2 = -61 * Game1.pixelZoom;
                    while (num2 < Game1.viewport.Width + 61 * Game1.pixelZoom)
                    {
                        Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2((float)num2 + this.weatherX % (float)(61 * Game1.pixelZoom), (float)(-Game1.tileSize / 2)), new Rectangle?(new Rectangle(643, 1142, 61, 53)), Color.DarkSlateGray * 1f, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1f);
                        num2 += 61 * Game1.pixelZoom;
>>>>>>> master
                    }
                }
            }
        }
    }
}
