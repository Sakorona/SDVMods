using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace HappyFishJump
{
    public class JumpFish
    {
        public float jumpTime = 1f;
        public Vector2 startPosition;
        public Vector2 endPosition;
        protected float _age;
        public Item _fishObject;
        protected bool _flipped;
        public Vector2 position;
        public float jumpHeight;
        public float angularVelocity;
        public float angle;

        public JumpFish(Item fish, Vector2 start_position, Vector2 end_position)
        {
            this.angularVelocity = (float)((double)Utility.RandomFloat(20f, 40f, (Random)null) * 3.14159274101257 / 180.0);
            this.startPosition = start_position;
            this.endPosition = end_position;
            this.position = this.startPosition;
            this._fishObject = fish;
            if ((double)this.startPosition.X > (double)this.endPosition.X)
                this._flipped = true;
            this.jumpHeight = Utility.RandomFloat(75f, 100f, (Random)null);
            this.Splash();
        }

        public void Splash()
        {
            if (HappyFishJump.ModConfig.SplashSound)
                Game1.playSound("dropItemInWater");

            Game1.currentLocation?.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, this.position + new Vector2(-0.5f, -0.5f) * 64f, false, false)
                {
                    delayBeforeAnimationStart = 0,
                    layerDepth = this.startPosition.Y / 10000f
                });
        }

        public bool Update(float time)
        {
            this._age += time;
            this.angle += this.angularVelocity * time;
            if ((double)this._age >= (double)this.jumpTime)
            {
                this._age = time;
                this.Splash();
                return true;
            }
            this.position.X = Utility.Lerp(this.startPosition.X, this.endPosition.X, this._age / this.jumpTime);
            this.position.Y = Utility.Lerp(this.startPosition.Y, this.endPosition.Y, this._age / this.jumpTime);
            return false;
        }

        public void Draw(SpriteBatch b)
        {
            float drawn_angle = this.angle;
            SpriteEffects effect = SpriteEffects.None;
            if (this._flipped)
            {
                effect = SpriteEffects.FlipHorizontally;
                drawn_angle *= -1f;
            }
            float draw_scale = 1f;
            Vector2 draw_position = this.position + new Vector2(0f, (float)Math.Sin((double)(this._age / this.jumpTime) * Math.PI) * (0f - this.jumpHeight));
            Vector2 origin = new(8f, 8f);
            ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this._fishObject.QualifiedItemId);
            b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, draw_position), itemData.GetSourceRect(), Color.White, drawn_angle, origin, 4f * draw_scale, effect, this.position.Y / 10000f + 1E-06f);
            b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position), Game1.shadowTexture.Bounds, Color.White * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Width / 2, Game1.shadowTexture.Bounds.Height / 2), 2f, effect, this.position.Y / 10000f + 1E-06f);
        }
    }
}