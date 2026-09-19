# Rule Engine Contracts

> **Phase:** 0.3.8
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed conceptual contracts. This document is design output
  only. It authorizes no implementation.
- **Scope:** This document owns the deterministic rule-engine contracts:
  `RuleDefinition` content, the evaluation pipeline stage order and
  stage responsibilities, the `RuleEvaluation` evaluation surface (what an
  evaluation record must convey, not its serialized schema), the
  applicability / capability-validation / input-validation order, rule
  failure-isolation boundaries, tenant-isolation obligations on evaluation,
  deterministic-configuration consumption, rule registration/discovery
  expectations, and rule extensibility/versioning rules — with exactly five
  terminal states.
- **What this document is not:**
  - It is not a catalog of concrete security rules. No concrete rule
    content is defined here.
  - It is not a C# type catalog, interface member list, namespace decision,
    or package selection (CON-003).
  - It does not define finding/emission schemas, evidence serialization,
    error-code catalogs, capability-state vocabulary, domain-type field
    lists, graph node/edge schemas, collector interfaces, authentication
    mechanics, configuration file formats, renderer contracts, or the
    test-scenario catalog. Those belong to their owning documents.
- **Ownership (per `README.md` §§5–6):**
  - This document owns: rule identity/version expectations, rule-definition
    content, pipeline stage order, `RuleEvaluation` evaluation-surface
    requirements, evaluation-order rules, isolation boundaries, and
    registration/discovery expectations.
  - This document references without redefining: the five-state semantics
    (`core-contracts.md` §4), normalized domain types (`domain-types.md`),
    capability states and transition constraints (`capability-model.md`
    §§5–6, §9), graph surface and integrity rules
    (`identity-graph-contracts.md`), `Finding` vs `RuleEvaluation` and
    evidence/provenance schemas (`findings-evidence-schema.md`),
    error/result layers and failure taxonomy (`error-result-model.md`),
    collector envelopes and completeness semantics
    (`collector-contracts.md`), authentication boundaries
    (`authentication-design.md`), configuration surfaces
    (`configuration-design.md`), seam placement
    (`dependency-boundaries.md`), and test scenarios (`testing-seams.md`).
  - Where this document uses a term owned elsewhere (e.g., "the
    `RuleEvaluation` defined here, consumed as evidence input by
    `findings-evidence-schema.md`"), the owner's definition governs.
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed conceptual contracts** owned by this
  document, and (c) **unresolved TBDs** naming the owning later document.
  Nothing herein claims a control is implemented, verified, or enforced.
  Where architecture states `DESIGNED / REQUIRED`, this document preserves
  that classification.
- **Syntax discipline:** Contracts below are conceptual (field tables,
  ordering rules, prohibitions). No concrete C# syntax, member list,
  namespace, or serialization form is normative here. Any future
  illustrative signature is non-normative.

---

## 2. Purpose and authority boundary

The rule engine is the authoritative component for deterministic
security-rule evaluation only (rule-engine architecture §1; INV-02,
INV-05). Authority boundaries are:

1. **Engine authority is evaluation only.** The engine assigns exactly one
   canonical assessment state per rule, per target subject, per assessment
   context, from normalized inputs through the pipeline in §4. It does not
   collect provider data, establish authentication, render output, or
   remediate tenants.
2. **Collectors do not decide PASS/FAIL.** Collectors (per
   `collector-contracts.md`) produce source observations with capability
   state, completeness semantics, provenance, and tenant context. They MUST
   NOT emit verdicts, findings, severity, or evaluation states. A collector
   result indicating "no rows" is an observation fact, never a PASS claim
   (see §6).
3. **Output/rendering layers do not reinterpret verdicts.** Renderers
   project the authoritative `RuleEvaluation` / finding data owned
   downstream. They MUST NOT rerun rules, call providers, request
   privilege, alter evaluation state, reinterpret severity into state,
   fabricate evidence, or suppress ERROR / NOT_EVALUATED into a
   false-secure impression (INV-12; `output-renderer-contracts.md` owns
   projection rules).
4. **AI/LLM components have no assessment authority.** Model output MUST
   NEVER determine, modify, override, or supply PASS, FAIL,
   NOT_EVALUATED, NOT_APPLICABLE, or ERROR; MUST NEVER supply or fabricate
   evidence, capability state, or severity; and MUST NEVER sit on the
   authentication, authorization, collection, or evaluation path. Any future
   AI use is downstream explanation only (INV-13).
5. **Provider-specific SDK/API objects cannot enter rule contracts.** Rules
   consume only normalized assessment inputs (§3.6). Provider SDK types,
   raw provider payloads, HTTP semantics, live API responses, undocumented
   external state, and model-generated facts MUST NOT appear in rule
   definitions, rule inputs, evaluation logic, evaluation records, or
   evidence references (INV-03; INV-04). Provider facts cross the boundary
   only as explicitly normalized capability/provenance data.

---

## 3. Rule contract model

Each rule is defined by a conceptual `RuleDefinition`. No concrete rule
content is defined in this phase. The conceptual contract families below
are implementation-oriented obligations; detailed schemas (field names,
serialization, code identifiers) are TBD (§15).

### 3.1 Rule identity

- Each rule carries a **stable, machine-readable unique identifier**
  (Rule ID) and a **version identifier** for the definition.
- Rule IDs MUST be stable across versions: updating title, description,
  or documentation MUST NOT change the ID.
- Any behavior change MUST be version-aware: the assessment MUST be able
  to attribute each evaluation to the exact rule definition and version
  that produced it.
- Rule removal is a deprecation, not an ID reassignment or ID reuse.
- Exact version-format syntax is TBD (§15, T-02).

### 3.2 Rule metadata

Conceptual metadata carried by each `RuleDefinition`:

| Conceptual field | Obligation |
| --- | --- |
| Title | Human-readable rule name. Non-authoritative for logic. |
| Description | What the rule checks. |
| Security rationale | Why the rule exists. |
| References / documentation metadata | Supporting standards or threat rationale links. No invented provider documentation. |
| Severity metadata | Classification for FAIL findings where applicable. Severity MUST NOT alter evaluation logic (rule-engine architecture §17). Exact taxonomy TBD (§15, T-02). |
| Deprecation / supersession metadata | Where deprecated or superseded, which version supersedes it. |

### 3.3 Rule applicability

- Each rule declares a **deterministic applicability predicate**: the logic
  determining whether the rule applies to a given identity and assessment
  context (target identity kinds, context kinds, documented
  applicability criteria).
- The predicate consumes only identity-kind / context-kind facts and the
  assessment context (§3.7). It MUST NOT depend on capability availability,
  collection completeness, wall-clock reads, randomness, provider calls,
  or model inference.
- Applicability is evaluated before capability validation and input
  validation (§4). A predicate result of "does not apply" yields
  NOT_APPLICABLE with structured reason (§7).
- Applicability MUST NOT be used to hide missing evidence or capability
  gaps: where the rule genuinely applies but required data is missing, the
  outcome is NOT_EVALUATED or ERROR per §6, never NOT_APPLICABLE.

### 3.4 Rule input requirements

Each rule declares the normalized inputs it requires to be evaluable:

- The specific normalized identity facts required (referencing the
  `IdentityRecord` concepts owned by `domain-types.md`; field lists are
  not restated here).
- The specific graph relationships / projections required (referencing the
  node/edge and traversal concepts owned by
  `identity-graph-contracts.md`).
- The required data-availability condition per prerequisite (e.g., observed
  content present, or observed content that may be confirmed-empty under
  the rule's explicit completeness semantics — see §6 and
  `capability-model.md` §9).
- The subject scope (which identity kinds / relationship scopes the
  requirements bind to).
- The unavailable-input behavior: which existing assessment state results
  when a prerequisite is unmet (constrained exclusively by §6; no new
  state may be introduced).

### 3.5 Capability requirements

- Each rule declares the **capabilities it requires** to be evaluable,
  using the capability identifiers and conceptual states owned by
  `capability-model.md` (§§4–5). This document MUST NOT restate the state
  vocabulary normatively.
- Capability validation MUST precede security-predicate execution: a rule
  MUST NOT execute its failure predicate against data whose required
  capability state is unmet (`capability-model.md` §8).
- Unmet required prerequisites yield the correct existing non-verdict
  assessment state — normally NOT_EVALUATED, or NOT_APPLICABLE only where
  the applicability predicate genuinely excludes the subject — with
  structured reason identifying the missing capability/data and its state.
- There is no universal capability-to-state mapping: the rule definition
  selects within the permitted set in §6. Different rules MAY handle the
  same capability gap differently within those constraints.

### 3.6 Normalized identity / graph inputs

Rules MAY consume only approved normalized assessment inputs
(rule-engine architecture §2.1):

- Normalized identities (the `IdentityRecord` concepts owned by
  `domain-types.md`).
- Normalized relationships / identity-graph projections (subgraphs,
  traversals, aggregations owned by `identity-graph-contracts.md`).
- Capability states (owned by `capability-model.md`).
- Provenance references (owned by `findings-evidence-schema.md`).
- Assessment context (§3.7).
- Deterministic configuration (§3.8).
- Rule definition and version metadata (§§3.1–3.2).
- Explicitly defined derived values produced by deterministic logic from
  the above inputs.

Rules MUST NOT consume provider SDK objects, raw provider payloads,
tokens, credentials, secrets, live provider responses, undocumented
external state, or model-generated facts (§2, item 5).

### 3.7 Assessment context

Rule evaluation consumes the assessment identity/context concepts owned
by `core-contracts.md` §5 (referenced, not redefined):

- Tenant assessment context reference (single-tenant scope).
- Assessment identifier.
- Execution-mode context (non-credential; token mechanics excluded).
- Deterministic configuration / rule-set reference (§3.8).
- Collection-window context.
- Assessment-time reference (see §8): the single explicit deterministic
  time input supplied with the scope where any current or future rule
  requires a time comparison. Rules MUST NOT read wall-clock directly.

### 3.8 Deterministic configuration

- Rule evaluation consumes **deterministic configuration**: the active
  rule set and versions, deterministic configuration values affecting
  evaluation, and the assessment-time reference where relevant.
- Configuration MUST be explicit and recorded in assessment metadata
  sufficient to reproduce the semantic outcome from preserved normalized
  inputs.
- Invalid configuration MUST fail visibly. Unknown configuration options
  MUST NOT silently change security semantics. Where optional
  configuration could weaken assessment integrity, the secure behavior is
  the default and weakening requires explicit opt-in (INV-16).
- Configuration MUST NOT enable provider access from rule logic.
- Configuration-surface details (option names, file formats, loading
  mechanics) belong to `configuration-design.md`; this document fixes only
  the consumption obligations above.

### 3.9 Rule evaluation

- For rules that are applicable, have sufficient capabilities, and have
  sufficient inputs, the engine executes the rule's **deterministic
  evaluation predicate**: pure logic over §3.6 inputs producing a
  PASS-or-FAIL determination with evidence references.
- Evaluation MUST be side-effect-free with respect to normalized inputs
  and the graph: rules MUST NOT mutate domain records, graph state,
  capability state, or shared engine state during evaluation.
- Evaluation MUST NOT perform network I/O, provider queries, privilege
  requests, wall-clock reads, randomness, or model inference (§8).

### 3.10 Rule result

Each rule evaluation produces a conceptual `RuleEvaluation` record
conveying, at minimum:

- The Rule ID and rule version that produced it.
- The target subject reference (normalized identity key, tenant-scoped).
- The assessment identifier and tenant assessment context reference.
- Exactly one canonical assessment state (§5).
- For PASS / FAIL: evidence and provenance references sufficient to
  explain the outcome (§9).
- For NOT_EVALUATED / NOT_APPLICABLE / ERROR: structured reason/context
  sufficient to explain the outcome (§§6–7, §9).
- Capability-snapshot reference: which required capability/data state
  grounded the outcome.
- Deterministic configuration / rule-set reference and, where relevant,
  the assessment-time reference value used.

`RuleEvaluation` records every evaluation state — including PASS,
NOT_EVALUATED, NOT_APPLICABLE, and ERROR — as machine-readable audit
data. Only FAIL outcomes MUST NOT be assumed to need records.

### 3.11 Finding production (handoff, not production)

- The engine produces `RuleEvaluation` records. It MUST NOT produce
  `Finding` representations.
- The findings/evidence layer (owned by `findings-evidence-schema.md`)
  transforms evaluations into reportable findings per an explicit emission
  policy, without changing evaluation state.
- The engine's handoff obligation is to supply evaluations with evidence
  and provenance references complete enough for that transformation; it
  MUST NOT pre-apply emission, severity-reinterpretation, or rendering
  decisions.

### 3.12 Evidence / provenance references

- Every evaluation carries evidence references (for PASS/FAIL) or
  structured reason/context (for NOT_EVALUATED / NOT_APPLICABLE / ERROR)
  sufficient to explain why that state was assigned (§9).
- Evidence references MUST point to normalized evidence/provenance
  (normalized facts, graph-derived references with observed-vs-derived
  labeling, capability/completeness context, collection operation/context
  references) — never to raw provider payloads, SDK objects, or
  secret-bearing values.
- Evidence MUST NOT be fabricated, inferred from absence, or supplied by
  model output. Provenance MUST be preserved, not invented.

---

## 4. Canonical assessment states

This document references — and MUST NOT redefine — the five-state
contract owned by `core-contracts.md` §4, carried forward from
requirements (VERD-001–VERD-005) and architecture (INV-02, INV-05;
rule-engine architecture §3):

- **PASS**
- **FAIL**
- **NOT_EVALUATED**
- **NOT_APPLICABLE**
- **ERROR**

Rules:

1. Every rule evaluation resolves to exactly one of the five. No sixth
   assessment state exists in V1. No alias, synonym, sub-state, or
   severity/confidence/emission label is an assessment state.
2. Capability, data-availability, operational-failure, retryability,
   cancellation, and severity vocabularies are consumed *before* or
   *around* state assignment — they are never verdicts and never a sixth
   state (`error-result-model.md` §§3–4; `capability-model.md` §§5–6, §9).
3. Renderer, severity, confidence, risk-score, and finding-emission
   decisions are downstream metadata/policy, not states.

---

## 5. False-PASS prevention

The following deterministic rules govern incomplete or uncertain inputs.
They constrain the §6 transition table; the rule definition selects only
within the permitted outcomes. In every row, silent conversion to PASS is
prohibited (INV-05; INV-14; rule-engine architecture §4).

1. **Missing required capability cannot become PASS.** Where a required
   capability is in a non-`Observed` conceptual condition (not requested,
   authorization-limited, licensing/service-limited, unsupported, failed,
   or indeterminate — vocabulary owned by `capability-model.md`), and the
   rule requires that capability, the outcome MUST be NOT_EVALUATED
   (or NOT_APPLICABLE only where applicability genuinely excludes the
   subject, or ERROR per the failure mapping in `error-result-model.md`
   §4) — never PASS.
2. **Authorization limitation cannot become PASS.** A required-data
   authorization limitation yields NOT_EVALUATED with reason identifying
   the limitation. It MUST NOT automatically yield FAIL either.
3. **Partial collection cannot silently become complete.** Partial required
   data where the available subset is not proven sufficient yields
   NOT_EVALUATED (or ERROR per failure mapping) with reason identifying
   the missing subset and its capability state. Only where the rule's
   explicit semantics prove the available subset sufficient for the
   deterministic outcome is the proven outcome permitted
   (`capability-model.md` R-11/R-12).
4. **Unknown collection completeness cannot become PASS.** Indeterminate
   cause or content (the `Unknown` conceptual condition, conflicting
   observations, unconfirmed emptiness) yields NOT_EVALUATED (or ERROR
   where execution trust is broken) — never PASS. Where in doubt, the
   indeterminate classification governs.
5. **Provider failure cannot become PASS.** Collection, normalization, or
   graph-construction failure affecting required input yields
   NOT_EVALUATED or ERROR per `error-result-model.md` §4 mapping — never
   PASS, never tenant FAIL.
6. **Systemic authentication failure cannot become tenant/rule FAIL.**
   Failure to establish any usable authorized context is systemic
   (failure taxonomy owned by `error-result-model.md`): it is recorded
   assessment-wide and MUST NOT be converted into per-rule PASS or into
   tenant-security FAIL. Dependent rules yield NOT_EVALUATED or ERROR
   with reason, per mapping.
7. **Confirmed-empty input may support PASS only when rule semantics
   explicitly permit it and completeness is proven.** Zero returned
   objects is not universally confirmed-empty
   (`collector-contracts.md` §8). PASS on emptiness requires the joint
   condition: the observation channel worked, the request scope covered
   the category/subject, the source explicitly indicates no value or an
   empty relationship where that distinction is documented and reliable,
   and the rule's explicit completeness semantics authorize treating that
   emptiness as conclusive (`capability-model.md` R-02/R-03). Otherwise
   the outcome is NOT_EVALUATED (or ERROR where execution failed).
8. **NOT_APPLICABLE cannot be used to hide missing evidence/capability.**
   NOT_APPLICABLE requires the applicability predicate to establish
   genuine non-applicability. Missing capability, incomplete inputs, or
   unsupported functionality on a subject the rule covers yields
   NOT_EVALUATED or ERROR — never NOT_APPLICABLE.
9. **ERROR cannot be silently converted to PASS/FAIL.** ERROR represents
   failure of trustworthy execution, not a tenant security verdict. It
   MUST NOT be converted to FAIL to simplify reporting nor to PASS to
   simplify output. ERROR carries structured cause without exposing
   credential material, provider exception types, or raw payloads.
10. **Cancellation cannot become PASS/FAIL.** Cancellation is an
    operational cause (owned by `error-result-model.md` §7), not an
    assessment state. Cancelled scope required by a rule yields
    NOT_EVALUATED (evaluation never meaningfully began) or ERROR
    (termination broke trust/completeness mid-execution) with cancellation
    reason and affected scope — never PASS or FAIL. Downstream stages MUST
    NOT evaluate or render cancelled scope as complete.
11. **Missing data cannot automatically become FAIL either.** FAIL requires
    affirmative deterministic evidence that the documented failure
    condition is satisfied. Missing data is absence of evidence, not
    evidence of failure.

---

## 6. Applicability semantics

Three questions MUST remain distinguishable end to end (evaluation →
finding handling → output projection → diagnostics):

| Question | Meaning | Assessment state |
| --- | --- | --- |
| Rule does not apply to the subject | The rule's deterministic applicability predicate establishes, from identity kind and assessment context, that the rule logically does not cover this subject/context. | NOT_APPLICABLE, with structured reason identifying the identity kind, context, and the applicability criterion applied. |
| Rule cannot be evaluated | The rule applies or may apply, but required capability or data is unavailable, unsupported, insufficient, or otherwise not collected as required by that rule. | NOT_EVALUATED, with structured reason identifying which required capability/data was missing and the capability state that caused the gap. |
| Evaluation encountered an error | The rule should have been evaluable or evaluation was attempted, but an unexpected engine, data, or runtime failure prevented trustworthy completion. | ERROR, with structured cause sufficient to explain the failure without exposing credential material. |

Constraints:

- Do not use NOT_APPLICABLE to hide unsupported functionality; do not use
  NOT_EVALUATED for a rule that deterministically does not apply.
- The permitted capability-to-assessment transitions constraining this
  section are owned by `capability-model.md` §9 (R-01–R-14); this document
  enforces them and MUST NOT restate a competing mapping.
- The operational-failure-to-state mapping (which failures are scoped gaps
  vs trust-breaking execution failures) is owned jointly with
  `error-result-model.md` §4; this document MUST NOT introduce a
  simplistic "all exceptions map alike" rule.

---

## 7. Determinism contract

For identical normalized inputs, graph projection, capability states,
deterministic configuration (including rule definitions/versions),
provenance, and assessment-time reference, the engine MUST produce the
same semantic evaluation outcome (INV-02; VERD-006).

1. **Stable rule ordering where observable.** Evaluation order MUST NOT
   affect verdicts. Observable aggregation order MUST be deterministic
   for identical inputs (a stable, documented ordering over rule identity
   and subject key). The exact ordering key and concurrency posture are
   TBD (§15, T-04); what is fixed here is the guarantee, not the key.
2. **Stable finding-identity strategy at the contract level.** The engine
   MUST supply stable evaluation-identity inputs for each evaluation
   (Rule ID/version, tenant-scoped subject key, assessment identifier)
   sufficient for the downstream stable finding/evidence identity strategy
   to be deterministic. Identifier formats, fingerprint/dedup strategy,
   and hashing/signing posture are owned by
   `findings-evidence-schema.md`; this document requires only the stable
   inputs.
3. **No ambient wall-clock dependence.** Rule logic MUST NOT read the
   current wall-clock time. Where time-dependent policy is required, the
   explicit assessment-time reference (§3.7) is supplied as deterministic
   input and recorded in assessment context and evidence. Presentation
   timestamps that do not affect evaluation MAY differ between runs.
4. **No random verdict generation.** Randomness MUST NOT influence rule
   outcomes, ordering, or evidence selection.
5. **No network/provider calls from rule evaluation.** Rules MUST NOT
   call providers, request privilege, or depend on live-service
   reachability (INV-03).
6. **No mutable global state affecting verdicts.** Evaluation MUST NOT
   read or write ambient singletons, static clients, implicit contexts,
   hidden caches, or cross-evaluation mutable state. All provider-derived
   content arrives as explicit normalized parameters (§3.6).
7. **Deterministic handling of collections/order where relevant.** Where
   rule semantics depend on a collection (e.g., existence checks,
   counting, traversal), the contract MUST define deterministic treatment
   of duplicates, ordering, and multiplicity so identical normalized sets
   yield identical outcomes regardless of incidental input order. Graph
   multiplicity/dedup specifics are owned by
   `identity-graph-contracts.md`; this document requires deterministic
   consumption.
8. **What determinism does not require:** byte-identical report
   serialization, identical presentation timestamps, identical live tenant
   observations, or identical performance characteristics
   (rule-engine architecture §9.2).

---

## 8. Evidence requirements

1. **PASS and FAIL MUST be evidence-backed** (INV-06). Each MUST carry
   evidence and provenance references sufficient to explain why the state
   was assigned: which normalized facts satisfied (or did not satisfy)
   the failure condition, which completeness condition grounded a PASS,
   and the provenance chain (source system, object reference, collection
   operation/context, assessment context) preserved across transformation
   boundaries. Provenance MUST NOT be fabricated.
2. **Non-verdict states MUST retain structured reason/context.**
   NOT_EVALUATED carries the missing capability/data identity and its
   capability state; NOT_APPLICABLE carries the identity kind, context,
   and applicability criterion; ERROR carries the failure category and
   sanitized diagnostic chain sufficient to distinguish it from transport
   details.
3. **Findings MUST reference normalized evidence/provenance rather than
   raw provider payloads.** Evidence references point to normalized facts
   and graph-derived references (with observed-vs-derived labeling),
   capability/completeness context, and collection references — never to
   SDK objects, raw payloads, HTTP semantics, or secret-bearing values.
   Evidence rendering belongs downstream; the engine produces references.
4. **Evidence/provenance record schemas**, identifier formats,
   serialization, emission policy, fingerprint strategy, redaction hooks,
   and integrity posture are owned by `findings-evidence-schema.md`; this
   document requires only that engine output be sufficient input to those
   schemas.
5. **Severity MUST NOT alter evaluation logic** and MUST NOT substitute
   for evidence.

---

## 9. Tenant isolation

1. Rule evaluation MUST operate within one explicit tenant assessment
   context. Each assessment execution targets a single tenant (INV-10).
2. The engine MUST NEVER combine subjects, relationships, evidence,
   capability observations, or findings across tenants: no cross-tenant
   normalization, no cross-tenant graph edges, no cross-tenant evidence
   correlation, no cross-tenant capability influence, and no cross-tenant
   artifact mixing in V1.
3. Every evaluation, evidence reference, capability observation consumed,
   and diagnostic emitted MUST carry the tenant assessment context
   reference; tenant-context mismatch or contamination is an integrity
   failure (layer D per `error-result-model.md` §3) and MUST fail safely
   and visibly — halting the affected scope rather than producing
   potentially incorrect verdicts. Cross-tenant contamination is an
   integrity failure, not a normal verdict.
4. Cross-tenant analysis is outside V1 unless explicitly designed later
   with appropriate isolation boundaries.

---

## 10. Rule failure isolation

Four failure scopes MUST remain distinct (layers owned by
`error-result-model.md` §3; mapping owned by §4 there and
`capability-model.md` §9):

1. **Rule-specific evaluation failure.** An exception, invalid normalized
   input for that rule's scope, or inconsistent graph state affecting only
   one rule's evaluation yields ERROR (or NOT_EVALUATED where the mapping
   so determines) for that rule with structured cause. Where safe
   continuation is possible, unrelated rules continue; aggregation
   preserves the per-rule failure visibly with an explicit incompleteness
   indication.
