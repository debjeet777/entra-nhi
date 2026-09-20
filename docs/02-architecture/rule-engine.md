# Deterministic Rule Engine Architecture

> **Status:** Phase 0.2.6
> **Date:** 2026-09-18

---

## Purpose

Define the deterministic EntraNHI V1 rule-engine architecture. The engine evaluates normalized assessment state and produces authoritative security verdicts. It MUST NOT collect provider data itself. It is the authoritative component for deterministic security-rule evaluation. AI/LLM functionality is NOT authoritative for rule outcomes.

---

## 1. Rule engine position in the assessment pipeline

```
Collectors
 -> normalization
 -> normalized domain model
 -> identity graph
 -> deterministic rule engine
 -> rule evaluation
 -> findings/evidence layer
```

The rule engine consumes only data that has already been collected, normalized, and projected into the identity graph. It produces deterministic evaluation outcomes that feed into the findings/evidence layer downstream. The engine never reaches upstream into collectors or providers.

---

## 2. Rule input boundary

### 2.1 Rules MAY consume only

Rules may consume only approved normalized assessment inputs:

- Normalized identities (`IdentityRecord`)
- Normalized relationships (graph edges)
- Identity graph projection (subgraphs, traversals, aggregations)
- Capability states
- Provenance references
- Assessment context
- Deterministic configuration
- Rule definition and version metadata
- Explicitly defined derived values produced by deterministic logic from the above inputs

### 2.2 Rules MUST NOT consume directly

Rules must not consume:

- Microsoft Graph SDK objects
- Raw Graph or provider payloads
- Access or refresh tokens
- Credentials, secrets, client secrets, private keys, or recovery codes
- Live provider API responses
- Undocumented external state
- LLM-generated facts or inferences

### 2.3 Rules MUST NOT call providers

Rules must not call Microsoft Graph, Azure Resource Manager, or any other provider. Rules must not request additional privileges beyond those already used for collection. This is the provider isolation boundary. [INV-03, INV-04]

---

## 3. Explicit evaluation states

The authoritative V1 rule evaluation states are exactly five. No additional terminal states may be created in V1.

### 3.1 PASS

The rule was applicable, required observations and capabilities were sufficiently available, evaluation completed successfully, and the defined failure condition was not satisfied.

PASS is a security-relevant assertion. It requires sufficient input completeness for the specific rule. The engine must not infer PASS merely because no failure record was found. "Nothing found" is PASS only when the rule explicitly establishes that the relevant collection was complete enough for absence to be meaningful.

Each rule must define its PASS semantics, including what constitutes sufficient completeness for a PASS outcome.

### 3.2 FAIL

The rule was applicable, required observations and capabilities were sufficiently available, evaluation completed successfully, and the rule's explicitly defined failure condition was satisfied.

FAIL requires:

- An applicable rule
- Sufficient capability and input state
- A deterministic failure predicate satisfied
- Evidence and provenance references sufficient to explain the failure

A collection, authentication, or system failure does not itself select an assessment state. Operational failures remain explicit and preserve relevant capability, completeness, and failure context. When the condition merely prevents assessment, the applicable evaluation and rule semantics, together with later failure-mapping mechanics, determine the legitimate handling, including whether a `RuleEvaluation` is produced and, if so, its state. A separate rule may explicitly evaluate the operational condition as assessment data under a documented requirement; that rule's normal deterministic semantics govern its state.

### 3.3 NOT_EVALUATED

The rule could not legitimately determine PASS or FAIL because required assessment capability or data was unavailable, unsupported, insufficient, or otherwise not collected as required by that rule.

NOT_EVALUATED must carry structured reason or context sufficient to explain why normal evaluation did not occur. It must identify which required capability or data was missing and the capability state that caused the gap.

### 3.4 NOT_APPLICABLE

The rule does not apply to the evaluated identity or context according to the rule's deterministic applicability criteria.

NOT_APPLICABLE must carry structured reason or context identifying the identity kind, context, and the applicability criterion that determined the rule does not apply.

