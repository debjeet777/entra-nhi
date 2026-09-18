# EntraNHI Engineering Methodology

> **Status:** Phase 0.2.12-E
> **Date:** 2026-09-18

---

## 1. Purpose

This document defines how EntraNHI is deliberately engineered from research through release. It enables future contributors, enterprise evaluators, and maintainers to understand how design decisions become verified capabilities and how engineering discipline is maintained across the project lifecycle.

---

## 2. Engineering Philosophy

EntraNHI is engineered with the principle that assessment correctness is itself a security property. An assessment tool that produces false confidence is more dangerous than no assessment. Every engineering decision — from research scoping through release gating — is evaluated against this principle.

The engineering philosophy prioritizes:

- **Correctness over completeness.** A smaller scope with verified properties is preferred over a larger scope with unverified assumptions.
- **Evidence over assertion.** Capabilities are claimed only after implementation, testing, adversarial verification, and review — never from design documentation alone.
- **Transparency over appearance.** Limitations, uncertainties, and incomplete assessments are documented alongside capabilities rather than obscured.
- **Determinism over convenience.** Assessment outcomes are reproducible from identical inputs without probabilistic inference or AI-generated verdicts.

---

## 3. Sources of Engineering Truth

EntraNHI maintains a hierarchy of engineering truth. When sources conflict, higher-ranked sources override lower-ranked sources.

| Rank | Source | Role |
| --- | --- | --- |
| 1 | **Documented Microsoft capabilities** | Authoritative reference for provider semantics, API behavior, endpoint availability, and identity-type definitions. Microsoft documentation defines what is observable. |
| 2 | **Approved product requirements** | Define V1 scope, functional requirements, security requirements, non-functional requirements, output requirements, capability handling, verdict model, and acceptance criteria. |
| 3 | **Architecture and security invariants** | Numbered invariants (INV-01 through INV-16) defining non-relaxable architectural properties. Violations are defects. |
| 4 | **Threat model** | Structured identification of assets, threat actors, threat categories, mitigations, and residual risks. |
| 5 | **Implementation design** | Detailed design specifications deriving from requirements and architecture. |
| 6 | **Tests and evidence** | Automated test results, adversarial test results, and verification artifacts constituting proof of implemented properties. |
| 7 | **Released behavior** | Observable behavior of released artifacts. Released behavior is the ultimate empirical truth about what the tool does. |

**Market research** (market-landscape.md, differentiation.md) informs product direction and competitive context but does not override Microsoft documentation, requirements, architecture invariants, or security design.

---

## 4. Controlled Development Lifecycle

EntraNHI follows a deliberate, sequential engineering lifecycle. Each phase produces artifacts required before the next phase begins.

```
Research
  → Problem Definition
    → Requirements
      → Architecture
        → Threat Modeling
          → Implementation Design
            → Implementation
              → Automated Testing
                → Adversarial / Security Testing
                  → Independent Review
                    → Release Candidate
                      → Release
                        → Adoption Feedback
                          → Reassessment
```

**Phase transitions are gates.** A phase must produce its required artifacts before the next phase begins. Skipping a phase or proceeding without required artifacts is a process violation.

| Phase | Required Artifacts |
| --- | --- |
| Research | Market landscape, competitive benchmark, differentiation strategy |
| Problem Definition | Problem statement defining scope, boundaries, and V1 problem areas |
| Requirements | Product requirements (functional, security, non-functional, output, capability, verdict, acceptance) |
| Architecture | Architecture invariants, system context, domain model, trust boundaries, collection architecture, output architecture, rule engine, findings/evidence, testing architecture, threat model, authentication/authorization |
| Threat Modeling | Threat model with assets, actors, categories, mitigations, residual risks, adversarial analysis matrix |
| Implementation Design | Detailed design specifications for each architectural component (NOT started) |
| Implementation | Source code implementing designed properties |
| Automated Testing | Unit, component, integration, determinism, state-matrix, and golden tests |
| Adversarial / Security Testing | False-PASS, false-FAIL, tenant-isolation, output-injection, malformed-input, secret-exclusion, and failure-injection tests |
| Independent Review | Review of evidence by someone other than the original implementer or through community scrutiny |
| Release Candidate | All release gates satisfied (see section 12) |
| Release | Versioned, tagged, signed release with changelog |
| Adoption Feedback | Issue reports, community feedback, usage experience |
| Reassessment | Periodic review of assumptions, competitive context, and Microsoft platform evolution |

