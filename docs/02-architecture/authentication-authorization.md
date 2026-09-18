# Authentication & Authorization Architecture

> **Status:** Phase 0.2.5 — Architecture documentation
> **Date:** 2026-09-18

---

## Purpose

Define the V1 authentication and authorization architecture for EntraNHI. This document establishes security principles, execution-context requirements, authentication/authorization boundaries, capability-aware authorization, secret handling, tenant boundary, failure semantics, and AI safety constraints. Exact OAuth flows, Graph permission names, credential mechanisms, and Microsoft-specific permission mappings require separate documentation validation before implementation.

---

## 1. Security Principles

The following principles govern all authentication and authorization design:

- **Least privilege:** Each collection capability requests only the minimum source access it requires. No silent privilege expansion.
- **Read-only tenant operation:** EntraNHI V1 does not mutate tenant state. Read-only does not mean permissionless — authorized directory/API access may still be required. [INV-01]
- **No remediation privileges:** Write, delete, or administrative operations are outside V1 scope. [INV-01]
- **Explicit authorization/capability failures:** Missing authorization produces NOT_EVALUATED or ERROR, never silent PASS. [INV-05, INV-07, INV-14]
- **No silent privilege expansion:** Authentication success does not imply authorization for every capability. [INV-08]
- **No secret persistence in assessment artifacts:** Tokens, credentials, and secrets are runtime-only and never enter normalized domain data, findings, evidence, or reports. [INV-09]
- **Tenant context preservation:** Each execution context targets a single tenant. Data from separate tenants must never be silently mixed. [INV-10]
- **Separation of authentication from collection:** Authentication establishes identity and access context. Authorization determines which capabilities that context may access. Collectors consume the authorized context but do not own credentials. [INV-03]
- **Security-sensitive defaults:** Where optional behavior could weaken authorization boundaries, the secure behavior is the default. [INV-16]
- **No undocumented capability dependency:** All collected data must originate from documented Microsoft APIs. No reverse-engineered or assumed behavior. [INV-15]

Authentication success MUST NOT imply that every collection capability is authorized.

---

## 2. Execution Contexts

EntraNHI V1 architecturally supports two execution contexts. Exact supported authentication mechanisms for each context require later validated implementation.

### 2a. Interactive / operator execution

A human security or identity practitioner runs EntraNHI and authenticates through an approved Microsoft identity mechanism.

- Operator-initiated, interactive session
- Authenticated with operator's identity
- Suitable for ad-hoc assessment and exploration

### 2b. Non-interactive automation / CI execution

An authorized workload identity runs EntraNHI without interactive user input.

- Workload-initiated, scheduled or triggered
- Authenticated with an application or workload identity
- Suitable for automation, CI/CD pipelines, and scheduled assessment

**Constraints on both contexts:**

- Do NOT prescribe the exact OAuth grant or flow.
- Do NOT require client secrets.
- Do NOT assume certificates, federated credentials, managed identities, device code, browser authentication, or any other mechanism as mandatory.
- Concrete supported mechanisms require later implementation and Microsoft documentation validation.

---

## 3. Authentication Boundary

The authentication layer establishes identity and obtains authorized access context. It is a distinct architectural boundary separated from collection and normalization.

### 3.1 Authentication layer MAY

- Establish the execution identity
- Obtain authorized access context / tokens
- Identify the tenant assessment context
- Expose non-secret authorization metadata needed for capability evaluation
- Refresh or reacquire access where the selected validated mechanism supports it
- Report structured authentication failures

### 3.2 Authentication layer MUST NOT

- Expose tokens to normalized domain records
- Expose tokens to findings, evidence, or reports
- Write secrets into logs
- Silently persist authentication material
- Grant itself additional tenant privileges
- Perform tenant remediation
- Determine security PASS/FAIL findings

---

## 4. Authorized Access Context

An abstract `AuthorizedAccessContext` conceptually carries only what runtime collection requires:

- Tenant assessment context
- Source / API family
- Authenticated execution context reference
- Transient access mechanism / token handle
- Authorization / capability metadata where available
- Expiry / lifetime context where required for runtime operation

### 4.1 Runtime-only security-sensitive state

The `AuthorizedAccessContext` is runtime-only security-sensitive state. It MUST NOT become:

- Normalized identity data
- Evidence
- Finding content
- Report content
- Persisted assessment state

### 4.2 Shape

The exact implementation shape is TBD. This document does not define a serialized token-bearing data contract.

---

## 5. Authorization / Capability Boundary

Authorization is evaluated per required collection capability, not as a single global "authenticated = authorized" flag.

