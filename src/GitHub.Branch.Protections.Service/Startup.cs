using GitHub.Branch.Protections.Service.Factories;
using Octokit;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;

namespace GitHub.Branch.Protections.Service;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        var secret = Configuration["secret"];

        app.UseEndpoints(endpoints => { endpoints.MapGitHubWebhooks(secret: secret); });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var token = Configuration["token"];

        services.AddSingleton<IGitHubClient>(GitHubClientFactory.CreateGitHubClient(token));
        services.AddSingleton<WebhookEventProcessor, GitHubEvent>();
    }
}
