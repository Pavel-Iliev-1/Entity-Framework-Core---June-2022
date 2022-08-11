using BookShop.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace BookShop.DataProcessor.ImportDto
{
    public class ImportAuthor
    {
        [Required]
        [JsonPropertyName("FirstName")]
        [MaxLength(Common.AuthorFirstNameMaxLenght)]
        [MinLength(Common.AuthorFirstNameMinLenght)]
        
        public string FirstName { get; set; }

        [Required]
        [JsonPropertyName("LastName")]
        [MaxLength(Common.AuthorLastNameMaxLenght)]
        [MinLength(Common.AuthorLastNameMinLenght)]
       
        public string LastName { get; set; }

        [Required]
        [JsonPropertyName("Phone")]
        [RegularExpression(Common.ValidatePhone)]
        public string Phone { get; set; }

        [Required]
        //[RegularExpression(Common.ValidateMail)]
        [EmailAddress]
        public string Email { get; set; }
        public ImportBookToAutor[] Books { get; set; }
    }
}
