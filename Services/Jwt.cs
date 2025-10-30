using ApartmentManagement.Models;
using System.Security.Claims;

namespace ApartmentManagement.Services
{
    public class Jwt
    {
        public ClaimsPrincipal CreatePrincipal(User user, string authScheme = "MyCookieAuth")
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Avatar", user.Avatar)
            };
            var identity = new ClaimsIdentity(claims, authScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}
