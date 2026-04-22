// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Server.Doors.Systems;
using Content.Shared._FreakStation.ERP.Parasites;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Doors.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server._FreakStation.ERP.Parasites;

public sealed class ParasiteAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DoorSystem _doors = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParasiteStageComponent, ParasiteSummonTentacleAction>(OnTentacleAction);
        SubscribeLocalEvent<ParasiteStageComponent, ParasiteDashActionEvent>(OnDashAction);
        SubscribeLocalEvent<ParasiteStageComponent, ParasiteLayEggsActionEvent>(OnLayEggsAction);
        SubscribeLocalEvent<ParasiteStageComponent, ParasiteHealActionEvent>(OnHealAction);
        SubscribeLocalEvent<ParasiteChimeraComponent, ChimeraDashActionEvent>(OnChimeraDashAction);
        // Door hack removed - now passive via PryingComponent
    }

    private void OnTentacleAction(EntityUid uid, ParasiteStageComponent component, ParasiteSummonTentacleAction args)
    {
        if (args.Handled)
            return;

        // Spawn tentacle effect at target location (like Goliath)
        var tentacleSpawn = Spawn("EffectParasiteTentacleSpawn", args.Target);

        _popup.PopupEntity("Вы призываете щупальца!", uid, uid);
        args.Handled = true;
    }

    private void OnDashAction(EntityUid uid, ParasiteStageComponent component, ParasiteDashActionEvent args)
    {
        if (!TryComp<PhysicsComponent>(uid, out var physics))
            return;

        var xform = Transform(uid);
        var direction = xform.LocalRotation.ToWorldVec();
        var dashDistance = 3f;
        var dashSpeed = 15f;

        // Apply impulse for dash
        _physics.SetLinearVelocity(uid, direction * dashSpeed, body: physics);

        // Check for collision with monkeys (for mouse stage)
        if (component.Stage == ParasiteStage.Mouse)
        {
            var entities = _lookup.GetEntitiesInRange(xform.MapPosition, 1f);
            foreach (var target in entities)
            {
                if (target == uid)
                    continue;

                // Check if target is a monkey
                if (HasComp<ParasiteStageComponent>(target))
                    continue;

                // Try to infect
                var parasiteSystem = EntityManager.System<ParasiteSystem>();
                if (parasiteSystem.TryInfect(uid, target))
                {
                    _popup.PopupEntity("Вы заражаете цель!", uid, uid);
                    _popup.PopupEntity("Что-то вонзилось в вас!", target, target, PopupType.LargeCaution);
                }
            }
        }

        _popup.PopupEntity("Вы делаете стремительный рывок!", uid, uid);
    }

    private void OnLayEggsAction(EntityUid uid, ParasiteStageComponent component, ParasiteLayEggsActionEvent args)
    {
        if (component.Stage != ParasiteStage.Monkey)
            return;

        // Spawn unfertilized egg
        var egg = Spawn("ParasiteUnfertilizedEgg", Transform(uid).Coordinates);

        // Try to put in hand
        if (TryComp<HandsComponent>(uid, out var hands))
        {
            _popup.PopupEntity("Вы откладываете яйцо паразита.", uid, uid);
        }
    }

    private void OnHealAction(EntityUid uid, ParasiteStageComponent component, ParasiteHealActionEvent args)
    {
        if (component.Stage != ParasiteStage.Monkey)
            return;

        // Heal damage
        var healAmount = new DamageSpecifier();
        healAmount.DamageDict.Add("Brute", -20);
        healAmount.DamageDict.Add("Burn", -20);

        _damageable.TryChangeDamage(uid, healAmount);
        _popup.PopupEntity("Вы регенерируете свои раны.", uid, uid);
    }

    private void OnChimeraDashAction(EntityUid uid, ParasiteChimeraComponent component, ChimeraDashActionEvent args)
    {
        if (!TryComp<PhysicsComponent>(uid, out var physics))
            return;

        var xform = Transform(uid);
        var direction = xform.LocalRotation.ToWorldVec();
        var dashDistance = 5f;
        var dashSpeed = 20f;

        // Apply powerful impulse
        _physics.SetLinearVelocity(uid, direction * dashSpeed, body: physics);

        // Stun everyone in path
        var entities = _lookup.GetEntitiesInRange(xform.MapPosition, dashDistance);
        foreach (var target in entities)
        {
            if (target == uid)
                continue;

            // Stun target
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(3), true);

            // Deal damage
            var damage = new DamageSpecifier();
            damage.DamageDict.Add("Blunt", 20);
            _damageable.TryChangeDamage(target, damage);

            _popup.PopupEntity("Химера сбивает вас с ног!", target, target, PopupType.LargeCaution);
        }

        _popup.PopupEntity("Вы совершаете мощный прыжок, сбивая всех на пути!", uid, uid);
    }
}
