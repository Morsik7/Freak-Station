// SPDX-FileCopyrightText: 2026 Freaky Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;

namespace Content.Shared._FreakyStation.Psionics;

[Serializable, NetSerializable]
public enum PsionicStudyUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum PsionicAbility : byte
{
    VectorImpulse,
    KineticDash,
    GravityWell,
    CrushingPulse,
    MolecularBlink,
    PhaseShift,
    BioSurge,
    NeuroshockWave,
}

[Serializable, NetSerializable]
public sealed class PsionicAbilityEntry(
    PsionicAbility ability,
    string name,
    string description,
    bool learned)
{
    public PsionicAbility Ability { get; } = ability;
    public string Name { get; } = name;
    public string Description { get; } = description;
    public bool Learned { get; } = learned;
}

[Serializable, NetSerializable]
public sealed class PsionicStudyBoundUserInterfaceState(
    List<PsionicAbilityEntry> abilities,
    int learnedCount,
    int maxLearned)
    : BoundUserInterfaceState
{
    public List<PsionicAbilityEntry> Abilities { get; } = abilities;
    public int LearnedCount { get; } = learnedCount;
    public int MaxLearned { get; } = maxLearned;
}

[Serializable, NetSerializable]
public sealed class PsionicStudyToggleAbilityMessage(PsionicAbility ability) : BoundUserInterfaceMessage
{
    public PsionicAbility Ability { get; } = ability;
}
