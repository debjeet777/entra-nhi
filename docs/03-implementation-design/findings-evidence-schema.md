# Findings / Evidence Schema

> **Phase:** 0.3.7
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Purpose and scope

- **Status:** Proposed conceptual findings/evidence contracts. This
  document is design output only. It authorizes no implementation.
- **Scope:** Defines the output-neutral authoritative finding and
  evidence model: what a `RuleEvaluation` is, what a `Finding` is, what
  counts as evidence, how evidence chains to provenance, what each of the
  five assessment states requires, and where findings/evidence authority
  ends. Derived from the approved findings/evidence architecture
  (`docs/02-architecture/findings-evidence.md`), rule-engine architecture
  (`docs/02-architecture/rule-engine.md`), domain model, identity graph,
  output architecture, invariants (INV-01–INV-16), trust boundaries,
  threat model, and testing architecture, as structured by
  `solution-structure.md` §§4–8 and governed by `README.md` §§4–6.
- **What this document is:** The owner of the findings/evidence/
  provenance/diagnostic schema contracts: the `RuleEvaluation` vs
  `Finding` distinction, evidence references, provenance chain, emission-
  policy hook, redaction hook, and fabrication-defense constraints
  (`README.md` §5–§6). First normative definition of the evidence schema
  vocabulary lives here; all other documents use it referentially.
- **What this document is not:** It is not a C# implementation, a
  signing/hashing implementation, a serialization-schema finalization, a
  finding-emission-policy finalization, a fingerprint-algorithm selection,
  a severity-taxonomy finalization, a rule catalog, a renderer
  specification, or a persistence/database design. Detailed neighbors live
  elsewhere: rule pipeline and evaluation surface in
  `rule-engine-contracts.md`, graph operations in
  `identity-graph-contracts.md`, capability states in
  `capability-model.md`, error/result shapes in `error-result-model.md`,
  collector envelopes in `collector-contracts.md`, output projections in
  `output-renderer-contracts.md`, auth boundaries in
  `authentication-design.md`, normalized domain shapes in
  `domain-types.md`, and the shared core surface in `core-contracts.md`
  (which references — never duplicates — the schemas defined here).
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed conceptual contracts** owned by this
  document, and (c) **unresolved TBDs** naming the owning later document
  (§37). Nothing herein claims a control is implemented, verified, or
  enforced. Where architecture states `DESIGNED / REQUIRED`, this document
  preserves that classification.
- **Illustrative syntax discipline:** This document MAY use C#-style
  pseudotypes where that materially improves precision. Any such syntax is
  **illustrative and non-normative — conceptual and non-production**: not
  final production code, not a namespace decision, and not a member list.
  No framework, package, SDK, or library implementation is selected here
  (CON-003). No source files are created by this document.
- **Canonical assessment states (referenced, not redefined):** The
  authoritative assessment states remain EXACTLY `PASS`, `FAIL`,
  `NOT_EVALUATED`, `NOT_APPLICABLE`, `ERROR`, with semantics owned by
  `core-contracts.md` §4. No sixth authoritative assessment state is
  created here. Severity, confidence, risk-score, emission, operational,
  capability, and error terms below are explicitly typed in their correct
  layer — never as assessment states.

---

## 2. Ownership boundary

1. Findings/evidence construction is owned conceptually by the `Findings`
   module inside `EntraNHI.Core` (`solution-structure.md` §7) and
   contracted here. It sits downstream of rule evaluation and upstream of
   output projection:
   `RuleEvaluation` → evidence references → `Finding` (where emission
   policy requires) → output-neutral result → renderer projection.
2. Findings/evidence architecture MUST NOT recompute rule logic, change
   evaluation state, upgrade/downgrade `PASS`/`FAIL`, convert `ERROR` to
   `FAIL`, convert `NOT_EVALUATED` to `PASS`, infer results from
   presentation behavior, or allow AI/LLM to modify evaluation state —
   carried forward from the findings-evidence architecture §1.
3. The `Findings` module MUST NOT recompute rule predicates and MUST NOT
   reference provider or renderer namespaces or AI/LLM types
   (`solution-structure.md` §9).
4. Findings/evidence are authoritative product data; renderers are
   projections with no assessment authority (§25).

---

## 3. Finding identity

1. Every `Finding` carries a stable machine-readable finding identifier
   plus an assessment/run reference binding it to the assessment that
   produced it (snapshot semantics, §31).
2. Finding identifiers MUST NOT depend solely on display names
   (`domain-types.md` §14; INV-G2 consequences).
3. No global-uniqueness claim is made until an identifier strategy is
   designed. Identifier stability holds within an assessment; cross-run
   correlation/dedup behavior is governed by §32 and the fingerprint TBD
   (§37).
4. Exact identifier formats (finding, assessment, correlation) are TBD
   (§37, T-01). This document fixes only the conceptual requirements
   above.

---

## 4. Rule identity/reference

1. Every finding and every `RuleEvaluation` references its originating
   rule by stable rule ID plus rule version (behavior changes produce a
   new version; deprecated rules retain IDs with supersession metadata —
   `core-contracts.md` §13).
2. Rule IDs are stable across versions; removal is deprecation, not ID
   reassignment (rule-engine architecture §8, carried forward).
3. The rule reference MUST be sufficient to resolve the rule definition
   that produced the outcome (including its deterministic configuration/
   rule-set reference) so historical records keep their meaning
   (version-aware evidence, `core-contracts.md` §13).
4. Exact version-format choices belong to `rule-engine-contracts.md`.
   This document requires only that the reference be carried.

---

## 5. Subject identity/reference

1. Every finding and every `RuleEvaluation` references its evaluated
   subject: the target normalized identity key (assessment-local,
   tenant-scoped) and/or the evaluated context reference, using safe
   identifiers only — never secrets, never display-name keys
   (`domain-types.md` §§4, 14).
