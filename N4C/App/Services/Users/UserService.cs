using Microsoft.EntityFrameworkCore;
using N4C.App.Services.Files;
using N4C.App.Services.Users.Models;
using N4C.Domain;
using N4C.Domain.Users;
using N4C.Extensions;
using System.Net;

namespace N4C.App.Services.Users
{
    public class UserService : Service<User, UserRequest, UserResponse>
    {
        public UserService(IDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
            SetTitle("Kullanıcı");
        }

        protected override IQueryable<User> Query(Action<MapperProfile> mapperProfile = null)
        {
            var systemAdminUserId = (int)SystemUsers.SystemAdmin;
            mapperProfile = u =>
            {
                u.Map<User, UserResponse>()
                    .Property(d => d.ActiveS, s => s.Active.ToHtml(TrueHtml, FalseHtml))
                    .Property(d => d.Roles, s => s.UserRoles.Where(ur => !ur.Role.Deleted)
                    .OrderBy(ur => ur.Role.Name)
                    .Select(ur => new RoleResponse()
                    {
                        Id = ur.Role.Id,
                        Guid = ur.Role.Guid,
                        Name = ur.Role.Name
                    }).ToList());
            };
            var systemAdminUserQuery = base.Query(mapperProfile).Where(u => u.Id == systemAdminUserId)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
            var otherUsersQuery = base.Query(mapperProfile).Where(u => u.Id != systemAdminUserId)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).OrderBy(u => u.UserName);
            return systemAdminUserQuery.Union(otherUsersQuery);
        }

        public override async Task<Result<UserRequest>> Create(UserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.RoleIds.Any(rId => rId == (int)SystemRoles.SystemAdmin))
                return Error(request, Culture == Cultures.TR ? "SystemAdmin rolünde yeni bir kullanıcı oluşturulamaz" : "New user with role SystemAdmin can't be created");
            return await base.Create(request, save, cancellationToken);
        }

        public override async Task<Result<UserRequest>> Update(UserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            var userRoles = Query(request).UserRoles;
            if (userRoles.Any(ur => ur.RoleId == (int)SystemRoles.SystemAdmin) && request.RoleIds.Any(rId => rId != (int)SystemRoles.SystemAdmin) ||
                userRoles.Any(ur => ur.RoleId != (int)SystemRoles.SystemAdmin) && request.RoleIds.Any(rId => rId == (int)SystemRoles.SystemAdmin))
                return Error(request, Culture == Cultures.TR ? "SystemAdmin rolü için kullanıcı güncellenemez" : "User for role SystemAdmin can't be updated");
            if (request.Id == (int)SystemUsers.SystemAdmin)
                request.RoleIds.Clear();
            else
                Update(userRoles);
            return await base.Update(request, save, cancellationToken);
        }

        public override async Task<Result<UserRequest>> Delete(UserRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            if (request.Id == (int)SystemUsers.SystemAdmin)
                return Error(request, Culture == Cultures.TR ? "SystemAdmin kullanıcısı silinemez" : "SystemAdmin user can't be deleted");
            Delete(Query(request).UserRoles);
            return await base.Delete(request, save, cancellationToken);
        }

        public virtual async Task<Result> Deactivate(int id, CancellationToken cancellationToken = default)
        {
            if (id == (int)SystemUsers.SystemAdmin)
                return Error(Culture == Cultures.TR ? "SystemAdmin kullanıcısı etkisizleştirilemez" : "SystemAdmin user can't be deactivated");
            var result = GetItemForEdit(id);
            if (result.Success)
            {
                var request = result.Data;
                Update(Query(request).UserRoles);
                request.Active = false;
                result = await base.Update(request, true, cancellationToken);
                if (result.Success)
                    return Success(request, Culture == Cultures.TR ? "Kullanıcı başarıyla etkisizleştirildi" : "User deactivated successfully");
            }
            return result;
        }

        public virtual async Task<Result> Activate(string guid, CancellationToken cancellationToken = default)
        {
            var result = GetItemForEdit(guid);
            if (result.Success)
            {
                if (result.Data.Id == (int)SystemUsers.SystemAdmin)
                    return Error(Culture == Cultures.TR ? "SystemAdmin kullanıcısı etkinleştirilemez" : "SystemAdmin user can't be activated");
                var request = result.Data;
                Update(Query(request).UserRoles);
                request.Active = true;
                result = await base.Update(request, true, cancellationToken);
                if (result.Success)
                    return Success(request, Culture == Cultures.TR ? "Kullanıcı başarıyla etkinleştirildi" : "User activated successfully");
            }
            return result;
        }

        public async Task<Result<UserResponse>> GetItem(string userName, string password, CancellationToken cancellationToken = default)
        {
            UserResponse response = null;
            try
            {
                var user = await Query().SingleOrDefaultAsync(u => u.UserName == userName && u.Password == password && u.Active, cancellationToken);
                if (user is null)
                    return Error(response, NotFound);
                response = user.Map<User, UserResponse>(MapperProfile);
                return Success(response);
            }
            catch (Exception exception)
            {
                LogService.LogError($"UserServiceException: {GetType().Name}.GetItem(UserName = {userName}): {exception.Message}");
                return Error(response, HttpStatusCode.InternalServerError);
            }
        }
    }
}
