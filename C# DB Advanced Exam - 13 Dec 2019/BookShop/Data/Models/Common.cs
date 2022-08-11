using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Data.Models
{
    public static class Common
    {
        public const int AuthorFirstNameMinLenght = 3;
        public const int AuthorFirstNameMaxLenght = 30;

        public const int AuthorLastNameMinLenght = 3;
        public const int AuthorLastNameMaxLenght = 30;

        public const int BookMinLenght = 3;
        public const int BookMaxLenght = 30;

        public const string PriceMinValue = "0";
        public const string PriceMaxValue = "79228162514264337593543950335";


        public const int PageMin = 50;
        public const int PageMax = 5000;

        public const string ValidateMail= @"/^[^@]+@[^@]+\.[^@]+$";

        public const string ValidatePhone= @"^\d{3}-\d{3}-\d{4}$";



    }
}
