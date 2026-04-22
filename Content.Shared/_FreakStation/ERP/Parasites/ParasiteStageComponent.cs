// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._FreakStation.ERP.Parasites;

/// <summary>
/// Marks the current stage of parasite evolution
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ParasiteStageComponent : Component
{
    [DataField]
    public ParasiteStage Stage = ParasiteStage.Larva;
}

public enum ParasiteStage : byte
{
    /// <summary>
    /// Initial larva form - can infect by being eaten or can possess a mouse
    /// </summary>
    Larva = 0,

    /// <summary>
    /// Possessed mouse - has tentacles and dash attack, can infect monkey
    /// </summary>
    Mouse = 1,

    /// <summary>
    /// Possessed monkey - has tentacles, egg laying, and healing
    /// </summary>
    Monkey = 2,

    /// <summary>
    /// Final form - human host at 100% infection with full abilities
    /// </summary>
    Chimera = 3
}
