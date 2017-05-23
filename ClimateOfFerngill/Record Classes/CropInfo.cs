using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimateOfFerngill
{
    public class CropInfo
    {
        public string CropName { get; set; }
        public int ParentSheetIndex { get; set; }
        public double FrostLimit { get; set; }

        public CropInfo()
        {

        }

        public CropInfo(string name, int index, double limit)
        {
            CropName = name;
            ParentSheetIndex = index;
            FrostLimit = limit;
        }
    }
}
