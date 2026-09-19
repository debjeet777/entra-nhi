# Authentication Design

> **Phase:** 0.3.6
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed conceptual security-boundary design. This document
  is design output only. It authorizes no implementation.
- **Scope:** Defines the security boundary by which EntraNHI establishes
  authorized provider access for supported execution modes without
  leaking credential material into Core or product outputs. Derived from
  the approved authentication/authorization architecture
  (`docs/02-architecture/authentication-authorization.md`), trust
  boundaries, threat model, system context, and invariants (INV-01,
  INV-03, INV-08–INV-10, INV-14, INV-16), as structured by
  `solution-structure.md` §§4–8 and governed by `README.md` §§4–6.
- **What this document is:** The owner of the authentication/
  authorization implementation boundaries: execution contexts, the
  authorized-access-context abstraction shape, per-capability
  authorization surface, secret/token handling, tenant-context
  establishment, and failure categories (`README.md` §5–§6). First
  normative definition of the access-context abstraction lives here;
  all other documents use it referentially.
- **What this document is not:** It is not a C# implementation, a flow
  selection, a permission/scope list, a credential-mechanism selection,
  an SDK authentication-class selection, a token-cache/storage design,
  or a configuration-file format. Detailed consumers live elsewhere:
  collector interfaces in `collector-contracts.md`, capability states in
  `capability-model.md`, error/result shapes in `error-result-model.md`,
  configuration surfaces in `configuration-design.md`, output
  projections in `output-renderer-contracts.md`, and the shared core
  surface in `core-contracts.md` (which references — never duplicates —
  the boundary defined here).
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
  created here. Operational terms used below (`AuthenticationFailed`,
  `AuthorizationLimited`, `Partial`, `Cancelled`, `Unsupported`,
  `Unavailable`) are explicitly typed in their correct layer per
  `error-result-model.md` and `capability-model.md` — never as
  assessment states.

---

## 2. Authentication vs authorization vs assessment

Four conceptually distinct questions. Conflating them is a defect;
each maps to a different contract layer.

1. **Authentication — who/what establishes execution identity.**
   The process by which EntraNHI establishes an execution identity and
   obtains a validated, tenant-bound, runtime-confined authorized context
   for collection. Answers: *under what authority is this run executing?*
   Boundary owned by this document (§§4–5).
2. **Authorization — what that execution identity is permitted to read.**
   The per-capability determination of which collection categories the
   established identity may access in the target environment. Answers:
   *which categories are readable, and where is access denied?* Denials
   become explicit capability/data limitations per `capability-model.md`
   (§11), never silent emptiness.
3. **Capability/completeness — what data was actually
   available/observable.** The per-category/per-subject record of what
   collection established (observed, confirmed-empty, gapped, failed,
   unknown, partial) per `capability-model.md` §§5–6 and
   `collector-contracts.md` §7. Answers: *what could the assessment
   actually see?*
4. **Assessment — what deterministic rules conclude from sufficient
   normalized data.** The per-rule verdict (`PASS`/`FAIL`/
   `NOT_EVALUATED`/`NOT_APPLICABLE`/`ERROR`) produced only from
   normalized inputs plus capability state plus deterministic context
   (`core-contracts.md` §§4, 6). Answers: *what does the evidence prove?*

Normative non-implications — successful authentication MUST NOT imply:

- Sufficient authorization (any capability MAY still be denied; §11).
- Complete collection (authorized categories MAY still fail, gap, or
  partial; `collector-contracts.md` §§9, 11).
- Supported capability (categories MAY be `Unsupported` or `Unknown`;
  `capability-model.md` §5).
- `PASS` (verdicts require rule-level completeness + evidence;
  `core-contracts.md` §4).
- Product/security compliance (assessment answers are per-rule verdicts,
  never a global compliance claim from authentication alone).

---

## 3. Supported execution-mode abstraction

Architecture requires conceptual support for two execution modes. Both
are defined abstractly here; no concrete mechanism is selected for
either (TBD §23).

### 3.1 Interactive / delegated execution (conceptual)

- A human operator initiates the run and establishes execution identity
  through an approved identity mechanism under operator control.
- The operator is aware of the target tenant/context being assessed
  (§15); tenant validation applies before collection.
- The established context carries only the minimum read-only access the
  enabled capabilities require (§7); interactive establishment MUST NOT
  be treated as proof of complete authorization.
