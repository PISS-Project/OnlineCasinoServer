using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Core.Exceptions;
using OnlineCasinoServer.Core.Helpers;
using System.Data.Entity.Migrations;
using OnlineCasinoServer.Data.Entities;
using OnlineCasinoServer.Data.Models;
using System.Linq;

namespace OnlineCasinoServer.Core.Repositories
{
    public class UserRepository : IUserRepository
    {
        private UserDto CreateUserDTO(User user)
        {
            return new UserDto()
            {
                Id = user.Id,
                Username = user.Username,
                Password = user.Password,
                FullName = user.FullName,
                Email = user.Email,
                Money = user.Money
            };
        }

        public UserDto Create(UserDto user)
        {
            User newUser;

            using (var db = new OnlineCasinoDb())
            {
                if (db.Users.FirstOrDefault(u => object.Equals(u.Username, user.Username)) != null)
                    throw new ConflictException();

                newUser = new User()
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Money = 0
                };

                CryptographicManager.SetNewUserInfo(newUser, user.Username, user.Password);

                db.Users.Add(newUser);
                db.SaveChanges();
            }

            return CreateUserDTO(newUser);
        }

        public void Delete(int id)
        {
            using (var db = new OnlineCasinoDb())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                    throw new NotFoundException();

                db.Users.Remove(user);
                db.SaveChanges();
            }
        }

        public UserDto Get(int id)
        {
            User user;

            using (var db = new OnlineCasinoDb())
            {
                user = db.Users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                    throw new NotFoundException();
            }

            return CreateUserDTO(user);
        }

        public UserDto Get(string username, string password)
        {
            User user;
            using (var db = new OnlineCasinoDb())
            {
                user = db.Users.FirstOrDefault(u => object.Equals(u.Username, username));
                if (user == null)
                    throw new NotFoundException();

                var saltedPassword = CryptographicManager.GenerateSHA256Hash(password, user.Salt);

                if (!object.Equals(user.Password, saltedPassword))
                    throw new BadRequestException();
            }

            return CreateUserDTO(user);
        }

        public bool IsPasswordCorrect(int id, string password)
        {
            using (var db = new OnlineCasinoDb())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    throw new BadRequestException();

                var saltedPassword = CryptographicManager.GenerateSHA256Hash(password, user.Salt);

                return object.Equals(user.Password, saltedPassword);
            }
        }

        public UserDto UpdateNameAndEmail(UserDto user)
        {
            User userForUpdate;
            using (var db = new OnlineCasinoDb())
            {
                userForUpdate = db.Users.FirstOrDefault(u => u.Id == user.Id);
                if (userForUpdate == null)
                    throw new NotFoundException();

                userForUpdate.FullName = user.FullName;
                userForUpdate.Email = user.Email;

                db.Users.AddOrUpdate(userForUpdate);
                db.SaveChanges();
            }

            return CreateUserDTO(userForUpdate);
        }

        public void UpdatePassword(int id, string oldPassword, string newPassword)
        {
            using (var db = new OnlineCasinoDb())
            {
                var userForUpdate = db.Users.FirstOrDefault(u => u.Id == id);
                if (userForUpdate == null)
                    throw new NotFoundException();

                var saltedOldPassword = CryptographicManager.GenerateSHA256Hash(oldPassword, userForUpdate.Salt);

                if (!object.Equals(userForUpdate.Password, saltedOldPassword))
                    throw new BadRequestException();

                var saltedNewPassword = CryptographicManager.GenerateSHA256Hash(newPassword, userForUpdate.Salt);

                userForUpdate.Password = saltedNewPassword;

                db.Users.AddOrUpdate(userForUpdate);
                db.SaveChanges();
            }
        }

        public UserDto AddMoney(int id, decimal money)
        {
            User user;
            using (var db = new OnlineCasinoDb())
            {
                user = db.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    throw new NotFoundException();

                user.Money += money;

                db.Users.AddOrUpdate(user);
                db.SaveChanges();
            }

            return CreateUserDTO(user);
        }

        public void UpdateMoney(int id, decimal newMoney)
        {
            using (var db = new OnlineCasinoDb())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    throw new NotFoundException();

                user.Money = newMoney;

                db.Users.AddOrUpdate(user);
                db.SaveChanges();
            }
        }
    }
}