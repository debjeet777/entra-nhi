# Domain Types

> **Phase:** 0.3.4
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Status and scope

- **Status:** Proposed normalized domain vocabulary. This document is design
  output only. It authorizes no implementation.
- **Scope:** Defines the provider-independent normalized domain concepts
  `EntraNHI` uses internally — the vocabulary consumed by normalization,
  the identity graph, the rule engine, findings/evidence, and output
  projections — derived from the approved architecture domain model
  (`docs/02-architecture/domain-model.md`) without inventing undocumented
  provider fields.
- **What this document is:** The owner of normalized domain type contracts:
  `IdentityRecord`, `IdentityKind`, credential-metadata, permission/access,
  accountability, tenant context, absent/unknown semantics, relationship
  vocabulary, provenance vocabulary, time principles, equality boundaries,
  and extensibility/versioning rules for the domain layer.
- **What this document is not:** It is not a C# implementation, a Graph
  binding, a property mapping, a permission list, an endpoint catalog, a
  serialization schema, or a rule catalog. Detailed consumers of this
  vocabulary are owned elsewhere: capability states by `capability-model.md`,
  graph operations by `identity-graph-contracts.md`, rule pipeline and
  `RuleEvaluation` by `rule-engine-contracts.md`, findings/evidence/
  provenance schemas by `findings-evidence-schema.md`, error/result shapes
  by `error-result-model.md`, collector envelopes by `collector-contracts.md`,
  output projections by `output-renderer-contracts.md`, and the shared core
  surface by `core-contracts.md` (which references — never duplicates —
  the concepts defined here).
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed domain contracts** owned by this document,
  and (c) **unresolved TBDs** naming the owning later document (§19).
  Nothing herein claims a control is implemented, verified, or enforced.
- **Illustrative syntax discipline:** This document MAY use C#-style
  signatures or pseudotypes where that materially improves precision. Any
  such syntax is **illustrative and non-normative** — not final production
  code, not a namespace decision, not a member list. No framework, package,
  SDK, or library implementation is selected here (CON-003). No source
  files are created by this document.

---

## 2. Domain modeling principles

Carried forward from the domain model, invariants, and requirements; stated
as constraints on the contracts below, not as implemented behavior.

1. **Provider normalization (INV-03; INV-04).** Provider/source objects are
   collector-side representations. They MUST be translated into the
   normalized contracts in §§3–11 before deterministic rule evaluation.
   Rules MUST NOT depend on provider SDK object types, raw payload
   structures, or provider HTTP semantics. Source-specific facts MAY appear
   only through normalized provenance and capability metadata.
2. **Stable internal identifiers (§4).** Identity, evidence linkage, graph
   construction, and rule evaluation use assessment-local normalized keys
   plus structured source references — never display names, never raw
   undocumented identifiers, never secrets.
3. **Tenant scoping (INV-10).** Every domain record carrying tenant-derived
   data carries the tenant assessment context. No cross-tenant edges,
   correlation, or artifact mixing is representable without triggering an
   integrity failure.
4. **Immutable/value-oriented representation where practical.** Domain
   records SHOULD be immutable values: constructed once by normalization,
   never mutated by graph construction or rule evaluation, freely
   fixture-constructible for tests. Exact C# record/class mechanics are a
   TBD (§19, T-01); the orientation is normative.
5. **Explicit unknown/absence semantics (§10; domain-model Principle 3).**
   One generic null MUST NOT represent every missing-data condition.
   Present, explicitly empty, not collected, unavailable, failed,
   unsupported, and not applicable are distinguishable domain states that
   drive — but are not themselves — evaluation states.
6. **Provenance-aware data (INV-11).** Every normalized observation retains
   enough provenance to identify its documented source and collection
   context (§11). Observed vs normalized vs derived vs evaluation data
   MUST remain distinguishable (domain-model Principle 2).
7. **Secret exclusion (INV-09; SEC-001; SEC-002; OUT-006).** Secret values,
   private-key material, client secrets, passwords, bearer/access/refresh
   tokens, and recovery codes are not domain data. They MUST NOT enter
   domain records, graph, findings, evidence, reports, logs, or persisted
   artifacts. Transient authentication material is runtime-only and never
   normalized.
8. **No invented provider semantics (INV-15; CAP-005).** The domain MUST NOT
   include fields, relationships, classifications, or mappings not supported
   by documented sources or explicitly required by product requirements.
   Where documentation is absent, the domain represents the gap explicitly
   (unsupported/unavailable) and resolves no verdict beyond what
   capability-aware rules determine (NOT_EVALUATED, never silent PASS).

---

## 3. Normalized identity concept

