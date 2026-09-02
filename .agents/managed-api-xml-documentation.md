# Managed API XML documentation warnings (2026-08-31)

## Objective and status

Added missing English XML documentation for the user's build log: 77 CS1591 warnings,
including 9 repeated Core diagnostics across target frameworks. All reported members
are covered in 16 source files. Restore and compilation are pending explicit approval.

## Decisions and affected files

- Abstractions: AnalyticsEvent.cs and CrashEvent.cs document positional record parameters;
  IAnalyticsSource.cs, IAnalyticsLoggerSource.cs, and ICrashEventLoggerSource.cs document
  public contracts and source identity/initialization properties.
- Core: AnalyticsBuilder.cs, AnalyticsInstance.cs, AnalyticsSourceOperationException.cs,
  IAnalyticsInstance.cs, and Extensions/AnalyticsDependencyExtensions.cs document public
  types and the previously undocumented builder and exception properties.
- Firebase.ManagedApple: FirebaseAnalyticsLoggerSource.cs and FirebaseCrashEventLoggerSource.cs
  document public types and disposal, inherit source contracts, and explicitly state that
  identity, initialization, and reporting still throw NotImplementedException.
- Microsoft.Extensions.Logging: AnalyticsLogEntryContext.cs documents positional parameters;
  AnalyticsLoggerProvider.cs documents the provider and diagnostics (failures can exceed
  entry count); AnalyticsLoggingOptions.cs documents defaults, routing, metadata and limits;
  Extensions/AnalyticsLoggingDependencyExtensions.cs documents the registration type.
- No signatures, runtime statements, project settings, or warning suppression were changed.
  Existing unrelated formatting and implementation issues were left untouched.

## Checks and next step

- User performed the pull; subsequent git fetch succeeded and upstream was aligned.
- Working tree was initially clean. Manually reviewed the source diff and supplied warning
  list, including defaults and routing against AnalyticsLoggerProvider implementation.
- No restore, build, or automated checks run. Asked separately for explicit restore/build
  authorization as required by AGENTS.md; approval is pending.
- Next: restore and build the .NET solution when authorized, checking for zero CS1591
  diagnostics and any XML documentation compiler errors. Apple native runtime validation
  and completing ManagedApple implementations are outside this documentation-only task.
