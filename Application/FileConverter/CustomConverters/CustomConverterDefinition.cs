using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace FileConverter.CustomConverters
{
    [XmlRoot("CustomConverter")]
    public class CustomConverterDefinition : IXmlSerializable
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlArray("Extensions")]
        [XmlArrayItem("Extension")]
        public string[] Extensions { get; set; }

        [XmlElement("OutputExtension")]
        public string OutputExtension { get; set; }

        [XmlElement("Program")]
        public string ProgramPath { get; set; }

        [XmlElement("Arguments")]
        public string Arguments { get; set; }

        [XmlElement("OutputTemplate")]
        public string OutputTemplate { get; set; }

        [XmlElement("Icon")]
        public string IconPath { get; set; }

        [XmlIgnore]
        public string IconAbsolutePath
        {
            get
            {
                if (string.IsNullOrEmpty(this.IconPath))
                {
                    return null;
                }

                string directory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "CustomConverters");
                return System.IO.Path.Combine(directory, this.IconPath);
            }
        }

        [XmlElement("PostProcessCommand")]
        public string PostProcessCommand { get; set; }

        [XmlElement("MultiFileAtOnce")]
        public bool MultiFileAtOnce { get; set; }

        [XmlElement("ShowProgram")]
        public bool ShowProgram { get; set; } = true;

        [XmlElement("PostAction")]
        public InputPostConversionAction PostAction { get; set; }

        [XmlAnyElement]
        public XmlElement[] CustomOptions { get; set; }

        [XmlIgnore]
        public Dictionary<string, string> Variables { get; private set; } = new Dictionary<string, string>();

        [XmlIgnore]
        public List<CustomConverterOptionDefinition> Options { get; private set; } = new List<CustomConverterOptionDefinition>();

        public void OnDeserializationComplete()
        {
            if (this.CustomOptions != null)
            {
                foreach (XmlElement element in this.CustomOptions)
                {
                    if (string.Equals(element.Name, "Variable", System.StringComparison.OrdinalIgnoreCase))
                    {
                        string name = element.GetAttribute("name");
                        string value = element.GetAttribute("value");
                        if (!string.IsNullOrEmpty(name))
                        {
                            this.Variables[name] = value;
                        }
                    }
                    else
                    {
                        this.Options.Add(CustomConverterOptionDefinition.FromXml(element));
                    }
                }
            }
        }
    }
}
