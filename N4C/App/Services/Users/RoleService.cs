using Microsoft.EntityFrameworkCore;
using N4C.App.Services.Files;
using N4C.App.Services.Users.Models;
using N4C.Domain;
using N4C.Domain.Users;
using N4C.Extensions;

namespace N4C.App.Services.Users
{
    public class RoleService : Service<Role, RoleRequest, RoleResponse>
    {
        public RoleService(IDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
            SetTitle("Rol");
        }

        protected override IQueryable<Role> Query(Action<MapperProfile> mapperProfile = null)
        {
            var systemAdminRoleId = (int)SystemRoles.SystemAdmin;
            mapperProfile = r =>
                r.Map<Role, RoleResponse>()
                    .Property(d => d.UsersCount, s => s.UserRoles.Where(ur => !ur.User.Deleted).Count())
                    .Property(d => d.Users, s => s.UserRoles.Where(ur => !ur.User.Deleted)
                    .OrderByDescending(ur => ur.User.Active).ThenBy(ur => ur.User.UserName)
                    .Select(ur => new UserResponse()
                    {
                        Id = ur.User.Id,
                        Guid = ur.User.Guid,
                        UserName = ur.User.UserName,
                        Active = ur.User.Active,
                        ActiveS = ur.User.Active.ToHtml(TrueHtml, FalseHtml)
                    }).ToList());
            var systemAdminRoleQuery = base.Query(mapperProfile).Where(r => r.Id == systemAdminRoleId)
                .Include(r => r.UserRoles).ThenInclude(ur => ur.User);
            var otherRolesQuery = base.Query(mapperProfile).Where(r => r.Id != systemAdminRoleId)
                .Include(r => r.UserRoles).ThenInclude(ur => ur.User).OrderBy(r => r.Name);
            return systemAdminRoleQuery.Union(otherRolesQuery);
        }

        public override async Task<Result<RoleRequest>> Update(RoleRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.Id == (int)SystemRoles.SystemAdmin)
                return Error(request, Culture == Cultures.TR ? "SystemAdmin rolü güncellenemez" : "SystemAdmin role can't be updated");
            return await base.Update(request, save, cancellationToken);
        }

        public override async Task<Result<RoleRequest>> Delete(RoleRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.Id == (int)SystemRoles.SystemAdmin)
                return Error(request, Culture == Cultures.TR ? "SystemAdmin rolü silinemez" : "SystemAdmin role can't be deleted");
            Delete(Query(request).UserRoles);
            return await base.Delete(request, save, cancellationToken);
        }
    }
}
