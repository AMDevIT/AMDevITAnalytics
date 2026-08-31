# Managed Apple Firebase implementation (2026-08-31)

## Objective and status

Implemented the managed iOS Firebase adapter over the existing native binding. Source review is complete; restore,
build, native linking, and runtime validation were explicitly excluded by the user.

## Decisions and affected files

- Added a shared, idempotent `FirebaseApple.Initialize` entry point. It either configures the default app once on
  the required main thread or adopts an app already configured by the host. Lazy source initialization reports a
  clear error if first-time configuration is attempted from a background thread.
- Implemented Analytics event reporting, Foundation parameter conversion, user controls, consent, default
  parameters, data reset, session timeout and retrieval, and app/session identifiers.
- Implemented managed and native error recording, managed stack-frame projection, custom context, user and
  collection controls, previous-crash state, and pending-report check/send/delete operations.
- Added DI extensions matching the Android surface and references to Core and BindingApple.
- Serialized source initialization, manager access, and disposal. Disposal releases wrapper managers without
  attempting to shut down the shared native Firebase app.
- Updated README Apple status, initialization instructions, supported parameter behavior, and release caveats.
- Affected: all files under `AMDevIT.Analytics.Firebase.ManagedApple`, its project file, README.md, and this context.

## Checks, limitations, and next step

- Git fetch succeeded; the branch remained aligned with upstream (0/0), and the working tree was clean initially.
- Reviewed generated binding signatures, Foundation reference metadata, project XML, source formatting, API coverage,
  conversion ownership, cancellation semantics, concurrency, disposal, and `git diff --check`.
- No restore, build, tests, or MSBuild command was run, per the user's explicit instruction.
- Verify on macOS/iOS next: compile, link the XCFramework and Firebase dependencies, configure from both ownership
  modes, exercise callbacks and parameter types, inspect Crashlytics symbolication, and test release trimming.
- The checked-in managed target remains iOS only. Native packaging, privacy resources, signing, dSYM upload, and
  Firebase coexistence with other host integrations remain release integration responsibilities.
