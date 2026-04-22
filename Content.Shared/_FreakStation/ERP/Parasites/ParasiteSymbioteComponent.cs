// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._FreakStation.ERP.Parasites;

/// <summary>
/// Marks a host that achieved symbiosis with parasite (stopped at ~90% infection).
/// Grants enhanced parasitic limbs and bonuses.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ParasiteSymbioteComponent : Component
{
    /// <summary>
    /// Damage resistance bonus from symbiosis
    /// </summary>
    [DataField]
    public float DamageResistance = 0.2f;

    /// <summary>
    /// Health regeneration per second
    /// </summary>
    [DataField]
    public float HealthRegeneration = 0.5f;

    /// <summary>
    /// Melee damage bonus from parasitic limbs
    /// </summary>
    [DataField]
    public float MeleeDamageBonus = 5f;
}
