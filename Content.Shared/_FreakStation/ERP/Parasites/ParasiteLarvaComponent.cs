// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._FreakStation.ERP.Parasites;

/// <summary>
/// Marks an entity as a parasite larva that can infect hosts.
/// Slow-moving worm that needs to infect a mouse first or constantly bite targets.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ParasiteLarvaComponent : Component
{
    /// <summary>
    /// Whether this larva has infected its first human host (gets mind transfer)
    /// </summary>
    [DataField]
    public bool HasInfectedHuman;

    /// <summary>
    /// Current host entity (if inside a mouse or human)
    /// </summary>
    [DataField]
    public EntityUid? CurrentHost;

    /// <summary>
    /// Damage dealt per bite
    /// </summary>
    [DataField]
    public int BiteDamage = 5;

    /// <summary>
    /// Chance to infect on bite (0-1)
    /// </summary>
    [DataField]
    public float InfectionChanceOnBite = 0.05f;

    /// <summary>
    /// Current evolution stage
    /// </summary>
    [DataField]
    public ParasiteStage Stage = ParasiteStage.Larva;

    /// <summary>
    /// Possess action entity
    /// </summary>
    [DataField]
    public EntityUid? PossessActionEntity;

    /// <summary>
    /// Possess action prototype ID
    /// </summary>
    [DataField]
    public string PossessAction = "ActionParasitePossess";
}
