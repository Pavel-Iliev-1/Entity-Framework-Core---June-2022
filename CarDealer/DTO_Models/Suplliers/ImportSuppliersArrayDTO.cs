using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.DTO_Models.Suplliers
{
    [XmlRoot("Suppliers")]
    public class ImportSuppliersArrayDTO
    {
        [XmlElement("Supplier")]
        public ImportSupllierDTO[] Importets { get; set; }
    }
}
