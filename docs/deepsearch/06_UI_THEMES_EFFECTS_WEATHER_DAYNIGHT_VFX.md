# UI theme, weather, day-night и VFX для LLMGameCreator

## Контекст проекта и главный вывод

Текущая архитектурная линия LLMGameCreator уже задаёт правильное направление для темы исследования: `GamePackage` и связанные manifests/catalog/recipes должны оставаться источником истины, а runtime/player не должен вызывать LLM, RAG, генераторы медиа или сетевые провайдеры. Визуальный слой в проекте уже мыслится как цепочка `Semantic features → VisualRuleStack → VisualRecipe → Procedural composition → Surface/facade/billboard/atlas outputs → Unity/player binding`, причём отсутствующий ассет не должен ломать runtime. Отдельно в дорожной карте visual pipeline закреплено, что сначала нужны metadata/validators/fixtures и deterministic evidence, а не «широкий UI» или прямые Unity-мутации. Это делает детерминированную систему UI themes / weather / day-night / VFX не побочной идеей, а прямым продолжением уже принятой стратегии проекта. citeturn2view0turn6view0turn8view6turn9view0

Практический вывод такой: для задачи 06 не нужен «AI рисует интерфейс». Нужен **процедурный theme/effect compiler**, который берёт компактные theme recipes, part packs, palette slots, masks, sockets, states, status modifiers и seed, а на выходе даёт стабильно воспроизводимые UI skins, icons, overlays, weather/day-night modifiers, status FX и fallback metadata. Это очень хорошо совпадает с уже описанной в репозитории идеей `VisualRuleStack`, где домен — не «готовый скин», а поле влияния, смешиваемое с биомом, фракцией, состоянием, историей и seed. Более того, part-pack стратегия в репозитории прямо перечисляет среди целевых результатов не только поверхности и фасады, но и `UI frames` и `VFX masks`, а среди задач Codex — palette system, layer composition, atlas writer, metadata writer и golden/snapshot tests. citeturn9view1turn9view2turn7view3

Главная рекомендация: делать не один «ThemeManager», а **четырёхслойную систему**. Слой первый — семантические профили и rule stack. Слой второй — вычисление theme tokens и icon/effect recipes. Слой третий — deterministic compiler, который собирает SVG/растр/atlas/metadata. Слой четвёртый — тонкие runtime adapters для WinForms preview и Unity consumption. В core при этом не должно быть внешней тяжёлой графической зависимости без adapter boundary; core должен знать только контракты, версии генератора, seed policy, fallback policy и validators. Это соответствует и вашим ограничениям, и собственным документам проекта, где recipes пока должны оставаться editor-owned/sidecar-owned до тех пор, пока не доказан потребитель. citeturn9view0turn2view0

## Рекомендуемая архитектура UI theme stack

Для LLMGameCreator тема интерфейса должна вычисляться тем же способом, что и визуал мира: не как «готовый скин фракции», а как **результат rule stack**. В вашем репозитории `VisualRuleStack` уже составляется из мирового стиля, доменного влияния, биома, политического контроля, культуры, религии, роли объекта, богатства, состояния, history layer, gameplay constraints и seed. Для UI это можно перенести почти напрямую: `WorldStyle + DomainInfluence + FactionInfluence + BiomeMood + WeatherState + DayNightState + UIContextRole + InteractionState + StatusEffects + AccessibilityPolicy + Seed`. Тогда тема панели замка, кнопка каравана, рамка инвентаря и рамка экрана гарнизона будут происходить из одних и тех же правил, но с разными `contextRole` и `presentationTarget`. citeturn8view4turn9view1

На уровне данных я бы рекомендовал разделить три типа сущностей. Во-первых, **ThemeProfile**: семантический профиль домена/фракции/биома/стиля. Во-вторых, **ThemeTokens**: уже вычисленные конкретные цвета, толщины рамок, inset/outset, shadow-depth, emissive-strength, shape motifs, icon stroke width, corner-family и slice metrics. В-третьих, **SkinRecipe**: конкретная композиция панели, кнопки, таба, рамки и иконки из part packs, palette slots, masks и slice rules. Это позволит повторно использовать одну и ту же тему как в 2D UI, так и в pseudo-3D HUD/overlay без дублирования логики. Такой подход естественно продолжает вашу модель `DomainProfile` и `VisualRecipe`, где семантика сначала превращается в доктрину и веса, а уже потом в композицию. citeturn9view1turn8view5