2. **Subject-specific inability to evaluate.** A missing, malformed, or
   capability-gapped input attributable to one subject yields
   NOT_EVALUATED or ERROR for evaluations over that subject only, with
   reason. Other subjects are unaffected; aggregation preserves the
   per-subject gap visibly.
3. **Systemic engine failure.** Integrity failures affecting shared
   normalized state — corrupted shared domain model or graph state,
   tenant-context mismatch/contamination, invalid rule-set definition or
   incompatible rule version, or fabrications detected — MUST fail closed:
   halt evaluation over the affected scope rather than produce potentially
   incorrect results, recording an assessment-level diagnostic. Such
   failures MUST NOT be caught and converted into PASS/FAIL for the
   affected scope.
4. **Cancellation / resource exhaustion.** Deliberate early termination or
   bound exhaustion affecting a scope yields visible incompleteness per
   §5 item 10 and §14: affected evaluations record NOT_EVALUATED or ERROR
   with reason; aggregation preserves the incomplete scope; downstream
   stages MUST NOT evaluate or render the scope as complete.

One malformed rule or input MUST NOT silently corrupt unrelated results.
The exact fatal-vs-isolated policy table is TBD (§15, T-05), constrained
by this section and `error-result-model.md` §§4, 12–13.

---

## 11. Rule registration / discovery boundary