The `IdentityRecord` is the normalized abstraction for a supported
non-human identity within an assessment and the primary unit of rule
evaluation (FR-001–FR-004, FR-010–FR-011; domain-model identity model).

- **Conceptual content (by reference, not a member list):** assessment-local
  normalized key (§4); identity kind (§5); tenant assessment context (§4);
  source references/provenance (§11); display metadata (human-readable,
  never an identity key); capability state references per observation
  category (owned by `capability-model.md`); accountability references
  (§6); credential-metadata references (§7); permission/access relationship
  references (§8); graph relationship references (§9).
- **V1 identity categories** (classification detail in §5; no SDK mapping
  asserted here):
  - **Application registrations** — application objects; MUST remain
    distinguishable from associated service principals (FR-001).
  - **Service principals** — service-principal identities not classified
    otherwise; includes application service principals, legacy service
    principals, and cross-tenant service principals as normalized
    observations permit (FR-002). Cross-tenant *observations* are data;
    cross-tenant *correlation/aggregation* remains out of scope (INV-10).
  - **Managed identities represented through documented Entra concepts** —
    a service principal classified as a managed identity only when
    documented source evidence provides sufficient reliable support. The
    underlying service-principal record and provenance MUST be preserved;
    the model MUST NOT pretend the classified record is an unrelated
    provider object (FR-003). Exact property/value mapping is
    collector-layer design (TBD §19, T-07) and MUST NOT be invented here.
    Optional enrichment from a second documented API family (if future
    approved rules require it) remains isolated, explicitly enabled, and
    non-mandatory.
  - **Supported Microsoft Entra agent identities where authoritative APIs/
    documentation make them available** — modeled only where supported
    through documented observable capability, capability-gated so that
    unsupported or unavailable agent-identity semantics remain explicit and
    MUST NOT block unrelated assessment (FR-004). Agent blueprints or
    related concepts MAY be represented only where requirements and
    documented capability support them.
- **Prohibitions for this section:** Do NOT invent Agent ID mappings. Do
  NOT assert undocumented subtype mappings. Do NOT hard-code provider
  property/value mappings. Do NOT claim universal identifier availability
  across kinds (§4). Use the extensible classification mechanism in §5
  rather than assumptions.

Illustrative (non-normative) sketch — vocabulary only, not a shape:

```csharp
// Illustrative only. Not final code. Field list owned by this document's
// sections; capability/provenance shapes owned by their documents.
record IdentityRecord {
  IdentityKey Key; TenantAssessmentContext Tenant; IdentityKind Kind;
  IReadOnlyList<SourceReference> Sources; DisplayMetadata Display;
  // + references to accountability, credential metadata,
  //   permission relationships, capability states, provenance.
}
```

---

## 4. Identity identifiers

Three distinct identity/reference concepts (domain-model keys and source
references). Conflating them is a defect.

1. **Internal/stable normalized identity identifier (assessment-local
   normalized key).** Generated during normalization; unique within a
   single assessment; scoped to the assessment context; used for internal
   deduplication, graph construction, and rule evaluation. MUST NOT be
   derived from concatenating undocumented identifiers. MUST NOT depend
   solely on display names. Exact format and generation strategy are TBD
   (§19, T-05).
2. **Tenant identifier (tenant assessment context).** Reference preventing
   silent cross-tenant identity mixing. Each assessment operates within a
   single tenant scope; normalized records are attributable to the correct
   tenant; no cross-tenant edges or relationships are silently created;
   assessment output is unambiguously scoped (INV-10). Tenant isolation
   (this concept) is distinct from SaaS multi-tenancy (simultaneous
   multi-tenant service operation, a V1 non-goal per NG-003). Tenant-
   validation mechanics belong to `authentication-design.md`; format
   belongs to `findings-evidence-schema.md` with this document (§19).
3. **Provider/native identifier(s) (source references).** Structured
   references identifying documented external source objects sufficiently
   for provenance: source system or API family; source object type within
   that system; source object identifier as documented and exposed by the
   source; collection context that produced the observation. Source
   references MUST use only identifiers documented and exposed by the
   source system. No endpoint, property, or identifier value is invented
   here.
4. **Application/client identifier where semantically applicable.** The
   documented application-associated identifier that links related records
   (e.g., an application registration to its service principals) where the
   source documents such linkage. It is an observed, provenance-carrying
   domain fact — not a join key to be guessed, not a display name, and not
   assumed present for every kind.

Non-assumptions: Do NOT assume every identity category exposes every
identifier. Not every kind exposes the same provider identifiers; the
domain MUST represent per-kind identifier absence through §10 semantics
(not collected / unavailable / unsupported / not applicable) rather than
placeholder values. Do NOT expose secrets: no identifier concept carries
credential material, tokens, or private-key data (see §16).

