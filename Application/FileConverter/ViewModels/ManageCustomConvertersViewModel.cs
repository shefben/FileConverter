using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileConverter.CustomConverters;
using FileConverter.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Xml.Serialization;

namespace FileConverter.ViewModels
{
    public class ManageCustomConvertersViewModel : ObservableObject
    {
        private CustomConverterDefinition selected;
        private ConversionPreset selectedCustomPreset;
        private string xmlText;
        private CustomConverterFolder rootFolder;
        private CustomConverterTreeItem selectedItem;

        public ManageCustomConvertersViewModel()
        {
            this.Converters = new ObservableCollection<CustomConverterDefinition>(CustomConverterManager.Converters);
            this.BuildTree();
            var settings = Ioc.Default.GetRequiredService<ISettingsService>().Settings;
            this.CustomPresets = new ObservableCollection<ConversionPreset>(settings.ConversionPresets.Where(p => p.OutputType == OutputType.Custom));
            CustomConverterManager.ConvertersUpdated += () =>
            {
                this.Converters = new ObservableCollection<CustomConverterDefinition>(CustomConverterManager.Converters);
                this.OnPropertyChanged(nameof(this.Converters));
                this.BuildTree();
            };
            this.removeCommand = new RelayCommand(this.RemoveSelected, () => this.Selected != null);
            this.editCommand = new RelayCommand(this.EditSelected, () => this.Selected != null);
            this.importCommand = new RelayCommand(this.ImportConverter);
            this.exportCommand = new RelayCommand(this.ExportSelected, () => this.Selected != null);
            this.addCommand = new RelayCommand(this.AddConverter);
            this.saveXmlCommand = new RelayCommand(this.SaveXml, () => this.Selected != null);
        }

        public ObservableCollection<CustomConverterDefinition> Converters { get; private set; }

        public CustomConverterFolder RootFolder
        {
            get => this.rootFolder;
            private set => this.SetProperty(ref this.rootFolder, value);
        }

