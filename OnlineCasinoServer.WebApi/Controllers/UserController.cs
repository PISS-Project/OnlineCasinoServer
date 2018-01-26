using OnlineCasinoServer.Core.Exceptions;
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
    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
        private readonly IUserRepository userRepository;
        private readonly ILoginRepository loginRepository;

        public UserController(IUserRepository userRepository, ILoginRepository loginRepository)
        {
            this.userRepository = userRepository;
            this.loginRepository = loginRepository;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("")]
        public Task<HttpResponseMessage> Register([FromBody] RegisterRequest registerRequest)
        {
            var user = new UserDto()
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password,
                FullName = registerRequest.FullName,
                Email = registerRequest.Email
            };

            user = userRepository.Create(user);

            var response = new
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email
            };

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.Created, response));
        }

        [HttpGet]
        [Route("{id}")]
        public Task<HttpResponseMessage> GetProfileData(int id)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(id, token))
                throw new ForbiddenException();

            var user = userRepository.Get(id);

            var response = new
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email
            };

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, response));
        }

        [HttpPut]
        [Route("{id}/profile")]
        public Task<HttpResponseMessage> UpdataProfileData(int id, [FromBody] UpdateProfileRequest updProfileRequest)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(id, token))
                throw new ForbiddenException();

            if (string.IsNullOrEmpty(updProfileRequest.FullName)
                && string.IsNullOrEmpty(updProfileRequest.Email))
                throw new BadRequestException();

            var user = userRepository.Get(id);

            if (!string.IsNullOrEmpty(updProfileRequest.FullName))
                user.FullName = updProfileRequest.FullName;

            if (!string.IsNullOrEmpty(updProfileRequest.Email))
                user.Email = updProfileRequest.Email;

            user = userRepository.UpdateNameAndEmail(user);

            var response = new
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, response));
        }

        [HttpPut]
        [Route("{id}/password")]
        public Task<HttpResponseMessage> ChangePassword(int id, [FromBody] ChangePasswordRequest chgPasswordRequest)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(id, token))
                throw new ForbiddenException();

            userRepository.UpdatePassword(id, chgPasswordRequest.OldPassword, chgPasswordRequest.NewPassword);

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.NoContent));
        }

        [HttpDelete]
        [Route("{id}")]
        public Task<HttpResponseMessage> DeleteAccount(int id, [FromBody] DeleteAccountRequest delAccountRequest)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(id, token))
                throw new ForbiddenException();

            if (!userRepository.IsPasswordCorrect(id, delAccountRequest.Password))
                throw new ForbiddenException();

            userRepository.Delete(id);

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.NoContent));
        }

        [HttpPut]
        [Route("{id}/wallet")]
        public Task<HttpResponseMessage> AddMoney(int id, [FromBody] AddMoneyRequest addMoneyRequest)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(id, token))
                throw new ForbiddenException();

            decimal money = addMoneyRequest.AddMoney;

            var user = userRepository.AddMoney(id, money);

            var response = new
            {
                NewBalance = user.Money
            };

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, response));
        }

        [HttpGet]
        [Route("{id}/wallet")]
        public Task<HttpResponseMessage> GetBalance(int id)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(id, token))
                throw new ForbiddenException();

            var user = userRepository.Get(id);

            var response = new
            {
                Balance = user.Money
            };

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, response));
        }
    }
}
