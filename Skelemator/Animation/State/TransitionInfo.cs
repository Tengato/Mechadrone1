using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Skelemator
{
    public class TransitionInfo
    {
        public string SourceStateFilter;
        public string DestinationStateFilter;
        public TransitionType Type;
        public float DurationInSeconds;
        public bool UseSmoothStep;

        public bool IsMatch(string sourceStateName, string destStateName)
        {
            if (Regex.IsMatch(sourceStateName, SourceStateFilter) &&
                Regex.IsMatch(destStateName, DestinationStateFilter))
            {
                return true;
            }

            return false;
        }

    }

    public enum TransitionType
    {
        Frozen,
        Smooth,
    }
}
