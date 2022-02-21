using Octokit;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.BranchProtectionRule;

namespace GitHub.Branch.Protections.Service;

public class GitHubEvent : WebhookEventProcessor
{
    private readonly IGitHubClient _gitHubClient;
    private readonly ILogger<GitHubEvent> _logger;

    public GitHubEvent(IGitHubClient githubClient, ILogger<GitHubEvent> logger)
    {
        _gitHubClient = githubClient;
        _logger = logger;
    }

    private async Task ApplyBranchProtectionPolicy(Repository repository)
    {
        var branchProtectionRequiredReviewsUpdate = new BranchProtectionRequiredReviewsUpdate(true, true, 1);

        var branchProtectionSettingsUpdate = new BranchProtectionSettingsUpdate(branchProtectionRequiredReviewsUpdate)
        {
            EnforceAdmins = true
        };

        await _gitHubClient.Repository.Branch.UpdateBranchProtection(repository.Id, repository.DefaultBranch, branchProtectionSettingsUpdate);
    }

    protected override async Task ProcessBranchProtectionRuleWebhookAsync(WebhookHeaders headers, BranchProtectionRuleEvent branchProtectionRuleEvent,
        BranchProtectionRuleAction action)
    {
        if (branchProtectionRuleEvent.Repository == null)
            throw new ArgumentNullException(nameof(branchProtectionRuleEvent.Repository), "The repository cannot be null");

        if (action == BranchProtectionRuleActionValue.Created) return;

        if (action == BranchProtectionRuleAction.Deleted && branchProtectionRuleEvent.Rule.Name == branchProtectionRuleEvent.Repository.DefaultBranch)
        {
            var repository = await _gitHubClient.Repository.Get(branchProtectionRuleEvent.Repository.Id);

            await ValidateBranchProtections(repository);
        }
    }

    protected override Task ProcessPingWebhookAsync(WebhookHeaders headers, PingEvent pingEvent)
    {
        _logger.LogInformation("Ping event received");

        return Task.FromResult(0);
    }

    protected override async Task ProcessPushWebhookAsync(WebhookHeaders headers, PushEvent pushEvent)
    {
        if (pushEvent.Repository == null) throw new ArgumentNullException(nameof(pushEvent.Repository), "The repository cannot be null");

        // Only care about the default branch (i.e., main)
        if (pushEvent.Ref.EndsWith(pushEvent.Repository.DefaultBranch))
        {
            var repository = await _gitHubClient.Repository.Get(pushEvent.Repository.Id);

            await ValidateBranchProtections(repository);
        }
    }

    private async Task ValidateBranchProtections(Repository repository)
    {
        var defaultBranch = await _gitHubClient.Repository.Branch.Get(repository.Id, repository.DefaultBranch);

        if (defaultBranch.Protected)
        {
            var branchProtectionSettings = await _gitHubClient.Repository.Branch.GetBranchProtection(repository.Id, defaultBranch.Name);

            if (branchProtectionSettings.EnforceAdmins.Enabled &&
                branchProtectionSettings.RequiredPullRequestReviews.RequireCodeOwnerReviews &&
                branchProtectionSettings.RequiredPullRequestReviews.RequiredApprovingReviewCount >= 1)
                return;
        }

        await ApplyBranchProtectionPolicy(repository);
    }
}
