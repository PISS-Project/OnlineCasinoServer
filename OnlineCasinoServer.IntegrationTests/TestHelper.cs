using OnlineCasinoServer.Core.DTOs;
using System;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace OnlineCasinoServer.IntegrationTests
{
    public static class TestHelper
    {
        public static HttpServer Server;
        public static HttpClient Client;
        public static UserDto TestUser;
        public static LoginDto TestLogin;

        public static HttpRequestMessage GenerateRequestMessage(string url, HttpMethod method, string token = null, string json = null)
        {
            var request = new HttpRequestMessage();

            request.RequestUri = new Uri(url);
            request.Method = method;

            if (token != null)
            {
                request.Headers.Add("OnlineCasino-Token", token);
            }

            if (json != null)
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return request;
        }
    }
}
