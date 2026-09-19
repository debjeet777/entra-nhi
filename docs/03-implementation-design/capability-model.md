# Capability Model

> **Phase:** 0.3.5
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed conceptual capability and data-availability model. This
  document is design output only. It authorizes no implementation.
- **Scope:** Defines the provider-independent capability and
  data-availability semantics consumed by collectors, application
  orchestration, `Core` (graph, rules, findings), and output projections,
  as referenced by `core-contracts.md` §8 and `domain-types.md` §10.
  It fixes the conceptual capability taxonomy, capability-identity
  requirements, capability/observation states, data-availability semantics,
  rule-prerequisite mechanics, capability-to-assessment transition
  semantics, partial-collection behavior, aggregation behavior, provider
  responsibility split, licensing/authorization modeling posture,
  caching/freshness requirements, and determinism — without selecting
  provider endpoints, permissions, SDKs, packages, or numeric constants.
- **What this document is:** The owner of capability-state semantics:
  conceptual state vocabulary, per-category granularity, propagation
  shape, per-rule prerequisite declaration mechanics, and the normative
  mapping from capability/data conditions onto the five authoritative
  assessment states. First normative definition of capability-state
  vocabulary lives here; all other documents use it referentially
  (`README.md` §6).
- **What this document is not:** It is not a C# implementation, an endpoint
  catalog, a permission/scope list, a property mapping, an Agent ID
  mapping, a licensing-behavior claim, a cache implementation, a retry
  policy, or a rule catalog. Detailed consumers live elsewhere:
  normalized domain shapes in `domain-types.md`, graph operations in
  `identity-graph-contracts.md`, rule pipeline and `RuleEvaluation` in
  `rule-engine-contracts.md`, findings/evidence/provenance schemas in
  `findings-evidence-schema.md`, error/result shapes in
  `error-result-model.md`, collector envelopes in `collector-contracts.md`,
  output projections in `output-renderer-contracts.md`, and the shared
  core surface in `core-contracts.md` (which references — never
  duplicates — the semantics defined here).
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed conceptual contracts** owned by this
  document, and (c) **unresolved TBDs** naming the owning later document
  (§19). Nothing herein claims a control is implemented, verified, or
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
  created here. Every capability/data term defined below is explicitly
  typed as a capability/data concept, never as an assessment state.

---

## 2. Why capability modeling exists

Assessment correctness depends on knowing not only *what was observed*
but *what could have been observed*. Capability modeling exists to keep
the following ten concepts distinguishable end to end (collection →
normalization → graph → rule evaluation → findings → output):

1. **Identity/resource existence** — whether a source system holds an
   identity, relationship, or attribute at all. Existence is a property
   of the assessed environment, not of the assessment run.
2. **Requested collection** — whether the assessment run asked for a
   data category within its configured scope. Unrequested data is a
   scope fact, not evidence of absence.
3. **Provider/API support** — whether the current provider and product
   surface exposes the data category through a documented observable
   capability. Lack of support is a product boundary, not a tenant
   security property.
4. **Authorization/permission availability** — whether the authenticated
   principal was permitted to read the requested category. Denial is an
   access fact, not a finding about the assessed identities.
5. **Licensing/service capability where relevant** — whether the target
   environment exposes the category given its service configuration.
   Absence is an environment-capability fact, never a verdict, and no
   commercial-behavior claim is made here (§13).
6. **Successful observation** — data was requested, supported,
   authorized, collected, normalized, and is present as a domain fact
   with provenance.
7. **Confirmed empty result** — the source was completely and
   successfully observed for the category and explicitly indicates no
   value or an empty relationship, where that distinction is documented
   and reliable. Confirmed-empty is meaningful only together with a
   completeness claim (§6, §9, §10).
8. **Unavailable data** — requested and in principle supported, but not
   accessible in this run (authorization, licensing/service, or
   transient cause — cause recorded where safely knowable, else
   indeterminate).
9. **Unsupported data** — the current implementation does not collect
   the category, or the provider/product surface does not support it.
10. **Failed collection** — collection was attempted but an operational
    failure prevented retrieval; distinct from confirmed-empty and
    from never-attempted.

Collapsing any of these into "no rows returned" produces false-PASS
risk (INV-05; INV-07; INV-14): a rule that treats *nothing observed*
as *nothing wrong* without a completeness basis asserts security from
ignorance. Capability modeling forces every rule to confront
completeness before asserting `PASS`, while keeping provider-specific
detection mechanics out of `Core` (§12).

