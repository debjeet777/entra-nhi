# Error/Result Model

> **Phase:** 0.3.5
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed shared error/result contract. This document is
  design output only. It authorizes no implementation.
- **Scope:** Defines how EntraNHI represents expected results, domain
  evaluation outcomes, operational failures, cancellation, and
  invariant/programmer failures as four explicitly separated conceptual
  layers (§3), with a conceptual operational-failure taxonomy (§5),
  retryability posture (§6), cancellation semantics (§7), error-identity
  principles (§8), diagnostic-context rules (§9), chaining/causality
  rules (§10), boundary-translation responsibilities (§11),
  partial-failure behavior (§12), invariant-violation handling (§13),
  result-composition rules (§14), and logging/telemetry and exit-code
  boundaries (§§15–16) — used consistently across collectors,
  normalization, graph construction, rules, findings/evidence, and
  output.
- **What this document is:** The owner of the shared error/result
  vocabulary: the conceptual `Result`/`Error` shape, the
  failure-category taxonomy, the mapping of failures to
  `NOT_EVALUATED` vs `ERROR`, diagnostic-vs-finding separation, and
  sanitization rules. First normative definition of the failure
  taxonomy lives here; all other documents use it referentially
  (`README.md` §6).
- **What this document is not:** It is not a C# implementation, an
  exception-type catalog, a provider-error mapping, a retry/backoff
  constant set, a logging-backend selection, an exit-code table, or a
  rule catalog. Detailed consumers live elsewhere: capability states
  in `capability-model.md`, graph operations in
  `identity-graph-contracts.md`, rule pipeline and `RuleEvaluation` in
  `rule-engine-contracts.md`, findings/evidence/provenance schemas in
  `findings-evidence-schema.md`, collector envelopes in
  `collector-contracts.md`, output projections and exit-code mapping
  in `output-renderer-contracts.md`, auth boundaries in
  `authentication-design.md`, and the shared core surface in
  `core-contracts.md` (which references — never duplicates — the
  taxonomy defined here).
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed conceptual contracts** owned by this
  document, and (c) **unresolved TBDs** naming the owning later document
  (§20). Nothing herein claims a control is implemented, verified, or
  enforced. Where architecture states `DESIGNED / REQUIRED`, this document
  preserves that classification.
- **Illustrative syntax discipline:** This document MAY use C#-style
  pseudotypes where that materially improves precision. Any such syntax is
  **illustrative and non-normative** — not final production code, not a
  namespace decision, and not a member list. No framework, package, SDK,
  or library implementation is selected here (CON-003). No source files
  are created by this document.
- **Canonical assessment states (referenced, not redefined):** The
  authoritative assessment states remain EXACTLY `PASS`, `FAIL`,
  `NOT_EVALUATED`, `NOT_APPLICABLE`, `ERROR`, with semantics owned by
  `core-contracts.md` §4. No sixth authoritative assessment state is
  created here. Operational and capability terms defined below are
  explicitly typed as non-verdict concepts.

---

## 2. Error/result design principles

1. **Explicit outcomes (INV-05; INV-14).** Every operation resolves to
   an explicit typed outcome: a domain answer, a capability gap, an
   operational failure, or an invariant violation — never a silent
   success, never an absent output, never a boolean that hides
   `ERROR`/`NOT_EVALUATED`/incompleteness.
2. **Deterministic interpretation (INV-02; VERD-006).** The same
   normalized inputs, capability state, rule version/configuration,
   and failure cause produce the same semantic mapping to
   `NOT_EVALUATED` vs `ERROR` and the same diagnostic shape. No
   randomness, wall-clock branching, or model inference on the
   mapping path.
3. **Typed/conceptual failure categories (§5).** Failures are
   classified by concept (authorization, throttling, timeout, …),
   never by provider exception class name. Exact provider-exception
   mappings are deferred (§20).
4. **Preservation of cause/context (§§9–10).** Sanitized causal
   information travels with the failure across every boundary
   (provider → orchestration → assessment → output-neutral result →
   renderer) without secret leakage or provider-type coupling.
5. **Safe diagnostic information (§9).** Diagnostics carry
   machine-readable codes, human-explainable reasons, and
   tenant-scoped correlation context — and MUST NOT carry secrets,
   tokens, credential-bearing URLs, or secret-bearing configuration.
6. **Secret exclusion (INV-09; SEC-001; SEC-002; OUT-006).** No
   `Result`/`Error` shape admits secret material in any field,
   including nested causes and rendered text.
7. **Tenant isolation (INV-10).** Every error/result is attributable
   to one explicit single-tenant assessment context; cross-tenant
   mixing is an integrity failure (§13), never a merged diagnostic.
8. **Failure transparency (INV-14).** Collection, normalization,
   graph, rule, evidence, and serialization failures surface as
   `ERROR`, `NOT_EVALUATED`, or explicit diagnostics — never silent
   `PASS`, never automatic tenant `FAIL`.
