using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Inventory;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Core.Managers.Character.Spells;
using AmeisenBotX.Core.Managers.Character.Talents;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character
{
    public interface ICharacterManager
    {
        ICharacterEquipment Equipment { get; }

        ICharacterInventory Inventory { get; }

        IItemComparator ItemComparator { get; set; }

        List<WowEquipmentSlot> ItemSlotsToSkip { get; set; }

        int LastLevelTrained { get; set; }

        int Money { get; }

        IEnumerable<IWowMount> Mounts { get; }

        Dictionary<string, (int, int)> Skills { get; }

        ISpellBook SpellBook { get; }

        ITalentManager TalentManager { get; }

        bool HasItemTypeInBag<T>(bool needsToBeUseable = false);

        bool IsAbleToUseArmor(IWowInventoryItem item);

        bool IsAbleToUseWeapon(IWowInventoryItem item);

        bool IsItemAnImprovement(IWowInventoryItem item, out IWowInventoryItem itemToReplace);

        void Jump();

        void MoveToPosition(Vector3 pos, float turnSpeed = 20.9f, float distance = 0.1f);

        void UpdateAll();

        void UpdateBags();

        void UpdateGear();
    }
}