---

## 5. Identity classification

An extensible normalized classification mechanism (`IdentityKind`),
derived from domain-model classification without provider-string coupling:

- **V1 classification values (conceptual):** `ApplicationRegistration`,
  `ServicePrincipal`, `ManagedIdentity`, `AgentIdentity`. Application
  registrations and service principals MUST remain distinguishable and are
  not interchangeable (FR-001, FR-002). `ManagedIdentity` is a
  service-principal-backed classification preserving underlying provenance
  (FR-003). `AgentIdentity` is capability-gated and explicit when
  unsupported/unavailable (FR-004).
- **Extensibility:** The mechanism MUST admit future identity categories
  (and future providers, structurally — see §13) without requiring rules
  to consume provider SDK models and without redesigning the normalized
  core. Extension adds a classified value plus its documented evidence
  basis, capability gating, and provenance preservation — never a silent
  reinterpretation of an existing value.
- **No fragile provider-string logic in rules:** Rules consume the
  normalized classification value, never raw provider type strings or
  undocumented property values. Any translation from provider observations
  to normalized classification happens in normalization/collector layers
  against documented evidence, and is recorded as derivation lineage
  (§11).
- **Unknown/not-yet-supported classifications:** MUST be representable
  without false interpretation — e.g., an observed identity whose
  documented classification evidence is absent or unrecognized is carried
  as an explicitly unknown/unclassified value with capability/provenance
  context, driving capability-aware rule behavior (NOT_EVALUATED where a
  rule requires the classification; NOT_APPLICABLE only where the rule's
  deterministic applicability predicate so determines) rather than silent
  coercion into a supported kind. Exact unknown-value representation and
  enum naming are TBD (§19, T-01/T-02).
- **Prohibitions:** No exact provider property/value mapping is defined
  here (TBD §19, T-07). No undocumented subtype mapping is asserted. No
  licensing-behavior claim is made (CAP-005).

---

## 6. Ownership/accountability concepts

Represent documented accountability relationships only where supported by
authoritative data (FR-012; domain-model accountability model):

- **V1 accountability categories (only):** owner, sponsor, manager — each
  as a normalized relationship to a subject reference carrying the subject
  source reference, subject display metadata where collected, and the
  normalized relationship kind. Do NOT define a generic "approver"
  relationship or any other invented category.
- **Subject references** embed no unnecessary directory objects; they
  reference normalized subjects (graph node kind `AccountabilitySubject`)
  with provenance.
- **Absence vs unavailability discipline:** where architecture requires the
  distinction, the domain MUST keep distinguishable: observed value;
  confirmed empty relationship (source explicitly indicates no value where
  that distinction is documented and reliable); not collected (outside
  collection scope); unavailable (authorization/licensing/service limit);
  failed (error prevented retrieval); unsupported (implementation does not
  support this data); not applicable (concept does not apply to this
  identity kind). Mechanism for this distinction is §10; accountability
  records MUST NOT conflate "no owner observed" with "ownership capability
  unavailable" (a false-PASS-relevant conflation, FR-012).
- **Collection note (non-normative for this document):** which documented
  relationship endpoints back each category is collector-layer design
  (TBD, `collector-contracts.md`); this document defines only the
  normalized vocabulary and the absence discipline above.

---

## 7. Credential metadata

Non-secret credential lifecycle metadata only (FR-013; SEC-001; INV-09;
domain-model credential metadata model; system-context data
classification):

- **Permitted conceptual fields — only what authoritative provider data
  can actually support:** credential type/category (where documented and
  exposed); non-secret identifier/key identifier/fingerprint-like metadata
  (only where the source documents the value as non-secret and
  requirements permit its inclusion); start/not-before time where
  available; expiry/end time where available; source reference (always);
  capability/provenance state (always).
- **Explicitly forbidden in credential metadata:** secret values, private
  keys, certificate private material, passwords, client-secret values,
  bearer/access/refresh tokens, recovery codes, authorization headers,
  secret-bearing configuration, recoverable credential data.
- **Rotation inference restriction:** Do NOT create a "last rotation date"
  unless directly and reliably exposed by a documented source. Do NOT infer
  rotation from creation or start dates.
- **Unknown/unavailable fields** MUST remain explicit through §10 semantics
  (e.g., expiry absent because the source does not expose it vs because
  collection was denied are different domain facts with different rule
  consequences). Invented placeholder timestamps or inferred values are
  prohibited (see also §12).

---

## 8. Permission/access concepts

