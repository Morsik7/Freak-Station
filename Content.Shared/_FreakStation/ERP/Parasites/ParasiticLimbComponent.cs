// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._FreakStation.ERP.Parasites;

/// <summary>
/// Marks a body part as parasitic
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ParasiticLimbComponent : Component
{
    [DataField]
    public string LimbType = string.Empty;

    [DataField]
    public float DamageMultiplier = 1.0f;
}