---

## 3. Capability taxonomy

Conceptual capability categories sufficient for V1. Categories are
*abilities to observe documented assessment-relevant state*, not API
routes, not permission names, and not SDK calls. The taxonomy is
extensible (§3.2); granularity rules follow (§11).

### 3.1 V1 conceptual categories

| # | Conceptual capability | Observes (conceptually) |
| --- | --- | --- |
| C-01 | Identity enumeration | Application registrations and service principals as normalized identity records (`domain-types.md` §3). |
| C-02 | Accountability/ownership relationships | Owner, sponsor, and manager relationships where documented; no other accountability category is in V1 scope. |
| C-03 | Credential lifecycle metadata | Non-secret credential metadata only (type/category, non-secret identifiers where documented non-secret, validity timestamps where available); never secret values. |
| C-04 | Permission/access information | Permission/access-assignment relationships (declaration vs grant/assignment vs resource vs principal preserved per `domain-types.md` §8). |
| C-05 | Identity relationships | Identity-link relationships (e.g., application registration to service principal linkage) and other documented identity-to-identity relationships. |
| C-06 | Supported agent-identity information | Agent-identity observations only where documented observable capability exists; capability-gated so unsupported/unavailable agent semantics remain explicit and never block unrelated assessment. |

No exact source endpoint, property, permission/scope, SDK call,
identifier mapping, or licensing prerequisite is stated for any
category. All such mappings are TBDs (§19).

### 3.2 Extensibility

- New categories enter **additively** with a documented evidence basis,
  explicit capability state, and provenance preservation.
- Extension MUST NOT silently reinterpret an existing category, merge
  two categories, or change any rule's completeness bar without a
  versioned rule change (`core-contracts.md` §13).
- A future source family enters as an isolated, explicitly enabled,
  separately provenanced capability; provider independence is
  structural, not a V1 multi-provider support claim
  (`domain-types.md` §13).

---

## 4. Capability identity

Stable capability identifiers are required so rules can declare
prerequisites (§8), diagnostics can reference causes (§7), and tests
can fixture each state (§17).

Requirements:

1. **Provider-independent at the `Core` boundary.** Identifiers name
   the *normalized observable ability* (e.g., the C-01–C-06 concepts),
   never a provider route, SDK type, response field, or permission
   string. Provider-specific detection detail stays in collectors
   (§12) and arrives in `Core` only as normalized state plus
   provenance.
2. **Stable semantic meaning.** An identifier's meaning MUST NOT drift
   across versions: narrowing, widening, or splitting a category is a
   versioned taxonomy change with supersession metadata, never a
   silent redefinition.
3. **Suitable for rule prerequisites.** Identifiers are addressable at
   the granularity rules declare requirements against (§8, §11):
   per category and, where needed, per subject (per-identity scoping)
   without conflating subject-level gaps with assessment-level gaps.
4. **No implementation names embedded.** Identifiers MUST NOT embed
   SDK/API type names, endpoint paths, permission names, commercial
   tier names, or product-build strings.

Exact code-constant spelling, casing, serialization, and wire format
are TBD (§19, T-01): this document fixes the conceptual vocabulary and
the requirements above, not the literal tokens. No naming constant is
finalized here beyond the C-01–C-06 conceptual labels, which are
document conveniences, not code identifiers.

---

## 5. Capability state / observation state

Conceptual representation of *what the assessment run established
about a capability*. The vocabulary below is normative as concepts;
exact code spelling is TBD per §4.

| Conceptual state | Meaning |
| --- | --- |
| `Observed` | Supported, authorized, requested, and successfully collected and normalized for the scoped subject(s). The only state that can ground a completeness claim. |
| `NotRequested` | Supported in principle but outside this run's collection scope (not requested / not collected). A scope fact, never evidence of absence. |
| `UnavailableAuthorization` | Requested but not accessible due to an authorization/permission limitation of the assessing principal. Collection authorization (what the assessing principal may read) MUST NOT be conflated with assessed-identity permissions (what an identity under assessment possesses). |
| `UnavailableLicensingService` | Requested but not exposable due to a licensing/service limitation of the target environment, where that cause is safely knowable. No commercial-tier assumption is embedded; where the cause cannot be safely distinguished, `Unknown` is used instead of guessing. |
| `Unsupported` | Not collectible by the current provider/product capability or the current implementation. Includes capability-gated agent-identity semantics on environments without documented support. |
| `Failed` | Collection was attempted but an operational failure prevented retrieval (transport, service, throttling-exhausted, malformed response, normalization-input validation, timeout, or other operational cause — cause taxonomy owned by `error-result-model.md`). |
| `Unknown` | Indeterminate: the cause cannot safely be established, or conflicting/partial observations prevent a trustworthy classification. The safe default when in doubt. |

