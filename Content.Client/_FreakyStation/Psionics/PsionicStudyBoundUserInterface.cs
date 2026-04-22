// SPDX-FileCopyrightText: 2026 Freaky Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._FreakyStation.Psionics;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using System;

namespace Content.Client._FreakyStation.Psionics;

[UsedImplicitly]
public sealed class PsionicStudyBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private PsionicStudyWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PsionicStudyWindow>();
        _window.OnToggleAbility += ability => SendMessage(new PsionicStudyToggleAbilityMessage(ability));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (_window == null || state is not PsionicStudyBoundUserInterfaceState cast)
            return;

        _window.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Close();
    }
}
