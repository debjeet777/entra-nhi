# EntraNHI Differentiation Strategy

> **Status:** Phase 0.2.12-D
> **Date:** 2026-09-18

---

## 1. Title

EntraNHI — Differentiation Through Measurable Engineering Evidence

---

## 2. Purpose

This document defines how EntraNHI intends to earn technical credibility through measurable engineering evidence rather than marketing claims. It establishes the engineering targets against which future implementation and releases can be evaluated, the standards by which differentiation claims must be validated, and the policies governing when a capability may be presented as a differentiator.

This is not a marketing document. It is an engineering decision-support and release-gate document.

---

## 3. Relationship to Product Vision

The product vision defines what EntraNHI is, who it serves, and what it does not claim. This differentiation strategy extends the vision by specifying:

- Which design intentions, if validated through implementation and evidence, would constitute defensible differentiators.
- What evidence is required before any design intention may be presented as a competitive property.
- What measurable engineering targets must be met before release gates are satisfied.

Differentiation claims are downstream of implementation and evidence. The product vision establishes boundaries that differentiation must not violate: no unsupported superiority claims, no design-as-implementation, no competitor ranking.

---

## 4. Relationship to Market Landscape

The market landscape document establishes the competitive benchmark framework against which EntraNHI's engineering decisions are contextualized. This differentiation strategy builds on that framework by:

- Identifying the specific engineering dimensions where EntraNHI's design intent targets differentiation.
- Defining what evidence is required to validate differentiation on each dimension.
- Acknowledging where commercial products and existing open-source projects maintain broader or deeper capabilities.
- Establishing that differentiation must be demonstrated, not asserted.

The market landscape does not rank products. This document does not rank products. Both are engineering decision-support tools.

---

## 5. Differentiation Philosophy

EntraNHI's differentiation philosophy is grounded in the following commitments:

1. **Differentiation through engineering evidence, not marketing language.** A capability becomes a differentiator only when it has been implemented, tested, adversarially challenged, and independently verifiable. Design documentation alone does not establish differentiation.

2. **Focused excellence over feature-count competition.** EntraNHI does not attempt to match the breadth of commercial platforms. It targets depth and rigor in a defined scope: deterministic, read-only, evidence-backed security assessment of non-human identities in Microsoft Entra.

3. **Honest representation of capability boundaries.** Where EntraNHI does not compete — runtime enforcement, broad multi-provider coverage, automatic remediation, real-time monitoring — this is stated explicitly rather than obscured.

4. **Transparent limitations alongside capabilities.** Every differentiator is accompanied by documented limitations, unknowns, and areas where the project has not yet demonstrated the claimed property.

5. **No premature competitive claims.** A capability must not become a public differentiator merely because it exists in design documentation.

---

## 6. Primary Differentiation Thesis

EntraNHI's primary differentiation thesis is:

> A transparent, open-source Microsoft Entra NHI security assessment tool that earns credibility through deterministic, evidence-backed evaluation with explicit handling of what it cannot observe — rather than through breadth, speed, or marketing superlatives.

This thesis rests on the hypothesis that security and identity engineers value:

- **Assessment correctness as a security property** — an assessment that produces false confidence is more dangerous than no assessment.
- **Inspectable logic** — the ability to read, audit, and adversarially challenge every rule and evaluation path.
- **Honest uncertainty** — explicit representation of what was not evaluated and why, rather than silent coercion of missing data into false confidence.

This thesis must be validated through implementation and evidence. It is a design intention, not a claimed achievement.

---

## 7. Differentiation Pillar — Microsoft Entra Specialization

### 7.1 Deliberate Microsoft Entra Focus

EntraNHI is designed to focus exclusively on Microsoft Entra identity semantics in V1. Rather than attempting broad multi-provider coverage, the project targets depth within a single identity platform. This focus is a deliberate scope decision, not a limitation imposed by inability to support other providers.

### 7.2 Application Registrations

EntraNHI is designed to assess application registration configurations, metadata, credential lifecycle, ownership, and permissions. Application registrations are distinct identity types in Microsoft Entra and must remain distinguishable from service principals in assessment.

### 7.3 Service Principals

EntraNHI is designed to assess service principal configurations, metadata, credential lifecycle, ownership, and permissions. Service principals carry distinct security properties from application registrations and require independent evaluation.

### 7.4 Managed Identities

