using System.Xml.Serialization;

namespace Footballers.DataProcessor.ExportDto
{
    [XmlType("Coach")]
    public class ExportCachesDTO
    {
        [XmlAttribute("FootballersCount")]
        public string FootballersCount { get; set; }

        [XmlElement("CoachName")]
        public string CoachName { get; set; }

        [XmlArray("Footballers")]
        public ExportFootbalerDTO[] Footballers { get; set; }
    }
}
