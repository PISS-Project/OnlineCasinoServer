using OnlineCasinoServer.Core.Exceptions;
using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Data.Entities;
using OnlineCasinoServer.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;


namespace OnlineCasinoServer.Core.Repositories
{
    public class DiceBetRepository : IDiceBetRepository
    {
        private DiceBetDto CreateBetDTO(DiceBet bet)
        {
            return new DiceBetDto()
            {
                Id = bet.Id,
                UserId = bet.UserId,
                DiceSumBet = bet.DiceSumBet,
                DiceSumResult = bet.DiceSumResult,
                Stake = bet.Stake,
                Win = bet.Win,
                CreationDate = bet.CreationDate
            };
        }

        public DiceBetDto Create(DiceBetDto bet)
        {
            var newBet = new DiceBet()
            {
                UserId = bet.UserId,
                DiceSumBet = bet.DiceSumBet,
                DiceSumResult = bet.DiceSumResult,
                Stake = bet.Stake,
                Win = bet.Win,
                CreationDate = bet.CreationDate
            };

            using (var db = new OnlineCasinoDb())
            {
                db.DiceBets.Add(newBet);
                db.SaveChanges();
            }

            return CreateBetDTO(newBet);
        }

        public void Delete(int id)
        {
            using (var db = new OnlineCasinoDb())
            {
                var bet = db.DiceBets.FirstOrDefault(b => b.Id == id);
                if (bet == null)
                    throw new NotFoundException();

                var user = bet.User;
                user.Money = user.Money - bet.Win + bet.Stake;

                db.Users.AddOrUpdate(user);
                db.DiceBets.Remove(bet);
                db.SaveChanges();
            }
        }

        public DiceBetDto Get(int id)
        {
            DiceBet bet;

            using (var db = new OnlineCasinoDb())
            {
                bet = db.DiceBets.FirstOrDefault(b => b.Id == id);

                if (bet == null)
                    throw new NotFoundException();
            }

            return CreateBetDTO(bet);
        }

        public IEnumerable<DiceBetDto> GetBets(int userId, int skip, int take, string orderby, string filter)
        {
            Func<DiceBet, bool> betFilter;
            if (object.Equals(filter, "win"))
                 betFilter = new Func<DiceBet, bool>(b => b.UserId == userId && b.Win != 0);
            else if (object.Equals(filter, "lose"))
                betFilter = new Func<DiceBet, bool>(b => b.UserId == userId && b.Win == 0);
            else betFilter = new Func<DiceBet, bool>(b => b.UserId == userId);

            List<DiceBet> bets;
            using (var db = new OnlineCasinoDb())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    throw new NotFoundException();

                if (object.Equals(orderby, "win"))
                {
                    bets = (from b in db.DiceBets.Where(betFilter)
                            orderby b.Win
                            select b).Skip(skip).Take(take).ToList();
                }
                else
                {
                    bets = (from b in db.DiceBets.Where(betFilter)
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