EntraNHI is designed to assess managed identities through their service-principal representation in Microsoft Entra. Managed identities are backed by Azure-managed credentials and carry distinct lifecycle properties. Assessment is intended to operate through documented Graph metadata; optional Azure Resource Manager enrichment is not required for V1.

### 7.5 Documented Agent ID Capabilities

EntraNHI is designed to handle Microsoft Entra agent identities as an emerging identity category. Agent identity support is intended to be capability-aware: where documented Microsoft Graph capabilities are available, agent identities are collected and evaluated; where capabilities are unavailable or undocumented, NOT_EVALUATED or NOT_APPLICABLE is reported. EntraNHI must not invent Graph endpoints, permissions, properties, or relationships for agent identities.

### 7.6 Capability-Aware Evolution

As Microsoft Entra capabilities evolve — particularly around agent identities — EntraNHI is designed to adapt its collection and evaluation behavior to documented capabilities rather than assuming uniform API availability across tenants.

### 7.7 No Undocumented Assumptions

EntraNHI must not depend on reverse-engineered, undocumented, or unsupported Microsoft behavior. All collected data must originate from documented Microsoft APIs. All rule logic must reference documented properties and behaviors. Where documentation is absent, NOT_EVALUATED must be used.

---

## 8. Differentiation Pillar — Assessment Correctness

### 8.1 Deterministic Evaluation

EntraNHI is designed to produce identical evaluation outcomes from identical inputs, capability states, rule configurations, and deterministic execution context. No AI, machine learning, or probabilistic inference determines verdicts. Determinism enables reproducibility, auditability, and adversarial verification.

### 8.2 False-PASS Resistance

The architecture is designed to prevent missing data, incomplete collection, authorization denial, capability gaps, collection failures, and system errors from being silently coerced into PASS. PASS requires affirmative rule conditions satisfied with sufficient input completeness. Missing data must not automatically become PASS.

### 8.3 False-FAIL Resistance

The architecture is designed to prevent authentication failures, authorization denials, capability gaps, system errors, and missing data from producing tenant security FAIL. FAIL requires affirmative deterministic evidence that the rule's failure condition is satisfied. Missing data alone does not produce FAIL. System errors produce ERROR, not tenant FAIL.

### 8.4 Five-State Semantics

Every rule evaluation must resolve to exactly one of PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR. These five states serve distinct semantic purposes and must not be collapsed:

- **PASS** — Rule conditions affirmatively satisfied with sufficient input completeness and evidence.
- **FAIL** — Rule failure condition affirmatively satisfied with traceable evidence.
- **NOT_EVALUATED** — Required capability, data, or input was unavailable; structured reason attached.
- **NOT_APPLICABLE** — Rule does not apply to the identity type or context; structured reason attached.
- **ERROR** — Unexpected evaluation failure; structured reason attached; distinct from tenant security FAIL.

### 8.5 Explicit Assessment Completeness

Assessment output must represent what was evaluated and what was not. Incomplete assessments must not appear complete. NOT_EVALUATED, NOT_APPLICABLE, and ERROR must remain visible in output and must not be suppressed or collapsed into other states.

### 8.6 Failure Transparency

Collection failures, normalization failures, rule evaluation failures, and output failures must be represented explicitly. No failure path may silently produce a successful or security-compliant result. A failure that is silently converted into a passing outcome is an integrity violation.

---

## 9. Differentiation Pillar — Evidence and Provenance

### 9.1 Evidence-Backed PASS/FAIL

Every PASS and FAIL verdict is designed to be accompanied by evidence references sufficient to explain the outcome and trace to the collected data point supporting the verdict. PASS without evidence is not trustworthy. FAIL without evidence is not actionable.

### 9.2 Source Observation Provenance

Normalized observations are designed to retain provenance identifying their documented source and collection context. Provenance enables consumers to validate findings against source data and audit collection methodology.

### 9.3 Normalized-Fact Provenance

Normalized domain objects are designed to carry source identifiers and collection context through every transformation boundary — from source observation through normalization through identity graph through rule evaluation through findings and evidence.

### 9.4 Graph-Derived Provenance

The identity graph is designed as a projection of normalized domain data, not a second source of truth. Every node and edge carries provenance references traceable to source observations. Derived relationships are distinguishable from directly observed relationships.

### 9.5 Explainability

