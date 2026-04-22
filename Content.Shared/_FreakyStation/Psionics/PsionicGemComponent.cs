// SPDX-FileCopyrightText: 2026 Freaky Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._FreakyStation.Psionics;

[RegisterComponent, NetworkedComponent]
public sealed partial class PsionicGemComponent : Component
{
    [DataField]
    public int PointValue = 1;
}
