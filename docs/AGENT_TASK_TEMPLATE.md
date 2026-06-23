# Agent Task Template

## Task type

Large but bounded backend slice / cleanup / hardening / review.

## Executor decision

Use Kilo Code first / Use Codex first.

Reason:
```text
...
```

## Git policy

```text
Work in the repository as it is currently checked out.
Do not create branches.
Do not switch branches.
Do not merge.
Do not rebase.
Do not cherry-pick.
Do not push.
Do not run git commands.
Branch management is handled manually by the user.
```

## Windows path policy

```text
Use repository-relative paths.
Use PowerShell commands.
Do not use /mnt, /home/oai, sandbox:/..., C:\mnt, or any ChatGPT/container path.
Do not read or reference paths outside the repository unless explicitly listed.
```

## Goal

...

## Read first

```text
...
```

## Allowed files

```text
...
```

## Forbidden files

```text
...
```

## Required behavior

...

## Non-goals

...

## Tests

...

## Required checks

```powershell
...
```

## Stop conditions

...

## Final report

```text
files read
files changed
tests/checks
recommendation
```
