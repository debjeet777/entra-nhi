# 0002 — Tenant-context representation

> **Status:** Accepted for Phase 0.4 implementation
> **Scope:** EntraNHI.Core
> **Phase:** 0.4 — Stage 2

## Context

The locked design requires tenant context to be explicit on tenant-derived
data contracts. Tenant context prevents silent cross-tenant identity mixing
and is distinct from SaaS multi-tenancy.

Tenant-context mismatch or contamination is an integrity failure and must
fail closed and visibly. It must never be converted into an assessment
verdict or treated as an ordinary data gap.

The locked design deliberately leaves the concrete C# shape, tenant-context
identifier format, and generation strategy unresolved.

## Decision

Represent tenant assessment context in Core as an opaque, provider-independent
value object.

The value object will:

- carry one non-empty opaque value used only for tenant assessment scoping;
- use value equality so independently constructed equal contexts compare equal;
- distinguish different opaque values as different tenant contexts;
- contain no provider SDK/API type;
- contain no credential, token, secret, or authentication material;
- make no claim that its value is globally unique outside its assessment
  and integration context.

Tenant-derived Core contracts introduced by later approved implementation
steps must carry this context explicitly rather than obtaining tenant state
from ambient/static/global state.

## Constraints preserved

This decision does not define:

- a Microsoft Entra tenant-ID representation;
- a GUID, URI, issuer, domain, or other provider identifier format;
- how a provider tenant identifier maps into this value;
- tenant-context generation strategy;
- authentication or authorization behavior;
- tenant discovery or validation against a live provider;
- cross-tenant failure/result-envelope mechanics;
- serialization or wire format;
- SaaS multi-tenancy.

Those remain governed by their existing owning documents and unresolved TBDs.

## Verification

Stage-2 Core tests will use synthetic tenant-context values only and verify:

1. equal opaque values produce equal tenant contexts;
2. different opaque values produce different tenant contexts;
3. empty context values are rejected;
4. tenant context requires no provider/network/authentication dependency.

Cross-tenant graph, evidence, findings, runtime contamination, and provider
mapping tests remain owned by their later implementation stages.

## Security rationale

An opaque Core value prevents provider-specific tenant semantics from leaking
into the domain model while making tenant scoping structurally explicit.
It therefore supports tenant-isolation enforcement without resolving the
provider mapping or identifier-format TBD prematurely.
