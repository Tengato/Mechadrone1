namespace Manifracture
{
    public class HalfIntervalControlSpecification
    {
        public HalfIntervalControls ValueControl { get; set; }
        public bool IsValueInverted { get; set; }
        public BinaryControls EnablerControl { get; set; }
        public bool IsEnablerInverted { get; set; }
    }

    public class FullIntervalControlSpecification
    {
        public FullIntervalControls ValueControl { get; set; }
        public bool IsValueInverted { get; set; }
        public BinaryControls EnablerControl { get; set; }
        public bool IsEnablerInverted { get; set; }
    }

    public class FullAxisControlSpecification
    {
        public FullAxisControls ValueControl { get; set; }
        public bool IsValueInverted { get; set; }
    }
}
