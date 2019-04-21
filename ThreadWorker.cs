using Plugins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedInventoryManager
{
    public class ThreadWorker
    {
        #region Vars
        public bool IsWorking = false;
        private static int InventorySlots = 0;
        public bool HasInventoryChanged = false;
        // Lists from Settings/Warehouse
        private HashSet<uint> KeepIds = new HashSet<uint>();
        //private HashSet<uint> CustomIds = new HashSet<uint>();
        private HashSet<uint> UseIds = new HashSet<uint>();
        private HashSet<uint> AlwaysUseIds = new HashSet<uint>();
        private HashSet<uint> SellIds = new HashSet<uint>();
        private HashSet<uint> DiscardIds = new HashSet<uint>();
        private HashSet<uint> AlwaysDiscardIds = new HashSet<uint>();
        private HashSet<uint> SalvageIds = new HashSet<uint>();
        private HashSet<uint> AlwaysSalvageIds = new HashSet<uint>();
        private HashSet<uint> WarehouseIds = new HashSet<uint>();
        private HashSet<uint> SharedWarehouseIds = new HashSet<uint>();
        private HashSet<uint> RefiningWarehouseIds = new HashSet<uint>();
        private HashSet<uint> SecretStoneWarehouseIds = new HashSet<uint>();
        private HashSet<uint> CostumeWarehouseIds = new HashSet<uint>();
        private HashSet<uint> SpecialWarehouseIds = new HashSet<uint>();
        // Lists from inventory
        public ConcurrentQueue<uint> IdsForCustom = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForUse = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForAlwaysUse = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForSell = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForDiscard = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForAlwaysDiscard = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForSalvage = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForAlwaysSalvage = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForWarehouse = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForSharedWarehouse = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForRefiningWarehouse = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForSecretStoneWarehouse = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForCostumeWarehouse = new ConcurrentQueue<uint>();
        public ConcurrentQueue<uint> IdsForSpecialWarehouse = new ConcurrentQueue<uint>();
        #endregion
        public ThreadWorker() { }
        public void CheckInventoryForChange()//Compares the inventory slots with before, modifies HasInvenotryChanged
        {
            IsWorking = true;
            if (Skandia.Me.InventoryTotalUsedSlots != InventorySlots || Skandia.Me.InventoryTotalEmptySlots == 0) // EXPERIMENTAL
            {
                InventorySlots = Skandia.Me.InventoryTotalUsedSlots;
                HasInventoryChanged = true;
            }
            else
                HasInventoryChanged = false;
            IsWorking = false;
        }
        public void RefreshHashSets(bool alsoScanInventory = true)// Used to fill in the Hash sets with Ids from Settings and Warehouse
        {
            IsWorking = true;
            H.Log("[TW]Starting Hash scan");
            ScanSettingsItems();
            if (Main.settings.AutoStackWarehouse)
            {
                ScanWarehouse();
                ScanSharedWarehouse();
                ScanRefiningWarehouse();
                ScanSecretStoneWarehouse();
                ScanSpecialWarehouse();
                ScanCostumeWarehouse();
            }
            H.Log("[TW]Finished Hash scan");
            IsWorking = false;
            if (alsoScanInventory)
                ScanInventory();
        }
        public void SearchItemsInDb()//Search for Left list, criterias are taken from the SettingsUI
        {
            IsWorking = true;
            // Search in both lists if enabled
            if (Main.settingsUI == null)
                return;

            if (Main.settings.SearchInBothItemLists)
                SearchItemsInProfile();

            // Get all items
            List<ItemCache> list = new List<ItemCache>();
            if (Main.settingsUI._SearchIn == 0)
            {
                for (ushort i = 0; i < ushort.MaxValue; i++)
                {
                    var item = ObjectManager.GetItemInfo(i);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
            }
            else if (Main.settingsUI._SearchIn == 1)
            {
                foreach (var i in Skandia.Me.Inventory)
                {
                    if (i.Id == 0)
                        continue;
                    var item = ObjectManager.GetItemInfo(i.Id);
                    list.Add(item);
                }
            }

            // Filters
            if (Main.settingsUI._FilterCategory)
            {
                list = list.Where(x => x.Category == Main.settingsUI._SearchCategory).ToList();
            }
            if (Main.settingsUI._FilterMoney)
            {
                list = list.Where(x => x.Currency == Main.settingsUI._SearchMoney).ToList();
            }
            if (Main.settingsUI._FilterQuality)
            {
                list = list.Where(x => x.Quality == Main.settingsUI._SearchQuality).ToList();
            }
            if (Main.settingsUI._SearchNameOrId != "")
            {
                if (Regex.IsMatch(Main.settingsUI._SearchNameOrId, "^[0-9]+$"))
                {
                    int id = 0;
                    int.TryParse(Main.settingsUI._SearchNameOrId, out id);
                    list = list.Where(x => x.Id.ToString().Contains(id.ToString())).ToList();
                }
                else
                {
                    list = list.Where(x => x.Name.ToLower().Contains(Main.settingsUI._SearchNameOrId)).ToList();
                }
            }
            if (Main.settingsUI._SearchMinLevel != 0 || Main.settingsUI._SearchMaxLevel != 99)
            {
                list = list.Where(x => Main.settingsUI._SearchMinLevel < x.LevelRequired && x.LevelRequired < Main.settingsUI._SearchMaxLevel).ToList();
            }
            IsWorking = true;
            // Add to the UI
            GenerateItemsForLeftListView(list);
        }
        public void SearchItemsInProfile()//Search for Right list, criterias are taken from the SettingsUI
        {
            IsWorking = true;
            if (Main.settingsUI == null)
                return;

            var list = Main.settings.Items.Where(x => x.HasFunction(Main.settingsUI._CurrentItemFunction));
            //Filters
            if (Main.settings.SearchInBothItemLists)
            {
                if (Main.settingsUI._FilterCategory)
                {
                    list = list.Where(x => ObjectManager.GetItemInfo(x.Id).Category == Main.settingsUI._SearchCategory);
                }
                if (Main.settingsUI._FilterMoney)
                {
                    list = list.Where(x => ObjectManager.GetItemInfo(x.Id).Currency == Main.settingsUI._SearchMoney).ToList();
                }
                if (Main.settingsUI._FilterQuality)
                {
                    list = list.Where(x => ObjectManager.GetItemInfo(x.Id).Quality == Main.settingsUI._SearchQuality).ToList();
                }
                if (Main.settingsUI._SearchNameOrId != "")
                {
                    if (Regex.IsMatch(Main.settingsUI._SearchNameOrId, "^[0-9]+$"))
                    {
                        int id = 0;
                        int.TryParse(Main.settingsUI._SearchNameOrId, out id);
                        list = list.Where(x => x.Id.ToString().Contains(id.ToString())).ToList();
                    }
                    else
                    {
                        list = list.Where(x => ObjectManager.GetItemInfo(x.Id).Name.ToLower().Contains(Main.settingsUI._SearchNameOrId)).ToList();
                    }
                }
            }
            IsWorking = false;
            // Add to the UI
            GenerateItemsForRightListView(list.ToList());
        }
        private void ScanInventory()//Adds Ids of items to be used later on in the ConcurrentQueues
        {
            IsWorking = true;
            H.Log("[TW]Starting Inventory scan");
            var inventory = GetSafeInventoryIds();
            foreach (var itemId in inventory)
            {
                // Ignore Keep Ids
                if (WarehouseIds.Contains(itemId) && !IdsForWarehouse.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForWarehouse.Enqueue(itemId);
                if (SharedWarehouseIds.Contains(itemId) && !IdsForSharedWarehouse.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForSharedWarehouse.Enqueue(itemId);
                if (RefiningWarehouseIds.Contains(itemId) && !IdsForRefiningWarehouse.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForRefiningWarehouse.Enqueue(itemId);
                if (SecretStoneWarehouseIds.Contains(itemId) && !IdsForSecretStoneWarehouse.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForSecretStoneWarehouse.Enqueue(itemId);
                if (CostumeWarehouseIds.Contains(itemId) && !IdsForCostumeWarehouse.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForCostumeWarehouse.Enqueue(itemId);
                if (SpecialWarehouseIds.Contains(itemId) && !IdsForSpecialWarehouse.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForSpecialWarehouse.Enqueue(itemId);
                if (UseIds.Contains(itemId) && !IdsForUse.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForUse.Enqueue(itemId);
                if (AlwaysUseIds.Contains(itemId) && !IdsForAlwaysUse.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForAlwaysUse.Enqueue(itemId);
                if ((SellIds.Contains(itemId) || ShouldItemBeSold(itemId)) && !IdsForSell.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForSell.Enqueue(itemId);
                if (DiscardIds.Contains(itemId) && !IdsForDiscard.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForDiscard.Enqueue(itemId);
                if (AlwaysDiscardIds.Contains(itemId) && !IdsForAlwaysDiscard.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForAlwaysDiscard.Enqueue(itemId);
                if ((SalvageIds.Contains(itemId) || ShouldItemBeSalvaged(itemId)) && !IdsForSalvage.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForSalvage.Enqueue(itemId);
                if ((AlwaysSalvageIds.Contains(itemId) || ShouldItemBeSalvaged(itemId)) && !IdsForAlwaysSalvage.Contains(itemId) && !KeepIds.Contains(itemId))
                    IdsForAlwaysSalvage.Enqueue(itemId);
            }
            H.Log("[TW]Finished Inventory scan");
            IsWorking = false;
        }
        private bool ShouldItemBeSalvaged(uint itemId)//For special salvaging options, Look Seller tab on SettingsUI
        {
            var item = ObjectManager.GetItemInfo(itemId);
            bool checkQuality = false;
            bool checkType = false;
            bool checkLevel = true;
            bool checkFragments = true;
            bool checkCategory = false;
            //if (Main.settings.SellGreenEquipment)
            //    checkQuality = item.Quality == ItemQuality.Green;
            switch (item.Quality)
            {
                case ItemQuality.White:
                    checkQuality = Main.settings.SalvageWhiteEquipment;
                    break;
                case ItemQuality.Green:
                    checkQuality = Main.settings.SalvageGreenEquipment;
                    break;
                case ItemQuality.Orange:
                    checkQuality = Main.settings.SalvageOrangeEquipment;
                    if (Main.settings.SellSalvageOrangeEquipmentBelowLevel != 0 && Main.settings.SellSalvageOrangeEquipmentBelowLevel != 99)
                        checkLevel = item.LevelRequired < Main.settings.SellSalvageOrangeEquipmentBelowLevel;
                    break;
                case ItemQuality.Blue:
                    checkQuality = Main.settings.SalvageBlueEquipment;
                    break;
                case ItemQuality.Yellow:
                    checkQuality = Main.settings.SalvageYellowEquipment;
                    if (Main.settings.SellSalvageYellowEquipmentBelowLevel != 0 && Main.settings.SellSalvageYellowEquipmentBelowLevel != 99)
                        checkLevel = item.LevelRequired < Main.settings.SellSalvageYellowEquipmentBelowLevel;
                    break;
                case ItemQuality.Purple:
                    checkQuality = Main.settings.SalvagePurpleEquipment;
                    break;
                default:
                    break;
            }
            checkType = item.BodyPart != ItemDataBodyPart.None;

            if (Main.settings.DontSalvageWhenFragsExceed != 0 && Main.settings.DontSalvageWhenFragsExceed != 30000)
            {
                checkFragments = Skandia.Me.GetCurrency(MoneyType.Fragments) < Main.settings.DontSalvageWhenFragsExceed;
                if (!checkFragments && Main.settings.DiscardEquipmentWhenCantBeSalvaged)
                    AlwaysDiscardIds.Add(itemId);
                if (!checkFragments && Main.settings.SellEquipmentWhenCantBeSalvaged)
                    SellIds.Add(itemId);
            }

            if (item.Category == ItemDataCategory.Trophy || item.Category == ItemDataCategory.Ring || item.Category == ItemDataCategory.Necklace || item.Category == ItemDataCategory.Lance ||
                item.Category == ItemDataCategory.Shuriken || item.Category == ItemDataCategory.LongSword || item.Category == ItemDataCategory.Scyte || item.Category == ItemDataCategory.Katana ||
                item.Category == ItemDataCategory.Bow || item.Category == ItemDataCategory.Claws || item.Category == ItemDataCategory.Harp || item.Category == ItemDataCategory.Book ||
                item.Category == ItemDataCategory.Gun || item.Category == ItemDataCategory.Pistols || item.Category == ItemDataCategory.Axe || item.Category == ItemDataCategory.Shield ||
                item.Category == ItemDataCategory.Blades || item.Category == ItemDataCategory.Cloak || item.Category == ItemDataCategory.Boots || item.Category == ItemDataCategory.Gloves ||
                item.Category == ItemDataCategory.Belt || item.Category == ItemDataCategory.Armor || item.Category == ItemDataCategory.Hat)
            {
                checkCategory = true;
            }

            return checkQuality && checkType && checkLevel && checkFragments && checkCategory;
        }
        private bool ShouldItemBeSold(uint itemId)//For special selling options, Look Seller tab on SettingsUI
        {
            var item = ObjectManager.GetItemInfo(itemId);
            bool checkQuality = false;
            bool checkType = false;
            bool checkLevel = true;
            switch (item.Quality)
            {
                case ItemQuality.White:
                    checkQuality = Main.settings.SellWhiteEquipment;
                    break;
                case ItemQuality.Grey:
                    checkQuality = Main.settings.SellGreyItems;
                    break;
                case ItemQuality.Green:
                    checkQuality = Main.settings.SellGreenEquipment;
                    break;
                case ItemQuality.Orange:
                    checkQuality = Main.settings.SalvageOrangeEquipment;
                    if (Main.settings.SellSalvageOrangeEquipmentBelowLevel != 0 && Main.settings.SellSalvageOrangeEquipmentBelowLevel != 99)
                        checkLevel = item.LevelRequired < Main.settings.SellSalvageOrangeEquipmentBelowLevel;
                    break;
                case ItemQuality.Blue:
                    checkQuality = Main.settings.SellBlueEquipment;
                    break;
                case ItemQuality.Yellow:
                    checkQuality = Main.settings.SalvageYellowEquipment;
                    if (Main.settings.SellSalvageYellowEquipmentBelowLevel != 0 && Main.settings.SellSalvageYellowEquipmentBelowLevel != 99)
                        checkLevel = item.LevelRequired < Main.settings.SellSalvageYellowEquipmentBelowLevel;
                    break;
                default:
                    break;
            }
            checkType = item.BodyPart != ItemDataBodyPart.None;
            if (Main.settings.SellGreyItems)
                checkType = true;
            return checkQuality && checkType;
        }
        private void GenerateItemsForLeftListView(List<ItemCache> _list)//Generate Items to add in Left list
        {
            var listOfItemsToAdd = new List<ListViewItem>();
            foreach (var item in _list)
            {
                string[] row = { item.Name, item.Id.ToString() };
                var _listItem = new ListViewItem(row);
                switch (item.Quality)
                {
                    case ItemQuality.White:
                    _listItem.BackColor = Color.White;
                    break;
                    case ItemQuality.Grey:
                    _listItem.BackColor = Color.LightGray;
                    break;
                    case ItemQuality.Green:
                    _listItem.BackColor = Color.LightGreen;
                    break;
                    case ItemQuality.Orange:
                    _listItem.BackColor = Color.Orange;
                    break;
                    case ItemQuality.Blue:
                    _listItem.BackColor = Color.LightBlue;
                    break;
                    case ItemQuality.Purple:
                    _listItem.BackColor = Color.MediumPurple;
                    break;
                    case ItemQuality.Yellow:
                    _listItem.BackColor = Color.Yellow;
                    break;
                    case ItemQuality.Undefined:
                    _listItem.BackColor = Color.Red;
                    break;
                    default:
                    _listItem.BackColor = Color.Red;
                    break;
                }
                listOfItemsToAdd.Add(_listItem);
            }
            UpdateLeftList(listOfItemsToAdd);
        }
        private void UpdateLeftList(List<ListViewItem> _listOfItems)//Add items to Left list
        {
            var arrayOfItems = _listOfItems.ToArray();
            if (Main.settingsUI.listView1.InvokeRequired)
                Main.settingsUI.listView1.Invoke((MethodInvoker)delegate
                {
                    Main.settingsUI.listView1.BeginUpdate();
                    Main.settingsUI.listView1.Items.Clear();
                    Main.settingsUI.listView1.Items.AddRange(arrayOfItems);
                    Main.settingsUI.listView1.EndUpdate();
                });
            else
            {
                Main.settingsUI.listView1.BeginUpdate();
                Main.settingsUI.listView1.Items.Clear();
                Main.settingsUI.listView1.Items.AddRange(arrayOfItems);
                Main.settingsUI.listView1.EndUpdate();
            }
        }
        private void GenerateItemsForRightListView(List<Item> _list)//Generate Items to add in Right list
        {
            IsWorking = true;
            var listOfItemsToAdd = new List<ListViewItem>();
            foreach (var item in _list)
            {
                string[] row = { ObjectManager.GetItemInfo(item.Id).Name, item.Id.ToString() };
                var _listItem = new ListViewItem(row);
                switch (ObjectManager.GetItemInfo(item.Id).Quality)
                {
                    case ItemQuality.White:
                    _listItem.BackColor = Color.White;
                    break;
                    case ItemQuality.Grey:
                    _listItem.BackColor = Color.LightGray;
                    break;
                    case ItemQuality.Green:
                    _listItem.BackColor = Color.LightGreen;
                    break;
                    case ItemQuality.Orange:
                    _listItem.BackColor = Color.Orange;
                    break;
                    case ItemQuality.Blue:
                    _listItem.BackColor = Color.LightBlue;
                    break;
                    case ItemQuality.Purple:
                    _listItem.BackColor = Color.MediumPurple;
                    break;
                    case ItemQuality.Yellow:
                    _listItem.BackColor = Color.Yellow;
                    break;
                    case ItemQuality.Undefined:
                    _listItem.BackColor = Color.Red;
                    break;
                    default:
                    _listItem.BackColor = Color.Red;
                    break;
                }
                listOfItemsToAdd.Add(_listItem);
            }
            IsWorking = false;
            UpdateRightList(listOfItemsToAdd);
        }
        private void UpdateRightList(List<ListViewItem> _listOfItems)//Add items to Right list
        {
            IsWorking = true;
            var arrayOfItems = _listOfItems.ToArray();
            Main.settingsUI.listView2.BeginUpdate();
            Main.settingsUI.listView2.Items.Clear();
            Main.settingsUI.listView2.Items.AddRange(arrayOfItems);
            Main.settingsUI.listView2.EndUpdate();
            IsWorking = false;
        }
        private void ScanSettingsItems()//Populates the HashSets <uint> with the Ids from the Settings
        {
            KeepIds.Clear();
            //CustomIds.Clear();
            UseIds.Clear();
            AlwaysUseIds.Clear();
            SellIds.Clear();
            DiscardIds.Clear();
            AlwaysDiscardIds.Clear();
            WarehouseIds.Clear();
            SharedWarehouseIds.Clear();
            RefiningWarehouseIds.Clear();
            SecretStoneWarehouseIds.Clear();
            CostumeWarehouseIds.Clear();
            SpecialWarehouseIds.Clear();
            KeepIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.Keep)).Select(x => x.Id));
            //CustomIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.Custom)).Select(x => x.Id));
            UseIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.Use)).Select(x => x.Id));
            AlwaysUseIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.AlwaysUse)).Select(x => x.Id));
            SellIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.Sell)).Select(x => x.Id));
            DiscardIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.Discard)).Select(x => x.Id));
            AlwaysDiscardIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.AlwaysDiscard)).Select(x => x.Id));
            SalvageIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.Salvage)).Select(x => x.Id));
            AlwaysSalvageIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.AlwaysSalvage)).Select(x => x.Id));
            WarehouseIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.Warehouse)).Select(x => x.Id));
            SharedWarehouseIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.SharedWarehouse)).Select(x => x.Id));
            RefiningWarehouseIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.RefiningWarehouse)).Select(x => x.Id));
            SecretStoneWarehouseIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.SecretStoneWarehouse)).Select(x => x.Id));
            CostumeWarehouseIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.CostumeWarehouse)).Select(x => x.Id));
            SpecialWarehouseIds.UnionWith(Main.settings.Items.Where(x => x.HasFunction(ItemFunction.SpecialWarehouse)).Select(x => x.Id));
        }
        private void ScanWarehouse()//Populates the WarehouseIds from the player Warehouse
        {
            if (Skandia.IsInGame)
                foreach (var item in Skandia.Me.Warehouse)
                    WarehouseIds.Add(item.Id);
        }
        private void ScanSharedWarehouse()//Populates the SharedWarehouseIds from the player Warehouse
        {
            if (Skandia.IsInGame)
                foreach (var item in Skandia.Me.SharedWarehouse)
                    SharedWarehouseIds.Add(item.Id);
        }
        private void ScanRefiningWarehouse()//Populates the RefiningWarehouseIdsfrom the player Warehouse
        {
            if (Skandia.IsInGame)
                foreach (var item in Skandia.Me.RefiningWarehouse)
                    RefiningWarehouseIds.Add(item.Id);
        }
        private void ScanSecretStoneWarehouse()//Populates the SecretStoneWarehouseIds from the player Warehouse
        {
            if (Skandia.IsInGame)
                foreach (var item in Skandia.Me.SecretStoneWarehouse)
                    SecretStoneWarehouseIds.Add(item.Id);
        }
        private void ScanSpecialWarehouse()//Populates the SpecialWarehouseIds from the player Warehouse
        {
            if (Skandia.IsInGame)
                foreach (var item in Skandia.Me.SpecialWarehouse)
                    SpecialWarehouseIds.Add(item.Id);
        }
        private void ScanCostumeWarehouse()//Populates the CostumeWarehouseIds from the player Warehouse
        {
            if (Skandia.IsInGame)
                foreach (var item in Skandia.Me.CostumeWarehouse)
                    CostumeWarehouseIds.Add(item.Id);
        }
        private List<uint> GetSafeInventoryIds()//Avoid getting items from the inventory which are protected
        {
            if (Main.settings.InventoryToBeUnaffected == 1)
                return Skandia.Me.Inventory.Where(x => x.Id != 0 && x.InventoryIndex >= Main.settings.InventorySlotsToBeUnaffected).Select(x => x.Id).Distinct().ToList();
            else if (Main.settings.InventoryToBeUnaffected == 2)
                return Skandia.Me.Inventory.Where(x => x.Id != 0 && x.InventoryIndex <= (Skandia.Me.InventoryTotalEmptySlots + Skandia.Me.InventoryTotalUsedSlots) -
                Main.settings.InventorySlotsToBeUnaffected).Select(x => x.Id).Distinct().ToList();
            else
                return Skandia.Me.Inventory.Where(x => x.Id != 0).Select(x => x.Id).Distinct().ToList();
        }
    }
}