### 5.1 Supported capability states

| State | Description |
| --- | --- |
| Capability available | Authorization is sufficient for this capability |
| Capability unavailable — authorization insufficient | Identity lacks required permissions for this capability |
| Capability unavailable — other reason | Non-authorization reason handled by capability detection (e.g., licensing, API availability) |
| Partial capability availability | Some sub-capabilities are authorized while others are not |

### 5.2 Authorization denial semantics

Authorization denial MUST NOT automatically become a security FAIL against the tenant or assessed identity. It informs capability state and may cause downstream rules to become NOT_EVALUATED or ERROR according to rule semantics.

The authentication layer MUST NOT assign rule states directly. Capability state flows into the rule engine through capability detection, not through authentication-layer override.

### 5.3 Cross-reference

- [INV-05] — Explicit evaluation states
- [INV-07] — Capability awareness
- [INV-14] — Failure transparency

---

## 6. Least-Privilege Model

- Each collection capability should eventually document its minimum required source access.
- Optional capabilities must not silently broaden required privileges.
- EntraNHI must operate with only the capabilities selected / required for the assessment.
- Requesting unnecessary write or remediation privilege violates V1 architecture. [INV-01, INV-08]
- Future exact Microsoft permission mapping must be validated against official documentation before implementation.

Do NOT invent exact Graph permission names. Do NOT claim one universal permission set is sufficient.

---

## 7. Read-Only Does Not Mean Zero Privilege

EntraNHI V1 is operationally read-only with respect to tenant mutation. However, reading directory and security-relevant metadata may still require authorized directory/API access.

"Read-only" MUST NOT be described as:

- Anonymous
- Permissionless
- Inherently low sensitivity

Authorization remains security-sensitive even though EntraNHI does not mutate tenant state.

---

## 8. Secret / Token Handling

Authentication material may exist transiently in runtime memory where required by the validated authentication mechanism.

### 8.1 Exclusion from assessment artifacts

Authentication material MUST NOT enter:

- Normalized domain model
- Identity graph
- Findings
- Evidence
- Reports
- Persisted assessment artifacts
- Normal diagnostic logs

### 8.2 Never collect or store

- Client secret values as assessment data
- Private keys as assessment data
- Passwords
- Refresh / access tokens as assessment data
- Recovery material

### 8.3 Future credential/token caching

If future mechanisms require local credential or token caching, that requires a separate threat-model and implementation decision. This document does not prescribe concrete secure-storage technology.

---

## 9. Tenant Boundary

Authentication must establish or validate the tenant assessment context before collection data is accepted into an assessment.

### 9.1 Requirements

- Observations must remain associated with the intended tenant assessment context.
- Collectors must not silently mix data obtained under different tenant contexts.
- Changing tenant context must be explicit.
- Authentication context and normalized tenant context must be reconcilable.
- Mismatch must fail safely and visibly.

### 9.2 Implementation

Do not invent exact tenant-ID validation mechanics yet. This remains TBD.

---

## 10. Interactive Execution

Define architecture requirements only. Interactive authentication must:

- Clearly identify when user interaction is required
- Fail explicitly when authentication cannot complete
- Avoid printing tokens or secrets
- Preserve selected tenant context
- Expose authorization limitations through capability detection

Do not choose an exact interactive OAuth flow in this phase.

---

## 11. Automation / CI Execution

Define architecture requirements only. Non-interactive execution must:

- Support an approved workload authentication mechanism
- Avoid embedded credentials in repository or configuration
- Support secretless mechanisms where validated and available, without making them universally mandatory
- Fail closed if required authentication material is unavailable
- Never downgrade into an insecure fallback automatically
- Preserve tenant context
- Expose authorization limitations through capability detection

Do not define GitHub-specific or Azure-specific identity configuration yet.

---

## 12. Failure Model

### 12.1 Authentication / authorization failure categories

| Category | Description |
| --- | --- |
| Authentication unavailable | No authentication mechanism is configured or reachable |
| Authentication rejected | Identity provider rejected the credential |
| Tenant-context mismatch | Authenticated identity is in a different tenant than expected |
| Token / access acquisition failure | Token endpoint returned an error or timed out |
| Access context expired / unusable | Obtained access context is no longer valid |
| Authorization insufficient for capability | Identity lacks required permissions for a specific capability |
| Interactive authentication unavailable | Interactive flow required but execution mode does not support it |
| Workload authentication unavailable | Workload credential not configured or inaccessible |
| Cancellation | Operator or system cancelled authentication |
| Unexpected identity-provider response | Unrecognized or malformed response from identity provider |