Deterministic registration/discovery expectations (mechanism-neutral):

1. Rules are discovered and registered into an **explicit, closed active
   rule set** before evaluation. Each registered rule has a stable ID,
   version, and definition (§3).
2. Before evaluation, the engine validates **rule compatibility and
   version consistency**: each definition is compatible with the current
   engine version and its version metadata is consistent with the
   assessment configuration. Incompatible or invalid definitions fail
   visibly; they MUST NOT be silently skipped into a false-complete
   assessment.
3. The active rule set and versions MUST be recorded in assessment
   metadata. Disabled rules MUST remain diagnosable in assessment metadata
   where relevant.
4. The registered set MUST be fixed for the assessment run: no rule may
   be added, removed, or re-versioned mid-evaluation in a way that makes
   ordering or outcomes non-deterministic.
5. No concrete mechanism is selected here — no commitment to reflection,
   DI scanning, source generation, plugins, or any other discovery
   implementation. Extension MUST NOT permit arbitrary untrusted code
   execution by default; no dynamic plugin marketplace is designed in V1
   (rule-engine architecture §18). Any future third-party rule execution
   model requires a separate trust, sandboxing, signing, and supply-chain
   design outside V1 scope. Mechanism selection is TBD (§15, T-01).

---

## 12. Extensibility / versioning