- Business logic MUST NOT persist, log, serialize, or expose credential
  material from the interactive establishment.

### 3.2 Non-interactive / workload execution (conceptual)

- An authorized workload identity initiates the run without interactive
  operator input (e.g., scheduled or pipeline-triggered assessment).
- The workload identity boundary, tenant binding (§8), credential
  containment (§§5–6), and least privilege (§7) apply identically.
- Secretless mechanisms are preferred where supported and practical —
  stated as a preference only, without claiming universal availability
  and without selecting any concrete workload credential type (§16).
- Workload establishment MUST NOT be treated as proof of complete data;
  authorization gaps, capability gaps, and collection failures remain
  explicit.

### 3.3 Mode-abstraction rules

- Application MAY select/request an execution mode and coordinate the
  boundary; Core MUST NOT inspect authentication mechanics
  (`core-contracts.md` §12).
- Do NOT claim all environments support all modes. Mode availability is
  environment-dependent; unsupported modes surface explicitly rather
  than degrading silently.
- Do NOT choose exact establishment mechanisms here unless already
  locked by authoritative design (none are). Mechanism selection,
  validation mechanics, and environment-specific behavior are TBDs
  (§23).

---

## 4. Authentication boundary

Conceptual provider-access/authentication abstraction consumed by
Infrastructure/provider collectors without exposing credential material
upward:

1. **Placement.** The boundary sits between Application orchestration
   and provider collector implementation: Application selects/requests
   the execution mode and coordinates establishment; collectors consume
   the resulting validated, tenant-bound access reference to perform
   authorized requests (`collector-contracts.md` §19); Core never sees
   the boundary internals.
2. **Direction.** Establishment flows inward (boundary produces a
   validated context; orchestration routes it; collectors consume it).
   Credential material never flows upward into domain, graph, rules,
   findings, evidence, output, logs, telemetry, AI context, or persisted
   product data (§5).
3. **Abstraction shape (conceptual, non-normative):** tenant assessment
   context; source/API family; authenticated execution-context
   reference (mode, non-credential); transient access handle confined
   to the boundary/runtime (never serialized into product models);
   authorization/capability metadata where safely available;
   lifetime/expiry context where required for runtime operation;
   sanitized failure/cancellation/correlation context.
4. **Core MUST NOT inspect authentication mechanics.** No token type,
   credential mechanism, establishment detail, cache/store object,
   permission-name constant, or authorization-header concept appears in
   any Core contract (`core-contracts.md` §12).
5. **Provider-specific authentication implementation lives outside
   Core** (§17): behind the adapter boundary, reached only through the
   abstract reference.

```csharp
// Illustrative only. Conceptual and non-production. Not a member list.
// No token, credential, cache, header, or SDK type appears here.
record AuthorizedAccessContext {
  TenantAssessmentContext Tenant; SourceFamilyRef Source;
  ExecutionModeRef Mode; AccessHandleRef Access;
  // + non-credential authorization metadata, lifetime context,
  //   sanitized diagnostics/correlation where applicable.
}
```

---

## 5. Credential/token containment

Normative prohibition. Credential/token material MUST NOT enter any of
the following, unless some future explicitly approved secure mechanism
requires storage outside those product data paths — and no such
mechanism is defined here (none is approved; this document designs no
storage):

- Normalized domain objects.
- Identity graph (nodes, edges, projections, diagnostics).
- Rule input (definitions, evaluation inputs, derived values).
- Findings.
- Evidence.
- Output-neutral assessment result.
- JSON / SARIF / HTML artifacts.
- Terminal output.
- Logs / telemetry.
- Exception / error messages and rendered diagnostic text.
- AI/LLM context (no model consumer sits on this path in V1, INV-13).
- Persisted configuration.
- Source repository (no checked-in secrets, sample secrets, or
  secret-bearing fixtures).

"Credential/token material" includes: access tokens, refresh tokens,
client secrets and secret values of any kind, private keys and
private-key material (including raw certificates containing private
material), passwords, recovery codes, raw authorization headers,
credential-bearing URLs/query strings, secret-bearing configuration
values, and any serialized form of the above. Do NOT define a storage
mechanism here.

---

## 6. Token lifetime / transient handling

Authentication implementations MAY require transient credential/token
handling to perform authorized requests. Only containment principles
are defined here (no lifetime constant, cache technology, or storage
mechanism is chosen):

