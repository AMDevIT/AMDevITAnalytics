import Foundation

/// Consent values exposed to Objective-C without depending on Firebase types.
@objc public enum AnalyticsConsentStatus: Int {
    /// Leaves the previously configured value unchanged.
    case unchanged = 0
    /// Grants consent for the selected category.
    case granted = 1
    /// Denies consent for the selected category.
    case denied = 2
}
