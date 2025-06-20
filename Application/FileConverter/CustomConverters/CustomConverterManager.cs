
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression; // ZipArchive & extensions<br>using System.Text; // Encoding
using System.Reflection;

namespace FileConverter.CustomConverters
{
    public static class CustomConverterManager
    {
        private static Dictionary<string, CustomConverterDefinition> converters;
        private static FileSystemWatcher watcher;

        public static event Action ConvertersUpdated;

        public static IEnumerable<CustomConverterDefinition> Converters
        {
            get
            {
                EnsureLoaded();
                return converters.Values;
            }
        }

        public static CustomConverterDefinition GetConverter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            EnsureLoaded();
            converters.TryGetValue(name, out var def);
            return def;
        }

        private static void EnsureLoaded()
        {
            if (converters != null)
            {
                return;
            }

            converters = new Dictionary<string, CustomConverterDefinition>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CustomConverters");
                if (!Directory.Exists(directory))
                {
                    return;
                }

                foreach (string file in Directory.GetFiles(directory, "*.xml"))
                {
                    try
                    {
                        FileConverter.XmlHelpers.LoadFromFile("CustomConverter", file, out CustomConverterDefinition def);
                        if (!string.IsNullOrEmpty(def?.Name))
                        {
                            converters[def.Name] = def;
                        }
                    }
                    catch (Exception ex)
                    {
                        Diagnostics.Debug.Log($"Failed to load custom converter {file}: {ex.Message}");
                    }
                }

                EnsureWatcher(directory);
            }
            catch (Exception ex)
            {
                Diagnostics.Debug.Log($"Failed to load custom converters: {ex.Message}");
            }
        }

        private static void EnsureWatcher(string directory)
        {
            if (watcher != null)
            {
                return;
            }

            watcher = new FileSystemWatcher(directory, "*.xml");
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            watcher.Changed += OnConvertersChanged;
            watcher.Created += OnConvertersChanged;
            watcher.Deleted += OnConvertersChanged;
            watcher.Renamed += OnConvertersChanged;
            watcher.EnableRaisingEvents = true;
        }

        private static void OnConvertersChanged(object sender, FileSystemEventArgs e)
        {
            converters = null;
            ConvertersUpdated?.Invoke();
        }

        public static void SaveConverter(CustomConverterDefinition def)
        {
            if (def == null || string.IsNullOrEmpty(def.Name))
            {
                return;
            }

            EnsureLoaded();
            string directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CustomConverters");
            Directory.CreateDirectory(directory);
            string file = Path.Combine(directory, def.Name + ".xml");
            XmlHelpers.SaveToFile("CustomConverter", file, def);
            converters[def.Name] = def;
        }

        public static void RemoveConverter(string name)
        {
            EnsureLoaded();
            string directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CustomConverters");
            string file = Path.Combine(directory, name + ".xml");
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            converters.Remove(name);
        }

        public static void Export(string name, string destination)
        {
            var def = GetConverter(name);
            if (def == null)
            {
                return;
            }

            string directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CustomConverters");
            string xmlPath = Path.Combine(directory, name + ".xml");

            using (var zip = System.IO.Compression.ZipFile.Open(destination, System.IO.Compression.ZipArchiveMode.Create))
            {
                if (File.Exists(xmlPath))
                {
                    zip.CreateEntryFromFile(xmlPath, name + ".xml");
                }

                if (!string.IsNullOrEmpty(def.IconPath))
                {
                    string icon = Path.Combine(directory, def.IconPath);
                    if (File.Exists(icon))
                    {
                        zip.CreateEntryFromFile(icon, Path.GetFileName(def.IconPath));
                    }
                }
            }
        }

        public static void Import(string package)
        {
            string directory = Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                ), "CustomConverters");
            Directory.CreateDirectory(directory);

            using (var archive = ZipFile.OpenRead(package))
            {
                foreach (var entry in archive.Entries)
                {
                    // recombine the entry name to a full path under `directory`
                    string filePath = Path.Combine(directory, entry.FullName);

                    // ensure the directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    // overwrite if already present
                    entry.ExtractToFile(filePath, overwrite: true);
                }
            }

            converters = null;
            ConvertersUpdated?.Invoke();
        }

    }
}
