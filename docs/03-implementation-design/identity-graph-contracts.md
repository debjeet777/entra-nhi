# Identity Graph Contracts

> **Phase:** 0.3.7
> **Package:** Implementation Design
> **Status:** Proposed — design only, no code or project files created
> **Date:** 2026-09-18

---

## 1. Purpose and scope

- **Status:** Proposed conceptual graph contracts. This document is design
  output only. It authorizes no implementation.
- **Scope:** Defines the provider-independent normalized identity graph
  contract: what the graph is, what feeds it, what it contains (nodes,
  edges, provenance, capability context), how it is queried, how it fails,
  and where its authority ends. Derived from the approved identity-graph
  architecture (`docs/02-architecture/identity-graph.md`), domain model
  (`docs/02-architecture/domain-model.md`), invariants (INV-01–INV-16,
  INV-G1–INV-G8), trust boundaries, threat model, and testing
  architecture, as structured by `solution-structure.md` §§4–8 and governed
  by `README.md` §§4–6.
- **What this document is:** The owner of the graph contract surface:
  node/edge shapes (conceptual), observed-vs-derived classification,
  traversal/projection operations (conceptual), and integrity rules
  INV-G1–INV-G8 as contracted for implementation (`README.md` §5–§6).
  First normative definition of graph construction/traversal semantics
  lives here; all other documents use it referentially.
- **What this document is not:** It is not a C# implementation, a graph
  database or storage selection, a graph-library selection, a query-language
  selection, an endpoint catalog, a permission/scope list, a property
  mapping, an Agent ID mapping, a rule catalog, a finding/evidence schema,
  or a numeric resource-limit set. Detailed consumers live elsewhere:
  normalized domain shapes in `domain-types.md`, capability states in
  `capability-model.md`, rule pipeline and `RuleEvaluation` in
  `rule-engine-contracts.md`, findings/evidence/provenance schemas in
  `findings-evidence-schema.md`, error/result shapes in
  `error-result-model.md`, collector envelopes in `collector-contracts.md`,
  output projections in `output-renderer-contracts.md`, auth boundaries in
  `authentication-design.md`, and the shared core surface in
  `core-contracts.md` (which references — never duplicates — the contracts
  defined here).
- **Language discipline:** Per `README.md` §1, this document distinguishes
  (a) **approved architectural requirements** traced to requirement/
  invariant IDs, (b) **proposed conceptual contracts** owned by this
  document, and (c) **unresolved TBDs** naming the owning later document
  (§29). Nothing herein claims a control is implemented, verified, or
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
  created here. Graph, capability, provenance, evidence, operational, and
  error terms used below are explicitly typed in their correct layer per
  `capability-model.md`, `domain-types.md`, and `error-result-model.md` —
  never as assessment states.

---

## 2. Graph ownership and architectural boundary

1. The identity graph is a **deterministic projection of the normalized
   domain model** (identity-graph architecture, "Projection, not source").
   It is owned conceptually by the `Graph` module inside `EntraNHI.Core`
   (`solution-structure.md` §7) and contracted here.
2. The graph is NOT a second source of truth. It MUST NOT be constructed
   from raw provider data, MUST NOT introduce facts absent from the
   normalized domain model, MUST NOT mutate after construction during a
   single assessment run, and MUST be discardable after assessment (no
   persistent storage required) — carried forward from the identity-graph
   architecture.
3. Authority boundaries (normative):
   - The graph owns: node/edge construction from normalized inputs,
     observed-vs-derived classification, deterministic traversal/
     projection operations, and INV-G1–INV-G8 enforcement at
     construction/query time.
   - The graph does NOT own: assessment verdicts (rule engine,
     `rule-engine-contracts.md`), findings/evidence records
     (`findings-evidence-schema.md`), rendering
     (`output-renderer-contracts.md`), authentication
     (`authentication-design.md`), remediation or any tenant mutation
     (INV-01; no such operation exists in V1), or collection itself
     (`collector-contracts.md`).
4. Placement in the pipeline (conceptual): normalization → graph
   construction → rule evaluation → findings construction → output
   projection. The graph sits strictly between normalization and rule
   evaluation; it never bypasses either.

---

## 3. Graph inputs

Conceptual inputs to graph construction (by reference, not a member list):

1. **Normalized domain records** — `IdentityRecord`, credential metadata,
   permission/access relationships, accountability subjects, and their
   provenance/capability references, as defined by `domain-types.md`
   (§§3–11). These are the ONLY fact inputs.
2. **Normalized relationship observations** — typed relationship facts
   carried by normalized records, per the vocabulary in `domain-types.md`
   §9 and the V1 categories in the identity-graph architecture.
3. **Capability/observation states** — per-category (and per-subject where
   meaningful) capability states per `capability-model.md` §5, so the
   graph records what could be observed, not only what was observed.
4. **Tenant assessment context** — the single-tenant scope binding every
   input (`core-contracts.md` §5). Inputs from more than one tenant
   context MUST NOT be accepted as one graph (§12).
5. **Deterministic construction context** — assessment/configuration/
   version references sufficient for reproducibility scoping, plus an
   explicit operation context carrying cancellation intent and
   bounding/time-reference policy where long-running or bounded
   construction applies (`core-contracts.md` §2, rule 12). No wall-clock
   reads inside construction semantics; observation timestamps are data.

Explicitly NOT inputs: provider SDK objects, raw provider payloads, HTTP
semantics, undocumented properties, tokens/secrets/credential material,
model-generated facts, live provider handles, renderer state, or ambient
provider/clock state.

