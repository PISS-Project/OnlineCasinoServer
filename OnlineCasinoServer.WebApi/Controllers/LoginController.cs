using OnlineCasinoServer.Core.Exceptions;
using OnlineCasinoServer.Core.Helpers;
using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Core.Repositories;
using OnlineCasinoServer.WebApi.Requests;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace OnlineCasinoServer.WebApi.Controllers
{

    [RoutePrefix("api/logins")]
    public class LoginController : ApiController
    {
        private readonly ILoginRepository loginRepository;
        private readonly IUserRepository userRepository;

        public LoginController(ILoginRepository loginRepository, IUserRepository userRepository)
        {
            this.loginRepository = loginRepository;
            this.userRepository = userRepository;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("")]
        public Task<HttpResponseMessage> Login([FromBody] LoginRequest loginRequest)
        {
            var user = userRepository.Get(loginRequest.Username, loginRequest.Password);

            string token = TokenManager.GenerateToken();

            var login = new LoginDto()
            {
                UserId = user.Id,
                Token = token
            };

            login = loginRepository.LoginUser(login);

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.Created, login));
        }

        [HttpDelete]
        [Route("{id}")]
        public Task<HttpResponseMessage> Logout(int id)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();

            if (!loginRepository.HasLoginAndToken(id, token))
                throw new NotFoundException();

            loginRepository.Delete(id);

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}
