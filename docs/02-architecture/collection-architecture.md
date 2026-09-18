# Collection Architecture

> **Status:** Phase 0.2.4
> **Date:** 2026-09-18

---

## Purpose

Define the EntraNHI V1 collection architecture: the collector contract, source observation envelope, capability detection semantics, failure model, normalization handoff, and architectural boundaries that govern how documented observable state is acquired from approved external sources and translated toward the normalized domain boundary.

The collection layer acquires documented observable state from approved external sources and translates source observations toward the normalized domain boundary. Collectors MUST NOT perform security-rule evaluation. Rules MUST NOT query collectors or providers directly.

---

## 1. Source boundary

### V1 primary source family

Microsoft Graph is the primary V1 collection API and source family. All V1 identity discovery and collection occurs through Microsoft Graph endpoints.

### Optional documented enrichment sources

Optional documented enrichment sources may exist in the future, but MUST:

- be isolated behind their own collector/adaptor boundary
- be explicitly enabled/configured
- preserve independent provenance
- expose their own capability state
- never become an undocumented implicit dependency

Azure Resource Manager or another Microsoft API MUST NOT be assumed to be required merely because some identity type may have related resource data. Exact optional enrichment design remains TBD.

Third-party tenant-data services MUST NOT be introduced.

---

## 2. Collector contract

A collector is a logical component responsible for acquiring documented observable state from a single source family or source concern.

### 2.1 Collector MAY

A collector MAY:

- acquire supported documented observations from its assigned source
- enumerate supported source objects
- collect supported relationships between source objects
- collect non-secret credential metadata
- collect supported permission/access relationships
- collect accountability relationships where documented
- preserve source references and provenance
- report collection and capability state
- report structured collection failures
- provide source observations and capability results to normalization

### 2.2 Collector MUST NOT

A collector MUST NOT:

- evaluate PASS/FAIL or produce security verdicts
- perform remediation or modify tenant state
- mutate any external resource
- request elevated privileges dynamically
- silently expand its collection scope beyond documented observations
- hide partial collection or missing data
- manufacture missing observations
- infer undocumented provider semantics
- pass authentication secrets into the normalized domain model
- construct security findings

---

## 3. Collector decomposition

The following are logical collector responsibilities, not mandatory implementation classes. They represent distinct areas of observable state that may be acquired from documented sources.

| Logical collector | Responsibility |
| --- | --- |
| Application registration collector | Application registration metadata, properties, and documented attributes |
| Service principal collector | Service principal metadata, properties, and documented attributes |
| Managed identity classification collector | Observations necessary for later normalization/classification of managed identities from service-principal metadata |
| Agent identity collector | Supported AgentIdentity observations where documented capability exists |
| Accountability collector | Ownership, sponsorship, and manager relationships where documented |
| Credential metadata collector | Non-secret credential lifecycle metadata (type, identifier, validity timestamps) |
| Permission/access collector | Application permissions, delegated permissions, role assignments, and consent status |
| Relationship collector | Identity-to-identity relationships, application-to-service-principal links, dependency observations |

These are logical responsibilities. A single API call may serve multiple logical collectors. A single logical collector may require multiple API calls. No assumption is made about one-to-one mapping between collectors and endpoints.

---

## 4. Source observation envelope

A `SourceObservation` is the conceptual envelope through which collectors convey source observations to normalization. It carries sufficient non-secret information to support normalization, provenance, and capability reporting.

### 4.1 Source observation content

A source observation carries:

| Concept | Description |
| --- | --- |
| Source family | The API family or source system (e.g., Microsoft Graph) |
| Source object/reference | The specific source object or resource from which this observation was obtained |
| Tenant assessment context | Reference preventing cross-tenant data mixing |
| Collection operation/context | The API call, query, or collection operation that produced this observation |
| Observation timestamp/context | Time context where appropriate for downstream consumers |
| Capability state | The collection and capability state for this observation |
| Source data | Data required for normalization into domain contracts |
| Provenance | Source system, object reference, collection context, and assessment context |
| Structured error information | Error details where collection failed or produced unexpected results |

### 4.2 Source observation restrictions

- Raw provider payload retention MUST NOT be required
- Authentication tokens and secret values MUST NOT become source observation content
- Exact serialization and schema are TBD

---

## 5. Capability detection

Capability detection is a first-class architectural function. It determines what data can be collected, what is unavailable, and what is not applicable, independent of rule evaluation.

### 5.1 Capability outcomes

Capability detection is capable of distinguishing:

| Outcome | Description |
| --- | --- |
| Available and collected | The capability was present, accessible, and data was successfully collected |
| Unavailable — authorization | The capability exists but the authenticated principal lacks permission to access it |
| Unavailable — licensing/service | The required service capability or licensing tier is absent from the target environment |
| Unsupported | The current EntraNHI implementation does not support collecting this data |
| Not applicable | The capability or data category does not apply to this identity kind or assessment context |
| Collection/runtime error | An error prevented determining availability or collecting data |
| Partial collection | A subset of expected observations was collected where this is a meaningful and documented distinction |

