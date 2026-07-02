# Pseudo-3D мост от 2D ассетов к first-person exploration для LLMGameCreator

## Исполнительное резюме

Для LLMGameCreator самый практичный путь к first-person exploration в духе *Might and Magic VII* — не пытаться сразу строить «настоящий 3D-мир из картинок», а закрепить промежуточный мост: `Semantic features -> VisualRuleStack -> VisualRecipe -> procedural atlas/facade/billboard outputs -> pseudo-3D presentation sidecar -> Unity/player binding`. Такой ход уже совпадает с текущей архитектурной линией репозитория: source of truth остаётся в `GamePackage`/recipe-данных, Unity остаётся consumer-слоем, а runtime/player не должен вызывать LLM, media providers или сетевые генераторы. Внутри самих proposal-документов это уже сформулировано как `VisualRecipe -> Surface/facade/billboard/atlas outputs -> Pseudo-3D presentation package -> Unity/player binding`, а в roadmap отдельно зафиксированы Stage 5 для pseudo-3D sidecar proof и Stage 10 для Unity consumption только approved refs и fallbacks. citeturn25view0turn26view2turn26view3turn12view2

Практически это означает: **не дом как ассет, а дом как экземпляр рецепта**; **не интерьер как hardcoded Unity сцена, а interior sidecar**; **не NPC как обязательный rigged 3D mesh, а сначала layered billboard/card family с state metadata**. Такая модель уже поддержана текущими документами проекта: в них есть explicit examples для `SurfaceTextureContract`, `FacadeContract`, `BillboardContract`, monster billboard states и grid first-person presentation, а также тезис, что простой PNG недостаточен без world size, pivot, collision footprint, sort policy, atlas rect, compatible presentation modes и fallback. citeturn13view0turn13view1turn13view3turn26view0

Если разделить решения по приоритетам, то **можно внедрять сейчас**: sidecar contracts для поверхностей/фасадов/билбордов, сохранение top-down/isometric как параллельного target, локальные deterministic atlases, collision proxies, local-only Addressables catalogs и Unity Alpha loader только для approved refs. **Нужно прототипировать**: interior sidecar family, свободную first-person прогулку вокруг settlement exteriors, layered creature/NPC presentation, chunk manifests и variant sets для day/night/weather. **Отложить** стоит mesh proxy/glTF quality-upgrade, UV unwrap/simplification toolchain и baked occlusion как основу мира. **Не подходит**: runtime-вызовы LLM/providers, перенос source of truth в Unity, и попытка строить close-range first-person на голом `BillboardRenderer` как на основной технологии. Последний пункт — инженерный вывод из документации Unity, где BillboardRenderer/BillboardAsset описаны прежде всего как LOD-метод для удалённых объектов. citeturn25view0turn22search7turn22search3turn22search12turn15search14turn16search1

## Рекомендуемая bridge-архитектура

Рекомендуемая bridge-архитектура должна строиться вокруг одного правила: **один и тот же `VisualRecipe` обязан уметь компилироваться как минимум в два presentation-target’а** — `top_down_or_isometric` и `pseudo3d_or_first_person`. Это прямо согласуется с вашими документами: top-down/tactical не отбрасывается, а pseudo-3D объявлен главным целевым graphics mode наряду с `first-person grid 2D textures`, `pseudo-3D billboards` и `first-person free billboard`. Такой подход позволяет не делать отдельный стек для «временного 2D» и отдельный стек для «будущего FP»; вместо этого один и тот же recipe отдаёт разные sidecar outputs. citeturn12view1turn26view2

Практическая форма этого моста выглядит так:

```text
GamePackage facts
-> VisualRuleStack
-> VisualRecipe
-> Part-Pack / Detail Generator compiler
-> Approved or fallback visual outputs
-> Pseudo3DPresentationSidecar
-> Unity Alpha loader
-> Top-down / isometric preview target
-> First-person walkable target
-> Later mesh/glTF upgrade target
```

Такой pipeline согласован и с контекстным brief, и с proposal-стратегией: LLM создаёт профили/правила/semantic packs, а код делает массовую deterministic generation, caching, validation и fallback resolution; отдельно зафиксировано решение `Codex writes the generator`, а не вываливает тысячи PNG/JSON dump’ов. citeturn6view0turn7view6turn14view0

