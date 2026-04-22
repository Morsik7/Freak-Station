// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared._FreakStation.ERP.Parasites;

/// <summary>
/// Tracks parasite infection progression in a host.
/// 0% = just infected, 100% = full takeover (mind transfer)
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ParasiteInfectionComponent : Component
{
    /// <summary>
    /// Current infection percentage (0-100)
    /// </summary>
    [DataField]
    public float InfectionPercent;

    /// <summary>
    /// Rate of infection growth per second when parasite is feeding
    /// Default: 0.1% per second = ~16.6 minutes to reach 100%
    /// </summary>
    [DataField]
    public float InfectionGrowthRate = 0.1f;

    /// <summary>
    /// The parasite entity controlling this infection
    /// </summary>
    [DataField]
    public EntityUid? ParasiteEntity;

    /// <summary>
    /// The parasite's mind ID (for observer mode in humans)
    /// </summary>
    [DataField]
    public EntityUid? ParasiteMindId;

    /// <summary>
    /// Whether the parasite is actively feeding (growing infection)
    /// </summary>
    [DataField]
    public bool IsFeeding;

    /// <summary>
    /// Time when infection started
    /// </summary>
    [DataField]
    public TimeSpan InfectionStartTime;

    /// <summary>
    /// Whether limbs have been severed (happens at 50%+)
    /// </summary>
    [DataField]
    public bool LimbsSevered;

    /// <summary>
    /// Whether parasitic limbs have grown (after severing)
    /// </summary>
    [DataField]
    public bool ParasiticLimbsGrown;

    /// <summary>
    /// Whether infection was stopped by anti-parasite drug
    /// </summary>
    [DataField]
    public bool InfectionStopped;

    /// <summary>
    /// Next time to apply infection effects
    /// </summary>
    [DataField]
    public TimeSpan NextEffectTime;

    /// <summary>
    /// Interval between infection effects
    /// </summary>
    [DataField]
    public TimeSpan EffectInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Whether parasite can communicate with host (unlocked at 20%)
    /// </summary>
    [DataField]
    public bool CanCommunicate;

    /// <summary>
    /// Whether parasite can temporarily take control (unlocked at 60%)
    /// </summary>
    [DataField]
    public bool CanTakeControl;

    /// <summary>
    /// Whether parasite is currently controlling the host body
    /// </summary>
    [DataField]
    public bool IsControlling;

    /// <summary>
    /// When current control session ends
    /// </summary>
    [DataField]
    public TimeSpan ControlEndTime;

    /// <summary>
    /// Original host mind ID (stored during control)
    /// </summary>
    [DataField]
    public EntityUid? OriginalHostMindId;

    /// <summary>
    /// Whether infection can be cured (false after 90%)
    /// </summary>
    [DataField]
    public bool CanBeCured = true;

    // Action entities for infected hosts
    [DataField]
    public EntityUid? FeedingToggleActionEntity;

    [DataField]
    public EntityUid? LeaveHostActionEntity;

    [DataField]
    public EntityUid? TentaclesActionEntity;

    [DataField]
    public EntityUid? DashActionEntity;

    [DataField]
    public EntityUid? LayEggsActionEntity;

    [DataField]
    public EntityUid? HealActionEntity;

    [DataField]
    public EntityUid? ChimeraDashActionEntity;

    [DataField]
    public EntityUid? TakeControlActionEntity;

    // Door hack removed - will be passive component instead
}