Ниже — практический вариант канонического контракта:

```json
{
  "uiThemeProfileId": "ui_theme/necropolis_swamp_command",
  "appliesTo": {
    "worldStyle": "world/dark_fantasy",
    "domain": "domain/necropolis",
    "faction": "faction/black_reed_garrison",
    "biome": "biome/swamp",
    "uiContextRole": "screen/city_management"
  },
  "tokenSlots": {
    "panel_bg": "palette/wet_dark_green_900",
    "panel_edge": "palette/old_bone_500",
    "accent_primary": "palette/sickly_green_400",
    "accent_danger": "palette/cold_red_500",
    "text_primary": "palette/bone_text_050",
    "text_muted": "palette/fog_gray_300",
    "shadow_soft": "palette/black_700_a45"
  },
  "frameRecipeId": "frame/crypt_bone_rivets_v2",
  "buttonRecipeIds": {
    "primary": "button/crypt_green_gem_v1",
    "secondary": "button/dark_iron_flat_v1"
  },
  "iconStylePolicy": {
    "silhouetteFamily": "icon_family/engraved_heraldic",
    "strokeWeight": 2,
    "badgePolicy": "badge/top_right_compact",
    "fallbackIconId": "icon/generic/default_action"
  },
  "modifierOrder": [
    "weather",
    "dayNight",
    "status",
    "accessibility"
  ],
  "seedMode": "stable_per_context"
}
```

Ключевой момент здесь — **не смешивать стили мира и UI без ограничений**. Мир может позволить себе туман, пепел, темноту и низкую насыщенность, а UI — нет. W3C требует минимум 4.5:1 контраста для обычного текста и минимум 3:1 для визуальной идентификации элементов интерфейса и их состояний; отдельно W3C прямо предупреждает, что слишком тонкие линии и элементы, проходящие формально по цвету, могут быть плохо различимы на практике из-за anti-aliasing. Из этого следует, что `WeatherState` и `DayNightState` не должны напрямую перекрашивать весь UI. Они должны влиять только на ограниченный набор mood-токенов — например, `accent_cool_shift`, `shadow_density`, `specular_noise`, `ambient_vignette` — при обязательной валидации contrast budget. citeturn27view0turn27view1turn27view2

Для Unity-адаптера эта схема хорошо ложится на rule-based styling. UI Toolkit поддерживает USS как rule-based формат, TSS как отдельный тип theme asset и переключение разных TSS во время runtime; кроме того, темы обычно собираются через `@import`, что удобно для layered composition. То есть ваш compiler может не хранить в source-of-truth готовые USS/TSS, а генерировать их из canonical `ThemeTokens` как артефакты адаптера — оставаясь верным принципу, что исходным состоянием являются recipes и metadata, а не движковая разметка. citeturn26view1turn26view2

## Контракты иконок, статус-эффектов и VFX

Для иконок я рекомендую сделать **recipe-based icon compiler**, а не хранить бесконечные папки PNG. В canonical contract иконка должна состоять из нескольких независимых уровней: `silhouette`, `interior glyph`, `motif overlays`, `border`, `badge sockets`, `status overlays`, `palette bindings`, `hover/pressed/disabled variants`, `seeded micro-variation`. В вашем part-pack подходе одна и та же визуальная деталь может быть перекрашена, повернута, масштабирована и переиспользована в разных системах; это напрямую применимо к иконкам. У вас же в part packs уже заявлены `UI frames` и `VFX masks`, а сами recipes уже предполагают слои, palette system и deterministic renderer. citeturn9view2turn8view1turn7view3

