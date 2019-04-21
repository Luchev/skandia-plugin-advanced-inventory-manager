using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins;
using PluginsCommon;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace AdvancedInventoryManager
{
    public class Main : IPluginChat
    {
        private static Stopwatch timer;
        private static Thread settingsUIThread;
        private static Thread workerThread;
        public static ThreadWorker TW;
        public static SettingsUI settingsUI;
        public static bool PendingUIRefresh = false;
        public static string Character;
        public static Settings settings;
        public static ModuleManager Manager;
        public static Data data;
        public static bool IsInventoryFull = false;
        public string Author
        {
            get
            {
                return "Hachiman";
            }
        }
        public string Description
        {
            get
            {
                return "Managing  your inventory, warehouse and seller for you";
            }
        }
        public string Name
        {
            get
            {
                return "Advanced Inventory Manager";
            }
        }
        public Version Version
        {
            get
            {
                return new Version(1, 0, 2);
            }
        }
        public void OnButtonClick()
        {
            if (settingsUI != null && settingsUI.Visible == false)
            {
                settingsUI.Show();
            }
            else if (settingsUI != null && settingsUI.Visible == true)
            {
                settingsUI.Hide();
            }
            else
            {
                StartSettingsUIThread();
            }
        }
        public static void StartSettingsUIThread()
        {
            if (settingsUIThread == null ||
            settingsUIThread.ThreadState == System.Threading.ThreadState.Aborted ||
            settingsUIThread.ThreadState == System.Threading.ThreadState.Stopped ||
            settingsUIThread.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                settingsUIThread = new Thread(SettingsUIThreadStartMethod);
                settingsUIThread.SetApartmentState(ApartmentState.STA);
                settingsUIThread.Start();
                return;
            }
            else
            {
                return;
            }
        }
        public static void SettingsUIThreadStartMethod()
        {
            if (settingsUI == null)
            {
                settingsUI = new SettingsUI();
            }
            settingsUI.ShowDialog();
        }
        public static void StartWorkerThread()
        {
            if (workerThread == null ||
            workerThread.ThreadState == System.Threading.ThreadState.Aborted ||
            workerThread.ThreadState == System.Threading.ThreadState.Stopped ||
            workerThread.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                workerThread = new Thread(workerThreadStartMethod);
                workerThread.SetApartmentState(ApartmentState.STA);
                workerThread.Start();
                return;
            }
            else
            {
                return;
            }
        }
        public static void workerThreadStartMethod()
        {
            if (TW == null)
            {
                TW = new ThreadWorker();
            }
        }
        public void OnStart()
        {
            timer = new Stopwatch();
            Manager = new ModuleManager();
            StartWorkerThread();
            if (Directory.Exists(H.DataDirectory))
            {
                Directory.CreateDirectory(H.DataDirectory);
            }

            if (!File.Exists(H.DataFile))
            {
                data = new Data();
                H.SerializeToFile(H.DataFile, data, true);
            }
            else
            {
                data = H.DeserializeFromFile<Data>(H.DataFile, true);
            }
        }
        public void OnStop(bool off)
        {
            if (settingsUI != null && settingsUI.InvokeRequired)
            {
                settingsUI.Invoke((MethodInvoker)delegate
                {
                    settingsUI.Close();
                    settingsUI.Dispose();
                });
            }
            else
            {
                if (settingsUI != null && !settingsUI.IsDisposed)
                {
                    settingsUI.Close();
                    settingsUI.Dispose();
                }
            }
        }
        public void Pulse()
        {
            Skandia.Update();
            if (!Skandia.IsInGame)
                return;
            if (Character == "" || Character != Skandia.Me.Name)
            {
                Character = Skandia.Me.Name;
                H.LoadSettings(Character);
            }
            if (!timer.IsRunning)
                timer.Start();
            if (timer.ElapsedMilliseconds > settings.TimerMiliseconds)
            {
                if (Skandia.Me.GotTarget)
                    H.Log(Skandia.Me.CurrentTarget.Name);
                Manager.Call();
                timer.Restart();
            }
        }
        public void OnChatMessage(string message, uint type, bool isWhisper)
        {
            if ((ChatMessageType)type == ChatMessageType.System && message == data.InventoryFull)
                IsInventoryFull = true;
        }
    }
}
