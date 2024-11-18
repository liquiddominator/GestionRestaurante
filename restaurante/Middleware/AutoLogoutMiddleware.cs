using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace restaurante.Middleware
{
    public class AutoLogoutMiddleware
    {
        private readonly RequestDelegate _next;

        public AutoLogoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Auth"))
            {
                // Si la solicitud es para una acción de Auth, cerrar sesión automáticamente
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            await _next(context);
        }
    }

    public static class AutoLogoutMiddlewareExtensions
    {
        public static IApplicationBuilder UseAutoLogout(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AutoLogoutMiddleware>();
        }
    }
}
