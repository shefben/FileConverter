using System;
using System.Collections.Generic;
using System.IO;
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

        internal static string GetDirectory()
        {
            string dir = Path.Combine(FileConverterExtension.PathHelpers.GetUserDataFolderPath, "CustomConverters");
            return dir;
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
                string directory = GetDirectory();
                if (!Directory.Exists(directory))
                {
                    return;
                }

                foreach (string file in Directory.GetFiles(directory, "*.xml", SearchOption.AllDirectories))
                {
                    try
                    {
                        FileConverter.XmlHelpers.LoadFromFile("CustomConverter", file, out CustomConverterDefinition def);
                        if (!string.IsNullOrEmpty(def?.Name))
                        {
                            def.FilePath = file;
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

            watcher = new FileSystemWatcher(directory, "*.xml")
            {
                IncludeSubdirectories = true
            };
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
            string directory = GetDirectory();
            string file = def.FilePath;
            if (string.IsNullOrEmpty(file))
            {
                file = Path.Combine(directory, def.Name + ".xml");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(file));
            XmlHelpers.SaveToFile("CustomConverter", file, def);
            def.FilePath = file;
            converters[def.Name] = def;
        }

        public static void RemoveConverter(string name)
        {
            EnsureLoaded();
            string directory = GetDirectory();
            string file = Path.Combine(directory, name + ".xml");
            if (converters.TryGetValue(name, out var def) && !string.IsNullOrEmpty(def.FilePath))
            {
                file = def.FilePath;
            }
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

            string directory = GetDirectory();
            string xmlPath = def.FilePath ?? Path.Combine(directory, name + ".xml");

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
            string directory = GetDirectory();
            Directory.CreateDirectory(directory);
            System.IO.Compression.ZipFile.ExtractToDirectory(package, directory, true);
            converters = null;
            ConvertersUpdated?.Invoke();
        }
    }
}
