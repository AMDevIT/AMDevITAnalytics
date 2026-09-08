import Foundation
import FirebaseAnalytics

/// Internal injection boundary. Constructing it never configures Firebase.
struct AnalyticsBackend {
    // MARK: - Properties

    var logEvent: (String, [String: Any]?) -> Void
    var setUserID: (String?) -> Void
    var setUserProperty: (String?, String) -> Void
    var setCollectionEnabled: (Bool) -> Void
    var setConsent: ([ConsentType: ConsentStatus]) -> Void
    var setDefaultParameters: ([String: Any]?) -> Void
    var resetData: () -> Void
    var setSessionTimeout: (TimeInterval) -> Void
    var sessionID: (@escaping (Int64, Error?) -> Void) -> Void
    var appInstanceID: () -> String?

    static var live: AnalyticsBackend {
        AnalyticsBackend(logEvent: { Analytics.logEvent($0, parameters: $1) },
                         setUserID: { Analytics.setUserID($0) },
                         setUserProperty: { Analytics.setUserProperty($0, forName: $1) },
                         setCollectionEnabled: { Analytics.setAnalyticsCollectionEnabled($0) },
                         setConsent: { Analytics.setConsent($0) },
                         setDefaultParameters: { Analytics.setDefaultEventParameters($0) },
                         resetData: { Analytics.resetAnalyticsData() },
                         setSessionTimeout: { Analytics.setSessionTimeoutInterval($0) },
                         sessionID: { Analytics.sessionID(completion: $0) },
                         appInstanceID: { Analytics.appInstanceID() })
    }
}
