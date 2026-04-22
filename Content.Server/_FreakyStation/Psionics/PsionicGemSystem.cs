// SPDX-FileCopyrightText: 2026 Freaky Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Store.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Store.Components;
using Content.Shared._FreakyStation.Psionics;

namespace Content.Server._FreakyStation.Psionics;

public sealed class PsionicGemSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicGemComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, PsionicGemComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<PsionicComponent>(args.User, out var psionic))
            return;

        if (!TryComp<StoreComponent>(args.User, out var store))
            return;

        var currency = new Dictionary<string, FixedPoint2>
        {
            { psionic.PsionicPointCurrency.ToString(), component.PointValue }
        };

        if (!_store.TryAddCurrency(currency, args.User, store))
            return;

        args.Handled = true;
        QueueDel(uid);
    }
}