9. **No renderer reinterpretation (INV-12).** Renderers project the
   authoritative result; they MUST NOT change severity, state, or
   meaning, suppress `ERROR`/`NOT_EVALUATED`, or convert failures
   for presentation convenience.
10. **No AI reinterpretation (INV-13; SEC-010; VERD-007; CON-004).**
    Model output MUST NEVER determine, modify, override, or supply
    assessment states, evidence, capability states, severity, or
    failure classifications, and MUST NEVER sit on the
    authentication, authorization, collection, or evaluation path.

---

## 3. Four conceptual layers

The model separates four layers that MUST NOT be collapsed into one
enum or one generic success/failure boolean:

**A. Assessment/rule outcome.** The authoritative per-rule answer:
exactly one of `PASS`, `FAIL`, `NOT_EVALUATED`, `NOT_APPLICABLE`,
`ERROR` (`core-contracts.md` §4). `PASS`/`FAIL` assert evaluated
security posture with evidence; `NOT_EVALUATED` records a
capability/data gap with reason; `NOT_APPLICABLE` records genuine
non-applicability with reason; `ERROR` records untrustworthy
execution with cause/context. This layer answers: *what did the
assessment conclude about the subject?*

**B. Capability/data availability condition.** What could be observed:
the capability states and data-availability conditions owned by
`capability-model.md` (§5) and `domain-types.md` (§10) — observed,
confirmed-empty, unavailable (authorization / licensing-service),
unsupported, not-requested, failed, unknown, not-applicable. This
layer answers: *what was the assessment able to see, and why not?*
It constrains — but never equals — layer A (§4, and
`capability-model.md` §9).

**C. Operational result/failure.** How execution went: per-operation
success, typed operational failure (§5), or cancellation (§7),
carrying retryability hints (§6), sanitized diagnostics (§9), and
causal chains (§10). Failures here are transport/processing facts
(service unavailable, throttling-exhausted, timeout, malformed
response, normalization failure, partial collection, configuration
error, serialization failure, internal operational fault). This
layer answers: *did the machinery work?* Operational failures are
translated into layer A only through the explicit mapping in §4 —
never by an "all exceptions = ERROR" shortcut and never silently.

**D. Programmer/invariant violation.** Broken internal contracts:
corrupted shared normalized state, tenant-context mismatch/
contamination, cross-tenant edge/evidence attempts, contract-
invariant breaches, invalid rule definitions/configurations that
violate structural invariants, fabricated-evidence attempts (§13).
These are integrity failures, not expected operational hazards.
They fail closed and visibly, potentially halting evaluation rather
than producing potentially incorrect results. This layer answers:
*is the assessment itself trustworthy?*

Why the layers cannot collapse:

1. A layer-C failure (e.g., throttling-exhausted on one category)
   is not automatically a layer-A tenant `FAIL`, nor automatically
   a layer-A `ERROR`: scoped gaps on required data normally become
   `NOT_EVALUATED` with capability reason, while trust-breaking
   failures become `ERROR` (§4).
2. A layer-B gap (e.g., unsupported capability) is assessment
   information, not a system crash: it yields `NOT_EVALUATED`, not
   an exception path.
3. A layer-D violation (e.g., tenant mismatch) MUST NOT be caught
   and converted into `PASS`/`FAIL`: the run's integrity is
   compromised, so no verdict from the affected scope is
   trustworthy.
4. Collapsing layers into one boolean ("success") hides exactly the
   distinctions false-PASS resistance depends on: gap vs failure
   vs verdict vs corruption.

```csharp
// Illustrative only. Non-normative. Layer separation sketch —
// exact shapes, member lists, and code identifiers are TBD (§20, T-01).
record OperationOutcome { OutcomeKind Kind; } // conceptual:
// - DomainAnswer (layer A: one of the five states + evidence/reason)
// - CapabilityGap (layer B reference: capability state + reason)
// - OperationalFailure (layer C: failure category + diagnostics + cause)
// - InvariantViolation (layer D: violated invariant + diagnostics)
```

---

## 4. Expected domain/evaluation outcomes

`PASS`, `FAIL`, `NOT_EVALUATED`, and `NOT_APPLICABLE` produced
through the normal pipeline from normalized inputs are assessment
answers, not system failures: they are returned — not thrown — with
evidence (for `PASS`/`FAIL`) or structured reason/context (for
`NOT_EVALUATED`/`NOT_APPLICABLE`), and they aggregate into the
assessment result with that context preserved.

`ERROR` is an authoritative assessment state (layer A) but MUST
retain structured cause/context sufficient to distinguish it from
operational transport details: *which* rule/target/stage failed,
*which* failure category (§5) caused it, and the sanitized
diagnostic chain — without exposing provider exception types, raw
payloads, or secrets as authoritative output.