2. Subject references MUST be tenant-scoped: a subject reference is valid
   only within its tenant assessment context (§28).
3. Where the subject is a graph-derived scope (subgraph, relationship
   neighborhood), the reference identifies the scope plus the underlying
   normalized subjects, preserving traceability to §13 graph-derived
   evidence.
4. Subject-identifier formats are TBD with `domain-types.md` (§37).

---

## 6. Canonical assessment state

1. Every `RuleEvaluation` and every `Finding` carries exactly one
   canonical assessment state — `PASS`, `FAIL`, `NOT_EVALUATED`,
   `NOT_APPLICABLE`, or `ERROR` — with semantics owned by
   `core-contracts.md` §4 and referenced here without redefinition
   (`README.md` §6).
2. The state on a `Finding` MUST equal the state of its originating
   `RuleEvaluation`. Finding emission MUST NOT change the underlying
   evaluation state (§2).
3. No sixth authoritative state exists in V1. Severity, confidence, risk
   scores, finding-emission decisions, capability states, operational
   conditions, provenance conditions, evidence conditions, and error
   conditions MUST NOT silently become assessment states.
4. State-specific content requirements are fixed in §§16–20. Renderer
   omission/transformation MUST never alter authoritative assessment
   state (§25).

---

## 7. Severity/category concepts

1. Severity/category metadata (where applicable) flows into findings and
   output WITHOUT altering evaluation logic: severity MUST NOT change
   `PASS`/`FAIL` determination, MUST NOT be reinterpreted into assessment
   state by any consumer, and MUST NOT convert `ERROR`/`NOT_EVALUATED`
   into apparent verdicts (INV-12; `core-contracts.md` §4, rule 5).
2. Severity is downstream metadata/policy, not an assessment state (§6,
   rule 3). Confidence-like, risk-score-like, or priority-like concepts,
   if ever introduced, are likewise metadata — never states and never a
   substitute for evidence.
3. No severity claim is made about any concrete rule here (no rule
   content is invented). The severity taxonomy itself belongs to
   `rule-engine-contracts.md` (TBD). This document requires only the
   non-authority posture above and that severity travel with provenance
   (which rule version assigned it).
4. AI MUST NEVER supply or alter severity authoritatively (INV-13).

---

## 8. Human-readable title/summary boundaries

1. `Finding` MAY carry human-readable title/summary and security-rationale
   text for operator consumption. Such text is presentation-supporting
   metadata, NOT authoritative evidence: presentation text is not the
   authoritative evidence itself (findings-evidence architecture §5).
2. Title/summary MUST be derivable from (or at minimum consistent with)
   the authoritative state plus evidence/reason; it MUST NOT assert facts
   the evidence does not establish, MUST NOT upgrade/downgrade state
   language, and MUST NOT suppress `ERROR`/`NOT_EVALUATED` into
   false-secure phrasing.
3. Title/summary text MUST NOT carry secrets (§27) and MUST treat
   tenant/provider-originated strings as untrusted data carried opaquely
   for renderer-side encoding (§29).
4. Exact title/summary templates, localization, and style policy are TBD
   with output design (§37). No template is finalized here.

---

## 9. Evidence model

Conceptual evidence content (carried forward from the findings-evidence
architecture §5; record mechanics deferred to §37):

1. Evidence supports WHY an evaluation was produced. `PASS` and `FAIL`
   both require evidence appropriate to rule semantics (§§19–20);
   `NOT_EVALUATED`, `NOT_APPLICABLE`, and `ERROR` require structured
   reason/context, not fabricated `PASS`/`FAIL` evidence (§§16–18).
2. Evidence MAY reference normalized, non-secret assessment facts such
   as: normalized identity attributes; normalized relationship
   observations; graph relationships; capability states; explicitly
   non-secret credential metadata; accountability observations;
   permission/access observations; deterministic derived values; relevant
   assessment context.
3. Evidence MUST preserve enough provenance to trace back toward the
   collected source observation WITHOUT requiring reports to expose raw
   provider payloads (§11).
4. Evidence MUST be structured where possible. Presentation text is not
   authoritative evidence (§8).
5. Evidence MUST NOT fabricate certainty that upstream collection did not
   establish: absence is evidence ONLY when the relevant
   collection/capability is known to be sufficiently complete for that
   rule (`capability-model.md` R-02/R-03; false-PASS controls in §19).
6. No raw provider payloads by default (§31).

```csharp
// Illustrative only. Conceptual and non-production. Not a member list.
record EvidenceReference {
  EvidenceRef Id; FindingOrEvaluationRef Parent;
  FactRef Fact; // normalized fact, graph edge, capability state, derived value
  ProvenanceRef Provenance; CapabilityStateRef Capability;
  // + assessment/tenant context, derivation lineage where applicable.
}
```

---

## 10. Evidence item identity

1. Every evidence item/reference carries a stable machine-readable
   evidence identifier within its assessment scope, supporting audit
   linkage (evaluation → evidence → provenance → source observation).
2. Evidence identifiers MUST NOT depend solely on display names and MUST
   NOT claim global uniqueness until an identifier strategy is designed
   (§33).
3. Evidence identity is distinct from finding identity (§3) and from
   provenance identity (§11): one finding references many evidence items;
   one evidence item references its provenance chain.
4. Exact evidence-identifier formats are TBD (§37, T-01).

---

## 11. Evidence provenance

1. Every evidence item carries (or references) provenance sufficient to
   identify where the underlying fact originated: source system (source
   family as a normalized value), source object reference (documented,
   source-exposed identifiers only), collection operation/context,
   assessment/tenant context, observation-time context where appropriate,
   and transformation/derivation lineage for normalized or derived facts
   (`domain-types.md` §11; `identity-graph-contracts.md` §9).
2. The explicit conceptual chain provider observation → source
   observation → normalized domain fact → graph/derived fact where
   applicable → evaluation evidence reference → finding where emitted →
   output representation MUST preserve traceability at each
   transformation boundary (findings-evidence architecture §6).
