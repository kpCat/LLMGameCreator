# Final report format

Report exactly:

```text
Final status: GREEN / BLOCKED / FAILED
Latest commit before work:
Latest commit after work:
Pushed to origin/main: yes/no
Preflight:
- branch:
- latest origin/main before work:
- adult docs commit present above Goal082: yes/no
- Goal082 implementation commits present: yes/no
- Goal082 accepted remains false: yes/no

Physical source formatting repair:
- malformed .cs files before:
- malformed .cs files after:
- zero-LF count before/after:
- CR-only count before/after:
- raw physical one-line source count before/after:
- raw physical max line before/after:
- files reformatted:

Scanner/guard changes:
- raw-byte scan scope:
- synthetic CR-only rejected: yes/no
- synthetic zero-LF one-line rejected: yes/no
- Unity probe included in scan: yes/no
- WinForms parent included in scan: yes/no
- Goal082 Application files included in scan: yes/no

Evidence:
- Goal082 report hash before/after:
- Goal082A report hash:
- AlphaRuntimeBootstrap hash before/after:
- Goal082 accepted remains false: yes/no

Validation:
- restore:
- build:
- Goal082 focused tests:
- Goal082 product smoke:
- CurrentState:
- check-all:
- artifact scope:
- git diff --check:
- git diff --cached --check:
- mojibake scan:

Forbidden areas touched:
Remaining debt:
Final git status:
Git commands used and why:
```
