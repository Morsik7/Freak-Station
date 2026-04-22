// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared._FreakStation.ERP.Parasites;

/// <summary>
/// Marks an egg that will hatch into a larva after a timer
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ParasiteEggComponent : Component
{
    /// <summary>
    /// Time when the egg will hatch
    /// </summary>
    [DataField]
    public TimeSpan HatchTime;

    /// <summary>
    /// How long until hatching (in seconds)
    /// </summary>
    [DataField]
    public float HatchDelay = 300f; // 5 minutes

    /// <summary>
    /// The mind to transfer to the hatched larva (for fertilized eggs)
    /// </summary>
    [DataField]
    public EntityUid? MindToTransfer;
}
