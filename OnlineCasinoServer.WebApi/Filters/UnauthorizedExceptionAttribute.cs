using OnlineCasinoServer.Core.Exceptions;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace OnlineCasinoServer.WebApi.Filters
{
    public class UnauthorizedExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is UnauthorizedException)
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.Unauthorized,
                    actionExecutedContext.Exception.Message);

            base.OnException(actionExecutedContext);
        }
    }
}