# Исследование по масштабируемой генерации внешности существ и NPC для LLMGameCreator

## Вывод для проекта

Для задачи `Creature / NPC Appearance / Body Plan / Paperdoll` у LLMGameCreator уже есть правильное архитектурное направление: source of truth должен оставаться в `GamePackage`/manifest/catalog/recipes/reviewed asset bindings, а runtime и Unity Player должны только потреблять уже утверждённые данные и ассеты, без вызовов LLM, RAG, ComfyUI, Fooocus, InvokeAI, сетевых провайдеров и вообще любых live-generation сервисов. Ваши текущие документы это фиксируют прямо и последовательно. citeturn1view0turn6view1turn7view0

Для масштаба в **100+ видов** ключевая единица должна быть не «готовая картинка NPC», а **иерархия из `SpeciesBodyPlan` + `CreatureVisualGenome` + `CharacterVisualRecipe` + view-specific contracts**. Это совпадает и с вашими проектными документами, где уже есть `bodyPlan`, `surfacePlan`, `clothingSlots`, `equipmentSlots`, `stateOverlays`, `projectionModes`, `adultPresentation`, и с внешними практиками модульных персонажей, где система опирается на слоты, скины, placeholder-ы, категории/метки и runtime composition, а не на хранение каждой комбинации руками. citeturn3view1turn12view2turn18view0turn18view1

Главный риск здесь не «как сделать больше вариаций», а **как не утонуть в комбинаторном мусоре**. У вас это особенно важно, потому что один и тот же персонаж должен консистентно раскрываться в нескольких представлениях: `portrait`, `paperdoll`, `world sprite/billboard`, `pseudo-3D encounter view`, а в будущем — и в first-person encounter/facade-like подаче. Для этого нужен не flat-набор переключателей, а строгая модель совместимости: adjacency, symmetry, part counts, slot ownership, coverage masks, equipment sockets, per-view anchors, fallback policy и rating/export rules. Внешние источники подтверждают, что именно сохранение семантики частей, симметрии и совместимости предотвращает появление «уродливых случайных сборок», а ваши документы уже требуют `forbidden combinations`, `safeFallbackRequired`, `candidateQuarantine`, `reviewStatus` и `exportPolicy`. citeturn23view0turn6view0turn6view1turn7view0

Практический вывод: **внедрять надо не генератор картинок существ, а компилятор визуальной идентичности существ**, который по нормализованным данным строит рецепты и связывает представления между собой. Это полностью укладывается в текущую стратегию проекта: “Codex writes the generator, LLMGameCreator generates variants locally by seed”. citeturn7view1turn5view0

## Таксономия и body plan grammar

Ваши документы уже задают минимальные first-class body-plan ids: `human`, `humanoid_variant`, `anthro_humanoid`, `alien_humanoid`, `monster_humanoid`, `nonhumanoid_safe_only`, `feral_safe_only`. Параллельно академическая работа по creature grammar показывает, что для правдоподобной генерации полезно фиксировать не просто «тип существа», а **категории частей, adjacency relations, число частей в каждой категории и симметрию**; именно эти ограничения сохраняют биологически читаемую форму даже при вариативной перестройке тела. citeturn3view1turn23view0

Из этого следует разумное проектное расширение: держать **двухуровневую таксономию**. Первый уровень — это coarse `bodyPlanFamily`, который определяет locomotion, допустимые слоты и базовую читаемость. Второй — `bodyPlanVariant`, который вводит сигнатурные видовые отличия, не ломая базовую грамматику. Это даёт масштабирование до 100+ видов без 100+ полностью отдельных пайплайнов. Основание для такой схемы есть и в вашем proposal-документе про visual genome, и во внешних системах вроде Spine/Unity, где вариации держатся на общих skeleton/placeholder/category-label схемах. citeturn3view1turn12view2turn18view0

Ниже — **рекомендуемая проектная таксономия**. Это не цитата из внешнего источника, а предлагаемый контракт, выведенный из ваших документов и creature-grammar подхода. Его смысл в том, чтобы `family` контролировала правила, а `species` — сигнатуру.