### 3.5 ERROR

The rule should have been evaluable or evaluation was attempted, but an unexpected engine, data, or runtime failure prevented trustworthy completion.

ERROR must carry structured reason or context sufficient to explain the failure. Credential material must not be exposed in the error context. ERROR must not be converted to PASS or FAIL merely to simplify reporting.

---

## 4. Critical fail-safe semantics

The following fail-safe rules are security-critical and must be enforced by the rule engine architecture:

### 4.1 Missing data MUST NOT automatically become PASS

- Missing data must not automatically become PASS.
- Authorization denial must not become PASS.
- Unsupported capability must not become PASS.
- Collection failure must not become PASS.
- Normalization failure must not become PASS.
- Graph-construction failure affecting required input must not become PASS.
- Rule execution exception must not become PASS.
- Unknown state must not be coerced into a secure value.

### 4.2 Missing data MUST NOT automatically become FAIL

- Missing data must not automatically become FAIL.
- FAIL requires affirmative deterministic evidence that the rule's documented failure condition is satisfied.

This distinction is security-critical. Missing data is not evidence of failure; it is an absence of evidence. The rule definition and verified collection completeness determine the legitimate evaluation behavior; missing or incomplete evidence cannot authorize PASS or automatically produce tenant-security FAIL.

### 4.3 ERROR is not FAIL

ERROR represents failure of trustworthy rule execution, not a security failure of the assessed identity. ERROR must not be converted to FAIL merely to simplify reporting.

After a valid rule/target evaluation context has been established, examples of conditions that may produce ERROR according to rule semantics include:

- Rule execution exception
- Internal deterministic engine failure

Invalid shared normalized state, inconsistent graph state, tenant-context mismatch, or invalid configuration that prevents trustworthy evaluation-context construction is an integrity/construction failure. Such a failure must be visible, must produce no fabricated `RuleEvaluation`, and is not automatically mapped to any of the five evaluation states. Exact fatal-versus-isolated handling remains TBD.

---

## 5. PASS evidence requirement

A PASS is a security-relevant assertion and therefore requires sufficient input completeness for the specific rule.

Each rule must define:

- Applicability requirements
- Required capabilities
- Required observations and relationships
- Failure condition
- PASS condition or deterministic complement where valid
- Evidence requirements
- Behavior when required inputs are unavailable
- Behavior when evaluation fails

The engine must not infer PASS merely because no failure record was found. "Nothing found" is PASS only when the rule explicitly establishes that the relevant collection was complete enough for absence to be meaningful.

---

## 6. FAIL evidence requirement

FAIL requires:

- Applicable rule
- Sufficient capability and input state
- Deterministic failure predicate satisfied
- Evidence and provenance references sufficient to explain the failure

A collection, authentication, or system failure does not itself select an assessment state. Operational failures remain explicit and preserve relevant capability, completeness, and failure context. When the condition merely prevents assessment, the applicable evaluation and rule semantics and later failure-mapping mechanics determine the legitimate handling, including whether a `RuleEvaluation` is produced and, if so, its state. A separate rule may explicitly evaluate the operational condition as assessment data under a documented requirement; that rule's normal deterministic semantics govern its state. A failure that prevents evaluation-context construction produces no fabricated `RuleEvaluation`.

---

## 7. Rule definition contract

Each rule is defined by a conceptual `RuleDefinition`. Do not define concrete C# types in this phase.

### 7.1 Required rule definition fields

