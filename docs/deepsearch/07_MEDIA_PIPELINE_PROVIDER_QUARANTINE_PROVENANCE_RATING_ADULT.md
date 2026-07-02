# Editor-only media pipeline для LLMGameCreator

Рекомендуемое имя файла для сохранения: `docs/deepsearch/media-pipeline-provider-quarantine-provenance-rating-adult.md`

## Вывод для проекта

Текущее направление репозитория уже задаёт правильную рамку: source of truth должен оставаться в `GamePackage`/manifest/catalog/recipes/reviewed asset bindings, а Runtime и Unity Player не должны вызывать LLM, RAG, ComfyUI, Fooocus, InvokeAI, сетевые media providers или любые live-generation сервисы. В repo README это зафиксировано как базовая runtime boundary, а в `VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md` и `VISUAL_ADULT_LAYER_CONTEXT_INDEX.md` прямо сказано, что provider output должен считаться quarantined candidate до детерминированной валидации и human review, при этом prompt text не может быть authoritative state. citeturn2view0turn3view0turn3view1

Для задачи safe pipeline под adult-capable games оптимальная архитектура не в том, чтобы “добавить генератор взрослого контента”, а в том, чтобы построить единый editor-only media pipeline с жёстким разделением четырёх состояний: **candidate**, **quarantine**, **reviewed/promotion-ready**, **approved asset ref**. Adult/NSFW в таком дизайне становится не отдельной генерацией, а лишь более строгим policy layer поверх общих правил: provenance, hash/byte validation, relative paths, review status, export policy, deterministic safe fallbacks и fail-closed export filtering. Именно такой подход совпадает и с вашими документами по adult layer, и с внешними требованиями платформ: Valve в Steamworks требует раскрывать весь adult content, даже если он не доступен игроку в текущем продукте, а live-generated AI adult sexual content во время gameplay они отдельно не хотят допускать из-за юридических и customer-risk причин. citeturn4view0turn3view1turn17view0

Практический итог: в LLMGameCreator имеет смысл внедрять **не provider-centric pipeline**, а **evidence-centric pipeline**. Провайдер — только опциональный editor adapter. Истина для системы — не prompt, не raw image, не ответ модели, а связка из reviewed metadata, validated bytes, project-relative artifact path, policy verdict и `approvedAssetRef`. Это полностью соответствует текущему проектному курсу на data-driven generation и разгрузку LLM в пользу детерминированного procedural/compiled visual stack. citeturn3view2turn3view0turn3view1

## Целевая архитектура pipeline

Рекомендуемый pipeline для проекта выглядит так: **recipe/spec drafting → optional provider request compilation → candidate materialization → quarantine validation → human review → promotion to approved asset ref → export filtering → runtime consumption only by approved refs/fallback refs**. Важный момент: provider adapter не должен писать прямо в package catalog или Unity asset area. Он пишет только в отдельный quarantined artifact root, а promotion — это отдельная транзакция Application layer, которая после валидации создаёт reviewed binding и только затем, при необходимости, публикует `approvedAssetRef` в runtime-facing catalog/manifests. Такой разрыв логически совпадает с существующим делением репозитория на `AssetPipeline`, `Application`, `Infrastructure` и editor/generation side, в то время как Runtime потребляет только compiled validated package data. citeturn2view0turn3view0turn3view1

Минимальный набор уровней хранения лучше сделать таким. Первый уровень — **ephemeral provider workspace**: здесь разрешены prompt templates, intermediate masks, временные control images и любые внешние job-id. Этот слой не является source of truth и может очищаться. Второй уровень — **quarantine ledger**: здесь лежат candidate files, immutable metadata, hash/byte facts, decode facts, provenance envelope, validation results, review status и причины блокировки. Третий уровень — **approved asset store**: здесь лежат только промотированные ассеты либо детерминированные fallback assets, каждый из которых имеет стабильный `approvedAssetRef` и runtime-safe contract. Четвёртый уровень — **exported package view**: это уже профильная выборка для `public_safe`, `mature_optional`, `adult_build_only` или `private_local_only`, где физически исключены запрещённые файлы, а не просто выключены флагом. Steam прямо требует disclosure всего adult content, загруженного в build, даже если оно недоступно; следовательно, “спрятать в билде, но не показывать” недостаточно. citeturn17view0turn3view1turn4view0