3. Unknown or unavailable provenance MUST be represented explicitly,
   never invented. Provenance is not evidence by itself — which
   provenance-backed observations become rule evidence is determined by
   rule semantics plus §§16–20.
4. Provenance schema/serialization details are TBD (§37). Integrity
   mechanisms (hashing/signing/timestamping) are future hardening, not
   claimed controls (§30).

---

## 12. Source observation references

1. Evidence MUST be able to chain back toward the collected source
   observation: each evidence item references the normalized fact(s) it
   rests on, each normalized fact references its source observation(s)
   and collection context (via `collector-contracts.md` envelope
   concepts and `domain-types.md` provenance vocabulary).
2. References are preferred over raw duplication (data minimization,
   §31): evidence carries normalized-fact references plus provenance,
   not full provider objects.
3. Source-observation envelope schema/serialization belongs to
   `collector-contracts.md`. This document requires only that evidence
   references resolve against that envelope vocabulary without leaking
   provider SDK types into findings/evidence records (INV-03; INV-04).
4. Where a source observation is unavailable (gap-derived reason rather
   than fact-derived evidence — §§16–18), the record carries the reason
   plus capability context INSTEAD of a fact reference, explicitly
   marked as such — never a fabricated fact reference.

---

## 13. Graph-derived evidence references

1. Evidence derived from graph relationships MUST reference the graph
   edge(s): edge identifier/scope, relationship kind, derivation
   classification (observed vs derived, INV-G6), derivation basis plus
   source observations for derived edges, and edge provenance/capability
   context — sufficient inputs supplied by the graph handoff
   (`identity-graph-contracts.md` §22).
2. Such evidence MUST remain traceable to normalized source
   observations/provenance where available: the chain normalized domain
   fact → graph edge (with derivation lineage) → evaluation evidence
   reference MUST NOT be broken by the graph boundary.
3. Derived-edge evidence MUST carry its derivation basis visibly; it MUST
   NOT present a deterministically computed relationship as directly
   observed, and MUST NOT assert certainty beyond the deterministic
   derivation.
4. Unresolved-reference placeholders and conflict metadata from the graph
   (`identity-graph-contracts.md` §§15–16) MUST surface in evidence/
   reason as incompleteness — never silently omitted to present a clean
   relationship picture.

---

## 14. Capability/completeness context

1. Every finding/`RuleEvaluation` preserves the relevant
   capability/completeness context: the capability-state references the
   evaluation depended on (`capability-model.md` §§5–7), the rule's
   prerequisite declarations (`capability-model.md` §8), and the
   completeness basis for any absence-dependent reasoning
   (`capability-model.md` §9, R-02/R-03).
2. `PASS` resting on absence MUST record the completeness argument that
   authorized it; a reviewer MUST be able to determine WHY absence was
   treated as conclusive for that rule (§19).
3. `NOT_EVALUATED` MUST record WHICH required capability/data was missing
   and its state (§16); gaps MUST NOT be summarized away in aggregation
   (§21).
4. Capability-state names/serialization remain owned by
   `capability-model.md`. This document carries them by reference only.

---

## 15. Failure/error context

1. `ERROR` and operationally caused `NOT_EVALUATED` records carry
   sanitized failure/error context: the failure category (conceptual
   taxonomy owned by `error-result-model.md` §5 — carried by reference,
   never redefined), the pipeline stage and scope (rule/target/stage,
   per-rule vs assessment-wide), the sanitized cause chain, and
   tenant-scoped correlation context — WITHOUT provider exception types,
   raw payloads, or secrets (§§26–27).
2. Diagnostics explain system/evaluation problems and are DISTINCT from
   tenant security findings (findings-evidence architecture §15): a
   system diagnostic MUST NOT automatically become a tenant security
   `FAIL` (§21).
3. Fatal-vs-isolated handling (whether a failure halts a scope or the
   whole evaluation) belongs to `rule-engine-contracts.md` with
   `error-result-model.md`; this document requires only that the chosen
   handling stay visible in evidence/diagnostics (never silent).
4. Exit-code mapping and renderer-failure isolation belong to
   `output-renderer-contracts.md`. Renderer failure is a system/output
   diagnostic — never a tenant finding and never silent success.

---

## 16. NOT_EVALUATED evidence requirements

1. `NOT_EVALUATED` — the rule could not legitimately determine `PASS` or
   `FAIL` because required capability or data was unavailable,
   unsupported, insufficient, or otherwise not collected as required —
   REQUIRES structured reason/context (not `PASS`/`FAIL`-shaped
   evidence):
   - which required capability/data was missing;
   - the capability/observation state that caused the gap (by reference
     to `capability-model.md` §5 concepts);
   - the rule prerequisite(s) unmet (`capability-model.md` §8);
   - the subject/scope affected (per-subject isolation preserved);
   - provenance/assessment-context references sufficient for audit.
2. `NOT_EVALUATED` MUST preserve the reason for inability to evaluate:
   authorization denials, licensing/service gaps, unsupported
   capabilities, collection failures, partial inputs, unknown/
   indeterminate causes, and cancellation consequences each remain
   distinguishable — never collapsed into a generic "skipped" label that
   hides the cause.
3. `NOT_EVALUATED` MUST NOT be used for a rule that deterministically
   does not apply (that is `NOT_APPLICABLE`, §17), and MUST NOT be used
   to hide unsupported functionality without naming it.
4. `NOT_EVALUATED` != `FAIL` (§21): a capability gap is absence of
   evidence, never evidence of failure.

---

## 17. NOT_APPLICABLE evidence requirements

1. `NOT_APPLICABLE` — the rule deterministically does not apply to the
   evaluated identity/context per its applicability predicate — REQUIRES
   deterministic applicability rationale (not `PASS`/`FAIL` evidence):
   - the identity kind/context evaluated;
   - the applicability criterion (predicate reference + rule version)
     that determined non-applicability;
   - sufficient assessment-context/provenance references for audit.
