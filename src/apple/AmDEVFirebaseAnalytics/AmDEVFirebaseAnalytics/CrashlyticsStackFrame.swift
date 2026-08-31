import Foundation

/// A symbolicated stack frame supplied by the caller's runtime.
@objc public final class CrashlyticsStackFrame: NSObject {

    // MARK: - Properties

    /// The fully qualified method or function name.
    @objc public let symbol: String

    /// The source file name; use an empty string when unavailable.
    @objc public let file: String

    /// The source line number; use zero when unavailable.
    @objc public let line: Int

    // MARK: - Initialization

    /// Creates a frame without exposing Firebase types to the binding.
    @objc public init(symbol: String, file: String, line: Int) {
        self.symbol = symbol
        self.file = file
        self.line = line
        super.init()
    }
}
