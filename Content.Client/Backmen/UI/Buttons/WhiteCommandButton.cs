using Robust.Client.Console;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Backmen.UI.Buttons;

[Virtual]
public class WhiteCommandButton : WhiteLobbyTextButton
{
    [DataField]
    public string? Command { get; set; }

    public WhiteCommandButton()
    {
        OnPressed += Execute;
    }

    protected override void EnteredTree()
    {
        base.EnteredTree();

        if (!CanPress())
            Visible = false;
    }

    private bool CanPress()
    {
        return string.IsNullOrEmpty(Command) ||
               IoCManager.Resolve<IClientConGroupController>().CanCommand(Command.Split(' ')[0]);
    }

    protected virtual void Execute(BaseButton.ButtonEventArgs args)
    {
        if (!string.IsNullOrEmpty(Command))
            IoCManager.Resolve<IClientConsoleHost>().ExecuteCommand(Command);
    }
}
