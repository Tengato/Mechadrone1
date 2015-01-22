using System;

namespace Mechadrone1
{
    class ActionScreenHoncho : IScreenHoncho
    {
        public Type HumanViewType { get { return typeof(ActionScreenHumanView); } }
        public Type BotViewType { get { return typeof(BotView); } }
        public Type RemoteViewType { get { return typeof(RemoteView); } }
        public bool PlayersUseSeparateViewports { get { return true; } }
    }
}