| `bodyPlanFamily` | Базовая форма | Ключевые слоты | Одежда/экипировка | Rating boundary |
|---|---|---|---|---|
| `human` | двуногий гуманоид | head, torso, arms, legs | полный paperdoll | adult-eligible по явным флагам |
| `humanoid_variant` | гуманоид с фантазийными признаками | + horns/ears/tail/markings | полный paperdoll | adult-eligible по явным флагам |
| `anthro_humanoid` | антропоморфный двуногий | muzzle/ears/tail/fur | частично полный, зависит от coverage profile | adult-eligible только при humanoid-compatible |
| `alien_humanoid` | инопланетный гуманоид | extra sockets, alternate face surface | полный или частичный | adult-eligible только по review |
| `monster_humanoid` | монстроподобный, но двуногий | asymmetry, oversized limbs, shell/plates | ограниченный full-body + per-slot gear | adult-eligible только по review |
| `quadruped` | четыре опоры | head, neck, trunk, fore/hind limbs, tail | gear-lite, harness/armor zones | safe-only по умолчанию |
| `avian_biped` | двуногий птицеподобный | beak, crest, wings, talons | attire-lite, back/torso/accessory | safe-only по умолчанию |
| `serpentine` | сегментированное/безногое | head, neck, body segments, tail | jewelry/armor bands/cloak substitutes | safe-only по умолчанию |
| `insectoid` | членистоногое | mandibles, thorax, abdomen, limb groups, shell plates | armor overlays только через part-zones | safe-only по умолчанию |
| `construct_blob_special` | голем/слизь/аморф | core, shell, emitters, aura | no traditional paperdoll | safe-only |

Рекомендую жёстко разделить следующие понятия: `bodyPlanFamily`, `locomotionClass`, `silhouetteClass`, `surfaceClass`, `compatibilityPolicy`, `adultEligibilityPolicy`. Сейчас в ваших документах часть этой информации уже существует, но смешана между `bodyPlan`, `surfacePlan`, `humanCompatibility` и `adultPresentation`; формализация снимет много двусмысленностей на этапе валидаторов. citeturn3view1turn4view1turn6view1

Отдельно важно ввести **species signature** как минимальный неизменяемый набор отличий вида: например, `hornProfile`, `earProfile`, `eyeShapeFamily`, `primarySurfaceFamily`, `tailPresence`, `silhouetteBias`, `paletteFamily`, `markingPatternFamily`. Это защитит визуальную различимость видов, когда поверх базового тела начнут накладываться одежда, броня, грязь, кровь, намокание и повреждения. Такой подход напрямую отвечает на ваш риск потери различимости между `portrait`, `paperdoll` и `world sprite`, а его логика согласуется с требованием сохранять идентичность вида при разных `projectionModes` и reviewed asset variants. citeturn3view1turn5view1turn29view0

## Предложение по visual genome

В текущих документах `CreatureVisualGenome` уже описан как стабильный data contract для visual identity species/family, который должен отвечать на вопросы о body plan, variation axes, clothing/equipment/wounds/states, deterministic composition, safe/public slots, adult-only slots и forbidden combinations. Это сильная база, но для production-scale системы на 100+ видов ей не хватает явного разделения между **видовыми константами**, **внутривидовой вариативностью** и **текущим presentation state**. citeturn3view1turn4view1

Рекомендую разложить модель на три уровня:

1. **`SpeciesBodyPlan`** — анатомическая грамматика вида. Здесь живут количество конечностей, symmetry class, adjacency graph, socket topology, part families, locomotion and equipment compatibility.
2. **`CreatureVisualGenome`** — видовая и подвидовая идентичность. Здесь живут silhouette signature, palette families, surface families, sexual/sex-presentation variants, allowed morph ranges, clothing/equipment capabilities, view projections.
3. **`CharacterVisualRecipe`** — конкретный resolved-state экземпляр для NPC/creature. Здесь живут seed, phenotype picks, age band within adult-safe range if applicable, face preset, hairstyle/horns/tail variant, equipped items, clothing state, damage state, weathering, active overlays, chosen fallback chain.

Такое разделение хорошо ложится на вашу архитектуру `semantic facts + rules + seed -> recipe -> asset slots -> fallback/control output`, а также на существующие `presentation_refs`/`actor_model_profile` планы, где runtime должен работать уже с валидированными ссылками, а не с authoring noise. citeturn3view0turn7view0turn29view0

