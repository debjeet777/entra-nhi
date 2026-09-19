# Collector Contracts

> **Phase:** 0.3.6
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed conceptual contracts. This document is design output
  only. It authorizes no implementation.
- **Scope:** Defines the provider-implementation boundary through which
  EntraNHI obtains normalized identity/security observations without
  allowing provider SDK/API concepts to contaminate Application/Core
  contracts. Derived from the approved collection architecture
  (`docs/02-architecture/collection-architecture.md`), domain model
  (`docs/02-architecture/domain-model.md`), invariants (INV-01–INV-16,
  INV-G1–INV-G8), trust boundaries, threat model, and testing
  architecture, as structured by `solution-structure.md` §§4–8 and
  governed by `README.md` §§4–6.
- **What this document is:** The owner of collector interfaces, the
  source-observation envelope contract, pagination/retry/cancellation
  surfaces, and the collector-to-normalization handoff
  (`README.md` §5–§6). First normative definition of the collector
  envelope and handoff lives here; all other documents use it
  referentially.
- **What this document is not:** It is not a C# implementation, an
  endpoint catalog, a permission/scope list, a property mapping, an
  Agent ID mapping, an SDK-method selection, a retry-constant set, or a
  rule catalog. Detailed consumers live elsewhere: normalized domain
  shapes in `domain-types.md`, capability states in `capability-model.md`,
  graph operations in `identity-graph-contracts.md`, rule pipeline and
  `RuleEvaluation` in `rule-engine-contracts.md`, findings/evidence/
  provenance schemas in `findings-evidence-schema.md`, error/result
  shapes in `error-result-model.md`, output projections in
  `output-renderer-contracts.md`, auth boundaries in
  `authentication-design.md`, and the shared core surface in
  `core-contracts.md` (which references — never duplicates — the
  contracts defined here).
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed conceptual contracts** owned by this
  document, and (c) **unresolved TBDs** naming the owning later document
  (§23). Nothing herein claims a control is implemented, verified, or
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
  created here. Capability/data/operational terms used below
  (`Observed`, `NotRequested`, `Unsupported`, `UnavailableAuthorization`,
  `UnavailableLicensingService`, `Failed`, `Unknown`, `Partial`,
  `Cancelled`, `AuthenticationFailed`, `AuthorizationLimited` as concepts)
  are explicitly typed in their correct layer per `capability-model.md`
  and `error-result-model.md` — never as assessment states.

---

## 2. Collector responsibilities

Conceptual flow (normative direction):

```text
Host / CLI
  -> Application orchestration
    -> validated authentication/provider boundary
      -> provider collector implementation
        -> normalization
          -> provider-independent collected domain data
            -> Core
```

### 2.1 Collectors conceptually own

- Provider communication for their assigned source concern (INV-03).
- Provider-specific request execution within documented observable
  behavior (INV-15).
- Pagination handling preserving completeness (§9).
- Provider response interpretation against documented expectations.
- Provider failure classification input (normalized into the
  operational failure taxonomy owned by `error-result-model.md`).
- Normalization handoff: delivery of source observations plus
  capability/provenance/failure context toward normalization (§6).
- Provenance capture sufficient for traceability without secrets (§12).
- Capability observation input: reporting what could be observed, per
  `capability-model.md` (§7).
- Partial-result awareness: never presenting incomplete collection as
  complete (§11).
- Cancellation propagation: observing, recording, and propagating
  cancellation intent (§10 and `error-result-model.md` §7).
- Bounded/resource-conscious collection: pages, records, payloads,
  expansion, concurrency, retries, and memory accumulation are bounded
  (§15).

### 2.2 Collectors do NOT own

- Rule verdicts (`PASS`/`FAIL` determination).
- `PASS`/`FAIL` decisions or completeness judgments for rules.
- Finding severity (severity flows downstream without altering
  evaluation logic).
- Renderer behavior or output projection.
- AI interpretation (no model output on the collection path, INV-13).
- Final assessment aggregation (owned by the rule-engine aggregation
  stage per `core-contracts.md` §3.9).
- Remediation or tenant mutation (INV-01; read-only V1).
- Dynamic privilege requests or silent scope expansion.
- Manufactured observations or undocumented-semantics inference.

---

## 3. Collector taxonomy

Conceptual collector families required by approved V1 scope, carried
forward from collection architecture §3. Families are **logical
responsibilities, not mandatory implementation classes**. A single
provider request MAY serve multiple families; a single family MAY
require multiple requests. No one-to-one family-to-request mapping is
assumed.