Для provider adapter boundary нужен контракт вида: `CompileRequest`, `Submit`, `Poll`, `MaterializeResult`, `DescribeProvenance`, `NeverPromoteDirectly`. Важно, чтобы adapter возвращал не “готовый ассет”, а только candidate package: относительный путь внутри quarantine root, declared media kind, provider-side run metadata, source recipe id, tool/model/version, timestamps, optional C2PA/EXIF/XMP findings и opaque provider job reference. Если провайдер ничего не вернул, вернул пустой файл, вернул файл другого формата, или вернул байты, не совпадающие с заявленным результатом, adapter обязан завершаться `candidateBlocked` и не создавать promotion path. Такая жёсткая граница напрямую следует из ваших документов, где provider output должен оставаться в quarantine до deterministic validation and human review, а prompt dump запрещён как evidence. citeturn3view0turn3view1

Для относительных путей и proof of locality в core достаточно BCL-подхода: входной путь сначала переводится в абсолютный через `Path.GetFullPath(path, basePath)`, затем проверяется на нахождение внутри разрешённого root, и только после этого нормализуется обратно в project-relative path через `Path.GetRelativePath`. Документация .NET отдельно предупреждает, что относительные пути опасны, если зависят от текущей директории процесса, а rooted path и fully qualified path — не одно и то же; поэтому валидация должна опираться на явно заданный base root, а не на process current directory. Для ссылок/junctions можно дополнительно использовать `ResolveLinkTarget`, но как защитное усиление, а не как единственный механизм. citeturn7search1turn7search8turn19search1turn19search2turn19search13turn22search0turn22search11

## Модель данных для candidate, quarantine и promotion

Для проекта лучше держать **две разные модели**, а не одну перегруженную. Первая — `MediaCandidateRecord`, editor-only. Вторая — `ApprovedAssetBinding`, runtime-facing. Это устраняет главную архитектурную ошибку, когда в runtime catalog случайно протаскиваются provider-specific fields, prompts, external URLs или review drafts. Подход полностью совместим с правилом репозитория о том, что runtime-facing references принадлежат GamePackage/manifests/catalogs, а visual recipes и rating metadata могут оставаться editor-side, пока не доказан отдельный consumer contract. citeturn3view1turn2view0

Рекомендуемая editor-only структура `MediaCandidateRecord` должна включать, как минимум, такие группы полей:

```json
{
  "candidateId": "cand/visual/....",
  "assetSlot": "portrait_safe | portrait_suggestive | adult_nude_reference | ...",
  "sourceRecipeId": "recipe/....",
  "sourceSpecRef": "catalog/....",
  "providerAdapterId": "adapter/comfyui | adapter/exifimport | adapter/procedural",
  "providerRunRef": "opaque external id",
  "originKind": "provider_generated | human_imported | procedural_compiled",
  "quarantineRelativePath": "quarantine/.../file.png",
  "declaredMediaKind": "image | video | audio | atlas | mask",
  "declaredExtension": ".png",
  "declaredMime": "image/png",
  "fileLengthBytes": 123456,
  "sha256": "....",
  "detectedMime": "image/png",
  "detectedExtension": ".png",
  "decodedFacts": {
    "width": 1024,
    "height": 1024
  },
  "provenance": {
    "tool": "....",
    "model": "....",
    "version": "....",
    "generatedWithAI": true,
    "c2paState": "missing | present_unverified | verified | invalid",
    "licenseAssertion": "unknown | reviewed_ok | restricted"
  },
  "policy": {
    "rating": "safe | suggestive | adult_nude_reference | adult_erotic_scene | adult_private_explicit",
    "adultEnabled": false,
    "safeFallbackRequired": true,
    "exportPolicy": "public_safe | mature_optional | adult_build_only | private_local_only | blocked"
  },
  "review": {
    "reviewStatus": "pending | approved | rejected | blocked_policy | superseded",
    "reviewer": null,
    "decisionReason": null
  },
  "promotion": {
    "approvedAssetRef": null,
    "promotedAtUtc": null
  },
  "promptEvidence": {
    "promptTemplateId": "optional reviewed template ref",
    "promptHash": "optional",
    "storeRawPrompt": false
  }
}
```

Эта структура отражает почти все явно названные поля из ваших roadmap/adult docs — `rating`, `adultEnabled`, `safeFallbackRequired`, `candidateQuarantine`, `reviewStatus`, `exportPolicy`, `assetSlot`, `approvedAssetRef`, provenance, relative path, byte/hash validation — и при этом сохраняет главный запрет: raw prompt text не становится source of truth. Для хеша и стримовой проверки достаточно стандартных .NET `SHA256.HashData(Stream)` и `FileStream`; это снижает число зависимостей в core. citeturn3view0turn3view1turn4view0turn7search2turn7search6turn7search3

