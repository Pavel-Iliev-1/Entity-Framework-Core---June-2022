using System.Xml.Serialization;

namespace CarDealer.DTO_Models.Suplliers
{
    [XmlType("Supplier")]
    public class ImportSupllierDTO
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("isImporter")]
        public bool IsImporter { get; set; }
    }
}