| # | Conceptual family | Observes (conceptually) |
| --- | --- | --- |
| F-C01 | Normalized identity collection | Application registrations and service principals as normalized identity records (`domain-types.md` §3). |
| F-C02 | Accountability/ownership collection | Owner, sponsor, and manager relationships where documented; no other accountability category is in V1 scope. |
| F-C03 | Credential-metadata collection | Non-secret credential lifecycle metadata only (type/category, non-secret identifiers where documented non-secret, validity timestamps where available); never secret values. |
| F-C04 | Permission/access collection | Permission/access-assignment relationships preserving declaration vs grant/assignment vs resource vs principal distinctions (`domain-types.md` §8). |
| F-C05 | Supported identity-relationship collection | Identity-link relationships (e.g., application registration to service principal linkage) and other documented identity-to-identity relationships. |
| F-C06 | Supported agent-identity collection | Agent-identity observations only where authoritative documentation/API capability exists; capability-gated so unsupported/unavailable agent semantics remain explicit and never block unrelated assessment. |

Rules for this section:

1. Do NOT prescribe one class/interface per family unless a later
   seam catalog (`dependency-boundaries.md`) justifies it on
   seam-clarity grounds.
2. Do NOT invent provider mappings: no endpoint, property, permission/
   scope, SDK-call, identifier, Agent ID, or licensing mapping is
   stated for any family. All such mappings are TBDs (§23).
3. Managed-identity classification support is a normalization concern
   over collected service-principal observations, not a separate
   provider object claim (`domain-types.md` §3).
4. Optional enrichment from a second documented source family (if future
   approved rules require it) remains isolated behind its own
   collector/boundary concern, explicitly enabled, separately
   provenanced, with its own capability state — never an implicit
   dependency.

---

## 4. Collector input contract

Conceptual inputs to a collector invocation (by reference, not a member
list):

1. **Tenant-scoped execution context.** The single-tenant assessment
   reference binding this invocation (INV-10). No ambient or implied
   tenancy. Mismatch is an integrity failure (§13,
   `error-result-model.md` §13).
2. **Validated provider/authentication boundary reference.** An opaque,
   non-credential handle to the authorized access established by
   `authentication-design.md`. The handle authorizes requests; it MUST
   NOT expose raw tokens, secrets, authorization headers,
   credential-bearing URLs, provider authentication objects, or
   SDK authentication objects to Application/Core consumers (§19).
3. **Requested collection/capability set.** The normalized capability
   identifiers (`capability-model.md` §4; C-01–C-06 concepts) in scope
   for this run. Unrequested categories MUST surface as `NotRequested`,
   never as empty observations.
4. **Operation/correlation context.** Assessment/run/operation reference
   enabling end-to-end tracing and diagnostic correlation, plus
   collection-window/observation-time context (window semantics, not
   snapshot claims unless the source provides them).
5. **Cancellation intent.** An explicit cancellation signal the collector
   MUST observe where implementation permits; mechanism is a TBD (§23).
6. **Deterministic configuration relevant to collection.** Explicit,
   validated, non-secret configuration values affecting collection scope
   (e.g., which optional capabilities are enabled). `Core` never reads
   environment or files directly; configuration surfaces belong to
   `configuration-design.md`.

CRITICAL constraints:

- The collector contract MUST NOT expose raw tokens/secrets to
  Application/Core consumers — by parameter, return, property, event,
  log, diagnostic, or ambient state.
- Do NOT finalize framework-specific types (no token type, task type,
  HTTP-client type, or SDK type is chosen here).

```csharp
// Illustrative only. Conceptual and non-production. Not a member list.
record CollectorRequest {
  TenantAssessmentContext Tenant; AccessBoundaryRef Access;
  IReadOnlyList<CapabilityRef> RequestedCapabilities;
  OperationContext Operation; CancellationIntent Cancellation;
  // + validated non-secret collection-relevant configuration.
}
```

---

## 5. Collector result contract

A collector invocation MUST return a conceptual result rich enough to
preserve all of the following. A bare boolean, a null/non-null signal,
or any single "success" substitute is prohibited
(`error-result-model.md` §3, §14).

1. **Normalized observations.** Source observations destined for the
   normalization handoff (§6), each carrying provenance.
2. **Capability observations.** Per-category (and per-subject where
   meaningful) capability states per `capability-model.md` §5
   (`Observed`, `NotRequested`, `UnavailableAuthorization`,
   `UnavailableLicensingService`, `Unsupported`, `Failed`, `Unknown`
   as concepts).
3. **Provenance.** Source system, object reference, collection
   operation/context, assessment context, observation-time context,
   and derivation lineage hooks (§12).
4. **Completeness information.** What was completely observed vs gapped,
   per §7 — including confirmed-empty vs unavailable vs unsupported vs
   failed vs unknown discipline (§8).
5. **Partial-result information.** Which subsets succeeded and which did
   not, with per-subset states (§11); never a single success boolean.
6. **Operational failures.** Typed conceptual failures per
   `error-result-model.md` §5 with sanitized diagnostics and retryability
   hints where safely known — never raw provider exceptions, never
   secret-bearing detail.
7. **Cancellation information.** Whether and at what scope cancellation
   occurred, distinguishable from failure, timeout, and verdicts
   (`error-result-model.md` §7).
