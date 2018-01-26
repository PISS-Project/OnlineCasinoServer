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
    public class UserActionsTests
    {
        [Test]
        public async Task RegisterNewUserAndLoginAndDeleteAccount()
        {
            var user = new UserDto()
            {
                Username = "_registerTest",
                Password = "password",
                FullName = "Fast Tester",
                Email = "test@fast.com"
            };

            // Register new user

            // Arrange
            var registerRequest = new RegisterRequest()
            {
                Username = user.Username,
                Password = user.Password,
                FullName = user.FullName,
                Email = user.Email
            };

            var json = JsonConvert.SerializeObject(registerRequest);
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/",
                                                            method: HttpMethod.Post,
                                                            json: json);

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
            string content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            Assert.IsTrue(object.Equals(jsonResponse["username"], user.Username));
            Assert.IsTrue(object.Equals(jsonResponse["fullName"], user.FullName));
            Assert.IsTrue(object.Equals(jsonResponse["email"], user.Email));

            user.Id = int.Parse(jsonResponse["userId"]);

            //Login into account

            // Arrange 2
            var loginRequest = new LoginRequest()
            {
                Username = user.Username,
                Password = user.Password
            };

            json = JsonConvert.SerializeObject(loginRequest);
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/logins/",
                                                        method: HttpMethod.Post,
                                                        json: json);

            // Act 2
            response = await TestHelper.Client.SendAsync(request);

            // Assert 2
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);

            content = await response.Content.ReadAsStringAsync();
            jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            string token = jsonResponse["token"];

            // Delete account

            // Arrange 3
            var deleteRequest = new DeleteAccountRequest()
            {
                Password = user.Password
            };

            json = JsonConvert.SerializeObject(deleteRequest);
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + user.Id.ToString(),
                                                        method: HttpMethod.Delete,
                                                        token: token,
                                                        json: json);

            // Act 3
            response = await TestHelper.Client.SendAsync(request);

            // Assert 3
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);

            using (var db = new OnlineCasinoDb())
            {
                var deletedUser = db.Users.FirstOrDefault(u => u.Id == user.Id);

                Assert.IsTrue(deletedUser == null);
            }
        }

        [Test]
        public async Task RegisterWithIncorrectDataAndCheckIfBadRequestReturned()
        {
            var user = new UserDto()
            {
                Username = "_registerTest",
                Password = "password",
                FullName = "Fast Tester",
                Email = "test@fast.com"
            };

            // Arrange
            var shortPassRequest = new RegisterRequest()
            {
                Username = user.Username,
                Password = "short",
                FullName = user.FullName,
                Email = user.Email
            };

            var noNameRequest = new RegisterRequest()
            {
                Username = user.Username,
                Password = user.Password,
                Email = user.Email
            };

            var noMailRequest = new RegisterRequest()
            {
                Username = user.Username,
                Password = user.Password,
                FullName = user.FullName
            };

            var noUsernameRequest = new RegisterRequest()
            {
                Password = user.Password,
                FullName = user.FullName,
                Email = user.Email
            };

            // Act / Assert
            string json = JsonConvert.SerializeObject(shortPassRequest);
            HttpRequestMessage request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/", method: HttpMethod.Post, json: json);
            HttpResponseMessage response = await TestHelper.Client.SendAsync(request);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);

            json = JsonConvert.SerializeObject(noNameRequest);
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/", method: HttpMethod.Post, json: json);
            response = await TestHelper.Client.SendAsync(request);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);

            json = JsonConvert.SerializeObject(noMailRequest);
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/", method: HttpMethod.Post, json: json);
            response = await TestHelper.Client.SendAsync(request);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);

            json = JsonConvert.SerializeObject(noUsernameRequest);
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/", method: HttpMethod.Post, json: json);
            response = await TestHelper.Client.SendAsync(request);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetProfileDataAndUpdateItAndCheckIfUpdated()
        {
            string userIdString = TestHelper.TestUser.Id.ToString();

            // Get profile data

            // Arrange
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString,
                                                            method: HttpMethod.Get,
                                                            token: TestHelper.TestLogin.Token);
            
            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            var initialUserData = new UserDto()
            {
                Username = jsonResponse["username"],
                FullName = jsonResponse["fullName"],
                Email = jsonResponse["email"]
            };

            // Update user data

            // Arrange 2
            var updateRequest = new UpdateProfileRequest()
            {
                FullName = initialUserData.FullName + "Updated",
                Email = initialUserData.Email + "Updated"
            };

            var json = JsonConvert.SerializeObject(updateRequest);
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/profile",
                                                        method: HttpMethod.Put,
                                                        token: TestHelper.TestLogin.Token,
                                                        json: json);

            // Act 2
            response = await TestHelper.Client.SendAsync(request);

            // Assert 2
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            Assert.IsTrue(object.Equals(jsonResponse["fullName"], updateRequest.FullName));
            Assert.IsTrue(object.Equals(jsonResponse["email"], updateRequest.Email));

            // Get profile data again to see if updated

            // Arrange 3
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString,
                                                        method: HttpMethod.Get,
                                                        token: TestHelper.TestLogin.Token);

            // Act 3
            response = await TestHelper.Client.SendAsync(request);

            // Assert 3
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            Assert.IsTrue(object.Equals(jsonResponse["fullName"], updateRequest.FullName));
            Assert.IsTrue(object.Equals(jsonResponse["email"], updateRequest.Email));
        }

        [Test]
        public async Task TryGetUserInfoWithInvalidTokenAndCheckIfUnauthorizedReturned()
        {
            string userIdString = TestHelper.TestUser.Id.ToString();

            // Get profile data

            // Arrange
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString,
                                                            method: HttpMethod.Get,
                                                            token: "invalidToken");

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetUserBalanceAndAddMoneyAndCheckIfBalanceIsUpdated()
        {
            string userIdString = TestHelper.TestUser.Id.ToString();

            // Get user balance

            // Arrange
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/wallet",
                                                            method: HttpMethod.Get,
                                                            token: TestHelper.TestLogin.Token);

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            var initialBalance = decimal.Parse(jsonResponse["balance"]);

            // Add money to user account

            // Arrange 2
            var addMoneyRequest = new AddMoneyRequest()
            {
                AddMoney = 5000
            };

            var json = JsonConvert.SerializeObject(addMoneyRequest);
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/wallet",
                                                        method: HttpMethod.Put,
                                                        token: TestHelper.TestLogin.Token,
                                                        json: json);

            // Act 2
            response = await TestHelper.Client.SendAsync(request);

            // Assert 2
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            
            var newBalance = decimal.Parse(jsonResponse["newBalance"]);

            Assert.IsTrue(newBalance == initialBalance + addMoneyRequest.AddMoney);

            // Get balance data again to see if updated

            // Arrange 3
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/wallet",
                                                        method: HttpMethod.Get,
                                                        token: TestHelper.TestLogin.Token);

            // Act 3
            response = await TestHelper.Client.SendAsync(request);

            // Assert 3
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            content = await response.Content.ReadAsStringAsync();
            jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            var balance = decimal.Parse(jsonResponse["balance"]);
            Assert.IsTrue(balance == newBalance);

            TestHelper.TestUser.Money = balance;
        }

        [Test]
        public async Task ChangePasswordOfUserAndLoginAgainWithNewPassword()
        {
            string userIdString = TestHelper.TestUser.Id.ToString();

            // Save user current password and salt, to revert it after test
            string initialPasswordHash;
            string initialPasswordSalt;
            using (var db = new OnlineCasinoDb())
            {
                var user = db.Users.First(u => u.Id == TestHelper.TestUser.Id);

                initialPasswordHash = user.Password;
                initialPasswordSalt = user.Salt;
            }

            // Change user password
            // Arrange
            var changePasswordRequest = new ChangePasswordRequest()
            {
                OldPassword = TestHelper.TestUser.Password,
                NewPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            };

            var json = JsonConvert.SerializeObject(changePasswordRequest);
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/password",
                                                            method: HttpMethod.Put,
                                                            token: TestHelper.TestLogin.Token,
                                                            json: json);

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);

            // Login with new password

            // Arrange 2
            var loginRequest = new LoginRequest()
            {
                Username = TestHelper.TestUser.Username,
                Password = changePasswordRequest.NewPassword
            };

            json = JsonConvert.SerializeObject(loginRequest);
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/logins/",
                                                        method: HttpMethod.Post,
                                                        json: json);

            // Act 2
            response = await TestHelper.Client.SendAsync(request);

            // Assert 2
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            Assert.IsTrue(object.Equals(userIdString, jsonResponse["userId"]));
            string token = jsonResponse["token"];
            string loginId = jsonResponse["id"];

            // Logout the user from this session

            // Arrange 3
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/logins/" + loginId,
                                                        method: HttpMethod.Delete,
                                                        token: token);

            // Act 3
            response = await TestHelper.Client.SendAsync(request);

            // Assert 3
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);

            // Revert user password and salt
            using (var db = new OnlineCasinoDb())
            {
                var user = db.Users.First(u => u.Id == TestHelper.TestUser.Id);

                user.Password = initialPasswordHash;
                user.Salt = initialPasswordSalt;

                db.Users.AddOrUpdate(user);
                db.SaveChanges();
            }
        }

        [Test]
        public async Task ChangePasswordButGiveTooShortPasswordAndCheckIfBadRequestReturned()
        {
            string userIdString = TestHelper.TestUser.Id.ToString();

            // Arrange
            var changePasswordRequest = new ChangePasswordRequest()
            {
                OldPassword = TestHelper.TestUser.Password,
                NewPassword = "short"
            };

            var json = JsonConvert.SerializeObject(changePasswordRequest);
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/users/" + userIdString + "/password",
                                                            method: HttpMethod.Put,
                                                            token: TestHelper.TestLogin.Token,
                                                            json: json);

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }
    }
}
