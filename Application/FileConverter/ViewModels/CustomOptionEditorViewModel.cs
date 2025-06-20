using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileConverter.CustomConverters;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FileConverter.ViewModels
{
    public class CustomOptionEditorViewModel : ObservableObject
    {
        private readonly CustomConverterOptionDefinition option;
        public CustomOptionEditorViewModel(CustomConverterOptionDefinition option)
        {
            this.option = option;
            this.OkCommand = new RelayCommand<Window>(w => { if (w != null) { w.DialogResult = true; w.Close(); } });
        }

        public RelayCommand<Window> OkCommand { get; }

        public string ElementName
        {
            get => this.option.ElementName;
            set
            {
                if (this.option.ElementName != value)
                {
                    this.option.ElementName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => this.option.Name;
            set
            {
                if (this.option.Name != value)
                {
                    this.option.Name = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string ControlType
        {
            get => this.option.ControlType;
            set
            {
                if (this.option.ControlType != value)
                {
                    this.option.ControlType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Scale
        {
            get => this.option.Scale;
            set
            {
                if (this.option.Scale != value)
                {
                    this.option.Scale = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string ValueType
        {
            get => this.option.ValueType;
            set
            {
                if (this.option.ValueType != value)
                {
                    this.option.ValueType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string CheckedValue
        {
            get => this.option.CheckedValue;
            set
            {
                if (this.option.CheckedValue != value)
                {
                    this.option.CheckedValue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string UncheckedValue
        {
            get => this.option.UncheckedValue;
            set
            {
                if (this.option.UncheckedValue != value)
                {
                    this.option.UncheckedValue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Group
        {
            get => this.option.Group;
            set
            {
                if (this.option.Group != value)
                {
                    this.option.Group = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Category
        {
            get => this.option.Category;
            set
            {
                if (this.option.Category != value)
                {
                    this.option.Category = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string ResourceKey
        {
            get => this.option.ResourceKey;
            set
            {
                if (this.option.ResourceKey != value)
                {
                    this.option.ResourceKey = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Condition
        {
            get => this.option.Condition;
            set
            {
                if (this.option.Condition != value)
                {
                    this.option.Condition = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string DefaultValue
        {
            get => this.option.DefaultValue;
            set
            {
                if (this.option.DefaultValue != value)
                {
                    this.option.DefaultValue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string ItemsText
        {
            get
            {
                if (this.option.Items == null || this.option.Items.Count == 0) return string.Empty;
                return string.Join(";", this.option.Items.Select(i => i.Text + ":" + i.Value));
            }
            set
            {
                var list = new List<CustomConverterOptionEntry>();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    foreach (var entry in value.Split(';'))
                    {
                        var parts = entry.Split(':');
                        if (parts.Length > 0)
                        {
                            list.Add(new CustomConverterOptionEntry { Text = parts[0].Trim(), Value = parts.Length > 1 ? parts[1].Trim() : parts[0].Trim() });
                        }
                    }
                }
                this.option.Items = list;
                this.OnPropertyChanged();
            }
        }
    }
}