8. **Safe diagnostics.** Machine-readable codes, human-explainable
   reasons, tenant-scoped correlation — secret-free, provider-type-free
   (`error-result-model.md` §§8–9).

Do NOT create alternative assessment states: the result contract MUST
NOT assign `PASS`/`FAIL`/`NOT_EVALUATED`/`NOT_APPLICABLE`/`ERROR` — that
mapping belongs downstream (rules/engine per `capability-model.md` §9
and `error-result-model.md` §4).

```csharp
// Illustrative only. Conceptual and non-production. Not a member list.
record CollectorResult {
  IReadOnlyList<SourceObservation> Observations;
  IReadOnlyList<CapabilityObservation> Capabilities;
  CompletenessDescriptor Completeness; PartialDescriptor Partiality;
  IReadOnlyList<OperationalFailure> Failures; CancellationRecord Cancellation;
  IReadOnlyList<SanitizedDiagnostic> Diagnostics;
}
```

---

## 6. Normalization boundary

Normative transformation direction:

```text
Provider-native data -> validation -> normalization ->
  provider-independent domain observations
```

1. **Provider-native responses MUST be normalized before crossing the
   provider boundary toward Core** (INV-03; INV-04).
2. **Validation first.** Provider responses are external/untrusted input
   (§14). Shape, identifier, bound, and value validation precede
   translation; validation failures become explicit normalization
   failures (`error-result-model.md` F-08), never silent drops that
   could fake completeness (§8, CASE D).
3. **Normalization translates** validated source observations into the
   normalized domain vocabulary owned by `domain-types.md`
   (`IdentityRecord`, `IdentityKind`, credential metadata,
   permission/access, accountability, tenant context, relationship
   vocabulary, absence semantics, provenance vocabulary) — without
   introducing undocumented properties and without provider SDK objects
   or resource references surviving the boundary.
4. **Rules MUST NOT inspect provider SDK/native objects.** Rules consume
   normalized contracts, capability states, provenance references, and
   deterministic configuration only (`core-contracts.md` §6).
5. **Observed vs normalized vs derived stays distinguishable**
   (`domain-types.md` §11): collectors produce observations;
   normalization produces canonical representations; deterministic logic
   produces derived values labeled as derived.
6. **Provenance and capability state survive normalization** intact:
   normalization MUST NOT strip gap metadata, merge conflicting
   observations silently, or degrade `Failed`/`Unknown` into
   `NotRequested`/empty.
7. **No verdicts at the boundary.** Normalization produces facts, never
   `PASS`/`FAIL`, never findings.

---

## 7. Completeness semantics

Collectors MUST distinguish the following per category/subject, aligned
exactly with `capability-model.md` §§5–6 and `domain-types.md` §10. No
additional authoritative assessment state is invented; these are
capability/data concepts constraining — never equaling — verdicts.

- Successfully observed data (`Observed` channel + present content).
- Confirmed empty where provider semantics justify that conclusion
  (explicit empty indication under complete observation, §8).
- Not requested / not collected (`NotRequested` scope fact).
- Unsupported (`Unsupported`).
- Authorization-limited (`UnavailableAuthorization`).
- Licensing/service-limited where knowable
  (`UnavailableLicensingService`, else `Unknown` — no commercial-behavior
  claims made here).
- Failed (attempted but an operational failure prevented retrieval).
- Unknown / indeterminate (cause or content cannot safely be
  established; the safe default when in doubt).
- Partial (a subset of expected observations failed while others
  succeeded; reported per-subset, §11).

Do NOT invent additional authoritative assessment states. The five
states (`core-contracts.md` §4) are the only verdicts; everything above
maps through `capability-model.md` §9 and `error-result-model.md` §4.

---

## 8. Empty-result handling

Normative rule (critical false-PASS control):

- **Provider/API success + zero returned objects is NOT universally
  equivalent to confirmed-empty.**
- **Confirmed-empty requires sufficient provider/collection semantics
  to establish completeness**: the observation channel worked
  (`Observed`), the request scope covered the category/subject, the
  source explicitly indicates no value or an empty relationship where
  that distinction is documented and reliable, and the consuming rule's
  explicit completeness semantics authorize treating that emptiness as
  conclusive (`capability-model.md` R-02/R-03).
- Otherwise, zero objects is `Unavailable`, `Failed`, `Unknown`,
  `NotRequested`, or `Unsupported` with reason preserved — and affected
  rules yield `NOT_EVALUATED` (or `ERROR` per failure mapping), never
  `PASS` (CASE C).
- Collectors MUST NOT normalize "no rows" into "secure condition."
  Whether verified-complete absence yields `PASS` or `NOT_EVALUATED` is
  determined per rule by its explicit completeness semantics — never by
  a universal absence rule (`core-contracts.md` §4, rule 2).

---

## 9. Pagination

