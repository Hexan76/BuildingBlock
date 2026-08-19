using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

public static class MinimalApiExtension
{
    public static IApplicationBuilder UseRedirectFromHome(
        this IApplicationBuilder app, string path)
    {
        app.UseEndpoints(endpoints =>
        {
           endpoints.MapGet("/", context =>
            {
                var pathBase = context.Request.PathBase;

                // Kong commonly provides the external route prefix
                // through X-Forwarded-Prefix.
                if (string.IsNullOrEmpty(pathBase))
                {
                    var forwardedPrefix =
                        context.Request.Headers["X-Forwarded-Prefix"].FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(forwardedPrefix))
                    {
                        pathBase = new PathString(
                            "/" + forwardedPrefix.Trim('/'));
                    }
                }

                var redirectPath = pathBase.Add(
                    new PathString("/" + path.Trim('/')));

                context.Response.Redirect(redirectPath);

                return Task.CompletedTask;
            });

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });

        return app;
    }
}
