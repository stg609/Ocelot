namespace Ocelot.DownstreamUrlCreator.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamUrlCreator.UrlTemplateReplacer;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System;
    using System.Threading.Tasks;
    using System.Linq.Expressions;
    using Microsoft.Extensions.DependencyInjection;
    using System.Net.Http;
    using System.Net;
    using System.IO;
    using System.Text;

    public class DownstreamFilterMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDownstreamPathPlaceholderReplacer _replacer;

        public DownstreamFilterMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IDownstreamPathPlaceholderReplacer replacer
            )
                : base(loggerFactory.CreateLogger<DownstreamFilterMiddleware>())
        {
            _next = next;
            _replacer = replacer;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var downstreamReRoute = httpContext.Items.DownstreamReRoute();

            if (downstreamReRoute.DownstreamFilters == null || downstreamReRoute.DownstreamFilters.Count == 0)
            {
                await _next.Invoke(httpContext);
            }

            var funcType = Expression.GetDelegateType(typeof(HttpContext), typeof(object[]), typeof(Task<bool>));
            foreach (var filter in downstreamReRoute.DownstreamFilters)
            {
                Logger.LogDebug($"Find filter {filter.Type}");

                var svc = httpContext.RequestServices.GetRequiredService(Type.GetType(filter.Type));
                var method = svc.GetType().GetMethod("BeforeAction");

                var func = (Func<HttpContext, object[], Task<bool>>)Delegate.CreateDelegate(funcType, svc, method);
                if (!await func(httpContext, filter.Params))
                {
                    Logger.LogDebug($"Filter {filter.Type} is failed.");
                    httpContext.Items.UpsertDownstreamResponse(new DownstreamResponse(GetResponseMessage(httpContext)));
                    return;
                }

                Logger.LogDebug($"Filter {filter.Type} is succeed.");
            }

            await _next.Invoke(httpContext);
        }

        private HttpResponseMessage GetResponseMessage(HttpContext httpContext)
        {
            var http = new HttpResponseMessage((HttpStatusCode)httpContext.Response.StatusCode);

            using (var reader = new StreamReader(httpContext.Response.Body))
            {
                var objText = reader.ReadToEnd();
                http.Content = new StringContent(objText, Encoding.UTF8, "application/json");
            }

            return http;
        }
    }
}
