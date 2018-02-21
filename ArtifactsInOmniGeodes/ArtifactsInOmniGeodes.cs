using StardewModdingAPI;

namespace ArtifactsInOmniGeodes
{
    public class ArtifactsInOmniGeodes : Mod, IAssetEditor
    {
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Data\ObjectInformation");
        }

        public void Edit<T>(IAssetData asset)
        {
            int id = 749;
            asset
            .AsDictionary<int, string>()
            .Set(id, "Omni Geode/0/-300/Basic/A blacksmith can break this open for you. These geodes contain a wide variety of Items./538 542 548 549 552 555 556 557 558 566 568 569 571 574 576 541 544 545 546 550 551 559 560 561 564 567 572 573 577 539 540 543 547 553 554 562 563 565 570 575 578 579 580 581 582 583 584 585 586 587 588 589 96 97 98 99 100 101 102 103 104 105 106 107 108 109 110 111 112 113 114 115 116 117 118 119 120 121 122 123 124 125 126 127");
        }

        public override void Entry(IModHelper helper)
        {
            //....
        }
    }
}