1. Future rules MUST be addable by registering new `RuleDefinition`
   instances without redesigning collectors or the normalized core where
   existing normalized capabilities suffice.
2. Adding a rule MUST NOT change canonical state semantics (§4), bypass
   evidence/capability requirements (§§6, 8), weaken the authority
   boundaries (§2), or introduce a sixth state.
3. Rule IDs remain stable across versions; behavior changes produce a new
   version; deprecated rules retain their ID with supersession metadata;
   removal is deprecation, never ID reassignment or silent deletion.
4. Historical assessment records MUST remain attributable to the specific
   rule definition and version that produced them; rules MUST NOT silently
   change historical meaning without version and change traceability.
5. Severity-taxonomy, version-format, and rule-signing/integrity choices
   (if external rule packs are ever supported) are TBD (§15, T-02); no
   scoring mathematics or signing mechanism is invented here.

---

## 13. Secret exclusion

Rule inputs, evaluation records, findings handoff data, evidence
references, reason/context payloads, diagnostics, and logs MUST NOT
contain (INV-09; SEC-001; SEC-002):

- Passwords, client secrets, or recovery/backup codes.
- Bearer, access, or refresh tokens; authentication cookies or session
  tokens.
- Private keys, certificate private-key material, or production signing
  material.
- Any secret-bearing provider payload, header, or credential-bearing
  configuration value.

