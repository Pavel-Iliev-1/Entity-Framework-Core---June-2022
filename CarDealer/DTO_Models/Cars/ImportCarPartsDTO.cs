using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.DTO_Models.Cars
{
    [Serializable]
    [XmlType("partId")]
    public class ImportCarPartsDTO
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
    }
}
