// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._FreakStation.ERP.Parasites;

/// <summary>
/// Marks an organ as parasitic
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ParasiticOrganComponent : Component
{
    [DataField]
    public string OrganType = string.Empty;
}