Transient authentication material is runtime-only and MUST NOT be
normalized into domain data, graph state, evaluation records, evidence,
or persisted artifacts. ERROR and diagnostic contexts MUST describe
failures without exposing credential material. Secret-exclusion test
obligations belong to `testing-seams.md`.

---

## 14. Resource boundedness

1. Rule execution MUST have explicit bounded behavior: bounded processing
   over normalized sets and graph projections, bounded traversal depth
   consumption, and deterministic termination behavior where feasible.
   Defensive execution against unusually large or malformed normalized
   state is required (rule-engine architecture §21).
2. Resource exhaustion (memory, traversal, input-size, or time-bound
   exhaustion) MUST fail visibly — affected evaluations record
   NOT_EVALUATED or ERROR with reason identifying the bound and scope,
   and aggregation preserves the incompleteness. **Resource exhaustion
   MUST NEVER produce PASS by truncation**, nor tenant FAIL.
3. Partial processing MUST NOT silently become a complete assessment:
   truncated input evaluated as if complete is prohibited.
4. No numeric limit, timeout value, concurrency bound, or page/record
   threshold is chosen here. Exact bounds, where they constrain rule
   evaluation or graph consumption, are TBDs owned by
   `identity-graph-contracts.md` (traversal bounds) and this document's
   execution-bounds TBD (§15, T-04), resolved without redesigning the
   normalized core.

