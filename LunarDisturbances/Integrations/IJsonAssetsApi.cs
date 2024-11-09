using System;
using StardewModdingAPI;

namespace TwilightShards.LunarDisturbances.Integrations
{
    public interface IJsonAssetsApi
    {
            /// <summary>Load a folder as a Json Assets content pack.</summary>
            /// <param name="path">The absolute path to the content pack folder.</param>
            void LoadAssets(string path);

            /// <summary>Load a folder as a Json Assets content pack.</summary>
            /// <param name="path">The absolute path to the content pack folder.</param>
            /// <param name="translations">The translations to use for <c>TranslationKey</c> fields, or <c>null</c> to load the content pack's <c>i18n</c> folder if present.</param>
            void LoadAssets(string path, ITranslationHelper translations);

            string GetObjectId(string name);
            string GetCropId(string name);
            string GetFruitTreeId(string name);
            string GetBigCraftableId(string name);
            string GetHatId(string name);
            string GetWeaponId(string name);
            string GetClothingId(string name);

            event EventHandler ItemsRegistered;
            event EventHandler AddedItemsToShop;
    }
}
