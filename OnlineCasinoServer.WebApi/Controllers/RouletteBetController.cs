using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Core.Exceptions;
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
    [RoutePrefix("api/users/{userId}/roulettebets")]
    public class RouletteBetController : ApiController
    {
        private readonly IUserRepository userRepository;
        private readonly ILoginRepository loginRepository;
        private readonly IRouletteBetRepository betRepository;
        private static readonly Random random = new Random();

        public RouletteBetController(IUserRepository userRepository, ILoginRepository loginRepository, IRouletteBetRepository betRepository)
        {
            this.userRepository = userRepository;
            this.loginRepository = loginRepository;
            this.betRepository = betRepository;
        }

        private int SpinWheel()
        {
            return random.Next(0, 37);
        }

        private int StakePayout(int valuesCount)
        {
            // Values Count | Odds against winning | Payout
            //      1       |         36 to 1      | 35 to 1
            //      2       |       17.5 to 1      | 17 to 1
            //      3       |      11.33 to 1      | 11 to 1
            //      4       |       8.25 to 1      | 8 to 1
            //      6       |      5.167 to 1      | 5 to 1
            //     12       |      2.083 to 1      | 2 to 1
            //     18       |      1.056 to 1      | 1 to 1

            switch (valuesCount)
            {
                case 1:
                    return 35;
                case 2:
                    return 17;
                case 3:
                    return 11;
                case 4:
                    return 8;
                case 6:
                    return 5;
                case 12:
                    return 2;
                case 18:
                    return 1;
                default:
                    return 0;
            }
        }

        [HttpPost]
        [Route("")]
        public Task<HttpResponseMessage> Bet(int userId, [FromBody] RouletteBetRequest betRequest)
        {
            string token = Request.Headers.GetValues("OnlineCasino-Token").FirstOrDefault();
            if (!loginRepository.HasUserAndToken(userId, token))
                throw new ForbiddenException();

            int[] userBets = betRequest.BetValues;
            decimal stake = betRequest.Stake;

            if (userBets.Length != 1 &&
                userBets.Length != 2 &&
                userBets.Length != 3 &&
                userBets.Length != 4 &&
                userBets.Length != 6 &&
                userBets.Length != 12 &&
                userBets.Length != 18)
            {
                throw new BadRequestException("Bet values count is invalid!");
            }


            var user = userRepository.Get(userId);

            if (user.Money < stake)
                throw new BadRequestException("Not enough money in balance!");

            decimal win = 0;
            int spin = SpinWheel();

            if (userBets.Contains(spin))
            {
                int payout = StakePayout(userBets.Length);
                win = stake + (stake * payout);
            }

            var bet = new RouletteBetDto()
            {
                UserId = userId,
                BetValues = userBets,
                SpinResult = spin,
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
                BetValues = bet.BetValues,
                SpinResult = bet.SpinResult,
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
                BetValues = bet.BetValues,
                SpinResult = bet.SpinResult,
                Stake = bet.Stake,
                Win = bet.Win                
            };

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, response));
        }
    }
}