1. **Shortest practical lifetime.** Transient material exists only as
   long as the authorized operation requires; refresh/reacquisition
   behavior, if any, is confined to the validated implementation behind
   the boundary.
2. **Process/boundary confinement.** Transient material lives only
   within the validated authentication/provider boundary runtime; it is
   never passed as a product-model value, never returned to
   Application/Core consumers, and never leaves the process except as
   the provider communication itself requires.
3. **No business-logic inspection.** Application orchestration,
   normalization, graph, rules, findings, and renderers MUST NOT read,
   branch on, log, or transform credential/token values.
4. **No serialization into product models.** Transient material MUST NOT
   appear in any record, envelope, event, diagnostic, artifact, or
   persisted state defined by this package.
5. **No logging.** No credential/token value, header, or
   credential-bearing URL appears in logs, telemetry, diagnostics, or
   error text (§§18–19).
6. **No evidence inclusion.** No credential/token material appears in
   evidence, provenance, findings, or output-neutral results.
7. **No renderer exposure.** No renderer receives, formats, or emits
   credential/token material under any output option.
8. **No AI exposure.** No credential/token material enters any model
   context (V1 has no authorized model consumer on this path).
9. **Sanitized failures.** Failures involving transient material are
   reported as sanitized operational failures (category + scope + safe
   reason), never with material-bearing detail (§§12, 18).

Do NOT claim "the application never holds a token" if a future
validated authentication implementation necessarily handles one
transiently: transient boundary-confined handling required by the
validated implementation is not prohibited by this document; what is
prohibited is upward/sideways leakage into product data paths (§5).

---

## 7. Least privilege

1. Authentication/authorization design MUST target documented read-only
   access necessary for approved V1 collection (INV-01; INV-08).
   Collection permissions (what the assessing principal needs) MUST NOT
   be conflated with assessed-identity permissions (what an identity
   under assessment possesses).
2. Each collection capability requests only the minimum source access it
   requires; optional capabilities MUST NOT silently broaden privilege
   requirements. Enabling an optional category requires explicit
   configuration, and its unavailability remains explicit rather than
   forcing broader access (INV-08; INV-16).
3. Do NOT list exact permission names here. Exact permission mapping is
   a documentation-validation and implementation TBD (§23).
4. No write/remediation access is justified by V1. The boundary MUST
   never grant, request, or assume mutation-capable access (INV-01;
   SEC-003–SEC-007).
5. Authentication success MUST NOT imply authorization for every
   capability: per-capability authorization is evaluated independently
   (§11).

---

## 8. Tenant binding

Authentication context MUST be explicitly bound to the intended
tenant/execution context (INV-10). Protections required:

1. **Accidental wrong-tenant assessment.** The boundary MUST establish
   and validate the tenant assessment context before collection begins;
   establishment against an unintended tenant MUST fail safely and
   visibly rather than assessing the wrong environment. Tenant-validation
   mechanics are a TBD (§23).
2. **Mixing observations from different tenants.** All observations,
   capability facts, diagnostics, and artifacts produced under the
   context carry its tenant binding; cross-tenant mixing is an integrity
   failure (`error-result-model.md` §13) — fail closed and visibly,
   never merged, never converted into verdicts.
3. **Reusing provider context under an incompatible tenant context.**
   A validated context bound to one tenant MUST NOT be reused for
   collection, normalization, graph construction, evaluation, or output
   under a different tenant context. Reuse attempts MUST be rejected
   explicitly.
4. Do NOT assume SaaS multi-tenancy. V1 executes one tenant per run;
   tenant isolation (this section) is distinct from simultaneous
   multi-tenant service operation (a V1 non-goal).

---

## 9. Execution identity vs assessed identities

1. The identity used to authenticate EntraNHI (the assessing execution
   identity — operator or workload) is distinct from the non-human
   identities being assessed (application registrations, service
   principals, managed-identity-classified records, supported
   agent identities per `domain-types.md` §3).
2. Do NOT merge those concepts: the assessing identity MUST NOT appear
   as an assessed identity record, MUST NOT supply evidence for rule
   predicates, and MUST NOT be normalized into the domain model as
   assessment data — except where the source independently returns it
   as an assessed-environment observation through normal collection
   with its own provenance (in which case it is assessment data like
   any other observation, not privileged by its assessing role).
