using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ES.Common.Helpers
{
    public class XmlManager
    {
        #region Extention Method

        #endregion
        #region Converters
        public static XElement Convert(XmlElement element)
        {
            if (element == null) return null;
            return XElement.Parse(element.OuterXml);
        }

        public static XmlElement Convert(XElement element)
        {
            if (element == null) return null;
            var doc = new XmlDocument();
            doc.LoadXml(element.ToString());
            return doc.DocumentElement;
        }
        public static XmlDocument Convert(XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }
        public static XDocument Convert(XmlDocument xmlDocument)
        {
            using (var memStream = new MemoryStream())
            {
                using (var w = XmlWriter.Create(memStream))
                {
                    xmlDocument.WriteContentTo(w);
                }
                memStream.Seek(0, SeekOrigin.Begin);
                using (var r = XmlReader.Create(memStream))
                {
                    return XDocument.Load(r);
                }
            }
        }
        #endregion
        #region Serialize and Deserialize
        public static XmlElement SerializeToXmlElement(object o)
        {
            XmlDocument doc = new XmlDocument();

            using (XmlWriter writer = doc.CreateNavigator().AppendChild())
            {
                new XmlSerializer(o.GetType()).Serialize(writer, o);
            }

            return doc.DocumentElement;
        }
        public static T DeserializeFromXmlElement<T>(XmlElement element)
        {
            if (element == null) return default(T);
            var serializer = new XmlSerializer(typeof(T));
            try
            {
                return (T) serializer.Deserialize(new XmlNodeReader(element));
            }
            catch (InvalidOperationException)
            {
                return default(T);
            }
        }
        #endregion
        #region Private properties
        private string _FileName = @"ESLConfig.Xml";
        private string _FilePath = Application.StartupPath;
        #endregion
        #region Public properties

        public string GetFilePath
        {
            get { return string.Format("{0}{1}{2}", _FilePath, @"\", _FileName); }
        }

        #endregion

        public XmlManager()
        {
        }

        public XmlManager(string fileName)
        {
            _FileName = fileName;
        }

        public XmlManager(string filePath, string fileName)
        {
            _FilePath = filePath;
            _FileName = fileName;
        }
        #region Public Methods
        public string GetElementInnerText(string element)
        {
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Elements(XmlTagItems.Root);
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == element);
                if (xEl != null)
                {
                    return xEl.Value;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        public bool SetElementInnerText(string value, string element)
        {
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Elements(XmlTagItems.Root);
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == element);
                if (xEl != null)
                {
                    xEl.Value = value;
                    xDoc.Save(_FileName);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public List<XmlSettingsItem> GetItemsByControl(string element)
        {
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Elements(XmlTagItems.Root);
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == element);
                if (xEl == null)
                {
                    return new List<XmlSettingsItem>();
                }
                var items = xEl.Elements().Select(el => new XmlSettingsItem()
                {
                    Key = el.Attribute(XmlSettingsItem.KeyProperty).Value,
                    Data = el.Attribute(XmlSettingsItem.DataProperty).Value,
                    Value = el.Attribute(XmlSettingsItem.ValueProperty).Value,
                    Member = el.Attribute(XmlSettingsItem.MemberProperty).Value
                }).ToList();
                return items;
            }
            catch (Exception)
            {
                return new List<XmlSettingsItem>();
            }
        }
        public bool SetStockItemsByControl(List<XmlSettingsItem> items, string element)
        {
            if (string.IsNullOrEmpty(element))
            {
                return false;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Elements(XmlTagItems.Root);
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == element);
                if (xEl != null)
                {
                    xEl.RemoveAll();
                    foreach (var item in items)
                    {
                        var stock = new XElement(item.Key);
                        stock.SetAttributeValue(XmlSettingsItem.KeyProperty, item.Key);
                        stock.SetAttributeValue(XmlSettingsItem.DataProperty, item.Data);
                        stock.SetAttributeValue(XmlSettingsItem.ValueProperty, item.Value);
                        stock.SetAttributeValue(XmlSettingsItem.MemberProperty, item.Member);
                        xEl.Add(stock);
                    }
                    xDoc.Save(_FileName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        public bool SetStockItemsByControl<T>(List<T> items, string element)
        {
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Elements("Settings");
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == element);
                if (xEl != null)
                {
                    xEl.RemoveAll();
                    var property = items.First().GetType();
                    foreach (var item in items)
                    {

                    }
                    var stock = new XElement("Stock");
                    stock.SetAttributeValue("Key", 122);
                    xEl.Add(stock);
                    xDoc.Save(_FileName);
                }

                //root.AppendChild(stocks);
                //
                //var doc = new XmlDocument();
                //doc.Load("ESLConf.Xml");
                //var node = doc.GetElementsByTagName(control);
                //doc.DocumentElement.SetAttribute("Stock","Key","aa");
                //using (FileStream stream = File.OpenWrite("ESLConf.Xml"))
                //{
                //    //stream.
                //    //;
                //}
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public bool SetStockItemsByControl<T>(T o, string control)
        {
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Elements("Settings");
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == control);
                if (xEl != null)
                {
                    var stock = new XElement("Stock");
                    stock.SetAttributeValue("Key", 120);
                    xEl.Add(stock);
                    xDoc.Save(_FileName);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public bool SetItems(List<XmlSettingsItem> items, string element)
        {
            if (string.IsNullOrEmpty(element))
            {
                return false;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Elements(XmlTagItems.Root);
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == element);
                if (xEl != null)
                {
                    xEl.RemoveAll();
                    foreach (var item in items)
                    {
                        var stock = new XElement(item.Key);
                        stock.SetAttributeValue(XmlSettingsItem.KeyProperty, item.Key);
                        stock.SetAttributeValue(XmlSettingsItem.DataProperty, item.Data);
                        stock.SetAttributeValue(XmlSettingsItem.ValueProperty, item.Value);
                        stock.SetAttributeValue(XmlSettingsItem.MemberProperty, item.Member);
                        xEl.Add(stock);
                    }
                    xDoc.Save(_FileName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public XElement GetXmlElement(string toElement)
        {
            if (string.IsNullOrEmpty(toElement))
            {
                return null;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return null;
                }
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                return xEl;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public XElement GetXmlElement(string fromEl, string element)
        {
            if (string.IsNullOrEmpty(fromEl))
            {
                return null;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return null;
                }
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == fromEl);
                if (xEl == null)
                {
                    xEl = new XElement(fromEl);
                    root.Add(xEl);
                }
                return xEl.Elements().FirstOrDefault(s => s.Name == element);
            }
            catch (Exception)
            {
                return null;
            }

        }
        public XElement GetXmlElementByKey(string toElement, string key)
        {
            if (string.IsNullOrEmpty(toElement) || string.IsNullOrEmpty(key))
            {
                return null;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return null;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                return xEl.Elements().FirstOrDefault(s => s.Attribute("key") != null && string.Equals(s.Attribute("key").Value, key.ToString(), StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception)
            {
                return null;
            }

        }
        public XElement GetXmlElementByKey(string toElement, long memberId)
        {
            if (string.IsNullOrEmpty(toElement) || memberId<1)
            {
                return null;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return null;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                return xEl.Elements().FirstOrDefault(s => s.Attribute("key") != null && string.Equals(s.Attribute("MemberId").Value, memberId.ToString(), StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception)
            {
                return null;
            }

        }
        public List<XElement> GetXmlElements(string toElement)
        {
            if (string.IsNullOrEmpty(toElement))
            {
                return null;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return null;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                return xEl.Elements().ToList();
            }
            catch (Exception)
            {
                return new List<XElement>();
            }

        }
        public List<XElement> GetXmlElements(string toElement, string key)
        {
            if (string.IsNullOrEmpty(toElement) || string.IsNullOrEmpty(key))
            {
                return null;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return null;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                return xEl.Elements().Where(s => s.Attribute("key") != null && string.Equals(s.Attribute("key").Value, key.ToString(), StringComparison.CurrentCultureIgnoreCase)).ToList();
            }
            catch (Exception)
            {
                return null;
            }

        }
        public List<XElement> GetXmlElements(string toElement, long memberId)
        {
            if (string.IsNullOrEmpty(toElement) || memberId<1)
            {
                return null;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return null;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                return xEl.Elements().Where(s => s.Attribute("key") != null && string.Equals(s.Attribute("memberId").Value, memberId.ToString(), StringComparison.CurrentCultureIgnoreCase)).ToList();
            }
            catch (Exception)
            {
                return null;
            }

        }
        public bool SetXmlElement(string toElement, XElement xElement)
        {
            if (string.IsNullOrEmpty(toElement) || xElement == null)
            {
                return false;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return false;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                var el = xEl.Elements().FirstOrDefault();
                if (el != null)
                {
                    el.Remove();
                }
                xEl.Add(xElement);
                xDoc.Save(_FileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public bool AddXmlElement(string toElement, XElement xElement)
        {
            if (string.IsNullOrEmpty(toElement) || xElement == null)
            {
                return false;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return false;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                var xEls = xEl.Elements().ToList();
                if (xEls.All(s => s.Value != xElement.Value))
                {
                    xEl.Add(xElement);
                    xDoc.Save(_FileName);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public bool RemoveXmlElement(string toElement, XElement xElement)
        {
            if (string.IsNullOrEmpty(toElement) || xElement == null)
            {
                return false;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return false;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    return false;
                }
                var el = xEl.Elements().FirstOrDefault(s=>s.Value==xElement.Value);
                if (el!=null)
                {
                    el.Remove();
                    xDoc.Save(_FileName);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public bool SetXmlElement(string toElement, XElement xElement, long memberId)
        {
            if (string.IsNullOrEmpty(toElement) || xElement == null || memberId<1)
            {
                return false;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return false;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                var el = xEl.Elements().FirstOrDefault(s => s.Attribute("key") != null && string.Equals(s.Attribute("MemberId").Value, memberId.ToString(), StringComparison.CurrentCultureIgnoreCase));
                if (el != null)
                {
                    el.Remove();
                }
                xElement.SetAttributeValue("MemberId", memberId);
                xEl.Add(xElement);
                xDoc.Save(_FileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public bool SetXmlElement(string toElement, XElement xElement, string key)
        {
            if (string.IsNullOrEmpty(toElement) || xElement == null)
            {
                return false;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return false;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                var el = xEl.Elements().FirstOrDefault(s => s.Attribute("key") != null && string.Equals(s.Attribute("key").Value, key.ToString(), StringComparison.CurrentCultureIgnoreCase));
                if (el != null)
                {
                    el.Remove();
                }
                xElement.SetAttributeValue("key", key);
                xEl.Add(xElement);
                xDoc.Save(_FileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public bool InsertItems(List<XmlSettingsItem> items, string element)
        {
            if (string.IsNullOrEmpty(element))
            {
                return false;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Elements(XmlTagItems.Root);
                var xEl = root.Descendants().FirstOrDefault(s => s.Name == element);
                if (xEl != null)
                {
                    foreach (var item in items)
                    {
                        var stock = new XElement(item.Key);
                        stock.SetAttributeValue(XmlSettingsItem.KeyProperty, item.Key);
                        stock.SetAttributeValue(XmlSettingsItem.DataProperty, item.Data);
                        stock.SetAttributeValue(XmlSettingsItem.ValueProperty, item.Value);
                        stock.SetAttributeValue(XmlSettingsItem.MemberProperty, item.Member);
                        xEl.Add(stock);
                    }
                    xDoc.Save(_FileName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        public bool RemoveXmlElement(string toElement, XElement xElement, string key)
        {
            if (string.IsNullOrEmpty(toElement) || xElement == null)
            {
                return false;
            }
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var root = xDoc.Element(XmlTagItems.Root);
                if (root == null)
                {
                    return false;
                }

                var xEl = root.Descendants().FirstOrDefault(s => s.Name == toElement);
                if (xEl == null)
                {
                    xEl = new XElement(toElement);
                    root.Add(xEl);
                }
                var el = xEl.Elements().FirstOrDefault(s => s.Attribute("key") != null && string.Equals(s.Attribute("key").Value, key.ToString(), StringComparison.CurrentCultureIgnoreCase));
                if (el != null)
                {
                    el.Remove();
                }
                xDoc.Save(_FileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public List<XmlSettingsItem> GetItemsFromRootByControl(string root, string element)
        {
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var rootPath = xDoc.Elements(XmlTagItems.Root);
                var rootEl = rootPath.Descendants().FirstOrDefault(s => s.Name == root);
                if (rootEl == null) { return new List<XmlSettingsItem>(); }
                var xEl = rootEl.Descendants().FirstOrDefault(s => s.Name == element);
                if (xEl == null)
                {
                    return new List<XmlSettingsItem>();
                }
                var items = xEl.Elements().Select(el => new XmlSettingsItem()
                {
                    Key = el.Attribute(XmlSettingsItem.KeyProperty).Value,
                    Data = el.Attribute(XmlSettingsItem.DataProperty).Value,
                    Value = el.Attribute(XmlSettingsItem.ValueProperty).Value,
                    Member = el.Attribute(XmlSettingsItem.MemberProperty).Value
                }).ToList();
                return items;
            }
            catch (Exception)
            {
                return new List<XmlSettingsItem>();
            }
        }
        public List<XmlSettingsItem> GetItemsByControl(string root, string element)
        {
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var rootPath = xDoc.Elements(XmlTagItems.Root);
                var rootEl = rootPath.Descendants().FirstOrDefault(s => s.Name == root);
                if (rootEl == null) { return new List<XmlSettingsItem>(); }
                var xmlEl = rootEl.Elements().Where(s => s.Name == element).ToList();
                var items = new List<XmlSettingsItem>();
                foreach (var xEl in xmlEl)
                {
                    var newItem = new XmlSettingsItem
                    {
                        Key = xEl.Attribute(XmlSettingsItem.KeyProperty) != null ? xEl.Attribute(XmlSettingsItem.KeyProperty).Value : string.Empty,
                        Data = xEl.Attribute(XmlSettingsItem.DataProperty) != null ? xEl.Attribute(XmlSettingsItem.DataProperty).Value : string.Empty,
                        Value = xEl.Attribute(XmlSettingsItem.ValueProperty) != null ? xEl.Attribute(XmlSettingsItem.ValueProperty).Value : string.Empty,
                        Member = xEl.Attribute(XmlSettingsItem.MemberProperty) != null ? xEl.Attribute(XmlSettingsItem.MemberProperty).Value : string.Empty
                    };
                    foreach (var xElement in xEl.Elements())
                    {
                        newItem.Elements.Add(xElement);
                    }
                    items.Add(newItem);
                }
                return items;
            }
            catch (Exception)
            {
                return new List<XmlSettingsItem>();
            }
        }
        public List<XmlSettingsItem> GetItemsByControl(string root, string element, long memberId)
        {
            try
            {
                var xDoc = XDocument.Load(_FileName);
                var rootPath = xDoc.Elements(XmlTagItems.Root);
                var rootEl = rootPath.Descendants().FirstOrDefault(s => s.Name == root);
                if (rootEl == null) { return new List<XmlSettingsItem>(); }
                var xEl = rootEl.Descendants().FirstOrDefault(s => s.Name == element && s.Attribute(XmlSettingsItem.ValueProperty).Value == memberId.ToString());
                if (xEl == null)
                {
                    return new List<XmlSettingsItem>();
                }
                var items = xEl.Elements().Select(el => new XmlSettingsItem
                {
                    Key = el.Attribute(XmlSettingsItem.KeyProperty).Value,
                    Data = el.Attribute(XmlSettingsItem.DataProperty).Value,
                    Value = el.Attribute(XmlSettingsItem.ValueProperty).Value,
                    Member = el.Attribute(XmlSettingsItem.MemberProperty).Value
                }).ToList();
                return items;
            }
            catch (Exception)
            {
                return new List<XmlSettingsItem>();
            }
        }
        public object GetElementValueByControl(string root, string element)
        {
            var item = GetItemsFromRootByControl(root, element).FirstOrDefault();
            if (item == null)
            {
                return string.Empty;
            }
            return item.Value;
        }

        public object GetElementsByControl(string element)
        {
            return GetItemsByControl(element);
        }

        public static void Save(Object o, String path)
        {
            // Create a new Serializer
            XmlSerializer serializer = new XmlSerializer(o.GetType());

            // Create a new StreamWriter
            TextWriter writer = new StreamWriter(path);

            // Serialize the file
            serializer.Serialize(writer, o);

            // Close the writer
            writer.Close();
        }

        public static object Read(String path, Type type)
        {
            // Create a new serializer
            XmlSerializer serializer = new XmlSerializer(type);

            // Create a StreamReader
            TextReader reader = new StreamReader(path);

            // Deserialize the file
            Object file;
            file = (Object)serializer.Deserialize(reader);

            // Close the reader
            reader.Close();

            // Return the object
            return file;
        }
        #endregion
    }

    public class XmlSettingsItem
    {
        public const string KeyProperty = "Key";
        public const string DataProperty = "Data";
        public const string ValueProperty = "Value";
        public const string MemberProperty = "Member";
        public string Key { get; set; }
        public object Data { get; set; }
        public object Value { get; set; }
        public object Member { get; set; }
        public List<XElement> Elements = new List<XElement>();
    }
    public struct XmlTagItems
    {
        /// <summary>
        /// Settings
        /// </summary>
        public const string Root = "Settings";
        public const string Users = "Users";
        public const string Login = "Login";
        public const string Logins = "Logins";
        public const string LocalMode = "LocalMode";
        /// <summary>
        /// Sale
        /// </summary>
        public const string Store = "Store";
        public const string CashDesk = "CashDesk";
        public const string SaleStocks = "SaleStores";
        public const string SaleCashDesks = "SaleCashDesks";
        public const string EsServers = "ESServers";
        public const string Equipments = "Equipments";
        public const string Weighers = "Weighers";
        public const string Weigher = "Weigher";
        public const string DefaultPrinter = "DefaultPrinter";
        public const string ActivePrinter = "ActivePrinter";
        /// <summary>
        /// Default settings
        /// </summary>
        public const string SaleBySingle = "SaleBySingle";
        public const string BuyBySingle = "BuyBySingle";
        /// <summary>
        /// Printers
        /// </summary>
        public const string SalePrinter = "SalePrinter";
        public const string BarcodePrinter = "BarcodePrinter";
        public const string PrintCashReceipt = "PrintCashReceipt";
        /// <summary>
        /// Cash desk elecric register
        /// </summary>
        #region ECR const tags
        public const string Ecr = "Ecr";
        public const string EcrConfig = "EcrConfig";
        public const string EcrSettings = "EcrSettings";
        #endregion
    }
}
