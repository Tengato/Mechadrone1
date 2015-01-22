using System;

namespace Mechadrone1
{
    class BVHInsertion
    {
        public enum InsertionMethod
        {
            AsSibling,
            AsChild,
        }

        public static BVHInsertion WorstInsertion { get; private set; } // It's often easiest to return this when we want to represent a failed search.

        public ImplicitBoundingBoxNode Location { get; private set; }
        public float Cost { get; set; }
        public float LocationInheritance { get; set; }
        public InsertionMethod Method { get; private set; }

        static BVHInsertion()
        {
            WorstInsertion = new BVHInsertion(null, BVHInsertion.InsertionMethod.AsChild);
            WorstInsertion.Cost = Single.MaxValue;
            WorstInsertion.LocationInheritance = Single.MaxValue;
        }

        public BVHInsertion(ImplicitBoundingBoxNode location, InsertionMethod method)
        {
            Location = location;
            Method = method;
        }
    }
}