Runtime-facing `ApprovedAssetBinding` должен быть уже предельно узким: `approvedAssetRef`, `assetSlot`, `contentRating`, `exportPolicy`, `runtimeRelativePath`, `fallbackApprovedAssetRef`, `reviewRevision`, `contentOriginSummary`, `generatedWithAI`, `publicSafeEligible`. Всё, что связано с provider job, raw metadata, hashes исходного candidate и review comments, остаётся в editor ledger. Это особенно важно для safe/public builds: export profile должен принимать решение только по reviewed binding и policy verdict, а не по внешним provider details. Steam и IARC смотрят на фактическое shipped content и disclosed content, а не на внутренние редакторские намерения. citeturn17view0turn15search2turn15search6

Для adult metadata boundary лучше сохранить ваши planning IDs как внутренние policy IDs, но не путать их с платформенными возрастными рейтингами. Внутренние поля `safe`, `suggestive`, `adult_nude_reference`, `adult_erotic_scene`, `adult_private_explicit` полезны для resolver/export logic; однако ESRB и PEGI оценивают итоговый контекст произведения, а не только asset-level label. ESRB AO 18+ описывает контент только для взрослых и допускает graphic sexual content, а PEGI 18 прямо относит explicit sexual activity к взрослой категории; IARC выдаёт рейтинги на основе questionnaire. Следовательно, в core нужно хранить **internal policy rating**, а в platform export adapters — строить **rating evidence packet**, который затем уже маппится на Steam/IARC/ESRB/PEGI workflows. citeturn15search8turn16search0turn15search2turn15search6

## Модель rating, export policy и deterministic fallback

Для проекта целесообразно развести три разных понятия, которые часто ошибочно смешивают: **content rating**, **export policy**, **build profile**. Content rating отвечает на вопрос “что это за тип контента”. Export policy — “куда это вообще можно выпускать”. Build profile — “какую именно витрину/сборку сейчас компилируем”. Это разделение прямо следует из ваших adult docs, где rating ids и export policies заданы разными словарями, а safe/public builds обязаны иметь deterministic fallbacks и fail closed при противоречивой metadata. citeturn4view0turn3view1

Рабочая таблица политики для LLMGameCreator должна выглядеть так:

| content rating | export policy | safe/public build | mature opt-in build | adult private build |
|---|---|---|---|---|
| `safe` | `public_safe` | включать | включать | включать |
| `suggestive` | `mature_optional` | по умолчанию заменять safe fallback либо исключать, в зависимости от storefront/profile | включать после review | включать |
| `adult_nude_reference` | `adult_build_only` | всегда исключать и заменять fallback | обычно исключать, если это не adult profile | включать только после review |
| `adult_erotic_scene` | `adult_build_only` | всегда исключать и заменять fallback | обычно исключать | включать только после review |
| `adult_private_explicit` | `private_local_only` | всегда исключать | всегда исключать | не шиппить по умолчанию; только local/private review pack |
| любое | `blocked` | исключать | исключать | исключать |

Эта таблица согласуется с вашими документами и с практикой платформенной модерации: safe/public build не должен полагаться на то, что запрещённый asset “просто не будет вызван”. Он должен физически не попадать в билд или быть заменён approved fallback ref. Иначе вы получаете одновременно и архитектурный риск, и риск платформенной несоответствующей disclosure. citeturn4view0turn3view1turn17view0

Детерминированный fallback здесь должен считаться обязательной compile-time сущностью. Если slot adult-capable, то у него есть либо `fallbackApprovedAssetRef`, либо validation failure. Никаких provider retries на runtime, никаких “если ассет отсутствует — дерни сеть”, никаких скрытых `prompt`-to-image веток. Для мира масштаба Heroes III / MM7-inspired pipeline это особенно важно: чем больше вариативных объектов, существ, наложений и состояний, тем дороже становится любой недетерминированный обход через внешнюю генерацию. Ваши visual docs прямо фиксируют идею универсального resolver “любой объект + стек влияний + seed → visual recipe → procedural output” и запрещают live runtime prompting. citeturn3view2turn3view1turn3view0

## Валидация и negative proof matrix