Normalized permission/access-assignment concepts sufficient for
deterministic assessment (FR-014; domain-model permission/access model):

- **Normalized relationship categories (where observable and supported):**
  application permission / app-role assignment style relationships;
  delegated permission grant style relationships (only where the identity
  kind and source support delegated flows); resource/target references
  (the target resource or principal to which the permission/role is
  scoped, where documented); permission/role-definition references (where
  documented); provenance (always); collection/capability state (always).
- **Preserved distinctions:** permission declaration vs grant/assignment vs
  resource vs principal vs provenance MUST remain distinct concepts.
  Collection permissions (what the assessing principal needed) MUST NOT be
  conflated with assessed-identity permissions (what the identity under
  assessment possesses).
- **Scope restrictions:** NOT every permission category applies to every
  identity kind; the model MUST NOT claim universal applicability. Do NOT
  specify exact permission endpoints, permission names, or SDK models here
  (TBD, `collector-contracts.md`; permission rationale TBD,
  `authentication-design.md`). Do NOT over-model semantics not yet
  approved (no invented consent-state machine, no invented role hierarchy
  beyond what requirements and documented capability support).

---

## 9. Relationship concepts

Normalized typed relationships for the identity graph (domain-model and
identity-graph architectures; INV-G1–INV-G8):

- **Relationship requirements:** every relationship is explicit, typed,
  tenant-scoped, provenance-aware, and extensible. Every edge has either
  observed source support (directly returned by a documented source) or an
  explicitly identified deterministic derivation rule based on supported
  observations. Relationships not supported by source data MUST NOT be
  present.
- **V1 relationship vocabulary (conceptual, by reference):** identity-link
  relationships between application registrations and service principals
  (both directions where documented); managed-identity classification
  derivation lineage (derived, traceable to source observations and the
  derivation basis); accountability relationships (owner/sponsor/manager
  per §6); credential associations (per §7); permission associations (per
  §8). Agent-identity relationships (to blueprints, blueprint principals,
  service-principal infrastructure, or other agent objects) MUST be
  introduced only after collector design validates documented semantics;
  unsupported or unavailable agent capability remains explicit and MUST NOT
  block unrelated assessment.
- **Observed vs derived:** derived relationships MUST be produced by
  deterministic logic, traceable to source facts plus the derivation
  basis, and clearly labeled as derived in diagnostic/debug/audit output.
  Display-name matching MUST NOT establish relationships; undocumented
  relationships MUST NOT be assumed; AI-inferred relationships are
  prohibited.
- **Deferral.** Detailed graph traversal/query semantics, node/edge
  shapes, graph operations (lookup, traversal, subgraph extraction,
  aggregation), serialization for diagnostics, size/memory policy, key
  generation, dedup/conflict policy detail, and INV-G enforcement
  mechanics belong to `identity-graph-contracts.md`. This document owns
  only the normalized relationship vocabulary and the requirements above.
  Do NOT invent undocumented Entra relationships here.

---

## 10. Capability/data availability concepts

Domain data MUST distinguish the following data/observation conditions
(domain-model Principle 3; collection architecture §5; rule-engine
capability-aware evaluation):

- **Observed value (present):** observed value is present in the source and
  was collected.
- **Confirmed empty:** source explicitly indicates no value or empty
  relationship where that distinction is documented and reliable.
- **Unavailable:** collection was attempted but data was not accessible due
  to authorization, licensing, or service limitations.
- **Unsupported:** the source or current implementation does not support
  collecting this data.
- **Not requested / not collected (where relevant):** property or
  relationship was not included in the collection scope for this
  assessment.
- **Collection/error condition (where relevant):** collection was attempted
  but an error prevented retrieval (failed), or conflicting observations
  require explicit conflict metadata rather than silent overwrite.
- **Not applicable (where relevant):** the concept does not apply to this
  identity kind (e.g., delegated-grant semantics for a kind that does not
  support delegated flows).

Rules for this section:

1. These are **DATA/OBSERVATION semantics, not PASS/FAIL states.** Do NOT
   create alternative authoritative assessment states. The canonical
   evaluation states remain exactly PASS, FAIL, NOT_EVALUATED,
   NOT_APPLICABLE, ERROR (see `core-contracts.md` §4, which this document
   agrees with without redefinition).
2. Missing or unavailable data MUST NOT automatically imply FAIL (INV-05;
   CAP-002). Whether verified-complete absence yields PASS or
   NOT_EVALUATED is determined per rule by its explicit completeness
   semantics — never by a universal absence rule.