Capability detection MUST NOT convert missing data into a security finding. It informs downstream deterministic rule evaluation.

### 5.2 Capability granularity

Capability state must be sufficiently granular that failure to collect one data category does not automatically invalidate unrelated successfully collected categories.

The following separations MUST hold:

- Credential metadata collection unavailable MUST NOT automatically mean identity enumeration failed
- Accountability collection unavailable MUST NOT erase successfully collected identity observations
- AgentIdentity capability unavailable MUST NOT prevent assessment of supported application registrations and service principals
- Permission collection unavailable MUST NOT invalidate identity metadata that was successfully collected
- A single identity's capability failures MUST NOT affect other identities' successfully collected data

Exact capability hierarchy and schema are TBD.

---

## 6. Authentication vs authorization vs collection

The collection architecture maintains a strict conceptual distinction among three concerns:

### Authentication

Establishes the caller/workload identity and obtains authorized access. Authentication is the responsibility of the authentication boundary, not the collector layer.

### Authorization

Determines whether that authenticated principal can access a requested source capability. Authorization boundaries are expressed as capability states, not as collector logic.

### Collection

Uses the already-established authorized context to obtain documented observations. Collection consumes authorized access tokens transiently and does not own, persist, or manage authentication material.

Collectors MUST NOT own long-lived credentials. Collectors MUST NOT persist access tokens, refresh tokens, client secret values, private keys, or passwords.

Authentication mechanics remain primarily defined by authentication-authorization.md and later implementation design. Exact OAuth flows are not prescribed in this document.

---

## 7. Least privilege

Collection architecture must support least privilege.

- Collection capabilities must be traceable to required source access
- Optional capabilities should not silently increase privilege requirements
- Unavailable optional capability should remain explicit and MUST NOT force broader privilege requirements
- Collector design must support future documentation of exact permission requirements per capability

Exact Microsoft Graph permission names are not defined in this document. Those require separate documentation validation.

---

## 8. Pagination, scale, and retry

Collectors must support, where the documented source requires it:

- pagination of multi-page result sets
- bounded retry behavior for transient failures
- transient failure handling with appropriate backoff
- rate and throttling handling
- cancellation of in-progress collection
- large tenant enumeration without unbounded memory growth
- partial failure reporting where some objects succeed and others fail

Do not invent retry counts, timeout values, Graph page-size constants, or throttling thresholds. Exact operational policies remain TBD.

---

## 9. Consistency and snapshot semantics

An assessment may require multiple source operations and therefore cannot assume that the tenant is transactionally frozen during collection.

The following architectural requirements apply:

- Observations can have collection timestamps and collection-time context
- The normalized assessment represents an assessment collection window, not a transactional snapshot
- Conflicting observations must not be silently overwritten
- Material collection inconsistencies must remain diagnosable
- EntraNHI must not claim transactional snapshot semantics unless the source actually provides them

Exact consistency policy remains TBD.

---

## 10. Failure model

Structured collection failure categories are defined conceptually. These categories do not map directly to rule PASS/FAIL outcomes.

| Failure category | Description |
| --- | --- |
| Authentication failure | The authentication boundary could not establish authorized access |
| Authorization/capability unavailable | The authenticated principal lacks permission or the capability is absent |
| Source/service unavailable | The source API or service is not reachable or is down |
| Throttling/transient failure exhausted | Repeated transient failures exceeded bounded retry behavior |
| Malformed/unexpected source response | The source returned a response that does not match documented expectations |
| Normalization-input validation failure | Source observation data failed normalization input validation |
| Unsupported capability | The current EntraNHI implementation does not support this collection |
| Cancellation | Collection was cancelled before completion |

Collection failure must remain visible downstream. A failed collector MUST NOT silently return an empty successful collection. Every collection error must be recorded and propagated to the domain model capability state so the rule engine can produce NOT_EVALUATED or ERROR as appropriate.

---

## 11. Normalization handoff

The collector-to-normalization boundary defines what the collector layer produces and what normalization consumes.

### 11.1 Collector output

Collectors produce:

- source observations (normalized toward domain-boundary data)
- capability results per observation category

### 11.2 Normalization responsibilities

Normalization:

- validates source observations
- converts provider-specific observations into normalized domain contracts
- preserves provenance from source observations
- distinguishes observed information from derived information
- preserves unavailable, unknown, and error states
- prevents provider SDK objects or resource references from entering rule contracts

### 11.3 Boundary constraint

The collector layer MUST NOT directly construct security findings. Findings are produced downstream by the deterministic rule engine after normalization and identity graph construction.

