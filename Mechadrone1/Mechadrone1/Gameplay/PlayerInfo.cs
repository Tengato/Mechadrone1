namespace Mechadrone1
{
    class PlayerInfo
    {
        public enum PlayerType
        {
            Local,
            Remote,
            Bot,
        }

        public PlayerInfo(int playerId, PlayerType playerType)
        {
            PlayerId = playerId;
            Type = playerType;
        }

        public int PlayerId { get; set; }
        public PlayerType Type { get; set; }
    }
}