3. Authorization available to the assessing identity constrains what can
   be collected; it says nothing about the security posture of the
   assessed identities (§§2, 11).

---

## 10. Authentication result

Authentication MUST NOT be represented simply as boolean success.
Conceptually preserve at minimum:

- Established / not established (whether a usable authorized context
  was produced).
- Tenant binding (the tenant assessment context the result is bound
  to, §8).
- Execution mode (interactive vs workload abstraction, §3 —
  non-credential reference only).
- Safe provider-context reference (opaque, non-credential handle
  collectors consume; §4).
- Sanitized operational failure (typed category per
  `error-result-model.md` §5 — authentication-boundary failure family —
  with safe reason; no provider error bodies, no credential-bearing
  diagnostics without sanitization; §12).
- Cancellation (establishment cancelled where implementation permits;
  distinct from failure and verdicts; §13).
- Correlation/operation context where appropriate (assessment/run/
  operation reference enabling end-to-end tracing).

Do NOT expose raw credentials in any authentication-result field,
including nested causes and rendered text.

---

## 11. Authorization limitation

1. Authentication success with insufficient read authorization MUST
   become explicit capability/data limitation: the affected categories
   carry `UnavailableAuthorization` (or `Unknown` where the cause cannot
   safely be distinguished) with tenant-scoped sanitized reason, per
   `capability-model.md` §§5, 13 — flowing into `NOT_EVALUATED` per
   `capability-model.md` §9 (R-06).
2. Authorization limitation MUST NOT become:
   - An empty dataset (no "zero rows" substitution).
   - Confirmed empty (`collector-contracts.md` §8 applies: completeness
     semantics must justify confirmed-empty; a denial justifies no such
     claim).
   - `PASS` (CASE A; false-PASS battery in `capability-model.md` §20).
3. It MUST NOT automatically become tenant `FAIL` either: denial is
   absence of evidence, not evidence of failure (`core-contracts.md` §4,
   rule 2).
4. Coordinate with `capability-model.md` (transition semantics) and
   `error-result-model.md` (scoped-gap vs systemic-failure mapping):
   per-category denials are scoped gaps (`NOT_EVALUATED`); total failure
   to establish any authorized context is systemic (assessment-wide,
   never per-rule `PASS`).

---

## 12. Authentication failure

1. Authentication failure — failure to establish any usable authorized
   context — is an operational/security boundary failure
   (`error-result-model.md` F-02 family): systemic, not per-rule.
2. It is recorded assessment-wide with sanitized cause, scope (which
   establishment was attempted, under which mode/tenant intent), and
   correlation context — never as per-rule `PASS`, never as tenant
   `FAIL`, never as confirmed-empty collection.
3. Downstream collection MUST NOT proceed as if authorized; any
   observations produced without a validated context are untrustworthy
   and MUST NOT feed evaluation.
4. Do NOT expose provider error bodies or credential-bearing diagnostics
   without sanitization: raw provider messages are untrusted data
   carried opaquely at most behind the adapter boundary, normalized into
   conceptual categories before crossing toward orchestration/Core, with
   secrets stripped at every link (`error-result-model.md` §§9–10).

---

## 13. Cancellation

1. Authentication acquisition/establishment MUST respect cancellation
   where implementation permits: an observed cancellation terminates
   establishment promptly and records cancellation (F-13 family) with
   scope and correlation context.
2. Cancellation remains distinct from rule verdicts: it is the
   operational cause; affected evaluations record the consequence
   (`NOT_EVALUATED` where evaluation never meaningfully began, or
   `ERROR` where termination broke trust/completeness mid-execution —
   exact per-scope mapping owned with `error-result-model.md`), and
   cancellation itself is never a sixth assessment state (CASE F).
3. Downstream stages MUST NOT evaluate or present cancelled scope as
   complete; orchestration propagates cancellation without converting it
   into `PASS`/`FAIL`.
4. No cancellation mechanism (token/task/pattern choice) is selected
   here (TBD §23).

---

## 14. Configuration boundary

1. Authentication design MAY consume validated configuration supplied
   through the future configuration boundary (which optional modes or
   capabilities are enabled, non-secret scoping choices) — as explicit,
   validated, non-secret parameters.
2. Do NOT have Core read environment variables directly
   (`core-contracts.md` §12): Core receives validated deterministic
   configuration values as explicit parameters only.