Semantic rules:

1. These are **capability/observation states, not assessment states.**
   No row above is a verdict; none authorizes `PASS`/`FAIL` by itself.
   Transition to verdicts is governed exclusively by §9.
2. `Observed` vs confirmed-empty is a §6 (data-level) distinction, not
   a capability-state distinction: `Observed` means the observation
   channel worked; whether the observed content is a value or a
   confirmed-empty is carried by data-availability semantics (§6).
3. `NotRequested` MUST NOT be treated as `Observed`-empty. A rule that
   requires the category MUST NOT evaluate its security predicate on
   `NotRequested` data.
4. `Failed` and `Unknown` MUST propagate visibly; collectors MUST NOT
   degrade them into `NotRequested` or into silent empty results
   (failure transparency, INV-14).
5. `UnavailableAuthorization`, `UnavailableLicensingService`,
   `Unsupported`, `Failed`, and `Unknown` MUST all be representable
   per category and per subject (§11), so one gap never invalidates
   unrelated successfully collected categories.

```csharp
// Illustrative only. Non-normative. Conceptual vocabulary —
// exact member list, naming, and serialization are TBD (see §19, T-01).
record CapabilityObservation {
  CapabilityRef Capability; SubjectRef Subject;
  CapabilityState State; // Observed | NotRequested |
    // UnavailableAuthorization | UnavailableLicensingService |
    // Unsupported | Failed | Unknown (conceptual)
  ProvenanceRef Provenance; DiagnosticRef Cause;
}
```

---

## 6. Data availability semantics

Individual normalized facts/collections carry data-availability
semantics that preserve — and agree with — `domain-types.md` §10.
This document consumes that vocabulary at the capability boundary; it
MUST NOT redefine it.

| Data condition | Meaning |
| --- | --- |
| Observed value | Value present in the source and collected with provenance. |
| Confirmed empty | Source explicitly indicates no value / empty relationship where that distinction is documented and reliable, under a complete observation (§9, §10). |
| Unavailable | Attempted but not accessible (authorization / licensing / service cause where knowable). |
| Unsupported | Source or implementation does not support this data. |
| Not requested / not collected | Outside this run's collection scope. |
| Failed / error condition | Attempted but an error prevented retrieval; or conflicting observations requiring explicit conflict metadata rather than silent overwrite. |
| Unknown / indeterminate | Cause or content cannot safely be established. |
| Not applicable | Concept does not apply to this identity kind (e.g., a relationship category the kind cannot bear). |

Rules:

1. These are **data/observation semantics, not assessment states.**
2. Null, absent, or empty collections MUST NOT be silently mapped to
   "no issue." An empty collection is `Confirmed empty` only when the
   observation channel (`Observed` capability state) plus the rule's
   completeness semantics jointly establish completeness; otherwise it
   is `Unavailable`, `Failed`, `Unknown`, `NotRequested`, or
   `Unsupported` with reason preserved.
3. Whether verified-complete absence yields `PASS` or another state is
   determined per rule by its explicit completeness semantics — never
   by a universal absence rule (`core-contracts.md` §4, rule 2).

---

## 7. Capability evidence/provenance

Every capability determination carries enough provenance and reason
information for later explanation, audit, and evidence linkage:

- Capability identifier (§4) and conceptual state (§5).
- Scoped subject reference(s) where per-subject granularity applies
  (§11), using safe identifiers only.
- Collection operation/context reference (which collection concern
  produced the determination), without provider SDK types.
- Assessment/tenant context reference (single-tenant scope; mismatch
  is an integrity failure per `core-contracts.md` §9).
- Observation-time / collection-window context where appropriate
  (window semantics, not snapshot claims).
- Sanitized cause/reason: which of authorization / licensing-service /
  unsupported / failure / scope-exclusion / indeterminacy applies,
  and the sanitized operational cause where relevant (failure
  taxonomy owned by `error-result-model.md`).