3. Exact implementation representation (enum names, serialization, schema)
   is TBD, coordinated with `capability-model.md` (state names, hierarchy,
   granularity, propagation) and `error-result-model.md` (failure-category
   taxonomy, NOT_EVALUATED-vs-ERROR mapping). This document MUST NOT
   redefine capability-state names and MUST NOT invent licensing behavior.

---

## 11. Provenance concepts

Enough normalized provenance vocabulary to identify where a fact
originated, without storing credentials or provider secrets (INV-11;
domain-model and findings-evidence provenance models):

- **Per-observation provenance content (conceptual):** source system (API
  family); source object reference; collection operation/context;
  assessment context; observation-time context where appropriate;
  transformation/derivation lineage for normalized or derived facts
  (derivation path plus source observations).
- **Observed / normalized / derived / evaluation distinction** MUST be
  preserved: collectors produce observations; normalization produces
  canonical representations without introducing undocumented properties;
  deterministic logic produces derived values labeled as derived;
  evaluation data is produced after normalization and graph construction
  are complete. Downstream consumers MUST be able to distinguish these
  categories.
- **Restrictions:** provenance MUST NOT store authentication tokens, raw
  secrets, unnecessary raw payloads, or secret-bearing configuration.
  Unknown or unavailable provenance is represented explicitly, never
  invented. Provenance is not evidence by itself — which provenance-backed
  observations become rule evidence is determined downstream per
  `findings-evidence-schema.md`.
- **Deferral.** Detailed evidence schema, provenance schema/serialization,
  identifier formats, persistence/retention posture, and integrity
  mechanisms (hashing/signing/timestamping) remain owned by
  `findings-evidence-schema.md` (§19).

---

## 12. Time semantics

Principles for UTC/offset handling, provider timestamps, missing
timestamps, and deterministic evaluation:

1. **No wall-clock inside evaluation.** Current time MUST NOT be read
   arbitrarily inside rule logic. If time-dependent policy is required, an
   explicit assessment-time reference is supplied as deterministic input
   and recorded in assessment context and evidence (rule-engine
   architecture §9.3; `core-contracts.md` §3.11). Assessment-time
   representation detail belongs to `rule-engine-contracts.md`.
2. **Provider timestamps are observations, not authority.** Validity/
   lifecycle boundaries (start/expiry) and creation timestamps are carried
   as observed values with provenance; their presence, absence, precision,
   and offset semantics are whatever the documented source exposes — no
   normalization-invented precision, no inferred rotation, no fabricated
   defaults.
3. **UTC/offset handling.** Where provider timestamps carry offsets, the
   offset is preserved as observed; comparison semantics for evaluation
   are deterministic and explicit (owned in detail by
   `rule-engine-contracts.md` time handling). This document selects no
   date/time library, format, or parsing implementation.
4. **Missing timestamps.** Absent start/expiry/creation timestamps are
   represented through §10 semantics (not collected / unavailable /
   unsupported / not applicable as appropriate) and flow into
   capability-aware rule behavior — never silently defaulted, never
   inferred. Do NOT invent unavailable timestamps.
5. **Collection-window context.** Observations carry collection/observation
   time context sufficient to express assessment-window semantics and to
   keep material inconsistencies diagnosable (collection architecture §9).
   Transactional-snapshot semantics MUST NOT be claimed unless the source
   provides them.

---

## 13. Extensibility/versioning

Allow future identity categories and providers without requiring rules to
consume provider SDK models:

1. **Kind/relationship/category extension** is additive and
   capability-gated: new `IdentityKind` values, relationship kinds,
   credential categories, permission categories, or accountability
   categories enter with documented evidence basis, explicit capability
   state, and provenance preservation. Unsupported environments report
   NOT_EVALUATED/NOT_APPLICABLE per rule semantics; existing states are
   never silently reinterpreted.
2. **Provider independence is structural, not a support claim.** The model
   is provider-independent by construction (INV-04): source system is a
   provenance value, translation happens at the normalization boundary,
   and rules consume only normalized contracts. Do NOT claim
   multi-provider support exists in V1 merely because the model is
   provider-independent. V1 assesses the approved documented sources
   only; any additional source family enters as an isolated, explicitly
   enabled, separately provenanced capability through architecture change
   control.
3. **Versioning discipline** follows `core-contracts.md` §13: additive
   preference, stable identities, version-aware evidence, no silent
   semantic change. Exact enum naming, schema versioning, and
   compatibility policy are TBD (§19) and MUST NOT promise a public stable
   API before V1.

---

## 14. Equality/identity semantics

Conceptual equality boundaries — carefully, especially across tenant plus
provider/native identifiers. No final C# equality implementation is
invented here.

