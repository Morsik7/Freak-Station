// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Mind;
using Content.Shared._FreakStation.ERP.Parasites;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._FreakStation.ERP.Parasites;

/// <summary>
/// Handles parasite possession of hosts
/// </summary>
public sealed class ParasitePossessionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ParasiteSystem _parasite = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParasiteLarvaComponent, ComponentStartup>(OnLarvaStartup);
        SubscribeLocalEvent<ParasiteLarvaComponent, ParasitePossessActionEvent>(OnPossessAction);
        SubscribeLocalEvent<ParasiteInfectionComponent, ParasiteLeaveHostActionEvent>(OnLeaveHostAction);
    }

    private void OnLarvaStartup(EntityUid uid, ParasiteLarvaComponent component, ComponentStartup args)
    {
        // Grant possession ability to larva
        _actions.AddAction(uid, ref component.PossessActionEntity, component.PossessAction);
    }

    private void OnPossessAction(EntityUid uid, ParasiteLarvaComponent component, ParasitePossessActionEvent args)
    {
        if (args.Handled || args.Target == null)
            return;

        var target = args.Target;

        // Check if target is valid
        if (!HasComp<BodyComponent>(target))
        {
            _popup.PopupEntity("Это не подходящая цель для вселения.", uid, uid);
            return;
        }

        // Check if target is dead
        if (_mobState.IsDead(target))
        {
            _popup.PopupEntity("Цель мертва.", uid, uid);
            return;
        }

        // Check if target is already infected
        if (HasComp<ParasiteInfectionComponent>(target))
        {
            _popup.PopupEntity("Эта цель уже заражена!", uid, uid);
            return;
        }

        // Check stage requirements
        if (component.Stage == ParasiteStage.Larva)
        {
            // Larva can only possess mice and monkeys
            if (!HasComp<BodyComponent>(target))
            {
                _popup.PopupEntity("Вы слишком слабы чтобы заразить эту цель. Найдите мышь или обезьяну.", uid, uid);
                return;
            }
        }

        // Perform possession
        if (_parasite.TryInfect(uid, target, component))
        {
            _popup.PopupEntity("Вы проникаете в тело цели!", uid, uid, PopupType.Large);
            _popup.PopupEntity("Что-то проникло в ваше тело!", target, target, PopupType.LargeCaution);

            // Update larva stage based on host
            var infection = Comp<ParasiteInfectionComponent>(target);
            infection.ParasiteEntity = uid;

            // Check if target is human - if so, DON'T transfer mind yet (parasite becomes observer)
            var isHuman = MetaData(target).EntityPrototype?.ID?.Contains("Human") == true ||
                          MetaData(target).EntityPrototype?.ID?.Contains("Humanoid") == true;

            if (!isHuman)
            {
                // For mouse/monkey - transfer mind immediately (full control)
                if (_mind.TryGetMind(uid, out var parasiteMindId, out var parasiteMind))
                {
                    _mind.TransferTo(parasiteMindId, target, mind: parasiteMind);
                }

                // DON'T delete larva - just make it invisible and store it
                // We need it for leaving the host later
                var xform = Transform(uid);
                xform.Coordinates = Transform(target).Coordinates;

                // Make invisible and non-collidable
                if (TryComp<AppearanceComponent>(uid, out var appearance))
                {
                    // Hide visually
                }
            }
            else
            {
                // For human - parasite becomes observer, store mind reference for later
                if (_mind.TryGetMind(uid, out var parasiteMindId, out var parasiteMind))
                {
                    infection.ParasiteMindId = parasiteMindId;
                    _popup.PopupEntity("Вы проникли в человека. Вы пока не можете контролировать тело, только наблюдать.", uid, uid);
                }

                // Delete larva entity - it's now inside the host
                QueueDel(uid);
            }

            args.Handled = true;
        }
    }

    private void OnLeaveHostAction(EntityUid uid, ParasiteInfectionComponent component, ParasiteLeaveHostActionEvent args)
    {
        if (args.Handled)
            return;

        // Check if parasite entity exists
        if (component.ParasiteEntity == null || !Exists(component.ParasiteEntity.Value))
        {
            _popup.PopupEntity("Паразит не найден!", uid, uid);
            return;
        }

        var parasite = component.ParasiteEntity.Value;

        // Check if this is a human (cannot leave humans)
        if (TryComp<ParasiteLarvaComponent>(parasite, out var larvaComp) && larvaComp.HasInfectedHuman)
        {
            _popup.PopupEntity("Вы не можете покинуть человеческого хоста!", uid, uid);
            return;
        }

        // Spawn larva at host location
        var newLarva = Spawn("ParasiteLarva", Transform(uid).Coordinates);

        // Transfer mind back to larva
        if (_mind.TryGetMind(uid, out var hostMindId, out var hostMind))
        {
            _mind.TransferTo(hostMindId, newLarva, mind: hostMind);
        }

        // Copy larva component data
        if (TryComp<ParasiteLarvaComponent>(newLarva, out var newLarvaComp) && larvaComp != null)
        {
            newLarvaComp.Stage = ParasiteStage.Larva;
            newLarvaComp.HasInfectedHuman = larvaComp.HasInfectedHuman;
        }

        // Remove infection from host
        RemComp<ParasiteInfectionComponent>(uid);

        _popup.PopupEntity("Вы покидаете тело хоста!", newLarva, newLarva, PopupType.Large);
        _popup.PopupEntity("Паразит покинул ваше тело!", uid, uid, PopupType.Medium);

        // Delete old parasite entity
        QueueDel(parasite);

        args.Handled = true;
    }
}
