using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server._Mini.AntagTokens;
using Content.Shared.CCVar;
using Content.SponsorImplementations.Shared;
using Robust.Server.ServerStatus;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Network;

namespace Content.Server._Sponsor;

public sealed class BoostyWebhookSystem : EntitySystem
{
    [Dependency] private readonly IStatusHost _statusHost = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;
    private string _webhookSecret = string.Empty;
    private string _webhookPath = "/api/boosty/webhook";
    private bool _enabled = false;

    private readonly Dictionary<string, SponsorTier> _tierMapping = new()
    {
        ["basic"] = new SponsorTier(1, "#87CEEB", 1, true, ["Vulpkanin"]),
        ["standard"] = new SponsorTier(2, "#FFD700", 2, true, ["Vulpkanin", "Arachnid"]),
        ["premium"] = new SponsorTier(3, "#FF69B4", 3, true, ["Vulpkanin", "Arachnid", "Felinid"]),
        ["vip"] = new SponsorTier(4, "#FF0000", 5, true, ["Vulpkanin", "Arachnid", "Felinid", "Moth"])
    };

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _logManager.GetSawmill("boosty.webhook");

        _cfg.OnValueChanged(CCVars.BoostyWebhookEnabled, OnEnabledChanged, true);
        _cfg.OnValueChanged(CCVars.BoostyWebhookSecret, OnSecretChanged, true);
        _cfg.OnValueChanged(CCVars.BoostyWebhookPath, OnPathChanged, true);
    }

    private void OnEnabledChanged(bool enabled)
    {
        _enabled = enabled;
        if (enabled)
        {
            RegisterWebhook();
            _sawmill.Info("Boosty webhook enabled");
        }
    }

    private void OnSecretChanged(string secret) => _webhookSecret = secret;
    private void OnPathChanged(string path)
    {
        _webhookPath = path;
        if (_enabled) RegisterWebhook();
    }

    private void RegisterWebhook()
    {
        _statusHost.AddHandler(HandleWebhookRequest);
        _sawmill.Info($"Registered webhook at {_webhookPath}");
    }

    private async Task<bool> HandleWebhookRequest(IStatusHandlerContext context)
    {
        if (!_enabled || context.RequestMethod != HttpMethod.Post ||
            !context.Url.AbsolutePath.Equals(_webhookPath, StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            using var reader = new StreamReader(context.RequestBody);
            var body = await reader.ReadToEndAsync();

            if (!string.IsNullOrEmpty(_webhookSecret))
            {
                var signature = context.RequestHeaders["X-Boosty-Signature"];
                if (!VerifySignature(body, signature))
                {
                    await context.RespondAsync("Invalid signature", HttpStatusCode.Unauthorized);
                    return true;
                }
            }

            var webhook = JsonSerializer.Deserialize<BoostyWebhook>(body);
            if (webhook != null)
                await ProcessWebhook(webhook);

            await context.RespondAsync("OK", HttpStatusCode.OK);
            return true;
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Error: {ex}");
            await context.RespondAsync("Error", HttpStatusCode.InternalServerError);
            return true;
        }
    }

    private bool VerifySignature(string body, string? signature)
    {
        if (string.IsNullOrEmpty(signature)) return false;
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(body));
        return signature.Equals(Convert.ToHexString(hash).ToLower(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task ProcessWebhook(BoostyWebhook webhook)
    {
        switch (webhook.Type)
        {
            case "subscription.created":
            case "subscription.updated":
                await HandleSubscriptionActive(webhook);
                break;
            case "subscription.cancelled":
            case "subscription.expired":
                await HandleSubscriptionCancelled(webhook);
                break;
        }
    }

    private async Task HandleSubscriptionActive(BoostyWebhook webhook)
    {
        var email = webhook.Data.User.Email;
        if (!_tierMapping.TryGetValue(webhook.Data.Level.Name.ToLower(), out var tier))
            return;

        var player = await _db.GetPlayerRecordByEmail(email);
        if (player == null) return;

        var sponsorData = new SponsorData
        {
            Color = Color.FromHex(tier.Color),
            ExtraCharSlots = tier.ExtraSlots,
            ServerPriorityJoin = tier.PriorityJoin,
            Prototypes = tier.Prototypes.ToList()
        };

        await _db.SetSponsordata(player.UserId, sponsorData);
        EntitySystem.Get<AntagTokenSystem>()?.SetSponsorLevelOverride(player.UserId, tier.Level);
        _sawmill.Info($"Granted tier {tier.Level} to {player.UserId}");
    }

    private async Task HandleSubscriptionCancelled(BoostyWebhook webhook)
    {
        var player = await _db.GetPlayerRecordByEmail(webhook.Data.User.Email);
        if (player == null) return;

        await _db.SetSponsordata(player.UserId, null);
        EntitySystem.Get<AntagTokenSystem>()?.SetSponsorLevelOverride(player.UserId, null);
        _sawmill.Info($"Removed sponsorship from {player.UserId}");
    }
}

public sealed class BoostyWebhook
{
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("data")] public BoostyWebhookData Data { get; set; } = new();
}

public sealed class BoostyWebhookData
{
    [JsonPropertyName("user")] public BoostyUser User { get; set; } = new();
    [JsonPropertyName("level")] public BoostyLevel Level { get; set; } = new();
}

public sealed class BoostyUser
{
    [JsonPropertyName("email")] public string Email { get; set; } = "";
}

public sealed class BoostyLevel
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public sealed record SponsorTier(int Level, string Color, int ExtraSlots, bool PriorityJoin, string[] Prototypes);
