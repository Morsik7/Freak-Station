// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._FreakStation.ERP.Parasites;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._FreakStation.ERP.Parasites;

/// <summary>
/// Handles parasite larva attacks and infection attempts
/// </summary>
public sealed class ParasiteLarvaSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ParasiteSystem _parasite = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private static readonly ProtoId<DamageTypePrototype> PierceDamage = "Piercing";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParasiteLarvaComponent, MeleeHitEvent>(OnLarvaMeleeHit);
    }

    private void OnLarvaMeleeHit(EntityUid uid, ParasiteLarvaComponent component, MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var target in args.HitEntities)
        {
            // Deal bite damage
            var damage = new DamageSpecifier();
            damage.DamageDict.Add(PierceDamage, component.BiteDamage);
            _damageable.TryChangeDamage(target, damage);

            // Try to infect
            if (_random.Prob(component.InfectionChanceOnBite))
            {
                _parasite.TryInfect(uid, target, component);
            }
        }
    }
}