Практически выгоднее всего держать канонический формат иконки не как один SVG-файл, а как **собственный JSON recipe**, который можно компилировать в два целевых представления: консервативный SVG и растровый atlas. Это важно, потому что даже при хорошем SVG-пути у Unity UI Toolkit есть ограничения: current UI Toolkit vector pipeline поддерживает только подмножество SVG 1.1 и не поддерживает text elements, per-pixel masking, filter effects, interactivity и animations. Значит, если в каноне иконки вы заложите «свечение через SVG filter», «drop-shadow через filter» или анимированный `stroke-dashoffset`, это будет ненадёжно. Гораздо безопаснее описывать такие эффекты отдельными слоями рецепта и при компиляции превращать их либо в дополнительную геометрию/shape-pass, либо в растрированный слой. citeturn26view3

Ниже — минимальный контракт, который имеет смысл:

```json
{
  "iconRecipeId": "icon/spell/poison_cloud_v1",
  "presentationRole": "ui/icon/action",
  "sizeClass": "square",
  "layers": [
    { "kind": "silhouette", "partId": "icon_part/cloud_lobed_03", "paletteSlot": "accent_primary" },
    { "kind": "interior", "partId": "icon_part/skull_small_engraved_01", "paletteSlot": "text_primary" },
    { "kind": "overlay", "partId": "icon_part/droplets_02", "paletteSlot": "accent_danger" }
  ],
  "sockets": {
    "badge_top_right": true,
    "stack_counter_bottom": true
  },
  "stateVariants": {
    "normal": {},
    "hover": { "brightnessBias": 0.08, "outlineBoost": 0.12 },
    "pressed": { "yOffset": 1, "shadowBias": -0.15 },
    "disabled": { "desaturation": 0.65, "alpha": 0.55 }
  },
  "fallbackIconId": "icon/generic/status_effect"
}
```

Для **status effects** и **simple VFX metadata** не нужен полноценный shader/VFX authoring pipeline в core. Достаточно сделать общий `EffectRecipe`, где задаются класс эффекта, целевой слой, тайминг, seed, blend mode, palette slots, target anchors и fallback policy. Я бы советовал ограничить первую версию шестью типами: `icon_pulse`, `screen_overlay`, `particle_emitter`, `billboard_strip`, `decal_stamp`, `light_flicker`. Этого уже достаточно для погоды, проклятий, баффов, горения, тумана, ауры, попаданий и экранных переходов — без тяжёлого графического стека. Built-in Particle System у Unity scriptable, CPU-based и официально поддерживается на всех платформах, поддерживаемых Unity, поэтому он хорошо подходит как **Unity adapter** именно для simple VFX из metadata. citeturn28view2

Для визуальных состояний панели и кнопки разумно ввести отдельные **state delta contracts**, а не отдельные полные скины на каждое состояние. UI Toolkit умеет runtime transitions между стилями, включая property, duration, easing и delay. Это подтверждает, что для Unity-адаптера выгодно хранить не «готовую анимацию», а компактную запись вида `hover => outline +4%, ambient glow +8%, shadow shift -2 px, duration 120 ms`. В WinForms preview или headless compiler вы можете просто материализовать это как два снапшота и validated interpolation metadata. citeturn26view5

Пример эффекта:

```json
{
  "effectRecipeId": "effect/status/poisoned_ui",
  "effectClass": "icon_pulse",
  "target": "ui/icon/status_badge",
  "timing": {
    "durationMs": 850,
    "loop": "ping_pong",
    "easing": "ease_in_out_sine"
  },
  "visuals": {
    "maskId": "mask/radial_soft_01",
    "paletteSlot": "accent_danger",
    "opacityMin": 0.22,
    "opacityMax": 0.52,
    "scaleMin": 0.95,
    "scaleMax": 1.08
  },
  "seedMode": "stable_per_subject",
  "fallbackPolicy": "drop_effect_keep_badge"
}
```

## Погода, day-night и слои атмосферы

