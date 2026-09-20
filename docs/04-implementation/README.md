# Phase 0.4 Implementation

> **Status:** In progress
> **Phase:** 0.4 — Secure Core Implementation

This directory records implementation-phase decisions made while implementing
the reviewed and locked Phase 0.3 implementation-design baseline.

## Authority

The authoritative design remains:

- `docs/01-requirements/`
- `docs/02-architecture/`
- `docs/03-implementation-design/`

Implementation decisions in this directory MUST conform to those approved
requirements, architecture invariants, security boundaries, contracts, and
TBD ownership rules.

An implementation decision MUST NOT silently reinterpret or relax the locked
design baseline. A decision that requires changing an approved requirement,
architecture invariant, trust boundary, security property, or contract
semantics must return through the applicable change-control process.

## Decision records

Decision records capture concrete implementation choices that resolve
implementation-phase details while preserving the authoritative design.

They are intentionally narrow and do not create a second architecture source
of truth.

Current records:

- `decisions/0001-canonical-assessment-state-representation.md`
- `decisions/0002-tenant-context-representation.md`
- `decisions/0003-domain-value-presence-representation.md`
- `decisions/0004-domain-value-implementation-mechanics.md`
- `decisions/0005-normalized-identity-reference-contract.md`
- `decisions/0006-normalized-identity-reference-implementation-mechanics.md`
- `decisions/0007-canonical-evaluation-outcome-contract.md`
- `decisions/0008-capability-and-completeness-semantic-contract.md`
- `decisions/0009-capability-and-completeness-core-representation.md`
- `decisions/0010-operational-failure-semantic-contract.md`
- `decisions/0011-operational-failure-category-representation.md`
- [Decision 0012 — PASS Precondition Proof Contract](decisions/0012-pass-eligibility-proof-contract.md)
- [Decision 0013 — PASS Precondition Guard Mechanics](decisions/0013-pass-precondition-guard-mechanics.md)
- [Decision 0014 — Evidence and Provenance Semantic Contract](decisions/0014-evidence-and-provenance-semantic-contract.md)
- [Decision 0015 — Rule Evaluation Support Mechanics](decisions/0015-rule-evaluation-support-mechanics.md)
- [Decision 0016 — Rule Evaluation Construction Mechanics](decisions/0016-rule-evaluation-construction-mechanics.md)
