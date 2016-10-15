using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

namespace ClimatesOfFerngill
{
    public class ModConfig : Config
    {
        public int[] springWeather { get; set; }
        public int[] summerWeather { get; set; }
        public int[] autumnWeather { get; set; }
        public int[] winterWeather { get; set; }

        public bool SuppressLog { get; set; }

        public override T GenerateDefaultConfig<T>()
        {
            //sunny, rain, debris, stormy, snow
            springWeather = new int[5] { 35, 35, 30, 15, 0 };
            summerWeather = new int[5] { 68, 32, 0, 85, 0 };
            autumnWeather = new int[5] { 30, 30, 40, 15, 0 };
            winterWeather = new int[5] { 35, 0, 0, 0 , 65 };

            SuppressLog = false;
            return this as T;
        }
    }
}
