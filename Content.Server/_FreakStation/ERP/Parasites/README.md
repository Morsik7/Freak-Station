# Husk Parasite System

Система паразитов в стиле Barotrauma Husk для FreakyStation.

## Компоненты

### ParasiteLarvaComponent
Моб-червь, медленный, может заражать через укусы.

### ParasiteInfectionComponent
Отслеживает прогресс заражения (0-100%):
- 0-25%: Ранняя стадия (лёгкий дискомфорт)
- 25-50%: Средняя стадия (боль, слабость)
- 50%: Отрыв конечностей хоста
- 60%: Рост паразитарных конечностей
- 85-95%: Окно для симбиоза (с антипаразитарным препаратом)
- 100%: Полный захват (передача майнда паразиту)

### ParasiteSymbioteComponent
Симбиоз с паразитом (если остановить на 85-95%):
- Урон сопротивление +20%
- Регенерация здоровья 0.5/сек
- Бонус к урону в ближнем бою +5

### ParasiticLimbComponent
Паразитарные конечности (сильнее обычных на 50%)

## Механики

### Заражение
- Личинка кусает цель (30% шанс заражения)
- При первом заражении человека паразит получает майнд
- Паразит может контролировать кормление (действие)

### Прогрессия
- Паразит растёт только когда активно питается
- Скорость роста: 0.5% в секунду
- Периодический урон зависит от стадии

### Лечение
- Антипаразитарный препарат (AntiParasitic) останавливает рост
- Если применить на 85-95% → симбиоз
- Если раньше → просто остановка

## Файлы

**Server:**
- `Content.Server/_FreakyStation/ERP/Parasites/ParasiteSystem.cs`
- `Content.Server/_FreakyStation/ERP/Parasites/ParasiteLarvaSystem.cs`
- `Content.Server/_FreakyStation/ERP/Parasites/ParasiteActionsSystem.cs`
- `Content.Server/_FreakyStation/ERP/Parasites/AntiParasiticEffect.cs`

**Shared:**
- `Content.Shared/_FreakyStation/ERP/Parasites/ParasiteInfectionComponent.cs`
- `Content.Shared/_FreakyStation/ERP/Parasites/ParasiteLarvaComponent.cs`
- `Content.Shared/_FreakyStation/ERP/Parasites/ParasiteSymbioteComponent.cs`
- `Content.Shared/_FreakyStation/ERP/Parasites/ParasiticLimbComponent.cs`

**Prototypes:**
- `Resources/Prototypes/_FreakyStation/Entities/Mobs/parasite_larva.yml`
- `Resources/Prototypes/_FreakyStation/Entities/Body/parasitic_limbs.yml`
- `Resources/Prototypes/_FreakyStation/Actions/parasite_actions.yml`
- `Resources/Prototypes/_FreakyStation/Alerts/parasite_alerts.yml`
- `Resources/Prototypes/_FreakyStation/Reagents/antiparasitic.yml`
