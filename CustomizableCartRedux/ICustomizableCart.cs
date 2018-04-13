using StardewValley;
using StardewValley.Locations;
using System;

namespace CustomizableCartRedux
{
    public interface ICustomizableCart
    {
        event EventHandler CartProcessingComplete;
        void AddItem(Item item, int price, int quality);
    }

    public class CustomizableCartAPI : ICustomizableCart
    {
        public event EventHandler CartProcessingComplete;
        internal void InvokeCartProcessingComplete()
        {
            Log.trace("Event: CartProcessingComplete");
            if (CartProcessingComplete == null)
                return;
            Util.invokeEvent("CustomizableCartAPI.CartProcessingComplete", CartProcessingComplete.GetInvocationList(), null);
        }

        public void AddItem(Item item, int price, int quantity = 1)
        {
            Forest f = Game1.getLocationFromName("Forest") as Forest;
            bool travelingMerchantDay = f.travelingMerchantDay;
            if (travelingMerchantDay)
            {
                f.travelingMerchantStock.Add(item, new int[]
                {
                    price,
                    quantity
                });
            }
        }
    }
}

