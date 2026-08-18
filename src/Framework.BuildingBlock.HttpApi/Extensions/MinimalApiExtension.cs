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

                var redirectPath = pathBase.Add(path);

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
