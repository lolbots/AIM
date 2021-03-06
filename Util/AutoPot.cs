﻿//Part of the opensource Marksman# project by Kortatu, xQx & legacy
//which can be found here http://www.joduska.me/forum/topic/977-marksman/

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace AIM.Util
{
    internal class PotionManager
    {
        private readonly Menu ExtrasMenu;
        private List<Potion> potions;

        public PotionManager(Menu extrasMenu)
        {
            ExtrasMenu = extrasMenu;
            potions = new List<Potion>
            {
                new Potion
                {
                    Name = "ItemCrystalFlask",
                    MinCharges = 1,
                    ItemId = (ItemId) 2041,
                    Priority = 1,
                    TypeList = new List<PotionType> { PotionType.Health, PotionType.Mana }
                },
                new Potion
                {
                    Name = "RegenerationPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2003,
                    Priority = 2,
                    TypeList = new List<PotionType> { PotionType.Health }
                },
                new Potion
                {
                    Name = "ItemMiniRegenPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2010,
                    Priority = 4,
                    TypeList = new List<PotionType> { PotionType.Health, PotionType.Mana }
                },
                new Potion
                {
                    Name = "FlaskOfCrystalWater",
                    MinCharges = 0,
                    ItemId = (ItemId) 2004,
                    Priority = 3,
                    TypeList = new List<PotionType> { PotionType.Mana }
                }
            };
            Load();
        }

        private void Load()
        {
            potions = potions.OrderBy(x => x.Priority).ToList();
            ExtrasMenu.AddSubMenu(new Menu("Potion Manager", "PotionManager"));

            ExtrasMenu.SubMenu("PotionManager").AddSubMenu(new Menu("Health", "Health"));
            ExtrasMenu.SubMenu("PotionManager")
                .SubMenu("Health")
                .AddItem(new MenuItem("HealthPotion", "Use Health Potion").SetValue(true));
            ExtrasMenu.SubMenu("PotionManager")
                .SubMenu("Health")
                .AddItem(new MenuItem("HealthPercent", "HP Trigger Percent").SetValue(new Slider(30)));

            ExtrasMenu.SubMenu("PotionManager").AddSubMenu(new Menu("Mana", "Mana"));
            ExtrasMenu.SubMenu("PotionManager")
                .SubMenu("Mana")
                .AddItem(new MenuItem("ManaPotion", "Use Mana Potion").SetValue(true));
            ExtrasMenu.SubMenu("PotionManager")
                .SubMenu("Mana")
                .AddItem(new MenuItem("ManaPercent", "MP Trigger Percent").SetValue(new Slider(30)));

            Game.OnGameUpdate += OnGameUpdate;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (ObjectHandler.Player.HasBuff("Recall") || ObjectHandler.Player.InFountain())
            {
                return;
            }

            try
            {
                if (ExtrasMenu.Item("HealthPotion").GetValue<bool>())
                {
                    if (GetPlayerHealthPercentage() <= ExtrasMenu.Item("HealthPercent").GetValue<Slider>().Value)
                    {
                        var healthSlot = GetPotionSlot(PotionType.Health);
                        if (!IsBuffActive(PotionType.Health))
                        {
                            ObjectHandler.Player.Spellbook.CastSpell(healthSlot.SpellSlot);
                        }
                    }
                }
                if (ExtrasMenu.Item("ManaPotion").GetValue<bool>())
                {
                    if (GetPlayerManaPercentage() <= ExtrasMenu.Item("ManaPercent").GetValue<Slider>().Value)
                    {
                        var manaSlot = GetPotionSlot(PotionType.Mana);
                        if (!IsBuffActive(PotionType.Mana))
                        {
                            ObjectHandler.Player.Spellbook.CastSpell(manaSlot.SpellSlot);
                        }
                    }
                }
            }

            catch (Exception) {}
        }

        private InventorySlot GetPotionSlot(PotionType type)
        {
            return (from potion in potions
                where potion.TypeList.Contains(type)
                from item in ObjectHandler.Player.InventoryItems
                where item.Id == potion.ItemId && item.Charges >= potion.MinCharges
                select item).FirstOrDefault();
        }

        private bool IsBuffActive(PotionType type)
        {
            return (from potion in potions
                where potion.TypeList.Contains(type)
                from buff in ObjectHandler.Player.Buffs
                where buff.Name == potion.Name && buff.IsActive
                select potion).Any();
        }

        private static float GetPlayerHealthPercentage()
        {
            return ObjectHandler.Player.Health * 100 / ObjectHandler.Player.MaxHealth;
        }

        private static float GetPlayerManaPercentage()
        {
            return ObjectHandler.Player.Mana * 100 / ObjectHandler.Player.MaxMana;
        }

        private enum PotionType
        {
            Health,
            Mana
        };

        private class Potion
        {
            public string Name { get; set; }
            public int MinCharges { get; set; }
            public ItemId ItemId { get; set; }
            public int Priority { get; set; }
            public List<PotionType> TypeList { get; set; }
        }
    }
}