3. Do NOT specify secret-bearing configuration file formats here: no
   format, path, schema, or persistence posture for credential-bearing
   settings is designed in this document.
4. Detailed configuration ownership — option catalog, secure defaults,
   explicit-opt-in, unknown/invalid-option semantics — belongs to
   `configuration-design.md`. Security-sensitive defaults apply
   (INV-16): where optional behavior could weaken authorization
   boundaries, the secure behavior is the default.

---

## 15. Interactive execution security

Conceptually cover (no mechanism selected; TBD §23):

1. **User awareness of tenant/context.** The operator is shown the
   intended tenant/execution context before and during assessment so
   wrong-tenant execution is preventable and detectable; establishment
   against an unintended tenant fails safely and visibly (§8).
2. **Least privilege.** The interactively established context carries
   only minimum read-only access for enabled capabilities (§7); no
   elevation, no silent broadening.
3. **Tenant validation.** Establishment validates the tenant binding
   before collection; validation mechanics are a TBD.
4. **Safe cancellation/failure.** The operator MAY cancel establishment;
   cancellation and failure surface as sanitized, explicit outcomes
   (§§10, 12–13) — never silent continuation, never verdicts.
5. **No credential persistence by business logic.** CLI/command,
   orchestration, normalization, graph, rule, finding, and renderer
   logic MUST NOT persist, cache beyond the transient boundary need,
   log, or serialize interactive credential material (§§5–6).
6. **No completeness assumption.** Interactive establishment MUST NOT be
   treated as implying complete authorization, supported capability, or
   complete collection (§2).

---

## 16. Workload execution security

Conceptually cover (no concrete credential type selected; TBD §23):

1. **Non-interactive identity boundary.** The workload identity under
   which the run executes is the execution identity (§9); its authority
   is bounded to the intended tenant and enabled read-only capabilities.
2. **Least privilege.** Same posture as §15: minimum read-only access,
   explicit opt-in for optional capabilities, no silent broadening (§7).
3. **Tenant binding.** Same protections as §8: wrong-tenant rejection,
   no cross-tenant mixing, no incompatible-context reuse.
4. **Credential containment.** Same prohibitions as §§5–6: no workload
   credential material in product data paths, diagnostics, logs, or
   persisted artifacts.
5. **Secretless mechanisms preferred where supported and practical** —
   stated as a preference only, without claiming universal availability
   and without selecting any concrete mechanism or storage technology.
6. **No repository-stored credentials.** Workload credential material
   MUST NOT live in the source repository, sample configuration,
   fixtures, or documentation.
7. **No completeness assumption.** Workload establishment MUST NOT be
   treated as implying complete data (§2).

---

## 17. Authentication provider abstraction

1. Provider-specific authentication implementation lives outside Core,
   behind the adapter boundary: establishment mechanics, provider
   credential handling, transient access handling, cache/store behavior
   (if any), and provider communication all stay in the outer
   implementation consumed through the §4 abstraction.
2. Do NOT expose into Core, Application contracts, or product data:
   - Provider SDK authentication classes or credential classes.
   - Token-cache objects or stores.
   - HTTP authorization headers or credential-bearing URLs.
   - Token/secret values in any form.
3. Application references the abstraction only (mode selection,
   coordination, opaque handle routing); it MUST NOT inspect, persist,
   or branch on authentication mechanics.
4. `Core` consumes only the non-credential assessment/execution context
   (tenant binding, mode reference, deterministic configuration) — never
   the access mechanics (`core-contracts.md` §§5, 12).

---

## 18. Failure/sanitization rules

Explicitly prohibit leakage of the following in every failure,
diagnostic, log, telemetry, output, and AI-context field — including
nested causes and rendered text:

- Tokens (access, refresh, or any transient access value).
- Secrets and secret values of any kind.
- Passwords and recovery codes.
- Private keys and private-key material (including raw certificates
  containing private material).
- Raw authorization headers.
- Credential-bearing URLs / query strings.
- Secret-bearing configuration values.
- Provider diagnostic payloads that may contain sensitive material
  (carried at most opaquely behind the adapter boundary; normalized and
  sanitized before crossing toward orchestration/Core).

Sanitization applies at every link of every cause chain
(`error-result-model.md` §10): strip secrets, normalize away provider
exception types, exclude raw payloads by default.

---

## 19. Logging/telemetry

