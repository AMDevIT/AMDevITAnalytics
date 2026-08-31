# Apple binding compilation (2026-08-31)

## Objective and status

Investigated the reported iOS binding build failure and removed two source-level
compilation blockers. The actual build diagnostic was not supplied; successful
compilation remains unverified.

## Decisions and affected files

- In src/dotnet/AMDevIT.Analytics/AMDevIT.Analytics.Firebase.BindingApple/ApiDefinition.cs,
  removed the stale AmDEVFirebaseAnalytics namespace import. The binding types and
  consent enum are declared in AMDevIT.Analytics.Firebase.BindingApple.
- Removed three unresolved Sharpie Verify(MethodToProperty) annotations after
  reviewing appInstanceID, isCrashlyticsCollectionEnabled, and
  didCrashDuringPreviousExecution against the bundled iOS arm64 Objective-C header.
  Kept their read-only property projections and native selectors unchanged.
- Did not change the native framework, target framework, or packaging configuration.

## Checks and next step

- Git fetch succeeded; HEAD and origin/Task-Apple-Library were aligned (0/0).
- The working tree was initially clean. Manually reviewed the source diff and header.
- No restore, build, or automated checks were run: repository instructions require
  explicit approval. This host runs Windows; full Apple validation needs macOS/Xcode.
- Next: obtain approval for restore/build where supported, and inspect the original
  build log if a failure remains. Native linking and runtime behavior are unverified.
