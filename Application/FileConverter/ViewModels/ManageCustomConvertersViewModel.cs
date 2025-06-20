using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileConverter.CustomConverters;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace FileConverter.ViewModels
{
    public class ManageCustomConvertersViewModel : ObservableObject
    {
        private CustomConverterDefinition selected;

        public ManageCustomConvertersViewModel()
        {
            this.Converters = new ObservableCollection<CustomConverterDefinition>(CustomConverterManager.Converters);
            CustomConverterManager.ConvertersUpdated += () =>
            {
                this.Converters = new ObservableCollection<CustomConverterDefinition>(CustomConverterManager.Converters);
                this.OnPropertyChanged(nameof(this.Converters));
            };
            this.removeCommand = new RelayCommand(this.RemoveSelected, () => this.Selected != null);
            this.editCommand = new RelayCommand(this.EditSelected, () => this.Selected != null);
            this.importCommand = new RelayCommand(this.ImportConverter);
            this.exportCommand = new RelayCommand(this.ExportSelected, () => this.Selected != null);
        }

        public ObservableCollection<CustomConverterDefinition> Converters { get; private set; }

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

        private void RemoveSelected()
        {
            if (this.Selected == null) return;
            CustomConverterManager.RemoveConverter(this.Selected.Name);
            this.Converters.Remove(this.Selected);
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
    }
}
