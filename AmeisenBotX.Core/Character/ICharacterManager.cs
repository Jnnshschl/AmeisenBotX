using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Spells;
using AmeisenBotX.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character
{
    public interface ICharacterManager
    {
        CharacterEquipment Equipment { get; }

        CharacterInventory Inventory { get; }

        IWowItemComparator ItemComparator { get; set; }

        int Money { get; }

        List<string> Skills { get; }

        SpellBook SpellBook { get; }

        void AntiAfk();

        bool IsAbleToUseArmor(WowArmor item);

        bool IsAbleToUseWeapon(WowWeapon item);

        bool IsItemAnImprovement(IWowItem item, out IWowItem itemToReplace);

        void Jump();

        void MoveToPosition(Vector3 pos, float turnSpeed = 20.9f, float distance = 0.5f);

        void UpdateAll();

        void UpdateCharacterGear();
    }
}