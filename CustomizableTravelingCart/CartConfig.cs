namespace CustomizableTravelingCart
{
    public class CartConfig
    {
        public double MondayChance { get; set; }
        public double TuesdayChance { get; set; }
        public double WednesdayChance { get; set; }
        public double ThursdayChance { get; set; }
        public double FridayChance { get; set; }
        public double SaturdayChance { get; set; }
        public double SundayChance { get; set; }

        public CartConfig()
        {
            MondayChance = .2;
            TuesdayChance = .2;
            WednesdayChance = .2;
            ThursdayChance = .2;
            FridayChance = 1;
            SaturdayChance = .4;
            SundayChance = 1;
        }
    }
}
