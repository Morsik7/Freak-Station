using Robust.Client.UserInterface;

namespace Content.Client._Donate.UI;

public sealed class DonateShopSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _interfaceManager = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
}
