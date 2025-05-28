namespace WebAPI.Extensions
{
    public class SecurityStampValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityStampValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenStamp = context.User.FindFirst("securityStamp")?.Value;

                if (userId != null && tokenStamp != null)
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var currentStamp = await userManager.GetSecurityStampAsync(user);
                        if (tokenStamp != currentStamp)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Token không hợp lệ do thông tin bảo mật đã thay đổi.");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
