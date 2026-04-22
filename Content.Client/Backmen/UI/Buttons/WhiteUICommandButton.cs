using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client.Backmen.UI.Buttons;

public sealed class WhiteUICommandButton : WhiteCommandButton
{
    [DataField]
    public Type? WindowType { get; set; }

    private DefaultWindow? _window;

    protected override void Execute(BaseButton.ButtonEventArgs args)
    {
        if (WindowType == null)
            return;

        var windowInstance = IoCManager.Resolve<IDynamicTypeFactory>().CreateInstance(WindowType);
        if (windowInstance is not DefaultWindow window)
            return;

        _window = window;
        _window.OpenCentered();
    }
}