Findings are designed to be explainable through evidence references, provenance chains, and structured reasons for non-terminal verdicts. An assessment consumer should be able to understand why a verdict was produced without requiring access to the source code or the live tenant.

### 9.6 No Fabricated Evidence

The rule engine must not fabricate evidence, invent provenance, or create references to data that was not collected. If required evidence cannot be constructed from available data, the evaluation must not silently produce trustworthy PASS or FAIL.

### 9.7 Evidence Minimization

Evidence must contain only what is necessary to explain the finding. Raw provider payloads, unnecessary metadata, and sensitive data must not be included in evidence unless required for the specific finding explanation.

---

## 10. Differentiation Pillar — Security Architecture

### 10.1 Read-Only Tenant Operation

V1 is designed to perform no write, delete, update, or administrative operations against Microsoft Entra or Azure Resource Manager. Authentication and session mechanics required to obtain authorized read access are not tenant mutation. Report and file creation in the execution environment is not tenant mutation.

### 10.2 Least Privilege

Authentication and authorization are designed to use only permissions required for enabled collection capabilities. Excess permissions must not be requested by default. Permission requirements must be documented per capability.

### 10.3 Tenant Isolation

Each assessment targets a single Microsoft Entra tenant. Data from separate tenant contexts must never be silently mixed. Cross-tenant identity correlation is prohibited in V1. Output must be unambiguously scoped to the assessed tenant.

### 10.4 Credential/Secret Exclusion

Secret values, private key material, client secrets, passwords, bearer/access/refresh tokens, and recovery codes are not assessment-domain data. They must not enter normalized domain data, findings, evidence, reports, logs, or source control. Authentication artifacts remain confined to the runtime authentication boundary.

### 10.5 Provider Isolation

The rule engine is designed to consume only normalized domain data. It must not query Microsoft Graph, Azure Resource Manager, or any other provider API directly. Provider isolation prevents the rule engine from introducing provider-specific coupling, bypassing normalization, or making undocumented API assumptions.

### 10.6 Trust Boundaries

The architecture defines explicit trust boundaries between the authentication boundary, the collector layer, the normalization boundary, the identity graph, the rule engine, the findings/evidence layer, and each output renderer. Each boundary enforces specific invariants.

### 10.7 Fail-Safe Behavior

Where optional behavior could materially weaken assessment integrity, confidentiality, or authorization boundaries, the secure behavior is the default. Weakening secure defaults requires explicit configuration where such configuration is permitted.

### 10.8 Output Security

Output renderers must not alter evaluation semantics, fabricate evidence, or reinterpret verdicts. All five evaluation states must be preserved and distinguishable across all output formats. Provider/tenant-originated strings must be treated as untrusted in rendering.

### 10.9 AI Non-Authority

AI and LLMs cannot determine authoritative assessment outcomes. AI/LLM output must never determine PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR verdicts. If AI is introduced, it must be limited to post-assessment explanation and must not alter verdicts.

---

## 11. Differentiation Pillar — Adversarial Engineering

### 11.1 Security/Adversarial Testing

EntraNHI is designed to include explicit security and adversarial test categories as first-class requirements. Security-critical properties must have explicit test verification. Absence of a test is not evidence of security.

### 11.2 False-PASS Scenarios

The testing architecture is designed to verify that every identified false-PASS attack path — incomplete collection, authorization denial, unsupported capability, collection error, normalization failure, graph failure, resource exhaustion, cancellation, missing evidence, missing provenance — produces the correct non-PASS state.

### 11.3 False-FAIL Scenarios

The testing architecture is designed to verify that every identified false-FAIL attack path — authentication failure, authorization denial, unsupported capability, missing data, system error — produces the correct non-FAIL state.

### 11.4 Malformed Provider Data

The testing architecture is designed to include fuzz and malformed-input testing against parsers, normalizers, serializers, output renderers, and filesystem path handlers. Malformed input must produce graceful failure, not security invariant violation.

### 11.5 Cross-Tenant Contamination

The testing architecture is designed to verify that cross-tenant data mixing does not occur at any pipeline stage. Cross-tenant normalization, cross-tenant graph edges, and cross-tenant evidence correlation must be rejected.

### 11.6 Output Injection

The testing architecture is designed to verify that provider/tenant-originated strings cannot become executable or rendered control content in any output format — HTML/XSS, terminal control-character injection, JSON injection, SARIF injection.

### 11.7 Filesystem Attacks