Conceptual requirements (no provider pagination mechanism, page size,
or SDK surface is selected here — TBD §23):

1. **Pagination MUST preserve completeness.** The collector result MUST
   account for every page in the sequence; completeness claims require
   the full sequence (or a documented source-completeness signal
   justifying an equivalent claim — no such signal is assumed here).
2. **Failure after earlier pages MUST preserve partial state** (CASE B):
   successfully retrieved pages keep their observations/provenance;
   the failure is recorded with scope (which page/subset failed, which
   failure category, sanitized cause); the result is partial, never
   "complete."
3. **Repeated/invalid continuation behavior MUST fail visibly.**
   Continuation signals that repeat, regress, contradict earlier pages,
   or violate documented sequencing expectations MUST NOT be silently
   retried forever or silently truncated; they become explicit
   operational failures (malformed-response / internal-cause family)
   with bounded handling (§15).
4. **Pagination loops MUST be bounded.** Maximum pages/records and
   anti-loop guards are required as design obligations; exact numeric
   thresholds are TBDs (§15, §23) — no numeric limit is chosen here.
5. **Duplicates MUST be handled deterministically.** Duplicate source
   observations for the same source object MUST normalize to a single
   canonical record deterministically; conflicting observations MUST
   remain detectable as explicit conflict metadata (INV-G5), never
   silently overwritten or heuristically resolved.
6. **No page may silently disappear.** Dropped, skipped, or
   unprocessable pages are recorded explicitly; silent truncation into
   a "success" result is prohibited.

---

## 10. Retry/throttling boundary

1. Collectors own recognizing normalized retry/throttle conditions
   (rate/throttle signals, transient vs deterministic-denial vs
   malformed-response distinctions as *hints observed from documented
   behavior*) and preserving them through the error model as
   `error-result-model.md` F-04 (throttling/rate limitation) with
   retryability hints and exhausted-bound records (`error-result-model.md`
   §6).
2. Classification is separated from policy: hints arrive normalized with
   provenance; `Core` treats them as data and MUST NOT invent
   retryability from provider details.
3. Do NOT choose retry counts, delay durations, algorithms, SDK policies,
   resilience libraries, timeout values, concurrency bounds, or
   throttling thresholds here. Those remain implementation TBDs (§23).
4. Retry exhaustion is itself explicit: after bounded retries the failure
   records that the bound was exhausted, preserves the original cause
   chain (sanitized), and flows into `NOT_EVALUATED` or `ERROR` per
   `capability-model.md` §9 — never silent `PASS`.
5. Deterministic denials (e.g., authorization failures) and malformed
   responses MUST NOT be retried as if transient; retry MUST NOT convert
   a denial into pressure on the provider or into hidden latency that
   masks incompleteness.

---

## 11. Partial failure

Usable data MAY be preserved where correctness allows, but
incompleteness MUST remain explicit (`error-result-model.md` §12).
Cover at minimum:

1. **One page fails after earlier pages succeed** → §9 rule 2: keep
   earlier observations, record the failed scope, report partial.
2. **One collector fails while another succeeds** → per-category
   isolation (`capability-model.md` §11): successful categories keep
   `Observed`; the failed category carries its gap/failure state;
   affected rules yield `NOT_EVALUATED`/`ERROR`; unaffected rules
   evaluate normally.
3. **One subject cannot be normalized** → per-subject isolation: the
   failed subject's downstream evaluations record the gap with reason
   (normalization failure F-08); other subjects are unaffected;
   aggregation preserves the per-subject failure visibly.
4. **One capability cannot be observed** → the capability gap is
   recorded per §7 with sanitized cause; dependent rules follow
   `capability-model.md` §9; independent rules proceed.
5. **Malformed provider records** → the record-level failure is
   preserved (F-07/F-08 family); the record MUST NOT be silently
   discarded if doing so could create false completeness (CASE D).
   Discard-without-trace is permitted only where the owning schema
   documents prove the discard cannot affect completeness — no such
   blanket proof is made here.
6. **Cancellation during collection** → cancellation (F-13) is recorded
   with scope and affected subjects; downstream stages MUST NOT
   evaluate or present cancelled scope as complete (CASE F).
7. The assessment MUST NOT automatically abort in full on every scoped
   failure unless correctness cannot be preserved; equally, usable
   partial results MUST NOT hide incompleteness.

---

## 12. Provenance capture

Collectors MUST capture sufficient source/provenance context for
normalized facts without exposing credentials/secrets, sufficient as
inputs to the schemas owned by `findings-evidence-schema.md`:

- Source system (source family as a normalized value, never SDK types
  or request paths).
- Source object reference (documented, source-exposed identifiers only).
- Collection operation/context (which collection concern produced the
  observation).
- Assessment/tenant context reference (single-tenant scope).
- Observation-time / collection-window context where appropriate.
- Transformation/derivation lineage hooks for normalized facts.

