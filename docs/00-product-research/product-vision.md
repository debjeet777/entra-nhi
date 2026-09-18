# EntraNHI Product Vision

> **Status:** Phase 0.2.12-B
> **Date:** 2026-09-18

---

## 1. Title

EntraNHI — Open-Source Security Assessment for Microsoft Entra Non-Human Identities

---

## 2. Purpose

This document defines EntraNHI's product vision: what the project is, who it serves, what it does, and what it explicitly does not claim. It establishes the product boundary for engineering decisions and distinguishes current design intent from long-term direction.

---

## 3. Product Category

EntraNHI belongs to:

- **Identity Security** → Non-Human Identity Security
- **Microsoft Entra Workload & AI Agent Security**
- **Security Assessment / Posture Management**

It is not an identity provider, not a SIEM, not a cloud infrastructure provider, and not a governance or administration platform.

---

## 4. Vision

A transparent, evidence-driven, open-source assessment platform that gives security and identity engineers a reliable, deterministic understanding of non-human identities in Microsoft Entra tenants.

---

## 5. Mission

To provide a trustworthy read-only assessment tool that produces deterministic, auditable, evidence-backed security findings for non-human identities, workload identities, and AI agents in Microsoft Entra — with clear, honest handling of what it can and cannot observe.

---

## 6. Why EntraNHI Exists

Non-human identities represent a growing proportion of identity footprints in enterprise Microsoft Entra tenants. Application registrations, service principals, managed identities, and emerging AI agent identities operate under distinct security assumptions from user identities: they carry credentials or federation configurations, possess API permissions, are often owned by teams rather than individuals, and may persist long after their operational need has ended.

EntraNHI exists to address a specific security-engineering need: deterministic, read-only security assessment that is explainable, auditable, and resistant to false-confidence outcomes. It is designed for security and identity engineers who need to understand the posture of non-human identities without modifying, remediating, or administering the tenant.

---

## 7. Intended Users

| User | Context |
| --- | --- |
| **Security engineers** | Assess non-human identity posture, investigate credential hygiene, identify ownership and accountability gaps, understand permission exposure. |
| **Identity engineers** | Evaluate application registrations, service principals, and managed identities for configuration and lifecycle risks. |
| **Platform / DevOps engineers** | Integrate read-only assessment into CI/CD or automation workflows using machine-readable output. |
| **Security-conscious individual practitioners** | Understand non-human identity posture in personal or lab Entra tenants. |

EntraNHI is not intended for general IT administration, end-user support, or business workflow management.

---

## 8. Core Value Proposition

EntraNHI provides:

- **Read-only security assessment** designed to operate without modifying tenant state.
- **Deterministic verdicts** that are reproducible from identical inputs, not probabilistic or AI-generated.
- **Evidence-backed findings** where every PASS or FAIL carries traceable provenance.
- **Honest representation of uncertainty** through NOT_EVALUATED, NOT_APPLICABLE, and ERROR verdict states — rather than silently coercing missing data into false confidence.
- **Capability-aware collection** that respects licensing, permission, and API boundaries without inventing unavailable data.
- **Normalized NHI and identity graph** that provide a consistent domain model across heterogeneous identity types.
- **Machine-readable output** suitable for integration with security tooling and automation.

---

## 9. Product Principles

1. **Read-only by default.** EntraNHI performs no write, delete, or administrative operations against Microsoft Entra or Azure Resource Manager in V1.

2. **Security-first architecture.** Security constraints are designed into the architecture, not bolted on after implementation.

3. **Deterministic security verdicts.** Every evaluation produces the same outcome from identical inputs. AI, machine learning, and probabilistic inference do not determine verdicts.

4. **Evidence attached to findings.** Every PASS and FAIL verdict is accompanied by evidence references sufficient to explain the outcome.

5. **Explicit handling of unavailable data.** Insufficient authorization, unavailable capability, missing licensed telemetry, partial collection, or provider failure must never silently become a successful assessment outcome.

6. **Least-privilege Microsoft Graph access.** Least privilege is an architectural requirement. Implementation will request only documented read permissions required for enabled capabilities. Exact permission mapping is established during implementation design and validation.

7. **Modular collectors and analyzers.** Collection and evaluation responsibilities are separated. Collectors do not evaluate rules. The rule engine does not query providers.

8. **Machine-readable output.** Assessment results are available in formats suitable for programmatic consumption, not only human presentation.

9. **No AI-generated security verdicts.** AI may eventually assist with explanation, summarization, or documentation, but it never determines PASS, FAIL, or any evaluation state.

10. **Transparent limitations.** The project documents what it cannot observe, what it does not evaluate, and what remains design-only rather than implemented.

---

## 10. Security Philosophy

EntraNHI is designed with the principle that assessment correctness is itself a security property. An assessment tool that produces false confidence is more dangerous than no assessment at all.

Security properties of the design include:

- **Credential exclusion:** EntraNHI must not intentionally collect credential secret values as assessment data. Credential secret values must not enter normalized domain data, findings, evidence, reports, logs, or source control. Authentication artifacts remain confined to the runtime authentication boundary and must not be persisted or exposed through assessment outputs.
- **Tenant boundary preservation:** Assessment operates within a single explicit tenant context. Cross-tenant correlation is prohibited in V1.
- **Provider isolation:** The rule engine consumes only normalized domain data. It never queries Microsoft Graph or any external provider directly.
- **Authorization boundary preservation:** Authentication artifacts are transient, never persisted in source control, logs, or output.
- **No unauthorized state mutation:** No write, delete, or administrative operation is performed against any external system.
- **No undocumented capability dependencies:** Rules reference only documented capabilities and properties. No undocumented Microsoft behavior is assumed.