1. **Assessment-local key equality.** Within one assessment, the normalized
   identity key is the equality basis for deduplication, graph node
   identity, and rule-target references. Duplicate source observations for
   the same source object MUST normalize deterministically to a single
   canonical record (deduplication algorithm TBD,
   `identity-graph-contracts.md`).
2. **Display names never establish equality.** Display names are
   human-readable metadata for diagnostic/output purposes only. They MUST
   NOT be used to match, merge, deduplicate, or establish equality between
   records or nodes (INV-G2).
3. **Tenant + source scope.** Equality is valid only within the same tenant
   assessment context. Records from different tenant contexts MUST NEVER
   compare equal or merge silently, even if provider/native identifiers
   coincide textually. Cross-tenant comparison is an integrity failure,
   not a deduplication opportunity (INV-10; INV-G1).
4. **Source-reference vs normalized-key distinction.** Source references
   identify external objects for provenance; the normalized key identifies
   the assessment-local record. Two records MAY share an application-level
   linkage identifier (e.g., application linkage, §4) while remaining
   distinct records (application registration vs service principal are
   never merged). Conflicting observations for the same source object MUST
   remain detectable — silently overwritten or heuristically resolved
   conflicts are prohibited (INV-G5); conflict surfacing mechanics belong
   to `identity-graph-contracts.md` with `error-result-model.md`.
5. **Deferral.** Final equality implementation (comparers, hash codes,
   operators), key generation strategy/format, and fingerprint/dedup
   strategy belong to `identity-graph-contracts.md` (graph keys, dedup)
   and `findings-evidence-schema.md` (finding fingerprint), coordinated in
   §19. This document fixes only the conceptual boundaries above.

---

## 15. Serialization boundary

Domain types MUST NOT be designed around JSON/SARIF/HTML presentation
requirements:

- The normalized domain is a semantic model, not a wire format. Field
  selection follows documented-source support and requirement need — never
  renderer convenience, never schema compactness for a single format.
- No serialization schema, format choice, version header, or
  renderer-specific encoding concern appears in the domain contracts.
  Output schemas belong to later output design
  (`output-renderer-contracts.md`); provenance/evidence serialization
  belongs to `findings-evidence-schema.md`; envelope serialization belongs
  to `collector-contracts.md`.
- The domain MUST NOT pre-encode tenant/provider-originated strings for
  any single format. Injection defense (HTML escaping, terminal/control-
  character defense, JSON correctness, SARIF field safety, path safety) is
  a renderer obligation over opaque string data, not a domain
  transformation.

---

## 16. Security/privacy implications

Stated as structural support, not as implemented controls:

- **Read-only domain (INV-01; FR-040; CON-001).** The domain defines no
  mutating operation. Assessment artifacts in the execution environment
  are not tenant mutation; no domain concept authorizes provider writes.
- **Secret exclusion (INV-09; SEC-001; SEC-002; OUT-006).** No domain
  record, relationship, provenance entry, diagnostic, or identifier
  carries secret values, private-key material, tokens, passwords, or
  recovery codes. Credential metadata is non-secret by construction (§7).
- **Least privilege alignment (INV-08).** The domain's per-category
  capability references let collection request only minimum source access
  for enabled capabilities; optional categories MUST NOT silently broaden
  requirements (rationale owned by `authentication-design.md`).
- **Tenant isolation (INV-10).** Tenant-scoped keys and references (§4)
  plus equality boundaries (§14) make cross-tenant mixing structurally
  representable only as an integrity failure.
- **Provenance without exposure (INV-11).** Provenance identifies origin
  and collection context without raw payloads or secrets (§11); data
  minimization applies (references over duplication).
- **Determinism and failure transparency (INV-02; INV-14).** Immutable
  values, explicit absence semantics (§10), and time principles (§12) keep
  evaluation reproducible and failures explicit — never silent PASS.
- **Output/AI non-authority (INV-12; INV-13).** The domain is
  output-neutral (§15) and admits no model-output path; severity flows
  downstream without altering evaluation logic.
- **Sensitive enterprise data awareness.** Even without secrets, domain
  records may reveal identity structure, privilege relationships,
  accountability gaps, credential lifecycle posture, and permission
  exposure. Minimization, explicit output destinations, no undisclosed
  telemetry, safe logging, and controlled persistence (all owned
  downstream) apply — findings data MUST NOT be treated as harmless merely
  because secrets are excluded.
- **Undocumented-behavior resistance (INV-15; CAP-005).** §§3–9 invent no
  endpoint, permission, property, mapping, or licensing claim; every such
  detail is a named TBD (§19).

---

## 17. Testability implications

Domain support for testing (scenario catalog itself belongs to
`testing-seams.md`; no tests are created here):

