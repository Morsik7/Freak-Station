
using Content.Server._CorvaxGoob.Skills;
using Content.Shared.Humanoid;
using Robust.Shared.Player;

namespace Content.Server._Freakystation;

public sealed class AllHaveSkillsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, PlayerAttachedEvent>(OnAttached);
    }

    private void OnAttached(EntityUid uid, HumanoidAppearanceComponent component, PlayerAttachedEvent args)
    {
        if (!HasComp<IgnoreSkillsComponent>(uid))
            AddComp<IgnoreSkillsComponent>(uid);
    }
}