Из этого следует важное архитектурное правило для репозитория: **никакие внешние библиотеки для atlas/mesh/gltf не должны попадать в Domain/GamePackage/Runtime как обязательная зависимость**. В самом репозитории уже есть естественные boundary-слои — `AssetPipeline`, `Application`, `Infrastructure`, а roadmap специально говорит держать recipes и sidecars editor-owned до отдельного consumer proof. Поэтому xatlas/meshoptimizer/glTF tooling нужно вешать только на optional adapter boundary, а не в core runtime contract. citeturn25view0turn26view3

В staged path я бы шёл так. Сначала — **`2D recipe -> top-down/isometric preview`**, чтобы быстро проверять biomes, settlements, roads, districts, props и footprint-логику. Затем — **`2D recipe -> pseudo3D sidecar`**, где появляются world scale, pivots, collision proxies, atlas rects и day/night/weather variant ids. Потом — **`Unity Alpha consumption of approved refs`**, но только через approved refs и safe fallbacks. Затем — **`first-person walkable prototype`**, где внешний мир всё ещё largely card/billboard/facade-based, а interiors materialize лишь для важнейших enterable buildings. И только после этого — **`later higher-quality rendering`**, где тот же sidecar начинает указывать уже не на плоскую карту, а на low-poly proxy, glTF asset или более сложный impostor family. citeturn12view2turn26view3turn26view0

## Контракты sidecar и данные рендеринга

В текущих proposal-файлах уже есть хороший минимальный каркас для pseudo-3D: `SurfaceTextureContract` для floor/wall/ceiling, `FacadeContract` для buildings, `BillboardContract` для vegetation/rocks/props/NPC/monsters, actor state sets для монстров и `GridRaycastPresentationContract` для first-person grid dungeon. Это очень хорошая база, потому что она уже отделяет art output от world metadata и не заставляет Unity придумывать ad-hoc logic. citeturn13view0turn13view1turn13view3turn26view0

Но для вашей цели не хватает как минимум трёх sidecar-семейств.

Первое — **`InteriorContract`**. В world-grammar proposal interior surfaces уже перечислены как отдельный объектный класс, но отдельного contract-примера для enterable interiors в найденных документах нет; детализированы сейчас поверхности, фасады, билборды и grid presentation. Поэтому следующий bounded contract должен закрывать именно пробел между фасадом здания и walkable interior. citeturn9view1turn13view0turn13view3

Предлагаемая минимальная форма:

```json
{
  "interiorId": "interior/black_reed_village/house_042",
  "buildingObjectId": "building/swamp_village/house_042",
  "layoutRecipeId": "interior/poor_dwelling/swamp_hut_v1",
  "cellSizeMeters": 2.0,
  "surfaceFamilies": {
    "wall": "surface_family/swamp_hut/interior_walls",
    "floor": "surface_family/swamp_hut/interior_floors",
    "ceiling": "surface_family/swamp_hut/interior_ceilings"
  },
  "propFamilies": [
    "billboard_family/swamp_hut/furniture",
    "billboard_family/swamp_hut/containers"
  ],
  "portalSockets": [
    { "socketId": "entry", "worldDoorRef": "door/main", "spawnCell": [1,0,0] }
  ],
  "collisionProfileId": "collision/interior/poor_dwelling_v1",
  "variantSetId": "variants/swamp_hut/interior/day_night_weather_v1",
  "fallbackInteriorId": "interior/generic/poor_dwelling"
}
```

Это логично продолжает уже существующие contracts, а не ломает их. Основа — те же surface families, billboard families, fallbacks и deterministic ids. citeturn13view3turn26view0turn26view3

Второе — **`ChunkPresentationManifest`**. Для large-world вариативной игры внешний мир нужно грузить chunks, а не монолитом. Документация Unity прямо допускает additive scene loading и local Addressables catalogs, а Addressables умеют грузить дополнительные content catalogs даже с локальной файловой системы. Это ровно то, что нужно для player-facing consumption approved refs без сети. Поэтому chunk manifest должен собирать sidecar refs, collider proxies, atlases, variant sets и approved asset refs по chunk-границе. citeturn16search0turn16search1turn16search7turn16search13

