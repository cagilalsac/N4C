using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.Extensions;
using N4C.Models;
using N4C.Services;
using N4C.Users.App.Domain;
using N4C.Users.App.Models;

namespace N4C.Users.App.Services
{
    public class N4CRoleService : Service<N4CRole, N4CRoleRequest, N4CRoleResponse>
    {
        public N4CRoleService(N4CUsersDb db, IHttpContextAccessor httpContextAccessor, ILogger<Service> logger) : base(db, httpContextAccessor, logger)
        {
        }

        protected override void Set(Action<ServiceConfig<N4CRole, N4CRoleRequest, N4CRoleResponse>> config)
        {
            base.Set(config =>
            {
                config.SetResponse()
                    .Map(destination => destination.UsersCount, source => source.UserRoles.Count)
                    .Map(destination => destination.Users, source => source.UserRoles.OrderBy(userRole => userRole.User.UserName).Select(userRole => new N4CUserResponse()
                    {
                        Id = userRole.User.Id,
                        Guid = userRole.User.Guid,
                        UserName = userRole.User.UserName,
                        Email = userRole.User.Email,
                        FullName = $"{userRole.User.FirstName} {userRole.User.LastName}",
                        Status = new N4CStatusResponse()
                        {
                            Id = userRole.User.Status.Id,
                            Guid = userRole.User.Status.Guid,
                            Title = userRole.User.Status.Title
                        }
                    }).ToList());
                config.SetTitle("Rol", "Role");
            });
        }

        protected override IQueryable<N4CRole> Query()
        {
            var systemRoleId = Defaults.SystemId;
            var systemRoleQuery = base.Query().Where(role => role.Id == systemRoleId)
                .Include(role => role.UserRoles).ThenInclude(userRole => userRole.User).ThenInclude(user => user.Status);
            var otherRolesQuery = base.Query().Where(role => role.Id != systemRoleId)
                .Include(role => role.UserRoles).ThenInclude(userRole => userRole.User).ThenInclude(user => user.Status).OrderBy(role => role.Name);
            return systemRoleQuery.Union(otherRolesQuery);
        }

        public override async Task<Result<N4CRoleRequest>> Update(N4CRoleRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.Id == Defaults.SystemId)
                return Error(request, "System rolü güncellenemez", "System role can't be updated");
            return await base.Update(request, save, cancellationToken);
        }

        public override async Task<Result<N4CRoleRequest>> Delete(N4CRoleRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.Id == Defaults.SystemId)
                return Error(request, "System rolü silinemez", "System role can't be deleted");
            Validate(GetEntity(request).UserRoles);
            return await base.Delete(request, save, cancellationToken);
        }
    }
}
