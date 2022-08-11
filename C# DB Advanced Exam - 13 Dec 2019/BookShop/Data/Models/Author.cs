using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace BookShop.Data.Models
{
    public class Author
    {
        public Author()
        {
            this.AuthorsBooks = new List<AuthorBook>();
        }
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(Common.AuthorFirstNameMaxLenght)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(Common.AuthorLastNameMaxLenght)]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        public ICollection<AuthorBook> AuthorsBooks { get; set; }
    }
}
