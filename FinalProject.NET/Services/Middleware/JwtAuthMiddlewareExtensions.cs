namespace FinalProject.NET.Services.Middleware
{
    public static class JwtAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtAuthMiddleware>();
        }
    }
}
