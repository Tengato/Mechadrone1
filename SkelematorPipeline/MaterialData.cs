using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Skelemator;

namespace SkelematorPipeline
{
    public struct MaterialData
    {
        public string Name;
        public string CustomEffect;
        public List<EffectParam> EffectParams;
        public RenderOptions HandlingFlags;
    }

    public class EffectParam : IXmlSerializable
    {
        public string Name;
        public EffectParamCategory Category;
        public string ValueTypeFullName;
        public object Value;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer strSerializer = new XmlSerializer(typeof(string));
            XmlSerializer epcSerializer = new XmlSerializer(typeof(EffectParamCategory));

            reader.ReadStartElement();
            Name = (string)(strSerializer.Deserialize(reader));
            Category = (EffectParamCategory)(epcSerializer.Deserialize(reader));
            ValueTypeFullName = (string)(strSerializer.Deserialize(reader));

            Type paramType = Type.GetType(ValueTypeFullName);
            XmlSerializer objSerializer = new XmlSerializer(paramType);

            Value = objSerializer.Deserialize(reader);
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer strSerializer = new XmlSerializer(typeof(string));
            XmlSerializer epcSerializer = new XmlSerializer(typeof(EffectParamCategory));
            Type paramType = Type.GetType(ValueTypeFullName);
            XmlSerializer objSerializer = new XmlSerializer(paramType);

            strSerializer.Serialize(writer, Name);
            epcSerializer.Serialize(writer, Category);
            strSerializer.Serialize(writer, ValueTypeFullName);
            objSerializer.Serialize(writer, Value);
        }
    }

    public enum EffectParamCategory
    {
        OpaqueData,
        Texture,
    }
}
