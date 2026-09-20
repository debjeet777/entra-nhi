# Identity Graph

> **Status:** Phase 0.2.3
> **Date:** 2026-09-18

---

## Purpose

Define the identity graph as a deterministic projection of the normalized domain model. The graph provides a relational representation of collected identity data used by the rule engine to evaluate rules that depend on relationships between identities, credentials, permissions, and accountability subjects.

The graph is NOT a second source of truth. It is constructed entirely from the normalized domain model and retains provenance and capability state references for all observations. [INV-03, INV-04]

---

## Graph fundamentals

### Projection, not source

The identity graph is a projection. Every node and edge is derived from normalized domain model records. The graph:

- MUST NOT be constructed from raw provider data.
- MUST NOT introduce facts absent from the normalized domain model.
- MUST NOT mutate after construction during a single assessment run.
- MUST be discardable after assessment (no persistent storage required).

### Deterministic construction

Given identical normalized input data, capability state, and rule version/configuration, the graph structure MUST be identical across executions. [INV-02]

Graph construction MUST NOT:

- introduce AI-inferred or probabilistic relationships
- silently repair missing data
- fabricate edges from display name matching
- assume undocumented Microsoft relationships

---

## Graph nodes

Each node represents a normalized entity or subject from the domain model.

### Node definition

| Concept | Description |
| --- | --- |
| **Graph key** | Stable assessment-local identifier for this node within the graph. Derived from the normalized identity key of the referenced domain entity. |
| **Node kind** | The type of entity this node represents (see Node kinds below). |
| **Domain entity reference** | Reference to the underlying normalized domain entity (IdentityRecord, CredentialMetadata, PermissionRelationship, AccountabilitySubject, or other supported entity). |
| **Provenance references** | One or more provenance records identifying the source and collection context for this node. |
| **Capability state** | The capability/collection state for the data supporting this node's existence. |
| **Display metadata** | Human-readable metadata for diagnostic and output purposes. NOT used for identity matching or equality. |

### Node kinds

| Node kind | Domain entity | Description |
| --- | --- | --- |
| **ApplicationRegistration** | IdentityRecord (IdentityKind: ApplicationRegistration) | A normalized application registration. |
| **ServicePrincipal** | IdentityRecord (IdentityKind: ServicePrincipal) | A normalized service principal not classified as a managed identity. |
| **ManagedIdentity** | IdentityRecord (IdentityKind: ManagedIdentity) | A service principal classified as a managed identity through documented source evidence. Preserves service-principal provenance. |
| **AgentIdentity** | IdentityRecord (IdentityKind: AgentIdentity) | An agent identity where supported by documented capability. |
| **CredentialMetadata** | CredentialMetadata | Non-secret credential metadata associated with an identity. |
| **PermissionRelationship** | PermissionRelationship | A normalized permission or role assignment relationship. |
| **AccountabilitySubject** | AccountabilitySubject | A normalized owner, sponsor, or manager subject. |

---

## Graph edges

Edges represent normalized relationships between nodes.

### Edge definition

