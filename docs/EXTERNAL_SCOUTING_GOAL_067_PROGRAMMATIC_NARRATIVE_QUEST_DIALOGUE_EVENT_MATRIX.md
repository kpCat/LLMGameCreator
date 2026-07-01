# External scouting — Goal 067 Programmatic Narrative / Quest / Dialogue / Event Matrix

## Decision

Do not add external dependencies in Goal 067.

Goal 067 must prove a deterministic, programmatic narrative/quest/dialogue/event realization layer over the existing generated campaign evidence. It must not introduce a writer-facing narrative scripting runtime, external dialogue runtime, or LLM-generated final prose.

## Checked options

### Yarn Spinner / YarnSpinner-Unity

Yarn Spinner is a mature open-source dialogue system. It is useful as a future optional export/import adapter because it represents dialogue lines, options and commands in a writer-friendly language and has Unity integration.

Why not now:

- Goal 067 needs a deterministic domain proof, not a new runtime/dialogue dependency.
- Using Yarn now would pull the project toward writer-authored script files before the generator has its own stable narrative IR.
- Yarn is useful later as an export target after we define our own contract-bound dialogue/event/quest IR.

Possible future use:

- Export generated dialogue graphs into Yarn files.
- Import hand-authored Yarn fragments into quarantined candidates.
- Validate generated option/command coverage against generated state deltas.

### ink / ink-Unity integration

ink is also a strong candidate for future interactive narrative export/import. It supports branching narrative and has Unity integration.

Why not now:

- Goal 067 must not become an ink runtime integration goal.
- Branching narrative syntax is not the same as our generator contract.
- We need state deltas, provenance, replay and causal diagnostics before adopting a narrative language adapter.

Possible future use:

- Export accepted narrative graphs into ink as an optional adapter.
- Import ink branches as quarantined candidates with provenance and validation.
- Provide authoring interoperability for writers later.

### Twine / InkJS / other narrative tooling

Useful later as external formats, not as core.

## Chosen approach

BCL-only in-house application-layer contract:

- narrative row records;
- quest stage graphs;
- dialogue option graphs;
- event trigger/consequence chains;
- localization-key/template-slot tables;
- memory/rumor propagation records;
- runtime state delta proof;
- save/load/replay proof;
- Unity marker proof.

## LLM policy

LLM must not write final dialogue/prose in this goal.

Allowed:

- lore intake candidates in future;
- abstract quest/dialogue/event intent proposals in future;
- repair suggestions in future.

Not allowed:

- final prose;
- unbounded generated dialogue text;
- provider/LLM/RAG calls;
- runtime LLM.

Goal 067 output should be localization-ready and template-bound:

- `lineKey`;
- `templateId`;
- `speakerRole`;
- `toneTags`;
- `slots`;
- `conditions`;
- `optionEffects`;
- `stateDeltaRefs`.

## License/provenance notes

Yarn Spinner and ink can be considered later after fresh license/trademark/package review. Do not add them now.