В этом pipeline нужно валидировать не только “позитивный успех”, но и **негативное доказательство**, то есть формально показывать, почему конкретный candidate не может быть promoted или exported. Иначе review ledger быстро превращается в неаудируемый список файлов. C2PA-спецификация полезна здесь как модель tamper-evident provenance: hard binding опирается на криптографические хеши, а guidance подчёркивает, что система должна подтверждать корректность и связь assertions с asset, но не выносить value judgment о “good/bad” или “истинности” контента. Для LLMGameCreator это означает: promotion должен опираться на собственную matrix of checks, а provenance служит доказательной частью, но не заменяет review. citeturn5search0turn18search14turn18search1

Ниже — минимальная матрица, которую имеет смысл прошить в validator suite и acceptance tests.

| код проверки | негативный кейс | ожидаемый результат |
|---|---|---|
| `MPV001` | provider сообщил success, но файла нет по `quarantineRelativePath` | `reviewStatus=blocked_policy`, promotion невозможен |
| `MPV002` | файл есть, но `fileLengthBytes` не совпадает с фактом | quarantine fail, пересчёт не лечит автоматически |
| `MPV003` | `sha256` не совпадает после materialization или повторного чтения | tamper fail, promotion невозможен |
| `MPV004` | declared MIME/extension не совпадают с signature/detected MIME | quarantine fail либо manual exception path |
| `MPV005` | декодирование файла не удалось | candidate остаётся blocked, approved ref не создаётся |
| `MPV006` | абсолютный путь, UNC, rooted path или выход за разрешённый root | path escape, reject |
| `MPV007` | symlink/junction уводит за artifact root | reject или manual-forensics path |
| `MPV008` | adult-capable slot без `safeFallbackRequired=true` и без fallback ref | reject |
| `MPV009` | adult rating при `adultEnabled=false` | reject |
| `MPV010` | `adult_*` + `public_safe` экспорт | reject |
| `MPV011` | `private_local_only` попадает в shipping export | reject и export fail closed |
| `MPV012` | candidate approved без review decision | reject |
| `MPV013` | prompt text используется как единственный источник классификации/происхождения | reject |
| `MPV014` | metadata говорит `safe`, но review помечает как adult-capable | reject до ручной коррекции policy |
| `MPV015` | `approvedAssetRef` указывает на unreviewed candidate file | reject |
| `MPV016` | safe/public export не может разрешить слот ни в approved safe ref, ни в fallback | build fail, не runtime fallback |
| `MPV017` | build содержит скрытый adult asset, даже если каталог его не ссылает | export fail, физическое исключение обязательно |
| `MPV018` | C2PA есть, но подпись invalid/manifest malformed | provenance invalid; не блокирует импорт автоматически, но запрещает повышать trust level |
| `MPV019` | C2PA/EXIF/XMP claims противоречат локальному ledger | локальный ledger имеет приоритет, candidate в review-needed |
| `MPV020` | provider adapter пытается писать прямо в approved asset area | hard architectural reject |

Эта матрица опирается на ваши внутренние stop conditions и validation expectations, на .NET path APIs, на C2PA hard-binding/tamper-evidence модель и на Steam disclosure rules. Особенно важны `MPV017` и `MPV020`: первый защищает от platform/compliance проблем, второй — от размывания quarantine boundary. citeturn3view1turn4view0turn4view1turn7search1turn19search2turn17view0turn5search0turn18search13

## Инструменты и адаптеры по статусам

**Можно внедрять сейчас.** Базовый core этого pipeline можно делать без внешних зависимостей: `System.IO.Path`, `Path.GetFullPath`, `Path.GetRelativePath`, `Path.IsPathRooted`, `SHA256.HashData(Stream)`, `FileStream`, `System.Text.Json`, плюс собственные validators и immutable review ledger. Это полностью совместимо с .NET 8, WinForms editor и с вашим требованием не тянуть внешнюю зависимость в core без adapter boundary. На этом же слое уже можно реализовать `candidate/quarantine/promotion` модель, export filters, deterministic fallback resolver, relative-path policy и reviewStatus/promotion decision flow. citeturn7search1turn7search0turn19search2turn7search6turn7search3turn3view0turn3view1

**Нужно прототипировать.**  
`C2PA Tool / c2pa-rs` — хороший кандидат для **optional provenance adapter**, но не для core. По официальным материалам CAI/C2PA доступны Rust SDK, C API и command-line tool; сам `c2pa-rs` находится в beta 0.x, лицензируется MIT/Apache-2.0, поддерживает создание/валидацию manifests и hard bindings. Для C#/.NET 8 это означает, что практичнее идти через CLI adapter или тонкую native boundary, а не вшивать Rust SDK в core доменную модель. Использовать стоит как **дополнительный provenance envelope**, а не как единственное доказательство легальности или удовлетворённости policy. citeturn20search0turn9view0turn5search5turn18search14

