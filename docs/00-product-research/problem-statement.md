# EntraNHI Problem Statement

> **Status:** Phase 0.2.12-B
> **Date:** 2026-09-18

---

## 1. Title

The NHI Assessment Problem: Why Non-Human Identity Security in Microsoft Entra Is Difficult

---

## 2. Problem Summary

Microsoft Entra tenants contain a growing number of non-human identities — application registrations, service principals, managed identities, and emerging AI agent identities. These identities carry credentials, possess API permissions, interact with other identities and cloud resources, and are subject to organizational accountability requirements. Unlike human user identities, non-human identities lack many of the lifecycle signals, behavioral patterns, and governance structures that established identity security practices rely on.

Assessing the security posture of non-human identities in Entra is difficult because:

- Identity data is distributed across multiple API surfaces and licensing tiers.
- Credential state is partially observable and partially opaque.
- Ownership and accountability are inconsistently assigned.
- Permission exposure is complex and context-dependent.
- Identity relationships are implicit, undocumented, or unavailable through standard APIs.
- Collection completeness cannot be assumed across tenants.
- The absence of data cannot be distinguished from the absence of risk without explicit capability awareness.
- AI agent identities introduce new identity categories with evolving, partially documented semantics.

EntraNHI is designed to address the need for deterministic, read-only, evidence-backed assessment with explicit handling of incomplete or unavailable data.

---

## 3. What Constitutes an NHI in EntraNHI's Scope

EntraNHI defines non-human identities as the following Microsoft Entra identity types:

| Identity Type | Description |
| --- | --- |
| **Application registrations** | The application object representing a software workload, service, or automation in Microsoft Entra. |
| **Service principals** | The security principal created for an application to access resources. |
| **Managed identities** | Service principals classified as managed identities through documented source evidence. Backed by Azure-managed credentials. |
| **Microsoft Entra agent identities** | First-class agent identity resources where supported by documented Microsoft Graph capabilities. Agent identities are represented only where supported by documented Microsoft capabilities. Exact provider mappings and relationship semantics remain implementation-design decisions and must not be inferred. |

Application registrations and service principals are distinct identity types. They are not interchangeable and must remain distinguishable in assessment.

Agent identities are an emerging category. Their API surface, data model, and relationship semantics are evolving. EntraNHI must remain capability-aware and must not invent undocumented properties or relationships for agent identities.

---

## 4. Why NHI Assessment Is Difficult

NHI assessment is difficult for several interrelated reasons:

1. **Distributed data sources.** Identity metadata, credential information, permissions, ownership, and relationships are spread across Microsoft Graph endpoints, Azure Resource Manager, and licensed telemetry sources. No single API provides a complete security-relevant view.

2. **Licensing variability.** Not all tenants have access to the same telemetry, identity insights, or advanced API capabilities. Assessment behavior must adapt to what is available, not assume uniform capability.

3. **Permission boundaries.** The authenticated principal's permissions determine what data is accessible. Insufficient permissions must be represented honestly, not coerced into security verdicts.

4. **Identity heterogeneity.** Application registrations, service principals, managed identities, and agent identities have different metadata schemas, relationship types, and observable properties. A one-size-fits-all assessment approach does not work.

5. **Credential opacity.** Credential metadata (type, identifier, validity windows) is partially observable, but the actual secret values are opaque by design. Assessment must reason about credential hygiene from metadata alone.

6. **Implicit relationships.** Many identity relationships — application-to-service-principal links, managed-identity backing, agent inheritance — are not always surfaced through standard API responses or require specific queries to discover.

7. **Temporal drift.** Identity state changes over time. Assessments are point-in-time observations. The meaning of an assessment must not depend on assumptions about state before or after the observation window.

---

## 5. Identity / Accountability Problem

Non-human identities in Microsoft Entra frequently lack clear, current accountability:

- Application registrations may have owners who have left the organization or changed roles.
- Service principals may have been created through automated processes with no designated owner.
- Managed identities may be associated with Azure resources whose ownership is unclear.
- Agent identities may inherit accountability from parent resources in ways that are not explicitly documented.
- Ownership records may be stale, inconsistent, or absent entirely.

Without reliable accountability data, it becomes difficult to:

- Determine who is responsible for a non-human identity's security configuration.
- Assign responsibility for credential rotation or lifecycle management.
- Evaluate whether an identity's permissions are appropriate and necessary.
- Decide whether an identity is still operationally required.

The accountability problem is compounded by the fact that ownership data may be unavailable due to authorization boundaries, licensing limitations, or the absence of the relationship in the source system. Distinguishing "no owner exists" from "owner data was not accessible" is essential for honest assessment.

