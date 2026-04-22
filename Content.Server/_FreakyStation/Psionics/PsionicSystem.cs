// SPDX-FileCopyrightText: 2026 Freaky Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Server.Damage.Systems;
using Content.Server.Store.Systems;
using Content.Server.Stunnable;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Effects;
using Content.Shared.Physics;
using Content.Shared.Store.Components;
using Content.Shared.Throwing;
using Content.Shared._FreakyStation.Psionics;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._FreakyStation.Psionics;

public sealed class PsionicSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _colorFlash = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PsionicComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PsionicComponent, VectorImpulseActionEvent>(OnVectorImpulse);
        SubscribeLocalEvent<PsionicComponent, KineticDashActionEvent>(OnKineticDash);
        SubscribeLocalEvent<PsionicComponent, GravityWellActionEvent>(OnGravityWell);
        SubscribeLocalEvent<PsionicComponent, CrushingPulseActionEvent>(OnCrushingPulse);
        SubscribeLocalEvent<PsionicComponent, MolecularBlinkActionEvent>(OnMolecularBlink);
        SubscribeLocalEvent<PsionicComponent, PhaseShiftActionEvent>(OnPhaseShift);
        SubscribeLocalEvent<PsionicComponent, BioSurgeActionEvent>(OnBioSurge);
        SubscribeLocalEvent<PsionicComponent, NeuroshockWaveActionEvent>(OnNeuroshockWave);
        SubscribeLocalEvent<PsionicComponent, OpenPsionicStudyMenuActionEvent>(OnOpenStudyMenuAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        PassiveRegen(frameTime);
    }

    private void OnStartup(EntityUid uid, PsionicComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.StudyMenuAction, component.StudyMenuActionId);
        EnsurePsionicStore(uid, component);
        SyncAbilityActions(uid, component);
        UpdateHeatAlert(uid, component);
    }

    private void OnShutdown(EntityUid uid, PsionicComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.StudyMenuAction);
        _actions.RemoveAction(uid, component.VectorImpulseAction);
        _actions.RemoveAction(uid, component.KineticDashAction);
        _actions.RemoveAction(uid, component.GravityWellAction);
        _actions.RemoveAction(uid, component.CrushingPulseAction);
        _actions.RemoveAction(uid, component.MolecularBlinkAction);
        _actions.RemoveAction(uid, component.PhaseShiftAction);
        _actions.RemoveAction(uid, component.BioSurgeAction);
        _actions.RemoveAction(uid, component.NeuroshockWaveAction);
        _alerts.ClearAlert(uid, component.HeatAlert);
    }

    private void EnsurePsionicStore(EntityUid uid, PsionicComponent component)
    {
        var store = EnsureComp<StoreComponent>(uid);
        store.Name = "store-psionic-title";
        store.RefundAllowed = false;
        store.OwnerOnly = true;

        store.CurrencyWhitelist.Add(component.PsionicPointCurrency);
        if (!store.Balance.ContainsKey(component.PsionicPointCurrency))
            store.Balance[component.PsionicPointCurrency] = component.StartingPsionicPoints;

        foreach (var category in component.StoreCategories)
        {
            store.Categories.Add(category);
        }

        foreach (var listing in component.StoreListings)
        {
            var listingId = listing.ToString();
            if (store.Listings.Any(l => l.ID == listingId))
                continue;

            _store.TryAddListing(store, listingId);
        }

        _store.RefreshAllListings(store);
    }

    private void OnVectorImpulse(EntityUid uid, PsionicComponent component, VectorImpulseActionEvent args)
    {
        if (args.Handled)
            return;

        if (!component.LearnedAbilities.Contains(PsionicAbility.VectorImpulse))
            return;

        if (!TryUseAbility(uid, component.VectorImpulseCost, component))
            return;

        var origin = _transform.GetMapCoordinates(uid);
        var forward = _transform.GetWorldRotation(uid).ToWorldVec().Normalized();
        var minDot = (float) Math.Cos(component.VectorImpulseArc.Theta / 2d);

        foreach (var target in _lookup.GetEntitiesInRange(origin, component.VectorImpulseRange, LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (target == uid || !TryComp<PhysicsComponent>(target, out _))
                continue;

            var offset = _transform.GetMapCoordinates(target).Position - origin.Position;
            if (offset.LengthSquared() <= float.Epsilon)
                continue;

            var direction = Vector2.Normalize(offset);
            if (Vector2.Dot(forward, direction) < minDot)
                continue;

            _throwing.TryThrow(target, direction, component.VectorImpulseStrength, uid, playSound: false, recoil: false);
        }

        _colorFlash.RaiseEffect(Color.Aqua, new List<EntityUid> { uid }, Filter.Pvs(uid, entityManager: EntityManager), 0.25f);
        _audio.PlayPvs(component.PsiBuzzSound, uid);

        args.Handled = true;
    }

    private void OnGravityWell(EntityUid uid, PsionicComponent component, GravityWellActionEvent args)
    {
        if (args.Handled)
            return;

        if (!component.LearnedAbilities.Contains(PsionicAbility.GravityWell))
            return;

        if (!TryUseAbility(uid, component.GravityWellCost, component))
            return;

        var origin = _transform.GetMapCoordinates(uid);
        foreach (var target in _lookup.GetEntitiesInRange(origin, component.GravityWellRange, LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (target == uid || !TryComp<PhysicsComponent>(target, out _))
                continue;

            var offset = origin.Position - _transform.GetMapCoordinates(target).Position;
            if (offset.LengthSquared() <= float.Epsilon)
                continue;

            _throwing.TryThrow(target, offset, component.GravityWellStrength, uid, playSound: false, recoil: false);
            if (offset.Length() <= component.GravityWellKnockdownRange)
                _stun.TryKnockdown(target, component.GravityWellKnockdownDuration, true);
        }

        _colorFlash.RaiseEffect(new Color(120, 180, 255), new List<EntityUid> { uid }, Filter.Pvs(uid, entityManager: EntityManager), 0.28f);
        _audio.PlayPvs(component.PsiBuzzSound, uid);
        args.Handled = true;
    }

    private void OnKineticDash(EntityUid uid, PsionicComponent component, KineticDashActionEvent args)
    {
        if (args.Handled)
            return;

        if (!component.LearnedAbilities.Contains(PsionicAbility.KineticDash))
            return;

        if (!TryUseAbility(uid, component.KineticDashCost, component))
            return;

        var xform = Transform(uid);
        var startPos = _transform.GetWorldPosition(uid);
        var forward = _transform.GetWorldRotation(uid).ToWorldVec().Normalized();
        var ray = new CollisionRay(startPos, forward, (int) (CollisionGroup.Impassable | CollisionGroup.InteractImpassable));
        var hits = _physics.IntersectRay(xform.MapID, ray, component.KineticDashDistance, uid, false).ToList();

        var targetPos = hits.Count > 0
            ? hits.MinBy(h => (h.HitPos - startPos).Length()).HitPos - forward
            : startPos + forward * component.KineticDashDistance;

        _transform.SetWorldPosition(uid, targetPos);
        _colorFlash.RaiseEffect(new Color(170, 220, 255), new List<EntityUid> { uid }, Filter.Pvs(uid, entityManager: EntityManager), 0.2f);
        _audio.PlayPvs(component.PsiBuzzSound, uid);
        args.Handled = true;
    }

    private void OnCrushingPulse(EntityUid uid, PsionicComponent component, CrushingPulseActionEvent args)
    {
        if (args.Handled)
            return;

        if (!component.LearnedAbilities.Contains(PsionicAbility.CrushingPulse))
            return;

        if (!TryUseAbility(uid, component.CrushingPulseCost, component))
            return;

        var origin = _transform.GetMapCoordinates(uid);
        var damageType = _prototype.Index(component.NeuroshockDamageType);
        var damage = new DamageSpecifier(damageType, component.CrushingPulseDamage);

        foreach (var target in _lookup.GetEntitiesInRange(origin, component.CrushingPulseRange, LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (target == uid)
                continue;

            _stun.TryKnockdown(target, component.CrushingPulseKnockdownDuration, true);
            _damageable.TryChangeDamage(target, damage, origin: uid);
        }

        _colorFlash.RaiseEffect(new Color(190, 190, 255), new List<EntityUid> { uid }, Filter.Pvs(uid, entityManager: EntityManager), 0.25f);
        _audio.PlayPvs(component.PsiBuzzSound, uid);
        args.Handled = true;
    }

    private void OnMolecularBlink(EntityUid uid, PsionicComponent component, MolecularBlinkActionEvent args)
    {
        if (args.Handled)
            return;

        if (!component.LearnedAbilities.Contains(PsionicAbility.MolecularBlink))
            return;

        if (!TryUseAbility(uid, component.MolecularBlinkCost, component))
            return;

        var xform = Transform(uid);
        var startPos = _transform.GetWorldPosition(uid);
        var startCoords = _transform.GetMapCoordinates(uid);
        var forward = _transform.GetWorldRotation(uid).ToWorldVec().Normalized();

        var ray = new CollisionRay(startPos, forward, (int) (CollisionGroup.Impassable | CollisionGroup.InteractImpassable));
        var hits = _physics.IntersectRay(xform.MapID, ray, component.MolecularBlinkDistance, uid, false).ToList();

        Vector2 targetPos;
        if (hits.Count > 0)
            targetPos = hits.MinBy(h => (h.HitPos - startPos).Length()).HitPos - forward;
        else
            targetPos = startPos + forward * component.MolecularBlinkDistance;

        _transform.SetWorldPosition(uid, targetPos);
        var endCoords = _transform.GetMapCoordinates(uid);
        RepulseAround(uid, endCoords, component.MolecularBlinkBurstRange, component.MolecularBlinkBurstStrength);

        _colorFlash.RaiseEffect(new Color(170, 255, 255), new List<EntityUid> { uid }, Filter.Pvs(uid, entityManager: EntityManager), 0.2f);
        var startEntityCoords = _transform.ToCoordinates((uid, xform), startCoords);
        var endEntityCoords = _transform.ToCoordinates((uid, Transform(uid)), endCoords);
        _audio.PlayPvs(component.PsiBuzzSound, startEntityCoords);
        _audio.PlayPvs(component.PsiBuzzSound, endEntityCoords);
        args.Handled = true;
    }

    private void OnBioSurge(EntityUid uid, PsionicComponent component, BioSurgeActionEvent args)
    {
        if (args.Handled)
            return;

        if (!component.LearnedAbilities.Contains(PsionicAbility.BioSurge))
            return;

        if (!TryUseAbility(uid, component.BioSurgeCost, component))
            return;

        var origin = _transform.GetMapCoordinates(uid);
        var damageType = _prototype.Index(component.BioSurgeDamageType);
        var drainDamageAmount = Math.Max(1, (int) MathF.Round(component.BioSurgeDrainDamage));
        var drainDamage = new DamageSpecifier(damageType, drainDamageAmount);

        var drainedTargets = 0;
        foreach (var target in _lookup.GetEntitiesInRange(origin, component.BioSurgeRange, LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (target == uid || !HasComp<DamageableComponent>(target))
                continue;

            var changed = _damageable.TryChangeDamage(target, drainDamage, origin: uid);
            if (changed == null)
                continue;

            drainedTargets++;
            _stun.TryUpdateParalyzeDuration(target, component.BioSurgeStunDuration);

            if (drainedTargets >= component.BioSurgeMaxTargets)
                break;
        }

        var totalHeal = component.BioSurgeBaseHeal + drainedTargets * component.BioSurgeHealPerTarget;
        if (totalHeal > 0f)
        {
            var healAmount = Math.Max(1, (int) MathF.Round(totalHeal));
            var bruteHeal = new DamageSpecifier(_prototype.Index<DamageGroupPrototype>("Brute"), -healAmount);
            var burnHeal = new DamageSpecifier(_prototype.Index<DamageGroupPrototype>("Burn"), -(int) MathF.Max(1f, healAmount * 0.75f));
            _damageable.TryChangeDamage(uid, bruteHeal, ignoreResistances: true);
            _damageable.TryChangeDamage(uid, burnHeal, ignoreResistances: true);
        }

        component.Heat = MathF.Max(0f, component.Heat - component.BioSurgeHeatRefund);
        Dirty(uid, component);
        UpdateHeatAlert(uid, component);

        _colorFlash.RaiseEffect(new Color(140, 255, 170), new List<EntityUid> { uid }, Filter.Pvs(uid, entityManager: EntityManager), 0.35f);
        _audio.PlayPvs(component.PsiBuzzSound, uid);
        args.Handled = true;
    }

    private void OnPhaseShift(EntityUid uid, PsionicComponent component, PhaseShiftActionEvent args)
    {
        if (args.Handled)
            return;

        if (!component.LearnedAbilities.Contains(PsionicAbility.PhaseShift))
            return;

        if (!TryUseAbility(uid, component.PhaseShiftCost, component))
            return;

        var xform = Transform(uid);
        var startPos = _transform.GetWorldPosition(uid);
        var startCoords = _transform.GetMapCoordinates(uid);
        var forward = _transform.GetWorldRotation(uid).ToWorldVec().Normalized();
        var ray = new CollisionRay(startPos, forward, (int) (CollisionGroup.Impassable | CollisionGroup.InteractImpassable));
        var hits = _physics.IntersectRay(xform.MapID, ray, component.PhaseShiftDistance, uid, false).ToList();

        var targetPos = hits.Count > 0
            ? hits.MinBy(h => (h.HitPos - startPos).Length()).HitPos - forward
            : startPos + forward * component.PhaseShiftDistance;

        _transform.SetWorldPosition(uid, targetPos);
        component.Heat = MathF.Max(0f, component.Heat - component.PhaseShiftHeatReduction);
        Dirty(uid, component);
        UpdateHeatAlert(uid, component);

        var startEntityCoords = _transform.ToCoordinates((uid, xform), startCoords);
        _audio.PlayPvs(component.PsiBuzzSound, startEntityCoords);
        _audio.PlayPvs(component.PsiBuzzSound, uid);
        args.Handled = true;
    }

    private void OnNeuroshockWave(EntityUid uid, PsionicComponent component, NeuroshockWaveActionEvent args)
    {
        if (args.Handled)
            return;

        if (!component.LearnedAbilities.Contains(PsionicAbility.NeuroshockWave))
            return;

        if (!TryUseAbility(uid, component.NeuroshockWaveCost, component))
            return;

        var origin = _transform.GetMapCoordinates(uid);
        var damageType = _prototype.Index(component.NeuroshockDamageType);
        var damage = new DamageSpecifier(damageType, component.NeuroshockWaveDamage);

        foreach (var target in _lookup.GetEntitiesInRange(origin, component.NeuroshockWaveRange, LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (target == uid)
                continue;

            _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(0.9f));
            _damageable.TryChangeDamage(target, damage, origin: uid);
        }

        _colorFlash.RaiseEffect(new Color(255, 200, 140), new List<EntityUid> { uid }, Filter.Pvs(uid, entityManager: EntityManager), 0.3f);
        _audio.PlayPvs(component.PsiBuzzSound, uid);
        args.Handled = true;
    }

    private void OnOpenStudyMenuAction(EntityUid uid, PsionicComponent component, OpenPsionicStudyMenuActionEvent args)
    {
        if (args.Handled)
            return;

        EnsurePsionicStore(uid, component);
        _store.ToggleUi(args.Performer, uid);
        args.Handled = true;
    }

    private void SyncAbilityActions(EntityUid uid, PsionicComponent component)
    {
        SyncAbilityAction(uid, component.LearnedAbilities.Contains(PsionicAbility.VectorImpulse), ref component.VectorImpulseAction, component.VectorImpulseActionId);
        SyncAbilityAction(uid, component.LearnedAbilities.Contains(PsionicAbility.KineticDash), ref component.KineticDashAction, component.KineticDashActionId);
        SyncAbilityAction(uid, component.LearnedAbilities.Contains(PsionicAbility.GravityWell), ref component.GravityWellAction, component.GravityWellActionId);
        SyncAbilityAction(uid, component.LearnedAbilities.Contains(PsionicAbility.CrushingPulse), ref component.CrushingPulseAction, component.CrushingPulseActionId);
        SyncAbilityAction(uid, component.LearnedAbilities.Contains(PsionicAbility.MolecularBlink), ref component.MolecularBlinkAction, component.MolecularBlinkActionId);
        SyncAbilityAction(uid, component.LearnedAbilities.Contains(PsionicAbility.PhaseShift), ref component.PhaseShiftAction, component.PhaseShiftActionId);
        SyncAbilityAction(uid, component.LearnedAbilities.Contains(PsionicAbility.BioSurge), ref component.BioSurgeAction, component.BioSurgeActionId);
        SyncAbilityAction(uid, component.LearnedAbilities.Contains(PsionicAbility.NeuroshockWave), ref component.NeuroshockWaveAction, component.NeuroshockWaveActionId);
    }

    private void SyncAbilityAction(EntityUid uid, bool shouldHaveAction, ref EntityUid? actionEntity, EntProtoId actionProto)
    {
        if (shouldHaveAction)
        {
            _actions.AddAction(uid, ref actionEntity, actionProto);
            return;
        }

        _actions.RemoveAction(uid, actionEntity);
        actionEntity = null;
    }

    public bool TryLearnAbility(EntityUid uid, PsionicAbility ability, PsionicComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        if (component.LearnedAbilities.Contains(ability))
            return false;

        component.LearnedAbilities.Add(ability);
        SyncAbilityActions(uid, component);
        Dirty(uid, component);
        return true;
    }

    private void RepulseAround(EntityUid uid, MapCoordinates center, float range, float strength)
    {
        foreach (var target in _lookup.GetEntitiesInRange(center, range, LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (target == uid || !TryComp<PhysicsComponent>(target, out _))
                continue;

            var offset = _transform.GetMapCoordinates(target).Position - center.Position;
            if (offset.LengthSquared() <= float.Epsilon)
                continue;

            _throwing.TryThrow(target, Vector2.Normalize(offset), strength, uid, playSound: false, recoil: false);
        }
    }

    private void PassiveRegen(float frameTime)
    {
        var query = EntityQueryEnumerator<PsionicComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Heat <= 0f)
                continue;

            var newHeat = MathF.Max(0f, component.Heat - component.PassiveRegenPerSecond * frameTime);
            if (MathF.Abs(newHeat - component.Heat) <= float.Epsilon)
                continue;

            component.Heat = newHeat;
            Dirty(uid, component);
            UpdateHeatAlert(uid, component);
        }
    }

    public bool TryUseAbility(EntityUid uid, float cost, PsionicComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        if (component.Heat + cost > component.MaxHeat)
        {
            TriggerNeuroshock(uid, component);
            return false;
        }

        component.Heat += cost;
        Dirty(uid, component);
        UpdateHeatAlert(uid, component);
        return true;
    }

    private void TriggerNeuroshock(EntityUid uid, PsionicComponent component)
    {
        _stun.TryUpdateParalyzeDuration(uid, component.NeuroshockStunDuration);

        var damageType = _prototype.Index(component.NeuroshockDamageType);
        var damage = new DamageSpecifier(damageType, component.NeuroshockDamage);
        _damageable.TryChangeDamage(uid, damage, ignoreResistances: true);
    }

    private void UpdateHeatAlert(EntityUid uid, PsionicComponent component)
    {
        if (component.MaxHeat <= 0f)
        {
            _alerts.ClearAlert(uid, component.HeatAlert);
            return;
        }

        var ratio = Math.Clamp(component.Heat / component.MaxHeat, 0f, 1f);
        var severity = (short) Math.Clamp((int) MathF.Round(ratio * 4f), 0, 4);
        _alerts.ShowAlert(uid, component.HeatAlert, severity);
    }
}
