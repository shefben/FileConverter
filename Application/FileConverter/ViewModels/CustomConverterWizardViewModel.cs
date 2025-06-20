using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileConverter.CustomConverters;
using FileConverter.Views;

namespace FileConverter.ViewModels
{
    public class CustomConverterWizardViewModel : ObservableObject
    {
        private int stepIndex;
        private string name;
        private string extensionsText;
        private string outputExtension;
        private string programPath;
        private string arguments;
        private string outputTemplate;
        private string iconPath;
        private string postProcessCommand;
        private bool multiFileAtOnce;
        private bool showProgram = true;
        private InputPostConversionAction postAction;

        private ObservableCollection<CustomConverterOptionDefinition> options = new ObservableCollection<CustomConverterOptionDefinition>();
        private CustomConverterOptionDefinition selectedOption;

        public CustomConverterWizardViewModel()
        {
            this.NextCommand = new RelayCommand(this.NextStep, this.CanNext);
            this.BackCommand = new RelayCommand(this.PreviousStep, this.CanBack);
            this.FinishCommand = new RelayCommand<Window>(this.Finish, this.CanFinish);
            this.AddOptionCommand = new RelayCommand(this.AddOption);
            this.EditOptionCommand = new RelayCommand(this.EditCurrentOption, () => this.SelectedOption != null);
            this.RemoveOptionCommand = new RelayCommand(this.RemoveCurrentOption, () => this.SelectedOption != null);
        }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand FinishCommand { get; }
        public ICommand AddOptionCommand { get; }
        public ICommand EditOptionCommand { get; }
        public ICommand RemoveOptionCommand { get; }

        public int StepIndex
        {
            get => this.stepIndex;
            set => this.SetProperty(ref this.stepIndex, value);
        }

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        public string ExtensionsText
        {
            get => this.extensionsText;
            set => this.SetProperty(ref this.extensionsText, value);
        }

        public string OutputExtension
        {
            get => this.outputExtension;
            set => this.SetProperty(ref this.outputExtension, value);
        }

        public string ProgramPath
        {
            get => this.programPath;
            set => this.SetProperty(ref this.programPath, value);
        }

        public string Arguments
        {
            get => this.arguments;
            set => this.SetProperty(ref this.arguments, value);
        }

        public string OutputTemplate
        {
            get => this.outputTemplate;
            set => this.SetProperty(ref this.outputTemplate, value);
        }

        public string IconPath
        {
            get => this.iconPath;
            set => this.SetProperty(ref this.iconPath, value);
        }

        public string PostProcessCommand
        {
            get => this.postProcessCommand;
            set => this.SetProperty(ref this.postProcessCommand, value);
        }

        public bool MultiFileAtOnce
        {
            get => this.multiFileAtOnce;
            set => this.SetProperty(ref this.multiFileAtOnce, value);
        }

        public bool ShowProgram
        {
            get => this.showProgram;
            set => this.SetProperty(ref this.showProgram, value);
        }

        public InputPostConversionAction PostAction
        {
            get => this.postAction;
            set => this.SetProperty(ref this.postAction, value);
        }

        public ObservableCollection<CustomConverterOptionDefinition> Options => this.options;

        public CustomConverterOptionDefinition SelectedOption
        {
            get => this.selectedOption;
            set
            {
                if (this.SetProperty(ref this.selectedOption, value))
                {
                    (this.RemoveOptionCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    (this.EditOptionCommand as RelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        public CustomConverterDefinition Definition { get; private set; }

        private bool CanNext()
        {
            if (this.StepIndex == 0)
            {
                return !string.IsNullOrWhiteSpace(this.Name) &&
                    !string.IsNullOrWhiteSpace(this.ExtensionsText) &&
                    !string.IsNullOrWhiteSpace(this.OutputExtension);
            }

            if (this.StepIndex == 1)
            {
                return !string.IsNullOrWhiteSpace(this.ProgramPath);
            }

            return false;
        }

        private void NextStep()
        {
            if (this.StepIndex < 2)
            {
                this.StepIndex++;
            }
        }

        private bool CanBack() => this.StepIndex > 0;

        private void PreviousStep()
        {
            if (this.StepIndex > 0)
            {
                this.StepIndex--;
            }
        }

        private bool CanFinish(Window _) =>
            this.StepIndex == 2;

        private void Finish(Window window)
        {
            var exts = this.ExtensionsText?.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().TrimStart('.')).ToArray();

            if (exts == null || exts.Length == 0 || exts.Any(e => e.Any(ch => !char.IsLetterOrDigit(ch))))
            {
                MessageBox.Show("Invalid extensions.");
                return;
            }

            if (!File.Exists(this.ProgramPath))
            {
                MessageBox.Show("Program file does not exist.");
                return;
            }

            this.Definition = new CustomConverterDefinition
            {
                Name = this.Name,
                Extensions = exts,
                OutputExtension = this.OutputExtension?.TrimStart('.'),
                ProgramPath = this.ProgramPath,
                Arguments = this.Arguments,
                MultiFileAtOnce = this.MultiFileAtOnce,
                ShowProgram = this.ShowProgram,
                PostAction = this.PostAction,
                OutputTemplate = this.OutputTemplate,
                IconPath = this.IconPath,
                PostProcessCommand = this.PostProcessCommand,
            };
            foreach (var opt in this.options)
            {
                this.Definition.Options.Add(opt);
            }
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void AddOption()
        {
            this.options.Add(new CustomConverterOptionDefinition { ElementName = "Option" });
        }

        private void RemoveOption(CustomConverterOptionDefinition option)
        {
            if (option != null)
            {
                this.options.Remove(option);
            }
        }

        private void EditCurrentOption()
        {
            if (this.SelectedOption == null) return;
            var vm = new CustomOptionEditorViewModel(this.SelectedOption);
            var wnd = new Views.CustomOptionEditor { DataContext = vm };
            wnd.ShowDialog();
        }

        private void RemoveCurrentOption()
        {
            if (this.SelectedOption != null)
            {
                this.options.Remove(this.SelectedOption);
            }
        }
    }
}