- **Synthetic-fixture construction.** Immutable value orientation plus
  explicit absence semantics let tests build any domain state — present,
  empty, unavailable, unsupported, conflicting, cross-kind linkages —
  without a live tenant, production credentials, or network access.
- **Determinism verification.** Same normalized inputs plus capability
  state plus rule version/configuration plus assessment-time reference
  reproduce the same semantic outcome; time principles (§12) make
  time-dependent policy testable without wall-clock dependence.
- **False-PASS/false-FAIL defense.** Per-category capability references
  and absence-vs-unavailability discipline give direct seams for
  "nothing found with incomplete collection MUST NOT be PASS" and
  "missing data MUST NOT be automatic FAIL" scenarios.
- **Isolation and integrity scenarios.** Tenant-scoped equality (§14),
  display-name non-equality, unknown-classification handling (§5), and
  conflict detectability give seams for tenant-isolation,
  adversarial-merging, and failure-injection coverage.
- **Secret-exclusion verification.** The absence of any secret-bearing
  field gives a statically assertable property: synthetic "secret-like"
  material MUST have no domain path to traverse.
- **Injection posture.** Opaque untrusted strings (§15) give renderer-test
  seams (HTML/XSS, ANSI/control, CR/LF, bidi, malformed Unicode) without
  domain-level encoding behavior to test.

---

## 18. Explicit non-goals

The following are explicitly NOT part of this domain vocabulary (each
traces to requirements non-goals or architecture out-of-scope):

1. No provider SDK binding, endpoint catalog, permission/scope list,
   property mapping, Agent ID mapping, or licensing-behavior claim.
2. No generic "approver" accountability semantics and no invented
   relationship, subtype mapping, consent-state machine, or role
   hierarchy beyond approved scope.
3. No secret values, private-key material, rotation inference, recoverable
   credential data, or unavailable-timestamp invention.
4. No alternative authoritative assessment states beyond the canonical
   five; no severity/confidence/risk-score modeling in the domain.
5. No multi-provider support claim for V1; no SaaS/multi-tenant service
   modeling; no cross-tenant correlation vocabulary.
6. No remediation, credential lifecycle operations, permission/policy
   modification, or identity create/delete/disable vocabulary (NG-001;
   NG-002; NG-007; SEC-003–SEC-007).
7. No serialization schema, output formatting, CLI syntax, SARIF mapping,
   HTML templating, signing/hashing implementation, or persistence/
   database design.
8. No concrete security rule content and no scoring mathematics.
9. No final C# equality implementation, comparers, hash codes, or
   operators.

---

## 19. Open TBDs and owner documents

Each TBD states what is unknown, why it cannot be resolved here, and which
later document owns its resolution. None is resolved by speculation.