Третье — **`MaterialVariantSet`**. В ваших docs уже упомянуты future normal/height/emissive hints later, а adult/safe pipeline уже опирается на metadata-driven fallbacks и export policies. Для pseudo-3D/FP bridge этого достаточно, чтобы не делать «динамическую графику из логики Unity», а решать day/night/weather как sidecar variant selection: `base`, `night`, `rain`, `snow`, `fog`, `wet`, `emissive_on`, `emissive_off`. В Alpha эти варианты могут быть просто разными approved textures/material refs; позже — уже shader/material proxies или glTF material overrides. citeturn26view0turn12view2turn11view8

Отдельно я бы ввёл **`CollisionProxyContract`** как строго data-driven metadata: `box`, `capsule`, `cylinder`, `polyline`, `stepHeight`, `isBlocking`, `isProjectileBlocking`, `interactionRadius`, `groundOffset`. Это особенно важно, потому что текущий pseudo-3D contract уже прямо требует footprint/collision metadata, а ваш runtime boundary запрещает превращать Unity Player в место, где коллизии «угадываются» по картинке или через LLM/provider. citeturn26view0turn25view0

## Объекты мира и промежуточные targets

Для **buildings и settlements** я рекомендую трёхслойную схему. На дальнем слое settlement живёт как top-down/isometric representation, потому что этот target в проекте явно сохраняется и служит быстрым world authoring/preview-режимом. На среднем слое улицы и внешние периметры materialize как `FacadeContract` + `SurfaceTextureContract` + props billboards. На ближнем слое лишь небольшой процент зданий становится enterable и получает `InteriorContract`. Так вы избежите самого дорогого провала — попытки делать walkable interior для каждой хижины ещё до того, как стабилизированы district grammar, footprints и portals. citeturn12view1turn13view0turn26view0

Для **interiors** лучшая Alpha-стратегия — не свободный бесшовный интерьер для всего мира, а controlled enterables. Иначе вы слишком рано упираетесь в ручной authoring, lighting, memory pressure и content explosion. В ваших документах buildings, settlement districts и interior surfaces уже перечислены как отдельные классы объектов, а Stage 5 и Stage 10 уже предполагают sidecar-driven consumption approved refs. Следовательно, практический шаг — делать interior как sidecar-driven instantiation, а не как hand-authored Unity scene, и materialize его только при входе в building или при приближении к landmark. citeturn9view1turn26view3turn12view2

Для **creatures и NPC** у вас уже есть очень сильная база: proposal по creature visual genome прямо описывает layered character stack и pseudo-3D presentation через metadata — anchor points, depth layers и parallax policy. Это означает, что 100+ существ вполне можно масштабировать не через ручной asset dump, а через `CreatureVisualGenome + VisualRuleStack + part packs + layered_pseudo3d_billboard metadata`. Для Alpha я бы не делал ставку на полноценные rigs/meshes. Лучше bounded set: front-facing/limited-angle actor cards, state sets `idle/attack/hurt/death`, damage/clothing overlays и отдельные shadow assets. citeturn12view3turn13view1turn14view1

Для **items и effects** нужно то же правило, что и для существ: сначала `sprite-to-3D placement`, а не real 3D item meshes. Предметы на земле, контейнеры, ловушки, костры, spell decals и weather effects должны иметь `pivot`, `worldScale`, `collisionProxyId`, `shadowAssetId`, `variantSetId` и atlas rect. Для VFX ранний target — atlas-driven cards и decals; ваши part-pack proposals уже прямо допускают VFX masks, optional control masks и atlas metadata. Это даёт быстрый first-person результат, не привязывая вас к частицам/шейдерам раньше времени. citeturn14view1turn26view0

Top-down/isometric target здесь нельзя считать временным мусором. Наоборот, он нужен как быстрый authoring-proof для grammar, district layout, roads, walls, biome transitions, water placement и settlement readability. Поскольку в proposal прямо сказано, что главная задача — не рисовать уникальные объекты, а из compact semantic features получать reusable visual recipes и pseudo-3D packages, сохранение top-down/isometric как sibling-target делает систему сильнее, а не слабее. citeturn6view0turn26view2

