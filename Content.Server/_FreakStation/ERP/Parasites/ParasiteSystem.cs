// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Server.Mind;
using Content.Shared._FreakStation.ERP.Parasites;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Cloning.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server._FreakStation.ERP.Parasites;

public sealed class ParasiteSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private static readonly ProtoId<DamageTypePrototype> SlashDamage = "Slash";

    // Infection thresholds
    private const float CommunicationThreshold = 20f;
    private const float LimbSeverThreshold = 50f;
    private const float LimbGrowthThreshold = 60f;
    private const float ControlThreshold = 60f;
    private const float SymbiosisMinThreshold = 85f;
    private const float SymbiosisMaxThreshold = 95f;
    private const float NoCureThreshold = 90f;
    private const float FullTakeoverThreshold = 100f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParasiteInfectionComponent, ComponentStartup>(OnInfectionStartup);
        SubscribeLocalEvent<ParasiteInfectiousComponent, IngestedEvent>(OnInfectiousIngested);
        SubscribeLocalEvent<ParasiteInfectionComponent, CloningAttemptEvent>(OnCloningAttempt);
        SubscribeLocalEvent<ParasiteInfectionComponent, CloningEvent>(OnCloned);
        SubscribeLocalEvent<ParasiteStageComponent, ComponentStartup>(OnStageStartup);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;

        // Process active infections
        var query = EntityQueryEnumerator<ParasiteInfectionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.InfectionStopped)
                continue;

            if (currentTime < comp.NextEffectTime)
                continue;

            // Grow infection if parasite is feeding
            if (comp.IsFeeding && comp.InfectionPercent < FullTakeoverThreshold)
            {
                comp.InfectionPercent += comp.InfectionGrowthRate * (float)comp.EffectInterval.TotalSeconds;
                comp.InfectionPercent = Math.Min(comp.InfectionPercent, FullTakeoverThreshold);
            }

            // Check infection stage and apply effects
            ProcessInfectionStage(uid, comp, currentTime);

            comp.NextEffectTime = currentTime + comp.EffectInterval;
        }
    }

    private void ProcessInfectionStage(EntityUid uid, ParasiteInfectionComponent comp, TimeSpan currentTime)
    {
        // Stage 1: Enable communication at 20%
        if (comp.InfectionPercent >= CommunicationThreshold && !comp.CanCommunicate)
        {
            comp.CanCommunicate = true;
            _popup.PopupEntity("Вы слышите чужой голос в своей голове...", uid, uid, PopupType.Medium);
            // TODO: Grant communication action to parasite
        }

        // Stage 2: Sever limbs at 50%
        if (comp.InfectionPercent >= LimbSeverThreshold && !comp.LimbsSevered)
        {
            SeverHostLimbs(uid, comp);
            comp.LimbsSevered = true;
            _popup.PopupEntity("Вы чувствуете как что-то разрывает ваши конечности изнутри!", uid, uid, PopupType.LargeCaution);
        }

        // Stage 3: Grow parasitic limbs at 60%
        if (comp.InfectionPercent >= LimbGrowthThreshold && comp.LimbsSevered && !comp.ParasiticLimbsGrown)
        {
            GrowParasiticLimbs(uid, comp);
            comp.ParasiticLimbsGrown = true;
            _popup.PopupEntity("Из ваших ран начинают расти странные органические конечности...", uid, uid, PopupType.Large);

            // Grant chimera abilities when limbs grow
            _actions.AddAction(uid, ref comp.TentaclesActionEntity, "ActionParasiteTentacles");
            _actions.AddAction(uid, ref comp.ChimeraDashActionEntity, "ActionChimeraDash");

            // Add passive door pry component (like xenomorphs have)
            EnsureComp<Content.Shared.Prying.Components.PryingComponent>(uid);

            _popup.PopupEntity("Вы получили способности химеры!", uid, uid);
        }

        // Stage 4: Enable temporary control at 60%
        if (comp.InfectionPercent >= ControlThreshold && !comp.CanTakeControl)
        {
            comp.CanTakeControl = true;

            // Grant temporary control action to parasite (if parasite mind exists)
            if (comp.ParasiteMindId != null && Exists(comp.ParasiteMindId.Value))
            {
                _actions.AddAction(comp.ParasiteMindId.Value, ref comp.TakeControlActionEntity, "ActionParasiteTakeControl");
            }

            _popup.PopupEntity("Паразит достаточно силён чтобы временно захватывать контроль над вашим телом!", uid, uid, PopupType.LargeCaution);
        }

        // Stage 5: Infection becomes incurable at 90%
        if (comp.InfectionPercent >= NoCureThreshold && comp.CanBeCured)
        {
            comp.CanBeCured = false;
            _popup.PopupEntity("Паразит достиг спинного мозга. Лечение больше невозможно.", uid, uid, PopupType.LargeCaution);
        }

        // Stage 6: Full takeover at 100%
        if (comp.InfectionPercent >= FullTakeoverThreshold)
        {
            PerformFullTakeover(uid, comp);
        }

        // Apply periodic damage based on infection level (reduced damage)
        if (comp.InfectionPercent > 0 && !comp.InfectionStopped)
        {
            var damageAmount = (int)(comp.InfectionPercent / 50f); // Very low damage
            var damage = new DamageSpecifier();
            damage.DamageDict.Add(SlashDamage, damageAmount);
            _damageable.TryChangeDamage(uid, damage);
        }
    }

    private void SeverHostLimbs(EntityUid uid, ParasiteInfectionComponent comp)
    {
        if (!TryComp<BodyComponent>(uid, out var body))
            return;

        // Remove all arms and legs by deleting them
        var limbsToRemove = new List<EntityUid>();

        foreach (var (partId, part) in _body.GetBodyChildren(uid, body))
        {
            if (part.PartType == BodyPartType.Arm || part.PartType == BodyPartType.Leg ||
                part.PartType == BodyPartType.Hand || part.PartType == BodyPartType.Foot)
            {
                limbsToRemove.Add(partId);
            }
        }

        foreach (var partId in limbsToRemove)
        {
            QueueDel(partId);
        }
    }

    private void GrowParasiticLimbs(EntityUid uid, ParasiteInfectionComponent comp)
    {
        if (!TryComp<BodyComponent>(uid, out var body))
            return;

        // Drop ALL equipment except headset, hat, and backpack
        var slotsToRemove = new[] { "gloves", "shoes", "outerClothing", "innerClothing", "jumpsuit", "belt", "id", "pocket1", "pocket2", "suitstorage" };
        foreach (var slot in slotsToRemove)
        {
            if (_inventory.TryGetSlotEntity(uid, slot, out _))
                _inventory.TryUnequip(uid, slot);
        }

        // Spawn parasitic limbs: whole arms (with claws), legs as one piece
        var leftArm = Spawn("ParasiticLeftArm", Transform(uid).Coordinates);
        var rightArm = Spawn("ParasiticRightArm", Transform(uid).Coordinates);
        var legs = Spawn("ParasiticLegs", Transform(uid).Coordinates);

        // Get root body part (torso/chest)
        if (!_body.TryGetRootPart(uid, out var rootPart, body))
        {
            _popup.PopupEntity("Не удалось найти торс для прикрепления конечностей!", uid, uid);
            return;
        }

        // Attach parasitic limbs to torso using proper API
        if (TryComp<BodyPartComponent>(leftArm, out var leftArmPart))
        {
            var slotId = _body.GetSlotFromBodyPart(leftArmPart);
            _body.TryCreatePartSlotAndAttach(rootPart.Value, slotId, leftArm, leftArmPart.PartType, leftArmPart.Symmetry, rootPart, leftArmPart);
        }

        if (TryComp<BodyPartComponent>(rightArm, out var rightArmPart))
        {
            var slotId = _body.GetSlotFromBodyPart(rightArmPart);
            _body.TryCreatePartSlotAndAttach(rootPart.Value, slotId, rightArm, rightArmPart.PartType, rightArmPart.Symmetry, rootPart, rightArmPart);
        }

        if (TryComp<BodyPartComponent>(legs, out var legsPart))
        {
            var slotId = _body.GetSlotFromBodyPart(legsPart);
            _body.TryCreatePartSlotAndAttach(rootPart.Value, slotId, legs, legsPart.PartType, legsPart.Symmetry, rootPart, legsPart);
        }

        _popup.PopupEntity("Паразитические конечности выросли из ваших ран и прикрепились к телу!", uid, uid, PopupType.Large);
    }

    private void PerformFullTakeover(EntityUid host, ParasiteInfectionComponent comp)
    {
        if (comp.ParasiteEntity == null || !Exists(comp.ParasiteEntity.Value))
            return;

        var parasite = comp.ParasiteEntity.Value;

        // Check for mind shield implant
        var hasMindShield = HasComp<MindShieldComponent>(host);

        // Get host's original mind before transfer
        EntityUid? hostMindId = null;
        if (_mind.TryGetMind(host, out var originalHostMindId, out _))
            hostMindId = originalHostMindId;

        // Transfer parasite mind to host
        if (_mind.TryGetMind(parasite, out var parasiteMindId, out var parasiteMind))
        {
            _mind.TransferTo(parasiteMindId, host, mind: parasiteMind);

            if (hasMindShield)
            {
                _popup.PopupEntity("Паразит захватил контроль над вашим телом, но имплант защиты разума сохраняет вашу личность!", host, host, PopupType.LargeCaution);
                // Host can still speak but not control body
            }
            else
            {
                _popup.PopupEntity("Паразит полностью поглотил ваш разум!", host, host, PopupType.LargeCaution);
            }
        }

        // Mark as chimera
        var chimera = EnsureComp<ParasiteChimeraComponent>(host);
        chimera.OriginalHostMind = hostMindId;

        // Spawn fertilized eggs with host's mind transferred to them
        if (!hasMindShield && hostMindId != null)
        {
            var eggPos = Transform(host).Coordinates;
            var egg = Spawn("ParasiteFertilizedEgg", eggPos);

            // Store mind reference in egg component for later transfer on hatch
            if (TryComp<ParasiteEggComponent>(egg, out var eggComp))
            {
                eggComp.MindToTransfer = hostMindId;
            }

            _popup.PopupEntity("Химера откладывает оплодотворённое яйцо!", host, PopupType.Large);
        }

        // Delete the original parasite entity
        QueueDel(parasite);
        comp.ParasiteEntity = null;
        comp.InfectionStopped = true; // Stop infection progression
    }

    /// <summary>
    /// Attempts to infect a target with a parasite larva
    /// </summary>
    public bool TryInfect(EntityUid parasite, EntityUid target, ParasiteLarvaComponent? larvaComp = null)
    {
        if (!Resolve(parasite, ref larvaComp))
            return false;

        // Check if target is already infected
        if (HasComp<ParasiteInfectionComponent>(target))
            return false;

        // Check if target is alive
        if (_mobState.IsDead(target))
            return false;

        // Create infection component
        var infection = EnsureComp<ParasiteInfectionComponent>(target);
        infection.ParasiteEntity = parasite;
        infection.InfectionStartTime = _timing.CurTime;
        infection.InfectionPercent = 0f;
        infection.IsFeeding = true; // Start feeding immediately

        // Grant feeding control action to PARASITE (not host!)
        if (Exists(parasite))
        {
            _actions.AddAction(parasite, ref infection.FeedingToggleActionEntity, "ActionToggleParasiteFeeding");
        }

        // Grant abilities based on host type
        // Check if it's a mouse (small animal)
        if (MetaData(target).EntityPrototype?.ID?.Contains("Mouse") == true)
        {
            larvaComp.Stage = ParasiteStage.Mouse;
            _actions.AddAction(target, ref infection.TentaclesActionEntity, "ActionParasiteTentacles");
            _actions.AddAction(target, ref infection.DashActionEntity, "ActionParasiteDash");
            _actions.AddAction(target, ref infection.LeaveHostActionEntity, "ActionParasiteLeaveHost");
            _popup.PopupEntity("Вы получили способности заражённой мыши!", target, target);
        }
        // Check if it's a monkey
        else if (MetaData(target).EntityPrototype?.ID?.Contains("Monkey") == true)
        {
            larvaComp.Stage = ParasiteStage.Monkey;
            _actions.AddAction(target, ref infection.TentaclesActionEntity, "ActionParasiteTentacles");
            _actions.AddAction(target, ref infection.LayEggsActionEntity, "ActionParasiteLayEggs");
            _actions.AddAction(target, ref infection.HealActionEntity, "ActionParasiteHeal");
            _actions.AddAction(target, ref infection.LeaveHostActionEntity, "ActionParasiteLeaveHost");
            _popup.PopupEntity("Вы получили способности заражённой обезьяны!", target, target);
        }
        // Human or other humanoid
        else if (HasComp<BodyComponent>(target))
        {
            larvaComp.HasInfectedHuman = true;
            larvaComp.CurrentHost = target;
            // Abilities will be granted at 60% when limbs grow
            // Cannot leave human host
        }

        _popup.PopupEntity("Вы чувствуете как что-то проникает под вашу кожу!", target, target, PopupType.LargeCaution);

        return true;
    }

    /// <summary>
    /// Stops infection progression and converts to symbiosis if in valid range
    /// </summary>
    public bool TryStopInfection(EntityUid uid, ParasiteInfectionComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        if (comp.InfectionStopped)
            return false;

        comp.InfectionStopped = true;
        comp.IsFeeding = false;

        // Check if infection level allows symbiosis
        if (comp.InfectionPercent >= SymbiosisMinThreshold && comp.InfectionPercent <= SymbiosisMaxThreshold)
        {
            var symbiote = EnsureComp<ParasiteSymbioteComponent>(uid);
            _popup.PopupEntity("Паразит перестал расти. Вы чувствуете странную связь с ним...", uid, uid, PopupType.Large);
            return true;
        }

        _popup.PopupEntity("Рост паразита остановлен.", uid, uid, PopupType.Medium);
        return true;
    }

    /// <summary>
    /// Toggles parasite feeding state
    /// </summary>
    public void ToggleFeeding(EntityUid uid, ParasiteInfectionComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (comp.InfectionStopped)
            return;

        comp.IsFeeding = !comp.IsFeeding;
    }

    private void OnInfectionStartup(EntityUid uid, ParasiteInfectionComponent component, ComponentStartup args)
    {
        component.NextEffectTime = _timing.CurTime + component.EffectInterval;
        component.InfectionStartTime = _timing.CurTime;
    }

    private void OnInfectiousIngested(EntityUid uid, ParasiteInfectiousComponent component, ref IngestedEvent args)
    {
        // Check infection chance
        if (_random.Prob(component.InfectionChance))
        {
            // Try to infect the target (the one who ate the food)
            if (component.ParasiteEntity != null && Exists(component.ParasiteEntity.Value))
            {
                TryInfect(component.ParasiteEntity.Value, args.Target);
            }
            else
            {
                // If no specific parasite entity, spawn a new larva
                var larva = Spawn("ParasiteLarva", Transform(args.Target).Coordinates);
                TryInfect(larva, args.Target);
            }
        }
    }

    private void OnCloningAttempt(EntityUid uid, ParasiteInfectionComponent component, ref CloningAttemptEvent args)
    {
        // Block cloning if infection is too advanced (60%+)
        if (component.InfectionPercent >= LimbGrowthThreshold)
        {
            args.Cancelled = true;
            _popup.PopupEntity("Паразит слишком глубоко интегрирован в тело для клонирования!", uid, uid, PopupType.LargeCaution);
        }
    }

    private void OnCloned(EntityUid uid, ParasiteInfectionComponent component, ref CloningEvent args)
    {
        // Transfer infection to clone
        var cloneInfection = EnsureComp<ParasiteInfectionComponent>(args.CloneUid);
        cloneInfection.InfectionPercent = component.InfectionPercent;
        cloneInfection.InfectionGrowthRate = component.InfectionGrowthRate;
        cloneInfection.IsFeeding = component.IsFeeding;
        cloneInfection.InfectionStopped = component.InfectionStopped;
        cloneInfection.LimbsSevered = component.LimbsSevered;
        cloneInfection.ParasiticLimbsGrown = false; // Limbs need to regrow
        cloneInfection.CanCommunicate = component.CanCommunicate;
        cloneInfection.CanTakeControl = component.CanTakeControl;
        cloneInfection.CanBeCured = component.CanBeCured;

        // Transfer parasite entity reference
        if (component.ParasiteEntity != null && Exists(component.ParasiteEntity.Value))
        {
            cloneInfection.ParasiteEntity = component.ParasiteEntity.Value;
        }

        _popup.PopupEntity("Паразит перенёсся в клонированное тело!", args.CloneUid, args.CloneUid, PopupType.LargeCaution);
    }

    private void OnStageStartup(EntityUid uid, ParasiteStageComponent component, ComponentStartup args)
    {
        // Grant abilities based on stage
        switch (component.Stage)
        {
            case ParasiteStage.Mouse:
                _actions.AddAction(uid, "ActionParasiteTentacles");
                _actions.AddAction(uid, "ActionParasiteDash");
                break;

            case ParasiteStage.Monkey:
                _actions.AddAction(uid, "ActionParasiteTentacles");
                _actions.AddAction(uid, "ActionParasiteLayEggs");
                _actions.AddAction(uid, "ActionParasiteHeal");
                break;

            case ParasiteStage.Chimera:
                _actions.AddAction(uid, "ActionParasiteTentacles");
                _actions.AddAction(uid, "ActionChimeraDash");
                _actions.AddAction(uid, "ActionChimeraDoorHack");
                break;
        }
    }
}