| Field | Description |
| --- | --- |
| **Rule ID** | Stable, machine-readable unique identifier. See [Rule identity and versioning](#8-rule-identity-and-versioning). |
| **Rule version** | Version identifier for this rule definition. Behavior changes require version awareness. |
| **Title** | Human-readable rule name. |
| **Description** | What the rule checks and why it matters. |
| **Security rationale** | The security justification for the rule's existence. |
| **Target identity and context kinds** | Which identity types and assessment contexts this rule applies to. |
| **Deterministic applicability predicate** | Logic that determines whether this rule applies to a given identity and context. |
| **Required capabilities** | The collection capabilities this rule requires to be evaluable. |
| **Required normalized inputs** | The specific normalized inputs and graph relationships this rule requires. |
| **Deterministic evaluation predicate** | The core evaluation logic. |
| **PASS semantics** | What constitutes a PASS for this rule, including input completeness requirements. |
| **FAIL semantics** | What constitutes a FAIL, including the specific failure condition. |
| **Unavailable-input behavior** | How the rule behaves when required inputs are unavailable. |
| **Evidence requirements** | What evidence references must accompany a PASS or FAIL outcome. |
| **Severity metadata** | Severity classification for FAIL findings where applicable. Does not alter evaluation logic. See [Severity boundary](#17-severity-boundary). |
| **References and documentation metadata** | Links to supporting documentation, standards, or threat rationale. |
| **Deprecation and supersession metadata** | Where a rule is deprecated or superseded, which version supersedes it. |

### 7.2 Rule definition principles

- Rules must not silently change historical meaning without version and change traceability.
- A rule definition must be self-contained enough for an evaluator to understand its applicability, inputs, logic, and expected outcomes without external undocumented assumptions.
- Do not invent concrete security rules in this phase.

---

## 8. Rule identity and versioning

### 8.1 Rule IDs

Rule IDs must be stable and machine-readable. They must not change when a rule's title, description, or documentation is updated.

### 8.2 Versioning

Rule behavior changes must be version-aware. An assessment must be able to identify which rule definition and version produced an evaluation.

- Do not prescribe an exact semantic-version format in this phase.
- A rule must not silently change historical meaning without version and change traceability.
- Historical assessment records must be attributable to the specific rule definition and version that produced them.

### 8.3 Stability

- Rule IDs are stable across versions.
- Rule behavior changes produce a new version.
- Deprecated rules retain their ID and are marked with supersession metadata.
- Rule removal is a deprecation, not an ID reassignment.

---

## 9. Determinism

### 9.1 Deterministic evaluation requirement

Given the same:

- Normalized inputs
- Graph projection
- Capability states
- Deterministic configuration
- Rule definition and version
- Relevant deterministic execution context

the rule must produce the same semantic evaluation outcome.

### 9.2 What determinism does not require

Determinism does not require:

- Byte-identical report serialization
- Identical timestamps in output
- Identical live tenant observations
- Identical performance characteristics

### 9.3 Prohibited non-determinism

- Non-deterministic external calls are prohibited during rule evaluation.
- Current wall-clock time must not be read arbitrarily inside rule logic.
- If time-dependent policy is eventually required, an explicit assessment-time reference must be supplied as deterministic input and recorded in assessment context and evidence.
- Randomness must not influence rule outcomes.
- AI or LLM inference must not influence rule outcomes.

---

## 10. Rule engine pipeline

The rule engine operates through conceptual stages. These are architectural stages, not implementation classes.

### 10.1 Rule discovery and registration

Rules are discovered and registered into the active rule set. Each registered rule has a stable ID, version, and definition.

### 10.2 Rule compatibility and version validation

Before evaluation, the engine validates that each rule definition is compatible with the current engine version and that its version metadata is consistent with the assessment configuration.

### 10.3 Applicability evaluation

The engine evaluates each rule's deterministic applicability predicate against the target identity and assessment context. Rules that do not apply produce NOT_APPLICABLE.

### 10.4 Required-capability validation

The engine checks whether the required capabilities for each applicable rule are available according to the current capability state. This step determines whether the rule can be meaningfully evaluated.

### 10.5 Required-input validation

The engine checks whether the required normalized inputs and graph relationships for each applicable rule with sufficient capabilities are present and valid.

### 10.6 Deterministic evaluation

For rules that are applicable, have sufficient capabilities, and have sufficient inputs, the engine executes the deterministic evaluation predicate.

### 10.7 Evaluation-state assignment

The engine assigns exactly one evaluation state: PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR.

### 10.8 Evidence reference construction and handoff

The engine constructs evidence and provenance references supporting the evaluation outcome. Evidence is handed off to the findings/evidence layer downstream.

### 10.9 Diagnostic and error capture

Structured diagnostic information is captured for every evaluation, including reasons for NOT_EVALUATED, NOT_APPLICABLE, and ERROR outcomes.

### 10.10 Result aggregation

Evaluation results are aggregated into a collection of rule evaluations for the assessment. The aggregation preserves per-rule evaluation details, evidence references, and diagnostic context.

---

## 11. Capability-aware evaluation

### 11.1 Rules declare required capabilities

Each rule declares the capabilities it requires to be evaluable. The engine evaluates those requirements before executing security predicates.

### 11.2 Capability-aware evaluation behavior

If a required capability is unavailable, the engine must not evaluate security predicates that depend on it. Examples:

- If credential metadata capability is required but unavailable, do not evaluate credential absence as PASS.
- If accountability capability is unavailable, do not interpret zero observed owners as confirmed no-owner state.
- If AgentIdentity capability is unsupported, the rule's deterministic applicability predicate independently determines whether it applies; for an applicable rule, documented unavailable-input semantics determine the evaluation state.

### 11.3 No universal capability-to-state mapping

Do not create one universal mapping from every capability failure to one rule state. The rule definition controls deterministic state semantics within architecture constraints. Different rules may handle the same capability gap differently based on their applicability predicates and evaluation logic.

---

## 12. Applicability vs availability

Keep these distinct:

| Concept | Meaning | Result |
| --- | --- | --- |
| **NOT_APPLICABLE** | The rule logically does not apply to this identity or context. | The rule's applicability predicate returned false. |
| **NOT_EVALUATED** | The rule applies or may apply, but sufficient assessment information or capability is unavailable. | The rule could not be meaningfully evaluated. |

Do not use NOT_APPLICABLE to hide unsupported functionality. Do not use NOT_EVALUATED for a rule that deterministically does not apply.

---

## 13. Rule isolation

### 13.1 Per-rule failure isolation

One rule failure or exception must not corrupt unrelated rule evaluations where safe continuation is possible. The architecture must support:

- Per-rule failure isolation
- Structured diagnostics
- Deterministic aggregation
- Explicit indication of incomplete assessment

### 13.2 Shared-state integrity failures

However, integrity failures affecting shared normalized state may require broader fail-closed behavior. If the normalized domain model or identity graph is corrupted in a way that affects multiple rules, the engine may need to halt evaluation rather than produce potentially incorrect results.

### 13.3 Policy

Exact fatal vs isolated failure policy remains TBD.

---

## 14. Finding vs evaluation

Keep `RuleEvaluation` distinct from `Finding`.

### 14.1 RuleEvaluation

Records the deterministic outcome for a rule, target identity, and context. Every evaluation state (PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, ERROR) must remain representable as a `RuleEvaluation` for auditability. Do not assume only FAIL results need machine-readable evaluation records.

### 14.2 Finding

A security-relevant, reportable representation produced according to the findings/evidence architecture. Findings are produced downstream of the rule engine.

### 14.3 Distinction

The rule engine produces `RuleEvaluation` records. The findings/evidence layer transforms these into `Finding` representations for output. This separation supports format extensibility, audit requirements, and different consumption patterns.

Exact finding emission policy remains TBD for the findings-evidence architecture.

---

## 15. Evidence handoff

### 15.1 Rule evaluation must identify evidence

Rule evaluation must identify evidence and provenance references supporting its outcome. Evidence references must be sufficient to explain why PASS or FAIL was determined.

### 15.2 Rule engine must not

- Store authentication secrets as evidence
- Copy unnecessary raw provider payloads
- Fabricate evidence
- Use LLM-generated text as factual evidence
- Silently omit evidence required to justify PASS or FAIL

### 15.3 Evidence rendering

Evidence rendering belongs downstream in the findings/evidence and output layers. The rule engine produces evidence references; it does not render them into output formats.

---

## 16. Configuration security

Rule configuration is security-sensitive input. The architecture must prevent configuration from silently weakening assessment integrity.

### 16.1 Configuration requirements

- Configuration must be explicit.
- Active rule set and version must be recorded.
- Disabled rules must remain diagnosable in assessment metadata where relevant.
- Invalid configuration must fail visibly.
- Unknown configuration options must not silently change security semantics.
- Security-sensitive defaults should favor assessment integrity.
- Configuration must not enable provider access from rule logic.

### 16.2 Defaults

Where optional configuration could weaken assessment integrity, the secure behavior is the default. Weakening requires explicit configuration where such configuration is permitted. [INV-16]

### 16.3 Format

Exact configuration format remains TBD.

---

## 17. Severity boundary

Severity is metadata associated with security findings and rules.

- Severity must not alter deterministic PASS/FAIL evaluation logic.
- A "critical" rule with insufficient capability is still not automatically FAIL.
- A "low" severity rule with affirmative failure evidence is still FAIL.
- Severity flows into findings and output, not into evaluation logic.

Exact severity taxonomy remains TBD unless already defined by requirements. Do not invent scoring mathematics in this phase.

---

## 18. Rule extensibility

### 18.1 V1 extensibility

Future rules should be addable without redesigning collectors or the normalized core where existing normalized capabilities suffice. The rule engine must support adding new rules by registering new `RuleDefinition` instances.

### 18.2 Extension constraints

Extension must not permit arbitrary untrusted code execution by default. Do not design a dynamic plugin marketplace in V1.

### 18.3 Third-party extension

Any future third-party rule or plugin execution model requires a separate trust, sandboxing, signing, and supply-chain security design. This is outside V1 scope.

---

## 19. AI / LLM boundary

### 19.1 AI may eventually

- Explain deterministic findings
- Summarize evidence already produced
- Assist documentation

### 19.2 AI must never

- Determine PASS or FAIL
- Change rule state
- Supply missing evidence
- Fabricate capability state
- Override ERROR or NOT_EVALUATED
- Alter severity authoritatively
- Execute remediation
- Request additional privilege
- Modify active security policy without explicit controlled configuration

### 19.3 Deterministic rule output remains authoritative

AI and LLM output must never determine PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR verdicts. [INV-13]

---

## 20. Tenant isolation

### 20.1 Single tenant per assessment

Rule evaluation must operate within one explicit tenant assessment context. Each assessment execution targets a single tenant.

### 20.2 No cross-tenant correlation

Rules must not silently correlate identities across tenant contexts. Tenant-context mismatch or contamination is an integrity failure and must fail safely.

### 20.3 Cross-tenant analysis

Cross-tenant analysis is outside V1 unless explicitly designed later with appropriate isolation boundaries. [INV-10]

---

## 21. Resource exhaustion and defensive execution

### 21.1 Defensive execution

Design for defensive execution against unusually large or malformed normalized assessment state. Future implementation must support appropriate:

- Cancellation
- Bounded processing
- Memory and resource safeguards
- Protection from pathological graph traversal
- Deterministic termination behavior where feasible

### 21.2 No silent PASS from exhaustion

A resource-exhaustion condition must not silently produce PASS.

### 21.3 Limits

Do not invent concrete limits in this phase.

---

## 22. Rule engine pipeline summary

The following summarizes the conceptual pipeline:

```
1.  Rule discovery / registration
2.  Rule compatibility / version validation
3.  Applicability evaluation -> NOT_APPLICABLE when the deterministic predicate is false
4.  Required-capability validation
5.  Required-input validation
6.  Deterministic evaluation
7.  Evaluation-state assignment -> exactly one of PASS / FAIL / NOT_EVALUATED / NOT_APPLICABLE / ERROR according to applicable rule semantics
8.  Evidence reference construction / handoff
9.  Diagnostic / error capture
10. Result aggregation
```

Each rule passes through applicable stages. Applicability is distinct from capability and input availability. When a legitimate evaluation context exists, the rule contract determines the appropriate state and structured diagnostic context; integrity/construction failures that prevent such a context produce no fabricated `RuleEvaluation`.

---

## 23. Testability

Future tests must cover at minimum:

- Deterministic repeated evaluation from identical normalized fixtures
- PASS with complete required inputs
- FAIL with affirmative evidence
- Missing capability never produces false PASS
- Missing data never produces false FAIL
- NOT_APPLICABLE semantics
- NOT_EVALUATED semantics
- ERROR semantics
- Per-rule exception isolation
- Shared-state integrity failure
- Tenant mismatch
- Malformed normalized input
- Time-dependent deterministic input handling
- Configuration validation
- AI cannot influence rule state
- Provider API unavailable to rule layer
- Secret material excluded from evidence and results
- Large and pathological graph behavior

Use synthetic fixtures for core tests. Do not create tests in this phase.

---

## 24. Open decisions / TBDs

The following remain TBD and must not be resolved by speculation:

- Concrete `RuleDefinition` type
- Concrete `RuleEvaluation` type
- Rule registration mechanism
- Rule version format
- Severity taxonomy
- Configuration format
- Finding emission policy
- Fatal vs isolated engine failure policy
- Rule dependency model, if any
- Assessment-time representation
- Rule execution ordering
- Parallelism and concurrency
- Performance and resource limits
- Graph query abstraction
- Third-party extension model beyond V1
- Rule signing and integrity model if external rule packs are ever supported

---

## 25. Invariant alignment

This section cross-references architecture invariants defined in `docs/02-architecture/architecture-invariants.md`. Do not modify `architecture-invariants.md`.

| Invariant | Alignment |
| --- | --- |
| INV-02 | Deterministic assessment — same normalized inputs, capability state, and rule version produce same outcome |
| INV-03 | Provider isolation — rule engine MUST NOT query providers directly |
| INV-04 | Normalized domain boundary — rules consume normalized contracts, not provider-specific types |
| INV-05 | Explicit evaluation states — exactly five terminal states; no silent coercion |
| INV-06 | Evidence traceability — PASS/FAIL require evidence references; other states require structured reason |
| INV-07 | Capability awareness — rules declare required capabilities; engine validates before evaluation |
| INV-09 | Secret exclusion — rule evidence and results must not contain secret material |
| INV-10 | Tenant boundary preservation — single tenant per assessment; no cross-tenant identity correlation |
| INV-11 | Provenance preservation — evidence references preserve source and collection context |
| INV-13 | AI non-authority — AI/LLM must never determine or modify rule outcomes |
| INV-14 | Failure transparency — failures remain explicit and do not themselves select an assessment state; applicable semantics determine legitimate handling, including whether a RuleEvaluation results and, if so, its state; a separate documented rule may evaluate an operational condition as assessment data; never silent PASS or tenant-security FAIL |
| INV-15 | No undocumented capability dependency — rules reference documented capabilities and properties only |
| INV-16 | Security-sensitive defaults — configuration defaults favor assessment integrity |

---

## 26. Self-verification

Confirm before finalizing:

1. Only `rule-engine.md` was modified.
2. Nothing was created, deleted, renamed, or moved.
3. Exactly five terminal evaluation states are defined: PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, ERROR.
4. Missing data cannot silently become PASS.
5. Missing data cannot automatically become FAIL.
6. FAIL requires affirmative deterministic evidence.
7. PASS requires sufficient input completeness.
8. ERROR remains distinct from FAIL.
9. NOT_APPLICABLE remains distinct from NOT_EVALUATED.
10. Rules cannot call providers.
11. AI cannot determine or modify outcomes.
12. Tenant isolation is explicit.
13. Configuration weakening is visible and controlled.
14. No actual security rules were invented.
15. No exact Microsoft permissions or endpoints were invented.
16. No Git operations were performed.
