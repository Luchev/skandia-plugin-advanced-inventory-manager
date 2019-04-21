using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedInventoryManager
{
    public class Settings
    {
        public int TimerMiliseconds { get; set; }
        public bool DynamicSearch { get; set; }
        public bool SearchInBothItemLists { get; set; }
        public bool StoreItemsInWarehouseWhenWindowOpen { get; set; }
        public bool SellItemsToVendorWhenWindowOpen { get; set; }
        public bool EnableAlwaysSalvageDiscardUse { get; set; }
        public bool AlwaysSalvageDiscardUseInCombat { get; set; }
        public bool AutoStackWarehouse { get; set; }
        public bool StopWarehouseIfFirstSlotIsFull { get; set; }
        public bool StopWarehouseIfLastSlotIsFull { get; set; }
        public int InventoryToBeUnaffected { get; set; }
        public int InventorySlotsToBeUnaffected { get; set; }
        public bool SellEverythingToAppropriateVendors { get; set; }
        public bool SellWhiteEquipment { get; set; }
        public bool SellBlueEquipment { get; set; }
        public bool SellGreenEquipment { get; set; }
        public bool SellOrangeEquipment { get; set; }
        public bool SellYellowEquipment { get; set; }
        public bool SalvagePurpleEquipment { get; set; }
        public bool SalvageWhiteEquipment { get; set; }
        public bool SalvageBlueEquipment { get; set; }
        public bool SalvageGreenEquipment { get; set; }
        public bool SalvageOrangeEquipment { get; set; }
        public bool SalvageYellowEquipment { get; set; }
        public int SellSalvageOrangeEquipmentBelowLevel { get; set; }
        public int SellSalvageYellowEquipmentBelowLevel { get; set; }
        public int DontSalvageWhenFragsExceed { get; set; }
        public bool SellEquipmentWhenCantBeSalvaged { get; set; }
        public bool DiscardEquipmentWhenCantBeSalvaged { get; set; }
        public bool SellGreyItems { get; set; }
        public bool TryToStoreItemsWhenWarehouseFull { get; set; }
        public List<Item> Items { get; set; }
        public Settings()
        {
            TimerMiliseconds = 500;
            SellItemsToVendorWhenWindowOpen = false;
            EnableAlwaysSalvageDiscardUse = false;
            StoreItemsInWarehouseWhenWindowOpen = false;
            AutoStackWarehouse = false;
            InventoryToBeUnaffected = 0;
            InventorySlotsToBeUnaffected = 0;
            DynamicSearch = false;
            SearchInBothItemLists = false;
            Items = new List<Item>();
            StopWarehouseIfFirstSlotIsFull = false;
            StopWarehouseIfLastSlotIsFull = false;
            AlwaysSalvageDiscardUseInCombat = false;
            SellWhiteEquipment = false;
            SellBlueEquipment = false;
            SellGreenEquipment = false;
            SellOrangeEquipment = false;
            SellYellowEquipment = false;
            SalvagePurpleEquipment = false;
            SalvageBlueEquipment = false;
            SalvageGreenEquipment = false;
            SalvageOrangeEquipment = false;
            SalvageWhiteEquipment = false;
            SalvageYellowEquipment = false;
            SellSalvageYellowEquipmentBelowLevel = 0;
            SellSalvageOrangeEquipmentBelowLevel = 0;
            DontSalvageWhenFragsExceed = 0;
            SellGreyItems = false;
            SellEverythingToAppropriateVendors = false;
            SellEquipmentWhenCantBeSalvaged = false;
            DiscardEquipmentWhenCantBeSalvaged = false;
            TryToStoreItemsWhenWarehouseFull = false;
        }
    }
}
