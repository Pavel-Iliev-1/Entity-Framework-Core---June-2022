using CarDealer.Data;
using System.IO;
using System;
using System.Xml.Serialization;
using CarDealer.DTO_Models.Suplliers;
using CarDealer.DTO_Models.Parts;
using CarDealer.Models;
using System.Collections.Generic;
using System.Linq;
using CarDealer.DTO_Models.Cars;
using CarDealer.DTO_Models.Customers;
using CarDealer.DTO_Models.Sales;
using System.Xml;
using System.Text;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {

            CarDealerContext context = new CarDealerContext();

            string xml = File.ReadAllText("../../../Datasets/sales.xml"); 
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            string result = GetTotalSalesByCustomer(context);

            Console.WriteLine(result);

        }

        // Problem 09

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Suppliers");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportSuppliersArrayDTO), rootAtr);

            using StringReader streamReader = new StringReader(inputXml);

            ImportSuppliersArrayDTO suppliers = (ImportSuppliersArrayDTO)serializer.Deserialize(streamReader);

            ICollection<Supplier> suplArray = new List<Supplier>();
            

            foreach (var sDto in suppliers.Importets)
            {
                Supplier suplier = new Supplier()
                {
                    Name = sDto.Name,
                    IsImporter = sDto.IsImporter
                };

                suplArray.Add(suplier);
            }

            context.AddRange(suplArray);
            context.SaveChanges();

            return $"Successfully imported {suplArray.Count}";
        }

        // Problem 10

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Parts");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportPartsDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(inputXml);

            ImportPartsDTO[] parts = (ImportPartsDTO[])serializer.Deserialize(streamReader);

            var partsDB = new List<Part>(); 

            foreach (var pDto in parts)
            {

                
                if (!context.Suppliers.Any(p => p.Id == pDto.SupplierId))
                {
                    continue;
                }

                Part part = new Part()
                {
                    Name=pDto.Name,
                    Price=pDto.Price,
                    Quantity=pDto.Quantity,
                    SupplierId=pDto.SupplierId
                };

                partsDB.Add(part);  
            }

            context.AddRange(partsDB);
            context.SaveChanges();

            return $"Successfully imported {partsDB.Count}";

        }

        // problem 11

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Cars");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportCarsDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(inputXml);

            ImportCarsDTO[] cars = (ImportCarsDTO[])serializer.Deserialize(streamReader);

            var carsDB = new List<Car>();

            foreach (var cDto in cars)
            {
                var car = new Car()
                {
                    Make = cDto.Make,
                    Model = cDto.Model,
                    TravelledDistance = cDto.TravelledDistance
                };

                var partsDB = new List<PartCar>();

                foreach (var pDto in cDto.Parts.Select(p => p.Id).Distinct())
                {
                    if (!context.Parts.Any(p => p.Id == pDto))
                    {
                        continue;
                    }

                    var part = new PartCar()
                    {
                        Car = car,
                        PartId = pDto
                    };

                    partsDB.Add(part);

                }

                car.PartCars = partsDB;

                carsDB.Add(car);

            }

            context.Cars.AddRange(carsDB);
            context.SaveChanges();

            return $"Successfully imported {carsDB.Count}";

        }

        // Problem 12

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Customers");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportCustomersDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(inputXml);

            ImportCustomersDTO[] cars = (ImportCustomersDTO[])serializer.Deserialize(streamReader);

            var customerDB = new List<Customer>();

            foreach (var cDto in cars)
            {
                var customer = new Customer()
                {
                    Name = cDto.Name,
                    BirthDate = cDto.BirthDate,
                    IsYoungDriver = cDto.IsYoungDriver
                };

                customerDB.Add(customer);
            }

            context.Customers.AddRange(customerDB);
            context.SaveChanges();

            return $"Successfully imported {customerDB.Count}";
        }

        // Problem 13

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute rootAtr = new XmlRootAttribute("Sales");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportSalesDTO[]), rootAtr);

            using StringReader streamReader = new StringReader(inputXml);

            ImportSalesDTO[] salesDTO = (ImportSalesDTO[])serializer.Deserialize(streamReader);

            var salesDB = new List<Sale>();

            foreach (var cDto in salesDTO)
            {
                if (!context.Cars.Any(c => c.Id == cDto.CarId))
                {
                    continue;
                }


                var sale = new Sale()
                {
                    CustomerId = cDto.CustomerId,
                    CarId = cDto.CarId, 
                    Discount = cDto.Discount
                };

                salesDB.Add(sale);
            }

            context.Sales.AddRange(salesDB);
            context.SaveChanges();

            return $"Successfully imported {salesDB.Count}";


        }

        // Problem 14

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder(); 

            ExportCarsDTO[] cars = context.Cars
                .Where(c => c.TravelledDistance > 2000000)
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .Select(c => new ExportCarsDTO
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .ToArray();

            XmlRootAttribute rootAtr = new XmlRootAttribute("cars");

            XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();

            xmlSerializerNamespaces.Add(string.Empty, string.Empty);

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCarsDTO[]), rootAtr);

            using StringWriter writer = new StringWriter(sb); 

            serializer.Serialize(writer, cars); 

            return sb.ToString();   
        }

        // Problem 15

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            ExportCarsBMW[] carsBMW = context.Cars
                .Where(c => c.Make == "BMW")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TravelledDistance)

                .Select(c => new ExportCarsBMW()
                {
                    Id = c.Id,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .ToArray();

            XmlRootAttribute rootAtr = new XmlRootAttribute("cars");

            //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            //namespaces.Add(string.Empty, string.Empty);

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCarsBMW[]), rootAtr);

            using StringWriter writer = new StringWriter(sb);

            serializer.Serialize(writer, carsBMW, ns);

            return sb.ToString();

        }

        // Problem 16

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            ExportLocalSuppliersDTO[] localSuplier = context.Suppliers
                .Where(s => s.IsImporter == false)
                .Select(x => new ExportLocalSuppliersDTO()
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Select(p => p.SupplierId == x.Id).Count()
                })
                .ToArray();


            XmlRootAttribute rootAtr = new XmlRootAttribute("suppliers");

            //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            //namespaces.Add(string.Empty, string.Empty);

            XmlSerializer serializer = new XmlSerializer(typeof(ExportLocalSuppliersDTO[]), rootAtr);

            using StringWriter writer = new StringWriter(sb);

            serializer.Serialize(writer, localSuplier, ns);

            return sb.ToString();

        }

        // Problem 17

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            ExportCarsDTO[] carsList = context.Cars
                .Select(c => new ExportCarsDTO()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance,
                    PartsList = c.PartCars.Select(cp => new ExportPartsDTO()
                    {
                        Name = cp.Part.Name,
                        Price = cp.Part.Price
                    }).OrderBy(p => p.Price).ToArray()

                })
                .OrderByDescending(c => c.TravelledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .ToArray();

            XmlRootAttribute rootAtr = new XmlRootAttribute("cars");

            //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            //namespaces.Add(string.Empty, string.Empty);

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCarsDTO[]), rootAtr);

            using StringWriter writer = new StringWriter(sb);

            serializer.Serialize(writer, carsList, ns);

            return sb.ToString();

        }

        // Problem 18

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            var expCustomerDTO = context.Customers
                .Where(c => c.Sales.Count() > 0)
                .Select(c => new ExportCustomerDTO()
                {
                    Name = c.Name,
                    boughtCars = c.Sales.Count(),
                    SpendMoney = c.Sales
                    .Select(c => c.Car)
                    .SelectMany(c => c.PartCars)
                    .Sum(cp => cp.Part.Price)
                })
                .OrderByDescending(m => m.SpendMoney)
                .ToArray();


            XmlRootAttribute rootAtr = new XmlRootAttribute("customers");

            //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            //namespaces.Add(string.Empty, string.Empty);

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCustomerDTO[]), rootAtr);

            using StringWriter writer = new StringWriter(sb);

            serializer.Serialize(writer, expCustomerDTO, ns);

            return sb.ToString();

        }
    }
}