```csharp
// Illustrative only. Conceptual and non-production. Not a member list.
record GraphConstructionInput {
  IReadOnlyList<NormalizedRecordRef> Records;      // domain-types.md shapes
  IReadOnlyList<RelationshipObservationRef> Relationships;
  IReadOnlyList<CapabilityObservationRef> Capabilities; // capability-model.md
  TenantAssessmentContext Tenant; OperationContext Operation;
}
```

---

## 4. Node model

Conceptual node content (carried forward from the identity-graph
architecture "Node definition"; field mechanics deferred to §29):

| Concept | Meaning |
| --- | --- |
| **Graph key** | Stable assessment-local identifier for this node within the graph, derived from the normalized identity key of the referenced domain entity (§5). |
| **Node kind** | The normalized category of entity this node represents (§6). |
| **Domain entity reference** | Reference to the underlying normalized domain entity (identity record, credential metadata, permission relationship, accountability subject, or other supported entity). The graph stores references, not duplicated provider facts. |
| **Provenance references** | One or more provenance records identifying the source and collection context for this node (§9; `domain-types.md` §11). |
| **Capability state** | The capability/collection state for the data supporting this node's existence (observed, gapped, failed, unknown — per `capability-model.md` §5). |
| **Display metadata** | Human-readable metadata for diagnostic/output purposes only. NEVER used for identity matching or equality (INV-G2). |

Rules:

1. Every node MUST trace to at least one normalized domain entity or an
   explicitly marked unresolved placeholder (§16). Nodes with no traceable
   basis are prohibited (fabrication defense).
2. Nodes MUST NOT carry secret values, tokens, private-key material,
   passwords, recovery codes, authorization headers, credential-bearing
   URLs, or secret-bearing configuration (§25).
3. Nodes MUST NOT carry provider SDK objects or raw payloads; only
   normalized references cross the boundary (INV-03; INV-04).
4. Node content is immutable after construction for the run (§2).

---

## 5. Stable node identity

1. **Graph key derivation.** Each node's graph key is derived from the
   normalized identity key of its referenced domain entity
   (`domain-types.md` §4) scoped to the assessment/tenant context. The
   key is stable within the assessment: the same normalized input always
   yields the same key (INV-02).
2. **Tenant scoping.** Stable identity/equality includes tenant context
   where required by the existing domain model (`domain-types.md` §14).
   Graph keys are valid only within their tenant assessment context;
   keys from different tenant contexts MUST NEVER compare equal or merge
   (§12; INV-10; INV-G1).
3. **Display-name exclusion.** Display names MUST NOT contribute to key
   generation, equality, deduplication, or merging (INV-G2).
4. **Undocumented-identifier exclusion.** Keys MUST NOT be derived from
   concatenating undocumented identifiers or from guessed join values
   (`collector-contracts.md` §14).
5. **Deferral.** Exact key format, generation strategy, comparer/hash-code
   mechanics, and serialization are TBD (§29, T-01/T-02). This document
   fixes only the conceptual boundaries above.

---

## 6. Node categories / identity kinds

1. V1 node kinds are carried forward from the identity-graph architecture
   "Node kinds": `ApplicationRegistration`, `ServicePrincipal`,
   `ManagedIdentity`, `AgentIdentity`, `CredentialMetadata`,
   `PermissionRelationship`, `AccountabilitySubject` — each bound to its
   domain entity (`domain-types.md` §§3–8).
2. Application registrations and service principals MUST remain
   distinguishable nodes; they are never merged even when linked
   (`domain-types.md` §§3–5, §14).
3. `ManagedIdentity` nodes preserve underlying service-principal
   provenance; the classified node MUST NOT pretend to be an unrelated
   provider object (`domain-types.md` §3).
4. `AgentIdentity` nodes exist only where documented observable capability
   supports them; unsupported/unavailable agent semantics remain explicit
   capability gaps and MUST NOT block unrelated graph content
   (`domain-types.md` §§3, 5; `capability-model.md` C-06).
5. Unknown/unclassified identities MUST be representable without false
   interpretation (explicit unknown/unclassified handling per
   `domain-types.md` §5), never silently coerced into a supported kind.
6. New kinds enter additively with documented evidence basis, capability
   gating, and provenance preservation (`domain-types.md` §13). No new
   kind is introduced here.
7. Exact enum/contract naming and serialization are TBD (§29, T-02).

---

## 7. Edge model

Conceptual edge content (carried forward from the identity-graph
architecture "Edge definition"; field mechanics deferred to §29):

| Concept | Meaning |
| --- | --- |
| **Source node** | The graph node from which this edge originates (§10). |
| **Target node** | The graph node to which this edge points (§10). |
| **Relationship kind** | The normalized category of relationship (§8). |
| **Derivation classification** | Whether this edge was **observed** (directly returned by a documented source) or **derived** (deterministically computed from observed facts by an explicit rule) — INV-G6. |
| **Provenance** | Source reference and collection context for this edge; for derived edges, the derivation basis plus source observations (§9). |
| **Capability/observation state** | The capability state for this edge — whether the relationship was fully observed, partially observed, or limited (per `capability-model.md` §5). |

Rules:

1. Every edge MUST have either observed source support (directly returned
   by a documented source) or an explicitly identified deterministic
   derivation basis over supported observations. Edges on any other basis
   are prohibited.
2. Relationships not supported by source data MUST NOT be present.
3. Edges MUST NOT carry secrets, tokens, or provider SDK references (§25).
4. Edges are immutable after construction for the run (§2).

```csharp
// Illustrative only. Conceptual and non-production. Not a member list.
record GraphEdge {
  GraphNodeRef Source; GraphNodeRef Target; RelationshipKindRef Kind;
  DerivationClass Derivation; // Observed | Derived (conceptual)
  ProvenanceRef Provenance; CapabilityStateRef Capability;
}
```