---

## 5. Design Before Implementation

Security-sensitive functionality is specified before coding. This is a deliberate engineering discipline, not an arbitrary process requirement.

Design-before-implementation exists because:

- **Architecture invariants are verifiable only against a design.** INV-01 (read-only), INV-02 (deterministic assessment), INV-03 (provider isolation), INV-09 (secret exclusion), and INV-10 (tenant boundary) define properties that must be designed, not discovered during coding.
- **Threat-driven design produces more resilient implementations.** The threat model identifies attack paths before implementation, enabling architectural mitigations to be designed into the system rather than bolted on after vulnerabilities are found.
- **Requirements traceability requires design.** Each requirement (FR, SEC, NFR, OUT, CAP, VERD) traces to architectural invariants, which trace to implementation design, which traces to implementation and tests. Without design, traceability is broken.
- **Adversarial testing requires defined properties.** False-PASS and false-FAIL attack paths are identified against designed properties, not against implemented code. Design-first enables more comprehensive adversarial test coverage.

---

## 6. Bounded AI-Assisted Engineering

EntraNHI uses AI coding assistants as bounded implementation tools under explicit human control. The following principles govern AI involvement in engineering:

- **AI coding assistants may help implement bounded approved tasks.** AI may assist with writing code that implements specifications derived from approved requirements, architecture, and implementation design. AI operates within the scope of a specific, human-approved task.
- **Architecture and security authority remains with reviewed project specifications.** AI does not establish, modify, or override architecture invariants, security requirements, threat model decisions, or product scope. These are human-reviewed and human-approved.
- **AI must not autonomously expand scope or weaken security boundaries.** AI must not introduce new dependencies, modify security constraints, expand product scope, redesign architecture, or alter evaluation semantics without explicit human approval.
- **No secrets supplied to AI agents.** AI coding assistants operate under the same secret-exclusion policy as all other repository operations. No credential material, tenant data, or sensitive configuration is provided to AI agents.
- **AI-generated implementation requires normal review and testing.** Code produced by AI is subject to the same review, testing, adversarial verification, and evidence requirements as human-produced code. AI origin does not change the verification bar.
- **Model and provider choice is tooling, not product architecture.** The choice of AI model, coding assistant, or LLM provider is a development tooling decision. It does not influence product architecture, evaluation semantics, or security properties.

---

## 7. Change Control

All changes to the EntraNHI codebase and documentation follow controlled change discipline:

- **Small, scoped changes.** Changes are scoped to the minimum necessary to accomplish the approved task. Unrelated refactoring, scope expansion, or architectural redesign within a change is prohibited.
- **Inspect diff before commit.** Every change is reviewed through its diff before being committed. The diff must match the approved scope.
- **Tests and security validation.** Changes to security-critical code require corresponding test updates or additions. Changes affecting architecture invariants require test verification.
- **Signed commits.** Commits are signed to establish provenance and accountability.
- **Clean checkpoints.** Each commit represents a coherent, working state. Partial or broken states are not committed.
- **No silent destructive changes.** Deletion of code, files, configuration, documentation, tests, or architectural artifacts requires explicit approval and explanation of impact and recovery path.
- **Architecture changes require deliberate review.** Changes to architecture invariants, trust boundaries, security requirements, or threat model require explicit design review and approval before implementation.
- **Implementation must not silently redefine requirements.** Code changes must not alter the semantics of requirements, evaluation states, evidence requirements, or capability handling without an approved requirements change.

---

## 8. Security Validation Lifecycle

EntraNHI applies layered security validation throughout development:

| Validation Layer | Scope | Timing |
| --- | --- | --- |
| **Static analysis** | Code quality, type safety, unused imports, formatting consistency | Continuous during development |
| **Unit / component testing** | Individual functions, classes, and components in isolation | During implementation |
| **Integration testing** | Component interactions, pipeline boundaries, data flow correctness | After component implementation |
| **Adversarial testing** | False-PASS attack paths, false-FAIL attack paths, security invariant violations | After implementation, before release |
| **Fuzz / property testing** | Malformed input handling, schema validation, resource-boundary resilience | During adversarial testing phase |
| **Secret scanning** | Detection of credential material in source, output, logs, and artifacts | Before release |
| **Dependency review** | Known vulnerabilities in dependencies, license compliance, supply-chain risk | Before release |
| **Output security testing** | HTML/XSS, terminal injection, JSON injection, SARIF injection, filesystem path traversal | During adversarial testing phase |
| **Tenant-isolation testing** | Cross-tenant data mixing prevention at every pipeline stage | During adversarial testing phase |
| **Failure injection** | Component failure behavior — failures remain visible, produce NOT_EVALUATED or ERROR, never PASS | During adversarial testing phase |
| **Release-gate verification** | All measurable engineering targets satisfied per differentiation.md §16 | Before release candidate |

Controls are claimed only when existing repository evidence proves them. Absence of a control is noted as a gap, not claimed as implemented.

---

## 9. Proof-Before-Claim Lifecycle

No capability may be presented as a verified property until it has completed the full proof lifecycle:

```
Design
  → Implementation
    → Automated Test
      → Adversarial Test
        → Evidence
          → Independent Review
            → Release
              → Public Claim
```

| Stage | Description |
| --- | --- |
| **Design** | Architecture documents define the intended property, invariant, and constraint. Status: DESIGNED / REQUIRED. |
| **Implementation** | Code implements the designed property. Status: IMPLEMENTED — not yet verified. |
| **Automated Test** | Automated tests verify the implemented property against defined scenarios. Test results constitute evidence. |
| **Adversarial Test** | Security and adversarial tests attempt to violate the property. Adversarial results constitute stronger evidence. |
| **Evidence** | Test results, adversarial results, and analysis constitute the evidence base. |
| **Independent Review** | Evidence is reviewed by someone other than the original implementer or through community scrutiny. |
| **Release** | The property passes release gates defined in the measurable engineering targets. |
| **Public Claim** | Only after release may the property be presented as a validated differentiator. |

**Design documentation alone is never evidence of an implemented capability.** Implementation without tests does not establish differentiation. Tests without adversarial testing do not establish strong differentiation. Evidence without review does not establish trusted differentiation.

---

## 10. Competitive Engineering Method

For significant capabilities, EntraNHI follows a deliberate competitive engineering process:

1. **Research relevant platform behavior.** Understand Microsoft Entra documented capabilities, API semantics, and identity-type definitions.
2. **Understand relevant benchmark approaches.** Study how commercial and open-source products address the same problem space.
3. **Identify defensible problem or gap.** Determine where EntraNHI's approach addresses a real, verifiable gap or provides measurably different properties.
4. **Design deliberately.** Produce architecture and implementation design that addresses the gap with defined invariants and testable properties.
5. **Implement.** Write code implementing the designed properties.
6. **Test.** Verify through automated and adversarial testing.
7. **Measure.** Establish measurable engineering targets and evidence thresholds.
8. **Document limitations.** Publish what the capability does not cover and where it may be weaker than alternatives.
9. **Communicate differentiation.** Only after proof-before-claim lifecycle completion may the capability be presented as a differentiator.

Competitors are never ranked. Superiority is never claimed. Differentiation is earned through engineering evidence, not marketing language.

---

## 11. Documentation Discipline

Every engineering decision in EntraNHI must be recoverable from repository history and documentation. The following information must be recoverable:

| Element | Question Answered |
| --- | --- |
| **WHY** | Why was this decision made? What problem does it address? |
| **WHAT** | What was decided? What is the specific design, constraint, or behavior? |
| **HOW** | How is it implemented? What are the mechanics? |
| **SECURITY ASSUMPTIONS** | What security assumptions underpin this decision? What must be true for the design to be correct? |
| **LIMITATIONS** | What does this decision not cover? Where may it be insufficient? |
| **EVIDENCE** | What evidence supports that the implementation matches the design? |
| **DECISIONS** | What alternatives were considered? Why was this approach chosen? |

Documentation is not optional overhead. It is a core engineering artifact that enables review, audit, reproduction, and knowledge transfer.

---

## 12. Release Discipline

EntraNHI intends to maintain enterprise-grade release discipline. The following controls are planned; they are marked as intended until implemented:

| Control | Status | Description |
| --- | --- | --- |
| **Semantic versioning** | Intended | Major.Minor.Patch versioning reflecting scope, capability, and fix changes. |
| **Signed and tagged releases** | Intended | Git-signed tags marking release points with human-readable release notes. |
| **Changelog / release notes** | Intended | Per-release documentation of changes, additions, removals, and known limitations. |
| **Dependency inventory** | Intended | Documented list of direct and transitive dependencies with versions and licenses. |
| **SBOM** | Intended | Software Bill of Materials where adopted by the build toolchain. |
| **Provenance / build evidence** | Intended | Build provenance records linking released artifacts to source commits and build processes. |
| **Vulnerability handling** | Intended | Documented process for receiving, triaging, and disclosing security-relevant defects. |
| **Release gates** | Intended | All measurable engineering targets from the differentiation strategy must pass before release. |
| **Rollback / recovery** | Intended | Documented process for reverting to a previous release if a release introduces regressions. |

None of these controls are claimed as currently implemented. They represent the intended release engineering standard.

---

## 13. Post-Release Learning

After release, EntraNHI incorporates learning from multiple sources:

- **Issues and bug reports.** User-reported defects become prioritized fixes with regression tests.
- **Security reports.** Security-relevant defects receive immediate triage, assessment, and coordinated disclosure.
- **Community feedback.** Practitioner experience informs prioritization of enhancements and usability improvements.
- **Adoption experience.** Real-world deployment patterns reveal integration needs, performance characteristics, and operational concerns.
- **Microsoft platform and API evolution.** Changes to Microsoft Entra capabilities, API behavior, licensing, or identity-type definitions may require reassessment of collection strategy, rule semantics, and capability mappings.
- **Benchmark reassessment.** Competitive landscape changes are tracked and inform future engineering priorities.
- **Regression prevention.** Every confirmed defect becomes a permanent regression test protecting against recurrence.

---

## 14. Engineering Integrity Rules

The following rules are absolute and may not be relaxed:

| Rule | Description |
| --- | --- |
| **Never fabricate evidence.** | Evidence must trace to actual collected data. Invented, assumed, or AI-generated evidence is an integrity violation. |
| **Never hide incomplete assessment.** | NOT_EVALUATED, NOT_APPLICABLE, and ERROR must remain visible. Suppressing or collapsing these states is an integrity violation. |
| **Never convert missing data silently to PASS or FAIL.** | Missing data produces NOT_EVALUATED. It must not be silently coerced into a terminal security verdict. |
| **Never introduce undocumented provider dependency.** | All collected data must originate from documented Microsoft APIs. Undocumented behavior must not be assumed. |
| **Never expose secrets.** | Credential material must not enter findings, evidence, output, logs, or any persisted artifact. |
| **Never allow AI to establish assessment truth.** | AI-generated content is non-authoritative and downstream of deterministic evaluation. AI must not determine or modify verdicts. |
| **Never advertise unverified capabilities.** | Design documentation is not evidence. Capabilities are claimed only after the proof-before-claim lifecycle is complete. |

---

## 15. Current Maturity and Next Engineering Gate

**EntraNHI remains early-stage design and development.**

The research package — problem statement, product vision, market landscape, differentiation strategy, architecture invariants, threat model, and this engineering methodology — establishes the engineering framework for the project. It does not describe implemented and verified behavior.

**The next engineering gate after this research package is implementation design.** Implementation design has NOT started. It will produce detailed component-level specifications deriving from the approved architecture and requirements, and it will be subject to the same deliberate review process as all preceding phases.

No implementation design decisions, component specifications, or code implementations are initiated or implied by this document.

---

## Final Self-Check

- [x] Only `docs/00-product-research/engineering-methodology.md` modified
- [x] No file created or deleted
- [x] No architecture or requirements changed
- [x] No product scope expanded
- [x] No implementation claims invented
- [x] Proof-before-claim lifecycle present (section 9)
- [x] AI engineering boundary present (section 6)
- [x] Security validation lifecycle present (section 8)
- [x] Next phase identified but not started (section 15)
- [x] No commit
- [x] No push

---

**Final line count: ~290 lines**

---

(End of file)