---

## 6. Credential Lifecycle Problem

Non-human identities use credentials (asymmetric keys, symmetric secrets, federated identity credentials) to authenticate. Credential lifecycle management is a core security concern:

- Credentials have validity windows (start, expiration) that are partially observable through metadata.
- Expired credentials may still be associated with active identities.
- Credentials may never have been rotated since initial creation.
- Credential metadata may be incomplete — start dates, expiration dates, or key identifiers may not all be available through documented APIs.
- The actual secret values are deliberately opaque and must never be collected or exposed.

Assessment must reason about credential hygiene from available metadata:

- Is a credential expired or nearing expiration?
- When was the credential last observed?
- What credential type is in use?

But assessment cannot determine:

- Whether the credential has been rotated outside of observed metadata.
- Whether the credential is in active use.
- Whether the credential value itself is compromised.

The gap between observable metadata and operational reality is a fundamental constraint of read-only assessment.

---

## 7. Permission / Exposure Problem

Non-human identities may possess API permissions, application roles, and delegated permissions that grant access to organizational data and resources:

- Application permissions grant broad, often highly privileged access to Microsoft Graph or other APIs.
- Delegated permissions grant access in the context of a signed-in user.
- Role assignments (Azure RBAC) grant access to Azure resources.
- Consent status determines whether permissions have been approved for use.

Assessing permission exposure is difficult because:

- Permission catalogs are extensive and vary across API surfaces.
- A permission's security impact depends on the target resource, the identity's operational context, and organizational policy — information not always available through standard APIs.
- Permissions may have been granted through automated processes, consent workflows, or administrative actions that are not fully visible in current telemetry.
- Distinguishing between permissions that are operationally necessary and those that are excessive or residual requires context that may not be available through read-only collection.

Permission assessment must be capability-aware: if permission data is not accessible, this must be represented explicitly rather than assumed to mean "no excessive permissions."

---

## 8. Relationship / Graph Problem

Non-human identities do not exist in isolation. They participate in relationships:

- An application registration may have one or more associated service principals.
- A service principal may depend on other service principals or applications.
- A managed identity is backed by an Azure resource with its own identity and access configuration.
- An agent identity may inherit permissions from or be associated with other identities.
- Ownership, sponsorship, and manager relationships link non-human identities to human accountability subjects.

These relationships form an identity graph that is essential for understanding:

- Normalized identity relationships across heterogeneous identity types.
- Observed and provenance-backed derived relationships.
- Ownership and accountability relationships where documented.
- Support for deterministic rule evaluation.

Richer security-path analysis (such as blast-radius, privilege-escalation path discovery, or attack-path analysis) may be evaluated as a future direction but is not promised for V1.

The relationship graph is difficult to construct because:

- Not all relationships are surfaced through standard API responses.
- Relationship discovery may require specific, potentially undocumented queries.
- Relationship semantics vary across identity types.
- Derived relationships (computed from observed facts) must be distinguished from directly observed relationships.
- Cross-tenant relationships are explicitly out of scope in V1.

The identity graph must be constructed deterministically from normalized domain data. It must not fabricate edges from display-name matching, AI inference, or undocumented assumptions.

---

## 9. Capability and Authorization Uncertainty

Assessment in Microsoft Entra is bounded by:

- **API capability availability.** Not all Microsoft Graph endpoints or properties are available in all tenants or licensing tiers.
- **Authorization boundary.** The authenticated principal's permissions determine what data is accessible.
- **Licensing tier.** Licensed telemetry features (sign-in logs, audit logs, identity insights) vary by Microsoft Entra license.
- **Implementation scope.** EntraNHI itself may not support all possible data collection in V1.

These boundaries create uncertainty:

- Insufficient permissions may prevent collection of credential metadata, ownership data, or permission relationships.
- Missing licensing may prevent access to identity insights or advanced telemetry.
- API evolution may change endpoint behavior, response schemas, or feature availability across tenants.
- EntraNHI's own implementation may lag behind available API capabilities.

This uncertainty must be represented honestly. The architecture distinguishes:

| Condition | Correct representation |
| --- | --- |
| Data was not accessible due to permissions | Capability state: Unavailable — authorization → NOT_EVALUATED |
| Data was not accessible due to licensing | Capability state: Unavailable — licensing/service → NOT_EVALUATED |
| Data source does not apply to this identity type | Capability state: Not applicable → NOT_APPLICABLE |
| EntraNHI does not yet support this collection | Capability state: Unsupported → NOT_EVALUATED |
| An error prevented collection | Capability state: Failed → ERROR or NOT_EVALUATED |