- Derivation lineage for derived capability facts (derived from which
  observations, by which deterministic basis).

Detailed findings/evidence/diagnostic record schemas, identifier
formats, serialization, emission policy, fingerprint strategy,
redaction, and integrity posture remain owned by
`findings-evidence-schema.md`. This section requires only that the
capability facts handed downstream are sufficient inputs to those
schemas. Provenance MUST NOT carry secrets, tokens, or
credential-bearing configuration (see §16).

---

## 8. Rule prerequisites

A deterministic rule declares capability/data prerequisites
conceptually so the engine can decide whether evaluation is safe and
complete enough to produce `PASS`/`FAIL`:

1. **Declaration content (conceptual):** the capability identifiers
   (§4) the rule requires; the required data-availability condition
   per prerequisite (e.g., `Observed` content, or `Observed` content
   that may be `Confirmed empty` under the rule's completeness
   semantics); the subject scope (which identity kinds / relationship
   scopes); and the unavailable-input behavior (which existing
   assessment state results when a prerequisite is unmet).
2. **Evaluation order:** applicability first, then capability
   validation, then input validation, then predicate execution — per
   the pipeline owned by `rule-engine-contracts.md`. Capability
   validation MUST precede security-predicate execution: a rule MUST
   NOT execute its failure predicate against data whose required
   capability state is unmet.
3. **Unmet prerequisites:** when a required prerequisite is not
   satisfied, the rule uses the correct existing non-binary
   assessment state — normally `NOT_EVALUATED`, or `NOT_APPLICABLE`
   where the rule's deterministic applicability predicate establishes
   genuine non-applicability — with structured reason identifying the
   missing capability/data and its state. Fabricating `PASS` or
   `FAIL` from unmet prerequisites is prohibited (§9).
4. **No universal mapping:** the rule definition controls
   deterministic state semantics within §9 constraints. Different
   rules MAY handle the same capability gap differently
   (`NOT_EVALUATED` vs `NOT_APPLICABLE`) based on documented
   applicability and evaluation logic.

The detailed rule-schema mechanics (declaration syntax, registration,
compatibility, versioning, pipeline stage contracts) belong to
`rule-engine-contracts.md`. This document fixes only the conceptual
prerequisite obligations above.

---

## 9. Capability-to-assessment transition semantics

Capability state does NOT directly equal assessment state. The table
below is the normative constraint: for a given capability/data
condition on data **required** by the rule, only the listed
assessment outcomes are permissible. The rule definition selects
within the permitted set; nothing outside it is permitted.

| # | Required-data condition | Permitted assessment outcomes | Rationale |
| --- | --- | --- | --- |
| R-01 | All required observations `Observed` with content present; failure predicate satisfied with evidence | `FAIL` | Affirmative deterministic failure evidence under complete inputs. |
| R-02 | All required observations `Observed` (content present or `Confirmed empty`); completeness established per rule semantics; failure condition not satisfied | `PASS` | `PASS` requires affirmative completeness plus unsatisfied failure condition; evidence required. |
| R-03 | Required observations `Observed` but `Confirmed empty`, where the rule's explicit semantics establish that emptiness under complete collection is the secure condition | `PASS` permitted (rule must prove it) | Confirmed-empty is NOT automatically `PASS`; only the rule's documented completeness semantics can authorize it. |
| R-04 | Required observations `Observed` but `Confirmed empty`, where rule semantics do NOT establish emptiness as conclusive | `NOT_EVALUATED` (or `ERROR` if execution itself failed) | Absence of evidence is not evidence of security. |
| R-05 | Required capability `Unsupported` | `NOT_EVALUATED` normally; `NOT_APPLICABLE` only where the rule's applicability predicate genuinely excludes the subject/context | Unsupported required capability prevents authoritative `PASS`/`FAIL`; MUST preserve explicit non-evaluation semantics. MUST NOT become `PASS`. |
| R-06 | Required data `UnavailableAuthorization` | `NOT_EVALUATED` | Authorization limitation MUST NOT produce `PASS` (false-PASS test). MUST NOT automatically produce `FAIL` either. |
| R-07 | Required data `UnavailableLicensingService` | `NOT_EVALUATED` | Licensing/service limitation MUST NOT produce `PASS`. MUST NOT automatically produce `FAIL`. |
| R-08 | Required collection `Failed` | `NOT_EVALUATED`, or `ERROR` where the failure taxonomy and rule semantics so determine (mapping owned with `error-result-model.md`) | Collection failure MUST NOT produce `PASS`. |
| R-09 | Required data `Unknown` / indeterminate | `NOT_EVALUATED`, or `ERROR` where execution-trust is broken | Unknown capability MUST NOT produce `PASS`. |
| R-10 | Required data `NotRequested` / not collected | `NOT_EVALUATED` | Unrequested data cannot ground `PASS`/`FAIL`. |
| R-11 | Partial required data (§10) where the available subset is NOT proven sufficient | `NOT_EVALUATED` (or `ERROR` per failure mapping) | Partial evidence MUST NOT be interpreted as complete evidence. |
| R-12 | Partial required data where rule semantics explicitly prove the available subset sufficient for the deterministic outcome | The proven outcome (`PASS` or `FAIL`) permitted; otherwise R-11 | Narrow, documented exception with proof burden on the rule. |
| R-13 | Rule genuinely does not apply to subject/context per deterministic applicability predicate | `NOT_APPLICABLE` | `NOT_APPLICABLE` means the rule does not apply — never a cover for collection failure. |
| R-14 | Unexpected engine/data/runtime failure preventing trustworthy completion | `ERROR` | `ERROR` remains distinguishable from `NOT_EVALUATED`; neither converts to `PASS`/`FAIL` for reporting convenience. |