Where operational failures translate into assessment states:

1. **Scoped capability/data gaps** (authorization-denied category,
   licensing/service-absent, unsupported, not-collected, partial
   required data, conflicting observations) flow into
   `NOT_EVALUATED` (or `NOT_APPLICABLE` where the rule's
   deterministic applicability predicate genuinely excludes the
   subject) with capability reason — per `capability-model.md` §9.
2. **Trust-breaking execution failures** (invalid normalized input
   violating a required invariant, rule execution exception,
   inconsistent graph state preventing evaluation, invalid rule
   definition/configuration, internal deterministic engine failure,
   evidence-reference failure that prevents trustworthy verdicts)
   flow into `ERROR` with structured cause.
3. **Pipeline-stage failures outside any single rule** (graph-
   construction failure, normalization failure, output
   serialization failure, resource exhaustion, cancellation) are
   recorded at their scope: per-rule `ERROR`/`NOT_EVALUATED` where
   attributable, assessment-level diagnostics where systemic —
   never silent success, never tenant `FAIL` (§12).
4. There is deliberately **no simplistic "all exceptions = ERROR"
   rule.** An authorization denial on one required category is a
   scoped gap (`NOT_EVALUATED`), not an engine `ERROR`; a
   malformed-response failure that corrupts shared normalized state
   may be an integrity `ERROR` halting evaluation. The per-category
   mapping table is TBD (§20, T-02), constrained by this section
   and `capability-model.md` §9.

---

## 5. Operational failure taxonomy

Conceptual categories, not provider-specific exception types. Exact
provider-exception mappings are deferred (§20, T-03).

| # | Conceptual failure category | Covers (conceptually) |
| --- | --- | --- |
| F-01 | Authorization failure | Authenticated principal lacks permission for the requested operation; scoped-gap mapping normally applies. |
| F-02 | Authentication boundary failure | Authorized access could not be established at all (no usable authorized context); systemic, not per-rule. |
| F-03 | Provider/service unavailable | Source system not reachable or down for the collection window. |
| F-04 | Throttling/rate limitation | Rate/throttle signals received; bounded-retry behavior exhausted or the operation could not complete within its bound. |
| F-05 | Timeout | Operation exceeded its bound without completing (collection, graph, evaluation, or rendering bound, per owning document). |
| F-06 | Network/transport failure | Connectivity/transport breakdown distinct from service-down and timeout where safely distinguishable; else `Unknown`-cause operational failure. |
| F-07 | Malformed/invalid provider response | Source returned content not matching documented expectations. |
| F-08 | Normalization failure | Source observation failed normalization input validation or translation to normalized contracts. |
| F-09 | Partial collection failure | A subset of expected observations failed while others succeeded; reported as partial with per-subset states, never as single success. |
| F-10 | Configuration failure | Invalid, unknown, or incompatible configuration/rule-set/version preventing trustworthy execution; fails visibly. |
| F-11 | Unsupported capability/operation (operationally relevant) | Requested operation has no implementation or product support in this run; operational face of the capability `Unsupported` state. |
| F-12 | Serialization/output failure | Canonical-result construction or renderer projection failed after assessment semantics were determined; a system/output diagnostic, never a tenant finding. |
| F-13 | Cancellation | Deliberate early termination (§7); distinguishable from failure, timeout, and verdicts. |
| F-14 | Internal/unclassified operational failure | Any other operational fault that does not fit F-01–F-13 and is not an invariant violation; unclassified cause MUST remain explicit, never silently coerced. |

Classification rules:

- Categories are **concepts carried in normalized diagnostics**,
  never provider exception class names and never SDK type names.
- Where the true cause cannot safely be established, the failure is
  recorded with indeterminate cause (an explicit unknown-cause form
  of the nearest category or F-14) rather than a guessed category.
- F-01 (scoped authorization gap) normally maps toward
  `NOT_EVALUATED`; F-02 (no authorized context at all) is systemic
  and may prevent the assessment from starting or invalidate its
  scope — recorded assessment-wide, never as per-rule `PASS`.
- No category maps to tenant `FAIL`. No category maps to `PASS`.

---

## 6. Retryability semantics

Failure classification (§5) is separated from retry policy:

1. A failure MAY carry conceptual retryability information where
   safely known (e.g., a throttling/timeout/transient signal vs a
   deterministic authorization denial vs a malformed-response that
   retry will not fix) — as a *hint observed from documented
   behavior*, not as policy.
2. `Core` MUST NOT invent retryability from provider details, and
   MUST NOT encode provider-specific retry interpretation. Hints
   arrive normalized from the collector/provider boundary with
   provenance; `Core` treats them as data.