Restrictions: provenance MUST NOT carry tokens, secrets, private-key
material, passwords, recovery codes, authorization headers,
credential-bearing URLs, secret-bearing configuration, or unnecessary
raw payloads. Unknown/unavailable provenance is explicit, never
invented. Do NOT finalize findings/evidence schema here.

---

## 13. Tenant isolation

1. Every collector invocation and every collector result MUST remain
   tenant scoped (INV-10; INV-G1).
2. Cross-tenant mixing is prohibited: no silent cross-tenant
   normalization, graph edges, evidence correlation, or artifact mixing.
3. Tenant-context mismatch or contamination is an integrity failure
   (`error-result-model.md` §13): fail closed and visibly — never silent
   continuation, never conversion into `PASS`/`FAIL`, never relabeling
   as an ordinary data gap.
4. Equality/dedup across tenant contexts is prohibited
   (`domain-types.md` §14): records from different tenant contexts MUST
   NEVER compare equal or merge, even if source identifiers coincide
   textually.
5. Do NOT assume SaaS hosting. V1 executes one tenant per run under an
   operator-controlled trusted local runtime. Tenant isolation (never
   mixing data across tenant contexts) is distinct from SaaS
   multi-tenancy (simultaneous multi-tenant service operation, a V1
   non-goal).

---

## 14. Provider-data validation

Provider responses are external/untrusted input. Conceptual validation
expectations (exact schemas deferred — do NOT invent provider property
schemas here):

1. **Required shape validation.** Responses MUST be checked against
   documented expectations before normalization; unexpected shapes
   become explicit malformed-response failures (F-07), never silent
   interpretation.
2. **Identifier validation.** Source identifiers MUST use only documented,
   source-exposed values; undocumented identifiers MUST NOT be
   concatenated, guessed, or joined on; display names MUST NOT serve as
   keys or join criteria.
3. **Bounds/resource limits.** Responses exceeding bounded pages, records,
   payloads, or expansion depths trigger explicit resource-exhaustion
   handling (§15), never unbounded accumulation.
4. **Malformed/unexpected values.** Values outside documented expectations
   (type, range, cardinality) are preserved as failures with sanitized
   diagnostics — never coerced, defaulted, or silently dropped where
   completeness is at stake (CASE D).
5. **Safe string handling.** Provider-originated strings are untrusted
   data carried opaquely; the collector MUST NOT interpret them as code,
   markup, paths, or commands. Encoding/escaping obligations belong to
   renderers per `output-renderer-contracts.md`; the canonical model
   pre-encodes for no format.
6. **Deterministic normalization.** Equivalent provider observations +
   equivalent configuration/context MUST normalize deterministically
   (§16); validation MUST NOT introduce ordering, timing, or
   randomness dependence.

---

## 15. Resource exhaustion controls

Design requirements for bounded processing (obligations here; exact
thresholds are TBDs — do NOT choose numeric limits here unless already
approved elsewhere, of which there are none):

- Pages (max pages per collection sequence + anti-loop guards).
- Records (max normalized records per category/run).
- Payloads (max response/object payload sizes).
- Relationship expansion (max traversal/expansion depth and breadth).
- Concurrency (max parallel collection operations).
- Retries (max retry budgets per operation/category).
- Memory accumulation (max in-memory observation accumulation before
  explicit exhaustion handling).

Rules:

1. Exhaustion surfaces explicitly as an operational condition flowing
   into `NOT_EVALUATED` or `ERROR` per failure mapping — never silent
   `PASS`, never silent truncation presented as complete.
2. Bounds MUST be explicit, deterministic, and diagnosable (which bound,
   which scope, which consequence).
3. Assign exact thresholds to implementation/configuration TBDs (§23).

---

## 16. Collector determinism

1. Equivalent provider observations + equivalent configuration/context
   MUST normalize deterministically: same inputs reproduce the same
   normalized facts, capability states, provenance references, and
   diagnostic shapes (INV-02; VERD-006 downstream).
2. Provider ordering MUST NOT accidentally change authoritative
   semantics: collectors MUST impose a deterministic ordering (or
   order-independent normalization) before handoff so that
   source-returned order variation cannot alter dedup, conflict
   detection, graph construction, or evaluation outcomes.
3. No randomness, wall-clock branching inside normalization semantics,
   probabilistic handling, or model inference on the collection/
   normalization path. Observation timestamps are carried as data;
   they MUST NOT alter normalization semantics non-deterministically.
4. Determinism applies downstream of the provider boundary: the provider
   itself is mutable external state and MAY return different content
   across runs; given the same returned content, everything downstream
   is deterministic.

---

## 17. Collector composition

How multiple collectors conceptually contribute normalized
observations/capabilities without silently overwriting contradictory
facts:

1. Each collector contributes its family's observations + capability
   states with provenance; contributions are additive by default.