1. Only sanitized authentication events/context MAY cross into abstract
   logging/telemetry: establishment succeeded/failed/cancelled, mode,
   tenant-scoped correlation identifiers, failure categories, safe
   reason codes — never credential material (§18).
2. Never log credential material: no token, secret, password, key,
   header, credential-bearing URL, or secret-bearing configuration
   value appears in any log, telemetry event, diagnostic, or error text.
3. Tenant-originated and provider-originated strings remain untrusted
   data in transit; no undisclosed telemetry or third-party transmission
   of assessment data; destinations, if any, are explicit and
   operator-visible (per `error-result-model.md` §16).
4. Tenant isolation applies to telemetry as to output: no cross-tenant
   correlation or aggregation in V1.
5. Do NOT choose telemetry backend, vendor, format, verbosity default,
   or retention policy here (CON-003). Any minimal abstraction boundary
   is owned by `dependency-boundaries.md`.

---

## 20. Testing implications

Design for testing (scenario catalog itself belongs to `testing-seams.md`;
no tests are created here; all fixtures synthetic, secret-free,
network-free):

- Correct tenant binding (establishment binds the intended tenant;
  downstream data carries the binding).
- Wrong-tenant rejection (establishment against an unintended tenant
  fails safely and visibly; incompatible-context reuse is rejected).
- Interactive/workload abstraction (both modes exercisable through the
  abstract boundary with faked establishment; no live identity, no real
  credential).
- Insufficient authorization (denied categories become explicit
  capability limitations → `NOT_EVALUATED`, never empty + `PASS`;
  CASE A).
- Authentication failure (no validated context → systemic,
  assessment-wide, sanitized; no evaluation proceeds as if authorized).
- Cancellation (establishment cancelled → explicit cancellation outcome,
  incomplete scope preserved; CASE F).
- Sanitized diagnostics (provider-shaped failure fixtures yield
  normalized categories with secrets stripped).
- Token/secret non-propagation (token/secret-shaped synthetic material
  has no path into domain, findings, evidence, outputs, logs, or
  diagnostics; CASE E).
- No credential serialization (no product-model, envelope, artifact, or
  persisted fixture carries credential material).
- Collector boundary interaction (collectors consume the faked opaque
  handle; absent/expired/tenant-incompatible handles produce explicit
  failures, never silent authorized behavior).
- Do NOT choose concrete test frameworks here.

---

## 21. Security implications

Stated as structural support, not as implemented controls:

- **Least privilege (INV-08).** §7 targets minimum read-only access per
  enabled capability; optional capabilities never silently broaden
  requirements.
- **Credential containment (INV-09).** §§5–6, 18–19 leave tokens/secrets
  no type, field, or path into domain, graph, findings, evidence,
  diagnostics, output, logs, telemetry, AI context, or persisted data
  (CASE E).
- **Tenant isolation (INV-10).** §8 plus wrong-tenant rejection and
  reuse prohibition keep every run single-tenant with integrity-failure
  handling for contamination.
- **False-PASS prevention (INV-05; INV-07; INV-14).** §§2, 11–12 keep
  authentication success, authorization gaps, and establishment failures
  from ever presenting as secure verdicts (CASE A).
- **No write access for V1 (INV-01).** §7 prohibits mutation-capable
  scope; no contract defines a mutating operation.
- **Fail-transparent behavior (INV-14).** §§10–13 keep establishment,
  authorization, and cancellation outcomes explicit with sanitized cause
  — never silent success, never verdict conversion.
- **Separation of authentication from collection (INV-03).**
  Establishment owns identity/context; collectors consume the validated
  reference without owning credentials.
- **AI non-authority (INV-13).** No contract path admits model output
  onto establishment, authorization, collection, evaluation, evidence,
  capability, or severity.
- **Undocumented-behavior resistance (INV-15).** No flow, permission,
  endpoint, property, mapping, or licensing claim is made here; every
  such detail is a named TBD (§23).

---

## 22. Explicit non-goals

1. No exact establishment-mechanism selection for either execution mode.
2. No token-cache/storage technology or mechanism selection.
3. No exact permission/scope-name mapping or rationale finalization
   (rationale detail is a TBD, §23).
4. No SDK authentication-class, credential-class, or provider-library
   selection.
5. No secret-bearing configuration format, path, schema, or persistence
   design.