Ниже — рекомендуемый каркас `CreatureVisualGenome`. Это проектное предложение, а не цитата; оно специально выстроено так, чтобы быть sidecar/editor-owned до появления утверждённого runtime consumer.

```json
{
  "schemaVersion": "creature_visual_genome_v2",
  "speciesId": "species/ashen_laminar",
  "bodyPlanFamily": "humanoid_variant",
  "bodyPlanVariant": "digitigrade_horned_tail_optional",
  "speciesSignature": {
    "silhouetteClass": "tall_graceful",
    "signatureParts": ["horns", "glow_markings", "tail_optional"],
    "primarySurfaceFamily": "skin_with_subtle_scales",
    "paletteFamilies": ["ash_gray", "violet_glow", "dark_gold"],
    "readabilityAnchors": ["horn_outline", "eye_glow", "tail_shape"]
  },
  "anatomyGrammar": {
    "symmetryClass": "bilateral",
    "requiredSlots": ["head", "torso", "arm_l", "arm_r", "leg_l", "leg_r"],
    "optionalSlots": ["horns", "tail", "wings"],
    "forbiddenSlots": ["muzzle_long", "mandibles"],
    "socketGraph": [
      ["head", "horns"],
      ["pelvis", "tail"],
      ["scapula", "wings"]
    ]
  },
  "surfaceGrammar": {
    "baseSurface": "skin",
    "secondarySurfaces": ["subtle_scales", "emissive_markings"],
    "conditionChannels": ["dirt", "wetness", "burn", "scar", "corruption"]
  },
  "paperdollGrammar": {
    "coverageZones": ["underlayer", "chest", "waist", "legs", "feet", "hands", "back"],
    "equipmentSockets": ["main_hand", "off_hand", "neck", "ring", "back", "faction_badge"],
    "occlusionPolicies": ["hide_body_under_armor", "keep_signature_horns_visible"]
  },
  "viewBindings": {
    "portrait": "view_profile/portrait_bust_v1",
    "paperdoll": "view_profile/fullbody_front_v1",
    "worldBillboard": "view_profile/billboard_actor_v1",
    "encounter": "view_profile/pseudo3d_encounter_v1"
  },
  "ratingPolicy": {
    "defaultRating": "safe",
    "allowedRatings": ["safe", "suggestive"],
    "adultEligibilityPolicy": "adult_sapient_humanoid_review_required"
  },
  "fallbackPolicy": {
    "missingPart": "species_default_part",
    "missingLayer": "drop_layer",
    "missingViewAsset": "species_generic_view"
  }
}
```

Ключевая добавка здесь — **`readabilityAnchors`**. В ваших документах уже есть `anchorPoints`, `depthLayers`, `pivot`, `state completeness` и `presentation refs`; я предлагаю поверх этого ввести обязательный минимальный набор визуальных якорей вида, которые нельзя заслонять или удалять без специального fallback rule. Иначе в броне, плаще или мокрой/грязной одежде вид быстро превращается в «ещё одного гуманоида». citeturn3view1turn5view1turn29view0

Второе важное добавление — **`coverageZones` и `occlusionPolicies`** как first-class data. Внешние модульные character systems работают стабильно именно тогда, когда часть набора отвечает за внешний вид, а часть — за сокрытие/замену перекрываемых поверхностей. Unreal официально описывает hidden-surface removal и layering как важные части кастомизируемых персонажей, а ваши собственные документы требуют, чтобы состояния одежды и equipment были state-driven, а не hand-authored per-character. citeturn19search6turn3view1

## Слои, состояния и защита от комбинаторного мусора

Ваш текущий recommendation stack для creatures уже очень близок к рабочему: base silhouette, body surface, species markings, face/expression, hair/mane/head ornaments, appendages, underclothing, outer clothing, armor/equipment, faction accessories, wound/dirt/wetness overlays, transformation overlays, lighting normalization, rating-gated overlays. Это хороший базовый порядок. Но для 100+ видов его надо сделать не просто ordered list, а **слоистую модель с правилами владения, сокрытия и деградации**. citeturn3view1turn4view0

На практике я рекомендую ввести четыре типа слоёв.