2. `NOT_APPLICABLE` MUST preserve that rationale explicitly so reviewers
   can distinguish genuine non-applicability from gap-hiding.
3. `NOT_APPLICABLE` MUST NOT be used to hide unsupported functionality
   or collection failure; `NOT_EVALUATED` MUST NOT be used for genuine
   non-applicability (`capability-model.md` §9 constraints).
4. `NOT_APPLICABLE` != `PASS` (§21): non-applicability asserts nothing
   about the subject's security posture.

---

## 18. ERROR evidence requirements

1. `ERROR` — evaluation was attempted or should have been evaluable, but
   an unexpected engine, data, or runtime failure prevented trustworthy
   completion — REQUIRES sanitized operational/integrity context
   sufficient for diagnosis WITHOUT leaking sensitive material:
   - which rule/target/stage failed;
   - which conceptual failure category caused it (§15, by reference);
   - the sanitized diagnostic chain (scope information at each link;
     no pure re-wrapping without new information);
   - tenant-scoped correlation context;
   - integrity-failure marking where applicable (tenant mismatch,
     corrupted shared state, fabricated-evidence attempts —
     `error-result-model.md` §13).
2. `ERROR` MUST NOT be converted to `FAIL` to simplify reporting, nor to
   `PASS` to simplify output, nor relabeled as ordinary
   `NOT_EVALUATED` where doing so would hide corruption as a mere data
   gap.
3. `ERROR` != `FAIL` (§21): execution failure is not a tenant security
   verdict.
4. Diagnostics MUST NOT expose secrets, tokens, raw authentication
   material, or unnecessary raw payloads (§§26–27).

---

## 19. PASS evidence requirements

1. `PASS` — applicable rule, sufficiently available inputs, completed
   evaluation, failure condition not satisfied — REQUIRES affirmative
   evidence sufficient to establish that required observations were
   complete enough AND the failure condition was not satisfied
   (`core-contracts.md` §4; findings-evidence architecture §7).
2. Missing evidence MUST NOT produce `PASS`. "Nothing found" is `PASS`
   ONLY when the rule explicitly establishes that collection was
   complete enough for absence to be meaningful (rule-defined input
   completeness + §14 completeness basis). Incomplete collection,
   authorization denial, unsupported capability, collection error,
   normalization failure, graph failure, and resource exhaustion MUST NOT
   produce `PASS`.
3. Absence-dependent `PASS` MUST record its completeness argument (§14,
   rule 2); reviewers MUST be able to audit WHY emptiness was conclusive.
4. If required evidence cannot be constructed, the finding layer MUST NOT
   silently emit a fully trustworthy `PASS` representation: preserve the
   underlying `RuleEvaluation`, produce explicit diagnostic/integrity
   handling, and fail safely per validated policy (fabrication defense,
   §30).

---

## 20. FAIL evidence requirements

1. `FAIL` — applicable rule, sufficiently available inputs, completed
   evaluation, explicitly defined failure condition satisfied — REQUIRES
   affirmative deterministic evidence supporting the failure predicate:
   the satisfying facts/relationships, their provenance chain, the
   capability context establishing input sufficiency, and the rule
   version/configuration that defined the predicate.
2. Missing data MUST NOT automatically become `FAIL`: `FAIL` requires
   affirmative evidence that the documented failure condition IS
   satisfied. A collection, authentication, or system failure is not
   itself a tenant security `FAIL` unless a separate rule explicitly
   evaluates that condition as assessment data under a documented
   requirement (`core-contracts.md` §4).
3. If required failure evidence cannot be constructed, §19 rule 4
   applies symmetrically: no silent fully-trustworthy `FAIL`
   representation; preserve the evaluation; explicit diagnostics; fail
   safely.
4. `FAIL` evidence MUST be structured where possible so renderers project
   facts without reinterpretation (§25).

---

## 21. Aggregation boundaries

1. Assessment-level aggregation collects per-rule evaluations into an
   assessment result preserving per-rule detail, evidence references,
   diagnostics, capability context, and configuration/version references
   (`core-contracts.md` §3.9). Aggregation MUST preserve — not summarize
   away — `ERROR` and `NOT_EVALUATED` conditions.
2. No "overall success" boolean substitutes for the five-state model. Any
   summary indicator (counts, rollups) is derived metadata over preserved
   detail — it MUST NOT hide `ERROR`, `NOT_EVALUATED`, or incomplete
   collection, and MUST NOT be consumable as a security verdict by itself
   (`error-result-model.md` §14).
3. Diagnostics are distinct from tenant findings (§15): aggregation MUST
   NOT fold system diagnostics into tenant `FAIL` counts and MUST NOT
   suppress them into false-secure summaries.
4. Aggregate severity/summary policy beyond per-rule severity metadata is
   NOT finalized here (TBD, §37). Composition is deterministic: same
   per-rule outcomes plus same configuration produce the same aggregate
   shape (§22).
5. Cross-document consistency (normative inequalities — aggregation
   face):
   - `NOT_EVALUATED` != `FAIL` (gap folding into failure counts is
     prohibited).
   - `NOT_APPLICABLE` != `PASS` (non-applicability folding into secure
     counts is prohibited).
   - `ERROR` != `FAIL` (execution-failure folding into tenant-failure
     counts is prohibited).
   - renderer output != authoritative finding (summaries Projections
     never rewrite the preserved detail, §25).

---

## 22. Deterministic ordering

1. Deterministic equivalent assessment input MUST produce semantically
   equivalent finding/evidence records (INV-02; VERD-006 downstream):
   same normalized inputs, capability states, rule versions/
   configurations, and deterministic execution context → same finding/
   evidence semantics.
