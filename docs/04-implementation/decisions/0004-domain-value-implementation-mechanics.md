# 0004 — Domain value implementation mechanics

> **Status:** Accepted for Phase 0.4 implementation
> **Scope:** EntraNHI.Core
> **Phase:** 0.4 — Stage 2

## Context

Decision 0003 established the Stage-2 semantic boundary for normalized domain
value presence:

1. a value is present; or
2. a value is not presently known.

It deliberately deferred the concrete C# representation.

The locked consumer contracts require missing or unknown information to remain
explicit and prohibit null, empty collections, or default values from silently
standing for every data-availability condition.

Stage 3 remains the owner of capability, completeness, confirmed-empty,
partial, unavailable, unsupported, failed, not-requested, and related
reason/cause semantics.

## Decision

Implement the Stage-2 value-presence boundary as a sealed generic Core
reference type named `DomainValue<T>`.

The type will expose exactly two controlled construction paths:

- `Present(T value)`
- `NotPresentlyKnown()`

A caller MUST NOT be able to construct an arbitrary or contradictory state.

The representation will expose whether a value is present independently from
the stored value.

A present value MUST be preserved without provider interpretation.

`Present(null)` MUST be rejected.

The not-presently-known form MUST NOT require or manufacture a value and MUST
NOT imply why the value is not known.

## Default and null behavior

`DomainValue<T>` is a reference type.

A null `DomainValue<T>` reference is invalid input and is not itself a domain
value-presence state.

The semantic not-presently-known condition is represented only by an actual
`DomainValue<T>` instance created through `NotPresentlyKnown()`.

This prevents `default(DomainValue<T>)` / null from becoming an accidental
third semantic state.

For the Present form, null is not a valid present value. This prevents a
contradictory "present but null" representation and keeps missing-data
semantics explicit.

## Stage boundary

This implementation MUST NOT introduce or encode:

- capability states;
- collection-completeness states;
- confirmed-empty semantics;
- partial-collection semantics;
- unavailable or unsupported causes;
- authorization or licensing causes;
- not-requested semantics;
- operational-failure categories;
- provider-specific behavior;
- assessment-state mappings;
- serialization or wire-format policy.

Those remain owned by later stages and their locked contracts.

## Verification

Stage-2 synthetic Core tests MUST verify at minimum:

1. Present preserves its supplied non-null value.
2. Present is distinguishable from NotPresentlyKnown.
3. NotPresentlyKnown does not expose a fabricated present value.
4. Present rejects null.
5. The representation is a reference type.
6. Arbitrary construction cannot create contradictory states.
7. The representation introduces no sixth `AssessmentState`.
8. Core remains free of provider, network, authentication, filesystem,
   renderer, and secret dependencies.

## Security rationale

Controlled construction prevents invalid combinations of presence metadata and
value data.

Rejecting null as a present value prevents missing information from being
silently represented as an observed value.

Using an explicit NotPresentlyKnown instance prevents null/default reference
behavior from being interpreted as a valid domain condition.

Keeping cause and completeness semantics outside this Stage-2 type prevents
premature false-PASS behavior and preserves the Stage-3 ownership boundary.

## Deferred decisions

This decision does not select:

- serialization representation;
- wire-format tokens;
- Stage-3 capability/completeness envelope composition;
- provider mappings;
- concrete domain records that will consume `DomainValue<T>`;
- collection-specific absence semantics.

Those decisions remain deferred to their owning implementation stages.