Первый тип — **identity layers**. Это тело, поверхность, видовые метки, базовая форма головы, хвоста, крыльев, рогов и других signature appendages. Эти слои задают различимость вида и почти никогда не должны полностью исчезать. Если экипировка перекрывает их, должен срабатывать не `hide`, а `signature-preserving substitute`, например уменьшенный контур рогов, outline-карта хвоста, alternate portrait crop. Поддержка такого подхода уже видна в ваших `safe fallback`, `projectionModes` и `species consistency` требованиях. citeturn3view1turn6view1

Второй тип — **coverage layers**. Это одежда, броня, плащи, ремни, юбки, набедренники, наплечники, перчатки и прочие надеваемые элементы. Здесь нельзя хранить всё как отдельные битовые флаги. Нужны `coverageProfiles`: например, `robe_full`, `armor_plate_medium`, `worker_rags_light`, `ceremonial_open`. Тогда один профиль определяет и какие зоны закрываются, и какие body parts должны быть скрыты, и какие secondary overlays разрешены поверх. Такой подход сильно снижает взрыв комбинаций. Внешние модульные системы подтверждают эту практику: Spine предлагает собирать предметы как skin-группы вместо хранения каждой комбинации, а Unity 2D Animation опирается на `Category/Label` с одинаковым skeleton и взаимозаменяемыми sprite-элементами. citeturn12view2turn13view0turn18view0turn18view1

Третий тип — **state overlays**. Их надо хранить независимо от одежды и экипировки, как вы уже и предлагаете: dirt, wetness, torn, blood, burns, corruption, exhaustion. Ключевое правило: overlay не создаёт новый костюм, а модифицирует уже выбранный костюм и тело. Это очень важно для экономии контента. Ваши документы прямо рекомендуют damage и wound states как независимые overlays, а не отдельные hand-authored variants. citeturn3view1turn4view0

Четвёртый тип — **view adapters**. Один и тот же `CharacterVisualRecipe` должен по-разному разворачиваться в `portrait`, `paperdoll`, `world billboard`, `encounter card`. Здесь нельзя копировать все слои 1:1. Например, `portrait` должен усиливать face/expression и species markers выше груди, `paperdoll` — показывать полнотелую сборку и coverage, а `world billboard` — упрощать детали, повышать силуэт и контраст. Это логически следует из ваших `projectionModes`, `Pseudo-3D presentation`, `BillboardContract`, `presentation_refs` и `actor model` планов. citeturn3view1turn5view1turn29view0

Чтобы защититься от комбинаторного мусора, нужна не только слоистость, но и **constraint model**. Минимальный набор правил я бы сделал таким:

| Правило | Что проверяет | Тип реакции |
|---|---|---|
| `slot compatibility` | может ли part быть установлен в данный slot/body plan | reject + fallback |
| `signature preservation` | не исчезли ли обязательные species markers | substitute/fallback |
| `coverage conflict` | не конфликтуют ли два coverage profiles | remove lower-priority layer |
| `occlusion policy` | какие body zones надо скрыть под одеждой/бронёй | deterministic hide/mask |
| `style coherence` | не смешались ли несовместимые families/palettes | reroll within family |
| `rarity budget` | сколько редких выразительных деталей допустимо | cap and reroll |
| `silhouette budget` | не стал ли персонаж визуально шумным | simplify to readable subset |
| `view readiness` | есть ли все нужные представления и fallbacks | fail closed for missing view |

Это не абстрактная перестраховка. Работа по creature grammar прямо показывает, что без knowledge about geometric styles among shape parts система начинает собирать экстремально несовместимые комбинации; в ваших же документах уже заложены `forbidden combinations`, `validation`, `compatibleBodyPlans`, `compatibleSexPresentationProfiles`, `safeFallbackPartId` и запрет на noisy large-data dumps вместо генератора. citeturn23view0turn4view0turn7view1

Если constraints вырастут до действительно сложной сетки, можно рассматривать отдельный solver-адаптер для editor-side резолва экипировки и coverage conflicts. Но это должно оставаться **optional adapter boundary**, а не основой core-архитектуры. Это полностью соответствует вашей политике внешних зависимостей: сначала внутренний контракт, потом обёртка, потом опциональная библиотека. citeturn30view0

## Связь portrait, paperdoll, world sprite и encounter view

