using Microsoft.AspNetCore.Authentication.JwtBearer;
using N4C.App.Services.Auth.Models;
using N4C.App.Services.Users;
using N4C.App.Services.Users.Models;
using N4C.Domain.Users;
using System.Security.Claims;

namespace N4C.App.Services.Auth
{
    public class AuthService : Service
    {
        protected HttpService HttpService { get; }

        protected Service<User, UserRequest, UserResponse> UserService { get; }

        public AuthService(HttpService httpService, LogService logService, Service<User, UserRequest, UserResponse> userService) : base(logService)
        {
            HttpService = httpService;
            UserService = userService;
            SetCulture(HttpService.Culture);
        }

        protected List<Claim> GetClaims(UserResponse userResponse)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userResponse.UserName),
                new Claim("Id", userResponse.Id.ToString())
            };
            foreach (var role in userResponse.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }
            return claims;
        }

        public virtual async Task<Result> Login(LoginRequest loginRequest)
        {
            var validationResult = Validate(loginRequest.ModelState);
            if (!validationResult.Success)
                return validationResult;
            var result = await (UserService as UserService).GetItem(loginRequest);
            if (result.Success)
                await HttpService.SignInAsync(GetClaims(result.Data));
            return result;
        }

        public virtual async Task Logout()
        {
            await HttpService.SignOutAsync();
        }

        public virtual async Task<Result> Register(RegisterRequest registerRequest, bool active = true, params int[] roleIds)
        {
            var validationResult = Validate(registerRequest.ModelState);
            if (!validationResult.Success)
                return validationResult;
            if (registerRequest.Password != registerRequest.ConfirmPassword)
                return Error(Culture == Cultures.TR ? "Şifre ve Şifre Onay aynı olmalıdır!" : "Password and Confirm Password must be the same!");
            return await UserService.Create(new UserRequest()
            {
                UserName = registerRequest.UserName,
                Password = registerRequest.Password,
                Active = active,
                RoleIds = roleIds.ToList()
            });
        }

        public virtual async Task<Result<JwtResponse>> GetJwt(LoginRequest loginRequest)
        {
            JwtResponse jwtResponse = null;
            var validationResult = Validate(loginRequest.ModelState);
            if (!validationResult.Success)
                return Error(jwtResponse, validationResult);
            var result = await (UserService as UserService).GetItem(loginRequest);
            if (result.Success)
            {
                var expiration = DateTime.Now.AddHours(Settings.JwtExpirationInHours);
                jwtResponse = new JwtResponse()
                {
                    Id = result.Data.Id,
                    Token = $"{JwtBearerDefaults.AuthenticationScheme} {HttpService.GetJwt(GetClaims(result.Data), expiration)}",
                    Expiration = expiration
                };
                return Success(jwtResponse);
            }
            return Error(jwtResponse, result);
        }
    }
}
