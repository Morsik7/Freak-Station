// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared._FreakStation.ERP.Parasites;

public sealed partial class ParasitePossessActionEvent : EntityTargetActionEvent
{
}

public sealed partial class ParasiteLeaveHostActionEvent : InstantActionEvent
{
}

public sealed partial class ToggleParasiteFeedingActionEvent : InstantActionEvent
{
}

public sealed partial class ParasiteTentacleActionEvent : InstantActionEvent
{
}

public sealed partial class ParasiteSummonTentacleAction : WorldTargetActionEvent
{
}

public sealed partial class ParasiteDashActionEvent : InstantActionEvent
{
}

public sealed partial class ParasiteLayEggsActionEvent : InstantActionEvent
{
}

public sealed partial class ParasiteHealActionEvent : InstantActionEvent
{
}

public sealed partial class ChimeraDashActionEvent : InstantActionEvent
{
}

public sealed partial class ParasiteTakeControlActionEvent : InstantActionEvent
{
}
