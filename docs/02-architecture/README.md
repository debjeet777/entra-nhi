# EntraNHI V1 Architecture & Security Design

> **Phase:** 0.2
> **Status:** Initial skeleton
> **Date:** 2026-09-18

---

## Relationship to requirements

The product requirements in `docs/01-requirements/` are authoritative. Architecture documents in this directory translate those requirements into design structure, invariants, and constraints. Where architecture decisions are still pending, they are marked **TBD** or **validation-required**.

This phase contains architecture and design only. No production implementation, dependencies, or application source code are introduced in this phase.

---

## Document index

| Document | Responsibility |
| --- | --- |
| [system-context.md](system-context.md) | System boundary, external systems, actors, inputs and outputs |
| [architecture-invariants.md](architecture-invariants.md) | Mandatory architectural and security invariants |
| [domain-model.md](domain-model.md) | Normalized NHI domain model and entity relationships |
| [collection-architecture.md](collection-architecture.md) | Collector responsibilities, boundaries, and capability-aware behavior |
| [identity-graph.md](identity-graph.md) | Normalized identity nodes, relationships, and graph construction rules |
| [rule-engine.md](rule-engine.md) | Deterministic rule evaluation and verdict lifecycle |
| [findings-evidence.md](findings-evidence.md) | Findings, evidence, provenance, and traceability |
| [authentication-authorization.md](authentication-authorization.md) | Authentication and authorization architecture boundaries |
| [output-architecture.md](output-architecture.md) | Shared core output model and surface-specific adapters |
| [trust-boundaries.md](trust-boundaries.md) | Trust zones, data-flow boundaries, and network constraints |
| [threat-model.md](threat-model.md) | Threat-model structure and coverage areas |
| [testing-architecture.md](testing-architecture.md) | Testing architecture for unit, contract, integration, and security tests |

---

## Reading order

1. [system-context.md](system-context.md)
2. [architecture-invariants.md](architecture-invariants.md)
3. [domain-model.md](domain-model.md)
4. [collection-architecture.md](collection-architecture.md)
5. [identity-graph.md](identity-graph.md)
6. [rule-engine.md](rule-engine.md)
7. [findings-evidence.md](findings-evidence.md)
8. [authentication-authorization.md](authentication-authorization.md)
9. [output-architecture.md](output-architecture.md)
10. [trust-boundaries.md](trust-boundaries.md)
11. [threat-model.md](threat-model.md)
12. [testing-architecture.md](testing-architecture.md)

---

## Normative language

This document uses RFC 2119 / RFC 8174 terminology:

- **MUST / MUST NOT** for genuine invariants.
- **SHOULD** where implementation flexibility is intended.
- **TBD** or **validation-required** for unresolved technical decisions.
