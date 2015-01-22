using System;
using Microsoft.Xna.Framework.Content;

namespace Mechadrone1
{
    abstract class PlayerViewFactory
    {
        public static PlayerView Create(PlayerInfo playerInfo, CharacterInfo selectedCharacter, IScreenHoncho screenHoncho, ContentManager viewContentLoader)
        {
            Type viewType = null;
            object[] ctorParams = null;

            switch (playerInfo.Type)
            {
                case PlayerInfo.PlayerType.Local:
                    viewType = screenHoncho.HumanViewType;
                    ctorParams = new object[] { playerInfo, selectedCharacter, viewContentLoader };
                    break;
                case PlayerInfo.PlayerType.Remote:
                    viewType = screenHoncho.RemoteViewType;
                    ctorParams = new object[] { playerInfo, selectedCharacter };
                    break;
                case PlayerInfo.PlayerType.Bot:
                    viewType = screenHoncho.BotViewType;
                    ctorParams = new object[] { playerInfo, selectedCharacter };
                    break;
            }

            return Activator.CreateInstance(viewType, ctorParams) as PlayerView;
        }
    }
}
