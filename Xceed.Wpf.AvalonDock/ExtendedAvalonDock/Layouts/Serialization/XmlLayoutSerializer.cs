using System.IO;
using System.Xml.Serialization;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Layouts.Serialization
{
    public class XmlLayoutSerializer : LayoutSerializer
    {
        public XmlLayoutSerializer(ExtendedDocingManager manager)
            : base(manager)
        { 
        
        }

        public void Serialize(System.Xml.XmlWriter writer)
        {
            var serializer = new XmlSerializer(typeof(LayoutRoot));
            serializer.Serialize(writer, Manager.Layout);
        }
        public void Serialize(System.IO.TextWriter writer)
        {
            var serializer = new XmlSerializer(typeof(LayoutRoot));
            serializer.Serialize(writer, Manager.Layout);
        }
        public void Serialize(System.IO.Stream stream)
        {
            var serializer = new XmlSerializer(typeof(LayoutRoot));
            serializer.Serialize(stream, Manager.Layout);
        }

        public void Serialize(string filepath)
        {
            using (var stream = new StreamWriter(filepath))
                Serialize(stream);
        }

        public void Deserialize(System.IO.Stream stream)
        {
            try
            {
                StartDeserialization();
                var serializer = new XmlSerializer(typeof(LayoutRoot));
                var layout = serializer.Deserialize(stream) as LayoutRoot;
                FixupLayout(layout);
                Manager.Layout = layout;
            }
            finally
            {
                EndDeserialization();
            }
        }

        public void Deserialize(System.IO.TextReader reader)
        {
            try
            {
                StartDeserialization();
                var serializer = new XmlSerializer(typeof(LayoutRoot));
                var layout = serializer.Deserialize(reader) as LayoutRoot;
                FixupLayout(layout);
                Manager.Layout = layout;
            }
            finally
            {
                EndDeserialization();
            }
        }

        public void Deserialize(System.Xml.XmlReader reader)
        {
            try
            {
                StartDeserialization();
                var serializer = new XmlSerializer(typeof(LayoutRoot));
                var layout = serializer.Deserialize(reader) as LayoutRoot;
                FixupLayout(layout);
                Manager.Layout = layout;
            }
            finally
            {
                EndDeserialization();
            }
        }

        public void Deserialize(string filepath)
        {
            using (var stream = new StreamReader(filepath))
                Deserialize(stream);
        }
    }
}
