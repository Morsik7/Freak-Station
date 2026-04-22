// SPDX-FileCopyrightText: 2022 Jesse Rougeau <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.GameTicking.Prototypes;
using System.Linq;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared._OS;

namespace Content.Server.GameTicking;

// Goobstation - this file is heavily modified to add credits for lobby backgrounds
public sealed partial class GameTicker
{
    [ViewVariables]
    public ProtoId<LobbyBackgroundPrototype>? LobbyBackground { get; private set; }

    [ViewVariables]
    private List<string>? _lobbyBackgrounds; // OpenSpace edit

    private void InitializeLobbyBackground()
    {
        var animatedLobbyBackgrounds = _prototypeManager
            .EnumeratePrototypes<AnimatedLobbyScreenPrototype>()
            .Select(x => x.Path);

        var staticLobbyBackgrounds = _prototypeManager
            .EnumeratePrototypes<LobbyBackgroundPrototype>()
            .Where(x => x.Background.ToString().EndsWith(".rsi", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.ID);

        _lobbyBackgrounds = animatedLobbyBackgrounds
            .Concat(staticLobbyBackgrounds)
            .Distinct()
            .ToList();

        RandomizeLobbyBackground();
    }

    private void RandomizeLobbyBackground()
    {
        LobbyBackground = _lobbyBackgrounds!.Any() ? _robustRandom.Pick(_lobbyBackgrounds!) : null; // OpenSpace edit
    }
}
