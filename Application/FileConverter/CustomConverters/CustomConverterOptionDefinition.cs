using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using FileConverter.Properties;

namespace FileConverter.CustomConverters
{
    public class CustomConverterOptionEntry
    {
        [XmlAttribute("text")]
        public string Text { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }

    public class CustomConverterOptionDefinition
    {
        [XmlIgnore]
        public string ElementName { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("control")]
        public string ControlType { get; set; }

        [XmlAttribute("scale")]
        public string Scale { get; set; }

        [XmlAttribute("value-type")]
        public string ValueType { get; set; }

        [XmlAttribute("checked-value")]
        public string CheckedValue { get; set; }

        [XmlAttribute("unchecked-value")]
        public string UncheckedValue { get; set; }

        [XmlAttribute("group")]
        public string Group { get; set; }

        [XmlAttribute("category")]
        public string Category { get; set; }

        [XmlAttribute("resource-key")]
        public string ResourceKey { get; set; }

        [XmlAttribute("condition")]
        public string Condition { get; set; }

        [XmlAttribute("value")]
        public string DefaultValue { get; set; }

        [XmlIgnore]
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ResourceKey))
                {
                    string localized = Properties.Resources.ResourceManager.GetString(this.ResourceKey);
                    return string.IsNullOrEmpty(localized) ? this.Name : localized;
                }

                return this.Name;
            }
        }

        [XmlElement("Item")]
        public List<CustomConverterOptionEntry> Items { get; set; } = new List<CustomConverterOptionEntry>();

        public bool EvaluateCondition(ConversionPreset preset)
        {
            if (string.IsNullOrEmpty(this.Condition) || preset == null)
            {
                return true;
            }

            string condition = this.Condition;
            bool notEquals = condition.Contains("!=");
            string[] parts = condition.Split(notEquals ? new[] { "!=" } : new[] { "=" }, 2, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return true;
            }

            string value = preset.GetSettingsValue(parts[0].Trim()) ?? string.Empty;
            string expected = parts[1].Trim();
            bool equals = string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
            return notEquals ? !equals : equals;
        }

        internal static CustomConverterOptionDefinition FromXml(XmlElement element)
        {
            var opt = new CustomConverterOptionDefinition
            {
                ElementName = element.Name,
                Name = element.GetAttribute("name"),
                ControlType = element.GetAttribute("control"),
                Scale = element.GetAttribute("scale"),
                ValueType = element.GetAttribute("value-type"),
                CheckedValue = element.GetAttribute("checked-value"),
                UncheckedValue = element.GetAttribute("unchecked-value"),
                Group = element.GetAttribute("group"),
                Category = element.GetAttribute("category"),
                ResourceKey = element.GetAttribute("resource-key"),
                Condition = element.GetAttribute("condition"),
                DefaultValue = element.GetAttribute("value")
            };

            foreach (XmlNode child in element.ChildNodes)
            {
                if (child is XmlElement childElement)
                {
                    if (string.Equals(childElement.Name, "Item", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(childElement.Name, "Entry", StringComparison.OrdinalIgnoreCase))
                    {
                        opt.Items.Add(new CustomConverterOptionEntry
                        {
                            Text = childElement.InnerText,
                            Value = childElement.GetAttribute("value")
                        });
                    }
                }
            }

            return opt;
        }
    }
}
