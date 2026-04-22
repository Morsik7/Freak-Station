cmd-chatmute-desc = Mute or unmute say/whisper/me/LOOC/OOC/dead chat channels for a specific player. Does not apply to active admins.
cmd-chatmute-help = Usage: chatmute <username/userId> <say|whisper|me|looc|ooc|dead|all> <on|off>
cmd-chatmute_status-desc = Show say/whisper/me/LOOC/OOC/dead chat mute status for a specific player.
cmd-chatmute_status-help = Usage: chatmute_status <username/userId>

nchatmute-error-player-not-found = Player '{ $player }' is not online.
nchatmute-error-target-admin = Player '{ $player }' is an active admin. chatmute does not apply to active admins.
nchatmute-error-channel = Unknown channel '{ $channel }'. Use: say, whisper, me, looc, ooc, dead, all.
nchatmute-error-state = Unknown state '{ $state }'. Use: on or off.
nchatmute-set = Updated chat mute for { $player }: { $target } -> { $state }.
nchatmute-status = Chat mute status for { $player }: SAY={ $say }, WHISPER={ $whisper }, ME={ $emote }, LOOC={ $looc }, OOC={ $ooc }, DEAD={ $dead }.
nchatmute-flag-on = ON
nchatmute-flag-off = OFF
nchatmute-feedback-send-blocked = You cannot send messages to { $channel }.

nchatmute-channel-say = local chat
nchatmute-channel-whisper = whispers
nchatmute-channel-emote = emotes
nchatmute-channel-looc = LOOC
nchatmute-channel-ooc = OOC
nchatmute-channel-dead = dead chat
