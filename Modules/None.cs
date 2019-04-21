using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins;

namespace AdvancedInventoryManager.Modules
{
    public static class None
    {
        private static bool IsInitialized = false;
        public static void Call()
        {
            if (Skandia.Me.InAction || (Main.settings.AlwaysSalvageDiscardUseInCombat && Skandia.Me.InCombat))
                return;
            else if (Main.TW.IdsForAlwaysSalvage.Count != 0)
                alwaysSalvageItem();
            else if (Main.TW.IdsForAlwaysDiscard.Count != 0)
                alwaysDiscardItem();
            else if (Main.TW.IdsForAlwaysUse.Count != 0 && !Main.IsInventoryFull)
                alwaysUseItem();
            else if (!IsInitialized)
            {
                Main.TW.CheckInventoryForChange();
                if (Main.TW.HasInventoryChanged && !Main.TW.IsWorking)
                {
                    new Task(() => Main.TW.RefreshHashSets()).Start();
                    IsInitialized = true;
                    Main.IsInventoryFull = false;
                }
            }
            else if (!Main.TW.IsWorking)
            {
                H.Log("Finished Always Use/Salvage/Discard");
                IsInitialized = false;
            }
        }
        private static void alwaysUseItem()
        {
            uint itemId = 0;
            Main.TW.IdsForAlwaysUse.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0 || Skandia.Me.InventoryTotalEmptySlots == 0)
                Main.TW.IdsForAlwaysUse.TryDequeue(out itemId);
            else
            {
                item.Use();
                H.Log("Using " + ObjectManager.GetItemInfo(item.Id).Name);
            }
        }
        private static void alwaysDiscardItem()
        {
            uint itemId = 0;
            Main.TW.IdsForAlwaysDiscard.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0)
                Main.TW.IdsForAlwaysDiscard.TryDequeue(out itemId);
            else
            {
                item.Discard();
                H.Log("Discarding " + ObjectManager.GetItemInfo(item.Id).Name);
            }
        }
        private static void alwaysSalvageItem()
        {
            uint itemId = 0;
            Main.TW.IdsForAlwaysSalvage.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0 || Skandia.Me.GetCurrency(MoneyType.Fragments) == 30000 ||
                ((Main.settings.DontSalvageWhenFragsExceed != 0 && Main.settings.DontSalvageWhenFragsExceed != 30000) && Skandia.Me.GetCurrency(MoneyType.Fragments) > Main.settings.DontSalvageWhenFragsExceed))
                Main.TW.IdsForAlwaysSalvage.TryDequeue(out itemId);
            else
            {
                item.Salvage();
                H.Log("Salvaging " + ObjectManager.GetItemInfo(item.Id).Name);
            }
        }
        private static Inventory selectSafeItem(uint _itemId)//Avoid getting items from the inventory which are protected
        {
            if (Main.settings.InventoryToBeUnaffected == 1)
                return Skandia.Me.Inventory.FirstOrDefault(x => x.Id == _itemId && x.InventoryIndex >= Main.settings.InventorySlotsToBeUnaffected);
            else if (Main.settings.InventoryToBeUnaffected == 2)
                return Skandia.Me.Inventory.FirstOrDefault(x => x.Id == _itemId && x.InventoryIndex <= (Skandia.Me.InventoryTotalEmptySlots + Skandia.Me.InventoryTotalUsedSlots) -
                    Main.settings.InventorySlotsToBeUnaffected);
            else
                return Skandia.Me.Inventory.FirstOrDefault(x => x.Id == _itemId);
        }
    }
}
