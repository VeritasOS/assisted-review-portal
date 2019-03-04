/*
Copyright (c) 2019 Veritas Technologies LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using garb.Authentication;
using garb.Data;
using garb.DTO;
using garb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace garb.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TokenController : Controller
    {
        private IConfiguration _config;
		private UnitOfWork _unitOfWork;
		private readonly ILogger<TokenController> _logger;

		public TokenController(GarbContext context, IConfiguration config, ILogger<TokenController> logger)
        {
			_unitOfWork = new UnitOfWork(context);
			_config = config;
			_logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateToken([FromBody]LoginDto login)
        {
            IActionResult response = Unauthorized();

            if (login != null && LdapHelper.ValidateDomainCredentials(_unitOfWork, login.Username, login.Password))
            {
                var tokenString = BuildToken(login.Username);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string BuildToken(string userName)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
					{
						new Claim( JwtRegisteredClaimNames.Sub, userName),
						new Claim( JwtRegisteredClaimNames.UniqueName, userName)
					};

			var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              expires: DateTime.Now.AddDays(30),
              signingCredentials: creds,
			  claims: claims);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
