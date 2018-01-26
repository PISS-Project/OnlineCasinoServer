using OnlineCasinoServer.Core.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace OnlineCasinoServer.WebApi.Filters
{
    public class DiceGamingAuthorizationFilterAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (SkipAuthorization(actionContext))
                return;

            if (!Authorize(actionContext))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "No rights to access this resource!");
            }

            return;
        }

        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                       || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }

        private bool Authorize(HttpActionContext actionContext)
        {
            var request = actionContext.Request;

            IEnumerable<string> values;
            if (!request.Headers.TryGetValues("OnlineCasino-Token", out values))
                return false;

            var token = values.FirstOrDefault();

            return TokenManager.IsTokenPresent(token);
        }
    }
}