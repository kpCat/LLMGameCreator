# VisualRuleStack and Domain Profiles

## Статус

Proposal.

Этот документ описывает, как универсализировать визуальную генерацию для разных миров, лоров, доменов, культур, фракций, религий, биомов и типов поселений.

## Проблема

Нельзя делать прямое правило:

```text
domain/necropolis → все здания выглядят как склепы нежити
```

Это ломает лор и делает мир карикатурным.

На территории Некрополя могут жить обычные люди, фанатики, чиновники, жрецы, военные, нежить, торговцы, иностранцы, бедные крестьяне и дворцовая элита. Их дома, районы, храмы, форты и дворцы должны отличаться.

## Решение

Каждый объект получает стек влияний:

```text
WorldStyle
+ DomainInfluence
+ BiomeAndResources
+ PoliticalControl
+ PopulationCulture
+ ReligionOrIdeology
+ SettlementTier
+ DistrictRole
+ ObjectFunction
+ OccupantProfile
+ WealthClass
+ Condition
+ HistoryLayer
+ SpecialInfluence
+ GameplayConstraints
+ Seed
```

Этот стек называется `VisualRuleStack`.

## Пример VisualRuleStack

```json
{
  "visualRuleStack": [
    { "source": "world/dark_fantasy", "weight": 1.0 },
    { "source": "domain/necropolis", "weight": 0.65 },
    { "source": "biome/swamp", "weight": 0.8 },
    { "source": "settlement/village", "weight": 0.7 },
    { "source": "population/living_humans", "weight": 0.85 },
    { "source": "religion/ancestor_cult", "weight": 0.45 },
    { "source": "building_role/dwelling", "weight": 1.0 },
    { "source": "wealth/poor", "weight": 0.9 },
    { "source": "condition/damaged", "weight": 0.6 }
  ]
}
```

## Домен — не скин

Домен — это поле влияния.

Например, `domain/necropolis` может добавлять:

- официальные символы;
- похоронные мотивы;
- ритуальные знаки;
- власть жрецов/некромантов;
- запреты на яркие солнечные мотивы;
- склонность к тёмному камню, кости, зелёному свечению;
- cemetery/crypt/ancestor motifs;
- особую атмосферу и историю.

Но бытовое жилище живого бедного человека остаётся бытовым жилищем.

## DomainProfile

```json
{
  "domainId": "domain/necropolis",
  "displayName": "Некрополь",
  "coreIdeas": [
    "death_as_order",
    "ancestor_authority",
    "ritual_law",
    "controlled_decay"
  ],
  "visualDoctrine": {
    "shapeLanguage": {
      "official": ["crypt_arches", "heavy_gates", "vertical_spires"],
      "common": ["low_houses", "small_altars", "dark_roofs"],
      "military": ["battlements", "bone_standards", "iron_grilles"]
    },
    "materials": {
      "common": ["dark_wood", "local_stone", "mossy_thatch"],
      "official": ["black_stone", "polished_bone", "oxidized_iron"],
      "poor": ["rotten_wood", "mud_brick", "reused_grave_stone"]
    },
    "motifs": {
      "public": ["ancestral_tablets", "funerary_runes", "green_lanterns"],
      "private": ["small_shrines", "protective_marks"],
      "forbidden": ["bright_flower_festival", "solar_gold_clean"]
    },
    "palette": {
      "dominant": ["black_gray", "old_bone", "wet_dark_green"],
      "accent": ["sickly_green", "cold_blue"]
    },
    "weights": {
      "death_motif": 0.75,
      "decay": 0.55,
      "symmetry": 0.35,
      "religiosity": 0.8,
      "militarization": 0.45
    }
  }
}
```

## PopulationProfile

Поселение в домене должно хранить состав населения.

```json
{
  "settlementId": "settlement/black_reed_village",
  "domainId": "domain/necropolis",
  "settlementTier": "village",
  "populationProfile": {
    "living_humans": 0.82,
    "civil_undead": 0.08,
    "necromancer_clergy": 0.04,
    "military_undead": 0.06
  },
  "religionProfile": {
    "ancestor_cult": 0.7,
    "state_necromancy": 0.4,
    "local_folk_beliefs": 0.45
  },
  "wealth": "poor",
  "dominantOccupation": ["reed_cutting", "grave_tending", "fishing"],
  "localBiome": "swamp",
  "controlLevel": "medium"
}
```

