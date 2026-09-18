# Domain Model

> **Status:** Phase 0.2.3
> **Date:** 2026-09-18

---

## Purpose

Define the normalized NHI domain model that EntraNHI V1 uses to represent collected identity data. This model serves as the shared data contract between collectors, the identity graph, the rule engine, findings, and output adapters.

The domain model is intentionally conservative. It represents only fields and relationships that are documented as available through approved sources or that requirements explicitly require. Unsupported fields and relationships MUST NOT be invented.

---

## Domain model principles

### Principle 1 — Provider isolation

Provider/source objects are collector-side representations. They MUST be translated into normalized EntraNHI domain contracts before deterministic rule evaluation.

Rules MUST NOT depend directly on:

- Microsoft Graph SDK object types
- raw provider payload structures
- provider HTTP semantics

Source-specific facts MAY be represented through normalized provenance and capability metadata where necessary. [INV-03, INV-04]

### Principle 2 — Observed vs derived data

The domain model distinguishes four data categories:

| Category | Definition |
| --- | --- |
| **Observed** | Facts obtained directly from an approved documented source (e.g., Microsoft Graph response, Azure Resource Manager response, licensed telemetry). |
| **Normalized** | Canonical representation of observed facts after translation into domain contracts. Normalized data preserves the semantic content of the observation without introducing undocumented properties. |
| **Derived** | Deterministic relationships or classifications computed from normalized facts by explicitly defined logic. Derived values MUST NOT be represented as though they were directly observed. |
| **Evaluation** | Rule outcomes, findings, and evidence produced later by the rule engine. Evaluation data is produced after normalization and graph construction are complete. |

Downstream consumers (rule engine, graph, findings) MUST be able to distinguish these categories. [INV-04]

### Principle 3 — Null, unknown, and absent semantics

The domain model MUST NOT use one generic null value to represent every missing-data condition. Architecturally, the model must distinguish at least:

| Semantic | Description |
| --- | --- |
| **Present** | Observed value is present in the source and was collected. |
| **Explicitly empty** | Source explicitly indicates no value or empty relationship where that distinction is documented and reliable (e.g., an empty `$expand` result for a documented relationship). |
| **Not collected** | Property or relationship was not included in the collection scope for this assessment. |
| **Unavailable** | Collection was attempted but the data was not accessible due to authorization, licensing, or service limitations. |
| **Failed** | Collection was attempted but an error prevented retrieval. |
| **Unsupported** | The source or current EntraNHI implementation does not support collecting this data. |
| **Not applicable** | The concept does not apply to this identity kind (e.g., delegated permission grants for a identity kind that does not support delegated flows). |

Missing or unavailable data MUST NOT automatically imply a security FAIL. [INV-05, CAP-002]

The exact implementation representation of these semantics is TBD.

---

## Identity model

### IdentityRecord

The `IdentityRecord` is the normalized abstraction for a supported non-human identity within an assessment. It is the primary unit of assessment and rule evaluation.