Additional constraints:

- `NOT_APPLICABLE` MUST NOT be used to hide unsupported
  functionality; `NOT_EVALUATED` MUST NOT be used for a rule that
  deterministically does not apply.
- `ERROR` MUST NOT be converted to `FAIL` to simplify reporting, nor
  to `PASS` to simplify output.
- Severity, confidence, risk scores, or finding-emission decisions
  are downstream metadata/policy, never assessment states and never
  a sixth state.

---

## 10. Partial collection

When some required data is available and some is not:

1. **Default posture is conservative.** Partial evidence MUST NOT be
   interpreted as complete evidence. The default outcome for a rule
   with unmet required prerequisites is `NOT_EVALUATED` (or `ERROR`
   per failure mapping), with reason identifying which required
   subset is missing and its capability state.
2. **Narrow sufficiency exception.** A rule MAY use partial data only
   if its documented rule semantics explicitly prove that the
   available subset is sufficient for the authoritative outcome —
   e.g., a `FAIL` predicate whose affirmative evidence is fully
   contained in the available subset with no dependence on the
   missing subset, or a `PASS` whose completeness argument provably
   does not depend on the missing subset. The proof burden is on the
   rule definition; the engine MUST NOT assume sufficiency.
3. **No silent narrowing.** Collectors MUST NOT hide partial
   collection behind a successful empty result; normalization MUST
   preserve per-category/per-subject gap metadata so rules and
   diagnostics observe the partial shape.
4. **Aggregation visibility.** Partial conditions MUST remain visible
   in assessment-level summaries (see §11): an assessment with
   partial required collection MUST NOT present as fully evaluated.

---

## 11. Capability aggregation

Conceptual behavior (algorithms and schemas deferred as TBDs):

1. **Subject-level capability.** Capability state is recorded at the
   granularity rules require: per data category and, where
   per-identity gaps are meaningful, per subject. A single identity's
   capability gap MUST NOT affect other identities' successfully
   collected data.
2. **Assessment-level capability summary.** The assessment result
   carries a summary of capability conditions sufficient to explain
   completeness: which categories/subjects were `Observed`, which
   were gapped, and the resulting `NOT_EVALUATED`/`ERROR` footprint.
   The summary MUST preserve — never summarize away — `ERROR` and
   `NOT_EVALUATED` conditions (`core-contracts.md` §3.9).
3. **Multiple collectors contributing to one capability.** Where more
   than one collection concern feeds a category, the category state
   reflects the combined observation: `Observed` only when the
   rule-required subset is completely observed; otherwise the
   applicable gap state with cause preserved per contributing
   concern.
4. **Conflicting/ambiguous observations.** Conflicts MUST NOT be
   silently overwritten or heuristically resolved. They are preserved
   as explicit conflict metadata (a §6 failed/error-condition form)
   with both observations and provenance retained, flowing into
   `NOT_EVALUATED` or `ERROR` per §9 unless a documented
   deterministic derivation provably resolves them.