        public CustomConverterTreeItem SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.SetProperty(ref this.selectedItem, value))
                {
                    this.Selected = (value as CustomConverterNode)?.Converter;
                }
            }
        }

        public CustomConverterDefinition Selected
        {
            get => this.selected;
            set
            {
                if (this.SetProperty(ref this.selected, value))
                {
                    (this.RemoveCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    (this.EditCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    (this.ExportCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    (this.SaveXmlCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    this.LoadSelectedXml();
                }
            }
        }

        private readonly RelayCommand removeCommand;
        public ICommand RemoveCommand => this.removeCommand;
        private readonly RelayCommand editCommand;
        public ICommand EditCommand => this.editCommand;
        private readonly RelayCommand importCommand;
        public ICommand ImportCommand => this.importCommand;
        private readonly RelayCommand exportCommand;
        public ICommand ExportCommand => this.exportCommand;
        private readonly RelayCommand addCommand;
        public ICommand AddCommand => this.addCommand;
        private readonly RelayCommand saveXmlCommand;
        public ICommand SaveXmlCommand => this.saveXmlCommand;

        public ObservableCollection<ConversionPreset> CustomPresets { get; private set; }

        public ConversionPreset SelectedCustomPreset
        {
            get => this.selectedCustomPreset;
            set => this.SetProperty(ref this.selectedCustomPreset, value);
        }

        public string XmlText
        {
            get => this.xmlText;
            set => this.SetProperty(ref this.xmlText, value);
        }

        private void BuildTree()
        {
            var root = new CustomConverterFolder("Root");
            string baseDir = CustomConverterManager.GetDirectory();
            foreach (var def in this.Converters)
            {
                string relative = string.IsNullOrEmpty(def.FilePath) ? def.Name + ".xml" : System.IO.Path.GetRelativePath(baseDir, def.FilePath);
                var parts = relative.Split(System.IO.Path.DirectorySeparatorChar);
                CustomConverterFolder folder = root;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];
                    var next = folder.Children.OfType<CustomConverterFolder>().FirstOrDefault(f => f.Name == part);
                    if (next == null)
                    {
                        next = new CustomConverterFolder(part, folder);
                        folder.Children.Add(next);
                    }
                    folder = next;
                }
                var node = new CustomConverterNode(def, folder);
                folder.Children.Add(node);
            }
            this.RootFolder = root;
        }

        private void RemoveSelected()
        {
            if (this.Selected == null) return;
            CustomConverterManager.RemoveConverter(this.Selected.Name);
            this.Converters.Remove(this.Selected);
            this.BuildTree();
        }

        private void EditSelected()
        {
            if (this.Selected == null) return;
            var vm = new CustomConverterWizardViewModel();
            vm.Name = this.Selected.Name;
            vm.ExtensionsText = string.Join(",", this.Selected.Extensions);
            vm.OutputExtension = this.Selected.OutputExtension;
            vm.ProgramPath = this.Selected.ProgramPath;
            vm.Arguments = this.Selected.Arguments;
            vm.OutputTemplate = this.Selected.OutputTemplate;
            vm.IconPath = this.Selected.IconPath;
            vm.PostProcessCommand = this.Selected.PostProcessCommand;
            vm.MultiFileAtOnce = this.Selected.MultiFileAtOnce;
            vm.ShowProgram = this.Selected.ShowProgram;
            vm.PostAction = this.Selected.PostAction;
            foreach (var opt in this.Selected.Options)
            {
                vm.Options.Add(opt);
            }
            var wnd = new Views.CustomConverterWizard { DataContext = vm };
            if (wnd.ShowDialog() == true)
            {
                CustomConverterManager.SaveConverter(vm.Definition);
                int index = this.Converters.IndexOf(this.Selected);
                this.Converters[index] = vm.Definition;
                this.Selected = vm.Definition;
                this.BuildTree();
            }
        }

        private void ImportConverter()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Converter package (*.zip)|*.zip" };
            if (dlg.ShowDialog() == true)
            {
                CustomConverterManager.Import(dlg.FileName);
                this.Converters = new ObservableCollection<CustomConverterDefinition>(CustomConverterManager.Converters);
                this.OnPropertyChanged(nameof(this.Converters));
                this.BuildTree();
            }
        }

        private void ExportSelected()
        {
            if (this.Selected == null) return;
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Converter package (*.zip)|*.zip", FileName = this.Selected.Name + ".zip" };
            if (dlg.ShowDialog() == true)
            {
                CustomConverterManager.Export(this.Selected.Name, dlg.FileName);
            }
        }

        private void AddConverter()
        {
            var vm = new CustomConverterWizardViewModel();
            var wnd = new Views.CustomConverterWizard { DataContext = vm };
            if (wnd.ShowDialog() == true)
            {
                CustomConverterManager.SaveConverter(vm.Definition);
                this.Converters.Add(vm.Definition);
                this.Selected = vm.Definition;
                this.BuildTree();
            }
        }

        private void LoadSelectedXml()
        {
            if (this.Selected == null)
            {
                this.XmlText = string.Empty;
                return;
            }

            try
            {
                string file = this.Selected.FilePath;
                if (string.IsNullOrEmpty(file))
                {
                    string directory = CustomConverterManager.GetDirectory();
                    file = Path.Combine(directory, this.Selected.Name + ".xml");
                }
                if (File.Exists(file))
                {
                    this.XmlText = File.ReadAllText(file);
                }
                else
                {
                    var serializer = new XmlSerializer(typeof(CustomConverterDefinition), new XmlRootAttribute("CustomConverter"));
                    using StringWriter sw = new StringWriter();
                    serializer.Serialize(sw, this.Selected);
                    this.XmlText = sw.ToString();
                }
            }
            catch (Exception ex)
            {
                this.XmlText = "<!-- " + ex.Message + " -->";
            }
        }

        private void SaveXml()
        {
            if (this.Selected == null)
            {
                return;
            }

            try
            {
                var serializer = new XmlSerializer(typeof(CustomConverterDefinition), new XmlRootAttribute("CustomConverter"));
                using StringReader sr = new StringReader(this.XmlText ?? string.Empty);
                var def = (CustomConverterDefinition)serializer.Deserialize(sr);
                CustomConverterManager.SaveConverter(def);
                int index = this.Converters.IndexOf(this.Selected);
                if (index >= 0)
                {
                    this.Converters[index] = def;
                }
                this.Selected = def;
                this.BuildTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
