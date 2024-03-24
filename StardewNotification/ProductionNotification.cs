using System;
using System.Collections.Generic;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace StardewNotification
{
    public class ProductionNotification
    {
        /// <summary>
        /// Production notification.
        /// Handles notifications for all production machines like
        /// Bee House, Cheese Press, Keg, etc. 
        /// </summary>
        public void CheckProductionAroundFarm(ITranslationHelper Trans)
        { 
            if (StardewNotification.Config.NotifyFarm) CheckFarmProductions(Trans);
            if (StardewNotification.Config.NotifyShed) CheckShedProductions(Trans);
            if (StardewNotification.Config.NotifyGreenhouse) CheckGreenhouseProductions(Trans);
            if (StardewNotification.Config.NotifyCellar) CheckCellarProductions(Trans);
            if (StardewNotification.Config.NotifyBarn) CheckBarnProductions(Trans);
        }

        public void CheckBarnProductions(ITranslationHelper Trans)
        {
            if (StardewNotification.Config.NotifyBarn)
            {
                //get barn(s)
                List<StardewValley.Object> autoGrabbers = new();
                Utility.ForEachLocation(location =>
                {
                    if (location is AnimalHouse)
                    {
                        foreach (StardewValley.Object obj in location.objects.Values)
                        {
                            if (obj.QualifiedItemId == "(BC)165" && ((obj.heldObject.Value as Chest)?.Items.CountItemStacks() ?? 0) > 0)
                                autoGrabbers.Add(obj);
                        }
                    }

                    return true;
                });

                foreach (var ag in autoGrabbers)
                {
                    Game1.addHUDMessage(new HUDMessage(Trans.Get("autoGrabber")));
                }
            }
        }

        public void CheckFarmProductions(ITranslationHelper Trans)
        {
            if (StardewNotification.Config.NotifyFarm)
            {
                CheckObjectsInLocation(Game1.getFarm());
                CheckFish(Trans, Game1.getFarm());
            }
        }

        public void CheckFish(ITranslationHelper Trans, Farm f)
        {
            foreach (Building b in f.buildings)
            {
                if (b is FishPond fish && fish.output.Value != null)
                {
                    Util.ShowMessage(Trans.Get("FishPond"));
                }
            }
        }

        public void CheckShedProductions(ITranslationHelper Trans)
        {
            if (StardewNotification.Config is null)
                Console.WriteLine("Config is null");

            if (StardewNotification.Config.NotifyShed)
            {
                Farm f = Game1.getFarm();

                if (f is null)
                    Console.WriteLine("Farm is null. Somehow.");

                if (f.buildings is null)
                    Console.WriteLine("Farm Buildings is null. Somehow.");

                foreach (var building in f.buildings)
                {
                    if (building.indoors.Value is Shed)
                    {
                        CheckObjectsInLocation(building.indoors.Value);
                    }
                }
            }
        }

        public void CheckGreenhouseProductions(ITranslationHelper Trans)
        {
            if (StardewNotification.Config.NotifyGreenhouse)
            {
                CheckObjectsInLocation(Game1.getLocationFromName("Greenhouse"));
            }
        }

        public void CheckCellarProductions(ITranslationHelper Trans)
        {
            if (StardewNotification.Config.NotifyCellar)
            {
                Console.WriteLine("RUNNING CELLAR NOTIFY");
                CheckObjectsInLocation(Game1.getLocationFromName("Cellar"));
            }
        }

        private void CheckObjectsInLocation(GameLocation location)
        {
            var counter = new Dictionary<StardewValley.Object, int>();

            foreach (var pair in location.Objects.Pairs)
            {
                if (!pair.Value.readyForHarvest.Value) continue;

                if (pair.Value.heldObject is not null)
                {
                    if (counter.ContainsKey(pair.Value.heldObject.Value)) counter[pair.Value.heldObject.Value]++;
                    else counter.Add(pair.Value.heldObject.Value, 1);
                }
                else
                {
                    if (counter.ContainsKey(pair.Value)) counter[pair.Value]++;
                    else counter.Add(pair.Value, 1);
                }
            }

            foreach (var pair in counter)
                Game1.addHUDMessage(HUDMessage.ForItemGained(pair.Key, pair.Value));
        }
    }
}