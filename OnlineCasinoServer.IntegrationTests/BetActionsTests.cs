using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Data.Models;
using OnlineCasinoServer.WebApi.Requests;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OnlineCasinoServer.IntegrationTests
{
    [TestFixture]
    public class BetActionsTests
    {
        [Test]
        public async Task MakeABetWithValidDataThenGetTheNewBetToCheckInfoAndDeleteIt()
        {
            string userIdString = TestHelper.TestUser.Id.ToString();

            // Make a bet

            decimal initialUserMoney = TestHelper.TestUser.Money;

            // Arrange
            var betRequest = new DiceBetRequest()
            {
                Bet = 12,
                Stake = 20
            };

            var json = JsonConvert.SerializeObject(betRequest);
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets",
                                                        method: HttpMethod.Post,
                                                        token: TestHelper.TestLogin.Token,
                                                        json: json);

            // Act
            var timeBeforeBet = DateTime.Now;
            var response = await TestHelper.Client.SendAsync(request);
            var timeAfterBet = DateTime.Now;

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            var bet = new DiceBetDto()
            {
                Id = int.Parse(jsonResponse["betId"]),
                DiceSumBet = int.Parse(jsonResponse["bet"]),
                Stake = decimal.Parse(jsonResponse["stake"]),
                Win = decimal.Parse(jsonResponse["win"]),
                UserId = TestHelper.TestUser.Id,
                CreationDate = DateTime.Parse(jsonResponse["timestamp"])
            };

            Assert.IsTrue(timeBeforeBet < bet.CreationDate);
            Assert.IsTrue(bet.CreationDate < timeAfterBet);

            // update new test user money
            using (var db = new OnlineCasinoDb())
            {
                var user = db.Users.First(u => u.Id == TestHelper.TestUser.Id);

                TestHelper.TestUser.Money = user.Money;
            }

            Assert.IsTrue(bet.Win > 0 ?
                TestHelper.TestUser.Money > initialUserMoney - betRequest.Stake
                : TestHelper.TestUser.Money == initialUserMoney - betRequest.Stake);

            // Get bet info

            // Arrange 2
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets/" + bet.Id.ToString(),
                                                        method: HttpMethod.Get,
                                                        token: TestHelper.TestLogin.Token);

            // Act 2
            response = await TestHelper.Client.SendAsync(request);

            // Assert 2
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            Assert.IsTrue(int.Parse(jsonResponse["bet"]) == bet.DiceSumBet);
            Assert.IsTrue(decimal.Parse(jsonResponse["stake"]) == bet.Stake);
            Assert.IsTrue(decimal.Parse(jsonResponse["win"]) == bet.Win);
            Assert.IsTrue(DateTime.Parse(jsonResponse["creationDate"]) == bet.CreationDate);

            // Delete bet

            // Arrange 3
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets/" + bet.Id.ToString(),
                                                        method: HttpMethod.Delete,
                                                        token: TestHelper.TestLogin.Token);

            // Act 3
            response = await TestHelper.Client.SendAsync(request);

            // Assert 3
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);

            using (var db = new OnlineCasinoDb())
            {
                var deletedBet = db.DiceBets.FirstOrDefault(b => b.Id == bet.Id);

                Assert.IsTrue(deletedBet == null);
            }
        }

        [Test]
        public async Task TryBettingWithoutEnoughMoneyAndCheckIfBadRequestReturned()
        {
            string userIdString = TestHelper.TestUser.Id.ToString();

            // Make a bet

            // Arrange
            var betRequest = new DiceBetRequest()
            {
                Bet = 12,
                Stake = TestHelper.TestUser.Money + 10000
            };

            var json = JsonConvert.SerializeObject(betRequest);
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets",
                                                        method: HttpMethod.Post,
                                                        token: TestHelper.TestLogin.Token,
                                                        json: json);

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateSomeBetsAndGetThemSortedByWinAndByTimeAndCheckSkipAndTakeFunctionality()
        {
            string userIdString = TestHelper.TestUser.Id.ToString();

            List<DiceBetDto> allBets = new List<DiceBetDto>();

            DiceBetRequest betRequest;
            string json;
            HttpRequestMessage request;
            HttpResponseMessage response;

            // Create 10 bets
            for (int i = 0; i < 10; i++)
            {
                // Arrange
                betRequest = new DiceBetRequest()
                {
                    Bet = 12,
                    Stake = 20
                };

                json = JsonConvert.SerializeObject(betRequest);
                request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets",
                                                            method: HttpMethod.Post,
                                                            token: TestHelper.TestLogin.Token,
                                                            json: json);

                // Act
                var timeBeforeBet = DateTime.Now;
                response = await TestHelper.Client.SendAsync(request);
                var timeAfterBet = DateTime.Now;

                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
                var cnt = await response.Content.ReadAsStringAsync();
                var jsonResp = JsonConvert.DeserializeObject<Dictionary<string, string>>(cnt);

                allBets.Add(new DiceBetDto()
                {
                    Id = int.Parse(jsonResp["betId"]),
                    DiceSumBet = int.Parse(jsonResp["bet"]),
                    Stake = decimal.Parse(jsonResp["stake"]),
                    Win = decimal.Parse(jsonResp["win"]),
                    UserId = TestHelper.TestUser.Id,
                    CreationDate = DateTime.Parse(jsonResp["timestamp"])
                });

                Assert.IsTrue(timeBeforeBet < allBets[i].CreationDate);
                Assert.IsTrue(allBets[i].CreationDate < timeAfterBet);
            }

            // Get bets sorted by win

            // Arrange 2
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets?skip=0&take=10&orderby=win",
                                                        method: HttpMethod.Get,
                                                        token: TestHelper.TestLogin.Token);

            // Act 2
            response = await TestHelper.Client.SendAsync(request);

            // Assert 2
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(content);

            decimal lastWin = 0;
            for (int i = 0; i < 10; i++)
            {
                decimal currentWin = decimal.Parse(jsonResponse[i]["win"]);
                Assert.IsTrue(lastWin <= currentWin);
                lastWin = currentWin;
            }
            Assert.IsTrue(jsonResponse.Count == 10);

            // Get bets sorted by time

            // Arrange 3
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets?skip=0&take=10&orderby=time",
                                                        method: HttpMethod.Get,
                                                        token: TestHelper.TestLogin.Token);

            // Act 3
            response = await TestHelper.Client.SendAsync(request);

            // Assert 3
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            var jsonResponseSortedByDateAll = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(content);

            DateTime lastDate = DateTime.Now.AddYears(-1);
            for (int i = 0; i < 10; i++)
            {
                DateTime currentDate = DateTime.Parse(jsonResponseSortedByDateAll[i]["creationDate"]);
                Assert.IsTrue(lastDate < currentDate);
                lastDate = currentDate;
            }
            Assert.IsTrue(jsonResponseSortedByDateAll.Count == 10);

            // Sort by date but skip 5 and take 3

            // Arrange 4
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets?skip=5&take=3&orderby=time",
                                                        method: HttpMethod.Get,
                                                        token: TestHelper.TestLogin.Token);

            // Act 4
            response = await TestHelper.Client.SendAsync(request);

            // Assert 4
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            jsonResponse = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(content);

            Assert.IsTrue(jsonResponse.Count == 3);
            for (int i = 0; i < 3; i++)
            {
                var creationDate = DateTime.Parse(jsonResponse[i]["creationDate"]);
                Assert.IsTrue(creationDate == DateTime.Parse(jsonResponseSortedByDateAll[5 + i]["creationDate"]));
            }

            // Restore user money before betting
            using (var db = new OnlineCasinoDb())
            {
                var user = db.Users.First(u => u.Id == TestHelper.TestUser.Id);

                user.Money = TestHelper.TestUser.Money;

                db.Users.AddOrUpdate(user);
                db.SaveChanges();
            }
        }        

        [Test]
        public async Task TryAllBetActionsWithInvalidTokenAndCheckIfUnauthorizedReturned()
        {
            string userIdString = TestHelper.TestUser.Id.ToString();

            // Make a bet

            // Arrange
            var betRequest = new DiceBetRequest()
            {
                Bet = 12,
                Stake = 10
            };

            var json = JsonConvert.SerializeObject(betRequest);
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets",
                                                            method: HttpMethod.Post,
                                                            token: "invalidToken",
                                                            json: json);

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Unauthorized);

            // Get some bets

            // Arrange 2
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets?skip=0&take=10&orderby=win",
                                                        method: HttpMethod.Get,
                                                        token: "invalidToken");

            // Act 2
            response = await TestHelper.Client.SendAsync(request);

            // Assert 2
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Unauthorized);

            // Get a specfic bet

            // Arrange 3
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets/1",
                                                        method: HttpMethod.Get,
                                                        token: "invalidToken");

            // Act 3
            response = await TestHelper.Client.SendAsync(request);

            // Assert 3
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Unauthorized);

            // Delete bet

            // Arrange 4
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/dicebets/1",
                                                        method: HttpMethod.Delete,
                                                        token: "invalidToken");

            // Act 4
            response = await TestHelper.Client.SendAsync(request);

            // Assert 4
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Unauthorized);
        }
    }
}