Для LLMGameCreator особенно важно, чтобы разные представления персонажа не жили раздельно. Ваши документы уже подсказывают правильный путь: `CreatureVisualGenome` знает про `projectionModes`; `Pseudo-3D` контракты знают про `anchorPoints`, `depthLayers`, `pivot`, `states`; `character_card_v1` уже резервирует `presentation_refs` для portrait/sprite/billboard/fallback. Значит, естественный следующий контракт — **`ActorVisualBinding`**. citeturn3view1turn5view1turn29view0

Я рекомендую сделать так, чтобы у каждого actor family был один корневой binding:

```text
ActorVisualBinding
  -> speciesBodyPlanId
  -> creatureVisualGenomeId
  -> defaultPaperdollProfileId
  -> defaultPortraitProfileId
  -> defaultWorldBillboardProfileId
  -> defaultEncounterProfileId
  -> approvedAssetRefs / generatedFallbackRefs
  -> validationReportRef
```

Тогда `portrait`, `paperdoll`, `world`, `encounter` становятся не отдельными независимыми активами, а **разными материализациями одного visual identity**. Это особенно полезно для видов с сильной сигнатурой: если species marker изменился в portrait, обязан обновиться и world-billboard resolver. Такой подход следует из вашей общей формулы `semantic facts -> recipe -> sidecar metadata -> player binding`, а также из того, что ваши character cards уже планируют единый набор `presentation_refs`. citeturn5view0turn7view0turn29view0

Практический пайплайн я бы описал так:

```text
SpeciesBodyPlan
+ CreatureVisualGenome
+ Character facts
+ Equipment loadout
+ Clothing state
+ Damage/weather state
+ Rating/export policy
+ Seed
-> CharacterVisualRecipe
-> View adapters:
   - portrait adapter
   - paperdoll adapter
   - world billboard adapter
   - encounter adapter
-> approved asset refs or deterministic safe fallbacks
```

Для `portrait` нужны дополнительные правила crop-композиции: какие signature anchors обязательно входят в кадр, какие эмоции доступны, какие overlays допустимы, как отображать мокрую/грязную/порванную одежду без потери читаемости. Для `paperdoll` приоритетом становится coverage and slot truth: игрок или редактор должны видеть реальное состояние слотов и одежды. Для `world billboard` приоритет — silhouette, pivot, nominal height, state availability и fallbackStatePolicy. Для `encounter` или `pseudo-3D billboard` — depth layers, parallax hints и front/back split по appendage layers. Всё это уже укладывается в ваши существующие предложения по `depthLayers`, `BillboardContract`, `fallbackStatePolicy`, `pivot_based sorting` и `presentation refs`. citeturn3view1turn5view1turn29view0

Особое правило я бы ввёл для first-person и pseudo-3D encounter представления: **не тянуть туда весь paperdoll**. Для encounter view должен существовать отдельный `encounterProjectionProfile`, который говорит, какие слои из recipe участвуют в этой подаче, какие упрощаются, а какие заменяются encounter-specific approved assets. Иначе вы получите либо чрезмерно дорогой compositing, либо визуальную кашу. Ваши Pseudo-3D contracts прямо показывают, что player-facing представление должно опираться на sidecar metadata, pivots, world scale, state sets и fallbacks, а не на импровизацию внутри Unity runtime. citeturn5view1turn7view0

## Rating-safe границы и neutral adult metadata

Здесь ваши документы уже задают очень жёсткую и правильную политику: adult-capable visuals допускаются только как **rating-gated metadata/asset slots/overlays/review decisions внутри общего визуального пайплайна**, только для adult + sapient + humanoid-compatible конфигураций, с обязательным `safeFallbackRequired`, `candidateQuarantine`, `reviewStatus`, `exportPolicy`, и с reject/quarantine для age-ambiguous, feral, non-sapient, coercive и unsafe-public-build случаев. Это полностью согласовано и между `VISUAL_WORLD_GENERATION_CONTEXT_BRIEF`, и между `ADULT_VISUAL_LAYER_STRATEGY`, и между `VISUAL_ADULT_LAYER_CONTEXT_INDEX`. citeturn3view0turn4view1turn6view1

Для этой задачи важно не расширять adult-функциональность, а **сделать metadata нейтральной и безопасной для масштабирования**. Я бы рекомендовал разделить внутренние поля на два уровня.

