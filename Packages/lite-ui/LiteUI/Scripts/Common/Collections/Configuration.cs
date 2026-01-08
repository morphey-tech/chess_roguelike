using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using static LiteUI.Common.Preconditions;

namespace LiteUI.Common.Collections
{
    public class Configuration
    {
        private const string ROOT_ELEMENT = "config";

        private readonly Dictionary<string, object?> _values;

        public Configuration()
        {
            _values = new Dictionary<string, object?>();
        }

        public static XmlDocument CreateXmlDocument(string xml)
        {
            string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (xml.StartsWith(byteOrderMarkUtf8, StringComparison.Ordinal)) {
                xml = xml.Remove(0, byteOrderMarkUtf8.Length);
            }
            xml = xml.Trim();
            
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            return xmlDocument;
        }
        
        public static Configuration CreateFromXml(string xml)
        {
            XmlDocument xmlDocument = CreateXmlDocument(xml);
            
            Configuration configuration = new Configuration();
            configuration.LoadXml(xmlDocument);
            return configuration;
        }
        
        public static Configuration CreateFromXml(XmlDocument xmlDocument)
        {
            Configuration configuration = new Configuration();
            configuration.LoadXml(xmlDocument);
            return configuration;
        } 
        
        public void LoadXml(XmlDocument doc)
        {
            LoadXml(doc.GetElementsByTagName(ROOT_ELEMENT)[0]);
        }

        public void LoadXml(XmlNode xml)
        {
            XmlAttributeCollection? attributes = xml.Attributes;
            if (attributes != null) {
                foreach (XmlAttribute xmlAttribute in attributes) {
                    string attributeName = xmlAttribute.Name;
                    _values[attributeName] = xmlAttribute.InnerText;
                }
            }

            if (!string.IsNullOrEmpty(xml.InnerText)) {
                _values[""] = xml.InnerText;
            }

            List<string> elementNames = new List<string>();
            foreach (XmlNode childNode in xml.ChildNodes) {
                if (childNode is XmlElement && !elementNames.Contains(childNode.Name)) {
                    elementNames.Add(childNode.Name);
                }
            }

            foreach (string elementName in elementNames) {
                XmlNodeList? elements = xml.SelectNodes(elementName);
                if (elements == null) {
                    continue;
                }

                List<object>? list = new();
                foreach (XmlNode xmlNode in elements) {
                    if (xmlNode.Attributes != null && xmlNode.Attributes.Count > 0 || IsComplexContent(xmlNode)) {
                        Configuration childConfiguration = new();
                        childConfiguration.LoadXml(xmlNode);
                        list.Add(childConfiguration);
                    } else {
                        list.Add(xmlNode.InnerText);
                    }
                }

                object? addElement = elements.Count > 1 ? list : list[0];
                _values.Add(elementName, addElement);
            }
        }

        public bool GetBoolean(string key, bool defaultValue = false)
        {
            string? value = GetString(key);
            return value == null ? defaultValue : Convert.ToBoolean(value);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            string? value = GetString(key);
            return value == null ? defaultValue : Convert.ToInt32(value);
        }

