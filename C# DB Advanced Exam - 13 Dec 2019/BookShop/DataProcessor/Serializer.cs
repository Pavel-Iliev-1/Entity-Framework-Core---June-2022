namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var crazyAutors = context.Authors
                .Select(a => new
                {

                    AuthorName = a.FirstName + " " + a.LastName,
                    Books = a.AuthorsBooks
                    .OrderByDescending(b => b.Book.Price)
                    .Select(ab => new
                    {

                        BookName = ab.Book.Name,
                        BookPrice = ab.Book.Price.ToString("f2")
                    })
                    .ToArray() 
                })
                .ToArray()
                .OrderByDescending(a => a.Books.Length)
                .ThenBy(a => a.AuthorName)
                .ToList();

            string jsonString = JsonConvert.SerializeObject(crazyAutors, Formatting.Indented);

            return jsonString;
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var books = context.Books
                .Where(b => b.PublishedOn < date && b.Genre == Genre.Science)
                .OrderByDescending(b => b.Pages)
                .ThenByDescending(b => b.PublishedOn)
                .Select(b => new ExportBooks
                {
                    Pages = b.Pages,
                    Name = b.Name,
                    Date = b.PublishedOn.ToString("d", CultureInfo.InvariantCulture)
                })
                .Take(10)
                .ToArray()

                .ToArray();

            var result = SerializeXml(books, "Books");

            return result;
        }

        private static string SerializeXml<T>(T[] objects, string root)
        {
            var serializer = new XmlSerializer(typeof(T[]), new XmlRootAttribute(root));
            var namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName() });

            var sb = new StringBuilder();

            serializer.Serialize(new StringWriter(sb), objects, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}