The testing architecture is designed to verify that path traversal, absolute-path injection, unsafe filenames, symlink/reparse-point hazards, and unsafe overwrite are prevented.

### 11.8 Resource Exhaustion

The testing architecture is designed to verify that pathological input produces visible failure, not silent successful assessment. Resource exhaustion must not produce PASS or a misleadingly successful artifact.

### 11.9 Failure Injection

The testing architecture is designed to include failure-injection testing that verifies correct behavior when components fail — that failures remain visible, produce NOT_EVALUATED or ERROR, and do not silently produce PASS.

### 11.10 Regression Testing

Known security bugs and semantic regressions are designed to become permanent regression tests. Each regression test captures the specific scenario, expected correct behavior, and the invariant it protects.

---

## 12. Differentiation Pillar — Open Verification

### 12.1 Inspectable Rules

All security rules are intended to be inspectable in source code. The evaluation logic for every rule must be readable, auditable, and challengable by any practitioner with access to the repository.

### 12.2 Inspectable Architecture

The architecture — including trust boundaries, provider isolation, normalization boundaries, evaluation states, and evidence/provenance chains — is intended to be documented with sufficient precision for security review.

### 12.3 Inspectable Tests

Test cases, including adversarial and security tests, are intended to be published alongside the source code. The verification strategy for security-critical properties must be inspectable.

### 12.4 Reproducibility

Identical inputs, capability states, rule configurations, and deterministic execution context must produce identical outcomes. Reproducibility enables independent verification without requiring access to the live tenant.

### 12.5 Limitations Published Alongside Capabilities

What EntraNHI cannot observe, evaluate, or determine must be documented alongside what it can. Limitations are not defects to be hidden; they are honest representations of scope.

### 12.6 Community Scrutiny

Open-source publication is intended to enable adversarial review of security-critical evaluation code. Community scrutiny is a verification mechanism, not a marketing strategy.

---

## 13. Differentiation Pillar — Integration and Developer Experience

The following are intended V1 capabilities. They are design intentions and must not be claimed as implemented capabilities until verified through implementation and testing.

### 13.1 CLI

EntraNHI is intended to be deliverable as a command-line tool suitable for interactive use by security and identity engineers.

### 13.2 JSON

EntraNHI is intended to produce machine-readable JSON output suitable for programmatic consumption, CI/CD integration, and security tooling integration.

### 13.3 SARIF

EntraNHI is intended to produce SARIF-format output for integration with security tooling and GitHub code scanning workflows.

### 13.4 HTML

EntraNHI is intended to produce self-contained HTML reports for review, sharing, and archival.

### 13.5 CI/CD Consumption

EntraNHI is intended to produce deterministic, machine-readable output suitable for integration into CI/CD pipelines, security dashboards, and automation workflows.

### 13.6 Deterministic Machine-Readable Output

All output formats are intended to preserve evaluation semantics deterministically. The same assessment must produce identical output across formats where format-specific serialization permits.

---

## 14. Enterprise Evaluation Characteristics

A security-conscious enterprise evaluator should eventually be able to inspect the following evidence:

| Characteristic | Description |
| --- | --- |
| **Threat model** | Structured threat identification covering assets, threat actors, threat categories, mitigations, and residual risks. |
| **Architecture invariants** | Numbered, traceable invariants with RFC 2119 language governing non-relaxable architectural properties. |
| **Permission model** | Documented least-privilege Microsoft Graph permissions required per capability, with rationale. |
| **Rule semantics** | Deterministic rule logic inspectable in source code, with explicit evaluation states and evidence requirements. |
| **Test evidence** | Security, adversarial, false-PASS, false-FAIL, tenant-isolation, and output-injection test results. |
| **Release provenance** | Versioned releases with documented build provenance, dependency inventory, and integrity verification where implemented. |
| **Dependency/supply-chain controls** | Dependency management policies, SBOM where implemented, and build-integrity verification where implemented. |
| **Vulnerability handling** | Documented process for receiving, triaging, and disclosing security-relevant defects. |
| **Limitations** | Honest documentation of what is not observed, not evaluated, and not yet implemented. |
| **Reproducibility** | Evidence that identical inputs produce identical outcomes, with documented deterministic execution context. |

These characteristics represent the standard against which EntraNHI's enterprise readiness will be evaluated. They are design targets, not current achievements.

---

