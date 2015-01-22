using System;

namespace Mechadrone1
{
    interface IScreenHoncho
    {
        Type HumanViewType { get; }
        Type BotViewType { get; }
        Type RemoteViewType { get; }
        bool PlayersUseSeparateViewports { get; }
    }
}
