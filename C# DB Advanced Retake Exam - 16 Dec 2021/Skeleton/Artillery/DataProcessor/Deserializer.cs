namespace Artillery.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Artillery.Data;
    using Artillery.Data.Models;
    using Artillery.Data.Models.Enums;
    using Artillery.DataProcessor.ImportDto;
    using Microsoft.EntityFrameworkCore.Internal;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage =
                "Invalid data.";
        private const string SuccessfulImportCountry =
            "Successfully import {0} with {1} army personnel.";
        private const string SuccessfulImportManufacturer =
            "Successfully import manufacturer {0} founded in {1}.";
        private const string SuccessfulImportShell =
            "Successfully import shell caliber #{0} weight {1} kg.";
        private const string SuccessfulImportGun =
            "Successfully import gun {0} with a total weight of {1} kg. and barrel length of {2} m.";

        public static string ImportCountries(ArtilleryContext context, string xmlString)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Countries");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportCountriesDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(xmlString);

            ImportCountriesDTO[] countries = (ImportCountriesDTO[])serializer.Deserialize(streamReader);

            var countriesDB = new HashSet<Country>();

            StringBuilder sb = new StringBuilder();

            foreach (var cDto in countries)
            {
                if (!IsValid(cDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                countriesDB.Add(new Country
                {
                    CountryName = cDto.CountryName,
                    ArmySize = cDto.ArmySize
                });

                sb.AppendLine(String.Format(SuccessfulImportCountry, cDto.CountryName, cDto.ArmySize));

            }

            context.Countries.AddRange(countriesDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Manufacturers");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportManufactorerDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(xmlString);

            ImportManufactorerDTO[] manufactors = (ImportManufactorerDTO[])serializer.Deserialize(streamReader);

            var manufactorerDB = new HashSet<Manufacturer>();

            StringBuilder sb = new StringBuilder();

            foreach (var mDto in manufactors)
            {
                if (!IsValid(mDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var manufactorerName = manufactorerDB.FirstOrDefault(m => m.ManufacturerName == mDto.ManufacturerName) != null;

                if (manufactorerName)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                manufactorerDB.Add(new Manufacturer
                {
                    ManufacturerName = mDto.ManufacturerName,
                    Founded = mDto.Founded
                });

                var townCountry = mDto.Founded.Split(", ");
                var townName = townCountry[townCountry.Length - 2];
                var townAddress = townCountry[townCountry.Length - 1];

                sb.AppendLine($"Successfully import manufacturer {mDto.ManufacturerName} founded in {townName}, {townAddress}.");

            }

            context.Manufacturers.AddRange(manufactorerDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Shells");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportShellsDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(xmlString);

            ImportShellsDTO[] shellsDTO = (ImportShellsDTO[])serializer.Deserialize(streamReader);

            var shellsDB = new HashSet<Shell>();

            StringBuilder sb = new StringBuilder();

            foreach (var sDto in shellsDTO)
            {
                if (!IsValid(sDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                shellsDB.Add(new Shell
                {
                    ShellWeight = sDto.ShellWeight,
                    Caliber = sDto.Caliber
                });

                sb.AppendLine(String.Format(SuccessfulImportShell, sDto.Caliber, sDto.ShellWeight));
            }

            context.Shells.AddRange(shellsDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            ImportGunsDTO[] gunsDTO = JsonConvert
                .DeserializeObject<ImportGunsDTO[]>(jsonString);

            var gunsDB = new HashSet<Gun>();

            StringBuilder sb = new StringBuilder();

            foreach (var gDto in gunsDTO)
            {
                if (!IsValid(gDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var checkManufId = context.Manufacturers.FirstOrDefault(x => x.Id == gDto.ManufacturerId) != null;

                if (!checkManufId)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                GunType gunType;

                var gunT = Enum.TryParse(gDto.GunType, out gunType);

                if (!gunT)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Gun currentGun = new Gun()
                {
                    ManufacturerId = gDto.ManufacturerId,
                    GunWeight = gDto.GunWeight,
                    BarrelLength = gDto.BarrelLength,
                    NumberBuild = gDto.NumberBuild,
                    Range = gDto.Range,
                    GunType = gunType,
                    ShellId = gDto.ShellId

    };

                    foreach (var cg in gDto.Countries)
                    {
                        CountryGun countryGun = new CountryGun()
                        {
                            CountryId = cg.Id,
                            Gun = currentGun
                        };

                    currentGun.CountriesGuns.Add(countryGun);

                    }

                gunsDB.Add(currentGun);

                sb.AppendLine(String.Format(SuccessfulImportGun, gDto.GunType, gDto.GunWeight, gDto.BarrelLength));

            }

            context.Guns.AddRange(gunsDB);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }
        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