## 15. Individual Practitioner Value

An individual security or identity engineer should be able to use EntraNHI without requiring an enterprise platform, hosted service, or organizational deployment infrastructure.

EntraNHI is designed to provide value to individual practitioners through:

- **CLI-based operation.** No mandatory server-side component. No SaaS deployment required. No multi-tenant service dependency.
- **Local execution.** Assessment runs in a trusted execution environment controlled by the operator. Authentication artifacts are transient and locally managed.
- **Single-tenant scope per execution.** An individual practitioner can assess a specific Entra tenant without cross-tenant complexity.
- **Synthetic/test-data capability.** Tests and examples use synthetic data; no production credentials required for learning or rule development.
- **Machine-readable output.** JSON and SARIF output support integration with individual practitioner workflows and personal security tooling.
- **Inspectable source code.** An individual practitioner can read, audit, and challenge the evaluation logic without requiring vendor trust.

EntraNHI is not designed to replace enterprise platforms, hosted assessment services, or managed security operations. It is designed to be a trustworthy, inspectable, standalone assessment tool that an individual practitioner can operate independently.

---

## 16. Measurable Engineering Targets

The following table defines measurable engineering targets for release-gate evaluation. Targets use qualitative gates where exact numeric thresholds remain established during implementation design.

| Target | Measurement | Required Evidence | Release Gate |
| --- | --- | --- | --- |
| Zero tenant write operations | No write, delete, update, or administrative API calls against Microsoft Entra or Azure Resource Manager | Collector boundary tests; provider-boundary tests; API-call audit in synthetic test execution | All collector and provider-boundary tests must verify zero write operations before release |
| Deterministic repeatability | Identical normalized input + identical capability state + identical rule configuration + identical deterministic execution context produce identical verdict | Rule-engine determinism tests; golden/snapshot tests; property-based tests with seeded generation | Rule-engine determinism tests must pass; golden tests must remain stable across runs |
| Five-state semantic preservation | Every verdict resolves to exactly one of PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR; no state collapse or silent coercion | Rule-state-matrix tests; explicit-evaluation-state tests; output-renderer tests across all formats | Rule-state-matrix tests must pass; all five states must be distinguishable in all output formats |
| False-PASS protection | Every identified false-PASS attack path produces the correct non-PASS state | False-PASS defense tests across full pipeline; adversarial test suite | All false-PASS attack-path tests must pass before release |
| False-FAIL protection | Every identified false-FAIL attack path produces the correct non-FAIL state | False-FAIL defense tests across full pipeline; adversarial test suite | All false-FAIL attack-path tests must pass before release |
| Tenant isolation | No cross-tenant data mixing at any pipeline stage | Tenant-isolation tests; cross-tenant normalization rejection tests; cross-tenant graph-edge rejection tests | All tenant-isolation tests must pass before release |
| Secret exclusion | No credential material enters output, logs, evidence, normalized state, or any persisted artifact | Secret-injection tests; output-format tests; logging tests; adversarial tests with synthetic credential patterns | All secret-exclusion tests must pass before release |
| Evidence traceability | Every PASS/FAIL verdict carries traceable evidence references; every non-terminal verdict carries structured reason | Findings/evidence/provenance tests; evidence-sufficiency tests | Evidence-traceability tests must pass before release |
| Renderer non-authority | Output renderers cannot alter RuleEvaluation state, reinterpret verdicts, or fabricate evidence | Cross-renderer verification tests; output-format tests; all five states preserved in all formats | Cross-renderer verification tests must pass before release |
| AI non-authority | AI cannot determine or modify verdicts; AI availability not required for core assessment correctness | AI-boundary tests; rule-engine determinism tests without AI involvement | Rule-engine determinism tests must pass without AI involvement |
| Malformed-input handling | Malformed, truncated, oversized, or adversarial inputs produce graceful failure, not security invariant violation | Fuzz/malformed-input tests across parsers, normalizers, serializers, output renderers | Fuzz/malformed-input tests must pass with no security-invariant violations |
| Output-injection resistance | Provider/tenant-originated strings cannot become executable or rendered control content in any output format | HTML/XSS tests; terminal-injection tests; JSON-injection tests; SARIF tests | All output-injection tests must pass before release |
| Dependency/supply-chain controls | Dependencies are managed, inventory is documented, build integrity is verifiable | Dependency audit; SBOM where implemented; build-integrity verification where implemented | Dependency management policy must be established; dependency audit must pass |
| Reproducible/repeatable build and test behavior | Identical source produces identical artifacts; identical test inputs produce identical results | Build-reproducibility evidence; test-repeatability evidence | Build must produce deterministic artifacts; test suite must be fully deterministic |
| Documentation/limitation completeness | Every architectural property, evaluation state, and security invariant is documented; limitations are published alongside capabilities | Documentation completeness audit; limitation-documentation review | Documentation must cover all architecture invariants, all evaluation states, and all known limitations before release |

