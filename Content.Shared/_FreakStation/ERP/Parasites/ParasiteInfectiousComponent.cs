// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._FreakStation.ERP.Parasites;

/// <summary>
/// Marks an entity as infectious - eating it will infect the consumer
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ParasiteInfectiousComponent : Component
{
    /// <summary>
    /// Chance to infect when eaten (0.0 to 1.0)
    /// </summary>
    [DataField]
    public float InfectionChance = 1.0f;

    /// <summary>
    /// The parasite entity that will infect the consumer
    /// </summary>
    [DataField]
    public EntityUid? ParasiteEntity;
}