---

## 15. Implementation TBDs

Explicitly unresolved decisions. Each names the owning later design
responsibility. None is resolved here by speculation.

| ID | Unknown | Why unresolved at this stage | Owner |
| --- | --- | --- | --- |
| T-01 | Concrete `RuleDefinition` / `RuleEvaluation` code shapes, member lists, namespaces, and registration/discovery mechanism (including any DI, reflection, source-generation, or plugin choice). | Requires interface-catalog and seam-placement decisions owned downstream; no implementation mechanism is approved yet. | `rule-engine-contracts.md` detailed schema work with `dependency-boundaries.md` (seam catalog) and `core-contracts.md` (shared surface). |
| T-02 | Rule version format, severity taxonomy and any scoring mathematics, configuration-surface syntax/format, finding-emission policy, and rule signing/integrity model if external rule packs are ever supported. | Each requires a dedicated design choice with traceability; inventing formats or mathematics here would pre-empt owning documents. | Version/severity: this document's future schema detail; configuration format: `configuration-design.md`; emission policy and signing: `findings-evidence-schema.md`. |
| T-03 | Assessment-time representation (type, resolution, offset handling) and staleness semantics. | Requires domain-type and collection-window decisions; no time-dependent V1 rule content is approved. | Representation: `domain-types.md`; mechanics: this document with `core-contracts.md` §3.11 position. |
| T-04 | Rule execution ordering key, parallelism/concurrency posture, graph-query abstraction, and numeric resource/time bounds. | Requires graph-contract, error-model, and seam-catalog coordination; numeric constants require validation, not guessing. | Ordering/concurrency/query: this document with `identity-graph-contracts.md`; bounds: this document §14 with `error-result-model.md`; enforcement seams: `dependency-boundaries.md`. |
| T-05 | Fatal-vs-isolated engine failure policy table and per-category operational-failure-to-state mapping detail. | Requires the failure-taxonomy mapping owned jointly with the error model; a simplistic universal mapping is prohibited. | This document §10 with `error-result-model.md` §§4–5 (§20, T-02 there) and `capability-model.md` §9. |
| T-06 | Stable evaluation/finding identifier formats and serialization forms. | Identifier and serialization choices require schema-finalization review. | `findings-evidence-schema.md` (identity, fingerprint, serialization). |
| T-07 | Any Microsoft Graph endpoint, permission/scope name, SDK method, property/relationship mapping, licensing-behavior claim, or Agent Identity mapping backing future rule inputs. | Requires validation against published Microsoft documentation; invention is prohibited (INV-15). | `collector-contracts.md` / `capability-model.md` / `authentication-design.md` after documentation validation. |

