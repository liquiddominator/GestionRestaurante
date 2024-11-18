using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace restaurante.Filters
{
    public class CustomAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
            {
                return; // Permite acceso anónimo si está especificado
            }

            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                if (!IsAuthController(context))
                {
                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                }
                return;
            }

            var authorizeAttributes = context.ActionDescriptor.EndpointMetadata
                .OfType<AuthorizeAttribute>().ToList();

            if (authorizeAttributes.Any() && !IsAuthorized(context, authorizeAttributes))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
            }
        }

        private bool IsAuthController(AuthorizationFilterContext context)
        {
            return context.RouteData.Values["controller"].ToString().Equals("Auth", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsAuthorized(AuthorizationFilterContext context, IEnumerable<AuthorizeAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (string.IsNullOrEmpty(attribute.Roles))
                {
                    return true;
                }

                var roles = attribute.Roles.Split(',');
                if (roles.Any(role => context.HttpContext.User.IsInRole(role.Trim())))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
