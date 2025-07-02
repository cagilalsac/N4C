using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.Extensions;
using N4C.Models;
using N4C.Services;
using N4C.User.App.Domain;
using N4C.User.App.Models;

namespace N4C.User.App.Services
{
    public class N4CUserService : Service<N4CUser, N4CUserRequest, N4CUserResponse>
    {
        public N4CUserService(N4CUserDb db, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, ILogger<Service> logger)
            : base(db, httpContextAccessor, httpClientFactory, logger)
        {
        }

        protected override IQueryable<N4CUser> Query(Action<ServiceConfig<N4CUser, N4CUserRequest, N4CUserResponse>> config = default)
        {
            var query = base.Query(config =>
            {
                config.SetResponse()
                    .Map(destination => destination.FullName, source => $"{source.FirstName} {source.LastName}")
                    .Map(destination => destination.Roles, source => source.UserRoles.OrderBy(userRole => userRole.Role.Name).Select(userRole => userRole.Role.Name).ToList())
                    .Map(destination => destination.Roles_, source => string.Join(", ", source.UserRoles.OrderBy(userRole => userRole.Role.Name).Select(userRole => userRole.Role.Name)))
                    .Map(destination => destination.Status, source => new N4CStatusResponse()
                    {
                        Id = source.Status.Id,
                        Guid = source.Status.Guid,
                        Title = source.Status.Title
                    })
                    .Map(destination => destination.Active, source => source.StatusId == Defaults.ActiveId)
                    .Map(destination => destination.Active_, source => source.Status.Title)
                    .Map(destination => destination.Active_Html, source => (source.StatusId == Defaults.ActiveId).ToHtml(Config.TrueHtml, Config.FalseHtml, Culture));
                config.SetEntity()
                    .Map(destination => destination.FirstName, source => source.FirstName.FirstLetterToUpperOthersToLower())
                    .Map(destination => destination.LastName, source => source.LastName.FirstLetterToUpperOthersToLower());
                config.SetTitle("Kullanıcı", "User");
                config.SetModelStateErrors(false);
                config.SetPageOrder([1, 2, 3], entity => entity.StatusId, entity => entity.UserName, entity => entity.CreateDate, entity => entity.UpdateDate);
            });
            var systemUserQuery = query.Where(user => user.Id == Defaults.SystemId)
                .Include(user => user.Status).Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role);
            var otherUsersQuery = query.Where(user => user.Id != Defaults.SystemId)
                .Include(user => user.Status).Include(user => user.UserRoles).ThenInclude(userRole => userRole.Role)
                .OrderByDescending(user => user.UpdateDate).OrderByDescending(user => user.CreateDate).ThenBy(user => user.UserName);
            return systemUserQuery.Union(otherUsersQuery);
        }

