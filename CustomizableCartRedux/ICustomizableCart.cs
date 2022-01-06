﻿using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;

namespace CustomizableTravelingCart
{

    public interface ICustomizableCart
    {
        event EventHandler CartProcessingComplete;
        void AddItem(StardewValley.Object item, int price, int quality);
    }

    public class CustomizableCartAPI : ICustomizableCart
    {
        public event EventHandler CartProcessingComplete;
        private readonly IReflectionHelper Reflector;

        public CustomizableCartAPI(IReflectionHelper Ref)
        {
            this.Reflector = Ref;
        }

        internal void InvokeCartProcessingComplete()
        {
            //CustomizableCartRedux.Logger.Log("Event: CartProcessingComplete", LogLevel.Trace);
            if (CartProcessingComplete == null)
                return;
            Util.invokeEvent("CustomizableCartAPI.CartProcessingComplete", CartProcessingComplete.GetInvocationList(), null);
        }


        public void AddItem(StardewValley.Object item, int price, int quantity = 1)
        {
            Forest f = Game1.getLocationFromName("Forest") as Forest;
            bool travelingMerchantDay = f.travelingMerchantDay;
            if (travelingMerchantDay)
            {
                CustomizableCartRedux.APIItemsToBeAdded.Add(item, new int[] { price, quantity });
            }
        }
    }
}

