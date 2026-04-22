using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._Mini.CharacterBlock;

public sealed partial class CharacterBlockedGui : DefaultWindow
{
    public CharacterBlockedGui()
    {
        RobustXamlLoader.Load(this);
    }
}
