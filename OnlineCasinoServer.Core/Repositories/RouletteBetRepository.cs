using System;
using System.Collections.Generic;
using System.Linq;
using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Core.Exceptions;
using OnlineCasinoServer.Data.Entities;
using OnlineCasinoServer.Data.Models;

namespace OnlineCasinoServer.Core.Repositories
{
    public class RouletteBetRepository : IRouletteBetRepository
    {
        private RouletteBetDto CreateBetDTO(RouletteBet bet)
        {
            return new RouletteBetDto()
            {
                Id = bet.Id,
                UserId = bet.UserId,
                BetValues = bet.BetValues.ToArray(),
                SpinResult = bet.SpinResult,
                Stake = bet.Stake,
                Win = bet.Win,
                CreationDate = bet.CreationDate
            };
        }

        public RouletteBetDto Get(int id)
        {
            RouletteBet bet;

            using (var db = new OnlineCasinoDb())
            {
                bet = db.RouletteBets.FirstOrDefault(b => b.Id == id);

                if (bet == null)
                    throw new NotFoundException();
            }

            return CreateBetDTO(bet);
        }

        public RouletteBetDto Create(RouletteBetDto bet)
        {
            var newBet = new RouletteBet()
            {
                UserId = bet.UserId,
                BetValues = bet.BetValues.ToArray(),
                SpinResult = bet.SpinResult,
                Stake = bet.Stake,
                Win = bet.Win,
                CreationDate = bet.CreationDate
            };

            using (var db = new OnlineCasinoDb())
            {
                db.RouletteBets.Add(newBet);
                db.SaveChanges();
            }

            return CreateBetDTO(newBet);
        }

        public IEnumerable<RouletteBetDto> GetBets(int userId, int skip, int take, string orderby, string filter)
        {
            Func<RouletteBet, bool> betFilter;
            if (object.Equals(filter, "win"))
                betFilter = new Func<RouletteBet, bool>(b => b.UserId == userId && b.Win != 0);
            else if (object.Equals(filter, "lose"))
                betFilter = new Func<RouletteBet, bool>(b => b.UserId == userId && b.Win == 0);
            else betFilter = new Func<RouletteBet, bool>(b => b.UserId == userId);

            List<RouletteBet> bets;
            using (var db = new OnlineCasinoDb())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    throw new NotFoundException();

                if (object.Equals(orderby, "win"))
                {
                    bets = (from b in db.RouletteBets.Where(betFilter)
                            orderby b.Win
                            select b).Skip(skip).Take(take).ToList();
                }
                else
                {
                    bets = (from b in db.RouletteBets.Where(betFilter)
                            orderby b.CreationDate
                            select b).Skip(skip).Take(take).ToList();
                }
            }

            foreach (var bet in bets)
            {
                yield return CreateBetDTO(bet);
            }
        }
    }
}
