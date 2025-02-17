using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App.Services.Users.Models;
using N4C.Domain;
using N4C.Domain.Users;
using N4C.Extensions;

namespace N4C.App.Services.Users
{
    public class UserService : Service<User, UserRequest, UserResponse>
    {
        public UserService(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
            SetTitle("Kullanıcı", "User");
        }

        protected override IQueryable<User> Data(Action<MapperProfile> mapperProfile = null)
        {
            return base.Data(u =>
                u.Map<User, UserResponse>()
                    .Property(d => d.ActiveS, s => s.Active.ToHtml(TrueHtml, FalseHtml))
                    .Property(d => d.Roles, s => s.UserRoles.Select(ur => new RoleResponse()
                    {
                        Id = ur.Role.Id,
                        Guid = ur.Role.Guid,
                        Name = ur.Role.Name
                    })))
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).OrderByDescending(u => u.Active).ThenBy(u => u.UserName);
        }

        public override Task<Result<UserRequest>> Update(UserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Update(Data(request).UserRoles);
            return base.Update(request, save, cancellationToken);
        }

        public override async Task<Result<UserRequest>> Delete(UserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            request.Active = false;
            var result = await base.Update(request);
            if (result.Success)
                return Success(request, Culture == Cultures.TR ? "Kullanıcı başarıyla deaktive edildi." : "User deactivated successfully.");
            return Error(request, result);
        }
    }
}
