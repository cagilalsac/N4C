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
    public class N4CUserService : Service<N4CUser, N4CUserRequest, N4CUserResponse>
    {
        public N4CUserService(N4CUsersDb db, IHttpContextAccessor httpContextAccessor, ILogger<Service> logger) : base(db, httpContextAccessor, logger)
        {
        }

        protected override void Set(Action<ServiceConfig<N4CUser, N4CUserRequest, N4CUserResponse>> config)
        {
            base.Set(config =>
            {
                config.SetResponse()
                    .Map(destination => destination.Roles, source => source.UserRoles.OrderBy(userRole => userRole.Role.Name).Select(userRole => userRole.Role.Name).ToList())
                    .Map(destination => destination.FullName, source => Culture == Cultures.TR ? $"{source.FirstName} {source.LastName}" : $"{source.LastName} {source.FirstName}")
                    .Map(destination => destination.Active, source => source.StatusId == (int)N4CStatuses.Active)
                    .Map(destination => destination.ActiveS, source => (source.StatusId == (int)N4CStatuses.Active).ToHtml(Config.TrueHtml, Config.FalseHtml, Culture))
                    .Map(destination => destination.Status, source => new N4CStatusResponse()
                    {
                        Id = source.Status.Id,
                        Guid = source.Status.Guid,
                        Title = source.Status.Title
                    });
                config.SetTitle("Kullanıcı", "User");
            });
        }

        protected override IQueryable<N4CUser> Entities()
        {
            var systemUserId = (int)N4CUsers.System;
            var systemUserQuery = base.Entities().Where(user => user.Id == systemUserId)
                .Include(user => user.Status).Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role);
            var otherUsersQuery = base.Entities().Where(user => user.Id != systemUserId)
                .Include(user => user.Status).Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
                .OrderByDescending(user => user.UpdateDate).OrderByDescending(user => user.CreateDate).ThenBy(user => user.UserName);
            return systemUserQuery.Union(otherUsersQuery);
        }

        public override async Task<Result<N4CUserRequest>> Create(N4CUserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.RoleIds.Any(roleId => roleId == (int)N4CRoles.System))
                return Result(request, "System rolünde yeni bir kullanıcı oluşturulamaz", "New user with role System can't be created");
            return await base.Create(request, save, cancellationToken);
        }

        public override async Task<Result<N4CUserRequest>> Update(N4CUserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            var userRoles = Entity(request).UserRoles;
            if (userRoles.Any(userRole => userRole.RoleId == (int)N4CRoles.System) && request.RoleIds.Any(roleId => roleId != (int)N4CRoles.System) ||
                userRoles.Any(userRole => userRole.RoleId != (int)N4CRoles.System) && request.RoleIds.Any(roleId => roleId == (int)N4CRoles.System))
                return Result(request, "System rolü için kullanıcı güncellenemez", "User for role System can't be updated");
            if (request.Id == (int)N4CUsers.System)
                request.RoleIds.Clear();
            else
                Update(userRoles);
            return await base.Update(request, save, cancellationToken);
        }

        public override async Task<Result<N4CUserRequest>> Delete(N4CUserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.Id == (int)N4CUsers.System)
                return Result(request, "System kullanıcısı silinemez", "System user can't be deleted");
            Delete(Entity(request).UserRoles);
            return await base.Delete(request, save, cancellationToken);
        }

        public async Task<Result> Deactivate(int id, CancellationToken cancellationToken = default)
        {
            if (id == (int)N4CUsers.System)
                return Result(false, "System kullanıcısı etkisizleştirilemez", "System user can't be deactivated", id);
            var result = await Result(id);
            if (result.Success)
            {
                Update(Entity(result.Data).UserRoles);
                result.Data.StatusId = (int)N4CStatuses.Inactive;
                return Result(await base.Update(result.Data, true, cancellationToken), "Kullanıcı başarıyla etkisizleştirildi", "User is deactivated successfully");
            }
            return result;
        }

        public async Task<Result> Activate(string guid, CancellationToken cancellationToken = default)
        {
            var result = await Result(guid);
            if (result.Success)
            {
                if (result.Data.Id == (int)N4CUsers.System)
                    return Result(false, "System kullanıcısı etkinleştirilemez", "System user can't be activated");
                Update(Entity(result.Data).UserRoles);
                result.Data.StatusId = (int)N4CStatuses.Active;
                return Result(await base.Update(result.Data, true, cancellationToken), "Kullanıcı başarıyla etkinleştirildi", "User is activated successfully");
            }
            return result;
        }

        public async Task<Result<N4CUserLoginRequest>> Login(N4CUserLoginRequest request, CancellationToken cancellationToken = default)
        {
            var validationResult = Validate(request.ModelState);
            if (!validationResult.Success)
                return Result(request, validationResult);
            var result = await Response(user => user.UserName == request.UserName &&
                user.Password == request.Password && user.StatusId == (int)N4CStatuses.Active, cancellationToken);
            if (result.Success)
                await SignIn(GetClaims(result.Data.Id, result.Data.UserName, result.Data.Roles));
            return Result(request, result);
        }

        public async Task Logout()
        {
            await SignOut();
        }

        public async Task<Result<N4CUserRegisterRequest>> Register(N4CUserRegisterRequest request)
        {
            var validationResult = Validate(request.ModelState);
            if (!validationResult.Success)
                return Result(request, validationResult);
            if (request.Password != request.ConfirmPassword)
                return Result(request, Culture == Cultures.TR ? "Şifre ve Şifre Onay aynı olmalıdır!" : "Password and Confirm Password must be the same!");
            var result = await Create(new N4CUserRequest()
            {
                UserName = request.UserName,
                Password = request.Password,
                Email = request.Email?.Trim(),
                FirstName = request.FirstName?.Trim(),
                LastName = request.LastName?.Trim(),
                StatusId = (int)N4CStatuses.Active,
                RoleIds = [(int)N4CRoles.User]
            });
            return Result(request, result);
        }
    }
}
