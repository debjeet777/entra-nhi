# System Context

> **Status:** Phase 0.2.2
> **Date:** 2026-09-18

---

## Purpose

Define EntraNHI's system boundary, external systems, actors, inputs, outputs, internal components, data flow, read-only boundary, data classification, and AI boundary. This establishes the architectural context that domain-model, collector, rule-engine, evidence, output, and security design must obey.

---

## 1. System purpose

EntraNHI is a read-only security assessment system for supported Microsoft Entra non-human identities.

V1 assesses documented observable state and produces deterministic findings and evidence.

It does not perform remediation or mutate tenant state.

---

## 2. Primary actors

| Actor | Role | Interaction |
| --- | --- | --- |
| **Security / identity practitioner** | Primary user | Configures and executes EntraNHI, authenticates to Microsoft identity platform, reviews assessment output |
| **Administrator / operator** | Execution controller | Runs EntraNHI against a target tenant, manages authentication configuration, interprets results |
| **CI / automation caller** | Non-interactive consumer | Invokes EntraNHI in workload mode, consumes machine-readable output (JSON, SARIF) per OUT-002, OUT-003, NFR-003 |

No additional personas are assumed beyond those documented here.

---

## 3. External system boundaries

### Microsoft identity platform

- Authentication and token issuance boundary.
- External to EntraNHI.
- EntraNHI receives transient authorized access tokens through the authentication layer; tokens are not domain data or evidence.

### Microsoft Graph

- Primary documented V1 tenant-data collection API boundary.
- External to EntraNHI.
- All V1 tenant data collection is read-only against this boundary.

### Optional documented enrichment APIs

- Architecturally isolated from core assessment.
- Must not become implicit dependencies of the core.
- Not mandatory for V1. If future approved rules require enrichment from an additional documented API, it is represented as an optional capability subject to capability awareness (INV-07).

### Local / runtime environment

- Executes EntraNHI.
- Handles transient authorized access tokens through the authentication layer.
- Tokens are not domain data, findings, evidence, logs, reports, or persisted assessment artifacts.

### CI / automation environment

- Optional execution context.
- Must obey the same authentication, authorization, and evidence rules as interactive execution.

### Output consumers

| Consumer | Format | Description |
| --- | --- | --- |
| CLI user | CLI (OUT-001) | Human-readable console output |
| JSON consumer | JSON (OUT-002) | Machine-readable structured output |
| SARIF consumer | SARIF (OUT-003) | Static analysis results interchange |
| HTML report consumer | HTML (OUT-004) | Self-contained report |
| Future dashboard consumer | TBD | Presentation layer not in V1 scope |

---

## 4. Internal high-level components

The following logical responsibilities exist within the EntraNHI boundary. They represent architectural responsibilities, not implementation technology choices.

| Component | Responsibility |
| --- | --- |
| **Authentication boundary** | Obtains and manages transient authorized access tokens. Does not persist or expose tokens as domain data. |
| **Collector layer** | Executes read-only data collection against documented external APIs. Respects capability detection results. |
| **Capability detection** | Determines which APIs, licensed features, permissions, and identity types are available in the target environment. |
| **Normalization / domain model** | Translates provider-specific source objects into stable normalized contracts consumed by the rule engine. |
| **Identity graph** | Constructs normalized identity relationships from collected data for cross-identity evaluation. |
| **Deterministic rule engine** | Evaluates normalized data against defined rules. Produces PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR verdicts. MUST NOT directly query Microsoft Graph or any other provider API. |
| **Finding / evidence construction** | Attaches traceable evidence to PASS and FAIL verdicts. Attaches structured reason/context to other verdict states. |
| **Output / serialization layer** | Transforms findings and evidence into output-format-specific contracts (CLI, JSON, SARIF, HTML). |
| **Application / orchestration boundary** | Coordinates component lifecycle, error handling, and execution flow. |

Provider-specific representation MUST NOT leak into deterministic rule contracts except through explicitly normalized capability/provenance data.

---

## 5. High-level data flow

The following is a conceptual representation of the assessment flow. It is not an implementation dependency graph.

```
Operator / automation
        |
Authentication boundary
        |
Microsoft Graph / approved documented source
        |
Collector layer
        |
Capability detection + normalization
        |
Normalized NHI model
        |
Identity graph
        |
Deterministic rule engine
        |
Finding / evidence construction
        |
Shared output contracts
        |
CLI / JSON / SARIF / HTML / future dashboard
```

Each arrow represents a conceptual transition, not a required implementation coupling.

---

## 6. Read-only boundary

- V1 tenant interactions are read-only.
- EntraNHI MUST NOT intentionally invoke tenant-state mutation operations.
- Remediation is outside V1.
- Report and file creation in the execution environment is not considered tenant mutation.
- Authentication and session mechanics required to obtain authorized read access are not remediation.

---

## 7. Data classification boundary

### Allowed as assessment data

- Documented identity metadata required for assessment (display name, object ID, application ID, creation timestamp, deletion status, available metadata)
- Non-secret credential lifecycle metadata (credential type, key identifier, start/valid-from timestamp, expiration/end timestamp)
- Permissions and relationships
- Capability and provenance metadata
- Findings and evidence

### Forbidden as assessment data

- Client secret values
- Private keys
- Certificate private-key material
- Passwords
- Bearer / access / refresh tokens (as domain data)
- Recovery codes
- Unrelated tenant content

### Transient token handling

Authentication tokens may exist transiently in process memory during authorized read access. They MUST NOT become normalized domain data, findings, evidence, logs, reports, or persisted assessment artifacts.

---

## 8. AI boundary

AI/LLM functionality is not part of the authoritative assessment path.

If introduced later, AI MAY explain deterministic results but MUST NOT:

- determine PASS/FAIL verdicts
- change rule outcomes
- fabricate evidence
- bypass authorization
- expand collection privileges
- become required for core assessment correctness

---

## 9. Scope boundaries

- EntraNHI V1 is an operator-run assessment tool delivered as a CLI tool or library with no mandatory server-side component (NFR-001). It executes within a trusted local runtime controlled by the operator.
- EntraNHI V1 is not a SaaS or multi-tenant service (NG-003).
- EntraNHI V1 does not include a production web dashboard (NG-004), real-time monitoring (NG-008), or autonomous remediation (NG-001).
- The system boundary is the local runtime; Microsoft Entra and Graph are external data sources, not part of the EntraNHI deployment.

---

## 10. Open items

- **TBD:** Exact network endpoint allowlist for runtime communication (CON-005).
- **TBD:** Formal data-flow diagram.
