using BalanzaWS;
using EFHBlazzer.Server.WSEntities;
using EFHBlazzer.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EFHBlazzer.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;

        public CuentasController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet("RenovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserToken>> Renovar()
        {
            var userInfo = new UserInfo()
            {
                User = HttpContext.User.Identity.Name
            };

            var usuario = await _userManager.FindByEmailAsync(userInfo.User);
            var roles = await _userManager.GetRolesAsync(usuario);

            return null;// BuildToken(userInfo, roles);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo userInfo)
        {
            //var result = await _signInManager.PasswordSignInAsync(userInfo.User, 
            //    userInfo.Password, isPersistent: false, lockoutOnFailure: false);
            BalanzaWSSoapClient clase = new BalanzaWSSoapClient(BalanzaWSSoapClient.EndpointConfiguration.BalanzaWSSoap, _configuration["path:balanzaWS"]);
            SecuredTokenWebservice secure = new SecuredTokenWebservice
            {
                UserName = userInfo.User
                //, Password = userInfo.Password
            };

            AuthenticationUserResponse loginResponse = clase.AuthenticationUserAsync(secure).Result;
            string response = loginResponse.AuthenticationUserResult;
            if (!response.Contains("Error"))
            {
                var resp = SimpleJson.DeserializeObject<AuthUser>(response);
                //var usuario = await _userManager.FindByEmailAsync(userInfo.Email);
                //var roles = await _userManager.GetRolesAsync(usuario);
                //AuthUser userInfox = new AuthUser();
                //userInfox.IdUsuario = "1";
                //userInfox.Nombre = "Juan";
                //userInfox.Apellido = "Perez";
                //userInfox.Token = "11111";
                return BuildToken(resp);
            }
            else
            {
                return BadRequest("Fallo al querer loggearse");
            }
        }

        private UserToken BuildToken(AuthUser userInfo)
        {
            var claims = new List<Claim>()
            {
        new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.IdUsuario),
        new Claim(ClaimTypes.Name, userInfo.NombreApellido),
        new Claim(ClaimTypes.Hash, userInfo.Token),
        new Claim("id", userInfo.IdUsuario),
        new Claim("rol", userInfo.Rol),
        new Claim("AUTH_B_Cali", userInfo.AUTH_B_Cali),
        new Claim("AUTH_B_Del", userInfo.AUTH_B_Del),
        new Claim("AUTH_B_DelPE", userInfo.AUTH_B_DelPE),
        new Claim("AUTH_B_ESap", userInfo.AUTH_B_ESap),
        new Claim("AUTH_B_Part", userInfo.AUTH_B_Part),
        new Claim("AUTH_B_Peso", userInfo.AUTH_B_Peso),
        new Claim("AUTH_C_ESap", userInfo.AUTH_C_ESap),
        new Claim("token", userInfo.Token),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            //foreach (var rol in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, rol));
            //}

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(24);

            JwtSecurityToken token = new JwtSecurityToken(
               issuer: null,
               audience: null,
               claims: claims,
               expires: expiration,
               signingCredentials: creds);

            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
