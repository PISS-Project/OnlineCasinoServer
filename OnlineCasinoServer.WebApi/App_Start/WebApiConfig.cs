using Autofac;
using Autofac.Integration.WebApi;
using Newtonsoft.Json.Serialization;
using OnlineCasinoServer.Core.Repositories;
using OnlineCasinoServer.WebApi.Filters;
using System.Reflection;
using System.Web.Http;

namespace OnlineCasinoServer.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "OnlineCasinoApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;

            config.Filters.Add(new DiceGamingAuthorizationFilterAttribute());
            config.Filters.Add(new BadRequestExceptionAttribute());
            config.Filters.Add(new ValidateModelAttribute());
            config.Filters.Add(new ConflictExceptionAttribute());
            config.Filters.Add(new ForbiddenExceptionAttribute());
            config.Filters.Add(new NotFoundExceptionAttribute());
            config.Filters.Add(new UnauthorizedExceptionAttribute());
            config.Filters.Add(new DefaultExceptionAttribute());

            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterWebApiFilterProvider(config);

            builder.RegisterType<UserRepository>().As(typeof(IUserRepository)).SingleInstance();
            builder.RegisterType<LoginRepository>().As(typeof(ILoginRepository)).SingleInstance();
            builder.RegisterType<BetRepository>().As(typeof(IBetRepository)).SingleInstance();

            var container = builder.Build();

            //var repo = container.Resolve<UserRepository>();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            config.EnsureInitialized();
        }
    }
}
