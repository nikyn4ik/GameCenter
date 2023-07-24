using GameCenter.Entities;
using GameCenter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GameCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthorizationController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Log model)
        {
            // Поиск пользователя по имени
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Получение ролей пользователя
                var usRols = await _userManager.GetRolesAsync(user);

                // Формирование списка претензий для создания токена
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                // Добавление ролей в претензии
                foreach (var usRol in usRols)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, usRol));
                }

                // Генерация токена
                var token = GenerateToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            // Ошибка авторизации
            return Unauthorized();
        }


        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] Register model)
        {
            // Проверка валидности модели
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Проверка существования пользователя с таким именем
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Conflict(new Reply { Status = "Error", Message = "Пользователь уже существует!" });

            // Создание нового пользователя
            var user = new IdentityUser
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            // Регистрация пользователя в системе
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Reply { Status = "Error", Message = "Ошибка создания пользователя! Пожалуйста, проверьте данные пользователя и попробуйте снова." });

            return Ok(new Reply { Status = "Success", Message = "Пользователь успешно создан!" });
        }

        [HttpPost("Register-admin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] Register model)
        {
            // Проверка валидности модели
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Проверка существования пользователя с таким именем
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Conflict(new Reply { Status = "Error", Message = "Пользователь уже существует!" });

            // Создание нового пользователя
            var user = new IdentityUser
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            // Регистрация пользователя в системе
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Reply { Status = "Error", Message = "Ошибка создания пользователя! Пожалуйста, проверьте данные пользователя и попробуйте снова." });

            // Создание и присвоение ролей пользователю
            await CreateAndAssignRolesAsync(user);

            return Ok(new Reply { Status = "Success", Message = "Пользователь успешно создан!" });
        }


        // Создание и присвоение ролей пользователю
        private async Task CreateAndAssignRolesAsync(IdentityUser user)
        {
            // Создание роли администратора, если она не существует
            if (!await _roleManager.RoleExistsAsync(UsRols.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UsRols.Admin));

            // Создание роли пользователя, если она не существует
            if (!await _roleManager.RoleExistsAsync(UsRols.User))
                await _roleManager.CreateAsync(new IdentityRole(UsRols.User));

            // Присвоение ролей пользователю
            await _userManager.AddToRoleAsync(user, UsRols.Admin);
            await _userManager.AddToRoleAsync(user, UsRols.User);
        }

        // Генерация токена
        private JwtSecurityToken GenerateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["GameCenter:SecretKey"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["GameCenter:ValidAudienceURL"],
                audience: _configuration["GameCenter:ValidIssuerURL"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }
    }
}