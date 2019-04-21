using Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedInventoryManager
{
    public class ModuleManager
    {
        private Moduletype currentModuleType = Moduletype.None;
        public ModuleManager(){ }
        public void SetModule(Moduletype _type) { currentModuleType = _type; }
        public Moduletype GetModule() { return currentModuleType; }
        public void Call()
        {
            updateModule();
            callModule();
        }

        private void callModule()
        {
            if (currentModuleType == Moduletype.Warehouse)
                Modules.Warehouse.Call();
            else if (currentModuleType == Moduletype.Sell)
                Modules.Sell.Call();
            else if (currentModuleType == Moduletype.None)
                Modules.None.Call();
        }

        private void updateModule()
        {
            if (Skandia.Me.IsWindowOpen(WindowType.Warehouse))
                currentModuleType = Moduletype.Warehouse;
            else if (Skandia.Me.IsWindowOpen(WindowType.Shop))
                currentModuleType = Moduletype.Sell;
            else
                currentModuleType = Moduletype.None;
        }
    }
    public enum Moduletype
    {
        None,
        Sell,
        Warehouse
    }
}