        public long GetLong(string key, long defaultValue = 0)
        {
            string? value = GetString(key);
            return value == null ? defaultValue : Convert.ToInt64(value);
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            string? value = GetString(key);
            return value == null ? defaultValue : Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        [ContractAnnotation("defaultValue:null=> canbenull")]
        public string? GetString(string key, string? defaultValue = null)
        {
            string? value = GetValue(key) as string;
            return value ?? defaultValue;
        }        
        
        public string RequireString(string key, string? defaultValue = null)
        {
            return CheckNotNull(GetString(key, defaultValue))!;
        }

        public Configuration? GetConfiguration(string key)
        {
            return GetValue(key) as Configuration;
        }

        public Configuration RequireConfiguration(string key)
        {
            return CheckNotNull(GetConfiguration(key))!;
        }

        public List<object> GetList(string key)
        {
            object? value = GetValue(key);
            List<object>? list = value as List<object>;
            return list ?? new List<object> {value ?? throw new InvalidOperationException()};
        }

        public List<T> GetList<T>(string key)
        {
            List<T> result = new List<T>();

            object? value = GetValue(key);
            if (value is List<object> objects) {
                foreach (object item in objects) {
                    result.Add((T) item);
                }
            } else if (value != null) {
                result.Add((T) value);
            }
            return result;
        }

        public List<string> GetNames()
        {
            string[] keys = new string[_values.Keys.Count];
            _values.Keys.CopyTo(keys, 0);
            return keys.ToList();
        }

        public object? GetValue(string key)
        {
            List<string> path = key.Split('.').ToList();

            string current = path[0];
            path.RemoveAt(0);

            if (path.Count == 0) {
                return _values.ContainsKey(current) ? _values[current] : null;
            }

            object? node = _values[current];
            return (node as Configuration)?.GetValue(string.Join(".", path.ToArray()));
        }

        public void SetValue(string key, object? value)
        {
            if (value is Configuration) {
                _values[key] = value;
            } else {
                _values[key] = "" + value;
            }
        }

        public override string ToString()
        {
            XmlDocument doc = ToXml();
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            doc.WriteTo(tx);

            string str = sw.ToString();
            return str;
        }

        public void Override(Configuration newConfig)
        {
            ValuesToConfig(this, newConfig);
        }

        private XmlDocument ToXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement config = doc.CreateElement("config");
            doc.AppendChild(config);
            ValuesToXml(config);
            return doc;
        }

        #region INTERNAL

        private void ValuesToConfig(Configuration oldConfig, Configuration overrideConfig)
        {
            foreach (string key in overrideConfig._values.Keys) {
                ValueToConfig(oldConfig, key, overrideConfig._values[key]);
            }
        }

        private void ValueToConfig(Configuration oldConfig, string key, object? newValue)
        {
            if (oldConfig._values.ContainsKey(key)) {
                oldConfig._values[key] = null;
            } else {
                oldConfig._values.Add(key, null);
            }

            switch (newValue) {
                case Configuration configuration: {
                    Configuration? newConfiguration = new Configuration();
                    newConfiguration.Override(configuration);
                    oldConfig._values[key] = newConfiguration;
                    break;
                }
                case List<object> listConfiguration: {
                    List<object?> list = new List<object?>();
                    Configuration listConfig = new Configuration();
                    foreach (object? item in listConfiguration) {
                        ValueToConfig(listConfig, key, item);
                        list.Add(listConfig._values[key]);
                    }
                    oldConfig._values[key] = list;
                    break;
                }
                default:
                    oldConfig._values[key] = newValue;
                    break;
            }
        }

        private void ValuesToXml(XmlNode parent)
        {
            foreach (string key in _values.Keys) {
                ValueToXml(parent, key, _values[key]);
            }
        }

        private bool IsComplexContent(XmlNode xmlNode)
        {
            return xmlNode.ChildNodes.OfType<XmlElement>().Any();
        }

        private void ValueToXml(XmlNode parent, string key, object? value)
        {
            if (parent.OwnerDocument == null) {
                return;
            }
            switch (value) {
                case Configuration configuration: {
                    XmlNode configNode = parent.OwnerDocument.CreateElement(key);
                    configuration.ValuesToXml(configNode);
                    parent.AppendChild(configNode);
                    break;
                }
                case List<object> list: {
                    foreach (object? item in list) {
                        ValueToXml(parent, key, item);
                    }
                    break;
                }
                default: {
                    XmlNode valueNode = parent.OwnerDocument.CreateElement(key);
                    valueNode.InnerText = (string) value;
                    parent.AppendChild(valueNode);
                    break;
                }
            }
        }

        #endregion
    }
}