### 12.2 Distinction from other failures

Authentication failures MUST remain distinguishable from:

- Collection failures
- Normalization failures
- Rule evaluation failures

### 12.3 No silent success

No authentication failure may silently produce a successful assessment.

---

## 13. Logging / Diagnostics

Security-sensitive diagnostics must:

- Identify failure category and affected capability / context
- Avoid tokens
- Avoid credential values
- Avoid private-key material
- Avoid unnecessary identity-provider response payloads
- Avoid misleading authentication failure as a tenant security finding

Correlation / diagnostic identifiers MAY be retained where non-secret and useful. Exact logging schema remains TBD.

---

## 14. Threat Boundary

Auth-specific threats to be addressed conceptually (cross-reference `threat-model.md` for later refinement):

- Credential / token disclosure
- Excessive privilege
- Wrong-tenant assessment
- Token reuse outside intended runtime boundary
- Insecure fallback authentication
- Secret leakage through logs / errors
- CI credential exposure
- Confused authentication vs. authorization state
- Authorization denial misrepresented as secure state

---

## 15. Agent Identity Distinction

Do not confuse:

**A. AgentIdentity objects being ASSESSED by EntraNHI**

These are domain data representing AI agent identities within the target tenant. They are normalized observations subject to rule evaluation.

**B. The execution identity / authentication mechanism used BY EntraNHI**

This is the identity through which EntraNHI authenticates to access tenant data. It is runtime-only and never enters normalized domain data.

An assessed AI agent identity is domain data. It does not automatically become EntraNHI's authentication identity. Do not introduce special AgentIdentity authentication semantics without validated requirements.

---

## 16. AI Security Boundary

AI / LLM functionality must never:

- Select broader privileges
- Alter authentication policy
- Obtain or handle raw authentication secrets
- Override authorization denial
- Fabricate capability availability
- Convert failed authorization into PASS
- Determine authentication trust

Authentication and authorization decisions remain deterministic and security-controlled. AI/LLM is not on the critical path for authentication or authorization decisions.

---

## 17. Testability

Design for future testing of:

- Successful interactive auth abstraction
- Successful workload auth abstraction
- Authentication rejection
- Tenant mismatch
- Expired / unusable access context
- Authorization denial for one capability while others remain available
- No token leakage into normalized domain / findings / reports / logs
- Cancellation
- Insecure fallback prevention
- Authentication / collection failure separation

Core tests should use synthetic / fake auth contexts. No real credential or production tenant is required for unit testing. Do not create tests in this phase.

---

## 18. TBDs

The following remain TBD and MUST NOT be resolved by speculation:

- Exact OAuth / OIDC flows
- Concrete Microsoft identity library / SDK
- Exact Graph permissions
- Delegated vs. application permission mapping
- Supported interactive mechanisms
- Supported workload mechanisms
- Token acquisition implementation
- Token cache policy
- Local credential-storage policy, if ever required
- CI federation configuration
- Tenant-context validation implementation
- Consent / admin-consent UX
- Sovereign / national cloud authentication endpoints
- Retry / timeout policy
- Concrete `AuthorizedAccessContext` type

---

## 19. Invariant Alignment

This section cross-references architecture invariants. Do not modify `architecture-invariants.md`.

| Invariant | Alignment |
| --- | --- |
| INV-01 | Read-only tenant operation — authentication MUST NOT enable mutation |
| INV-03 | Provider isolation — authentication/authorization boundary separated from rule engine |
| INV-05 | Explicit evaluation states — authorization denial produces NOT_EVALUATED/ERROR, never silent PASS |
| INV-07 | Capability awareness — authorization evaluated per capability |
| INV-08 | Least privilege — minimum permissions for enabled capabilities only |
| INV-09 | Secret exclusion — tokens/secrets are runtime-only, excluded from all artifacts |
| INV-10 | Tenant boundary preservation — single-tenant per execution, no silent mixing |
| INV-14 | Failure transparency — authentication failures are explicit and distinguishable |
| INV-15 | No undocumented capability dependency — all permissions validated against official documentation |
| INV-16 | Security-sensitive defaults — secure behavior is default, weakening requires explicit opt-in |

---

## Open Items

- Exact OAuth flows and token acquisition strategies.
- Specific Microsoft Graph permission list with rationale (pending collector design).
- Credential source abstraction interface design.
- Token refresh and caching strategy.
- Multi-tenant token handling strategy.
- Whether EntraNHI validates permission level before beginning collection.
- Exact `AuthorizedAccessContext` implementation type.
- Tenant-context validation implementation.
