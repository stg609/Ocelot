namespace Ocelot.DownstreamUrlCreator.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class DownstreamFilterMiddlewareExtensions
    {
        public static IApplicationBuilder UseDownstreamFilterMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DownstreamFilterMiddleware>();
        }
    }
}
