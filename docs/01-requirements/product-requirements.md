# EntraNHI V1 Product Requirements

> **Status:** Initial draft
> **Version:** 1.0-draft
> **Date:** 2026-09-18

---

## Table of contents

1. [Overview](#overview)
2. [Definitions](#definitions)
3. [Scope](#scope)
4. [Functional requirements](#functional-requirements)
5. [Security requirements](#security-requirements)
6. [Non-functional requirements](#non-functional-requirements)
7. [Output requirements](#output-requirements)
8. [Capability handling](#capability-handling)
9. [Deterministic verdict model](#deterministic-verdict-model)
10. [Assumptions](#assumptions)
11. [Constraints](#constraints)
12. [Non-goals](#non-goals)
13. [Acceptance criteria](#acceptance-criteria)
14. [Requirements index](#requirements-index)

---

## Overview

EntraNHI is an independent open-source security analysis tool for Microsoft Entra non-human identities, workload identities, and AI agents. In V1 it is an operator-run assessment tool capable of assessing a configured tenant per execution context; it is not a SaaS or inherently multi-tenant service.

Version 1 (V1) delivers read-only, evidence-based security assessment with deterministic verdicts. It does not perform write operations, automatic remediation, credential rotation, or autonomous AI-driven security decisions.

---

## Definitions

| Term | Definition |
| --- | --- |
| **Identity** | A Microsoft Entra application registration, service principal, managed identity, or agent identity examined by EntraNHI. |
| **Evidence** | A data point collected from Microsoft Graph, Azure Resource Manager, licensed telemetry, or configuration sources that supports a verdict. |
| **Rule** | A defined security check evaluated against collected evidence. |
| **Verdict** | The deterministic outcome of a rule evaluation (PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR). |
| **Findings** | One or more verdicts produced by rule evaluation for a given identity. |
| **Telemetry** | Data available through licensed Microsoft capabilities such as Microsoft Entra sign-in logs, audit logs, or identity insights. |

---

## Scope

### V1 identity coverage

EntraNHI V1 MUST analyze the following identity types:

| Identity type | Requirement ID | Coverage |
| --- | --- | --- |
| Application registrations | FR-001 | Full metadata collection and rule evaluation |
| Service principals | FR-002 | Full metadata collection and rule evaluation |
| Managed identities | FR-003 | Full metadata collection and rule evaluation |
| Microsoft Entra agent identities | FR-004 | Collection and rule evaluation where supported by available Microsoft Graph capabilities |

### V1 evidence coverage

For each identity type, EntraNHI V1 MUST collect and evaluate evidence in the following areas:

| Evidence area | Requirement ID | Description |
| --- | --- | --- |
| Identity metadata | FR-010 | Display name, object ID, application ID, creation timestamp, deletion status, and available metadata |
| Identity type | FR-011 | Classification of the identity (application registration, service principal, managed identity, agent identity) |
| Ownership and accountability | FR-012 | Owners, sponsors, managers, and other accountability relationships available through Microsoft Graph |
| Credential lifecycle metadata | FR-013 | Credential type, key/credential identifier, start/valid-from timestamp, and expiration/end timestamp where available; never the secret value itself |
| API and application permissions | FR-014 | Application permissions, delegated permissions, role assignments, and consent status |
| Identity relationships | FR-015 | Relationships between identities, applications, service principals, and managed identities |

---

## Functional requirements

### Identity collection

- **FR-001:** EntraNHI V1 MUST collect application registrations available through Microsoft Graph.
- **FR-002:** EntraNHI V1 MUST collect service principals available through Microsoft Graph.
- **FR-003:** EntraNHI V1 MUST collect managed identities represented in Microsoft Entra as service principals. Managed identities can be identified through supported Microsoft Graph service-principal metadata. Azure Resource Manager MAY be used as an optional enrichment source if future approved rules require Azure resource context, but is not mandatory for managed-identity discovery.
- **FR-004:** EntraNHI V1 MUST collect Microsoft Entra agent identities where supported by available Microsoft Graph capabilities. Microsoft Entra agent identities are first-class agent identity resources related to, and inheriting service-principal semantics from, their associated application and service principal. Agent identity blueprints are related application-derived resources. Agent-specific collection MUST remain capability-aware. Where agent identity endpoints or data structures are unavailable, EntraNHI MUST report this explicitly using NOT_EVALUATED or NOT_APPLICABLE as appropriate. EntraNHI V1 MUST NOT invent Graph endpoints, permissions, or undocumented properties for agent identities.

### Evidence collection

- **FR-010:** EntraNHI V1 MUST collect identity metadata including display name, object ID, application ID, creation timestamp, deletion status, and additional metadata made available through Microsoft Graph.
- **FR-011:** EntraNHI V1 MUST classify each identity by its Microsoft Entra identity type.
- **FR-012:** EntraNHI V1 MUST collect ownership and accountability data where available through Microsoft Graph, including documented relationship types such as owners, sponsors, and managers. Not all relationship types exist for all identity types; EntraNHI MUST report only those relationships actually available for each identity.
- **FR-013:** EntraNHI V1 MUST collect credential lifecycle metadata only in terms of documented, available properties: credential type, key/credential identifier, start/valid-from timestamp where available, and expiration/end timestamp where available. EntraNHI V1 MUST NOT invent credential properties not exposed by Microsoft Graph. EntraNHI V1 MUST NOT collect, store, log, or transmit credential secret values, private keys, access tokens, refresh tokens, or other authentication secrets.
- **FR-014:** EntraNHI V1 MUST collect application permissions, delegated permissions, and role assignments for each identity where available through Microsoft Graph.
- **FR-015:** EntraNHI V1 MUST collect supported identity relationships including links between application registrations and service principals, service principal dependencies, and managed identity associations.

### Analysis

- **FR-020:** EntraNHI V1 MUST evaluate each identity against defined security rules.
- **FR-021:** EntraNHI V1 MUST produce a deterministic verdict for each rule evaluation.
- **FR-022:** EntraNHI V1 MUST attach evidence to every PASS or FAIL verdict.
- **FR-023:** EntraNHI V1 MUST NOT use AI models to generate PASS, FAIL, or any security verdict.

### Authentication

- **FR-030:** EntraNHI V1 MUST authenticate to Microsoft Graph using the minimum permissions required for data collection.
- **FR-031:** EntraNHI V1 MUST support an interactive/delegated operator authentication mode and a non-interactive workload authentication mode suitable for automation. Both modes MUST follow least privilege. Neither mode MUST require credentials to be stored in source control. EntraNHI V1 MUST NOT prescribe exact OAuth flows, credential mechanisms, or SDKs; implementation details belong to architecture and security design.

### Processing

- **FR-040:** EntraNHI V1 MUST operate in read-only mode by default. No write, delete, update, or administrative operations MUST be performed against Microsoft Entra or Azure Resource Manager.
- **FR-041:** EntraNHI V1 MUST handle pagination, throttling, and transient errors returned by Microsoft Graph in accordance with Microsoft Graph API guidelines.

---

## Security requirements

- **SEC-001:** EntraNHI V1 MUST NOT collect, store, log, transmit, or expose credential secret values, client secrets, private keys, access tokens, refresh tokens, or recovery codes.
- **SEC-002:** EntraNHI V1 MUST NOT commit any credential material to source control.
- **SEC-003:** EntraNHI V1 MUST NOT perform credential rotation, credential deletion, or credential modification of any kind.
- **SEC-004:** EntraNHI V1 MUST NOT modify permissions, role assignments, or consent for any identity.
- **SEC-005:** EntraNHI V1 MUST NOT modify Conditional Access policies or other authentication/authorization policy configuration.
- **SEC-006:** EntraNHI V1 MUST NOT delete or disable any identity, service principal, application registration, or managed identity.
- **SEC-007:** EntraNHI V1 MUST NOT perform automatic remediation of any security finding.
- **SEC-008:** EntraNHI V1 MUST use least-privilege Microsoft Graph permissions sufficient for data collection only.
- **SEC-009:** EntraNHI V1 MUST use sanitized placeholders, synthetic test data, or mocks in all tests and documentation. No production credentials or tenant data MUST appear in the repository.
- **SEC-010:** EntraNHI V1 MUST NOT use AI models to generate security verdicts (PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR).
- **SEC-011:** EntraNHI V1 MUST preserve the authorization boundary between the analysis platform and the target tenant. Authentication artifacts MUST NOT be persisted in source control, logs, or output files.

---

## Non-functional requirements

- **NFR-001:** EntraNHI V1 MUST be deliverable as a command-line tool or library with no mandatory server-side component.
- **NFR-002:** EntraNHI V1 MUST NOT require production tenant credentials to be stored in the repository.
- **NFR-003:** EntraNHI V1 MUST produce machine-readable output suitable for integration with CI/CD pipelines, security dashboards, or SIEM systems.
- **NFR-004:** EntraNHI V1 MUST log operations using a structured logging approach that excludes credential material.
- **NFR-005:** EntraNHI V1 MUST support operation across Microsoft Entra tenants where the authenticated principal has sufficient permissions, without tenant-specific hardcoding.
- **NFR-006:** EntraNHI V1 MUST document all Microsoft Graph permissions required and the rationale for each.
- **NFR-007:** EntraNHI V1 MUST explicitly document supported and unsupported identity types, API capabilities, and licensed telemetry features.

---

## Output requirements

EntraNHI V1 MUST support the following output formats:

| Format | Requirement ID | Description |
| --- | --- | --- |
| CLI | OUT-001 | Human-readable console output suitable for interactive use and log capture. |
| JSON | OUT-002 | Machine-readable structured output for programmatic consumption and integration. |
| SARIF | OUT-003 | Static Analysis Results Interchange Format (SARIF) output for integration with security tooling and GitHub code scanning. |
| HTML | OUT-004 | Self-contained HTML report for review, sharing, and archival. |

- **OUT-005:** Every output format MUST include identity metadata, identity type, verdicts, evidence references, and evaluation timestamps.
- **OUT-006:** Output MUST NOT contain credential secret values, client secrets, private keys, tokens, or other secret-bearing material.

---

## Capability handling

- **CAP-001:** EntraNHI V1 MUST represent unavailable API capabilities, unavailable licensed telemetry, insufficient permissions, or unsupported identity types explicitly in output and findings.
- **CAP-002:** Unavailable API capability, unavailable licensed telemetry, insufficient permission, or unsupported identity type MUST NOT automatically resolve to FAIL.
- **CAP-003:** EntraNHI V1 MUST NOT assume all Microsoft Entra tenants expose identical APIs, licensing tiers, telemetry availability, identity types, or Microsoft Graph endpoint behavior.
- **CAP-004:** EntraNHI V1 MUST document which Microsoft Graph capabilities, API endpoints, and licensed features are required for each rule and the expected behavior when those capabilities are absent.
- **CAP-005:** EntraNHI V1 MUST NOT invent, hallucinate, or assume Microsoft Graph endpoints, permissions, licensing behavior, or undocumented Microsoft functionality.

---

## Deterministic verdict model

Every rule evaluation by EntraNHI V1 MUST resolve to exactly one of the following verdicts:

| Verdict | Requirement ID | Definition |
| --- | --- | --- |
| **PASS** | VERD-001 | The rule evaluated the available evidence and determined the identity satisfies the security requirement. Evidence MUST be attached. |
| **FAIL** | VERD-002 | The rule evaluated the available evidence and determined the identity does not satisfy the security requirement. Evidence MUST be attached. |
| **NOT_EVALUATED** | VERD-003 | The rule could not be evaluated due to unavailable data, insufficient permissions, missing licensed telemetry, or a capability gap. Structured reason/context sufficient to explain why normal evaluation did not occur MUST be attached. |
| **NOT_APPLICABLE** | VERD-004 | The rule does not apply to the identity type under evaluation. Structured reason/context identifying the identity type and rule applicability MUST be attached. |
| **ERROR** | VERD-005 | An unexpected error prevented rule evaluation. Structured reason/context describing the error MUST be attached without exposing credential material. |

- **VERD-006:** Each verdict MUST be deterministic. Given identical input evidence and rule configuration, EntraNHI V1 MUST produce the same verdict on every execution.
- **VERD-007:** EntraNHI V1 MUST NOT use AI, machine learning, or probabilistic models to determine verdicts. All verdicts MUST be derived from deterministic rule logic applied to collected evidence.

---

## Assumptions

- **ASM-001:** The authenticated principal has sufficient Microsoft Graph permissions to collect identity metadata for the target tenant. Insufficient permissions result in NOT_EVALUATED, not FAIL.
- **ASM-002:** Microsoft Graph API behavior, endpoint availability, and response schemas follow current published Microsoft documentation at the time of evaluation.
- **ASM-003:** EntraNHI V1 operates in a trusted execution environment controlled by the operator.
- **ASM-004:** Operators will configure authentication using supported Microsoft identity platform flows outside the scope of EntraNHI V1 source code.
- **ASM-005:** EntraNHI V1 will be deployed and operated by users with appropriate Microsoft Entra role assignments for read access to the target tenant's identity objects.

---

## Constraints

- **CON-001:** EntraNHI V1 MUST NOT modify any Microsoft Entra or Azure Resource Manager state.
- **CON-002:** EntraNHI V1 MUST NOT store credentials, secrets, tokens, or certificate material in the repository, configuration files, output files, or logs.
- **CON-003:** EntraNHI V1 MUST NOT introduce new external dependencies beyond those required for Microsoft Graph communication, output formatting, and CLI operation without explicit approval.
- **CON-004:** EntraNHI V1 MUST NOT perform autonomous or AI-driven security decisions or remediation.
- **CON-005:** Runtime network communication used for tenant assessment MUST be limited to explicitly documented and approved service endpoints required by enabled collectors and features. EntraNHI V1 MUST NOT perform undisclosed telemetry or transmit collected tenant assessment data to unrelated third parties. Normal development, build, package, and CI network access is not restricted by this constraint.

---

## Non-goals

The following are explicitly OUT OF SCOPE for EntraNHI V1:

- **NG-001:** Automatic remediation of security findings.
- **NG-002:** Write operations against Microsoft Entra (permission changes, role assignments, policy modifications, identity creation/deletion).
- **NG-003:** SaaS multi-tenancy or hosted service deployment.
- **NG-004:** Production web dashboard or user-facing web interface.
- **NG-005:** Autonomous AI-driven security decisions or AI-generated verdicts.
- **NG-006:** Replacement of Microsoft Entra administration or governance products.
- **NG-007:** Credential rotation or credential lifecycle management operations.
- **NG-008:** Real-time monitoring, alerting, or event-driven analysis.

---

## Acceptance criteria

EntraNHI V1 is considered functionally complete for initial release when the following criteria are satisfied:

| Criterion | Requirement ID | Description |
| --- | --- | --- |
| Identity collection | AC-001 | EntraNHI V1 can collect application registrations, service principals, managed identities, and agent identities from a configured tenant using Microsoft Graph. |
| Agent identity handling | AC-002 | EntraNHI V1 explicitly handles Microsoft Entra agent identities, reporting NOT_EVALUATED or NOT_APPLICABLE where the capability is unavailable. |
| Evidence attachment | AC-003 | Every PASS or FAIL verdict includes attached evidence referencing the collected data point. |
| Verdict determinism | AC-004 | Identical input and configuration produce identical verdicts across multiple runs. |
| Output formats | AC-005 | EntraNHI V1 produces valid CLI, JSON, SARIF, and HTML output. |
| No credential exposure | AC-006 | No credential secret value, client secret, private key, access token, or refresh token appears in output, logs, or repository. |
| No write operations | AC-007 | EntraNHI V1 performs zero write, delete, or administrative operations against Microsoft Entra or any connected service. |
| Capability gaps documented | AC-008 | Unavailable API capabilities, missing permissions, and unsupported identity types are reported explicitly with NOT_EVALUATED or NOT_APPLICABLE. |
| Permission documentation | AC-009 | All required Microsoft Graph permissions are documented with rationale. |
| Test data only | AC-010 | All tests, fixtures, mocks, and documentation examples use sanitized or synthetic data. No production credentials appear in the repository. |
| Authentication modes | AC-011 | EntraNHI V1 supports both interactive/delegated and non-interactive workload authentication modes without storing credentials in source control. |
| Network constraint | AC-012 | Runtime network communication is limited to documented and approved service endpoints; no undisclosed telemetry or third-party data transmission occurs. |

---

## Requirements index

| ID | Category | Summary |
| --- | --- | --- |
| FR-001 | Functional | Collect application registrations |
| FR-002 | Functional | Collect service principals |
| FR-003 | Functional | Collect managed identities via service-principal metadata |
| FR-004 | Functional | Collect agent identities where supported |
| FR-010 | Functional | Collect identity metadata |
| FR-011 | Functional | Classify identity type |
| FR-012 | Functional | Collect ownership and accountability |
| FR-013 | Functional | Collect documented credential lifecycle metadata |
| FR-014 | Functional | Collect API and application permissions |
| FR-015 | Functional | Collect identity relationships |
| FR-020 | Functional | Evaluate security rules |
| FR-021 | Functional | Produce deterministic verdicts |
| FR-022 | Functional | Attach evidence to verdicts |
| FR-023 | Functional | No AI-generated security verdicts |
| FR-030 | Functional | Least-privilege authentication |
| FR-031 | Functional | Support interactive/delegated and workload authentication modes |
| FR-040 | Functional | Read-only operation by default |
| FR-041 | Functional | Handle Graph pagination and throttling |
| SEC-001 | Security | No credential secret collection or storage |
| SEC-002 | Security | No credentials in source control |
| SEC-003 | Security | No credential rotation or modification |
| SEC-004 | Security | No permission or role modification |
| SEC-005 | Security | No Conditional Access modification |
| SEC-006 | Security | No identity deletion |
| SEC-007 | Security | No automatic remediation |
| SEC-008 | Security | Least-privilege Graph permissions |
| SEC-009 | Security | Sanitized test data only |
| SEC-010 | Security | No AI-generated verdicts |
| SEC-011 | Security | Preserve authorization boundary |
| NFR-001 | Non-functional | CLI or library delivery |
| NFR-002 | Non-functional | No stored production credentials |
| NFR-003 | Non-functional | Machine-readable output |
| NFR-004 | Non-functional | Structured logging without secrets |
| NFR-005 | Non-functional | Configurable tenant targeting without hardcoding |
| NFR-006 | Non-functional | Permission documentation |
| NFR-007 | Non-functional | Capability documentation |
| OUT-001 | Output | CLI output |
| OUT-002 | Output | JSON output |
| OUT-003 | Output | SARIF output |
| OUT-004 | Output | HTML output |
| OUT-005 | Output | Identity metadata in all outputs |
| OUT-006 | Output | No secrets in output |
| CAP-001 | Capability | Explicit unavailable capability reporting |
| CAP-002 | Capability | Not-auto-FAIL for missing capabilities |
| CAP-003 | Capability | No identical-tenant assumption |
| CAP-004 | Capability | Document required capabilities per rule |
| CAP-005 | Capability | No invented Microsoft behavior |
| VERD-001 | Verdict | PASS definition and evidence requirement |
| VERD-002 | Verdict | FAIL definition and evidence requirement |
| VERD-003 | Verdict | NOT_EVALUATED definition and structured reason requirement |
| VERD-004 | Verdict | NOT_APPLICABLE definition and structured reason requirement |
| VERD-005 | Verdict | ERROR definition and structured reason requirement |
| VERD-006 | Verdict | Deterministic verdict requirement |
| VERD-007 | Verdict | No AI/ML-based verdicts |
| ASM-001 | Assumption | Sufficient Graph permissions available |
| ASM-002 | Assumption | Graph API follows published behavior |
| ASM-003 | Assumption | Trusted execution environment |
| ASM-004 | Assumption | Auth configured outside EntraNHI |
| ASM-005 | Assumption | Operator has appropriate role assignments |
| CON-001 | Constraint | No Microsoft Entra state modification |
| CON-002 | Constraint | No credential storage anywhere |
| CON-003 | Constraint | No unapproved new dependencies |
| CON-004 | Constraint | No autonomous AI decisions |
| CON-005 | Constraint | Network communication limited to approved endpoints |
| NG-001 | Non-goal | No automatic remediation |
| NG-002 | Non-goal | No write operations |
| NG-003 | Non-goal | No SaaS multi-tenancy |
| NG-004 | Non-goal | No production web dashboard |
| NG-005 | Non-goal | No autonomous AI security decisions |
| NG-006 | Non-goal | No replacement of Entra admin products |
| NG-007 | Non-goal | No credential rotation operations |
| NG-008 | Non-goal | No real-time monitoring or alerting |
| AC-001 | Acceptance | Identity collection from configured tenant |
| AC-002 | Acceptance | Agent identity explicit handling |
| AC-003 | Acceptance | Evidence attached to verdicts |
| AC-004 | Acceptance | Verdict determinism validated |
| AC-005 | Acceptance | All output formats produced |
| AC-006 | Acceptance | No credential exposure validated |
| AC-007 | Acceptance | No write operations validated |
| AC-008 | Acceptance | Capability gaps documented in output |
| AC-009 | Acceptance | Permissions documented with rationale |
| AC-010 | Acceptance | Sanitized test data validated |
| AC-011 | Acceptance | Authentication modes validated |
| AC-012 | Acceptance | Network constraint validated |
