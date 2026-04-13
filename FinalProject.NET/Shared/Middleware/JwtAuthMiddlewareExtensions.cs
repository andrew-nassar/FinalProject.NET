namespace FinalProject.NET.Shared.Middleware
{
    public static class JwtAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtAuthMiddleware>();
        }
    }
}
