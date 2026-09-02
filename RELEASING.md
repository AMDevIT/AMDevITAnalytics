# Releasing AMDev.IT Analytics

Releases are manual. This document does not assert that a build, test, package, or
publication has already succeeded. The work that introduced Catalyst targets, resources,
and tests intentionally did not run restore or build.

## Version and packages

The six public packages share the version in
`src/dotnet/AMDevIT.Analytics/Directory.Build.props`, currently `0.0.6`:

- AMDevIT.Analytics.Abstractions
- AMDevIT.Analytics.Core
- AMDevIT.Analytics.Firebase.BindingApple
- AMDevIT.Analytics.Firebase.ManagedApple
- AMDevIT.Analytics.Firebase.ManagedDroid
- AMDevIT.Analytics.Microsoft.Extensions.Logging

Do not package the three projects under `Tests`; they declare `IsPackable=false`.
Use **AMDev.IT Analytics** in release notes and documentation. Keep existing package
IDs, namespaces, and the GitHub repository name **AMDevITAnalytics** unchanged.

Choose the next version explicitly. The shared props concatenate `VersionPrefix` and
`VersionSuffix` directly: for example, `0.0.7` and `-preview.1` produce
`0.0.7-preview.1`. Keep dependency versions aligned and update README status.
Do not reuse a version already published to the target feed.

## Release gates

1. Review the complete diff and commit identity. Record .NET SDK/workload, Xcode, Android
   SDK/JDK, and pinned Firebase dependency versions. Use toolchains compatible with the
   project and its dependencies; there is no repository-provided CI release gate.
2. On macOS, run the [Apple build procedure](src/apple/AmDEVFirebaseAnalytics/BUILDING.md)
   with the committed Package.resolved. Keep Firebase inside the Xcode/SPM wrapper;
   do not introduce an independently linked Firebase runtime for this release.
3. Replace the entire `BindingApple/libs/AmDEVFirebaseAnalytics.xcframework` with the
   generated artifact, including wrapper dSYMs and Catalyst symlinks. Do not overlay new
   slices onto an old directory. Review the public Objective-C header against
   `ApiDefinition.cs` and `StructsAndEnums.cs`; internal test seams must not change
   the public names/selectors. Check actual minimum OS versions in the binaries.
4. Pass the [privacy/resource audit](src/apple/AmDEVFirebaseAnalytics/PRIVACY.md).
   The previously committed XCFramework has no manifests, resource bundles, or dSYMs;
   it must not be published unchanged. Run the resource verifier on each slice.
   Compare dSYM UUIDs to their matching binaries with `dwarfdump --uuid`.
5. Restore and build the .NET solution in Release with the required Android, iOS, and
   Mac Catalyst workloads. Build each declared TFM, including plain net10.0 and
   net10.0-maccatalyst; inspect warnings, generated XML docs, and trimming diagnostics.
6. Run all suites and the real-Firebase host checks in [TESTING.md](TESTING.md).
   Record test reports separately for desktop, Android, iOS Simulator, Catalyst,
   and physical-device Release integration. Fake-backend tests do not verify uploads.
7. Pack the six library projects into a fresh output directory. Inspect all packages
   before publication and then install them into consumer apps from a local feed,
   without project references.
8. Repeat startup, Analytics events, non-fatal managed exception reporting, the README
   unobserved-task handler, collection/consent controls, cancellation, and lifecycle
   checks in those package-only consumers. Validate signing, native resources,
   symbolication, and the app archive's privacy report.
9. Review release notes, README rendering, dependency licenses/notices, native Firebase
   coexistence limitations, and remaining defects. Publish only the exact validated
   files after explicit release authorization. Tag the exact source/artifact commit
   with the chosen version after publication succeeds.

## Restore, build, and pack

The following commands are for an authorized release run from the repository root.
They have not been executed as part of this change.

```powershell
dotnet restore src/dotnet/AMDevIT.Analytics/AMDevIT.Analytics.slnx
dotnet build src/dotnet/AMDevIT.Analytics/AMDevIT.Analytics.slnx -c Release --no-restore
```

Run desktop tests and platform apps as described in TESTING.md before packing.
Use a new output directory for every candidate so old packages cannot be mistaken
for the current result. After the corresponding Release builds have succeeded:

```powershell
$solutionRoot = Join-Path (Get-Location) 'src/dotnet/AMDevIT.Analytics'
$packageOutput = Join-Path (Get-Location) ('artifacts/release-' + [Guid]::NewGuid().ToString('N'))
$libraryProjects = @(
    'AMDevIT.Analytics.Abstractions',
    'AMDevIT.Analytics.Core',
    'AMDevIT.Analytics.Firebase.BindingApple',
    'AMDevIT.Analytics.Firebase.ManagedApple',
    'AMDevIT.Analytics.Firebase.ManagedDroid',
    'AMDevIT.Analytics.Microsoft.Extensions.Logging'
)

foreach ($project in $libraryProjects)
{
    dotnet pack (Join-Path $solutionRoot "$project/$project.csproj") -c Release --no-build --no-restore -o $packageOutput
    if ($LASTEXITCODE -ne 0) { throw "Packing failed: $project" }
}
```

Do not infer success from the presence of files alone. Keep restore/build/test logs,
artifact checksums, .nupkg/.snupkg files, native archives, and dSYMs for the candidate.

## Package and consumer inspection

- Expect one .nupkg and the intended symbol package per public library, all with the
  chosen version. Shared `IncludeSymbols` uses snupkg; native dSYMs are separate
  assets and must be retained with the XCFramework/release archives.
- Check package metadata: ID/version, Apache-2.0 expression, author/company,
  repository URL/commit, XML documentation, root README.md, and nuget_icon_128.png.
- Check dependency groups for all declared TFMs. BindingApple and ManagedApple must
  support both net10.0-ios and net10.0-maccatalyst; Core must additionally support
  net10.0 and net10.0-android. Do not claim an optional Firebase runtime dependency.
- Inspect the binding package's native resource payload (including any SDK-generated
  binding resource archive), not just its managed assembly. Confirm all expected
  slices, headers, modules, privacy files, vendor bundles, and symbol paths survive.
- In each final .app, inspect the embedded framework's resources and native dependency
  load commands. Verify that the wrapper is signed/embedded correctly and no additional
  runtime dylib is accidentally missing.
- Configure GoogleService-Info.plist or the Android host Firebase setup in the consumer.
  Configure Crashlytics uploads for the final app's managed/native symbols and retain
  the matching wrapper dSYMs. Unit-test apps deliberately contain no credentials.
- Do not claim compatibility with another embedded Firebase implementation based only
  on successful linking. Test the exact host and document any unsupported combination.
- Do not include test credentials, private service configuration, signing material,
  or NuGet API keys in packages, logs, or the repository.

Publication commands and secrets are deliberately not embedded in this document.
A stable release requires a validated consumer on every advertised platform and no
unresolved release-blocking packaging or runtime failures.