## Runtime streaming и Unity Alpha

Для Unity Alpha я бы строил runtime streaming на **локальных Addressables и локальных content catalogs**, а не на Runtime Generation и не на сетевых провайдерах. Это особенно хорошо совпадает с вашим boundary: Stage 10 требует consumption только уже approved asset refs и fallbacks, а Unity Addressables по официальной документации как раз отделяет «по какому адресу код грузит ресурс» от того, где он физически лежит; built catalog попадает в `StreamingAssets`, а дополнительные catalogs можно подгружать и с локальной файловой системы. Это делает возможной схему `base catalog + biome catalog + settlement/interior catalog + reviewed upgrade catalog` без сетевых вызовов. citeturn9view5turn15search14turn16search1turn16search7turn16search13

Для чанков есть два рабочих режима. Первый — **additive scenes** для relatively static units: district shells, landmark blocks, dungeon floors, interiors. Unity официально поддерживает `LoadSceneMode.Additive`, но отдельно предупреждает, что при использовании light probes после additive load нужно вызвать `LightProbes.Tetrahedralize()`. Второй — **prefab/object pooling per chunk manifest** для повторяемых quads, props billboards и decals. На первом этапе я бы предпочёл второй режим для outdoor world и первый — для interiors/landmarks. citeturn16search0

Для атласов и материалов правильнее делать **bounded atlas families**, а не сверхкрупные универсальные атласы на весь мир. Unity Sprite Atlas консолидирует несколько текстур в одну combined texture и позволяет runtime-load control; это хорошо подходит для семейства `surface`, `facade`, `actor`, `props`, `effects` на biome/domain basis. Но в вашей архитектуре atlas должен оставаться производным выходом compiler-а, а не source of truth. Это полностью совпадает с proposal docs, где atlas output и metadata JSON — производные outputs, а recipe/seed/version — первичны. citeturn24search7turn24search0turn14view1turn7view6

С LOD стратегия должна быть ступенчатой. Вблизи — full facade card, layered actor card или simple low-poly proxy. Средняя дистанция — reduced card/cross-card/impostor. Дальняя — billboard/impostor/2D map proxy. Unity `LOD Group` управляет LOD для renderer’ов, а в учебных материалах Unity отдельно отмечено, что LOD group может состоять из meshes или sprites. Одновременно `BillboardAsset` у Unity описан как multi-direction billboard representation для удалённого объекта. Поэтому later upgrade path очень чистый: не менять recipe ids и chunk sidecars, а постепенно менять конкретные approved refs в LOD ladder. citeturn15search2turn15search9turn22search12

Что я **не рекомендую делать фундаментом Alpha**, так это baked occlusion culling. По официальной документации Unity occlusion culling генерирует данные в Editor и затем использует их в runtime; это подходит для заранее известных сцен, но для мира, который вы хотите процедурно собирать из chunks, district grammars и enterable interiors, это слишком жёсткая основа. Я бы использовал chunk visibility, frustum culling, distance culling и LOD, а occlusion добавлял только позже и локально — например, для фиксированных dungeon/interior blocks. Это вывод из характера Unity occlusion pipeline, а не запрет на саму технологию. citeturn15search3turn15search17

## Инструменты, риски и последовательность целей Codex

### Инструменты и библиотеки

**Unity Addressables** — лучший кандидат для внедрения уже сейчас. Это официальный Unity package, address-based, поддерживает local и remote размещение, строит catalog в `StreamingAssets`, умеет грузить дополнительные catalogs с локальной файловой системы и естественно ложится на модель approved refs + sidecar manifest. Для вашей архитектуры это означает: dependency допустима только в Unity Alpha consumer/adapters, но не в Domain/Runtime core. citeturn15search0turn15search18turn16search1turn16search13turn25view0

**Unity Sprite Atlas** тоже можно внедрять уже сейчас. Это официальный механизм для packing sprites в combined texture, с runtime loading control; он хорошо подходит для ваших surface/facade/actor/effect atlas outputs. Но его роль должна быть вторичной: atlas — производный output compiler-а, а не авторитетный проектный state. citeturn24search7turn24search0turn14view1

