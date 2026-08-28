# Agent Instructions

These instructions apply to the entire repository.

## Project Context

- The main project is a cross-platform .NET library that supports analytics libraries from various vendors, primarily Google Firebase.
- It is located in the `src\dotnet\AMDevIT.Analytics` subfolder.
- The files for the iOS and Mac Catalyst framework project are located in the `sources\apple` folder.
- The files for the Android library are located in the `sources\droid` folder.

## Workflow

Try always to minimize the use of token if it doesn't doesn't lower the quality of the answer.

Before everything, fetch from the repository and if pulls are ask for confirmation for executing the pulls. 
If the user agree, execute the pull. If merge of files is required, analyze a merge plan and if confirmed by the user, execute the merge. 
If not confirmed, close the request until the user manage to merge the files manually.

1. Before starting, read the `.md` files in the `.agents` subfolder.
2. Preserve the user's changes and do not modify files unrelated to the task.
3. Always analyze the requested work first and ask for confirmation.
4. Prefer small, targeted changes that are consistent with the existing architecture. When the requested change is extensive, warn the user and assess the risks.
5. After each change, run the relevant checks whenever the environment allows it.
6. At the end of a significant task, update the `.agents` subfolder with additional `.md` files containing the context for the current step, then update `.agents/context.md` with:
   - objective and status;
   - decisions made;
   - affected files;
   - checks performed and their results;
   - open issues and the recommended next step.

# Project Coding Conventions

Apply these conventions to all code added to or modified in this repository.

## Comments

- Write all code comments in English.

## Class Member Organization for C#

Order class members as follows:

1. Constants
2. Events
3. Private fields
4. Properties
5. Constructors
6. Public methods
7. Protected methods
8. Private methods
9. Event handlers

Group each applicable category in a `#region`/`#endregion` block, using exactly the following region names:

- `Const` for constants
- `Events` for events
- `Fields` for private fields
- `Properties` for properties
- `.ctor` for constructors
- `Methods` for all methods, with public methods first, followed by protected methods and then private methods
- `Event handlers` for all event handlers

Do not create empty regions when a class does not contain members belonging to the corresponding category.

## Variable Declarations for C#

Whenever possible, and especially when this does not interfere with `using` and `await using` directives, try to declare local variables at the beginning of their enclosing scope. This applies to C#, while for Swift and Kotlin doesn't apply becausae of peculiar languages syntax like, for example, guard blocks or let blocks. For these two languages, follow the best practice allocation syntax.
Avoid using the `var` keyword as much as possible, unless it is necessary or provides a clear improvement in readability, such as when dealing with multiple nested generic types or very long class names.
Always use `this` before class fields and properties whenever possible, even when it is redundant and optional. Do not use underscores (`_`) when declaring class fields.
The rule is simple: inside methods, when a field or property belongs to the class definition, access it through `this`. When a variable is local to the method, reference it directly because `this` does not apply.
This makes it immediately clear whether a variable should be looked for in the current local scope or in the class declaration.

## Type Member Organization for Swift

Order type members as follows:

1. Constants
2. Stored properties
3. Computed properties
4. Initializers
5. Public methods
6. Internal methods
7. File-private methods
8. Private methods

Group each applicable category using Swift `// MARK: -` sections, using exactly the following section names:

- `Constants`
- `Properties`
- `Initialization`
- `Methods`

Do not create empty sections.
When protocol conformances or extensions improve readability, prefer grouping them into separate extensions.

## Variable Declarations for Swift

Follow standard Swift scoping and declaration practices.
Declare local variables as close as reasonably possible to the point where they are first needed.
Prefer `let` whenever a value does not need to change after initialization.
Use `var` only when mutation is required.
Prefer Swift's concise collection syntax:

```swift
var products = [StoreKitProduct]()
var productsByIdentifier = [String: StoreKitProduct]()
```

Avoid unnecessarily verbose forms such as:

```swift
var products = Array<StoreKitProduct>()
var productsByIdentifier = Dictionary<String, StoreKitProduct>()
```

## Instance Member Access for Swift

Use `self` when accessing instance properties from within instance methods whenever possible, even when Swift does not require it.
If a property belongs to the type instance, access it through `self`.
If a variable or constant belongs to the current local scope, access it directly.
Do not prefix stored properties with underscores solely to distinguish them from local variables or parameters.
Use a leading underscore only when it has a specific Swift or API-design purpose.

## Class Member Organization for Kotlin

Order class members as follows:

1. Constants
2. Properties
3. Initializers
4. Constructors
5. Public methods
6. Protected methods
7. Internal methods
8. Private methods

Group each applicable category using Kotlin region comments, using exactly the following region names:

- `Constants` for constants and companion object constants
- `Properties` for properties and backing fields
- `Initialization` for `init` blocks and secondary constructors
- `Methods` for all methods, with public methods first, followed by protected methods, internal methods and then private methods

Use the following syntax:

```kotlin
// region Constants

// endregion
```

Do not create empty regions when a class does not contain members belonging to the corresponding category.

When interface implementations, companion objects, nested types or extensions improve readability, keep them logically separated and consistent with the existing architecture.

## Variable Declarations for Kotlin

Follow standard Kotlin scoping and declaration practices.
Declare local variables as close as reasonably possible to the point where they are first needed.

Prefer `val` whenever a value does not need to change after initialization.
Use `var` only when mutation is required.

Prefer explicit types when they improve readability or make the intended API contract clearer, but otherwise use Kotlin type inference when the type is immediately obvious from the assigned expression.

Prefer Kotlin's concise collection factory syntax:

```kotlin
val products = mutableListOf<StoreProduct>()
val productsByIdentifier = mutableMapOf<String, StoreProduct>()
```

Avoid unnecessarily verbose Java-style collection initialization such as:

```kotlin
val products = ArrayList<StoreProduct>()
val productsByIdentifier = HashMap<String, StoreProduct>()
```

unless a specific implementation type is required.

Avoid nullable variables when the value can be initialized directly or expressed using Kotlin null-safety constructs such as `?.`, `?:`, `let`, `run`, `also` or `apply`.

Do not use `lateinit` unless delayed initialization is actually required and the property cannot reasonably be initialized in the constructor or at declaration time.

## Instance Member Access for Kotlin

Use `this` when accessing instance properties from within instance methods whenever it improves clarity, especially when a local variable or parameter has the same or a similar name.

If a property belongs to the class instance and ambiguity may exist, access it through `this`.

If a variable or constant belongs to the current local scope, reference it directly.

Do not prefix properties with underscores solely to distinguish them from local variables or parameters.

Use a leading underscore only when it has a specific Kotlin or API-design purpose, such as exposing a private mutable backing property through a public immutable property.

Example:

```kotlin
private val _items = MutableStateFlow<List<Item>>(emptyList())
val items = _items.asStateFlow()
```

Do not introduce backing properties when they are not required.

## Indentation

Always indent code correctly. Do not insert a line break immediately after an opening parenthesis. Always align function arguments, and try to keep short lambdas on a single line.
Each indentation level uses 4 spaces.

### Method and Constructor Call Formatting

These rules apply both to source files and to C# examples inside Markdown files.

- Never place the first argument on a new line after the opening parenthesis.
- When arguments span multiple lines, keep the first argument on the same line as the method or constructor call.
- Align every subsequent argument vertically with the first argument.
- Apply the same formatting to target-typed `new(...)` expressions.
- Known code formatting rules for C#, Swift and Kotlin must be applied if not against these rules.
- Before completing documentation changes, verify every multiline C#, Kotlin or Swift invocation against these rules.

Example:

```csharp
HttpCookieData sessionCookie = new("session-id",
                                   sessionId,
                                   domain: "api.example.com",
                                   path: "/",
                                   isSecure: true);
```

Examples:

```csharp
builder.Services.AddTransient<IMyInterface>(services => services.GetRequiredService<MyInterfaceImplementation>());

builder.Services.AddDbContextFactory<MyDataSQLiteDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
    options.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.RedundantIndexRemoved));
});

public sealed partial class DataSourcesPageViewModel(IMyInterface myInterface,
                                                     ILogger<DataSourcesPageViewModel> logger)
    : ViewModelBase(logger)
```

instead of:

```csharp
builder.Services.AddTransient<IMyInterface>(services => 
     services.GetRequiredService<MyInterfaceImplementation>());

   builder.Services.AddDbContextFactory<MyDataSQLiteDbContext>(options =>
   {
       options.UseSqlite($"Data Source={dbPath}");
       options.ConfigureWarnings(warnings =>
           warnings.Ignore(CoreEventId.RedundantIndexRemoved));
   });

   public sealed partial class DataSourcesPageViewModel(
    IMyInterface myInterface,
    ILogger<DataSourcesPageViewModel> logger)
    : ViewModelBase(logger)
```

## Build and Verification

If a build and verification is needed always ask first before use tokens in an unwanted manner.
If confirmation to build and verify is given, from the C# solution root, restore the solution packages using `dotnet restore` and build each solution using `dotnet`:

```powershell
dotnet restore sources\dotnet\AMDevIT.Admob.Wrapper
dotnet build sources\dotnet\AMDevIT.Admob.Wrapper
```

If, after you got the consent, a check cannot be performed, briefly record the reason in the progressive context and in the final summary.
