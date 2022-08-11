using BookShop.Data.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace BookShop.DataProcessor.ImportDto
{
    [XmlType("Book")]
    public class ImportBooks
    {
        [XmlElement("Name")]
        [MaxLength(Common.BookMaxLenght)]
        [MinLength(Common.BookMinLenght)]
        public string Name { get; set; }

        [XmlElement("Genre")]
        public int Genre { get; set; }

        [XmlElement("Price")]
        [Range(typeof(decimal),Common.PriceMinValue, Common.PriceMaxValue)]
        public decimal Price { get; set; }

        [XmlElement("Pages")]
        [Range(Common.PageMin, Common.PageMax)]
        public int Pages { get; set; }

        [XmlElement("PublishedOn")]
        public string PublishedOn { get; set; }

    }
}