Погоду и смену времени суток лучше проектировать не как «один глобальный постэффект», а как **три независимых канала**: world modulation, overlay composition и local emitters. Первый канал меняет палитру и освещение сцены. Второй даёт экранные или плоскостные overlays: дождь, снег, пыль, пепел, болотный пар, ночную виньетку, солнечную дымку. Третий включает локальные простые эффекты: брызги у воды, искры факелов, летающую пыль, ауру статуса, свечения рун. Такой разрез одинаково применим и к top-down 2D, и к pseudo-3D, потому что это не конкретный renderer, а presentation contract. Он также хорошо ложится на ваш текущий принцип `2D/visual recipe outputs → pseudo-3D presentation package`, где одни и те же данные должны уметь быть развёрнуты в разные способы показа. citeturn8view2turn8view6

На уровне данных я бы ввёл отдельный `AtmosphereStack`, который применяется **после** базового world/theme resolution, но **до** финального adapter binding. Там должны жить `weatherPreset`, `dayNightPreset`, `seasonPreset`, `localHazardPreset`, `statusAuraPreset`. Каждый preset задаёт только разрешённые модификаторы: `paletteLutId`, `globalLightBias`, `overlayLayers`, `particleEmitters`, `soundHintId`, `uiMoodShift`, `iconVisibilityPolicy`. Это согласуется и с вашим `SpecialInfluence / MetaModule`, где уже предусмотрены `addSurfaceDecals` и `addAtmosphere`. По сути, weather/day-night — это частный случай того же механизма модификаторов. citeturn9view1

Для Unity-пути practical baseline такой. Если у вас проект на URP 2D Renderer, то day-night можно адаптировать через `Global Light 2D` и другие типы Light 2D, поскольку URP 2D lighting официально предоставляет Freeform/Sprite/Parametric/Point/Global lights именно для освещения спрайтов. Для более грубой тональной коррекции сцены можно использовать URP Color Adjustments через Volume, где официально доступны, в частности, post exposure и contrast. Но важно не превращать это в core contract. Core должен хранить только абстрактные параметры: `sceneExposureBias`, `ambientHueShift`, `shadowLift`, `globalLightMultiplier`, `lutRef`, `cameraOverlayRef`. А уже Unity adapter решает, выражать это через Light2D, Volume, материал, shader или простую overlay-текстуру. citeturn28view3turn28view4turn28view5turn28view6

Для weather overlays и маскирования важны две детали. Во-первых, Unity mask stack даёт штатный Sprite Mask путь для reveal/hide поведения, что удобно для укрытия эффектов за арками, воротами, скалами, рамками интерфейса и отдельными слоистыми спрайтами. Во-вторых, UI/scene overlays должны компилироваться в atlases, потому что Sprite Atlas уменьшает draw overhead за счёт упаковки нескольких текстур в одну и позволяет контролировать runtime loading. Поэтому хороший baseline для первой версии — не procedural full-screen shader magic, а набор тайловых overlay-паттернов, масок, градиентов и полос, собранных в atlas и применяемых по metadata. citeturn29view0turn28view0turn28view1

Для day-night влияния на интерфейс рекомендую жёсткое правило: **UI не наследует scene LUT напрямую**. Вместо этого применяйте только `uiMoodShift`, ограниченный валидатором. Примерно так: днём кнопки получают +4% к specular/contrast, ночью — +6% shadow density и +8% cool tint к вторичным элементам, в тумане — -10% saturation только у декоративных фонов, но не у текстов и state indicators. Это не эстетическая прихоть, а инженерный способ не нарушить читаемость и требуемый contrast ratio. citeturn27view0turn27view1

## Библиотеки и инструменты

Для этой задачи инструменты надо оценивать не по «красоте демо», а по четырём критериям: лицензия, фактическая активность, совместимость с .NET 8/WinForms/Unity и способность жить за optional adapter boundary.

**SkiaSharp** — самый сильный кандидат для первого пути. Репозиторий указывает MIT-лицензию, поддержку .NET 6+ и Windows Classic Desktop, включая Windows Forms/WPF; на NuGet у пакета есть совместимость с `net8.0` и обновления в июне 2026 года, что говорит о живой поддержке. Для вашей задачи SkiaSharp закрывает deterministic raster composition, palette transforms, layer blending, atlas generation, preview rendering и export в PNG без привязки к Unity. Недостаток только один: это всё же нативно-обвязанный стек, поэтому его нужно держать в отдельном compiler/adapter слое, а не в core. citeturn14view0turn15view0

