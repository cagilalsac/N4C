using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App.Services.Users.Models;
using N4C.Domain;
using N4C.Domain.Users;
using N4C.Extensions;

namespace N4C.App.Services.Users
{
    public class RoleService : Service<Role, RoleRequest, RoleResponse>
    {
        public RoleService(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
            SetTitle("Rol", "Role");
        }

        protected override IQueryable<Role> Data(Action<MapperProfile> mapperProfile = null)
        {
            return base.Data(r =>
                r.Map<Role, RoleResponse>()
                    .Property(d => d.UsersCount, s => s.UserRoles.Count)
                    .Property(d => d.Users, s => s.UserRoles.OrderByDescending(ur => ur.User.Active).ThenBy(ur => ur.User.UserName).Select(ur => new UserResponse()
                    {
                        Id = ur.User.Id,
                        Guid = ur.User.Guid,
                        UserName = ur.User.UserName,
                        Active = ur.User.Active,
                        ActiveS = ur.User.Active.ToHtml(TrueHtml, FalseHtml)
                    }).ToList()))
                .Include(r => r.UserRoles).ThenInclude(ur => ur.User).OrderBy(r => r.Name);
        }

        public override Task<Result<RoleRequest>> Delete(RoleRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Validate(Data(request).UserRoles);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
