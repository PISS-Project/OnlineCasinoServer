using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Core.Helpers;
using OnlineCasinoServer.Data.Models;
using OnlineCasinoServer.Data.Entities;
using OnlineCasinoServer.WebApi;
using NUnit.Framework;
using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Http;
using System.Web.Http;


namespace OnlineCasinoServer.IntegrationTests
{
    [SetUpFixture]
    public class TestsSetUp
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            // Configure server and client
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            TestHelper.Server = new HttpServer(config);
            TestHelper.Client = new HttpClient(TestHelper.Server);

            // Make sure a test user exists in database if not create it
            TestHelper.TestUser = new UserDto()
            {
                Username = "testUser",
                Password = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                FullName = "Test Testov",
                Email = "integration@test.com",
                Money = 9999
            };

            User user;
            using (var db = new OnlineCasinoDb())
            {
                user = db.Users.FirstOrDefault(u => object.Equals(u.Username, TestHelper.TestUser.Username));

                if (user == null)
                    user = new User();

                CryptographicManager.SetNewUserInfo(user, TestHelper.TestUser.Username, TestHelper.TestUser.Password);
                user.Username = TestHelper.TestUser.Username;
                user.FullName = TestHelper.TestUser.FullName;
                user.Email = TestHelper.TestUser.Email;
                user.Money = TestHelper.TestUser.Money;

                db.Users.AddOrUpdate(user);
                db.SaveChanges();
            }

            TestHelper.TestUser.Id = user.Id; // save user id

            // Create test login
            TestHelper.TestLogin = new LoginDto()
            {
                UserId = TestHelper.TestUser.Id,
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            };

            Login login;
            using (var db = new OnlineCasinoDb())
            {
                login = new Login()
                {
                    UserId = TestHelper.TestLogin.UserId,
                    Token = TestHelper.TestLogin.Token
                };

                db.Logins.Add(login);
                db.SaveChanges();
            }

            TestHelper.TestLogin.Id = login.Id; // save login id
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TestHelper.Server?.Dispose();
            TestHelper.Client?.Dispose();

            // Remove all logins of test user
            using (var db = new OnlineCasinoDb())
            {
                var logins = db.Logins.Where(l => l.UserId == TestHelper.TestUser.Id);

                db.Logins.RemoveRange(logins);
                db.SaveChanges();
            }

            // Remove all bets of test user
            using (var db = new OnlineCasinoDb())
            {
                var bets = db.DiceBets.Where(b => b.UserId == TestHelper.TestUser.Id);

                db.DiceBets.RemoveRange(bets);
                db.SaveChanges();
            }
        }
    }
}