2. Finding/evidence emission order MUST be deterministic for equivalent
   content (ordering mechanics TBD, §37): ordered by stable keys (rule
   ID/version, subject key, evidence identifier) with deterministic
   tie-breakers — never display-name order, never hash-code order, never
   parallelism-completion order.
3. Presentation metadata that does not affect evaluation (e.g., report
   timestamps) MAY differ; evaluation/finding/evidence semantics MUST
   NOT.
4. No randomness, wall-clock branching inside finding/evidence semantics,
   probabilistic handling, or model inference on the construction path.

---

## 23. Stable serialization semantics

1. Finding/evidence/provenance/diagnostic records MUST be serializable in
   a stable, deterministic manner: equivalent records serialize to
   semantically equivalent projections regardless of renderer
   (canonical serialization supports audit, snapshot comparison, and
   golden/snapshot testing).
2. Serialization MUST preserve all five states, evidence/reason
   references, provenance chains, capability context, tenant scoping, and
   incompleteness markers — nothing semantically load-bearing is
   serialization-optional.
3. The canonical model MUST NOT pre-encode for any single format (§24):
   no format-specific escaping, markup, or presentation state in stored
   records.
4. Exact canonical serialization format, schema versioning, and version
   headers are TBD (§37, T-02). No format is finalized here.

---

## 24. Output-neutrality

1. Evidence MUST be output-neutral: no HTML, SARIF, CLI formatting, ANSI
   sequences, Markdown presentation authority, or renderer-specific state
   in authoritative records. Assessment semantics are independent of CLI,
   JSON, SARIF, HTML, or future presentation (INV-12).
2. The authoritative finding/evidence record is the source of truth;
   output contracts are projections of it (`core-contracts.md` §11).
   Renderers MUST NOT rerun rules, call providers, request privilege,
   alter evaluation state, reinterpret severity into state, fabricate
   evidence/provenance, suppress diagnostics, or perform remediation.
3. Injection posture at the boundary: authoritative records treat all
   tenant/provider-originated strings as untrusted data carried opaquely
   (§29); encoding/escaping obligations belong to renderers per
   `output-renderer-contracts.md`.
4. Per-renderer contracts, semantic-preservation rules, and
   injection/file-safety obligations belong to
   `output-renderer-contracts.md`. This document requires only
   output-neutrality and non-authority.

---

## 25. Renderer boundary

1. Findings/evidence are canonical security data; JSON, SARIF, HTML, CLI,
   and future UI are representations (findings-evidence architecture
   §16). Output renderers MUST NOT change authoritative evaluation state,
   invent evidence, suppress `ERROR`/`NOT_EVALUATED` into a false-secure
   impression, reinterpret severity, perform provider calls, request
   additional privileges, or execute remediation.
2. Renderer omission/transformation MUST never alter authoritative
   assessment state: a renderer that omits a field, redacts content
   (§34), or summarizes detail MUST do so WITHOUT changing the stored
   authoritative record and WITHOUT presenting the projection as a
   re-verdict. Redacted artifacts indicate where information was withheld
   where necessary for correct interpretation (§34).
3. Renderer failure is a system/output diagnostic — never a tenant
   finding and never silent success (`error-result-model.md` §12, rule
   4). The authoritative result stands; partial artifacts MUST NOT
   present as complete.
4. Renderer-failure semantics, per-format schemas, and exit-code mapping
   belong to `output-renderer-contracts.md` (§37).

---

## 26. Sanitization

1. Sanitization applies at every link of every evidence/provenance/
   diagnostic chain: strip secrets, normalize away provider exception
   types, exclude raw payloads by default (`error-result-model.md` §10).
2. Provider-originated text is untrusted data carried opaquely; it is
   never trusted as code, path, or markup, and encoding obligations
   belong to renderers (§24).
3. If unexpected secret-like material reaches the evidence boundary, the
   finding layer MUST fail safely — sanitize/reject per validated policy
   — and MUST NOT propagate the material into records, artifacts, logs,
   or telemetry (concrete detection implementation TBD, §37; default
   posture: reject-or-sanitize visibly, never silently persist).
4. Sanitized failures are reported as category + scope + safe reason,
   never with material-bearing detail (§15).

---

## 27. Secret exclusion

Normative prohibition (INV-09; SEC-001; SEC-002; OUT-006). The following
MUST NEVER enter findings, evidence, provenance, diagnostics, reports,
logs, telemetry, or persisted artifacts — in any field, including nested
causes and rendered text:

- access tokens and refresh tokens;
- client-secret values and secret values of any kind;
- private keys and private-key material (including raw certificates
  containing private material);
- passwords and recovery codes;
- authentication cookies and raw credential material;
- raw authorization headers;
- credential-bearing URLs/query strings;
- secret-bearing configuration values.

Credential metadata is evidence ONLY when explicitly non-secret
(`domain-types.md` §7). Transient authentication material is
runtime-only and never normalized into findings/evidence
(`authentication-design.md` §§5–6). Test fixtures MUST NOT carry real
secrets either (synthetic placeholders only, per `AGENTS.md`).

---

## 28. Tenant isolation

1. Every `RuleEvaluation`, evidence reference, and `Finding` MUST remain
   bound to one explicit tenant assessment context (INV-10;
   findings-evidence architecture §12).
2. Evidence from one tenant MUST NOT support an evaluation for another
   tenant. Cross-tenant evidence correlation is prohibited in V1.
3. Tenant-context mismatch/contamination is an integrity failure and MUST
   fail safely and visibly (`error-result-model.md` §13): never silent
   mixing, never cross-tenant edges, never cross-tenant evidence
   correlation, never conversion into verdicts.
4. Tenant isolation applies to telemetry, logs, persistence, and
   aggregation as to output: no cross-tenant correlation or aggregation
   in V1.
5. Tenant isolation (this section) is distinct from SaaS multi-tenancy
   (simultaneous multi-tenant service operation, a V1 non-goal per
   NG-003).

---

