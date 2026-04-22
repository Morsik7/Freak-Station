// SPDX-FileCopyrightText: 2026 Casha (FreakStation)
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Mind;
using Content.Shared._FreakStation.ERP.Parasites;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._FreakStation.ERP.Parasites;

public sealed class ParasiteEggSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParasiteEggComponent, ComponentStartup>(OnEggStartup);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ParasiteEggComponent>();

        while (query.MoveNext(out var uid, out var egg))
        {
            if (currentTime < egg.HatchTime)
                continue;

            HatchEgg(uid, egg);
        }
    }

    private void OnEggStartup(EntityUid uid, ParasiteEggComponent component, ComponentStartup args)
    {
        component.HatchTime = _timing.CurTime + TimeSpan.FromSeconds(component.HatchDelay);
    }

    private void HatchEgg(EntityUid eggUid, ParasiteEggComponent egg)
    {
        var coordinates = Transform(eggUid).Coordinates;

        // Spawn larva
        var larva = Spawn("ParasiteLarva", coordinates);

        // Transfer mind if this is a fertilized egg
        if (egg.MindToTransfer != null && Exists(egg.MindToTransfer.Value))
        {
            if (_mind.TryGetMind(egg.MindToTransfer.Value, out var mindId, out var mind))
            {
                _mind.TransferTo(mindId, larva, mind: mind);
                _popup.PopupEntity("Вы вылупляетесь из яйца!", larva, larva);
            }
        }

        // Delete the egg
        QueueDel(eggUid);
    }
}