2. Where more than one collection concern feeds one capability category,
   the category state reflects the combined observation
   (`capability-model.md` §11): `Observed` only when the rule-required
   subset is completely observed; otherwise the applicable gap state
   with cause preserved per contributing concern.
3. Conflict/ambiguity MUST remain explicit: conflicting observations for
   the same source object are preserved as explicit conflict metadata
   with both observations and provenance retained (a failed/error-
   condition data form per `domain-types.md` §10), flowing into
   `NOT_EVALUATED` or `ERROR` per `capability-model.md` §9 unless a
   documented deterministic derivation provably resolves them.
4. Silently overwriting, last-writer-wins merging, or heuristic
   resolution is prohibited (INV-G5).
5. One family's gap MUST NOT invalidate unrelated families'
   successfully collected data (per-category/per-subject granularity,
   `capability-model.md` §11).

---

## 18. Error translation

1. Provider-specific failures MUST be translated into the conceptual
   operational failure model (`error-result-model.md` §5, F-01–F-14)
   before reaching Core — by concept, never by provider exception class
   name, never by SDK type name.
2. Translation occurs behind the adapter boundary: raw provider
   exceptions/signals MAY exist transiently for adapter-local handling;
   across the normalization boundary only the normalized category +
   sanitized diagnostics travel.
3. No provider exception types in Core — by type, by name, or by
   behavior (`core-contracts.md` §12).
4. Where the true cause cannot safely be established, the failure is
   recorded with indeterminate cause rather than a guessed category.
5. Chains are sanitized at every link: secrets stripped, provider types
   normalized away, raw payloads excluded by default
   (`error-result-model.md` §10). Raw-response diagnostic retention
   policy, if any, is a TBD (§23) and MUST exclude secrets.
6. No translated failure maps to tenant `FAIL` or to `PASS`; scoped gaps
   map toward `NOT_EVALUATED` and trust-breaking failures toward `ERROR`
   per `error-result-model.md` §4 and `capability-model.md` §9.

---

## 19. Authentication boundary interaction

1. Collectors MAY use the validated authentication/provider boundary
   (owned by `authentication-design.md`) to perform authorized requests:
   they consume an opaque, non-credential access reference supplied with
   the collector input (§4).
2. Do NOT specify raw token passing: no token value, authorization
   header, credential-bearing URL, or SDK authentication object crosses
   the collector contract in either direction.
3. Collectors do NOT own credentials: they MUST NOT acquire, cache,
   persist, log, serialize, or expand authentication material; they MUST
   NOT request elevated privilege dynamically; they MUST NOT silently
   broaden the authorized scope.
4. Authentication mechanics (establishment, lifetime, storage, refresh
   behavior) belong to `authentication-design.md`. This document requires
   only the consumption posture above.
5. If the access reference is absent, expired, or tenant-incompatible,
   the collector MUST NOT proceed as if authorized: it records the
   applicable authentication/authorization failure explicitly (CASE A
   posture: authorization limitation MUST NOT become empty + `PASS`).

---

## 20. Test seams

Seams needed (detailed testing design remains `testing-seams.md`; no
tests are created here; all fixtures synthetic, provider-free,
secret-free, network-free for core-adjacent tests):

- Synthetic provider responses covering every family (§3) and every
  capability/data state (§7).
- Pagination: multi-page success, mid-sequence failure (CASE B),
  repeated/invalid continuation, bounded-loop enforcement, duplicate
  handling, no-silent-drop verification.
- Throttling: normalized throttle recognition, exhaustion recording,
  no-transient-conflation for denials/malformed data.
- Malformed data: shape/identifier/value violations (CASE D) with
  explicit failure preservation.
- Partial results: per-page, per-collector, per-subject, per-capability
  partial fixtures with explicit incompleteness.
- Cancellation: mid-collection cancellation (CASE F) with incomplete
  scope preserved and distinct from failure/timeout/verdicts.
- Normalization: validation → translation → provenance/capability
  preservation, determinism, ordering-independence checks.
- Tenant isolation: cross-context mixing attempts MUST surface as
  integrity failures.
- Deterministic ordering: order-permuted provider content yields
  identical normalized outcomes.
- Secret-exclusion: token/secret-shaped synthetic material MUST have no
  path into observations, diagnostics, logs, or outputs.

---

## 21. Security implications

Stated as structural support, not as implemented controls:

- **Read-only posture (INV-01).** No collector defines a mutating
  operation; collection is constrained to documented read-only
  observable behavior.
- **Provider isolation / normalized boundary (INV-03; INV-04).**
  Provider types never cross into rule/graph/finding/output contracts;
  structural-test intent makes bypass visible.
- **False-PASS resistance (INV-05; INV-07; INV-14).** Completeness
  semantics (§§7–8), pagination integrity (§9), partial-failure
  explicitness (§11), and composition conflict preservation (§17) leave
  no path from ignorance to `PASS`.