---

## 8. Relationship semantics

1. V1 relationship categories are carried forward from the identity-graph
   architecture "V1 relationship categories" (identity-link relationships
   in both documented directions; managed-identity classification
   derivation lineage; owner/sponsor/manager accountability; credential
   associations; permission associations) — **referenced, not redefined**.
   This document MUST NOT restate them normatively beyond that reference
   (`README.md` §6 decision ownership is shared with `domain-types.md`
   §9; this document contracts construction/query behavior over that
   vocabulary).
2. NOT every relationship category applies to every identity kind. The
   graph MUST NOT assert a relationship merely because it is plausible.
3. No undocumented relationship is assumed. No relationship is invented
   here: no endpoint, property, permission/scope, SDK-method, or Agent ID
   mapping is stated or implied. Agent-identity relationships beyond the
   approved vocabulary MUST be introduced only after collector design
   validates documented semantics; unsupported/unavailable agent
   capability remains explicit.
4. Derived edges MUST be produced by deterministic logic, traceable to
   source facts plus the derivation basis, and clearly labeled as derived
   in diagnostic/debug/audit output (INV-G6).
5. Display-name matching MUST NOT establish relationships; AI-inferred
   relationships are prohibited (INV-02 consequences).
6. Exact relationship-kind naming/serialization is TBD (§29, T-02).

---

## 9. Relationship provenance

1. Every edge carries provenance: source system (source family as a
   normalized value, never SDK types), source object reference
   (documented, source-exposed identifiers only), collection
   operation/context, assessment/tenant context, observation-time context
   where appropriate, and derivation lineage for derived edges
   (derivation basis plus source observations) — per `domain-types.md`
   §11 and the identity-graph architecture edge definition.
2. Unknown or unavailable provenance MUST be represented explicitly,
   never invented.
3. Provenance MUST NOT store tokens, secrets, private-key material,
   passwords, recovery codes, authorization headers, credential-bearing
   URLs, secret-bearing configuration, or unnecessary raw payloads.
4. Provenance record shapes, serialization, identifier formats, and
   integrity mechanisms (hashing/signing/timestamping) remain owned by
   `findings-evidence-schema.md` (§29). This document requires only that
   graph edges hand downstream sufficient provenance inputs to those
   schemas (§22).

---

## 10. Directionality

1. Every edge has an explicit source node and target node (§7). Direction
   is part of edge identity: reversing source/target yields a different
   edge unless the relationship kind's documented semantics define the
   pair as mutual inverses with both directions explicitly constructed
   from observed support.
2. Where the approved vocabulary defines inverse pairs (e.g.,
   application-registration-to-service-principal linkage and its
   underlying-application inverse), each direction MUST have its own
   observed or derived basis; an inverse MUST NOT be assumed from a
   single direction without documented support.
3. Traversal operations (§20) MUST respect direction: following an edge
   against its direction without an explicit inverse edge is prohibited.
4. Directionality choices for future relationship kinds are TBDs resolved
   with their vocabulary introduction (§29).

---

## 11. Multiplicity/cardinality

