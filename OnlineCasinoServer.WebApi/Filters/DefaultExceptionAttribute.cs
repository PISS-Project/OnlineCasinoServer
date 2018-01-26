using OnlineCasinoServer.Core.Exceptions;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace OnlineCasinoServer.WebApi.Filters
{
    public class DefaultExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError,
                actionExecutedContext.Exception.Message);

            base.OnException(actionExecutedContext);
        }
    }
}