- **Secret exclusion (INV-09).** §§4–6, 12, 18–19 leave tokens/secrets
  no field or path into observations, domain facts, diagnostics, logs,
  or outputs.
- **Least privilege alignment (INV-08).** Per-category capability
  scoping (§§3–4, 7) lets collection request only minimum source access
  for enabled capabilities; optional categories never silently broaden
  requirements.
- **Tenant isolation (INV-10).** §13 makes cross-tenant mixing an
  integrity failure, never a merged observation.
- **Failure transparency (INV-14).** §§5, 9–11, 15, 18 keep every gap
  and failure explicit with sanitized cause.
- **AI non-authority (INV-13).** No contract path admits model output
  onto collection, normalization, capability, or failure
  classification.
- **Undocumented-behavior resistance (INV-15).** No endpoint,
  permission, property, mapping, or licensing claim is made here; every
  such detail is a named TBD (§23).

---

## 22. Explicit non-goals

1. No endpoint, permission/scope, property, SDK-call, or identifier
   mapping selection.
2. No Agent ID mapping and no licensing-tier/commercial mapping.
3. No concrete security rule content and no scoring mathematics.
4. No severity/confidence/risk-score modeling (severity flows downstream
   without altering evaluation logic).
5. No sixth authoritative assessment state and no generic boolean
   "success" substitute for the five-state model.
6. No retry counts, delays, algorithms, SDK policies, resilience
   libraries, timeout values, concurrency bounds, throttling thresholds,
   or numeric resource limits.
7. No framework-specific pagination/cancellation implementation and no
   threading/task-library selection.
8. No serialization schema, output formatting, CLI syntax, exit-code, or
   signing/hashing implementation.
9. No authentication-flow, credential-mechanism, token-cache, or storage
   selection (owned by `authentication-design.md`).
10. No SaaS/multi-tenant service, dashboard, remediation, monitoring, AI
    verdicts, or other NG-series capabilities.

---

## 23. Open TBDs and owner documents

Each TBD states what is unknown, why it cannot be resolved here, and
which later document owns its resolution. None is resolved by
speculation.

| # | TBD | Why unresolved here | Owned by |
| --- | --- | --- | --- |
| T-01 | Concrete collector interface/record shapes and member lists for §§4–5, and the source-observation envelope schema/serialization. | Member-level design needs coordination with capability, domain, findings, and output owners; choosing members here would preempt them. | This document (semantics) coordinated with `core-contracts.md`; physical shapes per consumer in their owning documents; seam placement in `dependency-boundaries.md` |
| T-02 | Exact provider endpoints, properties, and per-family observable mappings backing §3 (including managed-identity classification evidence, agent-identity mapping, credential/accountability/permission mappings). All require validation against published documentation and MUST NOT be invented. | Undocumented provider semantics cannot be resolved by design speculation (INV-15; CAP-005). | This document (mappings), granularity coordinated with `capability-model.md`, vocabulary with `domain-types.md` |
| T-03 | Permission rationale per collection capability (which source access each category needs). | Rationale needs validated documentation review outside collector mechanics. | `authentication-design.md` |
| T-04 | Documentation-sensitive licensing/service detection detail behind `UnavailableLicensingService` vs `Unknown`. | Commercial-behavior claims MUST NOT be embedded without validated documentation. | This document (detection detail) with `authentication-design.md`; semantics owned by `capability-model.md` |
| T-05 | Pagination mechanism specifics, page-size/bounding constants, and communication-approach choice. | Operational constants and mechanism choice need implementation-phase measurement and documentation validation. | This document with `dependency-boundaries.md` |
| T-06 | Retry/backoff/timeout/concurrency constants and throttling-threshold policy. | Operational constants need implementation-phase measurement and documentation validation. | This document with `dependency-boundaries.md` |
| T-07 | Cancellation mechanism choice per pipeline side. | Mechanism choice needs collector/rule/graph coordination. | This document (collection side), `rule-engine-contracts.md` / `identity-graph-contracts.md` (evaluation/graph side) |
| T-08 | Resource/bounding numeric limits for §15 (pages, records, payloads, expansion, concurrency, retries, memory). | Limits need measurement-backed implementation design, not speculation. | This document with `identity-graph-contracts.md` and `implementation-sequence.md` (staging) |
| T-09 | Raw-response diagnostic retention policy (whether any raw content is ever retained for diagnostics, with what redaction/bounds). | Retention needs security review; default is no raw retention. | This document with `findings-evidence-schema.md` (redaction) |
| T-10 | Exact provider-signal to failure-category mappings (which documented provider behaviors normalize to which `error-result-model.md` category). | Requires validation against published documentation; inventing mappings would violate INV-15. | This document (mappings), taxonomy owned by `error-result-model.md` |
| T-11 | Per-category mapping detail: collection-side failure to `NOT_EVALUATED` vs `ERROR` (with cancellation-scope mapping). | Mapping needs rule-engine and graph coordination; a premature table risks inventing semantics. | This document (collection side) with `rule-engine-contracts.md`, constrained by `capability-model.md` §9 and `error-result-model.md` §4 |
| T-12 | Identifier formats (assessment, subject, provenance, diagnostic, correlation references carried in §§4–5, 12). | Formats need dedicated design plus structural-test enforcement. | `findings-evidence-schema.md` with `domain-types.md` |
| T-13 | Freshness/caching policy: whether any observation carry-forward is ever authorized, and with what bounds. | Needs implementation-phase storage and lifecycle design. | This document with `implementation-sequence.md` (staging) |
| T-14 | Performance/memory bounds for collection over large tenants. | Limits need measurement-backed implementation design. | This document with `identity-graph-contracts.md` and `implementation-sequence.md` |
| T-15 | Per-seam fakes/mocks/fixtures and failure-injection scenario catalog for §20. | Test-scenario design belongs to the testing surface. | `testing-seams.md` |

