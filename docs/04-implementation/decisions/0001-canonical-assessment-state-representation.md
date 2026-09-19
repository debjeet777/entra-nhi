# 0001 — Canonical assessment-state representation

> **Status:** Accepted for Phase 0.4 implementation
> **Scope:** EntraNHI.Core
> **Phase:** 0.4 — Stage 2

## Context

The locked architecture and implementation design require every authoritative
rule evaluation to resolve to exactly one of five states:

- `PASS`
- `FAIL`
- `NOT_EVALUATED`
- `NOT_APPLICABLE`
- `ERROR`

Capability state, data availability, operational failure, severity,
presentation state, and other metadata are explicitly separate concepts and
must not create additional assessment states.

Stage 2 requires the canonical assessment-state vocabulary to exist in Core
without introducing provider, network, filesystem, renderer, authentication,
or tenant-access dependencies.

## Decision

Represent the authoritative V1 assessment-state vocabulary in
`EntraNHI.Core` as a public `AssessmentState` enum containing exactly:

- `PASS`
- `FAIL`
- `NOT_EVALUATED`
- `NOT_APPLICABLE`
- `ERROR`

The member spelling intentionally matches the canonical terminology of the
locked design.

## Constraints preserved

This decision does not:

- create a sixth assessment state;
- represent capability or data-availability conditions;
- represent operational success/failure;
- define serialization or wire-format behavior;
- define result-envelope shapes;
- define structured reason/context shapes;
- define tenant-context or identifier formats;
- introduce provider-specific types;
- introduce an external dependency.

Those concerns remain governed by their existing owning documents and
unresolved TBDs.

## Verification

Automated Core tests verify:

1. the enum exposes exactly five members; and
2. their names are exactly the canonical five-state vocabulary.

Architecture tests continue to verify Core independence.

## Deferred decisions

This record does not resolve:

- absent/unknown data representation;
- capability-state representation;
- tenant-context representation;
- assessment-result/result-envelope representation;
- reason/diagnostic representation;
- evidence/provenance schemas;
- serialization formats.

Those decisions require their own coordinated implementation review before
production types are introduced.