## 29. Untrusted-string handling

1. Tenant-originated/provider-originated strings are untrusted and MUST
   remain safe for later renderers: display names, descriptions,
   application names, owner names, externally controlled identifiers, and
   future textual metadata are carried opaquely as data — never trusted
   as code, path, markup, or commands.
2. The finding/evidence layer MUST NOT pre-encode strings for any single
   format (§24) and MUST NOT interpret strings as markup/paths/commands
   at construction time.
3. Architecture anticipates renderer-side defense (HTML injection,
   terminal/control-character injection, JSON correctness, SARIF field
   safety, log injection, formula/spreadsheet injection if tabular export
   is ever added, path/file-name injection where output names become
   configurable — findings-evidence architecture §17). Escaping
   implementation and library selection belong to
   `output-renderer-contracts.md`; no library is chosen here.
4. Malformed Unicode/control characters (including bidi controls, CR/LF
   sequences) MUST survive the finding layer intact as opaque data so
   renderer tests can prove safe encoding; the finding layer MUST NOT
   silently normalize them away where doing so could mask injection test
   fidelity.

---

## 30. Evidence integrity

1. No layer may fabricate evidence to make a finding appear complete
   (findings-evidence architecture §18). If required evidence cannot be
   constructed: do NOT silently emit a fully trustworthy `PASS`/`FAIL`
   representation; preserve the underlying `RuleEvaluation`; produce
   explicit diagnostic/integrity handling; fail safely per validated
   policy (exact evidence-integrity failure policy TBD, §37).
2. Do NOT claim cryptographic evidence integrity unless such a mechanism
   is actually selected and implemented: artifact hashing, signing,
   provenance manifests, timestamping, and integrity verification are
   future hardening/TBD (§37) — architected for, never falsely claimed.
3. Confidentiality vs integrity vs availability are explicitly
   distinguished (findings-evidence architecture §20): prevent
   unauthorized disclosure; prevent/detect unauthorized modification or
   fabrication; ensure failures/resource exhaustion do not silently
   create misleading results.
4. AI has no authority to create/change `PASS`/`FAIL` or authoritative
   evidence (§35).

---

## 31. Evidence minimization

1. Evidence and findings MUST contain only information necessary to:
   explain the evaluation; identify the affected target; support
   audit/review; support safe output generation (findings-evidence
   architecture §9).
2. Do NOT copy full provider objects for convenience. Do NOT retain
   unnecessary raw payloads. Do NOT duplicate sensitive directory
   attributes without a defined need. Prefer references to normalized
   facts over raw source duplication.
3. No raw provider payloads by default. Raw-response diagnostic retention
   policy, if any, belongs to `collector-contracts.md` (TBD) and MUST
   exclude secrets.
4. Even without secrets, findings data MUST NOT be treated as harmless:
   it may reveal identity structure, privilege relationships,
   accountability gaps, credential lifecycle posture, permission exposure,
   and tenant structure (findings-evidence architecture §11) — so
   minimization, explicit output destinations, no undisclosed telemetry,
   safe logging, and controlled persistence apply (§§33–34).

---

## 32. Correlation/traceability

1. Repeated observations or evaluations MUST NOT silently create
   contradictory security meaning. Deterministic correlation/
   deduplication support is required where appropriate: identical
   evaluations reproduced from identical inputs correlate stably;
   duplicates collapse without semantic drift.
2. Finding identity/fingerprint strategy (how repeated runs recognize
   "the same finding") is TBD (§37, T-03): no fingerprint algorithm is
   invented here.
3. Correlation/operation identifiers (assessment/run/operation references)
   enable end-to-end tracing across collection → normalization → graph →
   evaluation → findings → output, carried in sanitized diagnostic
   context (`error-result-model.md` §9).
4. Later tenant changes MUST NOT silently alter the meaning of an
   already-produced `RuleEvaluation` or `Finding` (snapshot semantics;
   findings-evidence architecture §8). Exact persistence/hashing/
   signing/timestamping/archival mechanisms are TBD (§37).

---

## 33. Schema/version evolution

1. Additive and reversible preference (`README.md` §8; `core-contracts.md`
   §13): prefer additive schema extensions (new optional fields, new
   evidence categories, new renderer projections) over breaking
   redefinitions. A breaking schema change requires explicit
   justification, impact statement, and recovery path before approval.
2. Every evaluation MUST remain attributable to the rule
   definition/version and deterministic configuration that produced it,
   so historical records keep their meaning (version-aware evidence).
3. Capability-gated evolution: new identity categories, relationship
   kinds, evidence categories, or enrichment sources enter as explicitly
   capability-gated, provenance-preserving additions that unsupported
   environments report as `NOT_EVALUATED`/`NOT_APPLICABLE` — never as
   silent reinterpretations of existing states (INV-07; INV-15).
4. No silent semantic change: a schema change altering the meaning of
   `PASS`/`FAIL`, the completeness bar for `PASS`, or failure mappings is
   a versioned behavior change with change traceability — never an
   editorial clarification.
5. Exact schema-version format, compatibility policy, and golden/snapshot
   policy are TBD (§37). No public stable-API promise is made here.

---

## 34. Test seams

Seams needed (detailed testing design remains `testing-seams.md`; no
tests are created here; all fixtures synthetic, provider-free,
secret-free, network-free for core-adjacent tests):

- `FAIL` evidence traceability and `PASS` evidence completeness fixtures
  (affirmative evidence present; completeness argument auditable).
- Per-state reason fixtures: `NOT_EVALUATED` structured reason,
  `NOT_APPLICABLE` structured reason, `ERROR` structured reason —
  each distinguishable, none collapsible into another.
- Provenance-chain preservation and missing-provenance fixtures
  (explicit unknown, never invented).
- Broken-evidence-reference, inconsistent-tenant-reference, unsupported-
  evidence-type, and invalid-finding-mapping fixtures (all fail visibly).
