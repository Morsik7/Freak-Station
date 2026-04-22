// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Shared._FreakStation.ERP.Parasites;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using JetBrains.Annotations;

namespace Content.Server._FreakStation.ERP.Parasites;

/// <summary>
/// Handles antiparasitic drug effects
/// </summary>
[UsedImplicitly]
public sealed partial class AntiparasiticReagent : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Kills parasites and reduces infection levels.";

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var uid = args.TargetEntity;

        // Damage parasite entities
        if (entityManager.TryGetComponent<ParasiteLarvaComponent>(uid, out _))
        {
            var damage = new DamageSpecifier();
            damage.DamageDict.Add("Poison", 50);
            entityManager.System<DamageableSystem>().TryChangeDamage(uid, damage);
            entityManager.System<SharedPopupSystem>().PopupEntity("Препарат обжигает ваше тело!", uid, uid, PopupType.LargeCaution);
            return;
        }

        // Reduce infection in hosts
        if (entityManager.TryGetComponent<ParasiteInfectionComponent>(uid, out var infection))
        {
            if (!infection.CanBeCured)
            {
                entityManager.System<SharedPopupSystem>().PopupEntity("Слишком поздно для лечения...", uid, uid);
                return;
            }

            // Reduce infection by 5% per unit
            infection.InfectionPercent -= 5f;
            infection.InfectionPercent = Math.Max(0f, infection.InfectionPercent);

            if (infection.InfectionPercent <= 0f)
            {
                entityManager.RemoveComponent<ParasiteInfectionComponent>(uid);
                entityManager.System<SharedPopupSystem>().PopupEntity("Паразит уничтожен!", uid, uid);
            }
            else
            {
                entityManager.System<SharedPopupSystem>().PopupEntity($"Заражение снижено до {infection.InfectionPercent:F0}%", uid, uid);
            }
        }
    }
}

/// <summary>
/// Handles parasite growth accelerator effects
/// </summary>
[UsedImplicitly]
public sealed partial class ParasiteGrowthAcceleratorReagent : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Accelerates parasite growth in exchange for healing. May cause parasitic limb growth.";

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var uid = args.TargetEntity;
        var random = IoCManager.Resolve<IRobustRandom>();

        if (!entityManager.TryGetComponent<ParasiteInfectionComponent>(uid, out var infection))
            return;

        // Heal the host
        var healAmount = new DamageSpecifier();
        healAmount.DamageDict.Add("Brute", -15);
        healAmount.DamageDict.Add("Burn", -15);
        entityManager.System<DamageableSystem>().TryChangeDamage(uid, healAmount);

        // Grow infection by 5-10%
        var growth = random.Next(5, 11);
        infection.InfectionPercent += growth;
        infection.InfectionPercent = Math.Min(100f, infection.InfectionPercent);

        entityManager.System<SharedPopupSystem>().PopupEntity($"Паразит растёт! Заражение: {infection.InfectionPercent:F0}%", uid, uid);

        // Chance to grow parasitic limb if missing limbs
        if (infection.LimbsSevered && !infection.ParasiticLimbsGrown && random.Prob(0.3f))
        {
            if (entityManager.TryGetComponent<BodyComponent>(uid, out var body))
            {
                var bodySystem = entityManager.System<BodySystem>();
                // TODO: Regrow one random parasitic limb
                entityManager.System<SharedPopupSystem>().PopupEntity("Паразитическая конечность начинает расти!", uid, uid);
            }
        }
    }
}