**Svg.Skia** выглядит лучшим дополнительным инструментом поверх SkiaSharp, если нужен импорт или генерация умеренно сложного SVG. У проекта MIT-лицензия, C#-кодовая база, NuGet-пакет, свежий релиз `v5.1.1` от 15 июня 2026 года и явный фокус на `SKPicture`/Skia output. Для LLMGameCreator это хороший **optional adapter** для import/export path: читать SVG part packs, конвертировать их в безопасное внутреннее представление, растрировать в atlas. Но канонический source-of-truth всё равно лучше держать в вашем JSON contract, потому что Unity UI Toolkit принимает только подмножество SVG 1.1. citeturn14view1turn26view3

**SVG.NET** формально пригоден, но я бы не делал его основным путём. У проекта MS-PL лицензия, а не MIT/Apache; на NuGet видны обновления до февраля 2024 года, то есть он не мёртвый, но и не выглядит самым удобным кандидатом для свежего compiler path. Он полезен, если вам нужен DOM-подобный editable SVG в .NET, но для задачи deterministic icon/theme compilation связка `SkiaSharp + Svg.Skia` выглядит практичнее и лицензированно проще. citeturn14view2turn15view1

**Magick.NET** полезен как вспомогательный batch/post-process инструмент, но не как первая обязательная зависимость. У него Apache-2.0, официальная поддержка `net8.0` и Windows/Linux/macOS, а GitHub показывает свежий релиз `14.14.0` от 6 июня 2026 года. Сильные стороны — конвертация форматов, quantization, channel ops, batch image transforms. Слабые стороны — тяжёлый нативный footprint и избыточность для базового UI compiler. Итог: годится как optional utility adapter для поздних стадий, когда понадобятся оптимизация atlas-артефактов, пакетная конверсия и сложные channel/mask операции. citeturn17view0turn18view3turn16search8

**NetVips** впечатляет производительностью и памятью. Проект MIT, поддерживает .NET 6+, имеет свежий релиз `3.2.0` от 1 января 2026 года и готовые нативные пакеты для нескольких платформ. Но для текущего объёма задачи NetVips скорее специализированный bulk-image tool, чем удобная основа для theme/icon compiler. Он начинает окупаться, когда вы обрабатываете действительно большие массивы изображений или строите тяжёлый pipeline import/export. Для первых итераций UI themes/weather/VFX это, скорее, преждевременное усложнение. citeturn17view1turn18view1

**ImageSharp** я не рекомендую как default dependency для core или даже для default compiler path. Причина не техническая, а лицензическая: Six Labors прямо пишет, что при direct package dependency в closed-source for-profit software с выручкой выше 1 млн USD нужна коммерческая лицензия; в pricing описаны конкретные платные планы и licence key для сборки. Да, это качественная библиотека, и Microsoft даже перечисляет её среди альтернатив `System.Drawing`, но именно по вашей формулировке «не тащить внешнюю зависимость в core без boundary» и с учётом потенциального future-commercial проекта это лишний юридический риск там, где есть MIT-альтернативы. citeturn31view0turn31view1turn31view2

**System.Drawing.Common** не подходит для shared core. Microsoft официально указывает, что в .NET 6+ пакет поддерживается только на Windows, а `System.Drawing` имеет ограничения по GDI+ и средам выполнения; среди рекомендованных альтернатив Microsoft сама перечисляет SkiaSharp и ImageSharp. Для Windows-only WinForms preview это допустимо как временный локальный путь, но не как долгоживущая основа общего compiler/adapters слоя. citeturn25search0turn31view2

Из Unity-стека наиболее интересен **UI Toolkit**, но только как **Unity-specific adapter**, а не как канон. Он уже имеет rule-based styling через USS, отдельные TSS-темы, runtime switching тем, 9-slice для Texture/RenderTexture/SVG, runtime UI через `UIDocument` и поддержку vector graphics для UI, включая C#-создание path/shape-контента. Слабое место то же: vector importer ограничен подмножеством SVG и не поддерживает filters/animations, поэтому канон не должен зависеть от них. Вывод: UI Toolkit подходит как consumption layer для меню, панелей, кнопок и части HUD, если ваш compiler выдаёт консервативные артефакты и умеет падать обратно в raster sprites. citeturn26view0turn26view1turn26view3turn26view4turn26view5