6. No numeric lifetime, timeout, retry, or resource-limit constants.
7. No framework-specific cancellation implementation and no
   threading/task-library selection.
8. No complete error-code catalog (principles via `error-result-model.md`
   §8; catalog TBD there).
9. No exit-code table, CLI syntax, SARIF mapping, HTML templating, or
   signing/hashing implementation.
10. No sixth authoritative assessment state and no generic boolean
    "success" substitute for the five-state model.
11. No SaaS/multi-tenant service, dashboard, remediation, monitoring, AI
    verdicts, or other NG-series capabilities.

---

## 23. Open TBDs and owner documents

Each TBD states what is unknown, why it cannot be resolved here, and
which later document owns its resolution. None is resolved by
speculation.

| # | TBD | Why unresolved here | Owned by |
| --- | --- | --- | --- |
| T-01 | Concrete access-context record/interface shapes and member lists for §4, and the authentication-result envelope shape/serialization. | Member-level design needs coordination with collector, capability, findings, and output owners; choosing members here would preempt them. | This document (semantics) coordinated with `core-contracts.md`; physical shapes per consumer in their owning documents; seam placement in `dependency-boundaries.md` |
| T-02 | Exact establishment mechanisms per execution mode (interactive and workload), including validation mechanics and environment-specific behavior. All require documentation validation and MUST NOT be invented. | Mechanism choice needs implementation-phase review outside boundary semantics. | This document |
| T-03 | Permission rationale per collection capability (which source access each category needs), validated against published documentation. | Rationale needs validated documentation review outside boundary semantics. | This document (rationale), mappings coordinated with `collector-contracts.md`, granularity with `capability-model.md` |
| T-04 | Documentation-sensitive licensing/service distinction detail behind authorization vs licensing/service causes. | Commercial-behavior claims MUST NOT be embedded without validated documentation. | This document with `collector-contracts.md`; semantics owned by `capability-model.md` |
| T-05 | Tenant-validation mechanics (how intended-tenant binding is established and verified before collection). | Mechanics need implementation-phase design plus structural-test enforcement. | This document with `dependency-boundaries.md` |
| T-06 | Token-cache/storage policy (whether any caching or persistence is ever authorized, with what confinement/bounds) — if the validated implementation requires it. | Storage and lifecycle design needs implementation-phase security review; no mechanism is authorized here. | This document with `implementation-sequence.md` (staging) |
| T-07 | Lifetime/expiry handling detail (how expiry context is observed and acted on at runtime). | Runtime detail needs collector/boundary coordination; no constant is chosen here. | This document with `collector-contracts.md` |
| T-08 | Cancellation mechanism choice for establishment. | Mechanism choice needs boundary/collector coordination. | This document (establishment side), `collector-contracts.md` (collection side) |
| T-09 | Identifier formats (assessment, execution-context, provenance, diagnostic, correlation references carried in §§4, 10). | Formats need dedicated design plus structural-test enforcement. | `findings-evidence-schema.md` with `domain-types.md` |
| T-10 | Evidence/provenance/diagnostic record schemas carrying authorization/capability context, emission policy, redaction schema. | Integrity mechanisms need separate security review. | `findings-evidence-schema.md` |
| T-11 | Failure-category mapping detail: establishment/authorization failures to assessment-wide vs per-category handling (with `error-result-model.md` taxonomy). | Mapping needs rule-engine and collector coordination; a premature table risks inventing semantics. | This document with `rule-engine-contracts.md` and `collector-contracts.md`, constrained by `capability-model.md` §9 and `error-result-model.md` §4 |
| T-12 | Exact provider-signal to failure-category mappings for establishment/authorization signals. | Requires validation against published documentation; inventing mappings would violate INV-15. | This document (mappings), taxonomy owned by `error-result-model.md` |
| T-13 | Logging/telemetry abstraction surface, enforcement tooling, and backend selection. | Seam-catalog and supply-chain decisions belong downstream. | `dependency-boundaries.md` |
| T-14 | Per-seam authentication-failure-injection scenario catalog (wrong-tenant, denial, expiry, cancellation fixtures). | Test-scenario design belongs to the testing surface. | `testing-seams.md` |
| T-15 | Configuration-file format, option catalog detail, secure defaults, unknown/invalid-configuration behavior touching §14. | Configuration design belongs to its owning surface. | `configuration-design.md` |

---

## 24. Acceptance criteria

