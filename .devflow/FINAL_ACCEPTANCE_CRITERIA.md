# FINAL_ACCEPTANCE_CRITERIA.md — что значит “финалочка” для LLMGameCreator

Этот файл задаёт high-level acceptance. Он не разрешает перескакивать через текущие gates.

## Product-level final target

LLMGameCreator считается готовым к практической v1, когда пользователь может пройти путь:

```text
Capability selection
 -> strict artifact generation/evaluation
 -> artifact review/approval
 -> deterministic package assembly
 -> package validation
 -> runtime smoke/simulation
 -> diagnostics bundle/report
 -> exportable GamePackage
```

Без прямого runtime LLM, без direct UI JSON mutation и без принятия LLM output без validation/review/apply boundary.

## Mandatory final properties

```text
[ ] GamePackage остаётся source of truth.
[ ] Runtime работает headless and deterministic.
[ ] Runtime does not call LLM/provider/WinForms/ComfyUI/Fooocus.
[ ] LLM generation is editor-side only.
[ ] LLM outputs are drafts/artifacts until validated and approved.
[ ] Lua, if enabled, is sandboxed and deterministic.
[ ] Package assembly rejects invalid references.
[ ] Runtime smoke validates playability beyond JSON validity.
[ ] Diagnostics bundle allows external analysis of failures.
[ ] Tests cover core contracts, parser/repair, validators, assembly, runtime smoke.
```

## Minimum final verification

Final acceptance requires:

```text
1. check-all passed.
2. strict LLM evaluation report reviewed.
3. parser/repair corpus tests pass.
4. diagnostics bundle export test passes.
5. package assembly fixture test passes.
6. package validation passes for at least one generated/assembled package.
7. runtime smoke scenario passes: load/start/wait/serialize/deserialize and at least one meaningful interaction path.
8. no unexpected warnings.
9. no hidden stop conditions.
```

## Game family acceptance

At least one complete family must be proven before claiming v1:

```text
- selected capability bundle;
- generated or fixture-equivalent artifact set;
- approved artifact set;
- assembled GamePackage;
- validation report;
- runtime smoke report;
- diagnostics bundle.
```

Future “full breadth” target: at least three distinct families sharing the same lifecycle. Do not claim this until M9-level work is explicitly complete.

## What is not final

Not enough:

```text
- one lucky LLM generation;
- docs-only plan;
- build passes without runtime smoke;
- package JSON validates but game cannot be smoke-played;
- artifact review exists but assembly is narrow/template-only;
- UI exists but workflow cannot be completed end-to-end.
```
