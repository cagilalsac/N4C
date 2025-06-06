using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using N4C.Controllers;
using N4C.Models;
using N4C.Users.App.Domain;

namespace N4C.Users.Web.Controllers
{
    public class DbController : MvcController
    {
        const int PIN = 2025;

        private readonly N4CUsersDb _db;

        public DbController(N4CUsersDb db, IModelMetadataProvider modelMetaDataProvider) : base(modelMetaDataProvider)
        {
            _db = db;
            Set(Defaults.EN);
        }

        public IActionResult Seed(int pin)
        {
            if (pin == PIN)
            {
                if (_db.Roles.Any() || _db.Users.Any() || _db.Statuses.Any())
                {
                    _db.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('Roles', RESEED, 0)");
                    _db.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('Users', RESEED, 0)");
                    _db.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('Statuses', RESEED, 0)");
                    _db.Database.ExecuteSqlRaw("dbcc CHECKIDENT ('UserRoles', RESEED, 0)");
                }

                _db.UserRoles.RemoveRange(_db.UserRoles.ToList());
                _db.Roles.RemoveRange(_db.Roles.ToList());
                _db.Users.RemoveRange(_db.Users.ToList());
                _db.Statuses.RemoveRange(_db.Statuses.ToList());

                _db.Roles.Add(new N4CRole()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Name = Defaults.System,
                    CreateDate = DateTime.Now,
                    CreatedBy = Defaults.System
                });
                _db.Roles.Add(new N4CRole()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Name = Defaults.Admin,
                    CreateDate = DateTime.Now,
                    CreatedBy = Defaults.System
                });
                _db.Roles.Add(new N4CRole()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Name = Defaults.User,
                    CreateDate = DateTime.Now,
                    CreatedBy = Defaults.System
                });

                _db.Statuses.Add(new N4CStatus()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = Defaults.Active,
                    CreateDate = DateTime.Now,
                    CreatedBy = Defaults.System
                });
                _db.Statuses.Add(new N4CStatus()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = Defaults.Inactive,
                    CreateDate = DateTime.Now,
                    CreatedBy = Defaults.System
                });

                _db.SaveChanges();

                _db.Users.Add(new N4CUser()
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserName = Defaults.System,
                    Password = Defaults.System,
                    CreateDate = DateTime.Now,
                    CreatedBy = Defaults.System,
                    RoleIds = new List<int>()
                    {
                        _db.Roles.SingleOrDefault(r => r.Name == Defaults.System).Id
                    },
                    StatusId = _db.Statuses.SingleOrDefault(s => s.Title == Defaults.Active).Id
                });
                _db.Users.Add(new N4CUser()
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserName = Defaults.Admin,
                    Password = Defaults.Admin,
                    CreateDate = DateTime.Now,
                    CreatedBy = Defaults.System,
                    RoleIds = new List<int>()
                    {
                        _db.Roles.SingleOrDefault(r => r.Name == Defaults.Admin).Id
                    },
                    StatusId = _db.Statuses.SingleOrDefault(s => s.Title == Defaults.Active).Id
                });
                _db.Users.Add(new N4CUser()
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserName = Defaults.User,
                    Password = Defaults.User,
                    CreateDate = DateTime.Now,
                    CreatedBy = Defaults.System,
                    RoleIds = new List<int>()
                    {
                        _db.Roles.SingleOrDefault(r => r.Name == Defaults.User).Id
                    },
                    StatusId = _db.Statuses.SingleOrDefault(s => s.Title == Defaults.Active).Id
                });

                _db.SaveChanges();

                var files = Directory.GetFiles(Path.Combine("wwwroot", "files")).Where(f => f != Path.Combine("wwwroot", "files", "noimage.png"));
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                }

                SetTempData("İlk veriler başarıyla oluşturuldu.", "Database seed successful.");
            }
            else
            {
                SetTempData("Geçersiz pin!", "Invalid pin!");
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
