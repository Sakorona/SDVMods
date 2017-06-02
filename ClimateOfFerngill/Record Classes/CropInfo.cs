using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimateOfFerngill
{
    public class CropInfo
    {
        public int ParentSheetIndex { get; set; }
        public double FrostLimit { get; set; }

        public CropInfo()
        {

        }

        public CropInfo(int index, double limit)
        {
            ParentSheetIndex = index;
            FrostLimit = limit;
        }
    }
}