**Unity BillboardRenderer/BillboardAsset** — это хорошая технология для прототипирования и later far-LOD, но неудачная база для всего first-person bridge. Unity прямо описывает billboards как LOD-представление сложных мешей на расстоянии, а `BillboardAsset` — как representation из нескольких направлений для примерно горизонтальных взглядов. Как инженерный вывод, для close-range creatures/NPC/buildings в first-person лучше собственный sidecar-driven quad/card renderer или prefab family, а BillboardAsset оставить для дальних impostors, vegetation и, возможно, поздних proxy upgrades. citeturn22search7turn22search3turn22search12

**glTFast** — хороший кандидат для **прототипирования и later higher-quality rendering**, но не для первого pseudo-3D slice. Это официальный Unity package для glTF import/export, он фокусируется на speed, memory efficiency и small build footprint, активно развивается, имеет Apache-2.0 лицензию, 100+ releases и актуальный релиз от 19 мая 2026 года. Но у него есть и прямой технический риск: репозиторий отдельно предупреждает о custom shader graphs и shader variants, которые нужно включать в builds, иначе материалы в Editor и Player будут расходиться. Поэтому glTFast стоит держать как optional Unity-side adapter для quality-upgrades approved refs, а не как базовый способ добраться до Alpha. citeturn21search0turn21search9turn21search15turn19view1turn19view2

**SharpGLTF** — хороший кандидат для editor/generation pipeline уже сейчас, особенно на стороне WinForms/.NET 8 tooling. Это MIT-licensed, 100% .NET Standard library для glTF 2.0 с релизом от 30 декабря 2025 года. .NET Standard поддерживается и .NET, и Unity; Unity официально поддерживает managed plugins, собранные против .NET Standard. Практически это делает SharpGLTF хорошим выбором для экспорта sidecar-approved proxies/GLB из WinForms или generation pipeline, но не для Unity rendering runtime itself. citeturn18view4turn18view5turn21search1turn23search0turn23search5turn23search11

**meshoptimizer** — зрелый и очень активный optional editor-side кандидат для later mesh proxy/LOD stages. Лицензия MIT, релиз свежий — 30 июня 2026 года, огромная коммит-история, а сам проект прямо пишет, что доступен из других языков через FFI/P/Invoke. Для LLMGameCreator это означает: подходит только через native adapter boundary для preprocessing approved mesh proxies, glTF simplification и LOD baking; не подходит как обязательный core dependency из-за native FFI сложности и потому, что Alpha ещё не требует mesh-first pipeline. citeturn20view1turn20view0turn19view4

**xatlas** — тоже уместен только как optional editor-side adapter. Это MIT-licensed C++11 UV unwrapping library без внешних зависимостей, с серьёзной историей коммитов, но без published releases. Для вашей задачи это хороший поздний инструмент под UV unwrap и atlas packing для low-poly/glTF proxies, однако зрелость у него инженерная, а интеграционная цена для C#/Unity заметная: нужен native wrapper, отдельная сборка и изоляция от core. Поэтому я бы поставил его в категорию «отложить до mesh upgrade phase». citeturn19view3turn21search2

### Ключевые Unity-риски

Главный риск Unity Alpha — **catalog/asset drift**. Если approved refs, sidecar manifests и actual local bundles разойдутся, вы получите либо silent fallback storm, либо missing asset errors. Это напрямую противоречит вашей roadmap-логике candidate quarantine, review, promotion и safe fallbacks. Поэтому у каждого chunk/interior/variant set должен быть manifest hash, approved ref hash и fallback proof до попадания в Unity consumption. citeturn12view2turn26view3

Второй риск — **материалы и build-time shader variance**, особенно если позже подключать glTFast или более сложные material variants. Репозиторий glTFast прямо предупреждает, что материалы могут быть корректны в Editor и сломаны в build при отсутствии нужных shader variants. Для Alpha лучше ограничиться максимально простыми material families и variant ids, а полноценные glTF/PBR upgrades переносить на более поздний этап. citeturn19view1

