# Goal155A current-package-correlated RC record truth hotfix

Status: GREEN

- The P1 found at `7084244a` is closed: CURRENT requires current package bytes, document build identity, typed identity and authoring fingerprint.
- Tampered or missing current package rejects the record; valid older evidence remains LAST_SUCCESS; missing current truth is UNKNOWN.
- Goal155A: 20/20 behavioral tests; Goal155 and required Goal154D/153C/150/149 regressions are GREEN.
- Controller cannot expose a ready RC after package tamper; portable and history-independent records remain CURRENT.
- Unity starts: 0. Standalone smoke invocations: 0. Goal154 acceptance is unchanged; Goal155 and Goal155A remain accepted=false with no human gate.
