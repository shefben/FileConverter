using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FileConverter.CustomConverters;

namespace FileConverter.ViewModels
{
    public abstract class CustomConverterTreeItem : ObservableObject
    {
        private CustomConverterFolder parent;
        public CustomConverterFolder Parent
        {
            get => this.parent;
            set => this.SetProperty(ref this.parent, value);
        }

        public abstract string Name { get; set; }
    }

    public class CustomConverterFolder : CustomConverterTreeItem
    {
        private string name;
        private ObservableCollection<CustomConverterTreeItem> children = new ObservableCollection<CustomConverterTreeItem>();

        public CustomConverterFolder(string name, CustomConverterFolder parent = null)
        {
            this.Name = name;
            this.Parent = parent;
        }

        public ObservableCollection<CustomConverterTreeItem> Children
        {
            get => this.children;
            set => this.SetProperty(ref this.children, value);
        }

        public override string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }
    }

    public class CustomConverterNode : CustomConverterTreeItem
    {
        private CustomConverterDefinition converter;

        public CustomConverterNode(CustomConverterDefinition def, CustomConverterFolder parent = null)
        {
            this.converter = def;
            this.Parent = parent;
        }

        public CustomConverterDefinition Converter
        {
            get => this.converter;
            set => this.SetProperty(ref this.converter, value);
        }

        public override string Name
        {
            get => this.converter?.Name;
            set
            {
                if (this.converter != null)
                {
                    this.converter.Name = value;
                }
                this.OnPropertyChanged();
            }
        }
    }
}
