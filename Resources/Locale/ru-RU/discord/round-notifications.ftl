discord-round-notifications-new =
    >>> <@&{ $roleId }>
    🆕 **Новый раунд начнётся через 3 минуты!**
    `{ $playerCount }` игроков сейчас играет
discord-round-notifications-started =
    >>> Раунд #{ $id } начался!
    Карта: { $map }
    Режим: { $gamemode }
    Игроков `{ $playerCount }`
discord-round-notifications-end =
    >>> Раунд #{ $id } завершён
    Длительность: { $hours }ч { $minutes }м { $seconds }с
    Игроков `{ $playerCount }`
    Режим: { $gamemode }
    ```
    { $manifest }
    ```
discord-round-notifications-end-no-manifest =
    >>> Раунд #{ $id } завершён
    Длительность: { $hours }ч { $minutes }м { $seconds }с
    Игроков `{ $playerCount }`
    Режим: { $gamemode }
discord-round-notifications-end-ping =
    >>> **Раунд перезапускается!**
    `{ $playerCount }` игроков сейчас играет
    Новый раунд начнётся через 3 минуты!
discord-round-notifications-unknown-map = *Неизвестная карта*
