using System.Collections.Generic;

namespace Manifracture
{
    public class InputMap
    {
        public Dictionary<BinaryControlActions, List<BinaryControls>> BinaryMap { get; set; }
        public Dictionary<HalfIntervalControlActions, HalfIntervalControlSpecification> HalfIntervalMap { get; set; }
        public Dictionary<FullIntervalControlActions, FullIntervalControlSpecification> FullIntervalMap { get; set; }
        public Dictionary<FullAxisControlActions, FullAxisControlSpecification> FullAxisMap { get; set; }

        public static InputMap Empty { get; private set; }

        static InputMap()
        {
            Empty = new InputMap();
            Empty.BinaryMap = new Dictionary<BinaryControlActions, List<BinaryControls>>();
            Empty.HalfIntervalMap = new Dictionary<HalfIntervalControlActions, HalfIntervalControlSpecification>();
            Empty.FullIntervalMap = new Dictionary<FullIntervalControlActions, FullIntervalControlSpecification>();
            Empty.FullAxisMap = new Dictionary<FullAxisControlActions, FullAxisControlSpecification>();
        }
    }
}
