using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilightShards.Common;

namespace ClimatesOfFerngillRebuild
{
    class Descriptions
    {
        private List<string> NonTownLocations = new List<string>()
        {
            "Castle Village",
            "Basket Town",
            "Pine Mesa City",
            "Point Break",
            "Minister Valley",
            "Grampleton",
            "Zuzu City",
            "Fort Josa",
            "Chestervale",
            "Fern Island",
            "Tanker Grove",
            "Pathos Isle"
        };

        private ITranslationHelper Helper;

        public Descriptions(ITranslationHelper Translaton)
        {
            Helper = Translaton;
        }


        internal string GetDescOfDay(SDate date)
        {
            return Helper.Get("date" + GeneralFunctions.FirstLetterToUpper(date.Season) + date.Day);
        }
    }
}
