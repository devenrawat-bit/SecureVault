using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Infrastructure.Authentication;

public class JwtTokenService
{
    private readonly JwtSettings _jwtSettings; //here the jwt settings class represents this 
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenService(
        IOptions<JwtSettings> jwtSettings,
        UserManager<ApplicationUser> userManager)
    {
        _jwtSettings = jwtSettings.Value;
        _userManager = userManager;
    }

    /// <summary>
    /// This will generate the token for the user 
    /// </summary>dotnet user-secrets list --project Auth.API
    /// <param name="user">The data for the logged in user </param>
    /// <returns>String token </returns>
    public async Task<string> GenerateTokenAsync(ApplicationUser user) 
    {
        var roles = await _userManager.GetRolesAsync(user); //fetches all the roles that are assigned to that user 

        //building the payload claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), //storing the id 
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty), //storing the 
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.OrganizationId.HasValue) //the has value property only applies on the nullable type, and here if the orgid is null then it wont be added in the jwt 
        {
            claims.Add(
                new Claim(
                    "OrganizationId",
                    user.OrganizationId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}