1. The graph MUST NOT assume universal cardinality (e.g., "every
   application registration has exactly one service principal" or "every
   identity has an owner"). Cardinality is whatever the normalized
   observations establish under the rule's completeness semantics — zero,
   one, or many edges of a kind from a node are all representable.
2. Zero edges of a kind from a node carry NO standalone meaning; their
   interpretation is governed exclusively by §13–§15 (missing vs absent
   vs unsupported; partial construction). In particular, zero edges MUST
   NOT be read as "confirmed absent" without the completeness basis §14
   requires.
3. Cardinality expectations invented from undocumented provider behavior
   are prohibited (INV-15). Where documented source semantics define a
   cardinality constraint relied upon by construction (if any), that
   reliance MUST be explicit and traceable — no such reliance is asserted
   here.
4. Aggregation operations (§20) report observed cardinality with
   capability context; they MUST NOT silently fill expected-but-missing
   edges.

---

## 12. Tenant isolation

1. Each graph is scoped to a single tenant assessment context (INV-G1;
   INV-10). Every node and edge carries its tenant binding.
2. Cross-tenant nodes/edges MUST never be silently merged. Records from
   different tenant contexts MUST NEVER compare equal, deduplicate, or
   link — even if source identifiers coincide textually
   (`domain-types.md` §14).
3. Tenant-context mismatch or contamination (inputs carrying a different
   tenant binding than the graph under construction; an edge attempting
   to join nodes of different tenant contexts; evidence-correlation
   attempts across tenants) is an integrity failure
   (`error-result-model.md` §13): fail closed and visibly — never silent
   continuation, never conversion into `PASS`/`FAIL`, never relabeling as
   an ordinary data gap.
4. Tenant isolation (never mixing data across tenant contexts) is
   distinct from SaaS multi-tenancy (simultaneous multi-tenant service
   operation, a V1 non-goal per NG-003). Contracts MUST enforce the
   former and MUST NOT assume, require, or design the latter.
5. Do NOT assume SaaS hosting. V1 executes one tenant per run under an
   operator-controlled trusted local runtime.

---

## 13. Capability/completeness interaction

1. Every node and edge carries (or references) the capability/observation
   state for its supporting data (`capability-model.md` §5 as concepts:
   observed, not-requested, unavailable, unsupported, failed, unknown).
   Capability absence MUST NOT be indistinguishable from confirmed-empty
   observation (`core-contracts.md` §2, rule 5).
2. Capability state is sufficiently granular that one category's
   unavailability does not invalidate unrelated successfully collected
   graph content (per-category/per-subject granularity,
   `capability-model.md` §11).
3. The graph MUST NOT resolve capability gaps itself into assessment
   states; it preserves gap metadata so the rule engine can apply
   `capability-model.md` §9 transition semantics (scoped gaps →
   `NOT_EVALUATED`; trust-breaking failures → `ERROR`).
4. `Core` graph contracts MUST NOT encode provider-specific detection
   mechanics (no endpoint probing logic, no licensing-tier inference, no
   permission-name checks, no SDK calls). Detection lives in
   collectors/capability detection; the graph sees only the resulting
   explicit state plus provenance (`capability-model.md` §12).
5. Cross-document consistency (normative inequalities — graph face):

   | Inequality | Graph meaning |
   | --- | --- |
   | missing relationship != confirmed absent relationship | An absent edge is absence of evidence unless §14 completeness authorizes confirmed-absence (§14). |
   | partial graph != complete graph | A graph with gaps/failures carries explicit incompleteness; it MUST NOT present as complete (§15). |
   | unresolved reference != nonexistent identity | An unresolvable reference stays an explicit placeholder, never a deletion (§16). |
   | provider failure != rule FAIL | Collection/operational failures are preserved as gap/failure metadata for downstream mapping, never as tenant FAIL (§23; `error-result-model.md` §4). |
   | operational condition != sixth assessment state | Capability/observation/error conditions are graph metadata constraining evaluation, never verdicts. |

---

## 14. Missing vs absent vs unsupported relationships

Normative reading rules for edge absence (critical false-PASS control;
INV-G3; INV-05; INV-07; INV-14):

1. **Missing edge (no information).** No observation was collected for
   the relationship (not requested, unavailable, failed, unknown,
   partial). A missing edge MUST NOT automatically mean the relationship
   does not exist. Missing information MUST NOT become `PASS` downstream.
2. **Confirmed absent relationship.** The source was completely and
   successfully observed for the category AND explicitly indicates no
   value or an empty relationship where that distinction is documented
   and reliable AND the consuming rule's explicit completeness semantics
   authorize treating that emptiness as conclusive
   (`collector-contracts.md` §8; `capability-model.md` R-02/R-03).
   Absence is authoritative ONLY where collection completeness and source
   semantics jointly justify it. Only this case may ground downstream
   absence-dependent `PASS` reasoning, and only per the rule's documented
   completeness semantics — never by a universal absence rule
   (`core-contracts.md` §4, rule 2).
3. **Unsupported relationship.** The source or implementation does not
   support observing the relationship. Unsupported required relationships
   prevent authoritative `PASS`/`FAIL` downstream (`NOT_EVALUATED`
   normally; `NOT_APPLICABLE` only where a rule's applicability predicate
   genuinely excludes the subject).
4. Collectors MUST NOT hide gaps behind successful empty results;
   normalization MUST preserve per-category/per-subject gap metadata so
   graph construction observes the true shape
   (`capability-model.md` §10, rule 3).

---

## 15. Partial graph construction

1. When some required data is available and some is not, construction MAY
   preserve usable nodes/edges where correctness allows, but
   incompleteness MUST remain explicit: the graph carries which
   categories/subjects/scopes are gapped, with per-subset capability
   states — never a single success boolean
   (`error-result-model.md` §12; `capability-model.md` §§10–11).
2. A partial graph MUST NOT present as complete. Summaries, diagnostics,
   and downstream handoffs MUST preserve the incomplete scope visibly so
   rules apply conservative transition semantics (unmet required
   prerequisites → `NOT_EVALUATED`/`ERROR`, never `PASS`).
3. Mid-construction failure (including pagination-sequence failure,
   normalization failure for a subset, cancellation) MUST preserve
   successfully constructed content with provenance, record the failed
   scope with sanitized cause, and report partial — never "complete"
   (`collector-contracts.md` §§9, 11).
4. Conflicting observations MUST remain detectable as explicit conflict
   metadata with both observations and provenance retained (INV-G5),
   flowing into `NOT_EVALUATED` or `ERROR` per transition semantics
   unless a documented deterministic derivation provably resolves them.
   Silently overwriting, last-writer-wins merging, or heuristic
   resolution is prohibited.
5. The assessment MUST NOT automatically abort in full on every scoped
   gap unless correctness cannot be preserved; equally, usable partial
   graphs MUST NOT hide incompleteness.

---

## 16. Unknown/unresolved references

1. Relationship observations that reference identities not present in the
   normalized input set (dangling references) MUST remain explicit
   **unresolved-reference placeholders**: carrying the referencing edge
   context, the unresolvable reference value (safe identifiers only),
   provenance, capability state, and the reason resolution failed.
2. Unresolved references MUST NOT be silently dropped (which would fake
   completeness), silently resolved by display-name or heuristic matching
   (INV-G2), or treated as nonexistent identities. `unresolved reference
   != nonexistent identity` (§13).
3. Unknown/unclassified identity observations (recognized as identities
   but without sufficient documented classification evidence) are carried
   as explicitly unknown/unclassified nodes with capability/provenance
   context (`domain-types.md` §5), driving capability-aware rule behavior
   rather than silent coercion.
4. Where the true resolution cause cannot safely be established
   (indeterminate reference vs failed lookup vs unsupported kind), the
   placeholder records indeterminate cause rather than a guessed one.
5. Placeholder representation mechanics are TBD (§29, T-03).

---

## 17. Duplicate handling

1. Duplicate source observations for the same source object MUST
   normalize deterministically to a single canonical node (INV-G4). The
   deduplication algorithm is TBD but MUST be deterministic and
   documented (§29, T-04).
2. Equality for deduplication follows `domain-types.md` §14: normalized
   key equality within the same tenant assessment context; display names
   never establish equality (INV-G2); cross-tenant comparison never
   merges (§12).
3. Application registrations and service principals are never merged with
   each other even when linked (§6; `domain-types.md` §14).
4. Duplicate edges (same source, target, relationship kind, derivation
   basis) collapse deterministically to one edge preserving combined
   provenance; edges differing in derivation basis or provenance scope
   MUST NOT be silently collapsed where the difference is material to
   auditability.
5. Conflicting (not merely duplicate) observations are governed by §15,
   rule 4 — never by deduplication.

---

## 18. Deterministic normalization

1. Graph construction is a pure function of its §3 inputs: identical
   normalized inputs, capability state, rule version/configuration, and
   relevant deterministic execution context produce the identical graph
   structure (INV-02; identity-graph architecture "Deterministic
   construction").
2. Construction MUST NOT introduce AI-inferred or probabilistic
   relationships, silently repair missing data, fabricate edges from
   display-name matching, or assume undocumented relationships.
3. Equivalent provider observations plus equivalent
   configuration/context MUST normalize deterministically upstream
   (`collector-contracts.md` §16); the graph MUST NOT introduce
   ordering, timing, or randomness dependence of its own.
4. No randomness, wall-clock branching inside construction semantics,
   probabilistic handling, or model inference on the construction path.
   Observation timestamps are carried as data; they MUST NOT alter
   construction semantics non-deterministically.
5. Determinism applies downstream of the provider boundary: the provider
   is mutable external state and MAY return different content across
   runs; given the same returned content, the graph is deterministic.

---

## 19. Deterministic ordering

1. Provider/source return order MUST NOT accidentally change graph
   semantics: construction MUST impose a deterministic ordering (or
   order-independent processing) before key generation, deduplication,
   conflict detection, and edge materialization, so source-order
   variation cannot alter the resulting graph.
2. Traversal/query results (§20) MUST be emitted in a deterministic order
   for equivalent graph content (ordering mechanics TBD, §29, T-05), so
   rule evaluation and evidence referencing observe stable sequences.
3. Sorting/ordering MUST use stable keys (graph keys, relationship-kind
   order, deterministic tie-breakers) — never display names, never
   hash-code order, never wall-clock order, never parallelism-completion
   order.
4. Ordering mechanics add no semantic content: two traversals differing
   only in emission order over the same graph MUST be semantically
   equivalent inputs to deterministic consumers.

---

## 20. Query/traversal contract

Conceptual operations supported for rule evaluation (carried forward from
the identity-graph architecture "Graph operations"; API mechanics TBD,
§29, T-05):

| Operation | Contract |
| --- | --- |
| **Node lookup** | Find nodes by kind, graph key, or normalized properties. Lookup by display name as an identity key is prohibited (INV-G2). |
| **Edge traversal** | Follow edges from a source node to target nodes by relationship kind, respecting direction (§10) and carrying edge provenance/capability context with each step. |
| **Subgraph extraction** | Extract the subgraph relevant to a specific rule or identity, preserving node/edge provenance, capability states, unresolved placeholders (§16), and incompleteness markers (§15) — never a silently trimmed "clean" subgraph. |
| **Aggregation** | Count, list, or summarize nodes/edges matching criteria, reporting observed cardinality with capability context (§11, rule 4). |

Rules for all operations:

1. All operations MUST be deterministic and idempotent for equivalent
   graph content (INV-02).
2. Operations MUST NOT mutate the graph during evaluation (read-only
   queries; §21).
3. Operations MUST NOT become provider-aware: no provider calls, no
   privilege requests, no SDK types, no raw payloads, no undocumented
   semantics (INV-G7).
4. Operations MUST surface capability/gap/unknown/partial context
   alongside results so callers cannot mistake a gapped traversal for a
   confirmed-empty one (§§13–15).
5. Formal query-API definition, serialization for diagnostics, and
   subgraph-extraction API detail are TBD (§29, T-05/T-06).

```csharp
// Illustrative only. Conceptual and non-production. Not a member list.
record GraphQuery {
  GraphRef Graph; TraversalSpec Spec; // kinds, direction, depth bound (§24)
  // Returns: deterministic ordered node/edge sets + capability context.
}
```

---

## 21. Rule-engine consumption boundary

1. Rules MAY consume: normalized identities, normalized relationships
   (edges), graph projections (subgraphs, traversals, aggregations),
   capability states, provenance references, deterministic assessment
   configuration/version context — carried forward from the identity-graph
   architecture "Rules MAY consume".
2. Rules MUST NOT (carried forward from "Rules MUST NOT"): call provider
   APIs, request additional privileges, silently repair or infer missing
   data, use LLM/AI to complete missing relationships, infer undocumented
   semantics, treat collection failure/unavailable data/missing
   capability as confirmed secure state, or mutate the graph or domain
   model during evaluation (INV-G7; INV-03; INV-04; INV-13).
3. The graph exposes read-only query/projection operations (§20) to the
   engine; the engine MUST NOT hold construction-mutating handles.
4. Detailed pipeline stage contracts, `RuleDefinition`/`RuleEvaluation`
   schemas, and applicability/capability/input validation order belong to
   `rule-engine-contracts.md`. This document fixes only the consumption
   posture above.
5. Every rule evaluation MUST resolve to exactly one of `PASS`, `FAIL`,
   `NOT_EVALUATED`, `NOT_APPLICABLE`, or `ERROR` — owned by
   `core-contracts.md` §4, referenced here without redefinition.

---

## 22. Evidence/provenance handoff

1. The graph hands downstream everything findings/evidence construction
   needs to reference graph-derived facts: node/edge identifiers,
   relationship kinds, derivation classifications (observed vs derived),
   derivation bases for derived edges, provenance references, capability
   states, unresolved-placeholder records, and conflict metadata —
   sufficient as inputs to the schemas owned by
   `findings-evidence-schema.md`.
2. Evidence derived from graph relationships MUST remain traceable to
   normalized source observations/provenance where available: the chain
   normalized domain fact → graph edge (with derivation lineage) →
   evaluation evidence reference MUST preserve traceability at each
   transformation (INV-06; INV-11).
3. The graph MUST NOT fabricate certainty: derived edges carry their
   derivation basis and source facts; confidence-like claims beyond the
   deterministic derivation are prohibited.
4. Evidence/provenance/diagnostic record shapes, serialization,
   identifier formats, emission policy, and integrity posture remain
   owned by `findings-evidence-schema.md` (§29). This document requires
   only the handoff sufficiency above.

---

## 23. Error/failure behavior

1. Graph construction failure MUST be surfaced explicitly (INV-G8). A
   failed or partial graph MUST NOT produce false `PASS` outcomes.
   Downstream rules MUST NOT evaluate against an incomplete graph without
   explicit capability state indicating the limitation.
2. Failure classes (typed per `error-result-model.md` §§3–5; graph terms
   below are operational/capability concepts, never assessment states):
   - **Scoped gaps** (authorization-limited, licensing/service-limited,
     unsupported, not-collected, partial, conflicting observations):
     preserved as capability/gap metadata; affected rules yield
     `NOT_EVALUATED` (or `NOT_APPLICABLE` where applicability genuinely
     excludes the subject) per `capability-model.md` §9.
   - **Trust-breaking failures** (invalid normalized input violating a
     required invariant, inconsistent graph state preventing evaluation,
     rule-engine-visible construction exceptions, resource exhaustion,
     cancellation mid-construction): recorded with sanitized cause;
     affected scope yields `ERROR` (or assessment-level diagnostics
     where systemic) per `error-result-model.md` §4.
   - **Integrity violations** (tenant-context mismatch/contamination,
     cross-tenant edge attempts, fabricated-evidence attempts,
     corrupted shared state): fail closed and visibly
     (`error-result-model.md` §13) — never converted into
     `PASS`/`FAIL`, never relabeled as ordinary gaps.
3. No translated failure maps to tenant `FAIL`. No failure maps to
   `PASS`. `ERROR` MUST NOT be converted to `FAIL` for reporting
   convenience.
4. Cancellation during construction is recorded with scope (F-13 family),
   distinguishable from failure, timeout, and verdicts; downstream
   stages MUST NOT evaluate cancelled scope as complete
   (`error-result-model.md` §7).
5. Per-category failure mapping detail (each failure × stage →
   `NOT_EVALUATED` vs `ERROR`) is TBD with `error-result-model.md`
   (§29, T-07).

---

## 24. Resource/boundedness considerations

Design requirements for bounded construction (obligations here; exact
thresholds are TBDs — no numeric limit is chosen here unless already
approved elsewhere, of which there are none):

- Nodes/edges (max graph content per run/scope).
- Relationship expansion (max traversal/expansion depth and breadth).
- Query result sizes (max nodes/edges per lookup/traversal/aggregation).
- Memory accumulation (max in-memory graph accumulation before explicit
  exhaustion handling).
- Construction time (bounded construction with explicit timeout
  behavior; no constant chosen here).

Rules:

1. Exhaustion surfaces explicitly as an operational condition flowing
   into `NOT_EVALUATED` or `ERROR` per failure mapping — never silent
   `PASS`, never silent truncation presented as complete. Resource
   exhaustion/bounds MUST fail visibly rather than silently truncating
   into an apparently complete graph.
2. Bounds MUST be explicit, deterministic, and diagnosable (which bound,
   which scope, which consequence).
3. Exact thresholds, graph-size/memory policy, and performance
   characteristics are TBDs (§29, T-08) — concrete algorithms, graph
   libraries, persistence engines, and numerical limits remain TBD unless
   already authoritatively decided (none are).

---

## 25. Security requirements

Stated as structural support, not as implemented controls:

- **Read-only posture (INV-01).** The graph defines no mutating
  operation against tenant state; construction reads normalized inputs
  only.
- **Provider isolation / normalized boundary (INV-03; INV-04).** §3 input
  restrictions plus `core-contracts.md` §12 prohibitions keep provider
  types out of graph contracts; structural-test intent makes bypass
  visible.
- **False-PASS resistance (INV-05; INV-07; INV-14).** §§13–15 plus §23
  leave no path from ignorance, gaps, partial construction, or failure
  to `PASS`.
- **Secret exclusion (INV-09).** §§4, 7, 9 leave tokens/secrets no field
  or path into nodes, edges, projections, diagnostics, or handoffs. The
  graph MUST NOT contain credential secret values, tokens, or
  authentication material.
- **Least privilege alignment (INV-08).** The graph requests no privilege
  and performs no collection; per-category capability references let
  collection request only minimum source access.
- **Tenant isolation (INV-10).** §12 plus §5 scoping make cross-tenant
  mixing structurally representable only as an integrity failure.
- **Provenance without exposure (INV-11).** §9 identifies origin and
  collection context without raw payloads or secrets; minimization
  applies (references over duplication).
- **Determinism (INV-02).** §§18–19 keep construction and queries
  reproducible from preserved normalized inputs.
- **Output/AI non-authority (INV-12; INV-13).** The graph is
  output-neutral and admits no model-output path; no AI-inferred edges.
- **Undocumented-behavior resistance (INV-15).** No endpoint,
  permission, property, mapping, or licensing claim is made here; every
  such detail is a named TBD (§29).

---

## 26. Test seams

Seams needed (detailed testing design remains `testing-seams.md`; no
tests are created here; all fixtures synthetic, provider-free,
secret-free, network-free for core-adjacent tests):

- Synthetic normalized inputs covering every node kind (§6) and every
  approved relationship category (§8), plus unknown/unclassified kinds.
- Capability/gap fixtures for §§13–15: missing vs confirmed-absent vs
  unsupported vs failed vs unknown vs partial — including the false-PASS
  battery (gapped relationship MUST NOT read as absent; partial graph
  MUST NOT present as complete).
- Unresolved-reference fixtures (§16): dangling references stay explicit
  placeholders; display-name matching never resolves them.
- Duplicate/conflict fixtures (§§15, 17): deterministic dedup;
  conflicts preserved visibly, never silently overwritten.
- Tenant-isolation fixtures (§12): cross-context inputs/edges rejected
  as integrity failures, never merged.
- Determinism fixtures (§§18–19): identical inputs → identical graph;
  order-permuted inputs → identical graph; order-stable query emission.
- Failure-injection fixtures (§23): construction failure, cancellation
  mid-construction, resource-exhaustion signals — all visible, none
  yielding `PASS`.
- Secret-exclusion fixtures (§25): secret-shaped synthetic material has
  no path into nodes, edges, diagnostics, or handoffs.
- Consumer-boundary fixtures (§21): rule-shaped consumers can read but
  never mutate; provider-shaped access is unreachable from queries.

---

## 27. Architecture-invariant mapping

| Invariant | Graph contract support (constraint, not implementation claim) |
| --- | --- |
| INV-01 | No mutating operation in graph contracts (§25). |
| INV-02 | Deterministic construction + ordering + idempotent queries (§§18–20). |
| INV-03 | Provider isolation: normalized inputs only; no provider calls/types (§§3, 21). |
| INV-04 | Normalized domain boundary: translation lives upstream; graph projects normalized contracts (§§2–3). |
| INV-05 | Five states preserved by reference; no sixth state; gaps never coerced (§§13–14, 23). |
| INV-06 | Evidence/provenance handoff sufficiency; observed-vs-derived traceability (§§9, 22). |
| INV-07 | Capability awareness: per-category states preserved; gaps constrain evaluation (§13). |
| INV-08 | Least privilege: graph requests no privilege (§25). |
| INV-09 | Secret exclusion from all graph content (§§4, 7, 9, 25). |
| INV-10 | Tenant isolation: single-tenant graphs; mismatch fails closed (§§5, 12). |
| INV-11 | Provenance preservation across construction/query/handoff (§§9, 22). |
| INV-12 | Core/output separation: graph owns no rendering; handoff is output-neutral (§§2, 22). |
| INV-13 | AI non-authority: no inferred edges; no model path (§§18, 21, 25). |
| INV-14 | Failure transparency: explicit partial/failure behavior; never silent PASS (§§15, 23–24). |
| INV-15 | No undocumented dependency: no invented endpoints/properties/mappings/relationships (§8, §25). |
| INV-16 | Secure defaults: weakening (e.g., broader optional categories) requires explicit configuration downstream; graph adds no silent broadening. |
| INV-G1 | No cross-tenant edges (§12). |
| INV-G2 | Display names never establish identity (§§4–5, 8, 16–17). |
| INV-G3 | Unavailable-collection absence never reads as confirmed absence (§14). |
| INV-G4 | Deterministic deduplication (§17). |
| INV-G5 | Conflict detectability; no silent overwrite (§15). |
| INV-G6 | Observed-vs-derived distinguishability (§§7–9, 22). |
| INV-G7 | Rules consume normalized contracts only (§21). |
| INV-G8 | Explicit construction-failure behavior (§23). |

---

## 28. Non-goals

1. No graph-database, storage-engine, persistence, caching-technology, or
   graph-library selection.
2. No query-language, query-syntax, or traversal-API finalization (shapes
   contracted conceptually only).
3. No provider endpoint, permission/scope, property, SDK-call, or
   identifier-mapping selection.
4. No Agent ID mapping and no licensing-tier/commercial mapping.
5. No concrete security rule content and no scoring mathematics.
6. No severity/confidence/risk-score modeling (severity flows downstream
   without altering evaluation logic).
7. No sixth authoritative assessment state and no generic boolean
   "success" substitute for the five-state model.
8. No retry counts, delays, algorithms, timeout values, concurrency
   bounds, throttling thresholds, or numeric resource limits.
9. No signing/hashing implementation, no serialization-schema
   finalization, no output formatting, CLI syntax, or exit-code design.
10. No authentication-flow, credential-mechanism, token-cache, or storage
    selection.
11. No remediation, credential lifecycle operations, permission/policy
    modification, or identity create/delete/disable (NG-001; NG-002;
    NG-007; SEC-003–SEC-007).
12. No SaaS/multi-tenant service, dashboard, monitoring, AI verdicts, or
    other NG-series capabilities.

---

## 29. Explicit TBD register

Each TBD states what is unknown, why it cannot be resolved here, and
which later document owns its resolution. None is resolved by
speculation. Concrete algorithms, graph libraries, persistence engines,
and numerical limits remain TBD unless already authoritatively decided
(none are).

| # | TBD (decision required) | Why deferred | Owned by |
| --- | --- | --- | --- |
| T-01 | Concrete node/edge/query record shapes and member lists for §§4, 7, 20. | Member-level design needs coordination with domain, capability, rule, findings, and output owners; choosing members here would preempt them. | This document (semantics) coordinated with `core-contracts.md`; physical shapes per consumer in their owning documents; seam placement in `dependency-boundaries.md` |
| T-02 | Exact code-constant spelling/casing, enum/contract names for node kinds, relationship kinds, graph keys, derivation classes, and serialization of all of the above. | Naming/serialization choices need cross-document agreement and MUST NOT be fixed unilaterally here. | This document (vocabulary) with `domain-types.md` (kinds), `capability-model.md` (state names), `findings-evidence-schema.md` (serialization) |
| T-03 | Unresolved-reference placeholder representation mechanics (§16). | Representation needs findings/output coordination to stay traceable end to end. | This document with `findings-evidence-schema.md` |
| T-04 | Deterministic deduplication algorithm and conflict-resolution policy detail (§§15, 17). | Algorithm choice needs implementation-phase design plus documented-behavior review; a premature algorithm risks inventing semantics. | This document |
| T-05 | Formal graph query/traversal/projection API definition, subgraph-extraction API, and deterministic result-ordering mechanics (§§19–20). | API mechanics need rule-engine consumer coordination; ordering mechanics need consumer agreement. | This document with `rule-engine-contracts.md` |
| T-06 | Graph serialization format for debugging and diagnostics. | Format choice needs findings/output coordination; no format is approved elsewhere. | This document with `findings-evidence-schema.md` and `output-renderer-contracts.md` |
| T-07 | Per-category failure mapping detail: graph-side failures × stage → `NOT_EVALUATED` vs `ERROR` (with cancellation-scope mapping). | Mapping needs rule-engine, collector, and error-model coordination; a premature table risks inventing semantics. | This document with `rule-engine-contracts.md` and `collector-contracts.md`, constrained by `capability-model.md` §9 and `error-result-model.md` §4 |
| T-08 | Graph size/memory policy, traversal/expansion bounds, construction-time bounds, and performance characteristics for large-tenant construction (§24 numerics). | Limits need measurement-backed implementation design, not speculation. | This document with `collector-contracts.md` and `implementation-sequence.md` (staging) |
| T-09 | Managed-identity classification derivation detail and agent-identity relationship mapping validation (§§6, 8). | Requires validation against published documentation; MUST NOT be invented (INV-15). | `collector-contracts.md` (mappings), vocabulary with `domain-types.md` |
| T-10 | Identifier formats (assessment, graph-key, provenance, diagnostic, correlation references). | Formats need dedicated design plus structural-test enforcement. | `findings-evidence-schema.md` with `domain-types.md` |
| T-11 | Per-seam graph fakes/mocks/fixtures and failure-injection scenario catalog for §26. | Test-scenario design belongs to the testing surface. | `testing-seams.md` |
| T-12 | Assessment-time representation touching graph staleness/window semantics. | Time design belongs to rule-engine and domain time principles. | `rule-engine-contracts.md` (time principles from `domain-types.md` §12) |

---

## 30. Acceptance checklist

This graph contract is accepted when:

1. **Input purity.** §3 admits only normalized provider-independent
   observations plus capability/provenance/tenant/operation context; no
   provider SDK/API object enters Core graph contracts.
2. **Node soundness.** §§4–6 define node content, stable tenant-scoped
   identity, and V1 kinds with unknown-handling and no invented mappings.
3. **Edge soundness.** §§7–10 define edge content, relationship semantics
   over the approved vocabulary with no invented relationships, provenance
   carriage, and explicit directionality.
4. **Cardinality honesty.** §11 assumes no universal cardinality and lets
   zero edges carry no standalone meaning.
5. **Tenant soundness.** §§5, 12 scope keys/equality/edges to one tenant
   with cross-tenant merging prohibited as an integrity failure.
6. **Completeness discipline.** §§13–15 reconcile missing != confirmed
   absent, partial != complete, and provider failure != rule FAIL, with
   absence authoritative only where completeness + source semantics
   justify it.
7. **Reference explicitness.** §16 keeps unresolved references explicit
   (unresolved != nonexistent); §17 deduplicates deterministically
   without heuristic merging.
8. **Determinism.** §§18–19 fix deterministic construction and ordering
   with no randomness, wall-clock branching, or model inference.
9. **Query restraint.** §§20–21 contract read-only, idempotent,
   provider-unaware queries behind a non-mutating rule-consumption
   boundary that owns no verdicts, rendering, authentication, or
   remediation.
10. **Handoff sufficiency.** §22 supplies traceable graph-derived evidence
    inputs without fabricating certainty.
11. **Failure visibility.** §§23–24 surface construction failure, partial
    construction, cancellation, and resource exhaustion explicitly —
    never silent `PASS`, never silent truncation into apparent
    completeness.
12. **Five-state agreement.** Exactly `PASS`, `FAIL`, `NOT_EVALUATED`,
    `NOT_APPLICABLE`, `ERROR` are the only authoritative states;
    operational/capability/graph terms exist only in their correct typed
    layer — in full agreement with `core-contracts.md`,
    `domain-types.md`, `capability-model.md`, and `error-result-model.md`.
13. **TBD explicitness.** Every implementation-sensitive unknown is listed
    in §29 with its owning document; no endpoint, permission, property,
    mapping, package, mechanism, algorithm, library, engine, or numeric
    limit is invented or selected.
14. **Non-goal containment.** Nothing in §28 appears as an assumed
    capability.
15. **Maturity honesty.** Nothing herein claims graph construction,
    traversal, controls, or readiness is implemented or tested — these
    are intended implementation contracts only.

---

*(End of file)*