---

## 17. Competitive Gap Handling

### 17.1 Commercial Products May Remain Broader

Commercial NHI security platforms — including Oasis/Cyera, Astrix/Cisco, and Entro Security — cover broader scope than EntraNHI V1: multi-environment discovery, runtime monitoring, threat detection, automatic remediation, behavioral analysis, and governance workflows. EntraNHI V1 does not attempt to match this breadth. This is a deliberate scope decision.

### 17.2 Graph/Security Projects May Remain Deeper

Graph and security-analysis projects — including BloodHound — provide mature identity graph construction, attack-path analysis, and relationship traversal capabilities. EntraNHI V1 does not implement attack-path analysis, blast-radius computation, or privilege-escalation path discovery. These are future considerations, not V1 capabilities.

### 17.3 Runtime IAM Products Serve a Different Problem

Runtime workload identity and access management products — including Aembit — address a fundamentally different problem: policy enforcement, secretless access, and runtime-context-based access decisions. EntraNHI addresses offline/posture assessment. These product categories are related but distinct.

### 17.4 Why EntraNHI Should Not Imitate Every Competitor Capability

Imitating every competitor capability would dilute focus, expand scope beyond V1 readiness, and prevent the project from achieving depth in its chosen differentiation pillars. Feature-count competition is not a viable strategy for an open-source project with limited resources and a commitment to engineering rigor.

### 17.5 Focused Excellence Versus Feature-Count Competition

EntraNHI's strategy is focused excellence: achieve measurable, verifiable quality in a defined scope rather than attempting to match the feature count of commercial platforms. Differentiation is earned through correctness, transparency, and evidence — not through breadth.

---

## 18. Proof Before Claim Policy

A capability must not become a public differentiator merely because it exists in design documentation. The following lifecycle governs when a capability may be presented as a differentiator:

```
Design
  -> Implementation
    -> Automated Test
      -> Adversarial Test
        -> Evidence
          -> Independent Review
            -> Release
              -> Claim
```

### Lifecycle stages

| Stage | Description |
| --- | --- |
| **Design** | Architecture documents define the intended property, invariant, and constraint. Design is DESIGNED / REQUIRED. |
| **Implementation** | Code implements the designed property. Implementation status is IMPLEMENTED but not yet verified. |
| **Automated Test** | Automated tests verify the implemented property against defined scenarios. Test results constitute evidence. |
| **Adversarial Test** | Security and adversarial tests attempt to violate the property. Adversarial results constitute stronger evidence. |
| **Evidence** | Test results, adversarial results, and analysis constitute the evidence base for the property. |
| **Independent Review** | The evidence is reviewed by someone other than the original implementer, or through community scrutiny. |
| **Release** | The property passes release gates defined in section 16. |
| **Claim** | Only after release may the property be presented as a validated differentiator. |

### Rules

1. Design documentation alone does not establish differentiation.
2. Implementation without tests does not establish differentiation.
3. Tests without adversarial testing do not establish strong differentiation.
4. Evidence without review does not establish trusted differentiation.
5. A capability must pass release gates before any public differentiator claim.

---

## 19. Anti-Differentiators / Things We Will Not Optimize For

The following are explicit anti-differentiators — areas where EntraNHI will not attempt to compete or optimize:

| Anti-Differentiator | Reason |
| --- | --- |
| **Maximum integration count** | Breadth of integrations is not a V1 objective. Focused depth within Microsoft Entra is the strategy. |
| **Automatic remediation in V1** | V1 is read-only by design. Remediation is outside V1 scope. |
| **Runtime enforcement** | EntraNHI assesses posture; it does not enforce access policies or intercept runtime decisions. |
| **Broad multi-provider coverage in V1** | V1 is scoped to Microsoft Entra. Multi-provider expansion is a future consideration, not a V1 commitment. |
| **AI-generated verdicts** | AI/LLMs cannot determine PASS, FAIL, or any evaluation state. Deterministic rule logic determines verdicts. |
| **Feature-count competition** | EntraNHI does not attempt to match the feature count of commercial platforms. Focused excellence is the strategy. |
| **Marketing superlatives** | EntraNHI does not claim to be the best, most secure, first, only, or unique in any category. |

