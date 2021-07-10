using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Inventory;
using AmeisenBotX.Core.Engines.Character.Inventory.Objects;
using AmeisenBotX.Core.Engines.Character.Spells;
using AmeisenBotX.Core.Engines.Character.Talents;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Character
{
    public interface ICharacterManager
    {
        CharacterEquipment Equipment { get; }

        CharacterInventory Inventory { get; }

        IItemComparator ItemComparator { get; set; }

        List<WowEquipmentSlot> ItemSlotsToSkip { get; set; }

        int Money { get; }

        List<WowMount> Mounts { get; }

        Dictionary<string, (int, int)> Skills { get; }

        SpellBook SpellBook { get; }

        TalentManager TalentManager { get; }

        void AntiAfk();

        void ClickToMove(Vector3 pos, ulong guid, WowClickToMoveType clickToMoveType = WowClickToMoveType.Move, float turnSpeed = 20.9f, float distance = 0.5f);

        bool HasItemTypeInBag<T>(bool needsToBeUseable = false);

        bool IsAbleToUseArmor(WowArmor item);

        bool IsAbleToUseWeapon(WowWeapon item);

        bool IsItemAnImprovement(IWowInventoryItem item, out IWowInventoryItem itemToReplace);

        void Jump();

        void MoveToPosition(Vector3 pos, float turnSpeed = 20.9f, float distance = 0.1f);

        void UpdateAll();

        void UpdateBags();

        void UpdateGear();
    }
}