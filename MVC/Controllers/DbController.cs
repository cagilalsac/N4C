#nullable disable

using APP.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.Controllers;
using N4C.Domain;
using N4C.Domain.Users;
using System.Globalization;
using System.Net;

namespace MVC.Controllers
{
    public class DbController : MvcController
    {
        const int PIN = 2025;

        private readonly AppDb _appDb;
        private readonly Db _db;

        public DbController(IAppDb appDb, IDb db)
        {
            _appDb = appDb as AppDb;
            _db = db as Db;
        }

        public IActionResult Seed(int pin)
        {
            if (pin == PIN)
            {
                if (_appDb.Stores.Any() || _appDb.Products.Any() || _appDb.Categories.Any() || _db.Roles.Any() || _db.Users.Any())
                {
                    _appDb.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('Stores', RESEED, 0)");
                    _appDb.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('Products', RESEED, 0)");
                    _appDb.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('ProductStores', RESEED, 0)");
                    _appDb.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('Categories', RESEED, 0)");
                    _appDb.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('Roles', RESEED, -1)");
                    _appDb.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('Users', RESEED, 0)");
                    _appDb.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('UserRoles', RESEED, 0)");
                }

                _appDb.ProductStores.RemoveRange(_appDb.ProductStores.ToList());
                _appDb.Stores.RemoveRange(_appDb.Stores.ToList());
                _appDb.Products.RemoveRange(_appDb.Products.ToList());
                _appDb.Categories.RemoveRange(_appDb.Categories.ToList());

                _appDb.Stores.Add(new Store()
                {
                    Name = "Hepsiburada",
                    Virtual = true
                });
                _appDb.Stores.Add(new Store()
                {
                    Name = "Vatan",
                    Virtual = false
                });
                _appDb.Stores.Add(new Store()
                {
                    Name = "Migros"
                });
                _appDb.Stores.Add(new Store()
                {
                    Name = "Teknosa"
                });
                _appDb.Stores.Add(new Store()
                {
                    Name = "İtopya"
                });
                _appDb.Stores.Add(new Store()
                {
                    Name = "Sahibinden",
                    Virtual = true
                });

                _appDb.SaveChanges();

                _appDb.Categories.Add(new Category()
                {
                    Name = "Computer",
                    Description = "Laptops, desktops and computer peripherals",
                    Products = new List<Product>()
                    {
                        new Product()
                        {
                            Name = "Laptop",
                            UnitPrice = 3000.5m,
                            StockAmount = 10,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Hepsiburada").Id
                            }
                        },
                        new Product()
                        {
                            Name = "Mouse",
                            UnitPrice = 20.5M,
                            StockAmount = null,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Hepsiburada").Id,
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Vatan").Id
                            }
                        },
                        new Product()
                        {
                            Name = "Keyboard",
                            UnitPrice = 40,
                            StockAmount = 45,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Hepsiburada").Id,
                                _appDb.Stores.SingleOrDefault(s => s.Name == "İtopya").Id,
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Sahibinden").Id
                            }
                        },
                        new Product()
                        {
                            Name = "Monitor",
                            UnitPrice = 2500,
                            StockAmount = 20,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Teknosa").Id,
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Vatan").Id
                            }
                        }
                    }
                });
                _appDb.Categories.Add(new Category()
                {
                    Name = "Home Theater System",
                    Description = null,
                    Products = new List<Product>()
                    {
                        new Product()
                        {
                            Name = "Speaker",
                            UnitPrice = 2500,
                            StockAmount = 70,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Teknosa").Id
                            }
                        },
                        new Product()
                        {
                            Name = "Receiver",
                            UnitPrice = 5000,
                            StockAmount = 30,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Hepsiburada").Id,
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Sahibinden").Id
                            }
                        },
                        new Product()
                        {
                            Name = "Equalizer",
                            UnitPrice = 1000,
                            StockAmount = 40,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                        }
                    }
                });
                _appDb.Categories.Add(new Category()
                {
                    Name = "Phone",
                    Description = "IOS and Android Phones",
                    Products = new List<Product>()
                    {
                        new Product()
                        {
                            Name = "iPhone",
                            UnitPrice = 10000,
                            StockAmount = 20,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Teknosa").Id,
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Vatan").Id,
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Hepsiburada").Id,
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Sahibinden").Id
                            }
                        }
                    }
                });
                _appDb.Categories.Add(new Category()
                {
                    Name = "Food",
                    Products = new List<Product>()
                    {
                        new Product()
                        {
                            Name = "Apple",
                            UnitPrice = 10.5m,
                            StockAmount = 500,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            ExpirationDate = new DateTime(2024, 12, 31),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Migros").Id
                            }
                        },
                        new Product()
                        {
                            Name = "Chocolate",
                            UnitPrice = 2.5M,
                            StockAmount = 125,
                            ExpirationDate = DateTime.Parse("09/18/2025", new CultureInfo("en-US")),
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Migros").Id
                            }
                        }
                    }
                });
                _appDb.Categories.Add(new Category()
                {
                    Name = "Medicine",
                    Description = "Antibiotics, Vitamins, Pain Killers, etc.",
                    Products = new List<Product>()
                    {
                        new Product()
                        {
                            Name = "Antibiotic",
                            UnitPrice = 35,
                            StockAmount = 5,
                            CreateDate = DateTime.Now,
                            CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                            ExpirationDate = DateTime.Parse("19.05.2027", new CultureInfo("tr-TR")),
                            StoreIds = new List<int>()
                            {
                                _appDb.Stores.SingleOrDefault(s => s.Name == "Migros").Id
                            }
                        }
                    }
                });
                _appDb.Categories.Add(new Category()
                {
                    Name = "Software",
                    Description = "Operating Systems, Antivirus Software, Office Software and Video Games"
                });

                _appDb.SaveChanges();

                _db.UserRoles.RemoveRange(_db.UserRoles.ToList());
                _db.Roles.RemoveRange(_db.Roles.ToList());
                _db.Users.RemoveRange(_db.Users.ToList());

                _db.Roles.Add(new Role()
                {
                    Name = SystemRoles.SystemAdmin.ToString(),
                    CreateDate = DateTime.Now,
                    CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower()
                });
                _db.Roles.Add(new Role()
                {
                    Name = SystemRoles.Admin.ToString(),
                    CreateDate = DateTime.Now,
                    CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower()
                });
                _db.Roles.Add(new Role()
                {
                    Name = SystemRoles.User.ToString(),
                    CreateDate = DateTime.Now,
                    CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower()
                });

                _db.SaveChanges();

                _db.Users.Add(new User()
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserName = SystemUsers.SystemAdmin.ToString().ToLower(),
                    Password = "systemadmin",
                    Active = true,
                    CreateDate = DateTime.Now,
                    CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                    RoleIds = new List<int>()
                    {
                        _db.Roles.SingleOrDefault(r => r.Name == SystemRoles.SystemAdmin.ToString()).Id
                    }
                });
                _db.Users.Add(new User()
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserName = "admin",
                    Password = "admin",
                    Active = true,
                    CreateDate = DateTime.Now,
                    CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                    RoleIds = new List<int>()
                    {
                        _db.Roles.SingleOrDefault(r => r.Name == SystemRoles.Admin.ToString()).Id
                    }
                });
                _db.Users.Add(new User()
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserName = "user",
                    Password = "user",
                    Active = true,
                    CreateDate = DateTime.Now,
                    CreatedBy = SystemUsers.SystemAdmin.ToString().ToLower(),
                    RoleIds = new List<int>()
                    {
                        _db.Roles.SingleOrDefault(r => r.Name == SystemRoles.User.ToString()).Id
                    }
                });

                _db.SaveChanges();

                var files = Directory.GetFiles(Path.Combine("wwwroot", "files"));
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                }

                SetTempData(new Result(HttpStatusCode.OK, "Database seed successful."));
            }
            else
            {
                SetTempData(new Result(HttpStatusCode.BadRequest, "Invalid pin!"));
            }

            return RedirectToAction("Index", "Products");
        }
    }
}
