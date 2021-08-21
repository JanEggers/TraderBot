namespace TraderBot.Extensions
{
    public class Macd
    {
        public double Slow { get; set; }

        public double Fast { get; set; }

        public double Value { get; set; }

        public double Signal { get; set; }

        public override string ToString()
        {
            return $"{Fast}/{Slow}/{Signal}";
        }
    }
}