3. Exact retry/backoff policy — counts, delays, jitter, budgets,
   timeout values, concurrency bounds, throttling thresholds —
   belongs to collector/provider design (`collector-contracts.md`)
   and is NOT defined here. No numeric constant appears in this
   document.
4. Retry exhaustion is itself explicit: an F-04/F-05 after bounded
   retries records that the bound was exhausted, preserving the
   original cause chain (§10) and flowing into `NOT_EVALUATED` or
   `ERROR` per §4 — never silent `PASS`.

---

## 7. Cancellation semantics

Cancellation is deliberate early termination and MUST remain
distinguishable from provider failure, timeout, rule `FAIL`,
`NOT_EVALUATED`, and `ERROR` where the distinction is meaningful:

1. **Represented explicitly.** Cancelled operations record
   cancellation (F-13) with scope (which operation: collection,
   graph construction, rule-set evaluation, rendering), the
   affected subjects/rules where attributable, and the assessment
   incompleteness that results. Cancellation is returned/propagated
   as a first-class outcome, not thrown away and not relabeled as a
   verdict.
2. **Propagation responsibilities (conceptual).** The boundary that
   observes cancellation (collector, graph, engine, renderer)
   records it; orchestration propagates it without converting it
   into `PASS`/`FAIL`; aggregation preserves the incomplete scope
   visibly (§12, §14). Downstream stages MUST NOT evaluate or
   render cancelled scope as complete.
3. **Resulting assessment state.** Cancellation of scope required by
   a rule normally yields `NOT_EVALUATED` for the affected
   evaluations (with cancellation reason) where evaluation never
   meaningfully began, or `ERROR` where termination broke
   trust/completeness mid-execution per §4 mapping — the exact
   per-scope mapping is TBD (§20, T-02). Cancellation itself is
   never a sixth assessment state: it is the operational cause; the
   assessment state records the consequence.
4. **No framework mechanism selected here.** No
   cancellation-token type, task library, threading model, or
   timeout constant is chosen unless already locked elsewhere
   (none is). Mechanism belongs to `collector-contracts.md`
   (collection side) and `rule-engine-contracts.md` /
   `identity-graph-contracts.md` (evaluation/graph side).

---

## 8. Error identity/code principles

Stable internal diagnostic/error identifiers are required for logs,
output, and tests. Requirements (conceptual; no catalog finalized
here):

1. **Machine-readable.** Codes are stable tokens suitable for
   assertions, log queries, and output projection — not free-text
   matching on provider messages.
2. **Human-explainable.** Each code has a documented meaning and
   remediation-agnostic explanation (what happened, which scope,
   which layer); explanation text carries no secrets.
3. **Stable enough for logs/output/tests.** A code's meaning MUST
   NOT drift across versions; splitting or redefining a code is a
   versioned change, never silent.
4. **No secrets.** Codes and their explanations carry no token,
   credential, key, password, header, or secret-bearing value.
5. **No raw provider content.** Codes MUST NOT embed provider
   exception class names, HTTP status text verbatim, or raw
   provider strings; provider signals are normalized into §5
   categories first.
6. **Not tied to provider exception class names.** The taxonomy is
   concept-first (§5); provider mappings live behind the adapter
   boundary (§11) and are deferred (§20, T-03).

A complete error-code catalog is NOT finalized in this document
(TBD §20, T-04).

---

## 9. Diagnostic context

Safe diagnostic context accompanies failures and non-binary
outcomes. Permitted content (conceptual):

- Operation category (collection, normalization, graph, rule
  evaluation, findings/evidence construction, output projection).
- Tenant-scoped context using safe identifiers (single-tenant
  assessment reference; no cross-tenant linkage).
- Subject reference where safe (assessment-local normalized key /
  safe source reference; never a secret-bearing value).
- Capability/rule reference (capability identifier, rule ID/version,
  pipeline stage).
- Provider category (source family as a normalized value — e.g.,
  the primary source family vs an explicitly enabled enrichment
  source — never SDK types or endpoint paths).
- Correlation/operation identifier (assessment/run/operation
  reference enabling end-to-end tracing).
- Timestamp where supplied by controlled context (collection-window
  / observation-time context; no wall-clock reads inside evaluation
  logic per `domain-types.md` §12).
- Cause chain in sanitized form (§10).

Explicitly prohibited in every diagnostic field, including nested
causes and rendered text:

- Access tokens and refresh tokens.
- Client secrets and secret values of any kind.
- Private keys and private-key material.
- Passwords and recovery codes.
- Raw authorization headers.
- Credential-bearing URLs/query strings.
- Secret-bearing configuration values.

Provider-originated text is untrusted data carried opaquely; it is
never trusted as code, path, or markup, and encoding obligations
belong to renderers (`output-renderer-contracts.md`).

---

## 10. Error chaining / causality

1. Failures preserve enough causal information for troubleshooting:
   the §5 category at each level, the boundary where each
   translation occurred (§11), and the sanitized cause chain from
   origin to assessment consequence.
