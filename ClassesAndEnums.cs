using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AdvancedInventoryManager
{
    public class Item
    {
        [XmlAttribute]
        public uint Id { get; set; }
        public List<ItemFunction> Functions { get; set; }
        public int? CountToKeepInInventory { get; set; }
        public int? CountToKeepInWarehouse { get; set; }
        public uint? CheckBuff { get; set; }
        // TODO Add more
        public Item()
        {
            Functions = new List<ItemFunction>();
        }
        public Item(uint id)
        {
            Id = id;
            Functions = new List<ItemFunction>();
        }
        public bool HasFunction(ItemFunction func)
        {
            return Functions.Contains(func);
        }
        public void AddFunction(ItemFunction func)
        {
            if (!Functions.Contains(func))
                Functions.Add(func);
            Functions.Sort();
        }
        public void RemoveFunction(ItemFunction func)
        {
            Functions.Remove(func);
            Functions.Sort();
        }
        public bool ShouldSerializeCheckBuff()
        {
            return CheckBuff.HasValue;
        }
        public bool ShouldSerializeCountToKeepInWarehouse()
        {
            return CountToKeepInInventory.HasValue;
        }
        public bool ShouldSerializeCountToKeepInInventory()
        {
            return CountToKeepInInventory.HasValue;
        }
        public override bool Equals(object obj)
        {
            try { return Id.Equals((obj as Item).Id); }
            catch { return false; }
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
    public enum ItemFunction
    {
        Keep,
        Use,
        AlwaysUse,
        Sell,
        Discard,
        AlwaysDiscard,
        Salvage,
        AlwaysSalvage,
        Warehouse,
        SharedWarehouse,
        SecretStoneWarehouse,
        RefiningWarehouse,
        CostumeWarehouse,
        SpecialWarehouse,
    }
    public static class Vars
    {
        public static string[] ComboBox2Items = { "Keep", "Use", "Always Use", "Sell", "Discard", "Always Discard", "Salvage", "AlwaysSalvage", "Store in Warehouse", "Store in Shared Warehouse",
                "Store in Secret-Stone Warehouse", "Store in Refining Warehouse", "Store in Costume Warehouse", "Store in Special Warehouse" };
        public static string[] ComboBox6Items = { "Database", "Inventory" };
        public static string Information = @"AIM plugin help file

[Save Profile] - saves an XML file in Skandia/Profiles/Plugins/AIM/YourName.xml where YourName.xml is your character name. The file contains all your settings
[Force Refresh] - Forces inventory scan. Useful when you made some changes to your settings because the inventory scanner works based on free inventory slots, hence it won't trigger unless you open Warehouse/Vendor OR Force Refresh
[Information] - Brings this file up
[Inventory to be unaffected] - Allows you to have protected inventory slots starting from the top or from the bottom
[Slots to be unaffected] - Specifies how many slots from the top OR bottom should be unaffected by the plugin
[Search in both Item list] - Filter not only your inventory/full database list but also the items in your profile. Allows for easy checking if you have a particular item in your sell/discard/etc. lists.
[Use dynamic search] - Allows you to not use the Search button or press Enter but instead searches in the Item lists when you type something. WARNING: This may cause lag on slower computers, use at your own risk!
[Plugin timeout in milliseconds] - Specify how much often should the plugin do its thing. 500 is default and recommended value, for slower computers using 1000 should be good enough. If you want to go super fast and abuse the speed at which you can use/discaard/sell items to the max go for 50-100. This is NOT recommended!
[Sell when NPC window open] - This enables the seller. If this is not ticked the plugin is INACTIVE when the shop window is open and you cannot sell/discard/use/etc. items
[Sell everything to the appropriate shop] - EXPERIMENTAL! Allows you to sell appropriate currency items to the appropriate shops
[Store items in Warehouse when the window is open] - This enables the Storage functionality. If this is not ticked the plugin is INACTIVE when the warehouse window is open and you cannot store items
[Auto stack warehouse] - The plugin scans your warehouse items and your inventory. If item from your inventory is in your warehouse already it will try to put the item in the warehouse. This option is for the lazy people to configure their Warehouse lists
[Try to store items when no empty slots are available] - EXPERIMENTAL! The plugin will try to fill stacks in your warehouse when you have no empty slots. If you have 45 ST tokens in your warehouse and you have another stack in your inventory the plugin will try to fill the stack in your warehouse to 100
[Pause warehouse function if the first inventory slot is full] - If your first inventory slot is full. Warehouse functionality is paused. Allows you to not stopping the plugin when you want to get something from your warehouse.
[Pause warehouse function if the last inventory slot is full] - If your last inventory slot is full. Warehouse functionality is paused. Allows you to not stopping the plugin when you want to get something from your warehouse.
[Enable Always Use/Discard/Salvage] - If this is not ticked when you don't have Shop window/Warehouse window open the plugin is INACTIVE and the Always use/discard/salvage lists are ignored
[Use item in the AlwaysUse in combat too] - This option enables the Always use/discard/salvage functionality in combat too. Not suggested because it can cause lag when fighting
[Item list -> Top input field] - The search box where you type what you want to search for
[Item list -> Search button OR Enter] - Perform a search, aka filter items. Used when you don't use [Use dynamic search]
[Item list -> Search in] - Search in your Inventory or the whole Database for items
[Item list -> Level] - Specify between what level are the items you desire. Default is 0 - 99 (all items)
[Item list -> Quality] - Specify the quality of the item - green/yellow/etc.
[Item list -> Category] - Specify the item category - Weapons/Armors/etc.
[Item list -> Currency] - Specify the currency of the item you're looking for
[Item list -> Left list] - Where all the results are shown. Be it from your inventory or from the whole Database. See [Item list -> Search in]
[Item list -> Items to:] - These are your items in your profile. Here you can see and add/remove items to your lists to Keep/Sell/Etc. This list can be filtered too so you can easily see if you have a specific item in it by enabling [Search in both Item lists]
[Items -> Keep] - Items to be unaffected by the other categories. Items in Keep cannot be sold/discarded/salvaged/etc.
[Items -> Use] - Use items ONLY when shop window is open. Put here items with cast-time using. Like unidentified armors/World boss chests/etc.
[Items -> Always Use] - Use items when Shop window is open AND when it is not. Suggested use: put items here like Loyalty points/Dragon points/etc. to keep your inventory clean at all times
[Items -> Sell] - Items to be sold when Shop window is open
[Items -> Discard] - Discard items when Shop window is open. Items to be Discaarded ONLY when the Seller is active
[Items -> Always Discard] - Discard items when Shop window is open AND when it is not. Allows you to discard on the go - when you're in dungeons or in town so you can keep your inventory clean
[Items -> Salvage] - Salvage items when Shop window is open. Items to be salvaged ONLY when the Seller is active
[Items -> Always Salvage] - Salvage items when Shop window is open AND when it is not. Allows you to salvage on the go - when you're in dungeons or in town so you can keep your inventory clean
[Items -> Store in Warehouses] - Store items in a particular warehouse.
[Seller -> Sell white/blue/etc. equipment] - Allows you to easily select frequently sold items by their quaility without having to manually add all to the Sell list
[Seller -> Salvage green/blue/etc. equipment] - Allows you to easily select frequently salvaged items by their quality without having to manually add all to the Salvage/Always salvage list. Items selected here are added to the Always salvage list. Look [Items -> Always Salvage] for more info
[Seller -> Sell equipment if can't be salvaged] - Easy way to add equipment to the Sell list if your fragments are capped or exceed the specified amount in [Seller -> Don't salvage when fragments exceed]
[Seller -> Discaard equipment if can't be salvaged] - Easy way to add equipment to the Discard list if your fragments are capped or exceed the specified amount in [Seller -> Don't salvage when fragments exceed]
[Seller -> Don't salvage when fragments exceed] - When the value is 0 or 30000 this option is ignored. Allows you to specify max number of fragments after which to stop salvaging. Useful if you want to keep your fragments around 20-25k and want to have the rest of the space for items you want to manually salvage like yellow equipment
[Seller -> Sell/Salvage orange equipment below level] - When the value is 0 or 99 this option is ignored. Allows you to specify under what leve should orange equipments be salvaged. That way you can choose to salvage only orange equipment under level 70 for example for the tools they can give you
[Seller -> Sell/Salvage orange equipment below level] - When the value is 0 or 99 this option is ignored. Allows you to specify under what leve should yellow equipments be salvaged. That way you can choose to salvage only yellow equipment under level 70 for example for the tools they can give you
";
    }
    public class Data
    {
        public string InventoryFull { get; set; }
        public Data()
        {
            InventoryFull = "Not enough backpack slots.";
        }
    }
}
