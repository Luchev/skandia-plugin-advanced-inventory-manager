using Plugins;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace AdvancedInventoryManager
{
    public static class H
    {
        private static XmlSerializer serializer;
        private static string LastLog = "";
        public static T DeserializeFromFile<T>(string file, bool useCutomDirectory = false)
        {
            if (!file.EndsWith(".xml"))
                file = file + ".xml";
            
            serializer = new XmlSerializer(typeof(T));
            try
            {
                if (useCutomDirectory)
                    using (var reader = new StreamReader(file))
                        return (T)serializer.Deserialize(reader);
                else
                    using (var reader = new StreamReader(Path.Combine(ProfilesDirectory, file)))
                        return (T)serializer.Deserialize(reader);
            }
            catch (Exception)
            {
                DialogResult dialogResult = MessageBox.Show("Your Profile is corrupted! Would you like to load the defaults? " + Environment.NewLine + "ATTENTION: This will erase all your old data!", "XML Error", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Main.settings = new Settings();
                    SaveSettings(Main.Character);
                    UpdateSettings();
                }
                else if (dialogResult == DialogResult.No)
                {
                    LoadSettings();
                }
                return default(T);
            }
        }
        public static void SerializeToFile<T>(string file, T instance, bool useCustomDirectory = false)
        {
            if (!file.EndsWith(".xml"))
                file = file + ".xml";
            
            if (!Directory.Exists(ProfilesDirectory))
            {
                Directory.CreateDirectory(ProfilesDirectory);
            }
            if (!Directory.Exists(Path.Combine(ProfilesDirectory, "Data")))
            {
                Directory.CreateDirectory(Path.Combine(ProfilesDirectory, "Data"));
            }

            serializer = new XmlSerializer(typeof(T));
            if (useCustomDirectory)
                using (var writer = new StreamWriter(file))
                    serializer.Serialize(writer, instance);
            else
                using (var writer = new StreamWriter(Path.Combine(ProfilesDirectory, file)))
                    serializer.Serialize(writer, instance);
        }
        public static string SerializeToString<T>(T instance)
        {
            serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringWriter()) {
                serializer.Serialize(writer, instance);
                return writer.ToString();
            }
        }
        public static T DeserializeFromString<T>(string data)
        {
            serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(data))
                return (T)serializer.Deserialize(reader);
        }
        public static string ProfilesDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "profiles", "plugins", "AIM");
            }
        }
        public static string GenerateProfileName(string file)
        {
            if (!file.EndsWith(".xml"))
                file = file + ".xml";
            return Path.Combine(ProfilesDirectory, "Data", file);
        }
        public static string GenerateDatafileName(string file)
        {
            if (!file.EndsWith(".xml"))
                file = file + ".xml";
            return Path.Combine(ProfilesDirectory, file);
        }
        public static string SellerProfilesDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "profiles", "seller");
            }
        }
        public static List<string> GetSellerProfiles()
        {
            List<string> files = Directory.GetFiles(SellerProfilesDirectory, "*.akss").ToList();
            for (int i = 0; i < files.Count; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            return files;
        }
        public static string PluginsDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "plugins");
            }
        }
        public static void Log(string _message)
        {
            string message = "[AIM]" + _message;
            if (LastLog != message)
                Skandia.MessageLog(message);
            LastLog = message;
        }
        public static void LoadSettings(string fileName = "")
        {
            if (fileName != "" && File.Exists(Path.Combine(ProfilesDirectory, fileName + ".xml")))
            {
                Main.settings = DeserializeFromFile<Settings>(fileName);
                Log("[H]Loading settings for character " + fileName);
            }
            else
            {
                Log("[H]Loading default settings");
                Main.settings = new Settings();
                if (fileName != "")
                    SaveSettings(fileName);
            }
            UpdateSettings();
        }
        private static void UpdateSettings()
        {
            Log("[H]Updating settings");
            if (Main.settingsUI != null)
            {
                Main.settingsUI.RefreshUI();
            }
            else
            {
                Main.PendingUIRefresh = true;
            }
        }
        public static void SaveSettings(string fileName)
        {
            SerializeToFile(fileName, Main.settings);
            Log("[H]Saving settings for character " + fileName);
        }
        public static string DataDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "profiles", "plugins", "AIM", "Data");
            }
        }
        public static string DataFile
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "profiles", "plugins", "AIM", "Data", "Data.xml");
            }
        }
    }
}
