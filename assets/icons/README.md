# Package icons

Shared visual identity for all AMDevIT.Analytics packages: an analytics chart and signal line combined with an abstract A. The original artwork does not use third-party platform logos.

| File | Purpose |
| --- | --- |
| `nuget_icon_128.png` | 128 × 128 PNG embedded in each NuGet package. |
| `nuget_icon_256.png` | 256 × 256 PNG for higher-resolution presentation. |
| `nuget_icon_512.png` | 512 × 512 PNG for documentation and promotional use. |
| `analytics-original.png` | Original generated artwork, retained for future exports. |

The PNG exports preserve the original transparency. Only the 128-pixel file is packed, at the package root as `nuget_icon_128.png`, using the shared `Directory.Build.props` configuration. NuGet recommends 128 × 128 pixels and limits package icons to 1 MB; see the [official reference](https://learn.microsoft.com/en-us/nuget/reference/nuspec#icon).

## Generation provenance

Created on 2026-08-31 using the built-in ImageGen tool, without the API/CLI fallback. Resolution variants were exported with Windows System.Drawing using bicubic resampling; the artwork was not redrawn.

Final generation prompt:

```text
Use case: logo-brand
Asset type: shared NuGet package icon for AMDevIT.Analytics, a .NET analytics and crash-reporting library.
Primary request: Create one polished original square icon with a bold abstract A formed from an ascending analytics chart and a connected signal line, readable at 32 and 128 pixels. Single centered mark, generous safe margins, crisp simple geometry, flat vector-like raster rendering. Tasteful professional developer-tool identity. Use a deep navy rounded-square tile with a vivid cyan-to-blue analytics mark and a small warm amber signal point. Transparent canvas outside the rounded-square tile. No text, no letters rendered as typography, no tiny details, no shadows, no 3D, no watermark. Do not include Firebase, Apple, Android, Microsoft or NuGet logos. Deliver just one icon, not a contact sheet or mockup, square PNG.
```