Missing capability must never silently become PASS. Missing capability must also never automatically become FAIL. The rule definition controls how capability gaps resolve to evaluation states.

---

## 10. Incomplete-Data / False-Confidence Problem

The most dangerous failure mode in security assessment is false confidence — believing the tenant is more secure (or less secure) than the evidence supports.

False confidence can arise from:

- **Silent coercion of missing data to PASS.** If credential data is unavailable and the assessment reports "no expired credentials found," this creates a false impression of credential hygiene.
- **Silent coercion of missing data to FAIL.** If ownership data is unavailable and the assessment reports "no owner," this may be incorrect — an owner may exist but be inaccessible.
- **Collapsing NOT_EVALUATED into PASS or FAIL.** If an assessment cannot evaluate a rule due to missing capability, reporting the rule as satisfied or violated is dishonest.
- **Suppressing ERROR or NOT_EVALUATED in output.** If error states are hidden from the assessment consumer, incomplete assessments appear complete.
- **AI-generated verdicts.** If an AI model determines PASS or FAIL, the assessment is no longer deterministic, auditable, or reproducible.

The architecture addresses this through:

- Five explicit evaluation states that remain distinct.
- Capability-aware rule evaluation.
- Evidence requirements for PASS and FAIL.
- Structured reasons for NOT_EVALUATED, NOT_APPLICABLE, and ERROR.
- Output renderers that cannot alter evaluation semantics.
- AI boundaries that prevent non-deterministic verdict determination.

---

## 11. AI-Agent Identity Evolution

Microsoft Entra agent identities are an emerging identity category. Their characteristics include:

- First-class agent identity resources in Microsoft Entra.
- Relationship to, and inheritance from, associated application and service principal objects.
- Agent blueprints as related application-derived resources.
- Evolving API surface and data model.

The assessment challenges specific to agent identities include:

- API endpoints and data structures may not yet be stable or fully documented.
- Relationship semantics between agent identities, applications, and service principals may be partially defined.
- Agent-specific properties and metadata may be unavailable or undocumented.
- The security implications of agent identity configurations are not yet well-established in community practice.

EntraNHI must handle agent identities with explicit capability awareness:

- Where agent identity APIs are available and documented, collect and evaluate.
- Where agent identity APIs are unavailable or undocumented, report NOT_EVALUATED or NOT_APPLICABLE.
- Never invent Graph endpoints, permissions, or properties for agent identities.
- Never assume agent identity relationships beyond documented and approved semantics.

---

## 12. Evidence and Explainability Problem

Security findings must be explainable. A FAIL verdict without supporting evidence is not actionable. A PASS verdict without evidence may be false confidence.

Evidence and explainability challenges include:

- **Provenance traceability.** Every evidence reference must be traceable to a source observation, normalized fact, and collection context.
- **Evidence sufficiency.** PASS and FAIL both require evidence appropriate to the rule semantics. Insufficient evidence must not be fabricated.
- **Evidence immutability.** Assessment evidence represents a point-in-time observation. Later tenant changes must not alter the meaning of an already-produced finding.
- **Evidence minimization.** Evidence must contain only what is necessary to explain the finding, without exposing raw provider payloads or sensitive data unnecessarily.
- **Secret exclusion.** Evidence and findings must never contain credential material, tokens, or private keys.
- **Output representation.** Findings and evidence are canonical security data. Output renderers present them; they do not recompute or reinterpret evaluation state.

The findings/evidence architecture must support:

- Every evaluation state (PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, ERROR) as an auditable record.
- Provenance references for all observations.
- Structured diagnostics for non-PASS/FAIL states.
- Output formats that preserve evaluation semantics.

---

## 13. Tenant Isolation / Security Boundary Problem

Assessment operates within a tenant context. Tenant isolation is a security property:

- Each assessment targets a single Microsoft Entra tenant.
- Identities, credentials, permissions, and relationships from one tenant must not silently support evaluation of another tenant.
- Cross-tenant service principals must be correctly attributed to the appropriate tenant context.
- Authentication artifacts are transient and must not persist across tenant boundaries.
- Output must be unambiguously scoped to the assessed tenant.

Tenant boundary violations would compromise assessment integrity. The architecture enforces:

- Tenant-scoped assessment context.
- Per-tenant normalized records and graph construction.
- Explicit tenant references in findings, evidence, and output.
- Prohibition on cross-tenant identity correlation in V1.

---

