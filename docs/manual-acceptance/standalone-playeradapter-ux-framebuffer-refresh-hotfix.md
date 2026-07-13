# Standalone PlayerAdapter UX framebuffer refresh hotfix

Status: accepted by human before Goal153; accepted by Codex remains false.

> Я принимаю Goals152/152A/152C: standalone показал зелёную автопроверку, интерфейс читаемый, кнопки Далее/Назад/В конец/Сбросить работают, текст обновляется без наложения; host cache переиспользован без запуска Unity Editor.

Accepted commit: `ac97859c8de861641e07f886250d053b5330fbe9`

1. Launch standalone.
2. Confirm the large green “Автопроверка пройдена” banner.
3. Click Далее, Назад, В конец and Сбросить.
4. Confirm readable controls and no ghosting or overlapping text.
5. Close standalone.
