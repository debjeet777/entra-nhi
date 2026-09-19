# Decision 0006 — Normalized Identity Reference Implementation Mechanics

**Status:** Accepted for Phase 0.4 implementation

## Context

Decision 0005 locks the Stage-2 semantic contract for normalized identity
references but intentionally defers the concrete C# representation.

Stage 2 now requires the minimum provider-independent implementation mechanics
for:

- the opaque assessment-local normalized identity key; and
- the normalized identity reference composed of explicit tenant context plus
  that key.

This decision chooses only the C# mechanics necessary to implement those
locked semantics. It does not resolve any identifier format, generation,
provider mapping, serialization, graph, provenance, or Stage-3 semantics.

## Decision

The Stage-2 implementation will use two sealed Core record reference types.

### `NormalizedIdentityKey`

`NormalizedIdentityKey` will:

- be a `sealed record`;
- accept one `string value` through its constructor;
- reject null, empty, and whitespace-only values;
- expose the accepted value through a read-only `Value` property;
- preserve the value exactly without parsing or provider interpretation.

The type defines value semantics only. It does not define the format,
generation strategy, provider origin, serialization, or persistence of the key.

### `NormalizedIdentityReference`

`NormalizedIdentityReference` will:

- be a `sealed record`;
- require a `TenantContext`;
- require a `NormalizedIdentityKey`;
- reject null components;
- expose both components through read-only properties;
- use record value equality over tenant context plus normalized identity key.

Therefore, identical normalized key values under different tenant contexts
produce unequal normalized identity references.

## Required tests

Implementation tests MUST prove:

- `NormalizedIdentityKey` preserves a valid opaque value;
- null, empty, and whitespace-only key values are rejected;
- equal key values have equal value semantics;
- different key values have unequal value semantics;
- `NormalizedIdentityReference` exposes its tenant context and key;
- equal tenant context plus equal key produces equal references;
- identical key values under different tenant contexts produce unequal references;
- null tenant context is rejected;
- null normalized identity key is rejected;
- the canonical `AssessmentState` vocabulary remains exactly five values;
- Core dependency isolation remains intact.

All test data MUST be synthetic.

## Explicitly deferred

This decision does NOT define:

- normalized identity key format or syntax;
- key-generation strategy;
- provider/native identifier mapping;
- GUID or Microsoft Entra identifier requirements;
- source-reference representation;
- serialization or wire format;
- persistence;
- `IdentityRecord` or `IdentityKind`;
- graph node or edge contracts;
- deduplication or conflict-resolution algorithms;
- capability/completeness semantics;
- operational failure semantics;
- evidence/provenance schemas.

## Acceptance condition

This decision may move from **Proposed** to **Accepted** only if review confirms
that the selected C# mechanics implement Decision 0005 without resolving its
explicitly deferred concerns or introducing provider-specific dependencies.