`MediaInfoLib / MediaInfoDLL` — хороший кандидат для **optional media inspection adapter** для видео/аудио. Репозиторий указывает BSD-2-Clause, проект зрелый, latest release в GitHub — 26.05 от 12 мая 2026, есть .NET wrapper packages для Windows. Для WinForms editor на Windows это практично; для Unity runtime это не нужно и туда не должно идти. Рекомендация: держать как внешнюю опцию для decode facts (`duration`, `fps`, `streams`, `codec summary`) в quarantine stage. citeturn9view3turn23search2turn23search11

`ExifTool` — очень зрелый и функционально мощный инструмент для EXIF/XMP/IPTC/C2PA-related metadata extraction; на официальном сайте он распространяется “на тех же условиях, что и Perl”, существует как platform-independent CLI, а Windows executable bundle включает Perl. Но GitHub-репозиторий помечен как GPL-3.0, тогда как официальный сайт и README говорят о dual Perl Artistic/GPL framing. Из-за этой лицензионной неоднозначности ExifTool лучше держать как **внешний CLI adapter**, который оператор сознательно включает, а не как dependency по умолчанию в core. С точки зрения зрелости это сильный кандидат, с точки зрения legal cleanliness — требует отдельной фиксации policy. citeturn12view0turn12view2turn14view0

`Magick.NET` — практичный optional adapter для image normalization, thumbnailing, probes и controlled transcodes. Репозиторий указывает Apache-2.0, NuGet-пакеты есть для `net8.0` и `netstandard2.0`, библиотека тестируется на Windows/Linux/macOS и поддерживает более 100 major formats. Ограничение не лицензионное, а архитектурное: это тяжёлая image stack с большой native surface, поэтому в core её тянуть не нужно; использовать стоит только за adapter boundary и только для editor-time operations. Дополнительно нужно помнить о документации Magick.NET по Ghostscript для некоторых форматов и коммерческому сценарию для Ghostscript. citeturn8search6turn9view2turn23search0turn8search14

**Отложить.**  
`ImageSharp` лучше не брать в первый slice. Формально библиотека split-licensed: Apache-2.0 либо Six Labors Commercial Use License, а коммерческое использование в closed-source софта при определённых условиях требует коммерческой лицензии. Для вашей задачи first slice это не даёт критического преимущества перед BCL + Magick.NET adapter/CLI tools, зато добавляет лицензионный контекст, который сейчас не нужен. citeturn8search7turn8search3turn8search15

**Не подходит как дефолтное решение.**  
`Mime-Detective` интересен технически и хорошо совместим с .NET 8, но в качестве дефолтной основы для production policy pipeline я бы его не выбирал. Причина не в качестве распознавания, а в лицензировании набора сигнатур: у ядра есть собственная MIT-based license с добавочным ограничением на derivative package distribution, а `Condensed`/`Exhaustive` definition packs производны от TrID signatures и прямо разделяют personal/non-commercial и paid commercial usage. Для LLMGameCreator такой dependency создаёт лишний legal surface. Если и использовать — то только default pack, только в optional adapter, и только после явной license note. Для core проще и безопаснее ограничиться BCL, whitelists по форматам и decode-based validation. citeturn11view0turn10view0turn23search1turn23search10

Безусловно не подходят для этой архитектуры любые **runtime provider calls**, **runtime LLM/RAG/media-provider integration**, **prompt text as source of truth**, **прямое попадание candidate files в Unity/StreamingAssets**, а также **live-generated adult sexual content during gameplay**. Последний пункт не только противоречит вашему проектному курсу, но и отдельно конфликтует с текущей позиционной документацией Steamworks. citeturn2view0turn3view1turn17view0

## Риски, legal notes и пределы provenance

Главный юридический риск здесь двойной. Во-первых, store/distribution risk: если adult-capable asset физически попал в shipped build, даже будучи якобы “неиспользуемым”, для платформенной проверки это уже disclosure issue. Во-вторых, provenance-overclaim risk: наличие Content Credentials или другого provenance marker не равно ни лицензии на asset, ни гарантии правомерности, ни доказательству “правды” о содержимом. Сама C2PA отдельно подчёркивает, что спецификация не должна выносить value judgment о том, “good” или “bad” provenance, а независимое исследование 2026 года показывает, что на C2PA нельзя преждевременно полагаться как на high-stakes perception of authenticity. Для LLMGameCreator это означает простое правило: **provenance усиливает review; provenance не заменяет review**. citeturn17view0turn18search14turn6search17