---

## 24. Acceptance criteria

These contracts are accepted when:

1. **Responsibility coverage.** §2 fixes collector ownership (provider
   communication, pagination, interpretation, failure input,
   normalization handoff, provenance, capability input, partial-result
   awareness, cancellation, boundedness) and non-ownership (no verdicts,
   severity, rendering, AI, aggregation).
2. **Taxonomy coverage.** §3 covers normalized identities,
   accountability/ownership, credential metadata, permission/access,
   supported identity relationships, and capability-gated agent-identity
   information — without prescribing class-per-category shapes and
   without invented provider mappings.
3. **Input hygiene.** §4 carries tenant scope, opaque access reference,
   requested capability set, operation context, cancellation intent, and
   deterministic configuration — with no raw token/secret exposure and
   no framework-specific types.
4. **Result richness.** §5 preserves observations, capabilities,
   provenance, completeness, partiality, failures, cancellation, and
   safe diagnostics — never a boolean or null/non-null, never an
   assessment state.
5. **Normalization integrity.** §6 enforces provider-native →
   validation → normalization → provider-independent observations, with
   no provider objects reaching rules.
6. **Completeness discipline.** §§7–8 distinguish observed, confirmed
   empty, not-requested, unsupported, authorization-limited,
   licensing/service-limited, failed, unknown, and partial — and deny
   that API-success-plus-zero-objects universally equals
   confirmed-empty.
7. **Pagination soundness.** §9 preserves completeness across pages,
   preserves partial state on mid-sequence failure, fails visibly on
   invalid continuation, bounds loops, handles duplicates
   deterministically, and loses no page silently — without selecting
   mechanisms or sizes.
8. **Retry restraint.** §10 recognizes and preserves normalized
   retry/throttle conditions without choosing counts, delays,
   algorithms, policies, or libraries.
9. **Partial-failure explicitness.** §11 keeps usable data where
   correctness allows while keeping every gap (page, collector,
   subject, capability, record, cancellation) explicit.
10. **Provenance sufficiency.** §12 captures origin/context for
    normalized facts without secrets and without finalizing
    findings/evidence schema.
11. **Tenant soundness.** §13 keeps every invocation/result tenant
    scoped with cross-tenant mixing prohibited as an integrity failure,
    assuming no SaaS hosting.
12. **Validation/boundedness.** §§14–15 treat provider data as
    untrusted and processing as bounded, with exact schemas and numeric
    limits deferred as TBDs.
13. **Determinism/composition/translation.** §§16–18 fix deterministic
    normalization, explicit conflict-preserving composition, and
    concept-first error translation with no provider exception types in
    Core.
14. **Boundary posture.** §19 consumes the validated auth boundary
    without raw token passing; §20 identifies all required test seams
    without creating tests.
15. **Five-state agreement.** Exactly `PASS`, `FAIL`, `NOT_EVALUATED`,
    `NOT_APPLICABLE`, `ERROR` are the only authoritative states;
    capability/data/operational terms exist only in their correct typed
    layer — in full agreement with `core-contracts.md`,
    `domain-types.md`, `capability-model.md`, and `error-result-model.md`.
16. **Critical-case coverage.** CASE B (mid-pagination failure stays
    partial), CASE C (zero records ≠ confirmed-empty by default),
    CASE D (malformed data never silently discarded into false
    completeness), and CASE F (cancellation never becomes a verdict)
    are all enforced by the sections above; CASE A and CASE E are
    enforced jointly with `authentication-design.md`.
17. **TBD explicitness.** Every implementation-sensitive unknown is
    listed in §23 with its owning document; no endpoint, permission,
    property, mapping, package, mechanism, or numeric limit is invented.
18. **Non-goal containment.** Nothing in §22 appears as an assumed
    capability.
19. **Maturity honesty.** Nothing herein claims collection,
    pagination/retry logic, authentication, permissions, controls, or
    readiness is implemented or tested — these are intended
    implementation contracts only.

---

*(End of file)*