- Tenant-context mismatch and cross-tenant evidence-rejection fixtures
  (integrity failures, never merged).
- Secret-exclusion and token-leakage-prevention fixtures (secret-shaped
  synthetic material has no path into records, artifacts, logs, or
  telemetry); excessive-raw-payload rejection/minimization fixtures.
- Output-injection string fixtures (HTML/XSS, ANSI/control, CR/LF, bidi,
  malformed Unicode) carried opaquely through the finding layer.
- Evidence-fabrication prevention fixtures (unconstructable evidence
  never yields trustworthy `PASS`/`FAIL`).
- Finding/evaluation state-consistency fixtures (finding state always
  equals originating evaluation state).
- Deterministic-correlation behavior fixtures (identical inputs →
  semantically equivalent records; stable emission order).
- Redaction-semantics fixtures (redacted projections indicate withholding
  without altering authoritative records).
- Renderer-cannot-change-state and AI-cannot-modify-evidence fixtures
  (pass-through preservation proofs).
- Persistence-independent core-behavior fixtures (records complete
  without any store).

---

## 35. Architecture-invariant mapping

| Invariant | Findings/evidence contract support (constraint, not implementation claim) |
| --- | --- |
| INV-01 | No remediation/mutation vocabulary in findings; remediation guidance references only, never execution (§36). |
| INV-02 | Deterministic finding/evidence construction + ordering (§22). |
| INV-03 | Provider isolation: no SDK types/payloads in records (§12). |
| INV-04 | Normalized boundary: evidence references normalized facts (§§9, 12–13). |
| INV-05 | Five states preserved end to end; state-specific content rules (§§6, 16–20). |
| INV-06 | Evidence traceability + sufficiency rules (§§9–14, 19–20). |
| INV-07 | Capability/completeness context preserved (§14). |
| INV-08 | Least privilege: findings request no privilege; optional categories never broaden silently. |
| INV-09 | Secret exclusion + sanitization (§§26–27). |
| INV-10 | Tenant isolation (§28). |
| INV-11 | Provenance chain preservation (§§11–13). |
| INV-12 | Core/output separation: output-neutral records; renderer non-authority (§§24–25). |
| INV-13 | AI non-authority over states, evidence, capability, severity (§§2, 7, 30; AI posture in findings-evidence architecture §21 carried forward). |
| INV-14 | Failure transparency: explicit reason/diagnostic requirements (§§15–18, 21). |
| INV-15 | No undocumented dependency: no invented endpoints/properties/mappings (§36). |
| INV-16 | Secure defaults: minimization, no undisclosed telemetry, explicit destinations (§§31, 33–34); weakening requires explicit configuration downstream. |

AI posture (carried forward, non-authoritative explanatory use only):
AI may eventually explain an existing deterministic finding, summarize
existing evidence, or translate explanation language — but MUST NOT
create/modify factual evidence, determine/change evaluation state or
severity authoritatively, invent provenance, suppress diagnostics,
convert incomplete evidence into `PASS`/`FAIL`, or alter canonical
records. AI-generated explanation is downstream/non-authoritative.

---

## 36. Non-goals

1. No concrete security rule content and no scoring mathematics.
2. No remediation execution (guidance references at most; NG-001;
   NG-002; NG-007; SEC-003–SEC-007).
3. No provider endpoint, permission/scope, property, SDK-call, or
   identifier-mapping selection.
4. No Agent ID mapping and no licensing-tier/commercial mapping.
5. No severity/confidence/risk-score taxonomy finalization (posture fixed
   here; taxonomy owned by `rule-engine-contracts.md`).
6. No sixth authoritative assessment state and no generic boolean
   "success" substitute for the five-state model.
7. No signing/hashing implementation, timestamping implementation,
   encryption-at-rest design, or persistence/database design (V1 may
   operate with generated local/CI artifacts; persistence posture is
   separately designed if ever needed).
8. No serialization-schema finalization, output formatting, CLI syntax,
   exit-code, SARIF-mapping, or HTML-templating selection.
9. No redaction-policy finalization (hook contracted here; policy TBD).
10. No SaaS/multi-tenant service, dashboard, monitoring, AI verdicts, or
    other NG-series capabilities.
11. No replacement of Entra administration/governance products (NG-006).

---

## 37. Explicit TBD register

Each TBD states what is unknown (decision required), why it cannot be
resolved here (why deferred), and which later document owns its
resolution (owning later document/phase where known). None is resolved
by speculation. Signing/hashing mechanisms remain TBD (no cryptographic
integrity claimed).