| Concept | Description |
| --- | --- |
| **Source node** | The graph node from which this edge originates. |
| **Target node** | The graph node to which this edge points. |
| **Relationship kind** | The normalized category of relationship (see [Graph relationship categories](#graph-relationship-categories)). |
| **Derivation classification** | Whether this edge was **observed** (directly returned by a documented source) or **derived** (deterministically computed from observed facts by an explicit rule). |
| **Provenance** | Source reference and collection context for this edge. For derived edges, the derivation rule and source observations are recorded. |
| **Capability/observation state** | The capability state for this edge — whether the relationship was fully observed, partially observed, or its observation was limited by authorization/licensing. |

### Observed vs derived edges

| Classification | Definition | Traceability |
| --- | --- | --- |
| **Observed** | Directly returned by a documented source (a Microsoft Graph relationship endpoint for V1, or a future explicitly enabled optional source such as Azure Resource Manager). Optional sources are not V1 dependencies. | Traceable to a specific source observation and API response. |
| **Derived** | Deterministically computed from observed facts by an explicitly defined derivation rule. | Traceable to the source observations and the derivation rule that produced the edge. |

Derived edges MUST be:

1. Produced by deterministic logic
2. Traceable to the source facts they derive from
3. Clearly labeled as derived in any diagnostic, debug, or audit output

---

## Graph relationship categories

### V1 relationship categories

The following relationship categories are justified by V1 requirements and supported documented capability:

| Category | Source node | Target node | Classification | Notes |
| --- | --- | --- | --- | --- |
| **hasServicePrincipal** | ApplicationRegistration | ServicePrincipal | Observed | Links an application registration to its associated service principal through documented Microsoft Graph relationship. |
| **hasServicePrincipal** | ApplicationRegistration | ManagedIdentity | Observed | Links an application registration to a managed-identity service principal where the managed identity is backed by an application. Preserves the underlying service-principal relationship. |
| **underlyingApplication** | ServicePrincipal | ApplicationRegistration | Observed | References the underlying application for a service principal where documented. Inverse of hasServicePrincipal. |
| **managedIdentityClassification** | ManagedIdentity | ServicePrincipal | Derived | A derived edge connecting a managed-identity-classified node to its underlying service-principal source observation. Produced when documented source evidence supports the ManagedIdentity classification. |
| **ownedBy** | ApplicationRegistration | AccountabilitySubject | Observed | Ownership relationship from application registration to an owner subject. |
| **ownedBy** | ServicePrincipal | AccountabilitySubject | Observed | Ownership relationship from service principal to an owner subject. |
| **ownedBy** | ManagedIdentity | AccountabilitySubject | Observed | Ownership relationship from managed identity to an owner subject. |
| **sponsoredBy** | ApplicationRegistration | AccountabilitySubject | Observed | Sponsorship relationship where documented. |
| **sponsoredBy** | ServicePrincipal | AccountabilitySubject | Observed | Sponsorship relationship where documented. |
| **managedBy** | ApplicationRegistration | AccountabilitySubject | Observed | Manager relationship where documented. |
| **managedBy** | ServicePrincipal | AccountabilitySubject | Observed | Manager relationship where documented. |
| **hasCredential** | ApplicationRegistration | CredentialMetadata | Observed | Credential metadata associated with an application registration. |
| **hasCredential** | ServicePrincipal | CredentialMetadata | Observed | Credential metadata associated with a service principal. |
| **hasCredential** | ManagedIdentity | CredentialMetadata | Observed | Credential metadata associated with a managed identity. |
| **hasPermission** | ApplicationRegistration | PermissionRelationship | Observed | Permission or role assignment associated with an application registration. |
| **hasPermission** | ServicePrincipal | PermissionRelationship | Observed | Permission or role assignment associated with a service principal. |
| **hasPermission** | ManagedIdentity | PermissionRelationship | Observed | Permission or role assignment associated with a managed identity. |

AgentIdentity provider/source mapping is validation-required. Relationships to agent identity blueprints, blueprint principals, service-principal infrastructure, or other Agent ID objects must be introduced only after the collector design validates documented Microsoft semantics. Unsupported or unavailable Agent ID capability remains explicit. The normalized model must preserve sufficient source provenance to support those mappings later without inventing them now.

### Relationship category restrictions

- NOT every relationship category applies to every `IdentityKind`. The graph MUST NOT assert a relationship merely because it is plausible.
- Every graph edge MUST have either:
  - observed source support (directly returned by a documented source), OR
  - an explicitly identified deterministic derivation rule based on supported observations.
- Relationships not supported by source data MUST NOT be present in the graph.

---

## Graph integrity invariants

The following invariants MUST be preserved by graph construction and maintained throughout the assessment lifecycle:

### INV-G1 — No cross-tenant edges

Edges MUST NOT silently join tenant contexts. V1 does not model cross-tenant relationships unless explicitly supported by a future approved requirement. Each graph is scoped to a single tenant assessment context. [INV-10]

### INV-G2 — Display names do not establish identity equality

Display names are human-readable metadata. They MUST NOT be used to match, merge, deduplicate, or establish equality between nodes. Identity equality is determined exclusively by normalized identity keys and source references.

### INV-G3 — Missing nodes and edges from unavailable collection

Missing nodes or edges due to unavailable collection capability, insufficient authorization, licensing limitations, or collection errors MUST NOT be interpreted as confirmed absence. The capability state MUST reflect why data is missing. [INV-05, INV-07, CAP-002]

### INV-G4 — Deterministic deduplication

Duplicate source observations MUST normalize deterministically. The normalization process MUST produce a single canonical record for a given source object rather than creating contradictory identities. The deduplication algorithm is TBD but must be deterministic and documented.

### INV-G5 — Conflict detection

Conflicting observations (e.g., two source responses providing contradictory data for the same source object) MUST remain detectable. They MUST NOT be silently overwritten or resolved by heuristic inference. Conflicts MAY be surfaced through capability state or explicit conflict metadata. [INV-02]

### INV-G6 — Derived edges distinguishable from observed

Every derived edge MUST be clearly distinguishable from directly observed edges through its derivation classification. Downstream consumers (rules, findings, diagnostics) MUST be able to determine whether a relationship was directly observed or deterministically computed.

### INV-G7 — Rules consume normalized contracts

Rules MUST consume normalized graph and domain contracts. Rules MUST NOT:

- call Graph or provider APIs
- request additional privileges
- use provider SDK types
- access raw provider payloads

This is the provider isolation boundary. [INV-03, INV-04]

### INV-G8 — Graph construction failure is explicit

Graph construction failure MUST be surfaced explicitly. A failed or partial graph MUST NOT produce false PASS outcomes. Downstream rules MUST NOT evaluate against an incomplete graph without explicit capability state indicating the limitation. [INV-14]

---

## Rule engine boundary

The identity graph serves as input to the deterministic rule engine. The handoff is defined by these boundaries:

### Rules MAY consume

- Normalized identities (IdentityRecord)
- Normalized relationships (edges)
- Graph projections (subgraphs, traversals, aggregations)
- Capability states
- Provenance references
- Deterministic assessment configuration and version context

### Rules MUST NOT

- call Microsoft Graph, Azure Resource Manager, or any other provider API
- request additional privileges beyond those already used for collection
- silently repair or infer missing data
- use an LLM or AI model to complete missing relationships
- infer undocumented Microsoft semantics
- treat collection failure, unavailable data, or missing capability as confirmed secure state
- mutate the graph or domain model during evaluation

Every rule evaluation MUST resolve to exactly one of PASS, FAIL, NOT_EVALUATED, NOT_APPLICABLE, or ERROR. [INV-02, INV-05]

---

## Graph operations

The identity graph supports the following operations for rule evaluation:

| Operation | Description |
| --- | --- |
| **Node lookup** | Find nodes by kind, graph key, or properties. |
| **Edge traversal** | Follow edges from a source node to target nodes by relationship kind. |
| **Subgraph extraction** | Extract a subgraph relevant to a specific rule or identity. |
| **Aggregation** | Count, list, or summarize nodes/edges matching criteria. |

All operations MUST be deterministic and idempotent. [INV-02]

---

## Graph scope

### In scope (V1)

- Application registrations
- Service principals
- Managed identities (classified through service-principal-backed source evidence)
- Agent identities (where supported by documented capability)
- Credential metadata (non-secret only)
- Permission relationships and role assignments
- Ownership, sponsorship, and manager accountability relationships

### Out of scope (V1)

- User identities (not non-human identities)
- Group memberships (unless exposed as NHI-relevant through documented Graph capability)
- Conditional Access policy internals (read-only reference only)
- Real-time relationship changes (V1 is point-in-time assessment)
- Graph database storage (implementation decision for later)

---

## Examples

> **Note:** The following examples are illustrative and non-normative. They use synthetic placeholder data.

### Example 1 — Application registration and service principal graph structure

Given a normalized domain model containing:

- IdentityRecord A (`ApplicationRegistration`, key: `app-001`, source: Graph application `aaaa...`)
- IdentityRecord B (`ServicePrincipal`, key: `sp-001`, source: Graph service principal `bbbb...`)
- Observed relationship: application `aaaa...` has service principal `bbbb...`

The graph contains:

```
Node(app-001, kind=ApplicationRegistration, provenance=[Graph application aaaa...])
    --hasServicePrincipal (observed)-->
Node(sp-001, kind=ServicePrincipal, provenance=[Graph service principal bbbb...])
```

The application registration and service principal are separate nodes. The edge is observed, not derived.

### Example 2 — Managed identity with service-principal provenance

Given a normalized domain model containing:

- IdentityRecord C (`ManagedIdentity`, key: `sp-002`, source: Graph service principal `cccc...`)

The graph contains:

```
Node(sp-002, kind=ManagedIdentity, provenance=[Graph service principal cccc...])
```

If the managed identity is backed by an application registration `dddd...`, and the source documents this relationship:

```
Node(sp-002, kind=ManagedIdentity, provenance=[Graph service principal cccc...])
    --hasServicePrincipal (observed)-->
Node(app-002, kind=ApplicationRegistration, provenance=[Graph application dddd...])

Node(app-002, kind=ApplicationRegistration, provenance=[Graph application dddd...])
    --hasServicePrincipal (observed)-->
Node(sp-002, kind=ManagedIdentity, provenance=[Graph service principal cccc...])
```

The `managedIdentityClassification` derived edge connects the managed-identity node to its underlying service-principal source observation:

```
Node(sp-002, kind=ManagedIdentity) --managedIdentityClassification (derived)--> [source: Graph service principal cccc...]
```

### Example 3 — Credential collection unavailable

Given a normalized domain model containing:

- IdentityRecord D (`ApplicationRegistration`, key: `app-003`)
- Credential collection capability state: `Unavailable — authorization`

The graph contains:

```
Node(app-003, kind=ApplicationRegistration)
    capabilityState = Unavailable — authorization (for CredentialMetadata)
```

No `hasCredential` edges are present for this node. The absence is NOT interpreted as "no credentials exist." The capability state explicitly indicates the limitation. A downstream rule evaluating credential hygiene applies its documented unavailable-input behavior; the capability state alone does not universally determine an evaluation state and MUST NOT be treated as evidence for `PASS` or tenant-security `FAIL`.

---

## Graph integrity

- The graph MUST NOT be mutated after construction during a single assessment run.
- The graph MUST NOT contain credential secret values, tokens, or authentication material.
- The graph MUST be discardable after assessment (no persistent storage required).
- Graph construction MUST be traceable to normalized domain model inputs.

---

## Open decisions / TBDs

- **TBD:** Graph serialization format for debugging and diagnostics.
- **TBD:** Maximum graph size handling strategy and memory limits.
- **TBD:** Formal graph query API definition.
- **TBD:** Deduplication algorithm for normalizing duplicate source observations into single graph nodes.
- **TBD:** Conflict-resolution policy for conflicting source observations.
- **TBD:** Performance characteristics for large-tenant graph construction.
- **TBD:** Exact graph key generation strategy and format.
- **TBD:** Subgraph extraction API for rule-specific evaluation contexts.
- **TBD:** AgentIdentity graph relationship mapping validation required before defining edges to application registrations, service principals, or other Agent ID objects.