---

## 20. Long-Term Differentiation Evolution

The following represent long-term directional interests for differentiation evolution. They are explicitly non-committed, contingent on validated design, successful implementation, community adoption, and explicit project approval.

- Extended identity type coverage as Microsoft Entra capabilities evolve, particularly around agent identities.
- Richer identity graph analysis and relationship traversal, potentially including controlled attack-path concepts.
- Optional licensed telemetry integration where documentation and capability validation support it.
- Enterprise-oriented adoption patterns including deeper CI/CD integration and security operations workflow support.
- Potential expansion beyond Microsoft Entra to other identity providers, subject to deliberate design and community need.
- Community-contributed rules and evaluation logic within the security architecture constraints.

These directions are not promises. They are contingent on validated engineering evidence and explicit project decisions.

---

## 21. Success Criteria

EntraNHI's differentiation will be considered demonstrated rather than asserted when the following are true:

1. **Deterministic assessment is verified.** Identical inputs produce identical verdicts across multiple runs, verified by automated determinism tests and golden/snapshot tests.

2. **False-PASS protection is verified.** Every identified false-PASS attack path produces the correct non-PASS state, verified by adversarial test suite.

3. **False-FAIL protection is verified.** Every identified false-FAIL attack path produces the correct non-FAIL state, verified by adversarial test suite.

4. **Evidence traceability is verified.** Every PASS and FAIL verdict carries traceable evidence references; every non-terminal verdict carries structured reason, verified by findings/evidence tests.

5. **Tenant isolation is verified.** No cross-tenant data mixing occurs at any pipeline stage, verified by tenant-isolation tests.

6. **Secret exclusion is verified.** No credential material enters output, logs, evidence, normalized state, or any persisted artifact, verified by secret-injection and adversarial tests.

7. **Renderer non-authority is verified.** Output renderers cannot alter evaluation state, verified by cross-renderer tests across all output formats.

8. **AI non-authority is verified.** AI cannot determine or modify verdicts; the rule engine produces deterministic results without AI involvement, verified by rule-engine determinism tests.

9. **Malformed-input resilience is verified.** Malformed, adversarial, and pathological inputs produce graceful failure, not security invariant violation, verified by fuzz and adversarial tests.

10. **Open inspectability is achieved.** All rules, architecture, tests, and limitations are published and inspectable by the community.

11. **Release gates are satisfied.** All measurable engineering targets from section 16 pass before release.

12. **No unsupported claims are made.** Differentiation claims are limited to verified capabilities. Design intentions are clearly distinguished from implemented properties.

---

## 22. Current Maturity Disclaimer

EntraNHI is in **early-stage design and development.** This differentiation strategy document describes intended engineering differentiators and the evidence standards against which they will be evaluated. It does not:

- Claim any design intention is currently implemented and verified.
- Assert competitive superiority over any named product or project.
- Present design documentation as evidence of differentiation.
- Establish that any measurable engineering target has been achieved.
- Replace independent evaluation of the project's actual capabilities.

Differentiation will be demonstrated through implementation, testing, adversarial verification, evidence, and transparent release — not through design documentation alone.

---

## Appendix A: Document Cross-References

| Document | Relationship |
| --- | --- |
| `docs/00-product-research/product-vision.md` | Defines product vision, principles, and non-claims that differentiation must respect |
| `docs/00-product-research/problem-statement.md` | Defines the assessment problem that differentiation pillars are designed to address |
| `docs/00-product-research/market-landscape.md` | Establishes competitive benchmark framework that differentiation contextualizes |
| `docs/01-requirements/product-requirements.md` | Defines V1 requirements that differentiation pillars derive from |
| `docs/02-architecture/architecture-invariants.md` | Defines architectural invariants that differentiation must satisfy |
| `docs/02-architecture/threat-model.md` | Defines threats that adversarial engineering pillar is designed to address |
| `docs/02-architecture/testing-architecture.md` | Defines testing strategy that measurable engineering targets rely on |

---

(End of file)