Третий риск — **слишком ранний full free-walk outdoors**. Ваши proposal docs уже допускают и grid first-person, и free billboard first-person. Практически безопаснее идти через `grid/rail/controlled free walk` в bounded exterior slice, а не сразу пытаться сделать полноценный MM7-sized free world. Иначе вы раньше времени упираетесь в streaming, navigation, interior continuity, near-camera billboard readability и collision complexity. Это не запрет на free-walk, а выбор правильной последовательности. citeturn13view3turn12view1

### Последовательность целей Codex

С учётом текущих proposal-docs я бы поставил такую последовательность целей.

Сначала — **цель на sidecar contract family**. Реализовать `SurfaceTextureContract`, `FacadeContract`, `BillboardContract`, `ActorBillboardContract`, новый `InteriorContract`, `CollisionProxyContract`, `MaterialVariantSet` и `ChunkPresentationManifest` как editor-owned contracts без мутации public `GamePackage` schema. Это прямо согласуется с current roadmap, где sidecar contracts идут до Unity renderer proof и до любых provider workflows. citeturn26view3turn25view0

Далее — **цель на `2D recipe -> pseudo3D sidecar compiler`**. Она должна брать `VisualRecipe` и материализовать atlas metadata, pivots, world scales, collision proxies, state sets и fallbacks. Это расширение уже существующих Stage 2–5: recipe resolver, detail generator core, part-pack compiler и pseudo-3D sidecar proof. Acceptance criterion здесь простой: один и тот же fixture recipe компилируется и в top-down preview, и в pseudo-3D sidecar, а один и тот же seed даёт стабильный output. citeturn12view2turn14view2turn14view0

Потом — **цель на Unity Alpha approved-ref loader**. Unity должен читать только approved refs, local catalogs, sidecar manifests и deterministic fallbacks; никакой provider logic, никакой prompt logic, никакой «умной» логики, угадывающей pivots или colliders. Acceptance criterion: safe/public build поднимает chunk, settlement exterior и один enterable interior без unreviewed assets и без сетевых зависимостей. citeturn9view5turn12view2turn26view3

Следом — **цель на first-person walkable prototype**. Минимальный slice: один outdoor settlement edge, один road segment, один dungeon/interior block, несколько vegetation/prop billboards, один creature actor family и item/effect presentation. Здесь важно не качество арта, а доказательство того, что sidecar family действительно закрывает surfaces, facades, interiors, actors, effects, LOD/fallback и chunk loading. Это совпадает с общей логикой проекта: сначала bounded composite proof, потом расширение. citeturn25view0turn26view0turn26view2

И только после этого — **цель на higher-quality rendering adapters**: `SharpGLTF` на generation side, `glTFast` на Unity side, позже optional `meshoptimizer` и `xatlas` в editor preprocessing. Все эти зависимости должны оставаться внешними, опциональными и изолированными boundary-адаптерами. Они не должны становиться необходимым условием для воспроизведения `GamePackage` или для минимального Unity Alpha player. citeturn18view4turn21search9turn20view1turn19view3turn25view0

## Практический итог

Если сформулировать вывод в одной фразе, то для задачи **Pseudo-3D / First-Person From 2D Assets** правильный мост для LLMGameCreator — это **не «sprite-to-3D conversion», а `recipe-to-sidecar presentation`**. То есть вы не конвертируете картинку в трёхмерный мир; вы компилируете recipe в пакет presentation-метаданных, где 2D outputs получают world scale, pivots, state sets, collision proxies, atlas rects, variant ids, portal links и approved refs. Именно такой подход уже заложен в ваших proposal docs и он лучше всего совместим с data-driven source of truth, отсутствием runtime LLM/provider calls и желанием сохранить top-down/isometric как быстрый промежуточный target. citeturn13view0turn26view0turn26view2turn12view2

Самая практичная дорожная карта выглядит так: **сначала recipes и deterministic previews, затем pseudo-3D sidecars, затем Unity Alpha loader approved refs, затем bounded first-person slice, затем optional mesh/glTF quality-upgrades**. Это минимизирует архитектурный риск, не перегружает core внешними зависимостями и оставляет вам прямой путь от HoMM3-подобного data-driven authoring к MM7-подобной first-person exploration без отказа от главного принципа проекта: source of truth остаётся в данных, а не в ручном Unity-контенте и не в live AI. citeturn6view0turn12view1turn26view3turn25view0