This design is accepted when:

1. **Distinction soundness.** §2 keeps authentication, authorization,
   capability/completeness, and assessment distinct, with successful
   authentication implying none of authorization sufficiency,
   collection completeness, capability support, `PASS`, or compliance.
2. **Mode abstraction.** §3 defines interactive and workload execution
   abstractly — without selecting mechanisms and without claiming all
   environments support all modes.
3. **Boundary integrity.** §4 places a consumable, credential-free
   abstraction between orchestration and collectors that Core never
   inspects, with provider-specific implementation outside Core (§17).
4. **Containment completeness.** §5 prohibits credential/token material
   from every product data path (domain, graph, rules, findings,
   evidence, output-neutral result, all renderers, terminal, logs,
   errors, AI context, persisted configuration, repository) with no
   storage mechanism defined here; §6 confines transient handling by
   principle (lifetime, confinement, no inspection/serialization/
   logging/evidence/renderer/AI exposure, sanitized failures).
5. **Privilege restraint.** §7 targets documented read-only minimum
   access, keeps assessing-vs-assessed permissions distinct, defers
   exact permission mapping as a TBD, and justifies no write access.
6. **Tenant soundness.** §8 binds context to the intended tenant with
   wrong-tenant rejection, no cross-tenant mixing, and no
   incompatible-context reuse — assuming no SaaS multi-tenancy; §9
   keeps execution identity distinct from assessed identities.
7. **Result richness.** §10 preserves established/not-established,
   tenant binding, mode, safe context reference, sanitized failure,
   cancellation, and correlation — never a boolean, never raw
   credentials.
8. **Limitation/failure honesty.** §11 turns authorization gaps into
   explicit capability limitations (never empty, never confirmed-empty,
   never `PASS`, never automatic `FAIL`); §12 treats establishment
   failure as systemic with sanitized propagation and no unauthorized
   collection; §13 keeps cancellation distinct from verdicts with no
   mechanism selected; §14 consumes only validated configuration
   without Core environment reads or secret-bearing formats.
9. **Execution-mode safety.** §§15–16 cover operator awareness, least
   privilege, tenant validation, safe cancellation/failure, no
   credential persistence, and no completeness assumptions for both
   modes — preferring secretless workload mechanisms only where
   supported and practical, requiring no repository-stored credentials,
   and selecting no concrete mechanism.
10. **Hygiene enforcement.** §§18–19 prohibit every credential/token
    form (plus sensitive provider payloads) from all diagnostics, logs,
    telemetry, and outputs, admit only sanitized events, and select no
    backend.
11. **Test readiness.** §20 identifies tenant-binding, wrong-tenant,
    mode-abstraction, denial, failure, cancellation, sanitization,
    non-propagation, non-serialization, and collector-interaction seams
    without creating tests or choosing frameworks.
12. **Five-state agreement.** Exactly `PASS`, `FAIL`, `NOT_EVALUATED`,
    `NOT_APPLICABLE`, `ERROR` are the only authoritative states;
    operational terms exist only in their correct typed layer — in full
    agreement with `core-contracts.md`, `domain-types.md`,
    `capability-model.md`, and `error-result-model.md`.
13. **Critical-case coverage.** CASE A (denial stays a limitation, never
    empty + `PASS`), CASE E (transient token handling never reaches
    product data), and CASE F (cancellation never becomes a verdict)
    are all enforced by the sections above; CASE B, CASE C, and CASE D
    are enforced jointly with `collector-contracts.md`.
14. **Cross-boundary coherence.** The inequalities hold end to end:
    authentication success ≠ authorization completeness;
    authorization limitation ≠ empty; empty ≠ confirmed-empty by
    default; partial pagination ≠ complete; cancellation ≠ assessment
    verdict; provider objects do not cross into Core; credentials/tokens
    do not cross into product data/results.
15. **TBD explicitness.** Every implementation-sensitive unknown is
    listed in §23 with its owning document; no flow, permission,
    endpoint, property, mapping, package, mechanism, cache/store
    technology, or numeric limit is invented or selected.
16. **Non-goal containment.** Nothing in §22 appears as an assumed
    capability.
17. **Maturity honesty.** Nothing herein claims authentication is
    implemented, permissions are configured, collectors exist,
    controls passed testing, or readiness exists — this is intended
    implementation-boundary design only.

---

*(End of file)*