Это значит:

```text
Это не город нежити.
Это деревня живых людей под властью Некрополя.
Дома должны быть человеческие, бедные, болотные,
но с некропольными символами, ритуалами и страхом власти.
```

## Settlement tiers

Генератор должен различать типы поселений:

- hamlet;
- village;
- town;
- city;
- palace_city;
- fortress;
- monastery;
- necropolis_city;
- industrial_outpost;
- space_station;
- orbital_city;
- mining_colony.

Один и тот же домен в разных settlement tiers выглядит по-разному.

## Districts

Большие поселения должны делиться на районы:

- residential_common;
- residential_elite;
- market;
- temple_district;
- military_district;
- craft_district;
- grave_district;
- palace_district;
- foreign_quarter;
- slums;
- industrial;
- port;
- academy;
- forbidden_zone.

Каждый district имеет свои influence weights.

## HistoryLayer

История должна влиять на визуал:

- поселение было захвачено другим доменом;
- поселение построено на руинах;
- здесь была война;
- здесь была эпидемия;
- здесь произошёл метамодульный разрыв;
- здесь поселились беженцы;
- здесь недавно усилилась власть религии;
- район был перестроен богатыми семьями.

Пример:

```text
Человеческий город, захваченный Некрополем:
старая человеческая архитектура
+ новые похоронные знаки
+ переделанные храмы
+ гарнизонные башни
+ кладбищенские расширения
+ следы оккупации.
```

## Blend profiles

На границах доменов нужны правила смешения.

```json
{
  "blendProfileId": "blend/necropolis_green_frontier",
  "primary": "domain/necropolis",
  "secondary": "domain/green_limit",
  "rules": [
    "bone motifs become root-and-bone hybrids",
    "green soul glow mixes with natural bioluminescence",
    "crypt stone is overgrown by moss and vines",
    "human houses keep practical shapes but use funerary symbols"
  ],
  "weights": {
    "necropolis": 0.6,
    "green_limit": 0.4
  }
}
```

## SpecialInfluence / MetaModule

Метамодули должны влиять на visual rules как modifiers.

```json
{
  "activeMetaModuleInfluence": {
    "moduleId": "metamodule/entropy",
    "strength": 0.45,
    "visualEffects": {
      "increaseTags": ["condition/decayed", "motif/crack", "palette/ash_gray"],
      "decreaseTags": ["condition/pristine", "shape/symmetric"],
      "addSurfaceDecals": ["decal/entropy_fracture"],
      "addAtmosphere": ["fog/gray_ash"]
    }
  }
}
```

Это позволяет gameplay/lore системам менять внешний вид мира.

## Универсальность

Механизм должен работать для любого мира:

- “Носитель метамодулей”;
- техно-будущее;
- космическая станция;
- постапокалипсис;
- фэнтези-королевство;
- город магов;
- подземная цивилизация;
- биотехнологическая цивилизация;
- империя машин.

Для другого мира меняются profiles/vocabularies/rules, но не сам resolver.

## Anti-patterns

Запрещённые направления:

- `domain == final visual style`;
- один генератор для одного лора;
- LLM-вызов на каждый объект;
- готовый список из 50 домов;
- жёсткое зашивание культуры в тип объекта;
- отсутствие social/population/district layers;
- отсутствие forbidden combinations;
- отсутствие gameplay readability constraints.

## Минимальная реализация

Для первого этапа достаточно:

- `VisualRuleSource`;
- `VisualRuleStack`;
- `DomainVisualProfile`;
- `BiomeVisualProfile`;
- `SettlementTierProfile`;
- `PopulationProfile`;
- `ObjectRoleProfile`;
- `VisualRecipe`;
- `VisualGrammarResolver`;
- deterministic fixture profiles;
- validators.

Реальная графика может быть вторым этапом.
