// SPDX-FileCopyrightText: 2026 Freaky Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Store;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;

namespace Content.Shared._FreakyStation.Psionics;

[Serializable, NetSerializable]
public enum PsionicDiscipline : byte
{
    Vector,
    Bio,
    Molecular,
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PsionicComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Heat;

    [DataField, AutoNetworkedField]
    public float MaxHeat = 100f;

    [DataField, AutoNetworkedField]
    public PsionicDiscipline Discipline = PsionicDiscipline.Vector;

    [DataField]
    public float PassiveRegenPerSecond = 2f;

    [DataField]
    public EntProtoId VectorImpulseActionId = "ActionPsionicVectorImpulse";

    [DataField]
    public EntityUid? VectorImpulseAction;

    [DataField]
    public float VectorImpulseCost = 15f;

    [DataField]
    public float VectorImpulseRange = 3f;

    [DataField]
    public Angle VectorImpulseArc = Angle.FromDegrees(45f);

    [DataField]
    public float VectorImpulseStrength = 10f;

    [DataField]
    public EntProtoId KineticDashActionId = "ActionPsionicKineticDash";

    [DataField]
    public EntityUid? KineticDashAction;

    [DataField]
    public float KineticDashCost = 18f;

    [DataField]
    public float KineticDashDistance = 4f;

    [DataField]
    public EntProtoId GravityWellActionId = "ActionPsionicGravityWell";

    [DataField]
    public EntityUid? GravityWellAction;

    [DataField]
    public float GravityWellCost = 28f;

    [DataField]
    public float GravityWellRange = 5f;

    [DataField]
    public float GravityWellStrength = 12f;

    [DataField]
    public float GravityWellKnockdownRange = 1.6f;

    [DataField]
    public TimeSpan GravityWellKnockdownDuration = TimeSpan.FromSeconds(2f);

    [DataField]
    public EntProtoId CrushingPulseActionId = "ActionPsionicCrushingPulse";

    [DataField]
    public EntityUid? CrushingPulseAction;

    [DataField]
    public float CrushingPulseCost = 26f;

    [DataField]
    public float CrushingPulseRange = 2.3f;

    [DataField]
    public TimeSpan CrushingPulseKnockdownDuration = TimeSpan.FromSeconds(1.2f);

    [DataField]
    public float CrushingPulseDamage = 10f;

    [DataField]
    public EntProtoId MolecularBlinkActionId = "ActionPsionicMolecularBlink";

    [DataField]
    public EntityUid? MolecularBlinkAction;

    [DataField]
    public float MolecularBlinkCost = 24f;

    [DataField]
    public float MolecularBlinkDistance = 7f;

    [DataField]
    public float MolecularBlinkBurstRange = 2.4f;

    [DataField]
    public float MolecularBlinkBurstStrength = 9f;

    [DataField]
    public EntProtoId PhaseShiftActionId = "ActionPsionicPhaseShift";

    [DataField]
    public EntityUid? PhaseShiftAction;

    [DataField]
    public float PhaseShiftCost = 20f;

    [DataField]
    public float PhaseShiftDistance = 5f;

    [DataField]
    public float PhaseShiftHeatReduction = 12f;

    [DataField]
    public EntProtoId BioSurgeActionId = "ActionPsionicBioSurge";

    [DataField]
    public EntityUid? BioSurgeAction;

    [DataField]
    public float BioSurgeCost = 34f;

    [DataField]
    public float BioSurgeRange = 4f;

    [DataField]
    public int BioSurgeMaxTargets = 5;

    [DataField]
    public float BioSurgeDrainDamage = 6f;

    [DataField]
    public TimeSpan BioSurgeStunDuration = TimeSpan.FromSeconds(0.8f);

    [DataField]
    public float BioSurgeBaseHeal = 5f;

    [DataField]
    public float BioSurgeHealPerTarget = 2.5f;

    [DataField]
    public float BioSurgeHeatRefund = 8f;

    [DataField]
    public ProtoId<DamageTypePrototype> BioSurgeDamageType = "Cellular";

    [DataField]
    public EntProtoId NeuroshockWaveActionId = "ActionPsionicNeuroshockWave";

    [DataField]
    public EntityUid? NeuroshockWaveAction;

    [DataField]
    public float NeuroshockWaveCost = 30f;

    [DataField]
    public float NeuroshockWaveRange = 3.2f;

    [DataField]
    public float NeuroshockWaveDamage = 8f;

    [DataField]
    public EntProtoId StudyMenuActionId = "ActionPsionicStudyMenu";

    [DataField]
    public EntityUid? StudyMenuAction;

    [DataField]
    public ProtoId<CurrencyPrototype> PsionicPointCurrency = "PsionicPoint";

    [DataField]
    public int StartingPsionicPoints = 1;

    [DataField]
    public List<ProtoId<StoreCategoryPrototype>> StoreCategories = new()
    {
        "PsionicBranchVector",
        "PsionicBranchGravity",
        "PsionicBranchMolecular",
        "PsionicBranchBio",
    };

    [DataField]
    public List<ProtoId<ListingPrototype>> StoreListings = new()
    {
        "PsionicLearnVectorImpulse",
        "PsionicLearnKineticDash",
        "PsionicLearnGravityWell",
        "PsionicLearnCrushingPulse",
        "PsionicLearnMolecularBlink",
        "PsionicLearnPhaseShift",
        "PsionicLearnBioSurge",
        "PsionicLearnNeuroshockWave",
    };

    [DataField]
    public HashSet<PsionicAbility> LearnedAbilities = new();

    [DataField]
    public TimeSpan NeuroshockStunDuration = TimeSpan.FromSeconds(5);

    // Fallback type for "brain damage" until dedicated type is added.
    [DataField]
    public ProtoId<DamageTypePrototype> NeuroshockDamageType = "Cellular";

    [DataField]
    public float NeuroshockDamage = 10f;

    [DataField]
    public SoundSpecifier PsiBuzzSound = new SoundPathSpecifier("/Audio/Machines/scanbuzz.ogg");

    [DataField]
    public ProtoId<AlertPrototype> HeatAlert = "PsionicHeat";
}
