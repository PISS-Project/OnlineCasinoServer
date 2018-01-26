using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Core.Exceptions;
using OnlineCasinoServer.Core.Repositories;
using OnlineCasinoServer.WebApi.Controllers;
using OnlineCasinoServer.WebApi.Requests;
using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OnlineCasinoServer.UnitTests
{
    [TestFixture]
    public class DiceBetControllerTests
    {
        [Test]
        public async Task Bet_EnoughMoney_CreatesBetAndReturnsBetInfoAndOKStatusCode()
        {
            var user = new UserDto()
            {
                Id = 1,
                Money = 1234
            };

            int diceSumResult = 0;

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockUserRepository.Setup(u => u.Get(user.Id)).Returns(user);
            mockBetRepository.Setup(b => b.Create(It.IsAny<DiceBetDto>())).Returns<DiceBetDto>(x =>
            {
                x.Id = 1; // set bet Id to 1
                diceSumResult = x.DiceSumResult; // save result of dice throw. Used in assertion of win amount
                return x;
            });

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var betRequest = new DiceBetRequest()
            {
                Bet = 10,
                Stake = 200
            };
            var timeBeforeBet = DateTime.Now;
            var response = await controller.Bet(user.Id, betRequest);
            var timeAfterBet = DateTime.Now;

            //Assert
            string cnt = await response.Content.ReadAsStringAsync();
            dynamic respData = JsonConvert.DeserializeObject(cnt);
            var win = (decimal)respData.Win;
            mockUserRepository.Verify(u => u.UpdateMoney(user.Id, It.IsAny<decimal>()));
            Assert.IsTrue(respData.BetId == 1);
            Assert.IsTrue(respData.Bet == betRequest.Bet);
            Assert.IsTrue(respData.Stake == betRequest.Stake);
            Assert.IsTrue(betRequest.Bet == diceSumResult ? win > 0 : win == 0);
            var timestamp = (DateTime)respData.Timestamp;
            Assert.IsTrue(timeBeforeBet < timestamp);
            Assert.IsTrue(timestamp < timeAfterBet);

            Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
        }

        [Test]
        public void Bet_NotEnoughMoney_ThrowsBadRequestException()
        {
            var user = new UserDto()
            {
                Id = 1,
                Money = 1234
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockUserRepository.Setup(u => u.Get(user.Id)).Returns(user);

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            var betRequest = new DiceBetRequest()
            {
                Bet = 10,
                Stake = user.Money + 1000
            };
            Assert.Throws<BadRequestException>(() => controller.Bet(user.Id, betRequest));
        }

        [Test]
        public async Task GetBets_ValidData_ReturnsBetsAndOKStatusCode()
        {
            var user = new UserDto() { Id = 1 };
            var bets = new List<DiceBetDto>()
            {
                new DiceBetDto() { Id = 1, UserId = user.Id, DiceSumBet = 6, DiceSumResult = 4, Win = 0, Stake = 20, CreationDate = DateTime.Now.AddDays(6) },
                new DiceBetDto() { Id = 2, UserId = user.Id, DiceSumBet = 6, DiceSumResult = 4, Win = 0, Stake = 20, CreationDate = DateTime.Now.AddDays(-8) },
                new DiceBetDto() { Id = 3, UserId = user.Id, DiceSumBet = 6, DiceSumResult = 4, Win = 0, Stake = 20, CreationDate = DateTime.Now.AddDays(2) },
                new DiceBetDto() { Id = 4, UserId = user.Id, DiceSumBet = 6, DiceSumResult = 6, Win = 20, Stake = 20, CreationDate = DateTime.Now.AddDays(4) },
                new DiceBetDto() { Id = 5, UserId = user.Id, DiceSumBet = 6, DiceSumResult = 6, Win = 40, Stake = 40, CreationDate = DateTime.Now.AddDays(-2) },
                new DiceBetDto() { Id = 6, UserId = user.Id, DiceSumBet = 6, DiceSumResult = 6, Win = 80, Stake = 80, CreationDate = DateTime.Now.AddDays(10) },
                new DiceBetDto() { Id = 7, UserId = user.Id, DiceSumBet = 6, DiceSumResult = 6, Win = 100, Stake = 100, CreationDate = DateTime.Now.AddDays(-5) },
                new DiceBetDto() { Id = 8, UserId = user.Id, DiceSumBet = 6, DiceSumResult = 6, Win = 120, Stake = 120, CreationDate = DateTime.Now.AddDays(3) },
                new DiceBetDto() { Id = 9, UserId = user.Id, DiceSumBet = 6, DiceSumResult = 6, Win = 140, Stake = 140, CreationDate = DateTime.Now.AddDays(-3) }
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockBetRepository.Setup(b => b.GetBets(user.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(bets);

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var response = await controller.GetBets(user.Id, "0", "0", "win");

            //Assert

            // Check ordered by win
            string cnt = await response.Content.ReadAsStringAsync();            
            var respData = JsonConvert.DeserializeObject<IEnumerable<object>>(cnt);
            int count = 0;
            foreach (dynamic bet in respData)
            {
                Assert.IsTrue(bet.Win == bets[count].Win);
                count++;
            }
            Assert.IsTrue(count == bets.Count);

            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void GetBets_InvalidData_ThrowsBadRequestException()
        {
            var user = new UserDto() { Id = 1 };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            Assert.Throws<BadRequestException>(() => controller.GetBets(user.Id, "-1", "0", "win"));
            Assert.Throws<BadRequestException>(() => controller.GetBets(user.Id, "invalid", "0", "win"));
            Assert.Throws<BadRequestException>(() => controller.GetBets(user.Id, "0", "-1", "win"));
            Assert.Throws<BadRequestException>(() => controller.GetBets(user.Id, "0", "invalid", "win"));
            Assert.Throws<BadRequestException>(() => controller.GetBets(user.Id, "0", "0", "invalid"));
            Assert.Throws<BadRequestException>(() => controller.GetBets(user.Id, "0", "0", "win", "invalid"));
        }

        [Test]
        public async Task GetBet_UserHasBet_ReturnsBetInfoAndOKStatusCode()
        {
            var user = new UserDto() { Id = 1 };
            var bet = new DiceBetDto()
            {
                Id = 1337,
                UserId = user.Id,
                DiceSumBet = 10,
                DiceSumResult = 8,
                Win = 0,
                Stake = 20,
                CreationDate = DateTime.Now
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockBetRepository.Setup(b => b.Get(bet.Id)).Returns(bet);

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var response = await controller.GetBet(user.Id, bet.Id);

            //Assert
            string cnt = await response.Content.ReadAsStringAsync();
            dynamic respData = JsonConvert.DeserializeObject(cnt);
            Assert.IsTrue(respData.CreationDate == bet.CreationDate);
            Assert.IsTrue(respData.Bet == bet.DiceSumBet);
            Assert.IsTrue(respData.Stake == bet.Stake);
            Assert.IsTrue(respData.Win == bet.Win);
            Assert.IsTrue(respData.ActualRoll == bet.DiceSumResult);

            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void GetBet_UserDoesNotHaveBet_ThrowsForbiddenException()
        {
            var user = new UserDto() { Id = 1 };
            var bet = new DiceBetDto()
            {
                Id = 1337,
                UserId = 1234
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockBetRepository.Setup(b => b.Get(bet.Id)).Returns(bet);

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            Assert.Throws<ForbiddenException>(() => controller.GetBet(user.Id, bet.Id));
        }

        [Test]
        public async Task DeleteBet_UserHasBetAndOneMinuteHaveNotPassed_DeletesBetAndReturnsNoContentStatusCode()
        {
            var user = new UserDto() { Id = 1 };
            var bet = new DiceBetDto()
            {
                Id = 1337,
                UserId = user.Id,
                CreationDate = DateTime.Now
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockBetRepository.Setup(b => b.Get(bet.Id)).Returns(bet);

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var response = await controller.DeleteBet(user.Id, bet.Id);

            //Assert
            mockBetRepository.Verify(b => b.Delete(bet.Id));

            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Test]
        public void DeleteBet_UserHasBetButMoreThanOneMinutePassed_ThrowsForbiddenException()
        {
            var user = new UserDto() { Id = 1 };
            var bet = new DiceBetDto()
            {
                Id = 1337,
                UserId = user.Id,
                CreationDate = DateTime.Now.AddMinutes(-2)
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockBetRepository.Setup(b => b.Get(bet.Id)).Returns(bet);

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            Assert.Throws<ForbiddenException>(() => controller.DeleteBet(user.Id, bet.Id));
        }

        [Test]
        public void DeleteBet_UserDoesNotHaveBet_ThrowsForbiddenException()
        {
            var user = new UserDto() { Id = 1 };
            var bet = new DiceBetDto()
            {
                Id = 1337,
                UserId = 1234,
                CreationDate = DateTime.Now
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockBetRepository.Setup(b => b.Get(bet.Id)).Returns(bet);

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            Assert.Throws<ForbiddenException>(() => controller.DeleteBet(user.Id, bet.Id));
        }

        [Test]
        public void AllMethodsWithTokenAuth_InvalidToken_ThrowsForbiddenException()
        {
            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IBetRepository> mockBetRepository = new Mock<IBetRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(1, "token")).Returns(false); // simulate invalid token

            var controller = new DiceBetController(mockUserRepository.Object, mockLoginRepository.Object, mockBetRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            Assert.Throws<ForbiddenException>(() => controller.Bet(1, new DiceBetRequest()));
            Assert.Throws<ForbiddenException>(() => controller.GetBets(1, string.Empty, string.Empty, string.Empty));
            Assert.Throws<ForbiddenException>(() => controller.GetBet(1, 1));
            Assert.Throws<ForbiddenException>(() => controller.DeleteBet(1, 1));
        }
    }
}