        protected override async Task<Result<N4CUserRequest>> Create(N4CUserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.RoleIds.Any(roleId => roleId == Defaults.SystemId))
                return Error(request, "System rolünde yeni bir kullanıcı oluşturulamaz", "New user with role System can't be created");
            return await base.Create(request, save, cancellationToken);
        }

        protected override async Task<Result<N4CUserRequest>> Update(N4CUserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            var userRoles = GetEntity(request).UserRoles;
            if (userRoles.Any(userRole => userRole.RoleId == Defaults.SystemId) && request.RoleIds.Any(roleId => roleId != Defaults.SystemId) ||
                userRoles.Any(userRole => userRole.RoleId != Defaults.SystemId) && request.RoleIds.Any(roleId => roleId == Defaults.SystemId))
                return Error(request, "System rolü için kullanıcı güncellenemez", "User for role System can't be updated");
            if (request.Id == Defaults.SystemId)
                request.RoleIds.Clear();
            else
                Update(userRoles);
            return await base.Update(request, save, cancellationToken);
        }

        public override async Task<Result<N4CUserRequest>> Delete(N4CUserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.Id == Defaults.SystemId)
                return Error(request, "System kullanıcısı silinemez", "System user can't be deleted");
            Delete(GetEntity(request).UserRoles);
            return await base.Delete(request, save, cancellationToken);
        }

        public async Task<Result> Deactivate(int id, CancellationToken cancellationToken = default)
        {
            if (id == Defaults.SystemId)
                return Error("System kullanıcısı etkisizleştirilemez", "System user can't be deactivated", id);
            var result = await GetRequest(id);
            if (result.Success)
            {
                Update(GetEntity(result.Data).UserRoles);
                result.Data.StatusId = Defaults.InactiveId;
                return Result(await base.Update(result.Data, true, cancellationToken), "Kullanıcı başarıyla etkisizleştirildi", "User is deactivated successfully");
            }
            return result;
        }

        public async Task<Result> Activate(string guid, CancellationToken cancellationToken = default)
        {
            var result = await GetRequest(guid);
            if (result.Success)
            {
                if (result.Data.Id == Defaults.SystemId)
                    return Error("System kullanıcısı etkinleştirilemez", "System user can't be activated");
                Update(GetEntity(result.Data).UserRoles);
                result.Data.StatusId = Defaults.ActiveId;
                return Result(await base.Update(result.Data, true, cancellationToken), "Kullanıcı başarıyla etkinleştirildi", "User is activated successfully");
            }
            return result;
        }

        public async Task<Result<N4CUserLoginRequest>> Login(N4CUserLoginRequest request, ModelStateDictionary modelState, Uri tokenApiUri = default, CancellationToken cancellationToken = default)
        {
            var validationResult = Validate(modelState);
            if (!validationResult.Success)
                return Result(validationResult, request);
            var result = await GetResponse(user => user.UserName == request.UserName &&
                user.Password == request.Password && user.StatusId == Defaults.ActiveId, cancellationToken);
            if (result.Success)
            {
                await Login(GetClaims(result.Data.Single().Id, result.Data.Single().UserName, result.Data.Single().Roles));
                if (tokenApiUri is not null && tokenApiUri.AbsoluteUri.HasAny())
                {
                    var tokenResult = await GetToken(tokenApiUri, request.UserName, request.Password, cancellationToken);
                    if (!tokenResult.Success)
                        return Result(tokenResult, request);
                }
            }
            return Result(result, request);
        }

        public async Task<Result<N4CUserRegisterRequest>> Register(N4CUserRegisterRequest request, ModelStateDictionary modelState, CancellationToken cancellationToken = default)
        {
            var validationResult = Validate(modelState);
            if (!validationResult.Success)
                return Result(validationResult, request);
            if (request.Password != request.ConfirmPassword)
                return Error(request, "Şifre ve Şifre Onay aynı olmalıdır!", "Password and Confirm Password must be the same!");
            var result = await Create(new N4CUserRequest()
            {
                UserName = request.UserName,
                Password = request.Password,
                Email = request.Email?.Trim(),
                FirstName = request.FirstName?.Trim(),
                LastName = request.LastName?.Trim(),
                StatusId = Defaults.ActiveId,
                RoleIds = [Defaults.UserId]
            }, true, cancellationToken);
            return Result(result, request);
        }

        public async Task<Result<TokenResponse>> GetToken(TokenRequest request, ModelStateDictionary modelState, CancellationToken cancellationToken = default)
        {
            TokenResponse response = null;
            var validationResult = Validate(modelState);
            if (!validationResult.Success)
                return Result(validationResult, response);
            var user = GetEntity(user => user.UserName == request.UserName && user.Password == request.Password && user.StatusId == Defaults.ActiveId);
            if (user is null)
                return Error(response, NotFound);
            user.RefreshToken = GetRefreshToken();
            user.RefreshTokenExpiration = DateTime.Now.AddMinutes(N4CAppSettings.RefreshTokenExpirationInMinutes);
            var result = await Update(user, true, cancellationToken);
            if (!result.Success)
                return Result(result, response);
            var claims = GetClaims(user.Id, user.UserName, user.UserRoles.Select(userRole => userRole.Role.Name));
            var expiration = DateTime.Now.AddMinutes(N4CAppSettings.JwtExpirationInMinutes);
            var token = GetToken(claims, expiration);
            return Success(new TokenResponse()
            {
                Id = user.Id,
                Guid = user.Guid,
                Token = token,
                BearerToken = $"{JwtBearerDefaults.AuthenticationScheme} {token}",
                RefreshToken = user.RefreshToken,
                CreateDate = DateTime.Now
            });
        }

        public async Task<Result<TokenResponse>> GetRefreshToken(RefreshTokenRequest request, ModelStateDictionary modelState, CancellationToken cancellationToken = default)
        {
            TokenResponse response = null;
            var validationResult = Validate(modelState);
            if (!validationResult.Success)
                return Result(validationResult, response);
            var claims = GetClaims(request.Token);
            if (claims is null)
                return Error(response, NotFound);
            var userId = GetUserId(claims);
            var user = GetEntity(user => user.Id == userId && user.RefreshToken == request.RefreshToken && user.RefreshTokenExpiration >= DateTime.Now);
            if (user is null)
                return Error(response, NotFound);
            user.RefreshToken = GetRefreshToken();
            if (N4CAppSettings.RefreshTokenSlidingExpiration)
                user.RefreshTokenExpiration = DateTime.Now.AddMinutes(N4CAppSettings.RefreshTokenExpirationInMinutes);
            var result = await Update(user, true, cancellationToken);
            if (!result.Success)
                return Result(result, response);
            claims = GetClaims(user.Id, user.UserName, user.UserRoles.Select(userRole => userRole.Role.Name));
            var expiration = DateTime.Now.AddMinutes(N4CAppSettings.JwtExpirationInMinutes);
            var token = GetToken(claims, expiration);
            return Success(new TokenResponse()
            {
                Id = user.Id,
                Guid = user.Guid,
                Token = token,
                BearerToken = $"{JwtBearerDefaults.AuthenticationScheme} {token}",
                RefreshToken = user.RefreshToken,
                CreateDate = DateTime.Now
            });
        }
    }
}
