using TwilightCore.PRNG;
using StardewModdingAPI;
using StardewValley;
using TwilightCore.StardewValley;

namespace ClimateOfFerngill
{
    /// <summary>
    /// This tracks buffs/debuffs that affect the farmer.
    /// </summary>
    public class FarmerStatus
    {
        private ClimateConfig Settings;
        private IMonitor Logger;
        private MersenneTwister pRNG;
        
        private static int MedicineID = 351;

        private bool HasACold;
        private bool HasGottenColdToday;

        public FarmerStatus(ClimateConfig Config, IMonitor Monitor, MersenneTwister Dice)
        {
            Settings = Config;
            Logger = Monitor;
            HasACold = false;
            pRNG = Dice;
        }

        public void UpdateForNewDay()
        {
            HasACold = false;
            HasGottenColdToday = false;
        }

        public void CheckToRemoveCold()
        {
            if (Game1.player.itemToEat.parentSheetIndex == MedicineID && Game1.isEating)
            {
                if (HasACold) RemoveCold();
            }
        }

        public void CatchACold()
        {
            //run non specific code first
            if (Game1.currentLocation.IsOutdoors && Game1.isLightning && !HasGottenColdToday)
            {
                double diceChance = pRNG.NextDouble();
                if (Settings.TooMuchInfo)
                    Logger.Log($"The chance of exhaustion is: {diceChance} with the configured chance of {Settings.DiseaseChance}");

                if (diceChance < Settings.DiseaseChance)
                {
                    HasACold = true;
                    SDVUtilities.ShowMessage("The storm has caused you to get a cold!");
                    HasGottenColdToday = true;
                }
            }
        }

        public void TenMinuteUpdate()
        {
            //disease code.
            if (HasACold)
            {
                Game1.player.stamina -= Settings.StaminaPenalty;
                if (pRNG.NextDouble() < .15) SDVUtilities.ShowMessage("You have a cold, and feel worn out!");
            }            
        }

        public bool IsSick()
        {
            return this.HasACold;
        }

        public void RemoveCold()
        {
            HasACold = false;
            Game1.addHUDMessage(new HUDMessage("You are no longer exhausted!"));
        }
    }
}