| # | TBD | Why unresolved here | Owned by |
| --- | --- | --- | --- |
| T-01 | Concrete C# record/class shapes and member lists for `IdentityRecord`, `IdentityKind`, credential metadata, permission/access, accountability, tenant context, identifiers, provenance references. | Member-level design requires coordination with capability, graph, rule, findings, collector, and output owners; choosing members here would preempt them. | This document for domain semantics, coordinated with `core-contracts.md`; physical shapes per consumer in their owning documents |
| T-02 | Enum/contract names for `IdentityKind` values (including unknown/unclassified representation), credential categories, permission categories, relationship kinds, absence-semantics representation, and serialization of all of the above. | Naming/serialization choices need cross-document agreement (domain, capability, graph, findings, output) and MUST NOT be fixed unilaterally here. | This document (vocabulary) with `capability-model.md` (state names), `identity-graph-contracts.md` (relationship/edge taxonomy), `findings-evidence-schema.md` (serialization) |
| T-03 | Capability-state names, hierarchy, per-category granularity, and propagation shape as consumed by domain records. | Capability semantics are a dedicated design surface; duplicating names here would create a second source of truth. | `capability-model.md` (this document references only) |
| T-04 | Graph node/edge shapes, query/traversal/projection API, INV-G1–INV-G8 enforcement mechanics, graph serialization, size/memory policy. | Graph operations are a dedicated contract surface over this vocabulary. | `identity-graph-contracts.md` |
| T-05 | Assessment-local key format and generation strategy; tenant-context identifier format; source-reference format; dedup algorithm; conflict-resolution policy. | Formats and algorithms need dedicated design plus structural-test enforcement; no format is chosen here. | `identity-graph-contracts.md` (keys, dedup, conflict) with `findings-evidence-schema.md` (identifier formats) and `domain-types.md` semantics (this document) |
| T-06 | `RuleDefinition`/`RuleEvaluation` schemas, applicability/capability/input validation order, assessment-time representation, ordering/parallelism, severity taxonomy, configuration format. | Rule-engine internals are a dedicated contract surface consuming this vocabulary. | `rule-engine-contracts.md` |
| T-07 | Exact provider property/value mappings for each domain field — including managed-identity classification mapping, agent-identity mapping, credential-field mapping, accountability-relationship mapping, permission-category mapping — plus exact endpoints and permission rationale. All require validation against published documentation and MUST NOT be invented. | Undocumented provider semantics cannot be resolved by design speculation (INV-15; CAP-005). | `collector-contracts.md` (endpoints/properties/mappings), `authentication-design.md` (permission rationale), `capability-model.md` (granularity) |
| T-08 | Evidence/provenance/diagnostic record schemas, emission policy, fingerprint strategy, redaction policy, hashing/signing/timestamping posture, retention/persistence posture. | Integrity and persistence mechanisms need separate security review. | `findings-evidence-schema.md` |
| T-09 | Shared error/result shape, failure-category taxonomy, NOT_EVALUATED-vs-ERROR mapping, sanitization rules. | Error taxonomy spans all pipeline stages, not the domain alone. | `error-result-model.md` |
| T-10 | Collector interfaces, source-observation envelope schema, pagination/retry/cancellation constants, caching strategy, raw-response diagnostic retention. | Collector mechanics are a dedicated surface; no constant is chosen here. | `collector-contracts.md` |
| T-11 | Canonical output projection shape, per-format schemas, injection/file-safety policies, exit-code mapping. | Presentation design belongs downstream of the domain. | `output-renderer-contracts.md` |
| T-12 | Performance and memory limits for normalized domain construction; large-tenant bounding policy. | Limits need measurement-backed implementation design, not speculation. | `collector-contracts.md` with `identity-graph-contracts.md` and `implementation-sequence.md` (staging) |
| T-13 | Formal schema-validation approach for normalized contracts. | Validation tooling is a build/test-seam decision. | `testing-seams.md` with `dependency-boundaries.md` |

---

## 20. Acceptance criteria

This domain vocabulary is accepted when:

1. **Category coverage.** Application registrations, service principals,
   managed identities (through documented Entra concepts with preserved
   provenance), and supported agent identities (capability-gated) are all
   representable per §3 without invented mappings; app registrations and
   service principals remain distinguishable.
2. **Identifier discipline.** §4 distinguishes normalized key, tenant
   context, source references, and application linkage; per-kind
   identifier absence is representable via §10; display names are never
   keys; no secret is carried.
3. **Classification extensibility.** §5 provides an extensible mechanism
   with no provider-string logic in rules and explicit unknown-handling
   without false interpretation.
4. **Accountability/credential/permission fidelity.** §§6–8 represent only
   documented, non-secret, applicable concepts with absence-vs-
   unavailability discipline, declaration-vs-grant distinction, and no
   invented semantics.
5. **Relationship integrity.** §9 relationships are explicit, typed,
   tenant-scoped, provenance-aware, and extensible, with observed-vs-
   derived discipline and no undocumented relationships.
6. **Absence explicitness.** §10 distinguishes observed, confirmed empty,
   unavailable, unsupported, not requested/not collected, error/conflict,
   and not applicable — without creating alternative assessment states.
7. **Provenance sufficiency.** §11 identifies fact origin without secrets
   and preserves observed/normalized/derived/evaluation distinction.
8. **Time determinism.** §12 admits no wall-clock reads in evaluation, no
   invented timestamps, and collection-window (not snapshot) semantics.
9. **Evolution safety.** §13 admits new kinds/providers structurally
   without claiming V1 multi-provider support and without rule SDK
   coupling.
10. **Equality soundness.** §14 fixes key-based, tenant-scoped equality
    with display-name exclusion and conflict detectability, without a
    final code implementation.
11. **Serialization neutrality.** §15 designs no domain concept around
    presentation requirements.
12. **TBD explicitness.** Every implementation-sensitive unknown is listed
    in §19 with its owning document; no endpoint, permission, property,
    mapping, package, or numeric limit is invented.
13. **Non-goal containment.** Nothing in §18 appears as an assumed domain
    capability.
14. **Cross-document agreement.** The consistency contract with
    `core-contracts.md` holds: identical five states (referenced, not
    redefined), tenant scoping, shared normalized vocabulary, provider
    isolation, capability awareness, evidence/provenance by reference,
    secret exclusion, determinism, failure transparency, renderer
    non-authority, AI non-authority — with detailed schemas deferred to
    their owners via explicit cross-references.

---

*(End of file)*