---

## 12. Agent identity safety boundary

AgentIdentity collection is capability-gated.

Do not define exact endpoints, properties, permission requirements, blueprint mappings, service-principal mappings, or licensing requirements in this document until separately validated against documented Microsoft capability.

The collector architecture must be able to add supported AgentIdentity observations without redesigning the normalized core.

Unsupported or unavailable AgentIdentity collection must remain explicit and must not block unrelated identity assessment of application registrations and service principals.

---

## 13. Managed identity safety boundary

ManagedIdentity classification must be based only on reliable documented source observations.

The collector architecture may collect the observations necessary for later normalization and classification. Do not hard-code exact provider property or value mappings in this architecture phase.

Optional ARM enrichment must remain separate and non-mandatory unless a future validated requirement explicitly changes that boundary.

---

## 14. Security and privacy boundary

Collectors operate read-only against tenant services. Local creation of assessment artifacts in the execution environment is not tenant mutation.

- Collection must avoid unnecessary data acquisition
- Do not collect secret values
- Do not require persistence of raw provider payloads
- Diagnostic information must not expose authentication material
- No undisclosed telemetry or third-party transmission of tenant assessment data

---

## 15. Determinism boundary

Collection itself interacts with mutable external state and therefore may produce different observations at different times.

Determinism applies downstream: given the same normalized observations, capability state, rule/configuration version, and relevant deterministic execution context, rule evaluation must produce the same semantic outcome.

Repeated live collection MUST NOT be assumed to produce identical data. The assessment collection window captures the state observed during a specific collection period.

---

## 16. Testability requirements

Design the collection layer so future implementation can test:

- provider responses using synthetic fixtures and mocks
- pagination behavior across multi-page results
- partial capability availability scenarios
- authorization denial scenarios
- transient failure and retry behavior
- malformed or unexpected responses
- cancellation of in-progress collection
- normalization handoff correctness
- secret-exclusion guarantees
- collector isolation between logical collector units
- AgentIdentity unsupported and unavailable scenarios

No real tenant must be required for core unit testing. Tests are not created in this phase.

---

## 17. Open decisions / TBDs

The following are implementation decisions not appropriate for resolution at this phase:

- exact Microsoft Graph endpoints per collector
- exact Microsoft Graph permission mapping per capability
- concrete authentication flows and OAuth mechanics
- concrete collector interfaces, classes, or composition
- Graph SDK vs direct HTTP boundaries
- pagination implementation details
- retry and backoff policy constants
- timeout policy constants
- concurrency limits
- capability schema and enum definitions
- exact managed-identity source property/value mapping
- AgentIdentity endpoint, property, and relationship mapping
- optional ARM enrichment scope and permissions
- caching strategy for collected observations
- raw-response diagnostic retention policy
- large-tenant performance limits and memory bounds

These must not be resolved by speculation.

---

## 18. Cross-references

The collection architecture is constrained by the following architecture invariants defined in `docs/02-architecture/architecture-invariants.md`:

| Invariant | Relevance |
| --- | --- |
| INV-01 | Read-only tenant operation — collectors MUST NOT mutate tenant state |
| INV-03 | Provider isolation — rule engine MUST NOT query collectors or providers directly |
| INV-04 | Normalized domain boundary — collectors produce observations for normalization, not rule-ready data |
| INV-05 | Explicit evaluation states — collection failure MUST NOT silently produce PASS/FAIL |
| INV-07 | Capability awareness — capability detection is a first-class collection function |
| INV-08 | Least privilege — collection capabilities traceable to required source access |
| INV-09 | Secret exclusion — collectors MUST NOT pass secrets into the domain model |
| INV-10 | Tenant boundary preservation — source observations carry tenant assessment context |
| INV-11 | Provenance preservation — every source observation preserves provenance metadata |
| INV-14 | Failure transparency — collection failures remain explicit and visible downstream |
| INV-15 | No undocumented capability dependency — collectors use only documented source behavior |
| INV-16 | Security-sensitive defaults — optional capabilities do not silently increase privilege |

---

## 19. Self-verification

Before finalizing this document, confirm the following:

1. Only `collection-architecture.md` was modified.
2. No file was created, deleted, renamed, or moved.
3. Microsoft Graph remains the primary V1 source family.
4. Optional enrichment remains isolated and non-mandatory.
5. No exact Graph permission name was invented.
6. No exact Graph endpoint or property mapping was invented.
7. AgentIdentity remains capability-gated.
8. ManagedIdentity mapping remains provider-independent in this architecture phase.
9. Missing capability is not treated as empty or secure data.
10. Collector failures remain explicit and visible downstream.
11. Collectors do not perform PASS/FAIL evaluation.
12. Rules do not query providers or collectors directly.
13. No secret-bearing data model was introduced.
14. No Git operation was performed.
