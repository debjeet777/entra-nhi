# 0003 — Domain value-presence representation

> **Status:** Accepted for Phase 0.4 implementation
> **Scope:** EntraNHI.Core
> **Phase:** 0.4 — Stage 2

## Context

The locked design requires normalized domain data to distinguish a value that
is present from data whose value is not presently known. One generic null
must not silently represent every missing-data condition.

The locked design also reserves the full data-availability, capability,
collection-completeness, and operational-failure semantics for their owning
contracts and later implementation stages.

In particular, Stage 3 owns capability states, collection completeness,
confirmed-empty versus indeterminate/partial/failed semantics, shared failure
taxonomy, and mapping of those conditions into assessment behavior.

Stage 2 therefore requires only the minimum inward value boundary needed to
prevent a present value from being confused with an unknown value without
prematurely defining why that value is unknown.

## Decision

Represent normalized domain value presence in Core with a provider-independent
value contract that distinguishes exactly two Stage-2 conditions:

1. a value is present; or
2. a value is not presently known.

The Stage-2 representation MUST preserve an actual present value and MUST make
the not-presently-known condition explicit rather than relying on an
unqualified null to carry all missing-data semantics.

The not-presently-known condition does not state why the value is unknown.

## Constraints preserved

This decision does not define:

- capability-state vocabulary;
- collection-completeness vocabulary;
- confirmed-empty semantics;
- unavailable, unsupported, or authorization-limited semantics;
- not-requested or not-collected semantics;
- collection-failure or conflict semantics;
- not-applicable semantics;
- operational failure taxonomy;
- mapping to `NOT_EVALUATED`, `NOT_APPLICABLE`, or `ERROR`;
- provider-specific data behavior;
- serialization or wire format;
- provider property/value mappings;
- licensing behavior;
- evidence or provenance schemas.

Those semantics remain owned by their locked design documents and later
implementation stages.

## Stage boundary

Stage 2 establishes only the structural distinction required to prove that
a present value and a value that is not presently known cannot be silently
conflated.

Stage 3 remains responsible for representing and propagating the reason that
data is unavailable or incomplete, including capability and completeness
semantics and the distinction between confirmed-empty and indeterminate,
partial, or failed collection.

Accordingly, Stage 2 MUST NOT introduce a standalone capability,
completeness, observation-condition, or operational-failure taxonomy as part
of this decision.

## Verification

Stage-2 Core tests for the eventual implementation will use synthetic values
only and verify that:

1. a present value is explicitly distinguishable from a value that is not
   presently known;
2. a present value is preserved without provider interpretation;
3. the not-presently-known representation does not imply confirmed empty;
4. the representation introduces no provider, network, authentication,
   filesystem, renderer, or secret dependency; and
5. the representation does not create an additional authoritative assessment
   state.

## Security rationale

An explicit value-presence boundary prevents missing information from being
silently interpreted as an observed value or as affirmative absence.

Keeping the Stage-2 representation deliberately narrower than capability and
completeness semantics also prevents premature false-PASS behavior: absence
cannot be treated as confirmed empty merely because a normalized value was
not available.

## Deferred decisions

This record does not yet select the concrete C# type name, generic type shape,
factory/member names, enum names, default-value behavior, serialization
representation, or interaction with Stage-3 capability/completeness
envelopes.

Those implementation mechanics require a separate review against the locked
domain, capability, error/result, and consumer contracts before production
types are introduced.