Первый уровень — **project-facing rating metadata**:

- `rating`: `safe | suggestive | adult_nude_reference | adult_erotic_scene | adult_private_explicit`
- `exportPolicy`: `public_safe | mature_optional | adult_build_only | private_local_only | blocked`
- `eligibilityFacts`: `adultEnabled`, `adultCharacterEligible`, `sapient`, `humanoidCompatible`, `safeFallbackRequired`
- `reviewFacts`: `candidateQuarantine`, `reviewStatus`, `approvedBy`, `reviewedAt`

Эти сущности уже почти полностью присутствуют в ваших документах. citeturn4view1turn6view1turn7view0

Второй уровень — **descriptor metadata**, уже ближе к внешним rating-системам. Это нужно не для прямого соответствия ESRB/PEGI, а чтобы не смешивать в одно поле совершенно разные вещи. У ESRB раздельно существуют `Suggestive Themes`, `Sexual Themes`, `Sexual Content`, `Strong Sexual Content`, `Partial Nudity`, `Nudity`; PEGI тоже различает sexual posturing/innuendo, erotic nudity or intercourse without visible genitals, explicit sexual activity. Это означает, что одна колонка `rating` почти наверняка будет недостаточна для точной внутренней фильтрации и ревью. citeturn9view0turn10view0turn10view2turn10view3

Поэтому я бы добавил нейтральные независимые дескрипторы:

```json
{
  "sexualDescriptor": "none | suggestive | references | non_explicit | explicit",
  "nudityDescriptor": "none | partial | nonsexual_reference | erotic | explicit",
  "violenceDescriptor": "none | mild | blood | gore",
  "consentPolicy": "not_applicable | implied_safe | review_required",
  "sapiencePolicy": "sapient_only",
  "ageClarityPolicy": "adult_only_clear"
}
```

Это поможет избежать опасной ситуации, когда `adult_nude_reference` и `adult_erotic_scene` технически попадают в один и тот же bucket, хотя для review/export/filtering это разные классы риска. Такая декомпозиция напрямую опирается на официальные content descriptor models ESRB/PEGI и на ваши собственные правила export gating. citeturn9view0turn10view0turn10view3turn4view1turn6view1

Важно и другое: для **nonhumanoid_safe_only** и **feral_safe_only** adult metadata вообще не должна materialize-иться как normal branch. Не `adultEnabled: false`, а `adultBranch: nonexistent`. Иначе в больших массивах генерации вы можете случайно получить dangling optional adult slots, которые потом всплывут на этапе export filter или review UI. Ваши документы явно требуют fail-closed поведение и reject/quarantine для inappropriate combinations; значит, safest path — вообще не создавать такие ветви там, где они семантически невозможны. citeturn3view1turn6view1turn7view0

## Инструменты, лицензии и решение по внедрению

Ниже — практический scouting по инструментам, с привязкой к вашим ограничениям: **C#/.NET 8/WinForms/Unity**, без live runtime providers, без внешней зависимости в core без adapter boundary, с оценкой лицензии, зрелости и рисков. Основание для такого подхода есть и в вашей internal policy по external technology scouting. citeturn30view0

