using Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedInventoryManager.Modules
{
    public static class Warehouse
    {
        private static bool IsInitialized = false;
        public static void Call()
        {
            // Checks to NOT use the Warehouse functionality
            if (!Main.settings.StoreItemsInWarehouseWhenWindowOpen ||
                (Main.settings.StopWarehouseIfFirstSlotIsFull && Skandia.Me.Inventory[0].Id != 0) ||
                (Main.settings.StopWarehouseIfLastSlotIsFull && Skandia.Me.Inventory[Skandia.Me.Inventory.Count - 1].Id != 0))
                return;
            if (Main.TW.IdsForWarehouse.Count != 0)
                storeItemInWarehouse();
            else if (Main.TW.IdsForSharedWarehouse.Count != 0)
                storeItemInSharedWarehouse();
            else if (Main.TW.IdsForRefiningWarehouse.Count != 0)
                storeItemInRefiningWarehouse();
            else if (Main.TW.IdsForSecretStoneWarehouse.Count != 0)
                storeItemInSecretStoneWarehouse();
            else if (Main.TW.IdsForCostumeWarehouse.Count != 0)
                storeItemInCostumeWarehouse();
            else if (Main.TW.IdsForSpecialWarehouse.Count != 0)
                storeItemInSpecialWarehouse();
            else if (!IsInitialized)
            {
                Main.TW.CheckInventoryForChange();
                if (Main.TW.HasInventoryChanged && !Main.TW.IsWorking)
                {
                    new Task(() => Main.TW.RefreshHashSets()).Start();
                    IsInitialized = true;
                }
            }
            else if (!Main.TW.IsWorking)
            {
                H.Log("Finished storing in Warehouse");
                IsInitialized = false;
            }
        }
        #region Store methods
        private static void storeItemInWarehouse()
        {
            uint itemId = 0;
            Main.TW.IdsForWarehouse.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0)
                Main.TW.IdsForWarehouse.TryDequeue(out itemId);
            else
            {
                if (Skandia.Me.WarehouseTotalEmptySlots != 0)
                {
                    item.Move(InventoryType.Warehouse, -1, -1);
                    H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                }
                else
                {
                    var nonStackedItemInWh = Skandia.Me.Warehouse.FirstOrDefault(x => x.Id == itemId && x.AmountMax != x.Amount);
                    if (nonStackedItemInWh != null && Main.settings.TryToStoreItemsWhenWarehouseFull)
                    {
                        item.Move(InventoryType.Warehouse, (short)nonStackedItemInWh.BagType, (short)nonStackedItemInWh.InventoryIndex);
                        H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                    }
                    else
                        Main.TW.IdsForWarehouse.TryDequeue(out itemId);
                }
            }
        }
        private static void storeItemInSharedWarehouse()
        {
            uint itemId = 0;
            Main.TW.IdsForSharedWarehouse.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0)
                Main.TW.IdsForSharedWarehouse.TryDequeue(out itemId);
            else
            {
                if (Skandia.Me.SharedWarehouseTotalEmptySlots != 0)
                {
                    item.Move(InventoryType.SharedWarehouse, (short)SharedWarehouseBagType.DefaultBag, -1);
                    H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                }
                else
                {
                    var nonStackedItemInWh = Skandia.Me.SharedWarehouse.FirstOrDefault(x => x.Id == itemId && x.AmountMax != x.Amount);
                    if (nonStackedItemInWh != null && Main.settings.TryToStoreItemsWhenWarehouseFull)
                    {
                        item.Move(InventoryType.SharedWarehouse, (short)nonStackedItemInWh.BagType, (short)nonStackedItemInWh.InventoryIndex);
                        H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                    }
                    else
                        Main.TW.IdsForSharedWarehouse.TryDequeue(out itemId);
                }
            }
        }
        private static void storeItemInRefiningWarehouse()
        {
            uint itemId = 0;
            Main.TW.IdsForRefiningWarehouse.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0)
                Main.TW.IdsForRefiningWarehouse.TryDequeue(out itemId);
            else
            {
                if (Skandia.Me.RefiningWarehouseTotalEmptySlots != 0)
                {
                    item.Move(InventoryType.SharedWarehouse, (short)RefiningWarehouseBagType.RefiningDefaultBag, -1);
                    H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                }
                else
                {
                    var nonStackedItemInWh = Skandia.Me.RefiningWarehouse.FirstOrDefault(x => x.Id == itemId && x.AmountMax != x.Amount);
                    if (nonStackedItemInWh != null && Main.settings.TryToStoreItemsWhenWarehouseFull)
                    {
                        item.Move(InventoryType.SharedWarehouse, (short)nonStackedItemInWh.BagType, (short)nonStackedItemInWh.InventoryIndex);
                        H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                    }
                    else
                        Main.TW.IdsForRefiningWarehouse.TryDequeue(out itemId);
                }
            }
        }
        private static void storeItemInSecretStoneWarehouse()
        {
            uint itemId = 0;
            Main.TW.IdsForSecretStoneWarehouse.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0)
                Main.TW.IdsForSecretStoneWarehouse.TryDequeue(out itemId);
            else
            {
                if (Skandia.Me.SecretStoneWarehouseTotalEmptySlots != 0)
                {
                    item.Move(InventoryType.SharedWarehouse, (short)SecretStoneWarehouseBagType.SecretStoneDefaultBag, -1);
                    H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                }
                else
                {
                    var nonStackedItemInWh = Skandia.Me.SecretStoneWarehouse.FirstOrDefault(x => x.Id == itemId && x.AmountMax != x.Amount);
                    if (nonStackedItemInWh != null && Main.settings.TryToStoreItemsWhenWarehouseFull)
                    {
                        item.Move(InventoryType.SharedWarehouse, (short)nonStackedItemInWh.BagType, (short)nonStackedItemInWh.InventoryIndex);
                        H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                    }
                    else
                        Main.TW.IdsForSecretStoneWarehouse.TryDequeue(out itemId);
                }
            }
        }
        private static void storeItemInCostumeWarehouse()
        {
            uint itemId = 0;
            Main.TW.IdsForCostumeWarehouse.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0)
                Main.TW.IdsForCostumeWarehouse.TryDequeue(out itemId);
            else
            {
                if (Skandia.Me.CostumeWarehouseTotalEmptySlots != 0)
                {
                    item.Move(InventoryType.SharedWarehouse, (short)CostumeWarehouseBagType.CostumeDefaultBag, -1);
                    H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                }
                else
                {
                    var nonStackedItemInWh = Skandia.Me.CostumeWarehouse.FirstOrDefault(x => x.Id == itemId && x.AmountMax != x.Amount);
                    if (nonStackedItemInWh != null && Main.settings.TryToStoreItemsWhenWarehouseFull)
                    {
                        item.Move(InventoryType.SharedWarehouse, (short)nonStackedItemInWh.BagType, (short)nonStackedItemInWh.InventoryIndex);
                        H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                    }
                    else
                        Main.TW.IdsForCostumeWarehouse.TryDequeue(out itemId);
                }
            }
        }
        private static void storeItemInSpecialWarehouse()
        {
            uint itemId = 0;
            Main.TW.IdsForSpecialWarehouse.TryPeek(out itemId);
            var item = selectSafeItem(itemId);
            if (item == null || item.Id == 0)
                Main.TW.IdsForSpecialWarehouse.TryDequeue(out itemId);
            else
            {
                if (Skandia.Me.SpecialWarehouseTotalEmptySlots != 0)
                {
                    item.Move(InventoryType.SharedWarehouse, (short)SpecialWarehouseBagType.SpecialDefaultBag, -1);
                    H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                }
                else
                {
                    var nonStackedItemInWh = Skandia.Me.SpecialWarehouse.FirstOrDefault(x => x.Id == itemId && x.AmountMax != x.Amount);
                    if (nonStackedItemInWh != null && Main.settings.TryToStoreItemsWhenWarehouseFull)
                    {
                        item.Move(InventoryType.SharedWarehouse, (short)nonStackedItemInWh.BagType, (short)nonStackedItemInWh.InventoryIndex);
                        H.Log("Storing " + ObjectManager.GetItemInfo(itemId).Name);
                    }
                    else
                        Main.TW.IdsForSpecialWarehouse.TryDequeue(out itemId);
                }
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