| Concept | Description |
| --- | --- |
| **Identity key** | Stable assessment-local identifier for this normalized record. See [Identity keys and source references](#identity-keys-and-source-references). |
| **Identity kind** | Classification of the identity category (see IdentityKind below). |
| **Tenant assessment context** | Reference preventing silent cross-tenant identity mixing. See [Tenant assessment context](#tenant-assessment-context). |
| **Source references / Provenance** | One or more source references identifying the documented external objects from which this normalized record was derived. See [Provenance model](#provenance-model). |
| **Display metadata** | Human-readable display name and related metadata where collected. Display metadata is NOT an identity key. |
| **Capability state** | Normalized capability and collection-state metadata. See [Capability model](#capability-model). |
| **Accountability references** | Normalized accountability relationships (owner, sponsor, manager) where documented and observable. See [Accountability model](#accountability-model). |
| **Credential metadata references** | References to normalized credential metadata records associated with this identity. See [Credential metadata model](#credential-metadata-model). |
| **Permission relationship references** | References to normalized permission/access relationships associated with this identity. See [Permission / access relationship model](#permission--access-relationship-model). |
| **Graph relationship references** | References to normalized graph relationships involving this identity. See [Identity graph](identity-graph.md). |

Display names MUST NOT serve as identity keys. Not every identity kind exposes the same provider identifiers.

### IdentityKind

`IdentityKind` classifies the normalized identity into a supported category for V1 assessment:

| Kind | Description | Distinguishing notes |
| --- | --- | --- |
| **ApplicationRegistration** | A Microsoft Entra application registration. | Represents the application object. MUST remain distinguishable from its associated service principals. |
| **ServicePrincipal** | A Microsoft Entra service principal that is NOT classified as a managed identity. | May represent an application's service principal, a legacy service principal, or a cross-tenant service principal. |
| **ManagedIdentity** | A service principal classified as a managed identity through documented source evidence. | EntraNHI MAY classify a supported service principal as `ManagedIdentity` when documented source data provides sufficient reliable evidence that the service-principal-backed identity is a managed identity. The underlying service-principal record and provenance MUST be preserved rather than pretending `ManagedIdentity` is an unrelated provider object. Exact property/value mapping is collector-layer design. |
| **AgentIdentity** | A Microsoft Entra agent identity where supported by documented capability. | Modeled only where supported through documented observable capability. Unsupported or unavailable semantics MUST remain explicit. Do not infer undocumented relationships. Agent blueprints or related concepts MAY be represented only where requirements and documented capability support them. |

Application registrations and service principals MUST remain distinguishable. They are not interchangeable objects. [FR-001, FR-002, FR-003, FR-004]

---

## Identity keys and source references

The domain model defines three distinct identity/reference concepts:

### 1. Assessment-local normalized key

A stable, unique identifier for a normalized record within a single assessment. This key is:

- Generated during normalization.
- Scoped to the assessment context.
- Used for internal deduplication, graph construction, and rule evaluation.
- NOT derived from concatenating undocumented identifiers.

The exact format is TBD.

### 2. Source reference

A structured reference identifying the documented external source object sufficiently for provenance. A source reference captures:

- Source system or API family (e.g., Microsoft Graph, Azure Resource Manager).
- Source object type within that system.
- Source object identifier as documented and exposed by the source.
- Collection context (e.g., which API call or query produced this observation).

Not every identity kind exposes the same provider identifiers. Source references MUST use only identifiers documented and exposed by the source system.

### 3. Tenant assessment context

A reference that prevents silent cross-tenant identity mixing. Each assessment operates within a single tenant scope. The tenant assessment context ensures:

- Normalized records are attributable to the correct tenant.
- No cross-tenant edges or relationships are silently created.
- Assessment output is unambiguously scoped.

See [INV-10] for tenant boundary preservation requirements.

---

## Accountability model

The domain model represents normalized accountability relationships where they are applicable and observable through supported documented capability.

### Accountability relationship categories

| Category | Description | Observability |
| --- | --- | --- |
| **Owner** | Documented ownership relationship for the identity. | Observable through supported Microsoft Graph relationship endpoints. |
| **Sponsor** | Documented sponsor relationship for the identity. | Observable where the source exposes sponsor-type relationships. |
| **Manager** | Documented manager relationship for the identity. | Observable through supported Microsoft Graph relationship endpoints. |

Do not define a generic "approver" relationship. Only the categories above are modeled for V1.

### Accountability subject reference

Each accountability relationship references a normalized subject rather than embedding unnecessary directory objects. A subject reference captures:

- Subject source reference (source system and object identifier).
- Subject display metadata where collected.
- Normalized relationship kind (owner, sponsor, manager).

### Distinguishing absence from unavailability

The model MUST distinguish:

- **No accountability relationship observed:** The source returned no relationship of this type for this identity.
- **Accountability capability not collected or unavailable:** Collection was attempted but the relationship data was not accessible, or the capability was not within the collection scope.

These are semantically different states and MUST NOT be conflated. [FR-012]

---

## Credential metadata model

`CredentialMetadata` represents non-secret metadata only. It MUST NOT contain secret material of any kind.

### Normalized credential metadata concepts

| Concept | Description | Condition |
| --- | --- | --- |
| **Credential category/type** | Classification of the credential (e.g., asymmetric key, symmetric secret, federated identity credential). | Where documented and exposed by the source. |
| **Source reference** | Reference to the source object from which this credential metadata was collected. | Always. |
| **Validity/lifecycle boundaries** | Start/valid-from and expiration/end timestamps as exposed by the source. | Where the source documents and exposes these timestamps. |
| **Non-secret identifier or fingerprint** | A non-secret identifier, key ID, thumbprint, or fingerprint-like metadata where the source documents it as non-secret and requirements permit. | Only where the source documents the value as non-secret and requirements permit its inclusion. |
| **Capability/provenance state** | The capability and collection state for this credential metadata observation. | Always. |

### Forbidden content

Credential metadata MUST NOT contain:

- client secret values
- private keys
- passwords
- certificate private-key material
- bearer/access/refresh tokens
- recovery codes

### Rotation inference restriction

Do NOT create a "last rotation date" unless it is directly and reliably exposed by a documented source. Do not infer rotation from creation or start dates. [FR-013, SEC-001, INV-09]

---

## Permission / access relationship model

The domain model defines normalized permission relationships without hard-coding Graph permission names.

### Normalized relationship categories

Where observable and supported, the model distinguishes:

| Category | Description | Notes |
| --- | --- | --- |
| **Application permission / app-role assignment** | An application permission or app-role assignment style relationship for the identity. | Where the source exposes this category of permission for the identity kind. |
| **Delegated permission grant** | A delegated permission grant style relationship where applicable to the assessed identity context. | Applicable only where the identity kind and source support delegated flows. |
| **Resource/target reference** | The target resource or principal to which the permission/role is scoped. | Where the source documents the target. |
| **Permission/role reference** | Reference to the specific permission or role definition. | Where the source documents the permission or role identifier. |
| **Provenance** | Source reference and collection context for the permission observation. | Always. |
| **Collection/capability state** | Whether this permission category was collectible and what was observed. | Always. |

### Scope restrictions

- NOT every permission category applies to every `IdentityKind`. The model MUST NOT claim universal applicability.
- The model MUST NOT define exact Graph endpoints or permission names in this phase.
- The model MUST NOT conflate:
  - permissions EntraNHI requires to collect data (collection permissions), with
  - permissions possessed by the identity being assessed (assessed-identity permissions).

These are separate security concepts. [FR-014]

---

## Capability model

The `CapabilityState` concept represents the collection and availability status of a capability, data point, or relationship for a specific observation.

### Capability states

The model must be able to distinguish conceptually:

| State | Description |
| --- | --- |
| **Available** | The capability was available and data was collected successfully. |
| **Unavailable — authorization** | Data was not accessible due to authorization or permission boundary of the authenticated principal. |
| **Unavailable — licensing/service** | Data was not accessible because the required licensing tier or service capability is not present in the target environment. |
| **Unsupported** | The current EntraNHI implementation does not support collecting this data. |
| **Failed** | Collection was attempted but an error occurred. |
| **Not applicable** | The capability or data point does not apply to this identity kind or assessment context. |

Exact enum names and serialization are TBD.

### Rule engine interaction

Capability state MUST be available to downstream deterministic rules so they can correctly choose among:

- PASS
- FAIL
- NOT_EVALUATED
- NOT_APPLICABLE
- ERROR

Do not invent Microsoft licensing behavior. Capability states reflect what was observed during collection, not assumptions about tenant licensing. [INV-05, INV-07, CAP-001, CAP-002]

---

## Provenance model

The provenance model ensures that normalized observations retain enough context to identify their origin and collection context.

### SourceObservation / Provenance

Each provenance record captures:

| Concept | Description |
| --- | --- |
| **Source system** | The API family or source system from which the observation was collected (e.g., Microsoft Graph, Azure Resource Manager). |
| **Source object reference** | The specific object or resource within the source system that was queried. |
| **Collection operation/context** | The API call, query, or collection operation that produced the observation. |
| **Assessment context** | The assessment run and tenant scope within which this observation was collected. |
| **Observation time** | The time at which the observation was collected, where appropriate for downstream consumers. |
| **Transformation/derivation lineage** | If the normalized record was derived from other observations, the derivation path and source observations are recorded. |

### Provenance restrictions

Provenance MUST NOT store:

- authentication tokens
- raw secrets
- unnecessary raw payloads

Provenance is not evidence by itself. Later finding/evidence architecture determines which provenance-backed observations become rule evidence. [INV-11]

---

## Examples

> **Note:** The following examples are illustrative and non-normative. They use synthetic placeholder data.

### Example 1 — Application registration and service principal remain distinct

An application registration `contoso-function-app` has an associated service principal. In the normalized model:

- **IdentityRecord A** — `IdentityKind: ApplicationRegistration`
  - Identity key: `assessment-local-app-001`
  - Source reference: Microsoft Graph application object `aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee`
  - Display name: `contoso-function-app`

- **IdentityRecord B** — `IdentityKind: ServicePrincipal`
  - Identity key: `assessment-local-sp-001`
  - Source reference: Microsoft Graph service principal object `11111111-2222-3333-4444-555555555555`
  - Display name: `contoso-function-app`

- **Graph edge** — `ApplicationRegistration` → `hasServicePrincipal` → `ServicePrincipal` (observed, source: Microsoft Graph application-to-service-principal relationship)

The application registration and service principal are separate normalized identities connected by a supported relationship. They are NOT merged or treated as the same record. Display names match but do NOT establish identity equality.

### Example 2 — Managed identity preserves service-principal provenance

A managed identity appears in Microsoft Entra as a service principal. A documented source observation reliably identifies the service-principal-backed identity as a managed identity. In the normalized model:

- **IdentityRecord C** — `IdentityKind: ManagedIdentity`
  - Identity key: `assessment-local-sp-002`
  - Source reference: Microsoft Graph service principal object `66666666-7777-8888-9999-aaaaaaaaaaaa`
  - Display name: `my-function-managed-identity`

The `ManagedIdentity` classification is a deterministic derivation based on documented source evidence. The underlying service-principal source reference is preserved. The model does NOT pretend `ManagedIdentity` is an unrelated provider object — it is a classified service principal with retained provenance. Exact property/value mapping is collector-layer design.

### Example 3 — Credential collection unavailable

During collection, credential metadata for an identity is not accessible due to insufficient permissions. In the normalized model:

- **IdentityRecord D** — `IdentityKind: ApplicationRegistration`
  - Identity key: `assessment-local-app-002`
  - Credential metadata: empty (no credential records present)
  - Credential collection capability state: `Unavailable — authorization`

The absence of credential records is NOT interpreted as "no credentials exist." The capability state explicitly indicates that credential data was unavailable due to an authorization boundary. A downstream rule evaluating credential hygiene would resolve to `NOT_EVALUATED` with a structured reason referencing the authorization-limited capability state, NOT to `PASS` or `FAIL`.

---

## Model constraints

- The domain model MUST NOT include fields not supported by documented sources or explicitly required by product requirements.
- The domain model MUST be extended only when new source capabilities are validated and documented.
- The domain model MUST NOT contain credential secret values, private keys, tokens, or other authentication secrets.
- The domain model MUST represent missing or unavailable data through the semantics defined in [Null, unknown, and absent semantics](#null-unknown-and-absent-semantics), not through invented placeholder values.
- Display names MUST NOT be used as identity keys.

---

## Open decisions / TBDs

The following are implementation decisions not appropriate for resolution at this phase:

- **TBD:** Concrete C# record/class shapes for domain entities.
- **TBD:** Enum names for IdentityKind, CapabilityState, credential categories, and permission categories.
- **TBD:** Serialization schema (JSON, etc.).
- **TBD:** Assessment-local key format and generation strategy.
- **TBD:** Deduplication algorithm details for normalizing duplicate source observations.
- **TBD:** Conflict-resolution policy for conflicting source observations.
- **TBD:** Exact Microsoft Graph endpoint and property mapping for each field.
- **TBD:** Exact permission mapping for collection requirements.
- **TBD:** Agent identity mapping details pending documented capability validation.
- **TBD:** Optional ARM enrichment mapping for managed identity Azure resource context.
- **TBD:** Performance and memory limits for normalized domain model construction.
- **TBD:** Formal schema validation approach for normalized contracts.