| Инструмент | Лицензия и активность | Платформа и совместимость | Для чего годится в этой задаче | Решение |
|---|---|---|---|---|
| **SkiaSharp** | MIT; GitHub отмечает MIT, репозиторий активен, latest `4.148.0` от 2026-06-23. citeturn20view5turn15search0 | Cross-platform 2D API для .NET; NuGet показывает совместимость с .NET 8, .NET Standard 2.0 и .NET Framework 4.6.2+, что хорошо для ваших editor-side .NET/WinForms инструментов. citeturn20view0turn27view1turn27view2 | Детерминированный compositor для masks, silhouettes, sprite assembly, preview rendering, atlas sidecars, control images. | **Можно внедрять сейчас**, но только как **optional adapter** в editor/generation pipeline. В core хранить собственные contracts, не типы SkiaSharp. |
| **Magick.NET** | Apache-2.0; свежие релизы в 2026 году, latest `14.14.0`, проект активен. citeturn26view0turn26view1 | Библиотека тестируется на Windows/Linux/macOS, доступна для `net8.0` и `netstandard20`; подходит для .NET tooling. citeturn26view3turn21view1 | Batch compositing, trimming, conversions, format pipeline, atlas post-processing, masks/overlays, byte/hash-friendly media tooling. | **Нужно прототипировать**. Сильный кандидат для editor-side media pipeline, но тяжелее SkiaSharp и несёт native/lib pipeline overhead. Только за adapter boundary. |
| **Unity 2D Animation** | Unity Companion License for Unity-dependent projects; пакет `16.0.0` выпущен для Unity Editor 6000.7. citeturn17search2turn17search13 | Официальный Unity package; `Sprite Library Asset`, `Sprite Library`, `Sprite Resolver` и `Category/Label` хорошо подходят для visual variants, но это Unity-dependent стек. citeturn12view3turn12view4turn18view0 | Unity-side consumption prototype для paperdoll/world presentation, если позже понадобится визуальный proof-of-consumption approved refs. | **Нужно прототипировать**, но **не в core**. Использовать только после sidecar contracts и approved asset refs, как Unity-specific adapter. |
| **Google OR-Tools** | Apache-2.0; официальный .NET install guide и свежие релизы/nuget-пакеты подтверждают активность. citeturn16search6turn15search2turn16search0 | NuGet-пакет поддерживает .NET 8 и .NET Framework 4.6.2+, значит editor-side C# интеграция возможна. citeturn21view2turn21view3 | Constraint solving для сложных outfit/coverage/socket conflicts, если hand-written validator/resolver станет слишком запутанным. | **Нужно прототипировать** только как offline/editor-side solver adapter. Для первого среза, вероятно, избыточен. |
| **Spine** | Коммерческий/проприетарный editor license; runtime use завязан на Spine license terms, включая распределение и условия интеграции. citeturn25view0turn25view1turn28search0 | Сильные runtime skin/placeholders/mix-and-match механики; отлично показывает, как собирать одежду и части без хранения всех комбинаций. citeturn12view1turn12view2turn13view0 | Хороший **эталонный reference pattern** и возможный импортный путь для hand-authored hero characters. | **Отложить** как зависимость. Использовать **как референс и необязательный import/export adapter**, только если коммерческие условия действительно подходят проекту. |
| **SixLabors.ImageSharp** | Split License / commercial considerations; .NET Foundation отдельно предупреждала, что проект сменил лицензию, а NuGet указывает Split License. citeturn20view2turn20view3turn14search3 | Технически .NET-friendly, но licensing assessment для closed-source/коммерческого роста менее комфортен. | Мог бы решать часть editor-side image задач. | **Не подходит как базовая рекомендация** для этого направления. При ваших требованиях проще и безопаснее стартовать со SkiaSharp или Magick.NET. |

К этому списку я добавлю ещё один важный практический вывод. **Spine и Unity 2D Animation полезны прежде всего как proof, что у модульной кастомизации должны быть template placeholders / categories / labels / shared skeleton assumptions, а не как призыв тянуть их в архитектурный центр LLMGameCreator.** Это полностью согласуется с вашей policy: внешняя библиотека не должна становиться архитектурой. citeturn12view2turn18view1turn30view0

## Матрица валидации и Codex goals

Для такого subsystem-а validator важнее рендера. Ваш roadmap уже ставит metadata/validator/fallback/quarantine раньше, чем media output widening, и это полностью правильно. Для appearance/paperdoll направления я бы зафиксировал следующую матрицу валидации. citeturn7view0turn6view1

| Слой | Что проверять | Ошибка | Реакция |
|---|---|---|---|
| `SpeciesBodyPlan` | required slots, symmetry class, adjacency graph, locomotion class | broken anatomy grammar | reject species profile |
| `CreatureVisualGenome` | species signature completeness, allowed projections, allowed equipment classes, fallback presence | incomplete identity contract | reject or demote to candidate |
| `CharacterVisualRecipe` | slot picks, palette family, rarity budget, silhouette budget, forbidden combinations | combinatorial garbage | reroll within seed scope or fallback |
| `Clothing/Equipment` | coverage conflicts, socket occupancy, occlusion policy, hidden-surface rules | intersecting layers / unreadable silhouette | remove lower-priority item or change coverage profile |
| `State overlays` | allowed overlay order, intensity caps, rating compatibility | broken state stack | clamp / simplify overlays |
| `View binding` | portrait/paperdoll/world/encounter consistency, anchor presence, nominal height/pivot/state completeness | inconsistent cross-view identity | fail closed for affected view, keep safe fallback |
| `Rating/export` | adult eligibility facts, safe fallback, export policy, review status, quarantine status | unsafe leak into public/safe build | block export |
| `Asset provenance` | relative paths, byte/hash, approved asset ref, no provider prompt as source of truth | unreviewed/tampered asset | quarantine |

