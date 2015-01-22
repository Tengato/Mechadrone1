using System;

namespace Mechadrone1
{
    class HubScreenHoncho : IScreenHoncho
    {
        public Type HumanViewType { get { return typeof(HubScreenHumanView); } }
        public Type BotViewType { get { return typeof(BotView); } }
        public Type RemoteViewType { get { return typeof(RemoteView); } }
        public bool PlayersUseSeparateViewports { get { return false; } }
    }
}
