using Microsoft.IdentityModel.Tokens;
using N4C.App.Services.Auth.Models;
using N4C.App.Services.Users;
using N4C.App.Services.Users.Models;
using N4C.Domain.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace N4C.App.Services.Auth
{
    public class AuthService : Service
    {
        protected HttpService HttpService { get; }

        protected Service<User, UserRequest, UserResponse> UserService { get; }

        public AuthService(HttpService httpService, Service<User, UserRequest, UserResponse> userService) 
        {
            HttpService = httpService;
            UserService = userService;
            SetCulture(HttpService.Culture);
        }

        protected virtual List<Claim> GetClaims(UserResponse userResponse)
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
            var result = await (UserService as UserService).GetItem(loginRequest.UserName, loginRequest.Password);
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

        protected virtual JwtResponse GetJwt(UserResponse userResponse)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSecurityKey));
            var signingCredentials = new SigningCredentials(securityKey, Settings.JwtSecurityAlgorithm);
            var expiration = DateTime.Now.AddDays(Settings.JwtExpirationInDays);
            var jwtSecurityToken = new JwtSecurityToken(Settings.JwtIssuer, Settings.JwtAudience, GetClaims(userResponse),
                DateTime.Now, expiration, signingCredentials);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var token = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);
            return new JwtResponse()
            {
                Token = "Bearer " + token,
                Expiration = expiration
            };
        }

        public virtual async Task<Result<JwtResponse>> GetJwt(LoginRequest loginRequest)
        {
            JwtResponse jwtResponse = null;
            var validationResult = Validate(loginRequest.ModelState);
            if (!validationResult.Success)
                return Error(jwtResponse, validationResult);
            var result = await (UserService as UserService).GetItem(loginRequest.UserName, loginRequest.Password);
            if (result.Success)
            {
                jwtResponse = GetJwt(result.Data);
                return Success(jwtResponse);
            }
            return Error(jwtResponse, result);
        }
    }
}