2. Chains are sanitized at every link: secrets stripped, provider
   exception types normalized away, raw payloads excluded by
   default (raw-response diagnostic retention policy, if any,
   belongs to `collector-contracts.md` and MUST exclude secrets).
3. Raw provider exceptions MUST NOT surface as authoritative public
   output. They may exist transiently behind the adapter boundary
   for adapter-local handling; across the normalization boundary
   only the normalized category + sanitized diagnostics travel.
4. Chain depth is bounded by usefulness: each link MUST add scope
   information (which operation, which subject, which stage); pure
   re-wrapping without new information is prohibited.

---

## 11. Boundary translation

Conceptual translation responsibilities between layers:

```
Provider / Infrastructure
  (provider signals, SDK-adjacent handling — behind adapter boundary)
  -> Application orchestration
    (scoped gaps vs systemic failures; per-rule vs assessment-wide;
     cancellation propagation; partial-failure shaping)
  -> Core assessment semantics
    (capability-aware NOT_EVALUATED vs trust-breaking ERROR per §4
     and capability-model.md §9; invariant violations fail closed)
  -> Output-neutral result
    (authoritative states + evidence/reason + sanitized diagnostics;
     format-agnostic, per core-contracts.md §11)
  -> Renderer / CLI
    (projection only: no severity/state/meaning change,
     no suppression, no re-evaluation)
```

Rules:

1. Provider-specific exceptions MUST NOT leak into `Core`
   contracts — by type, by name, or by behavior
   (`core-contracts.md` §12). The adapter normalizes them into §5
   categories with sanitized diagnostics before the normalization
   boundary.
2. Orchestration routes but never re-authors: it MUST NOT convert
   `ERROR` to `FAIL`, suppress `NOT_EVALUATED`, or fabricate
   `PASS`/`FAIL` to simplify aggregation.
3. `Core` maps normalized failures to assessment states only
   through §4 and `capability-model.md` §9 — never by inspecting
   provider artifacts.
4. The output-neutral result carries the full state + reason +
   diagnostics; renderers project it. Renderer failure is a
   layer-C diagnostic at the renderer boundary (§12 rule 4) —
   never a tenant finding and never silent success.
5. AI consumers, if any ever exist, attach downstream of the
   canonical result as non-authoritative consumers only (INV-13).

---

## 12. Partial failure

1. **One collector fails but others succeed.** Successfully
   collected categories keep their `Observed` state; the failed
   category carries its gap/failure state. Per-category granularity
   (`capability-model.md` §11) prevents one failure from
   invalidating unrelated observations. Affected rules yield
   `NOT_EVALUATED`/`ERROR` per §4; unaffected rules evaluate
   normally.
2. **One subject fails while others succeed.** Per-subject
   isolation: the failed subject's evaluations record the gap with
   reason; other subjects are unaffected. Aggregation preserves the
   per-subject failure visibly (§14).
3. **One rule errors while others can evaluate.** Per-rule
   isolation: the errored rule records `ERROR` with cause; unrelated
   evaluations continue where safe. Shared-state integrity failures
   (§13) are the exception: they fail closed, potentially halting
   evaluation rather than risking incorrect results.
4. **Output rendering fails after assessment completed.** The
   authoritative assessment result stands; the failure is recorded
   as a system/output diagnostic (F-12) with scope (which renderer,
   which artifact), partial-artifact handling, and safe file
   behavior — owned in detail by `output-renderer-contracts.md`.
   Renderer failure MUST NOT rewrite assessment states and MUST NOT
   present partial artifacts as complete.
5. The assessment MUST NOT automatically abort in full on every
   scoped failure unless architecture requires it or correctness
   cannot be preserved; equally, usable partial results MUST NOT
   hide incompleteness — the incomplete scope stays explicit in
   summaries and diagnostics.

---

## 13. Invariant/programmer violations

Expected operational failures (§5) are handled; impossible/internal
invariant violations fail visible:

1. **Scope.** Corrupted shared normalized state, tenant-context
   mismatch/contamination, cross-tenant edge/evidence attempts,
   contract-invariant breaches, structurally invalid rule
   definitions/configurations, fabricated-evidence attempts.
   Tenant-context mismatch is always in this class
   (`core-contracts.md` §9).
2. **Handling.** Fail closed and visibly: halt the affected scope
   (or the whole evaluation where shared state is compromised)
   with explicit diagnostics identifying the violated invariant —
   never silent continuation, never conversion into `PASS`/`FAIL`,
   never automatic `NOT_EVALUATED` relabeling that would hide
   corruption as a mere data gap.
3. **No silent conversion.** Catching invariant violations and
   converting them into `PASS`/`FAIL` (or into ordinary
   `NOT_EVALUATED`) to keep output tidy is prohibited. Where an
   invariant violation also prevents a rule from evaluating, the
   recorded outcome MUST preserve the integrity-failure cause
   distinctly from a scoped capability gap.
