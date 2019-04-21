using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Plugins;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace AdvancedInventoryManager
{
    public partial class SettingsUI : Form
    {
        #region Vars
        public ItemQuality _SearchQuality = ItemQuality.White;
        public ItemDataCategory _SearchCategory = ItemDataCategory.Armor;
        public MoneyType _SearchMoney = MoneyType.Gold;
        public bool _FilterQuality = false;
        public bool _FilterCategory = false;
        public bool _FilterMoney = false;
        public string _SearchNameOrId = "";
        public ushort _SearchMinLevel = 0;
        public ushort _SearchMaxLevel = 99;
        public ItemFunction _CurrentItemFunction = ItemFunction.Keep;
        public int _SearchIn = 0;
        #endregion
        public SettingsUI()
        {
            InitializeComponent();
            ResizeUI();

            textBox2.KeyDown += TextBox2_KeyDown;
            DoubleBuffered = true;

            comboBox4.Items.Add("All Qualities");
            foreach (var item in Enum.GetValues(typeof(ItemQuality)))
            {
                comboBox4.Items.Add(item);
            }

            comboBox3.Items.Add("All Categories");
            foreach (var item in Enum.GetValues(typeof(ItemDataCategory)))
            {
                comboBox3.Items.Add(item);
            }

            comboBox5.Items.Add("All Currencies");
            foreach (var item in Enum.GetValues(typeof(MoneyType)))
            {
                comboBox5.Items.Add(item);
            }

            comboBox6.Items.AddRange(Vars.ComboBox6Items);
            comboBox6.SelectedIndex = 0;
            comboBox2.Items.AddRange(Vars.ComboBox2Items);
            comboBox2.SelectedIndex = 0;
        }
        #region UI Control
        private void LeftListControlChanged()//Auto search if enabled in settings
        {
            if (Main.settings.DynamicSearch)
                new Task(() => Main.TW.SearchItemsInDb()).Start();
        }
        private void RightListControlChanged()//Search in Right List
        {
            new Task(() => Main.TW.SearchItemsInProfile()).Start();
        }
        private void _SizeChanged(object sender, EventArgs e)//Call Resize
        {
            ResizeUI();
        }
        private void ResizeUI()//Update position and size of controls
        {
            try
            {
                listView1.Width = tabControl1.Width / 2 - 30;
                listView1.Location = new Point(5, 125);
                listView1.Columns[0].Width = (int)(listView1.ClientRectangle.Width * 0.75);
                listView1.Columns[1].Width = (int)(listView1.ClientRectangle.Width * 0.25);
                listView1.Height = tabControl1.Height - listView1.Location.Y - 30;

                listView2.Width = tabControl1.Width / 2 - 30;
                listView2.Location = new Point(listView1.Width + button4.Width + 15, 125);
                listView2.Height = tabControl1.Height - listView2.Location.Y - 30;
                listView2.Columns[0].Width = (int)(listView2.ClientRectangle.Width * 0.75);
                listView2.Columns[1].Width = (int)(listView2.ClientRectangle.Width * 0.25);

                button3.Location = new Point(listView1.Width + 10, 125);
                button4.Location = new Point(listView1.Width + 10, 155);
                comboBox2.Location = new Point(listView2.Location.X + 2, listView2.Location.Y - 30);
                label4.Location = new Point(listView1.Location.X + 2, listView1.Location.Y - 25);
                label3.Location = new Point(comboBox2.Location.X - 55, comboBox2.Location.Y + 2);
            }
            catch (Exception)
            {
                H.Log("[ERROR]Resizing the UI failed");
            }
        }
        internal void RefreshUI()//Update fields and controls according to the Settings
        {
            checkBox1.Checked = Main.settings.AutoStackWarehouse;
            checkBox2.Checked = Main.settings.DynamicSearch;
            checkBox3.Checked = Main.settings.SellItemsToVendorWhenWindowOpen;
            checkBox4.Checked = Main.settings.SearchInBothItemLists;
            checkBox5.Checked = Main.settings.AlwaysSalvageDiscardUseInCombat;
            checkBox6.Checked = Main.settings.StopWarehouseIfLastSlotIsFull;
            checkBox7.Checked = Main.settings.StopWarehouseIfFirstSlotIsFull;
            checkBox8.Checked = Main.settings.StoreItemsInWarehouseWhenWindowOpen;
            checkBox9.Checked = Main.settings.SellGreenEquipment;
            checkBox10.Checked = Main.settings.EnableAlwaysSalvageDiscardUse;
            checkBox11.Checked = Main.settings.SellBlueEquipment;
            checkBox12.Checked = Main.settings.SellYellowEquipment;
            checkBox13.Checked = Main.settings.SalvageWhiteEquipment;
            checkBox14.Checked = Main.settings.SalvageYellowEquipment;
            checkBox15.Checked = Main.settings.SalvageGreenEquipment;
            checkBox16.Checked = Main.settings.SalvageBlueEquipment;
            checkBox17.Checked = Main.settings.SellWhiteEquipment;
            checkBox18.Checked = Main.settings.SellGreyItems;
            //checkBox19.Checked = Main.settings.SellEverythingToAppropriateVendors;
            checkBox20.Checked = Main.settings.SellOrangeEquipment;
            checkBox21.Checked = Main.settings.SalvageOrangeEquipment;
            checkBox22.Checked = Main.settings.SalvagePurpleEquipment;
            checkBox23.Checked = Main.settings.SellEquipmentWhenCantBeSalvaged;
            checkBox24.Checked = Main.settings.DiscardEquipmentWhenCantBeSalvaged;
            checkBox25.Checked = Main.settings.TryToStoreItemsWhenWarehouseFull;

            textBox1.Text = Main.settings.InventorySlotsToBeUnaffected.ToString();
            textBox5.Text = Main.settings.TimerMiliseconds.ToString();
            textBox6.Text = Main.settings.SellSalvageOrangeEquipmentBelowLevel.ToString();
            textBox7.Text = Main.settings.SellSalvageYellowEquipmentBelowLevel.ToString();
            textBox8.Text = Main.settings.DontSalvageWhenFragsExceed.ToString();

            comboBox1.SelectedIndex = Main.settings.InventoryToBeUnaffected;
            comboBox6.SelectedIndex = 0;

            new Task(() => Main.TW.SearchItemsInProfile()).Start();
            Main.PendingUIRefresh = false;
        }
        #endregion
        #region Events
        private void TextBox2_KeyDown(object sender, KeyEventArgs e)// Search with Enter
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                button5_Click(this, new EventArgs());
            }
        }
        private void SettingsUI_Load(object sender, EventArgs e)//Refresh UI on load
        {
            if (Main.PendingUIRefresh)
                RefreshUI();
        }
        private void button1_Click(object sender, EventArgs e)// Save
        {
            H.SaveSettings(Main.Character);
        }
        private void button2_Click(object sender, EventArgs e)// Force Refresh
        {
            Main.TW.RefreshHashSets(true);
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)// Inventory to be unaffected
        {
            Main.settings.InventoryToBeUnaffected = comboBox1.SelectedIndex;
            if (comboBox1.SelectedIndex == 0)
                textBox1.Enabled = false;
            else
                textBox1.Enabled = true;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)// Slots to keep safe
        {
            var cleanedTextFromDigits = Regex.Replace(textBox1.Text, "[^\\d]", "");
            if (textBox1.Text != "" && textBox1.Text[0] == '0')
                textBox1.Text.Remove(0, 1);
            textBox1.Text = cleanedTextFromDigits;
            try { Main.settings.InventorySlotsToBeUnaffected = int.Parse(textBox1.Text); }
            catch { Main.settings.InventorySlotsToBeUnaffected = 0; }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)// Search text box
        {
            _SearchNameOrId = textBox2.Text.ToLower();
            LeftListControlChanged();
        }
        private void button3_Click(object sender, EventArgs e)// ->
        {
            foreach (var _item in listView1.SelectedItems)
            {
                var item = _item as ListViewItem;
                uint id = 0;
                uint.TryParse(item.SubItems[1].Text, out id);
                if (id == 0)
                    continue;
                var itemInSettings = Main.settings.Items.FirstOrDefault(x => x.Id == id);

                if (itemInSettings == null)
                {
                    var clonedItem = item.Clone() as ListViewItem;
                    listView2.Items.Add(clonedItem);
                    var itemToAdd = new Item(id);
                    itemToAdd.AddFunction(_CurrentItemFunction);
                    Main.settings.Items.Add(itemToAdd);
                }
                else
                {
                    if (!itemInSettings.HasFunction(_CurrentItemFunction))
                    {
                        var clonedItem = item.Clone() as ListViewItem;
                        listView2.Items.Add(clonedItem);
                        itemInSettings.AddFunction(_CurrentItemFunction);
                    }
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)// <-
        {
            foreach (var _item in listView2.SelectedItems)
            {
                var item = _item as ListViewItem;
                uint id = 0;
                uint.TryParse(item.SubItems[1].Text, out id);
                if (id == 0)
                    continue;
                var itemInSettings = Main.settings.Items.FirstOrDefault(x => x.Id == id);
                H.Log("found item");
                if (itemInSettings != null)
                {
                    itemInSettings.RemoveFunction(_CurrentItemFunction);
                    listView2.Items.Remove(item);
                }
                if (itemInSettings.Functions.Count == 0)
                    Main.settings.Items.Remove(itemInSettings);
            }
        }
        private void button6_Click(object sender, EventArgs e)//Reset
        {
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
            textBox2.Text = "";
            textBox4.Text = "0";
            textBox3.Text = "99";
            new Task(() => Main.TW.SearchItemsInDb()).Start();
            if (!Main.settings.SearchInBothItemLists)
                new Task(() => Main.TW.SearchItemsInProfile()).Start();
        }
        private void button5_Click(object sender, EventArgs e)// Search
        {
            new Task(() => Main.TW.SearchItemsInDb()).Start();
        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)// Category
        {
            if (comboBox3.SelectedIndex == 0)
                _FilterCategory = false;
            else
            {
                _SearchCategory = (ItemDataCategory)comboBox3.SelectedItem;
                _FilterCategory = true;
            }
            LeftListControlChanged();
        }
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)// Quality
        {
            if (comboBox4.SelectedIndex == 0)
                _FilterQuality = false;
            else
            {
                _SearchQuality = (ItemQuality)comboBox4.SelectedItem;
                _FilterQuality = true;
            }
            LeftListControlChanged();
        }
        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)// Currency
        {
            if (comboBox5.SelectedIndex == 0)
                _FilterMoney = false;
            else
            {
                _SearchMoney = (MoneyType)comboBox5.SelectedItem;
                _FilterMoney = true;
            }
            LeftListControlChanged();
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)// Combobox "Item to Keep/Sell/etc."
        {
            _CurrentItemFunction = (ItemFunction)comboBox2.SelectedIndex;
            RightListControlChanged();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)// Use dynamic search
        {
            Main.settings.DynamicSearch = checkBox2.Checked;
            LeftListControlChanged();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)// Auto Stack Warehouse
        {
            Main.settings.AutoStackWarehouse = checkBox1.Checked;
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)// Search in both lists
        {
            Main.settings.SearchInBothItemLists = checkBox4.Checked;
            RightListControlChanged();
        }
        private void checkBox5_CheckedChanged(object sender, EventArgs e)// Use Item in AlwaysUse in Combat too
        {
            Main.settings.AlwaysSalvageDiscardUseInCombat = checkBox5.Checked;
        }
        private void checkBox6_CheckedChanged(object sender, EventArgs e)// Stop WH functions if last slot is full
        {
            Main.settings.StopWarehouseIfLastSlotIsFull = checkBox6.Checked;
        }
        private void checkBox7_CheckedChanged(object sender, EventArgs e)// Stop WH functions if first slot is full
        {
            Main.settings.StopWarehouseIfFirstSlotIsFull = checkBox7.Checked;
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)// Sell items to NPC when window open
        {
            Main.settings.SellItemsToVendorWhenWindowOpen = checkBox3.Checked;
        }
        private void checkBox8_CheckedChanged(object sender, EventArgs e)// Store items in Warehoue when window open
        {
            Main.settings.StoreItemsInWarehouseWhenWindowOpen = checkBox8.Checked;
        }
        private void checkBox9_CheckedChanged(object sender, EventArgs e)// Sell green equipment
        {
            Main.settings.SellGreenEquipment = checkBox9.Checked;
        }
        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)// Inventory or all items
        {
            _SearchIn = comboBox6.SelectedIndex;
            LeftListControlChanged();
        }
        private void textBox4_TextChanged(object sender, EventArgs e)// Min level
        {
            var cleanedTextFromDigits = Regex.Replace(textBox4.Text, "[^\\d]", "");
            if (cleanedTextFromDigits == "")
            {
                textBox4.Text = "0";
                _SearchMinLevel = 0;
                return;
            }
            if (cleanedTextFromDigits[0] == '0' && cleanedTextFromDigits.Length == 2)
                cleanedTextFromDigits = cleanedTextFromDigits.Remove(0, 1);
            textBox4.Text = cleanedTextFromDigits;
            ushort.TryParse(cleanedTextFromDigits, out _SearchMinLevel);
            LeftListControlChanged();
        }
        private void textBox3_TextChanged(object sender, EventArgs e)// Max level
        {
            var cleanedTextFromDigits = Regex.Replace(textBox3.Text, "[^\\d]", "");
            if (cleanedTextFromDigits == "")
            {
                _SearchMaxLevel = 99;
                return;
            }
            if (cleanedTextFromDigits[0] == '0' && cleanedTextFromDigits.Length == 2)
                cleanedTextFromDigits = cleanedTextFromDigits.Remove(0, 1);
            textBox3.Text = cleanedTextFromDigits;
            ushort.TryParse(cleanedTextFromDigits, out _SearchMaxLevel);
            LeftListControlChanged();
        }
        private void textBox5_TextChanged(object sender, EventArgs e)// Set timer milliseconds
        {
            var cleanedTextFromDigits = Regex.Replace(textBox5.Text, "[^\\d]", "");
            if (textBox5.Text != "" && textBox5.Text[0] == '0')
                textBox5.Text.Remove(0, 1);
            textBox5.Text = cleanedTextFromDigits;
            try { Main.settings.TimerMiliseconds = int.Parse(textBox5.Text); }
            catch { Main.settings.TimerMiliseconds = 500; }
        }
        private void checkBox10_CheckedChanged(object sender, EventArgs e)// Enable Always Use/Discard/Salvagess
        {
            Main.settings.EnableAlwaysSalvageDiscardUse = checkBox10.Checked;
        }
        private void checkBox11_CheckedChanged(object sender, EventArgs e)// Sell blue equipment
        {
            Main.settings.SellBlueEquipment = checkBox11.Checked;
        }
        private void checkBox15_CheckedChanged(object sender, EventArgs e)// Salvage all green
        {
            Main.settings.SalvageGreenEquipment = checkBox15.Checked;
        }
        private void checkBox16_CheckedChanged(object sender, EventArgs e)// Salvage all blue
        {
            Main.settings.SalvageBlueEquipment = checkBox16.Checked;
        }
        private void checkBox13_CheckedChanged(object sender, EventArgs e)// Salvage all white
        {
            Main.settings.SalvageWhiteEquipment = checkBox13.Checked;
        }
        private void textBox6_TextChanged(object sender, EventArgs e)// Salvage orange equipment below level
        {
            var cleanedTextFromDigits = Regex.Replace(textBox6.Text, "[^\\d]", "");
            if (cleanedTextFromDigits == "")
            {
                textBox6.Text = "0";
                _SearchMinLevel = 0;
                return;
            }
            if (cleanedTextFromDigits[0] == '0' && cleanedTextFromDigits.Length == 2)
                cleanedTextFromDigits = cleanedTextFromDigits.Remove(0, 1);
            textBox6.Text = cleanedTextFromDigits;
            int temp = 0;
            int.TryParse(cleanedTextFromDigits, out temp);
            Main.settings.SellSalvageOrangeEquipmentBelowLevel = temp;
            LeftListControlChanged();
        }
        private void checkBox19_CheckedChanged(object sender, EventArgs e)//Sell to appropriate shops
        {
            //Main.settings.SellEverythingToAppropriateVendors = checkBox19.Checked;
        }
        private void checkBox12_CheckedChanged(object sender, EventArgs e)// Sell all yellow equipment
        {
            Main.settings.SellYellowEquipment = checkBox12.Checked;
        }
        private void checkBox18_CheckedChanged(object sender, EventArgs e)// Sell all grey items
        {
            Main.settings.SellGreyItems = checkBox18.Checked;
        }
        private void checkBox14_CheckedChanged(object sender, EventArgs e)// Salvage all yellow
        {
            Main.settings.SalvageYellowEquipment = checkBox14.Checked;
        }
        private void checkBox20_CheckedChanged(object sender, EventArgs e)// Sell orange
        {
            Main.settings.SellOrangeEquipment = checkBox20.Checked;
        }
        private void textBox7_TextChanged(object sender, EventArgs e)// Salvage yellow equipment below level
        {
            var cleanedTextFromDigits = Regex.Replace(textBox7.Text, "[^\\d]", "");
            if (cleanedTextFromDigits == "")
            {
                textBox7.Text = "0";
                _SearchMinLevel = 0;
                return;
            }
            if (cleanedTextFromDigits[0] == '0' && cleanedTextFromDigits.Length == 2)
                cleanedTextFromDigits = cleanedTextFromDigits.Remove(0, 1);
            textBox7.Text = cleanedTextFromDigits;
            int temp = 0;
            int.TryParse(cleanedTextFromDigits, out temp);
            Main.settings.SellSalvageYellowEquipmentBelowLevel = temp;
            LeftListControlChanged();
        }
        private void textBox8_TextChanged(object sender, EventArgs e)// Don't salvage when fragments exceed
        {
            var cleanedTextFromDigits = Regex.Replace(textBox8.Text, "[^\\d]", "");
            if (cleanedTextFromDigits == "")
            {
                textBox8.Text = "0";
                _SearchMinLevel = 0;
                return;
            }
            if (cleanedTextFromDigits[0] == '0' && cleanedTextFromDigits.Length == 2)
                cleanedTextFromDigits = cleanedTextFromDigits.Remove(0, 1);
            textBox8.Text = cleanedTextFromDigits;
            int temp = 0;
            int.TryParse(cleanedTextFromDigits, out temp);
            Main.settings.DontSalvageWhenFragsExceed = temp;
            LeftListControlChanged();
        }
        private void checkBox17_CheckedChanged(object sender, EventArgs e)// Sell white
        {
            Main.settings.SellWhiteEquipment = checkBox17.Checked;
        }
        private void checkBox21_CheckedChanged(object sender, EventArgs e)// Salvage orange equipment
        {
            Main.settings.SalvageOrangeEquipment = checkBox21.Checked;
        }
        private void checkBox22_CheckedChanged(object sender, EventArgs e)// Salvage purple
        {
            Main.settings.SalvagePurpleEquipment = checkBox22.Checked;
        }
        private void checkBox23_CheckedChanged(object sender, EventArgs e)// Sell eq if can't be salvaged
        {
            Main.settings.SellEquipmentWhenCantBeSalvaged = checkBox23.Checked;
        }
        private void checkBox24_CheckedChanged(object sender, EventArgs e)// Discard eq if can't be salvaged
        {
            Main.settings.DiscardEquipmentWhenCantBeSalvaged = checkBox24.Checked;
        }
        private void checkBox25_CheckedChanged(object sender, EventArgs e)// Try to store items in WH when it's full (to complete stacks)
        {
            Main.settings.TryToStoreItemsWhenWarehouseFull = checkBox25.Checked;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Path.Combine(H.DataDirectory, "Information.txt"), Vars.Information);
            Process.Start(Path.Combine(H.DataDirectory, "Information.txt"));
        }
    }
        #endregion
}
