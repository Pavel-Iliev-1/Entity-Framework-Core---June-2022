namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Books");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportBooks[]), rootAtr);

            using StringReader streamReader = new StringReader(xmlString);

            ImportBooks[] booksDTO = (ImportBooks[])serializer.Deserialize(streamReader);

            var books = new List<Book>();

            StringBuilder sb = new StringBuilder();

            foreach (var bDto in booksDTO)
            {

                if (!IsValid(bDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!Enum.IsDefined(typeof(Genre), bDto.Genre))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime date ;

                if (!DateTime.TryParseExact(bDto.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture,DateTimeStyles.None, out DateTime dateResult ))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                else
                {
                    date = dateResult;
                }

                Book book = new Book()
                {
                    Name = bDto.Name,
                    Genre = (Genre)bDto.Genre,
                    Price = bDto.Price,
                    Pages = bDto.Pages,
                    PublishedOn = date
                };

                books.Add(book);


                sb.AppendLine($"Successfully imported book {bDto.Name} for {bDto.Price:F2}.");

            }

            context.Books.AddRange(books);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            ImportAuthor[] autors = JsonConvert.DeserializeObject<ImportAuthor[]>(jsonString);

            var authorDB = new List<Author>();

            StringBuilder sb = new StringBuilder();

            foreach (var aDto in autors)
            {
                if (!IsValid(aDto))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                bool emailExist = authorDB.FirstOrDefault(x => x.Email == aDto.Email) != null;

                if (emailExist)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                Author author = new Author()
                {
                    FirstName = aDto.FirstName,
                    LastName = aDto.LastName,
                    Phone = aDto.Phone,
                    Email = aDto.Email
                };

               
                foreach (var ab in aDto.Books)
                {

                    var book = context.Books.Find(ab.Id);

                    if (book == null)
                    {
                        sb.AppendLine("Invalid data!");
                        continue;
                    }

                    AuthorBook authorBook1 = new AuthorBook()
                    {
                        Author = author,
                        Book = book
                    };

                    author.AuthorsBooks.Add(authorBook1);

                }

                if (author.AuthorsBooks.Count == 0)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                authorDB.Add(author);

                sb.AppendLine($"Successfully imported author - {author.FirstName + " " + author.LastName} with {author.AuthorsBooks.Count} books. ");

            }

            context.Authors.AddRange(authorDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}