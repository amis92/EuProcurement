using System.Collections.Generic;
using System.Xml.Serialization;

namespace EuProcurement.Uwp
{
    [XmlRoot("CPV_CODE", Namespace = "")]
    public class CpvCatalogueXml
    {
        [XmlElement("CPV")]
        public List<CpvDescriptorXml> Descriptors { get; set; }
    }

    [XmlType("CPV")]
    public class CpvDescriptorXml
    {
        [XmlAttribute("CODE")]
        public string CpvCode { get; set; }

        [XmlElement("TEXT")]
        public List<CpvNameTranslationXml> Translations { get; set; }
    }

    [XmlType("TEXT")]
    public class CpvNameTranslationXml
    {
        [XmlAttribute("LANG")]
        public string LanguageCode { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
