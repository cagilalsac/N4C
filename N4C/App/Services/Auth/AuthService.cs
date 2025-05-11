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
            Set(HttpService.Culture, "Kullanıcı", "User");
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
                await HttpService.SignIn(GetClaims(result.Data));
            return result;
        }

        public virtual async Task Logout()
        {
            await HttpService.SignOut();
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

        public virtual async Task<Result<TokenResponse>> GetToken(LoginRequest loginRequest)
        {
            TokenResponse tokenResponse = null;
            var validationResult = Validate(loginRequest.ModelState);
            if (!validationResult.Success)
                return Error(tokenResponse, validationResult);
            var result = await (UserService as UserService).GetItem(loginRequest);
            if (result.Success)
            {
                tokenResponse = new TokenResponse()
                {
                    Id = result.Data.Id,
                    Token = $"{JwtBearerDefaults.AuthenticationScheme} {HttpService.GetToken(GetClaims(result.Data), DateTime.Now.AddMinutes(Settings.JwtExpirationInMinutes))}"
                };
                return Success(tokenResponse);
            }
            return Error(tokenResponse, result);
        }
    }
}
