using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    abstract class AgentState
    {
        protected long mDurationTicks;

        // Make sure the steering object can produce a usable movement immediately after Enter() is called.
        public abstract void Enter(/* inout */ SteeringBlender steering, Actor owner, IAgentStateManager agent);
        public abstract void Update(/* inout */ SteeringBlender steering, Actor owner, IAgentStateManager agent);
    }
}