4. **Detection surfaces.** Structural assertions in tests
   (`dependency-boundaries.md` enforcement intent), validation at
   normalization/graph/rule-definition boundaries, and explicit
   tenant-scope checks at every transformation boundary. Exact
   assertion mechanics belong to `testing-seams.md` and
   `dependency-boundaries.md`.

---

## 14. Result composition

Conceptual composition of multiple operational results and
assessment results:

1. Composition preserves detail: per-rule evaluations, per-subject
   gaps, per-category capability states, evidence/reason references,
   and diagnostics all survive aggregation. Aggregation MUST
   preserve — not summarize away — `ERROR` and `NOT_EVALUATED`
   conditions (`core-contracts.md` §3.9).
2. No "overall success" boolean substitutes for the five-state
   model. Any summary indicator (counts, rollups) is derived
   metadata over preserved detail — it MUST NOT hide `ERROR`,
   `NOT_EVALUATED`, or incomplete collection, and MUST NOT be
   consumable as a security verdict by itself.
3. Composition is deterministic: same per-rule outcomes plus same
   configuration produce the same aggregate shape and the same
   derived summary values.
4. Aggregate severity policy (if any rollup semantics beyond
   per-rule severity metadata are ever needed) is NOT finalized
   here unless already approved elsewhere (none is); per-rule
   severity flows into findings/output without altering evaluation
   logic. TBD (§20, T-05).

---

## 15. Exit-code boundary

A future CLI may map authoritative result/error semantics to process
exit codes, but exact mappings belong to
`output-renderer-contracts.md` and/or CLI implementation design.
No exit code is chosen here. Constraints carried forward:

- The mapping MUST preserve the five-state distinction and MUST
  NOT collapse `ERROR`/`NOT_EVALUATED`/incompleteness into a
  generic zero/non-zero success signal that invites false-secure
  CI gating.
- Renderer/output failure after completed assessment MUST remain
  distinguishable from assessment verdicts in whatever mapping is
  eventually designed.
- CLI syntax and exit-code finalization are TBDs owned by
  `output-renderer-contracts.md` (with configuration semantics
  from `configuration-design.md`).

---

## 16. Logging/telemetry boundary

1. Error/result information supplied to logging/telemetry
   abstractions is limited to the sanitized diagnostic content in
   §9: codes, categories, safe identifiers, correlation IDs, and
   redacted reason text. No secret-bearing payload is ever
   supplied.
2. No logging vendor, backend, format, verbosity default, or
   retention policy is selected here (CON-003). `Core` MUST NOT
   reference a concrete logging/telemetry backend
   (`core-contracts.md` §12); any minimal abstraction boundary is
   owned by `dependency-boundaries.md`.
3. No undisclosed telemetry or third-party transmission of tenant
   assessment data. Destinations, if any, are explicit,
   operator-visible, and documented — tenant-originated strings
   remain untrusted data in transit.
4. Tenant isolation applies to telemetry as to output: no
   cross-tenant correlation or aggregation in V1.

---

## 17. Security implications

Stated as structural support, not as implemented controls:

- **False-PASS prevention (INV-05; INV-14).** Layer separation (§3)
  plus §4 mapping plus composition rules (§14) leave no path from
  failure or gap to `PASS`. Every failure/gap terminates in
  `NOT_EVALUATED`, `ERROR`, or an explicit diagnostic.
- **Sanitization (INV-09).** §9 prohibitions plus §10 chain hygiene
  plus §16 payload limits leave secrets no field or path into
  domain, graph, findings, evidence, diagnostics, output, logs, or
  persisted artifacts.
- **Tenant isolation (INV-10).** Single-tenant scoping on every
  outcome; mismatch handled as §13 integrity failure.
- **Untrusted provider error text.** Provider-originated messages
  are untrusted data: carried opaquely, never interpreted as markup,
  paths, or commands; injection defense is a renderer obligation.
- **Output injection concerns.** Canonical result pre-encodes for
  no format; escaping obligations belong to renderers
  (`output-renderer-contracts.md`).
- **Denial/resource-exhaustion considerations.** Bounded processing,
  cancellation (§7), and resource safeguards keep pathological
  inputs from silently producing `PASS` or hanging the run;
  exhaustion surfaces explicitly. Numeric bounds are NOT chosen
  here (TBD, §20).
- **Fail-transparent behavior.** Every layer-C and layer-D
  condition is visible in results, summaries, and diagnostics;
  nothing degrades silently into success.

---

## 18. Testability implications

Tests required per boundary (scenario catalog itself belongs to
`testing-seams.md`; no tests are created here):

- Every §5 failure category F-01–F-14, each with a synthetic
  fixture exercising classification without provider types.
