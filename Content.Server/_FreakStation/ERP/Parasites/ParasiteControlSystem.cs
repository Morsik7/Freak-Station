// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Mind;
using Content.Shared._FreakStation.ERP.Parasites;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._FreakStation.ERP.Parasites;

/// <summary>
/// Handles temporary parasite control of human hosts
/// </summary>
public sealed class ParasiteControlSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const float ControlDuration = 30f; // 30 seconds of control

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParasiteInfectionComponent, ParasiteTakeControlActionEvent>(OnTakeControl);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;

        // Check for expired control sessions
        var query = EntityQueryEnumerator<ParasiteInfectionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsControlling)
                continue;

            if (currentTime >= comp.ControlEndTime)
            {
                // Return control to host
                ReturnControlToHost(uid, comp);
            }
        }
    }

    private void OnTakeControl(EntityUid uid, ParasiteInfectionComponent component, ParasiteTakeControlActionEvent args)
    {
        if (args.Handled)
            return;

        // Check if can take control
        if (!component.CanTakeControl)
        {
            _popup.PopupEntity("Паразит ещё недостаточно силён для контроля.", uid, uid);
            return;
        }

        // Check if already controlling
        if (component.IsControlling)
        {
            _popup.PopupEntity("Вы уже контролируете тело!", uid, uid);
            return;
        }

        // Check if parasite mind exists
        if (component.ParasiteMindId == null || !Exists(component.ParasiteMindId.Value))
        {
            _popup.PopupEntity("Разум паразита не найден!", uid, uid);
            return;
        }

        // Store original host mind
        if (_mind.TryGetMind(uid, out var hostMindId, out var hostMind))
        {
            component.OriginalHostMindId = hostMindId;
        }

        // Transfer parasite mind to host body
        if (_mind.TryGetMind(component.ParasiteMindId.Value, out var parasiteMindId, out var parasiteMind))
        {
            _mind.TransferTo(parasiteMindId, uid, mind: parasiteMind);
            component.IsControlling = true;
            component.ControlEndTime = _timing.CurTime + TimeSpan.FromSeconds(ControlDuration);

            _popup.PopupEntity($"Вы захватываете контроль над телом на {ControlDuration} секунд!", uid, uid, PopupType.Large);
        }

        args.Handled = true;
    }

    private void ReturnControlToHost(EntityUid uid, ParasiteInfectionComponent component)
    {
        if (!component.IsControlling)
            return;

        // Get current mind (parasite)
        if (_mind.TryGetMind(uid, out var currentMindId, out var currentMind))
        {
            // Store parasite mind back
            component.ParasiteMindId = currentMindId;
        }

        // Return host mind to body
        if (component.OriginalHostMindId != null && Exists(component.OriginalHostMindId.Value))
        {
            if (_mind.TryGetMind(component.OriginalHostMindId.Value, out var hostMindId, out var hostMind))
            {
                _mind.TransferTo(hostMindId, uid, mind: hostMind);
            }
        }

        component.IsControlling = false;
        _popup.PopupEntity("Контроль паразита ослабевает. Вы снова контролируете своё тело.", uid, uid, PopupType.Medium);
    }
}
