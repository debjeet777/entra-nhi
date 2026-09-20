# EntraNHI

Open-source security analysis for Microsoft Entra non-human identities, workload identities, and AI agents.

> **Project status:** Early-stage design and implementation. Not production-ready.

EntraNHI is an independent open-source project focused on helping security and identity engineers understand and assess non-human identities in Microsoft Entra.

The project is being developed around evidence-driven, read-only V1 security assessment with deterministic evaluation and clear handling of unavailable or insufficient telemetry.

## Planned scope

EntraNHI is intended to analyze areas including:

- Application registrations
- Service principals
- Workload and managed identities
- Credentials and credential lifecycle
- Ownership and accountability
- Microsoft Graph and API permissions
- Identity relationships and dependencies
- Stale or potentially unnecessary identities
- Emerging Microsoft Entra AI-agent identities

## Design principles

- Read-only V1 assessment with no write or remediation mode
- Security-first architecture
- Deterministic security rules
- Evidence attached to findings
- Explicit handling of unavailable data
- Least-privilege Microsoft Graph access
- Modular collectors and analyzers
- Machine-readable output
- No AI-generated security verdicts

## Development

Detailed requirements, architecture, threat modeling, security controls, and implementation-design documentation are present, and implementation is in early progress. The project remains early-stage and is not production-ready. Documentation describing intended architecture and controls is not proof that the corresponding security behavior has been implemented or verified.

## Independence

EntraNHI is an independent open-source project and is not affiliated with, endorsed by, or sponsored by Microsoft.

Microsoft, Microsoft Entra, Azure, and Microsoft Graph are trademarks of the Microsoft group of companies.
