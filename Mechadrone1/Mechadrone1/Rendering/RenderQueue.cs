using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mechadrone1.Gameplay;

namespace Mechadrone1.Rendering
{
    class RenderQueue
    {
        public List<GameObject> Entries;

        public RenderQueue()
        {
            Entries = new List<GameObject>();
        }

        public void Execute()
        {

        }
    }
}