| # | TBD (decision required) | Why deferred | Owned by |
| --- | --- | --- | --- |
| T-01 | Concrete record shapes and member lists for `RuleEvaluation`, `Finding`, evidence references/records, provenance records, diagnostics; identifier formats (assessment, rule, evaluation, finding, target, evidence, provenance). | Member-level design needs coordination with rule-engine, graph, collector, capability, and output owners; choosing members here would preempt them. Identifier formats need structural-test enforcement design. | This document (semantics) coordinated with `core-contracts.md`; physical shapes per consumer in their owning documents; seam placement in `dependency-boundaries.md` |
| T-02 | Canonical serialization format, schema versioning, and version headers for finding/evidence/provenance records. | Format/version choices need cross-document agreement (findings, collector envelope, output projection) and MUST NOT be fixed unilaterally here. | This document with `collector-contracts.md` (envelope) and `output-renderer-contracts.md` (projection) |
| T-03 | Finding-emission policy (which evaluations become findings) and finding fingerprint/dedup strategy. | Policy needs rule-engine and output coordination; a premature policy risks hiding states or inventing correlation semantics. | This document with `rule-engine-contracts.md` and `output-renderer-contracts.md` |
| T-04 | Provenance schema detail and provenance-manifest format. | Schema needs collector/graph/findings coordination plus security review. | This document with `collector-contracts.md` and `identity-graph-contracts.md` |
| T-05 | Redaction policy and redaction schema (output-specific policies; withholding-indication mechanics). | Policy needs output-consumer coordination; premature redaction risks altering semantics silently. | This document with `output-renderer-contracts.md` |
| T-06 | Sensitive-data classification policy for findings content beyond secrets. | Classification needs security review beyond secret exclusion. | This document |
| T-07 | Evidence-integrity failure policy (fatal vs isolated handling for broken references, inconsistent tenants, unsupported types, invalid mappings, serialization failures). | Handling needs rule-engine and error-model coordination; premature choice risks inventing failure semantics. | This document with `rule-engine-contracts.md` and `error-result-model.md` |
| T-08 | Artifact hashing/signing, timestamping, SBOM/release-provenance association, integrity-verification posture; output-artifact permissions; encryption-at-rest requirements where persistence exists. | Integrity mechanisms need separate security review and build-control coordination; no mechanism is approved elsewhere. | This document with `dependency-boundaries.md` and `implementation-sequence.md` (staging) |
| T-09 | Evidence persistence model, retention/deletion policy, access-control/backup/recovery/residency posture. | Persistence needs implementation-phase storage and lifecycle design; V1 assumes local/CI artifacts only. | This document with `implementation-sequence.md` (staging) |
| T-10 | Title/summary/rationale templates, style/localization policy, documentation/reference metadata shape, remediation-guidance reference shape. | Templates need output-consumer coordination; no presentation authority is finalized here. | This document with `output-renderer-contracts.md` |
| T-11 | Severity-taxonomy finalization and per-rule severity assignment mechanics. | Taxonomy belongs to the rule-engine surface; fixing it here would preempt the owner. | `rule-engine-contracts.md` (this document references only) |
| T-12 | Secret-detection/sanitization implementation mechanics at the evidence boundary. | Implementation choice needs security review; principles (§§26–27) suffice at this stage. | This document with `testing-seams.md` (verification seams) |
| T-13 | Per-seam findings/evidence fakes/mocks/fixtures, adversarial scenario catalog, golden/snapshot policy for finding/evidence shapes. | Test-scenario design belongs to the testing surface. | `testing-seams.md` |
| T-14 | Assessment-time representation detail in findings context. | Time design belongs to rule-engine and domain time principles. | `rule-engine-contracts.md` (time principles from `domain-types.md` §12) |

---

## 38. Acceptance checklist

This findings/evidence schema is accepted when:

1. **Authority placement.** §2 keeps `RuleEvaluation` authoritative and
   `Finding` derivative; no layer recomputes rules, changes state, or
   admits AI onto the verdict/evidence path.
2. **Identity soundness.** §§3–5 bind every finding/evaluation to stable
   finding, rule (ID + version), and tenant-scoped subject references
   without display-name keys, global-uniqueness claims, or finalized
   formats.
3. **State fidelity.** §6 carries exactly the five canonical states with
   finding state always equal to originating evaluation state and no
   sixth state.
4. **Metadata restraint.** §§7–8 keep severity as non-authoritative
   metadata and title/summary as non-evidence presentation support.
5. **Evidence sufficiency.** §9 fixes structured, provenance-preserving,
   output-neutral evidence over normalized non-secret facts with no
   fabricated certainty and no raw payloads by default.
6. **Reference integrity.** §§10–13 give every evidence item stable
   identity, full provenance-chain traceability, source-observation
   linkage by reference, and graph-derived traceability with
   observed-vs-derived visibility.
7. **Context preservation.** §§14–15 preserve capability/completeness and
   sanitized failure/error context with diagnostics distinct from tenant
   findings.
8. **Per-state requirements.** §§16–20 fix `NOT_EVALUATED` (reason for
   inability preserved), `NOT_APPLICABLE` (deterministic rationale
   preserved), `ERROR` (sanitized diagnostic context without leakage),
   `PASS` (affirmative completeness + unsatisfied failure condition;
   missing evidence never `PASS`), and `FAIL` (affirmative failure
   evidence; missing data never automatic `FAIL`).
9. **Aggregation honesty.** §21 preserves per-rule detail with no boolean
   success substitute and reconciles `NOT_EVALUATED` != `FAIL`,
   `NOT_APPLICABLE` != `PASS`, `ERROR` != `FAIL`, renderer output !=
   authoritative finding.
10. **Determinism + serialization.** §§22–23 give semantically equivalent
    records from equivalent inputs with deterministic emission order and
    stable output-neutral serialization (format TBD).
11. **Renderer non-authority.** §§24–25 keep records output-neutral with
    renderer omission/transformation never altering authoritative state.
12. **Hygiene.** §§26–29 enforce per-link sanitization, secret exclusion
    (no tokens/secrets/keys/passwords/codes/headers/credential-bearing
    URLs or configuration), tenant isolation, and opaque untrusted-string
    carriage.
13. **Integrity honesty.** §30 prohibits fabrication, claims no
    cryptographic integrity, and reserves signing/hashing as TBD.
14. **Minimization + correlation.** §§31–32 minimize content to audit
    necessity, prefer references, and correlate deterministically with
    snapshot semantics and no invented fingerprint.
15. **Evolution safety.** §33 prefers additive versioned evolution with no
    silent semantic change and no stable-API promise.
16. **Test readiness.** §34 identifies all required seams without creating
    tests or choosing frameworks.
17. **TBD explicitness.** Every implementation-sensitive unknown is listed
    in §37 with identifier, decision required, why deferred, and owning
    document; no endpoint, permission, property, mapping, package,
    mechanism, algorithm, template, taxonomy, or numeric limit is
    invented or selected.
18. **Non-goal containment.** Nothing in §36 appears as an assumed
    capability.
19. **Maturity honesty.** Nothing herein claims findings, evidence,
    controls, signing, persistence, or readiness is implemented or
    tested — these are intended implementation contracts only.

---

*(End of file)*
