using AmeisenBotX.Core.Data.Objects.WowObjects;

namespace AmeisenBotX.Core.Common.Extensions
{
    public static class WowInterfaceExtensions
    {
        public static WowUnit LastTarget(this WowInterface wowInterface) => wowInterface.ObjectManager.LastTarget;

        public static WowUnit Pet(this WowInterface wowInterface) => wowInterface.ObjectManager.Pet;

        public static WowPlayer Player(this WowInterface wowInterface) => wowInterface.ObjectManager.Player;

        public static WowUnit Target(this WowInterface wowInterface) => wowInterface.ObjectManager.Target;
    }
}