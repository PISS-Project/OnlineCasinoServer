using OnlineCasinoServer.Core.Exceptions;
using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Core.Repositories;
using OnlineCasinoServer.WebApi.Requests;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace OnlineCasinoServer.WebApi.Controllers
{
    [RoutePrefix("api/users/{userId}/dicebets")]
    public class DiceBetController : ApiController
    {
        private readonly IUserRepository userRepository;
        private readonly ILoginRepository loginRepository;
        private readonly IBetRepository betRepository;
        private static readonly Random random = new Random();

        public DiceBetController(IUserRepository userRepository, ILoginRepository loginRepository, IBetRepository betRepository)
        {
            this.userRepository = userRepository;
            this.loginRepository = loginRepository;
            this.betRepository = betRepository;
        }

        private int RollDices()
        {
            // Sum | Chances | Coresponding random number range
            //  2  |  1/36   | [  1,  1]
            //  3  |  2/36   | [  2,  3]
            //  4  |  3/36   | [  4,  6]
            //  5  |  4/36   | [  7, 10]
            //  6  |  5/36   | [ 11, 15]
            //  7  |  6/36   | [ 16, 21]
            //  8  |  5/36   | [ 22, 26]
            //  9  |  4/36   | [ 27, 30]
            // 10  |  3/36   | [ 31, 33]
            // 11  |  2/36   | [ 34, 35]
            // 12  |  1/36   | [ 36, 36]
            int num = random.Next(1, 37);

            if (num <= 1) return 2;
            else if (num <= 3) return 3;
            else if (num <= 6) return 4;
            else if (num <= 10) return 5;
            else if (num <= 15) return 6;
            else if (num <= 21) return 7;
            else if (num <= 26) return 8;
            else if (num <= 30) return 9;
            else if (num <= 33) return 10;
            else if (num <= 35) return 11;
            else return 12;
        }

        private decimal StakeMultiplier(int bet)
        {
            // Sum | Chances | Multiplier
            //  2  |  1/36   | 36 / 1 = 36
            //  3  |  2/36   | 36 / 2 = 18
            //  4  |  3/36   | 36 / 3 = 12
            //  5  |  4/36   | 36 / 4 = 9
            //  6  |  5/36   | 36 / 5 = 7.2
            //  7  |  6/36   | 36 / 6 = 6
            //  8  |  5/36   | 36 / 5 = 7.2
            //  9  |  4/36   | 36 / 4 = 9
            // 10  |  3/36   | 36 / 3 = 12
            // 11  |  2/36   | 36 / 2 = 18
            // 12  |  1/36   | 36 / 1 = 36
            switch (bet)
            {
                case 2:
                case 12:
                    return 36;
                case 3:
                case 11:
                    return 18;
                case 4:
                case 10:
                    return 12;
                case 5:
                case 9:
                    return 9;
                case 6:
                case 8:
                    return 7.2M;
                case 7:
                    return 6;
                default:
                    return 0;
            }
        }

        [HttpPost]
        [Route("")]
        public Task<HttpResponseMessage> Bet(int userId, [FromBody] DiceBetRequest betRequest)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(userId, token))
                throw new ForbiddenException();

            int userBet = betRequest.Bet;
            decimal stake = betRequest.Stake;

            var user = userRepository.Get(userId);

            if (user.Money < stake)
                throw new BadRequestException("Not enough money in balance!");

            decimal win = 0;
            int roll = RollDices();

            if (roll == userBet)
            {
                decimal multiplier = StakeMultiplier(userBet);
                win = stake * multiplier;
                win -= win / 10; // for the house
            }

            var bet = new DiceBetDto()
            {
                UserId = userId,
                DiceSumBet = userBet,
                DiceSumResult = roll,
                Stake = stake,
                Win = win,
                CreationDate = DateTime.Now
            };

            bet = betRepository.Create(bet);

            user.Money = user.Money - stake + win;

            userRepository.UpdateMoney(user.Id, user.Money);

            var response = new
            {
                BetId = bet.Id,
                Bet = bet.DiceSumBet,
                Stake = bet.Stake,
                Win = bet.Win,
                Timestamp = bet.CreationDate
            };

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.Created, response));
        }

        [HttpGet]
        [Route("")]
        public Task<HttpResponseMessage> GetBets(int userId, [FromUri] string skip, [FromUri] string take, [FromUri] string orderBy, [FromUri] string filter = "none")
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(userId, token))
                throw new ForbiddenException();

            int skipNum;
            if (!int.TryParse(skip, out skipNum))
                throw new BadRequestException();

            int takeNum;
            if (!int.TryParse(take, out takeNum))
                throw new BadRequestException();

            if (skipNum < 0 || takeNum < 0)
                throw new BadRequestException();

            if (!object.Equals(orderBy, "time")
                && !object.Equals(orderBy, "win"))
                throw new BadRequestException();

            if (!object.Equals(filter, "win")
                && !object.Equals(filter, "lose")
                && !object.Equals(filter, "none"))
                throw new BadRequestException();

            var bets = betRepository.GetBets(userId, skipNum, takeNum, orderBy, filter);

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, from b in bets
                                                                             select new
                                                                             {
                                                                                 CreationDate = b.CreationDate,
                                                                                 Stake = b.Stake,
                                                                                 Win = b.Win
                                                                             }));
        }

        [HttpGet]
        [Route("{id}")]
        public Task<HttpResponseMessage> GetBet(int userId, int id)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(userId, token))
                throw new ForbiddenException();

            var bet = betRepository.Get(id);

            if (bet.UserId != userId)
                throw new ForbiddenException();

            var response = new
            {
                CreationDate = bet.CreationDate,
                Bet = bet.DiceSumBet,
                Stake = bet.Stake,
                Win = bet.Win,
                ActualRoll = bet.DiceSumResult
            };

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, response));
        }

        [HttpDelete]
        [Route("{id}")]
        public Task<HttpResponseMessage> DeleteBet(int userId, int id)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(userId, token))
                throw new ForbiddenException();

            var bet = betRepository.Get(id);

            if (bet.UserId != userId)
                throw new ForbiddenException();

            if (DateTime.Now.AddMinutes(-1) > bet.CreationDate)
                throw new ForbiddenException();

            betRepository.Delete(id);

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}