---

## 16. Explicit non-goals

- No concrete security rule is defined, approved, or implied here.
- No Microsoft Graph endpoint, permission/scope name, SDK API, licensing
  behavior, Entra property, Agent Identity identifier, or undocumented
  provider behavior is invented or assumed.
- No C# type, interface member list, namespace, package, SDK, library,
  framework, retry/timeout/concurrency constant, serialization format, or
  registration mechanism is selected.
- No SaaS/multi-tenant execution, remediation, monitoring, dashboard,
  plugin marketplace, AI verdict, or other NG-series non-goal is
  introduced.
- No test, fixture, workflow, or CI implementation is created here.

---

## 17. Acceptance criteria

This document is complete for Phase 0.3.8 when:

1. The authority boundaries in §2 are explicit and traceable to INV-03,
   INV-04, INV-12, INV-13.
2. Every contract family in §3 is present with implementation-oriented
   obligations and without normative C# syntax or provider invention.
3. The five canonical states in §4 are referenced exactly — PASS, FAIL,
   NOT_EVALUATED, NOT_APPLICABLE, ERROR — with no sixth state and no
   alias.
4. Every false-PASS rule in §5 is explicit, including confirmed-empty
   gating, NOT_APPLICABLE/ERROR non-conversion, and cancellation handling.
