using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> BoostyWebhookEnabled =
        CVarDef.Create("boosty.webhook_enabled", false, CVar.SERVERONLY);

    public static readonly CVarDef<string> BoostyWebhookSecret =
        CVarDef.Create("boosty.webhook_secret", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> BoostyWebhookPath =
        CVarDef.Create("boosty.webhook_path", "/api/boosty/webhook", CVar.SERVERONLY);
}
