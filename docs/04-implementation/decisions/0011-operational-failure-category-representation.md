# Decision 0011 — Operational Failure Category Representation

- **Status:** Accepted for Phase 0.4 implementation
- **Phase:** 0.4 — Secure Core Implementation
- **Stage:** 3 — Capability, Completeness, and Error Semantics

## Context

The locked implementation design defines fourteen conceptual operational
failure categories, F-01 through F-14.

The categories are normalized concepts. They are not provider exception
classes, SDK types, assessment states, capability states, completeness
states, findings, or programmer/invariant violations.

Decision 0010 established the operational-failure semantic boundary while
deliberately deferring concrete Result/Error shapes and code mechanics.

Stage 3 nevertheless requires the shared failure taxonomy to become
representable before downstream implementation can depend on it.

This decision therefore selects only the minimum Core representation of the
already-established taxonomy.

It does not design an operational result envelope.

## Decision

### 1. Core owns the normalized failure-category vocabulary

The normalized F-01 through F-14 conceptual taxonomy belongs to
`EntraNHI.Core`.

Core MUST remain independent of provider exception classes, SDK types,
network libraries, authentication implementations, rendering concerns, and
secret-bearing data.

### 2. Representation

The fourteen conceptual categories SHALL be represented by one closed Core
enum named:

`OperationalFailureCategory`

The enum is a semantic category vocabulary only.

It is not:

- an operational failure object;
- a `Result<T>` implementation;
- an assessment outcome;
- a capability observation;
- a completeness descriptor;
- a diagnostic record;
- an exception hierarchy; or
- a provider-error representation.

### 3. Canonical members

`OperationalFailureCategory` SHALL contain exactly these fourteen members,
in F-01 through F-14 conceptual order:

1. `AuthorizationFailure`
2. `AuthenticationBoundaryFailure`
3. `ProviderServiceUnavailable`
4. `ThrottlingRateLimitation`
5. `Timeout`
6. `NetworkTransportFailure`
7. `MalformedInvalidProviderResponse`
8. `NormalizationFailure`
9. `PartialCollectionFailure`
10. `ConfigurationFailure`
11. `UnsupportedCapabilityOperation`
12. `SerializationOutputFailure`
13. `Cancellation`
14. `InternalUnclassifiedOperationalFailure`

These member names are implementation identifiers for the already-fixed
conceptual taxonomy. They do not change the normative meanings defined by
`error-result-model.md`.

### 4. No numeric or wire-format semantics

The enum's underlying numeric values MUST NOT be treated as:

- F-01 through F-14 external codes;
- stable serialized values;
- persistence identifiers;
- protocol values;
- output-schema values; or
- compatibility guarantees.

This decision does not select explicit numeric assignments.

The external/stable code catalog and serialization representation remain
deferred.

### 5. Unknown cause is not a fifteenth category

No `Unknown`, `Other`, `None`, `Success`, or equivalent fifteenth enum
member is introduced.

Where the true cause cannot safely be established, the locked taxonomy
requires an explicit indeterminate-cause form of the nearest defensible
category or F-14 rather than a guessed category.

The concrete mechanism for carrying cause certainty/indeterminacy remains
deferred to the future operational-failure representation.

### 6. F-14 is not an invariant-violation bucket

`InternalUnclassifiedOperationalFailure` represents operational faults that
do not fit F-01 through F-13 and are not programmer/invariant violations.

Programmer errors and invariant violations remain a separate conceptual
layer and MUST NOT be normalized into F-14 merely to avoid failing visibly.

### 7. No universal state mapping

No `OperationalFailureCategory` member universally maps to an
`AssessmentState`.

In particular, this representation MUST NOT encode a direct enum-to-enum
conversion to:

- `PASS`;
- `FAIL`;
- `NOT_EVALUATED`;
- `NOT_APPLICABLE`; or
- `ERROR`.

Scoped gaps trend toward `NOT_EVALUATED` and trust-breaking failures toward
`ERROR` only through later owning contracts and deterministic context.

No category may silently produce `PASS` or tenant `FAIL`.

### 8. Capability and completeness remain separate

`OperationalFailureCategory` does not replace `CapabilityState` or
`ObservationCompleteness`.

For example, F-11 is the operational face of an unsupported operation but
does not collapse the separate `CapabilityState.Unsupported` concept into
the operational taxonomy.

Likewise, F-09 does not itself prove a specific completeness value without
the relevant observation-scope semantics.

### 9. Provider translation remains outside Core

Provider-specific signals may later be translated into this normalized
category vocabulary at the provider/adapter boundary.

This decision does not select:

- Microsoft Graph exception mappings;
- HTTP-status mappings;
- SDK exception mappings;
- permission mappings;
- licensing mappings;
- network-library mappings; or
- provider retry behavior.

Where a provider cause cannot safely be classified, implementations MUST
not guess.

### 10. Secret exclusion

The category enum carries no diagnostic text, payload, token, credential,
header, URI, provider object, or secret-bearing value.

Any future failure/diagnostic representation carrying contextual
information requires its own reviewed contract and sanitization rules.

## Explicitly deferred

This decision deliberately does NOT select:

- `Result<T>` or equivalent result representation;
- `OperationalFailure` class/record shape;
- `OperationalError` class/record shape;
- diagnostic class/record shape;
- failure-cause certainty representation;
- stable external F-01 through F-14 code strings;
- enum numeric values as external identifiers;
- serialization or persistence representation;
- provider-exception mappings;
- HTTP/Graph/SDK mappings;
- category-to-assessment-state mapping table;
- fatal-versus-isolated engine policy;
- cancellation propagation mechanism;
- retry/backoff/timeout constants;
- tenant-bearing failure envelope;
- correlation/provenance identifiers;
- logging/telemetry schema;
- output mappings; or
- CLI exit codes.

## Security consequences

The representation provides a closed, provider-independent vocabulary while
preserving:

- failure transparency;
- false-PASS resistance;
- false-FAIL resistance;
- provider isolation;
- secret exclusion;
- capability/completeness separation;
- programmer/invariant-violation separation; and
- deterministic downstream classification.

The closed enum MUST NOT be interpreted as proof that every provider signal
can always be classified precisely. Indeterminate cause remains explicit
through later failure representation rather than through guessed categories.

## Implementation authorization

While this decision remains **Proposed**, it authorizes no C# implementation.

If accepted after independent review, implementation authorization is
limited to:

1. one Core enum named `OperationalFailureCategory`;
2. exactly the fourteen members defined in this decision; and
3. narrow Core tests proving exact vocabulary and semantic separation.

Acceptance would NOT authorize any result/error/diagnostic envelope,
provider mapping, assessment-state mapping, serialization contract, retry
policy, cancellation mechanism, or other failure infrastructure.

## Review gate

Before this decision may become Accepted, review MUST confirm:

1. all fourteen members correspond one-to-one with F-01 through F-14;
2. no fifteenth category is introduced;
3. F-14 excludes programmer/invariant violations;
4. no universal category-to-assessment-state mapping exists;
5. no provider-specific type or mapping enters Core;
6. no secret-bearing field is introduced;
7. capability and completeness remain separate;
8. no wire/numeric compatibility semantics are invented;
9. concrete Result/Error/diagnostic shapes remain deferred; and
10. the decision does not modify the normative taxonomy meanings.