---

## 11. Assessment-Correctness Principle

Assessment correctness is itself a security property. The following semantics are architectural commitments, not implementation details:

- **Missing data does not automatically mean FAIL.** FAIL requires affirmative deterministic evidence that a rule's failure condition is satisfied. Absence of data is not evidence of failure.
- **Missing data does not automatically mean PASS.** PASS requires sufficient input completeness. The rule definition determines whether absence under verified complete collection is PASS or NOT_EVALUATED.
- **Authorization failure is not automatically a tenant security FAIL.** Insufficient permissions produce NOT_EVALUATED with structured reason, not FAIL.
- **Collection failure must remain visible.** A collection failure that is silently converted into a successful assessment outcome is an integrity violation.
- **NOT_EVALUATED, NOT_APPLICABLE, and ERROR remain distinct.** These states serve different semantic purposes and must not be collapsed.
- **Findings and evidence cannot alter RuleEvaluation truth.** The findings/evidence layer transforms evaluation output for presentation; it cannot recompute, suppress, or upgrade evaluation states.
- **Renderers cannot reinterpret assessment truth.** Output formats are representations. They do not change evaluation semantics.
- **AI cannot establish PASS or FAIL.** AI-generated content is non-authoritative and downstream of deterministic evaluation.

---

## 12. Open-Source Philosophy

EntraNHI is an independent open-source project. It is not affiliated with, endorsed by, or sponsored by Microsoft.

Open-source publication is intended to:

- Provide transparency in assessment logic, rule definitions, and evaluation behavior.
- Enable community review, validation, and adversarial scrutiny of security-critical evaluation code.
- Allow practitioners to understand exactly what is assessed, how, and why.
- Support adaptation to organizational needs through local deployment and configuration.

The project does not open-source for the purpose of building a commercial funnel or creating artificial lock-in. It is designed to be useful as an independent security assessment tool in its own right.

---

## 13. Commercial-Quality Engineering Objective

EntraNHI should be engineered to commercial-quality open-source standards. This includes:

- **Architecture documentation** that is precise, auditable, and sufficient for security review.
- **Test coverage** including deterministic evaluation, capability-gap handling, secret exclusion, and adversarial test cases.
- **Release engineering** with versioning, provenance, and reproducible builds.
- **Clear limitations** documented alongside capabilities.
- **Structured logging** that excludes credential material.

The project demonstrates quality through architecture, implementation, tests, evidence, documentation, release engineering, and transparent limitations — not through marketing language.

---

## 14. Benchmark Philosophy

EntraNHI will study and learn from:

- **Microsoft Entra** as the authoritative platform and reference implementation for the identity types and APIs under assessment.
- **Leading commercial NHI and security products** for understanding product capabilities, assessment patterns, and user expectations.
- **Leading open-source security projects** for transparency practices, engineering rigor, test strategies, and community engagement.
- **Graph and security projects** for relationship modeling, attack-path concepts, and identity graph construction approaches.

EntraNHI must not clone competitors. Research informs design; it does not dictate reproduction. The engineering method is:

1. Research the strongest existing approaches.
2. Identify defensible security and product gaps.
3. Design deliberately.
4. Implement securely.
5. Test and adversarially challenge.
6. Provide evidence.
7. Improve based on validated findings and adoption feedback.

Market and competitor claims are not to be invented in this document or the problem statement. A separate market-landscape.md will contain sourced competitive research.

---

## 15. Long-Term Direction

EntraNHI is early-stage design and development. The following represent long-term directional interests, not implemented or committed features:

- Extended identity type coverage as Microsoft Entra capabilities evolve.
- Additional rule categories addressing emerging NHI security patterns.
- Richer identity graph analysis and relationship traversal.
- Optional licensed telemetry integration where documentation and capability validation support it.
- Potential expansion beyond Microsoft Entra to other identity providers, subject to deliberate design and community need.
- Enterprise-oriented adoption patterns including integration with security operations workflows.

These directions are contingent on validated design, secure implementation, community adoption, and explicit project approval. They are not promises.

---

## 16. Explicit Non-Goals / Claims We Will Not Make

### Non-goals

- Automatic remediation of security findings.
- Write operations against Microsoft Entra or Azure Resource Manager.
- SaaS multi-tenancy or hosted service deployment.
- Production web dashboard or user-facing web interface in V1.
- Autonomous AI-driven security decisions or AI-generated verdicts.
- Replacement of Microsoft Entra administration or governance products.
- Credential rotation or credential lifecycle management operations.
- Real-time monitoring, alerting, or event-driven analysis in V1.

### Claims we will not make

- "100% secure" or equivalent absolute security claims.
- "World's most secure" or "world's best" superlatives.
- "First" or "only" product in any category.
- "Nobody else does this."
- Guaranteed enterprise suitability or compliance certification.
- Capabilities that are not implemented.
- Superiority over named competitors.
- Market leadership or market dominance.
- Current implementation of architecture that remains design-only.

The project should demonstrate quality through engineering practice, not marketing language.

---

## 17. Current Maturity Disclaimer

EntraNHI is in **early-stage design and development.** Architecture documents describe intended design properties and constraints. They do not describe implemented and verified behavior unless explicitly stated.

No security claims, assessment capabilities, or architectural properties should be assumed to be production-ready based solely on the existence of design documentation. The project's maturity will be demonstrated through implementation, testing, evidence, and transparent release — not through design documentation alone.

---

(End of file)
