using Content.Shared.Chat;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;

namespace Content.Shared._Nuclear.Chat;

public sealed class NuclearChatSendAttemptEvent : EntityEventArgs
{
    public ICommonSession Player { get; }
    public ChatChannel Channel { get; }
    public bool Cancelled { get; private set; }

    public NuclearChatSendAttemptEvent(ICommonSession player, ChatChannel channel)
    {
        Player = player;
        Channel = channel;
    }

    public void Cancel()
    {
        Cancelled = true;
    }
}
