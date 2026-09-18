# Architecture Invariants

> **Status:** Phase 0.2.2
> **Date:** 2026-09-18

---

## Purpose

Define the stable numbered invariant catalog for EntraNHI V1 architecture. These invariants derive from requirements in `docs/01-requirements/product-requirements.md`, from security principles in `AGENTS.md` and `SECURITY.md`, and from the architectural discipline established in `docs/02-architecture/system-context.md`.

Violations of any invariant are defects. Invariants MUST NOT be relaxed without explicit approval and an updated architecture decision record.

Each invariant uses RFC 2119 / RFC 8174 normative language. MUST / MUST NOT = invariant. SHOULD = preferred design with legitimate implementation flexibility. MAY = optional.

---

## Invariant catalog

### INV-01 — Read-only tenant operation

**Statement:** V1 does not intentionally mutate Microsoft Entra tenant state. EntraNHI MUST NOT perform any write, delete, update, or administrative operation against Microsoft Entra or Azure Resource Manager. Authentication and session mechanics required to obtain authorized read access are not tenant mutation. Report and file creation in the execution environment is not tenant mutation.

**Rationale:** Read-only operation is the fundamental architectural boundary that prevents the tool from causing unintended side effects in the target tenant.

**Architectural consequence:** No write, delete, or administrative API call paths may be implemented. Remediation and credential management are outside V1 scope. [FR-040, SEC-003, SEC-004, SEC-005, SEC-006, SEC-007, CON-001, NG-001, NG-002]

### INV-02 — Deterministic assessment

**Statement:** The same normalized inputs, capability state, rule version/configuration, and relevant deterministic execution context produce the same evaluation outcome. Timestamps or other presentation metadata that do not affect rule evaluation may differ between executions. Every verdict MUST resolve to exactly one of PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR.

**Rationale:** Deterministic assessment enables reproducible, auditable security findings that do not depend on runtime state, AI inference, or non-deterministic behavior.

**Architectural consequence:** The rule engine MUST be pure with respect to normalized inputs and configuration. No random, probabilistic, or time-dependent branching is permitted in rule evaluation logic. [VERD-001, VERD-002, VERD-003, VERD-004, VERD-005, VERD-006]

### INV-03 — Provider isolation

**Statement:** The rule engine MUST NOT directly query Microsoft Graph, Azure Resource Manager, or any other provider API. All data consumed by the rule engine arrives through the collector layer and normalization boundary.

**Rationale:** Provider isolation prevents the rule engine from introducing provider-specific coupling, bypassing normalization, or making undocumented API assumptions.

**Architectural consequence:** Rule evaluation code MUST NOT contain API client references, HTTP calls, or provider SDK imports. Data access occurs exclusively through normalized contracts.

### INV-04 — Normalized domain boundary

**Statement:** Provider-specific source objects MUST be translated into stable normalized contracts before rule evaluation. Provider-specific representation MUST NOT leak into deterministic rule contracts except through explicitly normalized capability/provenance data.

**Rationale:** A stable normalized domain model insulates rules from API schema changes, supports cross-provider assessment if additional sources are introduced, and makes evaluation logic testable without live API responses.

**Architectural consequence:** A translation/normalization layer MUST exist between collector output and rule input. Rule contracts MUST reference normalized types, not provider-specific types.

### INV-05 — Explicit evaluation states

**Statement:** Every rule evaluation MUST resolve to exactly one of PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR. No silent coercion of missing capability, collection failure, or unsupported state into FAIL is permitted.

**Rationale:** Explicit evaluation states prevent false negatives from masking capability gaps and ensure consumers of findings understand why evaluation did or did not occur.

**Architectural consequence:** The rule engine MUST have explicit code paths for each verdict state. Error and gap handling MUST NOT collapse into FAIL. [VERD-001 through VERD-005, CAP-002]

### INV-06 — Evidence traceability

**Statement:** PASS and FAIL verdicts MUST be supported by traceable evidence referencing the collected data point that supports the verdict. NOT_EVALUATED, NOT_APPLICABLE, and ERROR verdicts MUST carry structured reason/context sufficient to explain the evaluation outcome.

**Rationale:** Evidence traceability enables audit, validation, and remediation guidance. Structured reasons for non-terminal states enable consumers to distinguish capability gaps from security failures.

**Architectural consequence:** Every PASS/FAIL finding MUST carry an evidence reference. Every non-terminal verdict MUST carry structured reason/context metadata. [FR-022, VERD-001, VERD-002, VERD-003, VERD-004, VERD-005]

### INV-07 — Capability awareness

**Statement:** Licensing, API, permission, and support limitations MUST be represented explicitly. Unavailable capability MUST NOT automatically resolve to FAIL. EntraNHI MUST NOT assume all tenants expose identical APIs, licensing tiers, telemetry availability, identity types, or Graph endpoint behavior.

**Rationale:** Capability awareness prevents false assessments from assuming uniform platform behavior across tenants with different licensing, configuration, and permission profiles.

**Architectural consequence:** A capability detection mechanism MUST inform collectors and the rule engine. Rules MUST be gated on required capabilities. Gaps MUST produce NOT_EVALUATED, not FAIL. [CAP-001, CAP-002, CAP-003]