5. **Partial provider responses.** Partially successful collection
   (some objects/relationships retrieved, others failed) is reported
   as partial with per-subset states, never as a single success
   boolean.

Exact aggregation algorithms, summary schemas, and serialization are
TBDs (§19).

---

## 12. Provider adapter responsibility

1. Provider/collector layers own all provider-specific capability
   facts: detecting support, authorization, licensing/service, and
   failure causes against documented source behavior, and reporting
   them as normalized capability observations (§5 + §7) before `Core`
   consumes them.
2. `Core` MUST NOT inspect provider SDK exceptions/types, HTTP
   semantics, raw payloads, or permission-name checks directly. `Core`
   sees only the resulting explicit normalized state plus provenance
   and sanitized cause.
3. Normalization validates source observations, translates them into
   normalized domain contracts, preserves provenance, distinguishes
   observed from derived, preserves unavailable/unknown/error states,
   and prevents provider SDK objects from entering rule contracts
   (`core-contracts.md` §10).
4. Collectors MUST NOT evaluate `PASS`/`FAIL`, construct findings, or
   manufacture observations; rules MUST NOT call providers or request
   privilege (`core-contracts.md` §§6, 10).

---

## 13. Licensing and authorization boundaries

1. Known authorization limitations are modeled as
   `UnavailableAuthorization` with tenant-scoped, sanitized reason;
   known licensing/service limitations are modeled as
   `UnavailableLicensingService` where the cause is safely knowable,
   else `Unknown`. Both flow into `NOT_EVALUATED` per §9 — never
   `PASS`, never automatic `FAIL`.
2. No hard-coded commercial tier/plan assumptions and no permission/
   scope-name constants appear in `Core` capability semantics. Exact
   permission rationale and any documentation-sensitive tier mappings
   are TBDs owned by `authentication-design.md` (permission
   rationale) and `collector-contracts.md` (endpoint/property
   mappings with documentation validation); capability granularity
   is owned here.
3. Optional capabilities MUST NOT silently broaden privilege
   requirements: enabling an optional category requires explicit
   configuration, and its unavailability remains explicit rather than
   forcing broader access (INV-08; INV-16).
4. Collection authorization vs assessed-identity permissions remain
   distinct concepts end to end (see §5).

---

## 14. Caching/freshness semantics

Conceptual requirements (no technology, TTL, persistence, or storage
selected here):

1. Capability information is valid only for the collection window
   that produced it. Reuse across runs, tenants, or time periods
   without re-observation is prohibited unless a documented
   freshness policy explicitly authorizes it (no such policy is
   authorized here; its design is a TBD, §19).
2. Stale capability information MUST NOT silently masquerade as
   current authoritative observation. Any cached or carried-forward
   capability fact MUST carry its observation-time context and
   freshness provenance, and rules MUST treat expired or
   indeterminate freshness as `Unknown`, not `Observed`.
3. Within a run, conflicting re-observations MUST remain diagnosable
   (collection-window semantics per `core-contracts.md` §5), never
   silently overwritten.
4. No cache technology, TTL constant, persistence mechanism, or
   storage location is chosen in this document.

---

## 15. Determinism

Given equivalent normalized capability/data inputs and equivalent
assessment context (same normalized observations, capability states,
rule version/configuration, and relevant deterministic execution
context), capability-dependent rule behavior MUST remain
deterministic: the same capability/data conditions produce the same
semantic assessment outcome (INV-02; VERD-006).

- Capability detection itself interacts with mutable external state
  and MAY yield different observations across runs; determinism
  applies downstream of the normalized capability/data boundary.
- Presentation metadata (timestamps, serialization order) MAY differ;
  evaluation semantics MUST NOT.
- Randomness, wall-clock reads inside evaluation logic,
  probabilistic branching, and model inference on the evaluation
  path are prohibited.

---

## 16. Security implications

Stated as structural support, not as implemented controls:

- **False-PASS resistance (INV-05; INV-07; INV-14).** Completeness
  gating (§§8–10) plus the §9 transition table make ignorance an
  explicit non-evaluation, never a silent secure verdict.
- **Least privilege compatibility (INV-08).** Per-category
  granularity (§§3, 11) lets collection request only minimum source
  access for enabled capabilities; optional categories never
  silently broaden requirements (§13).
- **No secret storage (INV-09).** Capability observations,
  provenance, and diagnostics carry no secret values, tokens,
  private-key material, passwords, or recovery codes — only
  non-secret metadata and references.
