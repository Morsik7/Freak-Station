cmd-chatmute-desc = Включить или выключить блокировку каналов say/whisper/me/LOOC/OOC/dead chat для конкретного игрока. На активных администраторах не работает.
cmd-chatmute-help = Использование: chatmute <username/userId> <say|whisper|me|looc|ooc|dead|all> <on|off>
cmd-chatmute_status-desc = Показать статус блокировки каналов say/whisper/me/LOOC/OOC/dead chat у конкретного игрока.
cmd-chatmute_status-help = Использование: chatmute_status <username/userId>

nchatmute-error-player-not-found = Игрок '{ $player }' не найден онлайн.
nchatmute-error-target-admin = Игрок '{ $player }' является активным администратором. chatmute не применяется к активным администраторам.
nchatmute-error-channel = Неизвестный канал '{ $channel }'. Используйте: say, whisper, me, looc, ooc, dead, all.
nchatmute-error-state = Неизвестное состояние '{ $state }'. Используйте: on или off.
nchatmute-set = Обновлена блокировка чата для { $player }: { $target } -> { $state }.
nchatmute-status = Статус блокировки чата для { $player }: SAY={ $say }, WHISPER={ $whisper }, ME={ $emote }, LOOC={ $looc }, OOC={ $ooc }, DEAD={ $dead }.
nchatmute-flag-on = ВКЛ
nchatmute-flag-off = ВЫКЛ
nchatmute-feedback-send-blocked = Вам запрещено отправлять сообщения в канал { $channel }.

nchatmute-channel-say = обычный чат
nchatmute-channel-whisper = шепот
nchatmute-channel-emote = эмоуты
nchatmute-channel-looc = LOOC
nchatmute-channel-ooc = OOC
nchatmute-channel-dead = dead chat
