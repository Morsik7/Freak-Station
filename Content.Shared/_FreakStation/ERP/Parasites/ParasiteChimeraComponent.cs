// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._FreakStation.ERP.Parasites;

/// <summary>
/// Marks a fully infected human (100% infection) as a chimera with special abilities
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ParasiteChimeraComponent : Component
{
    /// <summary>
    /// Whether the chimera has laid eggs yet
    /// </summary>
    [DataField]
    public bool HasLaidEggs;

    /// <summary>
    /// The original host's mind (transferred to eggs on 100% infection)
    /// </summary>
    [DataField]
    public EntityUid? OriginalHostMind;
}
