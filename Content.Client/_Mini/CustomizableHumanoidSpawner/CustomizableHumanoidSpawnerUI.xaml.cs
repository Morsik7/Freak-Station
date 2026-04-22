using Content.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._Mini.CustomizableHumanoidSpawner;

public sealed partial class CustomizableHumanoidSpawnerUI : FancyWindow
{
    public CustomizableHumanoidSpawnerUI()
    {
        RobustXamlLoader.Load(this);
    }
}
