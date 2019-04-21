using Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedInventoryManager.Modules
{
    public static class Sell
    {
        private static bool IsInitialized = false;
        public static void Call()
        {
            // Checks to NOT use the Sell functionality
            if (!Main.settings.SellItemsToVendorWhenWindowOpen)
                return;
            if (Main.TW.IdsForSalvage.Count != 0)
                salvageItem();
            else if (Main.TW.IdsForAlwaysSalvage.Count != 0)
                alwaysSalvageItem();
            else if (Main.TW.IdsForSell.Count != 0)
                sellItem();
            else if (Main.TW.IdsForDiscard.Count != 0)
                discardItem();
            else if (Main.TW.IdsForAlwaysDiscard.Count != 0)
                alwaysDiscardItem();
            else if (Main.TW.IdsForUse.Count != 0 && !Main.IsInventoryFull)
                useItem();
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
                H.Log("Finished selling");
                IsInitialized = false;
            }
        }
        #region Sell/Use/etc. methods
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
        private static void useItem()
        {
            uint itemId = 0;
            Main.TW.IdsForUse.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0 || Skandia.Me.InventoryTotalEmptySlots == 0)
                Main.TW.IdsForUse.TryDequeue(out itemId);
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
        private static void discardItem()
        {
            uint itemId = 0;
            Main.TW.IdsForDiscard.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0)
                Main.TW.IdsForDiscard.TryDequeue(out itemId);
            else
            {
                item.Discard();
                H.Log("Discarding " + ObjectManager.GetItemInfo(item.Id).Name);
            }
        }
        private static void sellItem()
        {
            uint itemId = 0;
            Main.TW.IdsForSell.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0)
                Main.TW.IdsForSell.TryDequeue(out itemId);
            else
            {
                item.Sell();
                H.Log("Selling " + ObjectManager.GetItemInfo(item.Id).Name);
            }
        }
        private static void alwaysSalvageItem()
        {
            uint itemId = 0;
            Main.TW.IdsForAlwaysSalvage.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0 || Skandia.Me.GetCurrency(MoneyType.Fragments) == 30000)
                Main.TW.IdsForAlwaysSalvage.TryDequeue(out itemId);
            else
            {
                item.Salvage();
                H.Log("Salvaging " + ObjectManager.GetItemInfo(item.Id).Name);
            }
        }
        private static void salvageItem()
        {
            uint itemId = 0;
            Main.TW.IdsForSalvage.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0 || Skandia.Me.GetCurrency(MoneyType.Fragments) == 30000)
                Main.TW.IdsForSalvage.TryDequeue(out itemId);
            else
            {
                item.Salvage();
                H.Log("Salvaging " + ObjectManager.GetItemInfo(item.Id).Name);
            }
        }
        #endregion
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