- Assessment/error separation: scoped gap → `NOT_EVALUATED` vs
  trust-breaking failure → `ERROR`, with no "all exceptions =
  ERROR" shortcut and no verdict-from-failure leakage.
- Capability/error separation: `Unsupported`/unavailable causes
  stay distinguishable from operational failures end to end.
- Cancellation: mid-collection, mid-evaluation, and mid-render
  cancellation each propagate visibly with incomplete scope
  preserved — distinct from timeout, failure, and verdicts.
- Partial failure: per-collector, per-subject, per-rule, and
  post-assessment renderer failures (§12) with preserved usable
  results and explicit incompleteness.
- Sanitization: secret-bearing synthetic payloads (token-shaped,
  key-shaped, header-shaped, URL-shaped, config-shaped) MUST have
  no path into diagnostics, logs, or output.
- Causal chaining: multi-link causes preserve scope information at
  each link with secrets stripped and provider types normalized
  away.
- Deterministic result composition: identical per-rule outcomes
  yield identical aggregates; no boolean hides `ERROR` /
  `NOT_EVALUATED` / incompleteness.
- Renderer non-authority: identical canonical results in yield
  identical semantics out, across all formats, including failure
  and cancellation projections.

---

## 19. Explicit non-goals

1. No sixth authoritative assessment state and no generic boolean
   "success" substitute for the five-state model.
2. No provider-exception mapping table, SDK-call reference, or
   endpoint/permission catalog.
3. No concrete retry counts, delays, backoff constants, timeout
   values, concurrency bounds, or resource limits.
4. No framework-specific cancellation implementation and no
   threading/task-library selection.
5. No complete error-code catalog (principles only, §8).
6. No exit-code table, CLI syntax, SARIF mapping, HTML templating,
   or signing/hashing implementation.
7. No logging vendor/backend selection and no telemetry-destination
   design.
8. No aggregate severity policy beyond approved per-rule severity
   metadata.
9. No SaaS/multi-tenant service, dashboard, remediation,
   monitoring, AI verdicts, or other NG-series capabilities.

---

## 20. Open TBDs and owner documents

Each TBD states what is unknown, why it cannot be resolved here, and
which later document owns its resolution. None is resolved by
speculation.

| # | TBD | Why unresolved here | Owned by |
| --- | --- | --- | --- |
| T-01 | Concrete `Result`/`Error` record shapes, member lists, and code-identifier spelling for categories/codes. | Member-level design needs coordination with capability, rule, findings, collector, and output owners; choosing members here would preempt them. | This document (taxonomy/semantics) coordinated with `core-contracts.md`; physical shapes per consumer in their owning documents |
| T-02 | Per-category mapping table: each §5 failure category × pipeline stage → `NOT_EVALUATED` vs `ERROR` (including cancellation-scope mapping, §7 rule 3). | Mapping needs rule-engine, graph, and collector coordination; a premature table risks inventing semantics. | This document (taxonomy) with `rule-engine-contracts.md` (evaluation-side mapping) and `collector-contracts.md` (collection-side mapping), constrained by `capability-model.md` §9 |
| T-03 | Exact provider-signal → §5 category mappings (which documented provider behaviors normalize to which category). | Requires validation against published documentation; inventing mappings here would violate INV-15. | `collector-contracts.md` (mappings), taxonomy owned here |
| T-04 | Complete error-code catalog: code list, meanings, stability guarantees. | Code proliferation before shapes exist invites drift; principles (§8) suffice at this stage. | This document, with projection via `output-renderer-contracts.md` |
| T-05 | Aggregate severity/summary policy detail beyond per-rule severity metadata (§14 rule 4). | Needs findings/output coordination; no policy is approved elsewhere. | `rule-engine-contracts.md` (severity taxonomy) with `findings-evidence-schema.md` and `output-renderer-contracts.md` |
| T-06 | Retry/backoff/timeout/concurrency constants and throttling-threshold policy. | Operational constants need implementation-phase measurement and documentation validation. | `collector-contracts.md` with `dependency-boundaries.md` |
| T-07 | Cancellation mechanism (token/task/pattern choice) per pipeline side. | Mechanism choice needs collector/rule/graph coordination. | `collector-contracts.md` (collection side), `rule-engine-contracts.md` / `identity-graph-contracts.md` (evaluation/graph side) |
| T-08 | Assessment-time representation and resource/bounding limits touching error paths (timeouts, exhaustion). | Time and bound design belong to rule-engine and testing-seam surfaces. | `rule-engine-contracts.md` with `testing-seams.md` (time principles from `domain-types.md` §12) |
| T-09 | Identifier formats (assessment, evaluation, finding, evidence, provenance, correlation IDs carried in §9). | Formats need dedicated design plus structural-test enforcement. | `findings-evidence-schema.md` with `domain-types.md` |
| T-10 | Evidence/provenance/diagnostic record schemas carrying §9–§10 content, emission policy, redaction schema, hashing/signing/timestamping posture. | Integrity mechanisms need separate security review. | `findings-evidence-schema.md` |
| T-11 | Collector interfaces, source-observation envelope schema carrying failures, pagination/retry/cancellation surfaces, raw-response diagnostic retention policy. | Collector mechanics are a dedicated surface. | `collector-contracts.md` |
| T-12 | Canonical output projection of errors/diagnostics, per-format schemas, injection/file-safety policies, exit-code mapping, renderer-failure isolation policy. | Presentation design belongs downstream of assessment semantics. | `output-renderer-contracts.md` (with `configuration-design.md` for output options) |
| T-13 | Logging/telemetry abstraction surface, enforcement tooling, and backend selection. | Seam-catalog and supply-chain decisions belong downstream. | `dependency-boundaries.md` |
| T-14 | Per-seam failure-injection scenario catalog, golden/snapshot policy for error shapes. | Test-scenario design belongs to the testing surface. | `testing-seams.md` |
| T-15 | Fatal-vs-isolated engine failure policy detail and invalid-configuration semantics touching §5 F-10. | Engine-failure policy needs rule-engine design. | `rule-engine-contracts.md` with `configuration-design.md` |

