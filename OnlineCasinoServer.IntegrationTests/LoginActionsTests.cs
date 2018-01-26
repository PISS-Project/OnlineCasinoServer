using OnlineCasinoServer.WebApi.Requests;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OnlineCasinoServer.IntegrationTests
{
    [TestFixture]
    public class LoginActionsTests
    {
        [Test]
        public async Task LoginAndTryLogoutWithDifferentTokenCheckIfNotFoundReturnedAndThenLogout()
        {
            // Arrange
            var loginRequest = new LoginRequest()
            {
                Username = TestHelper.TestUser.Username,
                Password = TestHelper.TestUser.Password
            };

            var json = JsonConvert.SerializeObject(loginRequest);
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/logins/",
                                                            method: HttpMethod.Post,
                                                            json: json);

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            Assert.IsTrue(object.Equals(TestHelper.TestUser.Id.ToString(), jsonResponse["userId"]));
            string token = jsonResponse["token"];
            string loginId = jsonResponse["id"];

            // Try logout with token of the other login of this user

            // Arrange 2
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/logins/" + loginId,
                                                        method: HttpMethod.Delete,
                                                        token: TestHelper.TestLogin.Token);

            // Act 2
            response = await TestHelper.Client.SendAsync(request);

            // Assert 2
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);

            // Now logout this session

            // Arrange 3
            request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/logins/" + loginId,
                                                        method: HttpMethod.Delete,
                                                        token: token);

            // Act 3
            response = await TestHelper.Client.SendAsync(request);

            // Assert 3
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Test]
        public async Task TryToLoginWithWrongPassworAndCheckIfBadRequestReturned()
        {
            // Arrange
            var loginRequest = new LoginRequest()
            {
                Username = TestHelper.TestUser.Username,
                Password = "wrongPass"
            };

            var json = JsonConvert.SerializeObject(loginRequest);
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/logins/",
                                                            method: HttpMethod.Post,
                                                            json: json);

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task TryLoginWithNonExistantUserAndCheckIfNotFoundReturned()
        {
            // Arrange
            var loginRequest = new LoginRequest()
            {
                Username = "NoSuchUserPleaseOrTestBreaks",
                Password = "password"
            };

            var json = JsonConvert.SerializeObject(loginRequest);
            var request = TestHelper.GenerateRequestMessage(url: "http://localhost/api/logins/",
                                                            method: HttpMethod.Post,
                                                            json: json);

            // Act
            var response = await TestHelper.Client.SendAsync(request);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
        }
    }
}