5. Applicability semantics (§6), determinism (§7), evidence (§8), tenant
   isolation (§9), failure isolation (§10), registration (§11),
   extensibility (§12), secret exclusion (§13), and boundedness (§14) are
   each explicit and consistent with the owning documents cited.
6. Every implementation-sensitive unknown is listed in §15 as a TBD with
   a named owner; no TBD is silently resolved.
7. `git diff --check` is clean and `git status --short` shows only the
   two authorized files modified.

---

## 18. Invariant and requirement traceability

| Concern | Traces to |
| --- | --- |
| Deterministic evaluation, no randomness/wall-clock/provider calls | INV-02; VERD-006; rule-engine architecture §§2, 9 |
| Provider isolation, normalized boundary, no SDK leakage | INV-03; INV-04; rule-engine architecture §2 |
| Five states, no coercion, no sixth state | INV-05; VERD-001–VERD-005; `core-contracts.md` §4 |
| Evidence/provenance for verdicts and reasons | INV-06; INV-11; rule-engine architecture §§5–6, 15 |
| Capability gating, no universal mapping | INV-07; CAP-001–CAP-004; `capability-model.md` §§8–9 |
| Secret exclusion | INV-09; SEC-001; SEC-002; OUT-006 |
| Tenant isolation | INV-10; rule-engine architecture §20 |
| Renderer non-authority, AI non-authority | INV-12; INV-13; SEC-010; VERD-007; CON-004 |
| Failure transparency, false-PASS resistance | INV-14; rule-engine architecture §§4, 13, 21 |
| No undocumented provider dependency | INV-15; CAP-004; CAP-005 |
| Secure configuration defaults | INV-16; rule-engine architecture §16 |

---

## Self-verification (Phase 0.3.8 authoring note)

- Only `docs/03-implementation-design/rule-engine-contracts.md` and
  `docs/03-implementation-design/testing-seams.md` were modified in this
  phase task.
- No requirements or architecture document was modified.
- No application/source/test code and no `.sln`/`.csproj` files were
  created.
- Exactly five canonical assessment states are used: PASS, FAIL,
  NOT_EVALUATED, NOT_APPLICABLE, ERROR. No sixth state or alias was
  introduced.
- No Microsoft Graph endpoint, permission, SDK API, licensing behavior,
  Entra property, Agent Identity mapping, or undocumented provider
  behavior was invented.
- No secrets or credential material were introduced.
- No `git add/commit/push/reset/clean/checkout/switch` was performed.

(End of file)
