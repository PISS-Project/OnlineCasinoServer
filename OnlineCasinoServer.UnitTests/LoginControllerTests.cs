using OnlineCasinoServer.Core.DTOs;
using OnlineCasinoServer.Core.Exceptions;
using OnlineCasinoServer.Core.Repositories;
using OnlineCasinoServer.WebApi.Controllers;
using OnlineCasinoServer.WebApi.Requests;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace OnlineCasinoServer.UnitTests
{
    [TestFixture]
    public class LoginControllerTests
    {
        [Test]
        public async Task Login_CorrectCredentials_ReturnsTokenAndCorrectUserIdAndCreatedStatusCode()
        {
            var user = new UserDto() { Id = 1 };

            //Arrange
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();

            mockUserRepository.Setup(u => u.Get("username", "password")).Returns(user);
            mockLoginRepository.Setup(l => l.LoginUser(It.IsAny<LoginDto>())).Returns<LoginDto>(x => x);

            var controller = new LoginController(mockLoginRepository.Object, mockUserRepository.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            //Act
            var response = await controller.Login(new LoginRequest() { Username = "username", Password = "password" });

            //Assert
            LoginDto loginData;
            response.TryGetContentValue<LoginDto>(out loginData);
            Assert.AreEqual(loginData.UserId, user.Id);
            Assert.IsFalse(string.IsNullOrEmpty(loginData.Token));
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
        }

        [Test]
        public async Task Logout_CorrectLoginIdAndToken_SuccessfulLogoutAndReturnsNoContentStatusCode()
        {
            var login = new LoginDto() { Id = 1, Token = "test" };

            //Arrange
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();

            mockLoginRepository.Setup(l => l.HasLoginAndToken(login.Id, login.Token)).Returns(true);

            var controller = new LoginController(mockLoginRepository.Object, mockUserRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", login.Token);
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var response = await controller.Logout(login.Id);

            //Assert
            mockLoginRepository.Verify(l => l.Delete(login.Id));
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Test]
        public void Logout_IncorrectToken_ThrowsNotFoundException()
        {
            var login = new LoginDto() { Id = 1, Token = "test" };

            //Arrange
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();

            mockLoginRepository.Setup(l => l.HasLoginAndToken(login.Id, login.Token)).Returns(false); //simulate incorrect token

            var controller = new LoginController(mockLoginRepository.Object, mockUserRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", login.Token);
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            var ex = Assert.Throws<NotFoundException>(() => controller.Logout(login.Id));
        }
    }
}
