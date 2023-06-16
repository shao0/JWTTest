using JWTTest.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTTest.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly JwtHelper _jwtHelper;

        public AccountController(JwtHelper jwtHelper)
        {
            _jwtHelper = jwtHelper;
        }

        [HttpGet]
        public ActionResult<string> GetToken()
        {
            return _jwtHelper.CreateToken();
        }

        [Authorize]
        [HttpGet]
        public ActionResult<string> GetTest()
        {
            return "Test Authorize";
        }
    }
}
