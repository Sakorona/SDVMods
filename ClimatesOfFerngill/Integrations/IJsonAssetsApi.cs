using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimatesOfFerngillRebuild.Integrations
{
    public interface IJsonAssetsApi
    {
        void LoadAssets(string path);

        int GetObjectId(string name);
        int GetCropId(string name);
        int GetFruitTreeId(string name);
        int GetBigCraftableId(string name);

        event EventHandler AddedItemsToShop;
    }
}
