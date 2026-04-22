// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Shared._FreakStation.ERP.Parasites;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._FreakStation.ERP.Parasites;

/// <summary>
/// Anti-parasitic drug that stops infection progression.
/// If used at 85-95% infection, grants symbiosis.
/// </summary>
public sealed partial class AntiParasiticEffect : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Останавливает рост паразитарной инфекции. При правильном применении может привести к симбиозу.";

    public override void Effect(EntityEffectBaseArgs args)
    {
        var parasiteSystem = args.EntityManager.System<ParasiteSystem>();

        if (args.EntityManager.TryGetComponent<ParasiteInfectionComponent>(args.TargetEntity, out var infection))
        {
            parasiteSystem.TryStopInfection(args.TargetEntity, infection);
        }
    }
}
