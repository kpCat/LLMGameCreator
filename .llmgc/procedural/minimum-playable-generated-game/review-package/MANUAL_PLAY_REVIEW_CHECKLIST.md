# Manual Play Review Checklist

Launch command:

```powershell
.\RUN_MANUAL_PLAY.ps1
```

Expected first screen: LLMGameCreator Alpha scenario for frontier_survival with readable quest and controls panels.
Expected quest panel: Quest 000.
Expected objective checklist:

- Start generated quest
- Open generated dialogue
- Select generated dialogue choice
- Obtain generated item
- Apply generated event
- Complete quest and grant reward

Movement controls: WASD/arrows.
Focus/select controls: Tab to focus, Space/Enter to interact.
Expected inventory/reward panel: Reward: item 004, reward granted after completion.
Expected event/status log: quest started, dialogue opened, choice selected, item obtained, event applied, reward granted.
Expected completion state: quest complete and reward granted.

Review boxes:

- [ ] Player launched from review package.
- [ ] First screen was readable.
- [ ] Movement worked.
- [ ] Focus/select worked.
- [ ] Objective checklist was understandable.
- [ ] Quest completed.
- [ ] Reward appeared.
- [ ] Event/status log was understandable.
- [ ] Known Alpha limitations are acceptable for this gate.
- [ ] minimum_playable_generated_game_verification can be marked passed in a later user review.
