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
using Newtonsoft.Json;

namespace OnlineCasinoServer.UnitTests
{
    [TestFixture]
    public class UserControllerTests
    {
        [Test]
        public async Task Register_CorrectInfo_RetunrsNewUserDataAndCreatedSatusCode()
        {
            var user = new UserDto()
            {
                Id = 1,
                Username = "username",
                Password = "pass",
                FullName = "Test",
                Email = "Test"
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockUserRepository.Setup(u => u.Create(It.IsAny<UserDto>())).Returns(user);

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            //Act
            var registerRequest = new RegisterRequest()
            {
                Username = user.Username,
                Password = user.Password,
                FullName = user.FullName,
                Email = user.Email
            };
            var response = await controller.Register(registerRequest);

            //Assert
            mockUserRepository.Verify(u => u.Create(It.Is<UserDto>(x => x.Username == user.Username
                                                                        && x.Password == user.Password
                                                                        && x.FullName == user.FullName
                                                                        && x.Email == user.Email)));

            string cnt = await response.Content.ReadAsStringAsync();
            dynamic respData = JsonConvert.DeserializeObject(cnt);
            Assert.IsTrue(respData.UserId == user.Id);
            Assert.IsTrue(respData.Username == user.Username);
            Assert.IsTrue(respData.FullName == user.FullName);
            Assert.IsTrue(respData.Email == user.Email);

            Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
        }

        [Test]
        public async Task GetProfileData_CorrectIdAndToken_ReturnsUserDataAndOKStatusCode()
        {
            var user = new UserDto()
            {
                Id = 1,
                Username = "username",
                Password = "pass",
                FullName = "Test",
                Email = "Test"
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockUserRepository.Setup(u => u.Get(user.Id)).Returns(user);

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var response = await controller.GetProfileData(user.Id);

            //Assert
            string cnt = await response.Content.ReadAsStringAsync();
            dynamic respData = JsonConvert.DeserializeObject(cnt);
            Assert.IsTrue(respData.Username == user.Username);
            Assert.IsTrue(respData.FullName == user.FullName);
            Assert.IsTrue(respData.Email == user.Email);

            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public async Task UpdateProfileData_CorrectData_ReturnsUpdatedInfoAndOkStatusCode()
        {
            var user = new UserDto()
            {
                Id = 1,
                FullName = "Normal",
                Email = "Normal"
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockUserRepository.Setup(u => u.Get(user.Id)).Returns(user);
            mockUserRepository.Setup(u => u.UpdateNameAndEmail(It.IsAny<UserDto>())).Returns<UserDto>(x => x);

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var updateRequest = new UpdateProfileRequest()
            {
                FullName = "Updated",
                Email = "Updated"
            };
            var response = await controller.UpdataProfileData(user.Id, updateRequest);

            //Assert
            string cnt = await response.Content.ReadAsStringAsync();
            dynamic respData = JsonConvert.DeserializeObject(cnt);
            Assert.IsTrue(respData.UserId == user.Id);
            Assert.IsTrue(respData.FullName == updateRequest.FullName);
            Assert.IsTrue(respData.Email == updateRequest.Email);

            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void UpdateProfileData_DataNotGiven_ThrowsBadRequestException()
        {
            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(1, It.IsAny<string>())).Returns(true);

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            Assert.Throws<BadRequestException>(() => controller.UpdataProfileData(  1,
                                                                                    new UpdateProfileRequest()
                                                                                    {
                                                                                        FullName = null,
                                                                                        Email = string.Empty
                                                                                    }));
        }

        [Test]
        public async Task ChangePassword_ValidData_PasswordIsUpdatedAndReturnsNoContentSatusCode()
        {
            var user = new UserDto() { Id = 1 };
            var changePass = new ChangePasswordRequest() { OldPassword = "old", NewPassword = "new" };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var chgPassReq = new ChangePasswordRequest()
            {
                OldPassword = "oldPassword",
                NewPassword = "newPassword"
            };
            var response = await controller.ChangePassword(user.Id, chgPassReq);

            //Assert
            mockUserRepository.Verify(u => u.UpdatePassword(user.Id, chgPassReq.OldPassword, chgPassReq.NewPassword));
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Test]
        public async Task DeleteAccount_ValidPassword_AccountIsDeletedAndReturnsNoContentStatusCode()
        {
            var user = new UserDto() { Id = 1 };
            var delAccReq = new DeleteAccountRequest() { Password = "password" };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockUserRepository.Setup(u => u.IsPasswordCorrect(user.Id, delAccReq.Password)).Returns(true);

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var response = await controller.DeleteAccount(user.Id, delAccReq);

            //Assert
            mockUserRepository.Verify(u => u.Delete(user.Id));
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Test]
        public void DeleteAccount_InvalidPassword_ThrowsForbiddenException()
        {
            var user = new UserDto() { Id = 1 };
            var delAccReq = new DeleteAccountRequest() { Password = "password" };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockUserRepository.Setup(u => u.IsPasswordCorrect(user.Id, delAccReq.Password)).Returns(false); //simulate wrong pass

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            Assert.Throws<ForbiddenException>(() => controller.DeleteAccount(user.Id, delAccReq));
        }

        [Test]
        public async Task AddMoney_ValidData_ReturnsNewBalanceAndOKStatusCode()
        {
            var user = new UserDto()
            {
                Id = 1,
                Money = 1234
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockUserRepository.Setup(u => u.AddMoney(user.Id, user.Money)).Returns(user);

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var addMoneyRequest = new AddMoneyRequest()
            {
                AddMoney = user.Money
            };
            var response = await controller.AddMoney(user.Id, addMoneyRequest);

            //Assert
            string cnt = await response.Content.ReadAsStringAsync();
            dynamic respData = JsonConvert.DeserializeObject(cnt);
            Assert.IsTrue(respData.NewBalance == addMoneyRequest.AddMoney);

            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public async Task GetBalance_ExistingUser_ReturnsBalanceAndOKStatusCode()
        {
            var user = new UserDto()
            {
                Id = 1,
                Money = 1234
            };

            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(user.Id, It.IsAny<string>())).Returns(true);
            mockUserRepository.Setup(u => u.Get(user.Id)).Returns(user);

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act
            var response = await controller.GetBalance(user.Id);

            //Assert
            string cnt = await response.Content.ReadAsStringAsync();
            dynamic respData = JsonConvert.DeserializeObject(cnt);
            Assert.IsTrue(respData.Balance == user.Money);

            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void AllMethodsWithTokenAuth_InvalidToken_ThrowsForbiddenException()
        {
            //Arrange
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<ILoginRepository> mockLoginRepository = new Mock<ILoginRepository>();

            mockLoginRepository.Setup(l => l.HasUserAndToken(1, "token")).Returns(false); // simulate invalid token

            var controller = new UserController(mockUserRepository.Object, mockLoginRepository.Object);
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Add("OnlineCasino-Token", "token");
            controller.Request = requestMessage;
            controller.Configuration = new HttpConfiguration();

            //Act / Assert
            Assert.Throws<ForbiddenException>(() => controller.GetProfileData(1));
            Assert.Throws<ForbiddenException>(() => controller.UpdataProfileData(1, new UpdateProfileRequest()));
            Assert.Throws<ForbiddenException>(() => controller.ChangePassword(1, new ChangePasswordRequest()));
            Assert.Throws<ForbiddenException>(() => controller.DeleteAccount(1, new DeleteAccountRequest()));
            Assert.Throws<ForbiddenException>(() => controller.AddMoney(1, new AddMoneyRequest()));
            Assert.Throws<ForbiddenException>(() => controller.GetBalance(1));
        }

    }
}
