using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;

namespace Mechadrone1.Gameplay
{
    interface ISimulationParticipant
    {
        ISpaceObject SimulationObject { get; }
    }
}
