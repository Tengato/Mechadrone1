namespace Mechadrone1
{
    // A PlayerView object can control an actor in the game. It collects the context of a player into one place
    // which is useful to have for various game queries. In an RTS game, the PlayerView would still control an actor - it would
    // probably just consist of a ControllerComponent that accepts game commands from the PlayerView.
    abstract class PlayerView
    {
        public int ActorId { get; private set; }
        public int PlayerId { get; private set; }

        // Describes how to spawn the actor when the player joins the game.
        public CharacterInfo AvatarDesc { get; private set; }

        public PlayerView(PlayerInfo playerInfo, CharacterInfo selectedCharacter)
        {
            PlayerId = playerInfo.PlayerId;
            ActorId = Actor.INVALID_ACTOR_ID;
            AvatarDesc = selectedCharacter;
        }

        public virtual void Load() { }

        public virtual void Unload() { }

        public virtual void AssignAvatar(int actorId)
        {
            ActorId = actorId;
        }
    }
}