Для простых Unity-эффектов я бы выбрал **Built-in Particle System**, а не heavy VFX stack. Unity официально указывает, что Built-in Particle System работает на всех поддерживаемых платформах Unity и позволяет управлять системой и отдельными частицами из C#. Для вашей задачи это означает хороший adapter для дождя, снега, дыма, искр, статусов и аур, управляемых чисто через metadata. citeturn28view2

**VFX Graph** на первом этапе лучше отложить. Не потому, что он плохой, а потому что его compatibility и hardware story существенно тяжелее: он зависит от render pipeline, compute shaders, SSBO support и исторически имел урезанную/preview-совместимость с URP и mobile. Если задача сейчас — deterministic metadata-first effects system для 2D и pseudo-3D, то сначала нужно добиться полной закрывающей способности на простых overlays, particles, lights и palette shifts, а уже потом решать, нужен ли VFX Graph отдельным high-end adapter layer. citeturn30view0turn30view1

По transition libraries картина такая. **DOTween** всё ещё живой, бесплатный/open-source в базовой версии, официально совместим с современными Unity-ветками, а в issue tracker есть активность в 2025–2026 годах. Его можно использовать как optional Unity adapter для screen/menu transitions, если вы хотите быстро поднять презентационный слой. Но в архитектуру проекта его стоит пускать только за boundary, потому что он нужен не для source-of-truth, а лишь для materialization уже вычисленных transition contracts. **LeanTween** имеет MIT-лицензию, но по открытому issue-трекеру выглядит заметно менее свежим и менее предсказуемым вариантом. Если вообще брать внешний tweening plugin, то DOTween рациональнее; если переходы будут умеренно простыми, лучше вообще написать очень маленький свой adapter-исполнитель. citeturn19search1turn19search7turn21search2turn21search5turn20search1turn20search4

## Приоритет внедрения и требования к Codex-валидации

**Можно внедрять сейчас.**  
Сейчас имеет смысл внедрять именно то, что соответствует вашим stages 2–4: contracts, fixture packs, deterministic compiler и validation. Конкретно — `UiThemeProfile`, `ThemeTokens`, `SkinRecipe`, `IconRecipe`, `StatusEffectRecipe`, `WeatherLayerSpec`, `DayNightSpec`, `FallbackPolicy`, `AccessibilityPolicy`, а также compiler-артефакты `png/svg/atlas/manifest`. В качестве optional compiler adapter разумно брать SkiaSharp сразу; в Unity consumption — Sprite Atlas и Built-in Particle System; в runtime данных — только precompiled refs и metadata. Это напрямую согласуется с roadmap, где Stage 2 — deterministic recipe resolver, Stage 3 — local deterministic detail generation, Stage 4 — part-pack compiler, а first implementation надо держать metadata/validator/fixture focused. citeturn9view0turn7view3

**Нужно прототипировать.**  
Прототипировать стоит Unity UI Toolkit как consumption layer для меню/панелей/кнопок/HUD и Svg.Skia как import/export tool. Также стоит сделать маленький Unity-prototype для `AtmosphereStack` через `Global Light 2D + Color Adjustments + overlay sprites + Particle System`, чтобы проверить, где проходит граница между «достаточно выразительно» и «уже нужен свой shader-pass». Отдельно допустим прототип DOTween как adapter для переходов между окнами и состояниями, но только после того, как у вас уже будут сами transition contracts. citeturn26view1turn26view3turn28view2turn28view3turn28view4turn21search2

**Отложить.**  
Отложить разумно VFX Graph, NetVips и любые сложные shader-driven distortion/fog/fire pipelines. Они могут понадобиться позже для high-end presentation, но не добавляют критической ценности первому deterministic recipe stack и резко повышают сложность. Также я бы отложил полноценный editable SVG DOM path на базе SVG.NET, если только он не окажется нужен для специального editor workflow. citeturn30view0turn17view1turn14view2

