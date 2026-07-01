# External scouting — Goal 072 Generator Spine Quality Consolidation And Risk Audit

## Decision

No new external dependencies are required for Goal 072.

This goal is not a feature/runtime/library adoption slice. It is a quality-consolidation and proof-hardening slice over the existing repository. The correct default is BCL-only analysis, local repository scans, existing test infrastructure, existing artifact-scope guard, existing Unity Alpha proof routes, and deterministic compact evidence.

## Considered tools/libraries

### Roslyn analyzers / custom analyzers

Potentially useful later for permanent complexity and proof-quality rules. Not adopted in Goal 072 because adding analyzer packages or build integration would require `.csproj` / build-pipeline changes and could introduce broad churn.

### NDepend / Sonar / third-party quality dashboards

Useful as external human tools, but not appropriate as repository dependencies. Visual Studio code metrics already revealed risk. Goal 072 should convert the relevant red flags into repository-local deterministic checks/evidence.

### dotnet format

Useful later, but not required for this goal. It can create broad formatting churn. Goal 072 should perform bounded formatting/line-ending fixes only when a concrete P0/P1 issue is detected.

### Source generators / automated refactoring frameworks

Not appropriate. Goal 072 must not perform wide mechanical rewrites or create a new meta-framework.

## Required stance

Use BCL-only repo scanners and tests. If a useful permanent checker is added, it must be small, deterministic, and local to the current solution/test flow. Do not add packages, analyzers, or broad build changes.