## 14. Operational / Reporting Problem

Security and identity engineers need assessment output that is:

- **Auditable.** Findings must be attributable to specific rules, evidence, and evaluation contexts.
- **Machine-readable.** Integration with CI/CD pipelines, security dashboards, and SIEM systems requires structured output (JSON, SARIF).
- **Human-readable.** Interactive use and report sharing require CLI and HTML output.
- **Reproducible.** Identical inputs should produce identical findings.
- **Honest.** Incomplete assessments must not appear complete. Error states must not be hidden.

The operational challenge is producing output that serves all these needs without:

- Altering evaluation semantics for presentation.
- Fabricating evidence to fill gaps.
- Suppressing error or NOT_EVALUATED states.
- Exposing credential material or sensitive data in output.

---

## 15. Problem Boundaries

This problem statement defines the assessment problem that EntraNHI is designed to address. It does not define:

- Administrative management of non-human identities.
- Credential rotation or lifecycle management operations.
- Automatic remediation of security findings.
- Real-time monitoring or alerting.
- Governance workflow or policy enforcement.
- Azure infrastructure security assessment.
- Microsoft Entra ID Protection or Conditional Access analysis.
- SIEM correlation or security operations center workflows.
- Identity threat detection and response.

These are separate product categories and engineering concerns. EntraNHI focuses specifically on deterministic, read-only, evidence-backed security assessment of non-human identity posture in Microsoft Entra.

---

## 16. V1 Problem Scope

EntraNHI V1 addresses the following problem scope:

| Area | V1 Scope |
| --- | --- |
| **Identity types** | Application registrations, service principals, managed identities, agent identities (where supported by documented capability). |
| **Evidence areas** | Identity metadata, identity type classification, ownership/accountability, credential lifecycle metadata, API/application permissions, identity relationships. |
| **Evaluation model** | Deterministic rule evaluation producing PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR verdicts. |
| **Output** | CLI, JSON, SARIF, HTML. |
| **Authentication** | Interactive/delegated and non-interactive workload modes. |
| **Execution** | Operator-run, single-tenant per assessment, no SaaS or multi-tenant service. |
| **Write operations** | None. Read-only by default. |
| **AI involvement** | None in verdict determination. |

---

## 17. Explicitly Deferred Problems

The following problems are explicitly out of scope for V1 and are deferred:

- **Automatic remediation.** EntraNHI does not fix, rotate, delete, or modify any identity, credential, permission, or policy.
- **Credential rotation.** Credential lifecycle management operations are not performed.
- **Real-time monitoring.** No event-driven analysis, streaming, or alerting.
- **SaaS deployment.** No hosted service, multi-tenancy, or web dashboard in V1.
- **Cross-tenant analysis.** Tenant isolation is enforced; cross-tenant correlation is prohibited.
- **Azure Resource Manager enrichment.** Optional ARM enrichment for managed identity Azure resource context is not required for V1 core assessment. It may be supported as an optional future capability.
- **Third-party identity providers.** Assessment is scoped to Microsoft Entra. Other identity providers are out of scope.
- **Policy enforcement.** EntraNHI assesses posture; it does not enforce or configure policies.
- **Incident response or threat detection.** These are separate security operations concerns.
- **Certification or compliance attestation.** EntraNHI produces findings; it does not certify compliance with specific frameworks.

---

## 18. Success Characteristics

EntraNHI's success is defined by the following characteristics:

1. **Deterministic reproducibility.** Identical inputs produce identical findings across multiple assessment runs.

2. **Evidence-backed verdicts.** Every PASS and FAIL finding carries traceable evidence references. No finding is unsupported.

3. **Honest uncertainty representation.** NOT_EVALUATED, NOT_APPLICABLE, and ERROR are used correctly and remain distinct. Missing data is never silently coerced into PASS or FAIL.

4. **Credential exclusion.** No secret value, token, private key, or recovery code appears in output, logs, or repository at any point.

5. **Capability awareness.** Assessment behavior adapts to what is observable in the target tenant. No undocumented capability dependencies.

6. **No state mutation.** Zero write, delete, or administrative operations against Microsoft Entra or Azure Resource Manager.

7. **Auditable design.** Architecture documentation is sufficient for security review. Evaluation logic is transparent and inspectable.

8. **Machine-readable integration.** Output formats support programmatic consumption by security tooling and automation.

9. **Transparent limitations.** What EntraNHI cannot observe, evaluate, or determine is documented alongside what it can.

10. **Community-scrutinized.** Open-source publication enables adversarial review of security-critical evaluation logic.

---

(End of file)
