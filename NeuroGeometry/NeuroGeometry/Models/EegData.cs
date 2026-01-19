namespace NeuroGeometry.Abstractions
{
    public class EegData
    {
        public int PoorSignalLevel { get; set; }
        public Esense eSense { get; set; }
        public EegPower eegPower { get; set; }

        // Feature 1: Raw Wave Data
        public int rawEeg { get; set; }

        // Feature 3: Blink Strength
        public int blinkStrength { get; set; }
    }

    public class Esense
    {
        public int attention { get; set; }
        public int meditation { get; set; }
    }

    public class EegPower
    {
        public int delta { get; set; }
        public int theta { get; set; }
        public int lowAlpha { get; set; }
        public int highAlpha { get; set; }
        public int lowBeta { get; set; }
        public int highBeta { get; set; }
        public int lowGamma { get; set; }
        public int highGamma { get; set; }

        // Feature 4: Critical Hit Detection
        public int midGamma { get; set; }
    }
}