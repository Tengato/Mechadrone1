using System;

namespace Mechadrone1
{
    delegate void ActorEventHandler(object sender, ActorEventArgs e);

    class ActorEventArgs : EventArgs
    {
        public int ActorId { get; set; }

        public ActorEventArgs(int actorId)
            : base()
        {
            ActorId = actorId;
        }
    }

}
