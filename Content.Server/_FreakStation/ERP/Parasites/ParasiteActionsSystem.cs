// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._FreakStation.ERP.Parasites;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Server._FreakStation.ERP.Parasites;

/// <summary>
/// Handles parasite-specific actions (toggle feeding, etc)
/// </summary>
public sealed class ParasiteActionsSystem : EntitySystem
{
    [Dependency] private readonly ParasiteSystem _parasite = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParasiteInfectionComponent, ToggleParasiteFeedingActionEvent>(OnToggleFeeding);
    }

    private void OnToggleFeeding(EntityUid uid, ParasiteInfectionComponent component, ToggleParasiteFeedingActionEvent args)
    {
        if (args.Handled)
            return;

        _parasite.ToggleFeeding(uid, component);

        var message = component.IsFeeding
            ? "Паразит начинает активно питаться..."
            : "Паразит прекращает питаться.";

        _popup.PopupEntity(message, uid, uid);

        args.Handled = true;
    }
}