### INV-08 — Least privilege

**Statement:** Authentication and authorization MUST use only permissions required for enabled collection capabilities.

**Rationale:** Least privilege limits blast radius if credentials are compromised and aligns with organizational security policies.

**Architectural consequence:** The authentication boundary MUST request only the minimum scope needed for enabled collectors. Excess permissions MUST NOT be requested by default. [SEC-008, FR-030, FR-031]

### INV-09 — Secret exclusion

**Statement:** Secret values, private key material, client secrets, passwords, bearer/access/refresh tokens, and recovery codes are not assessment-domain data. They MUST NOT enter findings, evidence, logs, reports, or persisted assessment artifacts.

**Rationale:** Secret exclusion prevents credential exposure through tool output and ensures the tool does not become an attack surface for credential harvesting.

**Architectural consequence:** Output serialization, logging, evidence construction, and all persistence paths MUST exclude secret material. Transient tokens in process memory MUST NOT be normalized into domain data. [SEC-001, SEC-002, OUT-006]

### INV-10 — Tenant boundary preservation

**Statement:** Data from separate tenant assessment contexts MUST never be silently mixed. EntraNHI V1 is not a SaaS or simultaneous multi-tenant service; each execution context targets a single tenant.

**Rationale:** Tenant boundary preservation prevents cross-tenant data leakage and ensures findings are attributable to the correct tenant scope.

**Architectural consequence:** Execution context, collected data, and output artifacts MUST be scoped to a single tenant per run. No cross-tenant data aggregation or mixing paths may exist. [NG-003, NFR-005]

### INV-11 — Provenance preservation

**Statement:** Relevant normalized observations MUST retain enough provenance to identify their documented source and collection context without storing secrets.

**Rationale:** Provenance enables consumers to validate findings against source data, audit collection methodology, and reproduce assessment steps.

**Architectural consequence:** Normalized domain objects MUST carry source identifiers and collection context. Provenance MUST NOT include secret material. [FR-013, SEC-001]

### INV-12 — Core/output separation

**Statement:** Assessment semantics are independent from CLI, JSON, SARIF, HTML, or dashboard presentation.

**Rationale:** Separating assessment logic from presentation enables output format extensibility without modifying core rule evaluation.

**Architectural consequence:** The rule engine and finding/evidence layer MUST produce format-agnostic findings. Output adapters MUST transform these findings into format-specific contracts. Core logic MUST NOT depend on output format choices. [OUT-001, OUT-002, OUT-003, OUT-004]

### INV-13 — AI non-authority

**Statement:** AI and LLMs CANNOT determine authoritative assessment outcomes. AI/LLM output MUST NEVER determine PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR verdicts.

**Rationale:** Authoritative security assessment requires deterministic, auditable, reproducible evaluation. AI/LLM inference introduces non-determinism, hallucination risk, and audit opacity that are incompatible with this requirement.

**Architectural consequence:** AI/LLM functionality MUST NOT be on the critical path for verdict determination. If AI is introduced, it MUST be limited to post-assessment explanation or presentation and MUST NOT alter verdicts. [SEC-010, VERD-007, CON-004]

### INV-14 — Failure transparency

**Statement:** Collection, normalization, rule, or serialization failures MUST be represented explicitly and MUST NOT silently produce successful or security-compliant results.

**Rationale:** Transparent failure handling prevents silent data loss from being interpreted as a passing security posture.

**Architectural consequence:** Error handling MUST surface failures through ERROR verdicts, NOT_EVALUATED states, or explicit error reporting. No failure path may result in a PASS or false-compliant outcome. [VERD-005, VERD-003, CAP-001]

### INV-15 — No undocumented capability dependency

**Statement:** Core correctness MUST NOT depend on reverse-engineered, undocumented, or unsupported Microsoft behavior. EntraNHI MUST NOT invent, hallucinate, or assume Microsoft Graph endpoints, permissions, licensing behavior, or undocumented Microsoft functionality.

**Rationale:** Dependency on undocumented behavior creates fragile assessments that may break silently when Microsoft changes internal implementation details.

**Architectural consequence:** All collected data MUST originate from documented Microsoft APIs. All rule logic MUST reference documented properties and behaviors. Where documentation is absent, NOT_EVALUATED MUST be used. [FR-004, CAP-004, CAP-005]

### INV-16 — Security-sensitive defaults

**Statement:** Where optional behavior could materially weaken assessment integrity, confidentiality, or authorization boundaries, the secure behavior is the default. Weakening secure defaults requires explicit configuration where such configuration is permitted.

**Rationale:** Secure defaults reduce the risk of accidental misconfiguration and ensure the tool operates with maximum integrity out of the box.

**Architectural consequence:** Configuration options that affect security posture MUST default to the most restrictive secure behavior. Weakening options MUST require explicit opt-in. No configuration features are invented merely to satisfy this invariant.

---

## Traceability

Each invariant references one or more requirement IDs from `docs/01-requirements/product-requirements.md`. Architecture design must maintain bidirectional traceability between invariants, design decisions, and requirements.

---

## Open items

- **TBD:** Invariant for structured logging format (NFR-004) pending logging architecture design.
- **TBD:** Invariant for dependency/supply-chain integrity pending build architecture design.
