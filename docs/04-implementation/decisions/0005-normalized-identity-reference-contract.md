# Decision 0005 — Normalized Identity Reference Contract

**Status:** Accepted for Phase 0.4 implementation

## Context

Phase 0.4 Stage 2 establishes the stable inward Core vocabulary required by
later stages. The locked implementation design requires normalized domain
references, explicit tenant assessment context, identifier/value concepts, and
the canonical five-state evaluation contract.

The domain design distinguishes an assessment-local normalized identity key
from a source reference:

- the normalized identity key identifies the canonical assessment-local
  identity record and is the equality basis used by later graph and rule
  consumers;
- a source reference identifies an external/source object for provenance and
  is not the normalized record identity;
- display names are metadata and MUST NOT establish identity or equality;
- identity equality is valid only inside the same tenant assessment context;
- records from different tenant contexts MUST NOT silently compare equal or
  merge merely because identifier text happens to coincide.

Downstream findings/evidence contracts also require subject references to be
tenant-scoped and traceable to normalized identity keys.

The locked design explicitly defers identifier formats, key-generation
strategy, source-reference format, serialization, fingerprinting, hashing,
deduplication algorithms, and conflict-resolution mechanics. This decision
therefore establishes only the minimum Stage-2 semantic contract and does not
resolve those deferred concerns.

## Decision

Stage 2 will represent a normalized identity reference as a provider-independent
Core concept composed of exactly these semantic elements:

1. an explicit `TenantContext`; and
2. an opaque assessment-local normalized identity key.

The assessment-local key will be represented by a dedicated Core value concept
rather than by an unqualified raw string throughout consuming contracts.

The normalized identity reference is therefore conceptually:

```text
NormalizedIdentityReference
    TenantContext
    + opaque assessment-local identity key
```

This conceptual notation defines semantic composition only. It does not define
a serialization or wire format.

### Tenant scoping

Tenant context is part of identity-reference semantics, not ambient execution
state.

Two normalized identity references from different tenant contexts are distinct
even if their opaque assessment-local key values contain identical text.

A consumer MUST NOT discard tenant context when carrying, comparing, or handing
off a normalized identity reference.

### Assessment-local key semantics

The assessment-local key:

- identifies a normalized identity record inside one tenant assessment
  context;
- is opaque to Core consumers;
- MUST NOT derive identity equality from display names;
- MUST NOT expose or encode secret material;
- MUST NOT require a provider SDK/API type;
- MUST NOT imply a Microsoft Entra object-ID, application-ID, GUID, URI, or
  other provider-native identifier format;
- MUST NOT itself establish cross-tenant identity.

Core consumers compare or carry the normalized key according to its value
semantics but do not parse provider meaning from it.

### Source-reference separation

A normalized identity reference is not a provenance/source reference.

Provider/source-native references remain a separate concept owned by the
normalization/provenance contracts. They MUST NOT be silently substituted for
the normalized assessment-local identity reference merely because both may
eventually contain identifier-like values.

This decision does not define the source-reference representation.

### Provider independence

Neither the normalized identity reference nor its assessment-local key may
depend on:

- Microsoft Graph SDK types;
- HTTP/provider payload types;
- Graph endpoints;
- provider permission names;
- undocumented Microsoft properties or relationships;
- authentication/token material;
- filesystem or renderer types.

### Secret exclusion

Neither component may contain credential secrets, access tokens, refresh
tokens, client secrets, passwords, private-key material, authorization headers,
or other secret-bearing material.

An identifier-like value is not automatically safe merely because it is
represented as text. Future producer boundaries remain responsible for
supplying only approved non-secret identity-key material.

## Stage-2 implementation constraints

The concrete Stage-2 implementation MUST preserve the following properties:

1. tenant context is explicit and structurally carried with the normalized
   identity reference;
2. the assessment-local key is a dedicated value concept;
3. an empty or whitespace-only key is not a valid normalized identity key;
4. the key value is preserved opaquely without provider interpretation;
5. normalized identity references have deterministic value semantics over
   tenant context plus assessment-local key;
6. same key text under different tenant contexts does not produce equal
   normalized identity references;
7. display names cannot participate in identity equality;
8. no public construction path permits a reference without tenant context or
   without a valid assessment-local key;
9. no sixth `AssessmentState` is introduced;
10. Core remains provider-, network-, secret-, filesystem-, authentication-,
    and renderer-independent.

## Required Stage-2 tests

At minimum, implementation tests MUST prove:

1. an opaque assessment-local key value is preserved;
2. empty key values are rejected;
3. whitespace-only key values are rejected;
4. equal tenant context plus equal key produces equal normalized references;
5. equal key text under different tenant contexts produces unequal normalized
   references;
6. different key values under the same tenant context produce unequal
   normalized references;
7. tenant context is directly recoverable from the normalized reference;
8. key is directly recoverable from the normalized reference;
9. provider interpretation is not performed;
10. the canonical `AssessmentState` vocabulary remains exactly five values;
11. Core dependency-isolation tests continue to pass.

All test data MUST be synthetic.

## Explicitly deferred

This decision does NOT choose or define:

- assessment-local key textual format;
- key-generation strategy;
- provider/native identifier mapping;
- Microsoft Entra object-ID/application-ID semantics;
- GUID requirements;
- source-reference representation or format;
- `IdentityRecord` member list;
- `IdentityKind` vocabulary;
- graph node/edge shapes;
- deduplication algorithm;
- conflict-resolution algorithm;
- evaluation/finding/evidence/provenance identifier formats;
- fingerprints;
- hashing or signing;
- serialization or wire schemas;
- JSON/SARIF representation;
- persistence;
- database/storage representation;
- capability/completeness semantics;
- operational error/failure taxonomy.

Those remain with their locked owning contracts and later implementation
stages.

## Consequences

### Positive

- Later graph, rule, finding, and evidence consumers receive a tenant-scoped
  normalized identity reference without depending on provider types.
- Cross-tenant identity collision is structurally harder to introduce.
- Provider/source identity remains separated from normalized assessment-local
  identity.
- Deferred identifier-format work can be completed later without changing the
  semantic purpose of the Stage-2 reference.

### Trade-offs

- An additional small Core value type is required for the assessment-local
  key.
- Producers must explicitly construct normalized references rather than pass
  arbitrary identifier strings.
- This decision intentionally leaves key generation and external identifier
  mapping unresolved.

## Rejected alternatives

### Raw string identity references

Rejected because an unqualified string does not structurally distinguish an
assessment-local normalized identity key from a source/native identifier and
does not carry tenant scope.

### Provider-native identifier as the Core identity

Rejected because it would couple Core identity semantics to provider-specific
formats and prematurely resolve deferred provider mapping decisions.

### Key without tenant context

Rejected because normalized identity equality is tenant-scoped. Identical key
text across tenant contexts MUST NOT establish identity equality.

### Display name as identity

Rejected because the locked domain contract explicitly treats display names as
human-readable metadata, not equality keys.

### Resolving identifier format now

Rejected because Stage 2 explicitly defers identifier formats and the locked
design assigns format/generation mechanics to later coordinated work.

## Acceptance condition

This decision may move from **Proposed** to **Accepted** only after review
confirms that it:

- implements only the minimum Stage-2 semantic requirement;
- preserves explicit tenant scoping;
- preserves normalized-key versus source-reference separation;
- does not invent an identifier format or generation algorithm;
- does not introduce provider-specific semantics;
- does not introduce Stage-3 capability/completeness semantics;
- does not introduce Stage-4 evidence/provenance schemas;
- does not weaken secret exclusion or Core dependency isolation.
