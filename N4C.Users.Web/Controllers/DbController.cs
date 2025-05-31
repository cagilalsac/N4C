using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using N4C.Controllers;
using N4C.Models;
using N4C.Users.App.Domain;
using N4C.Users.App.Models;

namespace N4C.Users.Web.Controllers
{
    public class DbController : MvcController
    {
        const int PIN = 2025;

        private readonly N4CUsersDb _db;

        public DbController(N4CUsersDb db, IModelMetadataProvider modelMetaDataProvider) : base(modelMetaDataProvider)
        {
            _db = db;
            Set(Cultures.EN);
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
                    Name = N4CRoles.System.ToString(),
                    CreateDate = DateTime.Now,
                    CreatedBy = N4CRoles.System.ToString().ToLower()
                });
                _db.Roles.Add(new N4CRole()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Name = N4CRoles.Admin.ToString(),
                    CreateDate = DateTime.Now,
                    CreatedBy = N4CRoles.System.ToString().ToLower()
                });
                _db.Roles.Add(new N4CRole()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Name = N4CRoles.User.ToString(),
                    CreateDate = DateTime.Now,
                    CreatedBy = N4CRoles.System.ToString().ToLower()
                });

                _db.Statuses.Add(new N4CStatus()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = nameof(N4CStatuses.Active),
                    CreateDate = DateTime.Now,
                    CreatedBy = N4CRoles.System.ToString().ToLower()
                });
                _db.Statuses.Add(new N4CStatus()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Title = nameof(N4CStatuses.Inactive),
                    CreateDate = DateTime.Now,
                    CreatedBy = N4CRoles.System.ToString().ToLower()
                });

                _db.SaveChanges();

                _db.Users.Add(new N4CUser()
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserName = N4CRoles.System.ToString().ToLower(),
                    Password = N4CRoles.System.ToString().ToLower(),
                    CreateDate = DateTime.Now,
                    CreatedBy = N4CRoles.System.ToString().ToLower(),
                    RoleIds = new List<int>()
                    {
                        _db.Roles.SingleOrDefault(r => r.Name == N4CRoles.System.ToString()).Id
                    },
                    StatusId = _db.Statuses.SingleOrDefault(s => s.Title == nameof(N4CStatuses.Active)).Id
                });
                _db.Users.Add(new N4CUser()
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserName = N4CRoles.Admin.ToString().ToLower(),
                    Password = N4CRoles.Admin.ToString().ToLower(),
                    CreateDate = DateTime.Now,
                    CreatedBy = N4CRoles.System.ToString().ToLower(),
                    RoleIds = new List<int>()
                    {
                        _db.Roles.SingleOrDefault(r => r.Name == N4CRoles.Admin.ToString()).Id
                    },
                    StatusId = _db.Statuses.SingleOrDefault(s => s.Title == nameof(N4CStatuses.Active)).Id
                });
                _db.Users.Add(new N4CUser()
                {
                    Guid = Guid.NewGuid().ToString(),
                    UserName = N4CRoles.User.ToString().ToLower(),
                    Password = N4CRoles.User.ToString().ToLower(),
                    CreateDate = DateTime.Now,
                    CreatedBy = N4CRoles.System.ToString().ToLower(),
                    RoleIds = new List<int>()
                    {
                        _db.Roles.SingleOrDefault(r => r.Name == N4CRoles.User.ToString()).Id
                    },
                    StatusId = _db.Statuses.SingleOrDefault(s => s.Title == nameof(N4CStatuses.Active)).Id
                });

                _db.SaveChanges();

                SetTempData("İlk veriler başarıyla oluşturuldu.", "Database seed successful.");
            }
            else
            {
                SetTempData("Geçersiz pin!", "Invalid pin!");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