Отдельный compliance-фактор на 2026 год — transparency rules для AI-generated content. Европейская комиссия в июне 2026 опубликовала Code of Practice on Transparency of AI-Generated Content; obligations по Article 50 AI Act, применимые с 2 августа 2026, касаются marking/labelling AI-generated content, а service-desk формулировка прямо говорит о machine-readable marking и detectability synthetic outputs. Это не означает, что LLMGameCreator обязан немедленно внедрять C2PA как юридически обязательный механизм во всех случаях; но это означает, что хранить machine-readable origin/provenance fields и иметь optional cryptographic/metadata marking adapter — уже не “избыточная идея на будущее”, а разумная архитектурная страховка. citeturn21search1turn21search2turn21search11

Для свежего legal baseline по adult-capable контенту проекту стоит держать позицию максимально консервативной. Внутренний policy vocabulary должен быть уже, чем потенциально допустимый рынок: только adult, sapient, humanoid-compatible, no age ambiguity, no feral/non-sapient, no coercive framing, no hidden adult assets in safe/public exports. Это полностью совпадает с вашими adult strategy docs и хорошо согласуется с ESRB/PEGI/IARC логикой по возрастным ограничениям и disclosure. Архитектурно это снижает риск не только unsafe output, но и дрейфа предметной области в сторону pipeline, который невозможно будет верифицировать и экспортировать. citeturn4view0turn3view1turn16search0turn15search8turn15search2

## Последовательность Codex для внедрения

Для вашего репозитория лучшая последовательность внедрения почти полностью совпадает с существующим roadmap, но её стоит зафиксировать более операционно.

Сначала нужен **contract-only slice**: editor-only модели `MediaCandidateRecord`, `ApprovedAssetBinding`, `ExportBuildProfile`, `VisualAssetPolicy`, `MediaValidationReport`, `ReviewDecisionLedger`. На этом же шаге фиксируются vocabularies для `rating`, `exportPolicy`, `reviewStatus`, `promotionStatus`, negative proof codes, а также path/hash/byte validators на BCL. Этот шаг можно внедрять сразу. citeturn3view0turn3view1turn7search1turn7search6

Затем нужен **quarantine service slice**: materialization root, immutable ledger, recompute-and-verify hash pass, signature/decode checks, safe-fallback validator, export-profile resolver и tests по negative proof matrix. Здесь же следует запретить абсолютные пути, сетевые пути и прямую запись outside artifact root. Этот шаг тоже можно внедрять сразу, без внешних зависимостей. citeturn19search1turn19search2turn3view1turn4view0

После этого имеет смысл сделать **review/promotion slice**: WinForms workspace для candidate queue, provenance summary, rating/export badges, diff between candidate and approved replacement, reviewer decision logging и creation of `approvedAssetRef`. Промоция должна быть отдельным use-case в Application layer, который никогда не вызывается из provider adapter. citeturn3view0turn2view0

Только затем нужен **optional adapter slice**: сначала `C2PA Tool` adapter, затем `MediaInfo` adapter, затем при необходимости `ExifTool` и/или `Magick.NET`. Все эти интеграции должны быть выключаемыми, не менять core schema и не становиться обязательными для runtime/package consumption. Уместнее всего это оформить через optional infrastructure adapters и capability flags. citeturn20search0turn9view3turn12view0turn9view2

В самом конце — **export hardening slice**: физическое исключение blocked/adult/private assets из safe/public builds, deterministic fallback substitution report, rating evidence packet для target storefronts, и тест “если скрытый adult asset лежит в build, export падает”. С учётом Steam disclosure и AI content survey именно этот шаг закрывает не только архитектуру, но и реальную операционную пригодность пайплайна. citeturn17view0turn15search2turn15search6

Итоговая рекомендация проста: **core сейчас — BCL-only, policy-first, quarantine-first, review-first; adapters — потом, строго optional; runtime calls to providers — никогда**. Это наилучшим образом совпадает и с текущим устройством LLMGameCreator, и с внешней регуляторной и платформенной средой середины 2026 года. citeturn2view0turn3view0turn3view1turn17view0turn21search1