---

## 21. Acceptance criteria

This error/result model is accepted when:

1. **Principle coverage.** §2 establishes explicit outcomes,
   deterministic interpretation, typed categories, cause/context
   preservation, safe diagnostics, secret exclusion, tenant
   isolation, failure transparency, and renderer/AI
   non-authority — as constraints, not implementation claims.
2. **Layer separation.** §3 distinguishes assessment outcome,
   capability/data condition, operational result/failure, and
   programmer/invariant violation with stated reasons they cannot
   collapse into one enum or boolean.
3. **Outcome precision.** §4 keeps `PASS`/`FAIL`/`NOT_EVALUATED`/
   `NOT_APPLICABLE` as returned answers and `ERROR` as a
   cause-carrying assessment state, with explicit translation rules
   and no "all exceptions = ERROR" shortcut.
4. **Taxonomy coverage.** §5 defines F-01–F-14 conceptually —
   authorization, authentication-boundary, service-unavailable,
   throttling, timeout, transport, malformed response,
   normalization, partial collection, configuration, unsupported
   operation, serialization/output, cancellation, and
   internal/unclassified — without provider exception types.
5. **Retry/cancellation soundness.** §6 separates hints from policy
   with no numeric constants; §7 keeps cancellation distinct from
   failure, timeout, and verdicts with propagation duties and no
   framework selection.
6. **Identity/diagnostic hygiene.** §8 requires stable,
   explainable, secret-free, provider-decoupled codes (no catalog
   finalized); §9 permits safe context while prohibiting tokens,
   secrets, keys, passwords, headers, credential-bearing URLs, and
   secret-bearing configuration; §10 preserves sanitized causality
   without raw provider exceptions in public output.
7. **Translation integrity.** §11 fixes provider → orchestration →
   core → output-neutral → renderer duties with no provider-type
   leakage into `Core` and no renderer state authority.
8. **Partial-failure and invariant discipline.** §12 preserves
   usable results without hiding incompleteness and without
   automatic full abort; §13 fails closed and visibly on integrity
   violations with no silent conversion to verdicts.
9. **Composition/exit/logging restraint.** §14 forbids boolean
   success substitutes and finalizes no severity rollup; §15
   chooses no exit codes; §16 selects no backend and admits no
   secret-bearing telemetry.
10. **False-PASS battery.** Permission-limited, unsupported, failed,
    unknown, and unproven-partial required conditions MUST NOT
    yield `PASS` through any §4/§5/§12/§14 path; confirmed-empty is
    `PASS` only per proven rule semantics; `NOT_APPLICABLE` never
    covers gaps; `ERROR` stays distinct from `NOT_EVALUATED` and
    from tenant `FAIL`.
11. **TBD explicitness.** Every implementation-sensitive unknown is
    listed in §20 with its owning document; no endpoint,
    permission, property, mapping, package, mechanism, code
    catalog, or numeric limit is invented.
12. **Non-goal containment.** Nothing in §19 appears as an assumed
    capability.
13. **Cross-document agreement.** The consistency contract with
    `core-contracts.md` (§§4, 9) and `domain-types.md` (§10) holds:
    identical five states (referenced, not redefined), capability
    conditions constraining — never equaling — verdicts,
    provider isolation, provenance preservation, secret exclusion,
    determinism, failure transparency, renderer non-authority, AI
    non-authority — with detailed schemas deferred to their owners
    via explicit cross-references, and full agreement with
    `capability-model.md` transition semantics.

---

*(End of file)*
