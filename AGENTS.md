# EntraNHI Agent Security Policy

This repository uses AI-assisted development under explicit human control.

## Scope

AI coding agents must operate only on tasks explicitly scoped by the project owner.

Do not autonomously redesign architecture, expand project scope, perform unrelated refactoring, inspect unrelated directories, or introduce new external dependencies without explicit approval.

## Secrets and sensitive data

Never read, request, expose, generate, copy, log, commit, or transmit:

- passwords;
- access or refresh tokens;
- client secrets;
- private keys;
- credential-bearing certificates;
- recovery codes;
- real production credentials;
- employer or customer confidential information;
- production tenant exports;
- personal data not explicitly required for a sanitized test case; or
- secret-bearing local configuration.

Do not inspect files excluded as secrets or local configuration merely because they exist on the workstation.

Use sanitized placeholders, synthetic test data, mocks, fixtures, and documented environment-variable names instead.

## Repository boundary

Work only inside this repository unless the project owner explicitly authorizes another path.

Do not inspect sibling repositories, user-profile data, SSH directories, credential stores, browser data, cloud credential caches, or unrelated workstation files.

## Non-destructive change policy

Prefer additive and reversible changes.

Do not delete or remove existing:

- source code;
- files or directories;
- configuration;
- tests;
- documentation;
- branches or tags;
- Git history;
- repositories;
- data;
- cloud resources; or
- security controls

without explicit approval from the project owner.

If deletion or another destructive modification appears necessary, stop and explain:

1. exactly what would be removed or overwritten;
2. why it is necessary;
3. the expected impact; and
4. how the previous state can be recovered.

Wait for explicit approval before proceeding.

Never force-push, rewrite shared Git history, delete branches, delete tags, or delete the repository without explicit approval.

## Security architecture

Preserve these principles unless an explicitly approved architecture decision changes them:

- read-only security assessment by default;
- least privilege;
- deterministic security verdicts;
- evidence-based findings;
- explicit authorization boundaries;
- explicit handling of unavailable data; and
- no AI-generated security verdicts.

## Git operations

Do not commit or push automatically unless explicitly instructed.

Before proposing a commit, report the files changed and relevant validation/test results.

Never bypass repository security controls, secret scanning, signing requirements, branch protection, or required checks.