Эта матрица не выдумана с нуля: почти все её элементы уже есть либо в ваших документах (`safeFallbackRequired`, `candidateQuarantine`, `approvedAssetRef`, `state completeness`, `pivot`, `fallbackStatePolicy`, `validation`), либо во внешних системах, где модульность строится на совместимости skeleton/attachments/skins/categories. citeturn5view1turn6view1turn7view0turn12view2turn18view1

Ниже — **рекомендуемые Codex goals** именно для этого исследования.

| Goal | Суть | Статус |
|---|---|---|
| `species_body_plan_contracts` | Ввести `SpeciesBodyPlan`, `CoverageZone`, `EquipmentSocketProfile`, `SpeciesSignature`, `ViewProfile` как editor-side contracts | **Можно внедрять сейчас** |
| `creature_visual_genome_v2` | Пересобрать текущий proposal в трёхуровневую модель: `SpeciesBodyPlan` → `CreatureVisualGenome` → `CharacterVisualRecipe` | **Можно внедрять сейчас** |
| `appearance_constraint_validator` | Написать deterministic validator для slot compatibility, coverage conflicts, forbidden combinations, silhouette budget, fallback completeness | **Можно внедрять сейчас** |
| `paperdoll_binding_sidecar` | Ввести `ActorVisualBinding`/`PresentationBindingMap`, который связывает portrait/paperdoll/world/encounter refs | **Можно внедрять сейчас** |
| `deterministic_placeholder_materializer` | Сделать editor-side placeholder materializer для silhouette/paperdoll/world previews на SkiaSharp adapter | **Нужно прототипировать** |
| `solver_adapter_for_complex_outfits` | Прототип OR-Tools adapter только если constraint matrix станет реально NP-подобной по сложным outfit rules | **Нужно прототипировать** |
| `unity_sprite_library_adapter` | Показать consumption proof approved refs через Unity `Sprite Library`/`Sprite Resolver` без изменения core contracts | **Отложить** |
| `spine_import_or_export_adapter` | Рассмотреть только как optional importer for hero-grade authored rigs/skins | **Отложить** |
| `runtime_provider_or_llm_appearance_calls` | Любые live runtime provider/LLM/media generation calls | **Не подходит** |

Если выбирать **самый правильный первый срез**, я бы делал не artwork pipeline, а следующий пакет:

- contracts для `SpeciesBodyPlan`, `CreatureVisualGenome`, `CharacterVisualRecipe`, `ActorVisualBinding`;
- deterministic validator;
- 3–5 body plan families;
- 12–20 fixture species;
- 4–6 clothing coverage profiles;
- 6–10 equipment socket profiles;
- 4 view profiles;
- safe fallback matrix;
- snapshot tests на consistency между portrait/paperdoll/world/encounter.

Такой срез даст архитектурное доказательство без провала в art production, не изменит runtime boundary, не подтянет запрещённые провайдеры и прямо продолжит ваш текущий roadmap в духе metadata-first, validator-first, fallback-first. citeturn7view0turn6view1turn30view0

Сводный итог по задаче: **для LLMGameCreator масштабируемый appearance subsystem должен быть не “генератором картинок существ”, а data-driven compiler-ом визуальной идентичности**, где `body plan grammar` задаёт анатомию, `visual genome` — видовую сигнатуру, `layering/coverage/state model` — сборку и деградацию, `ActorVisualBinding` — связь между portrait/paperdoll/world/encounter, а rating/adult metadata остаётся нейтральной, export-gated и fail-closed. Это архитектурно совместимо с уже существующими документами проекта и практически реализуемо без нарушения главной границы: runtime не вызывает LLM и не зависит от media providers. citeturn1view0turn3view0turn3view1turn6view1turn7view0