- **No token exposure.** Transient authentication material never
  enters capability state, domain facts, evidence, or output; token
  mechanics belong to `authentication-design.md`.
- **Tenant isolation (INV-10).** All capability facts are
  tenant-scoped; cross-tenant mixing is an integrity failure that
  fails closed and visibly, never a merged observation.
- **Untrusted provider data.** Provider-originated strings and
  capability signals are treated as untrusted data carried opaquely;
  encoding/escaping obligations belong to renderers per
  `output-renderer-contracts.md`.
- **Fail-transparent behavior (INV-14).** `Failed` and `Unknown`
  propagate explicitly into `NOT_EVALUATED`/`ERROR` with reason;
  no failure path yields `PASS`.

---

## 17. Testability implications

Boundary tests are required for each capability/data state and each
§9 transition (scenario catalog itself belongs to `testing-seams.md`;
no tests are created here):

- Each §5 state × each V1 category (§3.1): `Observed`,
  `NotRequested`, `UnavailableAuthorization`,
  `UnavailableLicensingService`, `Unsupported`, `Failed`, `Unknown`.
- Each §6 data condition, including confirmed-empty vs unavailable
  vs unsupported vs failed vs unknown discipline ("empty collection
  MUST NOT equal no-issue" fixtures).
- Each §9 transition row R-01–R-14, including the false-PASS battery
  (§20): permission-limited, unsupported, failed, unknown, partial,
  confirmed-empty, and `NOT_APPLICABLE`-vs-gap misuse cases.
- Partial-collection fixtures (§10): available-sufficient vs
  available-insufficient subsets.
- Aggregation fixtures (§11): per-subject isolation, multi-collector
  contribution, conflicting observations, partial responses.
- Freshness fixtures (§14): stale capability MUST NOT present as
  current observation.
- All fixtures synthetic, provider-free, secret-free, and
  network-free for core tests.

---

## 18. Explicit non-goals

1. No provider endpoint, permission/scope, property, SDK-call, or
   identifier-mapping selection.
2. No Agent ID mapping and no licensing-tier/commercial mapping.
3. No concrete security rule content and no scoring mathematics.
4. No severity/confidence/risk-score modeling (severity flows
   downstream without altering evaluation logic).
5. No sixth authoritative assessment state and no generic boolean
   "success" substitute for the five-state model.
6. No cache technology, TTL, persistence, retry/backoff constants,
   or numeric resource limits.
7. No SaaS/multi-tenant service, dashboard, remediation, monitoring,
   AI verdicts, or other NG-series capabilities.
8. No serialization schema, output formatting, CLI syntax, exit-code,
   or signing/hashing implementation.

---

## 19. Open TBDs and owner documents

Each TBD states what is unknown, why it cannot be resolved here, and
which later document owns its resolution. None is resolved by
speculation.

| # | TBD | Why unresolved here | Owned by |
| --- | --- | --- | --- |
| T-01 | Exact code-constant spelling/casing, schema, and serialization for capability identifiers (§4), capability states (§5), and the capability-observation envelope. | Literal token and wire-format choices need cross-document agreement (capability, collector envelope, findings serialization, output projection) and MUST NOT be fixed unilaterally here. | This document (conceptual vocabulary) with `collector-contracts.md` (envelope mechanics) and `findings-evidence-schema.md` (serialization); wire projection via `output-renderer-contracts.md` |
| T-02 | Per-category collection-scope granularity detail: which documented observable fields back each V1 category (§3.1) and exact endpoints/properties. | Requires validation against published documentation; inventing mappings here would violate INV-15. | `collector-contracts.md` (endpoints/properties/mappings), granularity coordinated with this document |
| T-03 | Agent-identity observable mapping: which documented agent-identity semantics back C-06. | Requires documentation validation; MUST NOT be invented. | `collector-contracts.md` with `domain-types.md` (classification vocabulary) |
| T-04 | Managed-identity classification observation mapping backing C-01 classification. | Requires documentation validation; MUST NOT be invented. | `collector-contracts.md` with `domain-types.md` |
| T-05 | Permission rationale per capability (which source access each category needs). | Rationale needs validated documentation review outside capability semantics. | `authentication-design.md` |
| T-06 | Documentation-sensitive licensing/service mapping detail behind `UnavailableLicensingService` vs `Unknown`. | Commercial-behavior claims MUST NOT be embedded in `Core` without validated documentation. | `collector-contracts.md` (detection detail) with `authentication-design.md`; semantics owned here |
| T-07 | Detailed rule-prerequisite declaration syntax and registration/compatibility mechanics (§8). | Rule-schema mechanics belong to the rule-engine surface. | `rule-engine-contracts.md` |
| T-08 | Capability aggregation algorithms and assessment-summary schema/serialization (§11). | Algorithm and schema design needs coordination with findings/output owners. | This document (behavior) with `findings-evidence-schema.md` (schema) and `output-renderer-contracts.md` (projection) |
| T-09 | Freshness/caching policy: whether any carry-forward is ever authorized, and with what bounds. | Needs implementation-phase storage and lifecycle design. | `collector-contracts.md` with `implementation-sequence.md` (staging) |
| T-10 | Failure-cause taxonomy detail and NOT_EVALUATED-vs-ERROR mapping for `Failed` capability (§5, §9 R-08/R-09). | Failure taxonomy spans all pipeline stages, not capability alone. | `error-result-model.md` (this document references only) |
| T-11 | Identifier formats (assessment, subject, provenance, diagnostic references carried in §7). | Formats need dedicated design plus structural-test enforcement. | `findings-evidence-schema.md` with `domain-types.md` |
| T-12 | Performance/memory bounds for capability propagation over large tenants. | Limits need measurement-backed implementation design. | `collector-contracts.md` with `identity-graph-contracts.md` and `implementation-sequence.md` |

---

## 20. Acceptance criteria

This capability model is accepted when:

1. **State coverage.** §5 distinguishes observed, not-requested, two
   known-unavailability causes, unsupported, failed, and unknown —
   each with precise semantics never confused with the five
   authoritative assessment states.
2. **Taxonomy coverage.** §3.1 covers identity enumeration,
   accountability/ownership, credential metadata, permission/access,
   identity relationships, and capability-gated agent-identity
   information, extensibly and without provider-specific mechanics.
3. **Identity stability.** §4 identifiers are provider-independent,
   semantically stable, prerequisite-suitable, and free of
   implementation names; no naming constant is finalized beyond
   justification.
4. **Availability fidelity.** §6 preserves `domain-types.md` §10
   semantics exactly; null/empty collections are never silently
   "no issue."
5. **Prerequisite soundness.** §8 lets a rule determine evaluation
   safety/completeness, routing unmet prerequisites to the correct
   existing non-binary state without finalizing the rule schema.
6. **Transition normativity.** The §9 table constrains every
   capability/data condition to permitted assessment outcomes with
   the false-PASS battery explicit: permission-limited, unsupported,
   failed, unknown, and unproven-partial conditions MUST NOT yield
   `PASS`; confirmed-empty is `PASS` only per proven rule semantics;
   `NOT_APPLICABLE` never covers collection gaps; `ERROR` stays
   distinct from `NOT_EVALUATED`.
7. **Partial-collection conservatism.** §10 defaults to
   non-evaluation with a narrow, rule-proven sufficiency exception.
8. **Aggregation integrity.** §11 isolates subject-level gaps,
   preserves `ERROR`/`NOT_EVALUATED` in summaries, and keeps
   conflicts/partial responses explicit without premature
   algorithms.
9. **Provider isolation.** §12 keeps provider detection in collectors
   and provider types out of `Core`.
10. **Licensing/authorization restraint.** §13 models limitations
    without commercial or permission-name constants in `Core`.
11. **Freshness safety.** §14 prevents stale capability from posing
    as current observation without selecting technology.
12. **Determinism.** §15 reproduces semantic outcomes from equivalent
    normalized inputs plus assessment context.
13. **TBD explicitness.** Every implementation-sensitive unknown is
    listed in §19 with its owning document; no endpoint,
    permission, property, mapping, package, or numeric limit is
    invented.
14. **Non-goal containment.** Nothing in §18 appears as an assumed
    capability.
15. **Cross-document agreement.** The consistency contract with
    `core-contracts.md` (§§4, 8–9) and `domain-types.md` (§10)
    holds: identical five states (referenced, not redefined),
    capability-aware deterministic evaluation, provider isolation,
    provenance preservation, secret exclusion, renderer/AI
    non-authority — with detailed schemas deferred to their owners
    via explicit cross-references, and full agreement with
    `error-result-model.md` failure mapping.

---

*(End of file)*