**Не подходит как default path.**  
Не подходит как default dependency ImageSharp из-за split/commercial licensing модели в closed-source scenarios при росте бизнеса; не подходит как shared-core rendering basis `System.Drawing.Common`, потому что Microsoft официально ограничила его Windows-only support в .NET 6+; не подходят любые runtime provider/LLM/media calls, что прямо запрещено и вашим проектом, и текущей задачей. citeturn31view0turn31view1turn25search0turn2view0turn9view0

Для **Codex validation requirements** я рекомендую зафиксировать следующие обязательные проверки.

Первая группа — **детерминизм и воспроизводимость**. Один и тот же `ThemeRequest + Seed + GeneratorVersion` обязан давать одинаковый `ThemeTokens`, одинаковые recipe resolutions и одинаковые compiled artifact hashes. Это прямо продолжает вашу текущую установку на same-seed stability / different-seed variation и golden/snapshot tests. citeturn9view0turn7view3

Вторая группа — **семантическая целостность rule stack**. Валидатор должен проверять, что итоговая тема действительно собирается из допустимых влияний, что forbidden combinations не проходят, что domain/faction/biome/style modifiers применены в установленном порядке и что `fallbackIconId`, `fallbackFrameId`, `fallbackWeatherOverlayId` существуют. Здесь особенно важно сохранить ваш принцип «missing asset must not break runtime». citeturn6view0turn9view1

Третья группа — **визуальная пригодность**. Для текста — минимум 4.5:1. Для контуров кнопок, иконок состояний, selected/focused/disabled indicators и читаемо значимых графических элементов — минимум 3:1. Дополнительно нужен валидатор на «тонкость линий», потому что W3C отдельно предупреждает о практических проблемах у очень тонких не-текстовых элементов из-за сглаживания. Это можно делать через compiled metrics: `minStrokePx`, `minIconInteriorArea`, `focusRingThickness`, `disabledStateDelta`. citeturn27view0turn27view1turn27view2

Четвёртая группа — **adapter safety**. Для Unity UI Toolkit нужно проверять, что SVG-выход не использует text elements, per-pixel masking, filter effects и animations, если артефакт заявлен как `UnityVectorSafe`. Если такой безопасный экспорт невозможен, compiler обязан автоматически переключиться на raster target и записать это в metadata. Это особенно важно, потому что иначе у вас source-of-truth будет обещать то, что Unity runtime просто не материализует. citeturn26view3

Пятая группа — **performance envelope**. Для atlas compiler нужны проверки на максимальный размер atlas, пустое пространство, число вариантов на пакет, bounds слоя, slice metrics и packing policy. Unity официально указывает, что Sprite Atlas помогает снизить draw overhead, но также предупреждает, что чрезмерно большие atlases могут приводить к лишней загрузке текстур, если сцена использует только малую их часть. Поэтому compiler должен уметь резать UI/world/effects atlases по usage group, а validator — ругаться, если паковка начинает смешивать редко совместно используемые категории. citeturn28view0turn28view1

Шестая группа — **boundary enforcement**. Проверка сборки должна подтверждать, что runtime/player не получили ссылки на LLM/provider/media-generation code paths, а theme/effect system не пытается вызывать внешние сервисы. В вашем README это правило сформулировано очень жёстко, и его имеет смысл превращать не только в документацию, но и в архитектурные тесты на ссылки/зависимости. citeturn2view0

Если свести всё к одному practically useful решению, то первая реализация для `docs/deepsearch/` должна выглядеть так:  
**canonical contracts в core, deterministic compiler с optional SkiaSharp adapter, precompiled PNG/SVG/atlas outputs, строгий fallback stack, accessibility validator, Unity adapters на Sprite Atlas + UI Toolkit + Built-in Particle System, а погоду и day-night — через metadata-driven atmosphere stack без runtime AI и без тяжёлых графических зависимостей в первой фазе.** Это лучший баланс между вашим текущим roadmap, будущей масштабируемостью и техническим риском. citeturn9view0turn7view3turn14view0turn26view1turn28view2