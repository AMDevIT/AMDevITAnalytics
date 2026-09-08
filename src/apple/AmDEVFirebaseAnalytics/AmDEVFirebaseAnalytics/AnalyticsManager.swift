//
//  AmDEVFirebaseAnalytics.swift
//  AmDEVFirebaseAnalytics
//
//  Created by Alessandro Morvillo on 30/08/2026.
//

import Foundation
import FirebaseAnalytics

/// Exposes Firebase Analytics through an Objective-C-compatible API.
/// Configure Firebase before using this manager.
@objc public class AnalyticsManager
    : NSObject {

    // MARK: - Properties

    private let backend: AnalyticsBackend

    // MARK: - Initialization

    public override init() {
        self.backend = .live
        super.init()
    }

    internal init(backend: AnalyticsBackend) {
        self.backend = backend
        super.init()
    }

    // MARK: - Methods

    /// Records an event using Firebase's event-name and parameter restrictions.
    @objc public func logEvent(name: String, parameters: [String: Any]? = nil) {
        self.backend.logEvent(name, parameters)
    }

    /// Associates events with an identifier, or removes it when nil.
    @objc public func setUserID(_ userID: String?) {
        self.backend.setUserID(userID)
    }

    /// Sets a user property, or removes it when the value is nil.
    @objc public func setUserProperty(_ value: String?, forName name: String) {
        self.backend.setUserProperty(value, name)
    }

    /// Enables or disables Analytics collection; Firebase persists this setting.
    @objc public func setAnalyticsCollectionEnabled(_ enabled: Bool) {
        self.backend.setCollectionEnabled(enabled)
    }

    /// Updates the specified consent categories without changing omitted categories.
    @objc public func setConsent(analyticsStorage: AnalyticsConsentStatus = .unchanged,
                                 adStorage: AnalyticsConsentStatus = .unchanged,
                                 adUserData: AnalyticsConsentStatus = .unchanged,
                                 adPersonalization: AnalyticsConsentStatus = .unchanged) {
        let settings: [(ConsentType, AnalyticsConsentStatus)] = [(.analyticsStorage, analyticsStorage),
                                                                (.adStorage, adStorage),
                                                                (.adUserData, adUserData),
                                                                (.adPersonalization, adPersonalization)]
        var consent = [ConsentType: ConsentStatus]()

        for (type, status) in settings {
            switch status {
            case .unchanged:
                continue
            case .granted:
                consent[type] = .granted
            case .denied:
                consent[type] = .denied
            }
        }

        if !consent.isEmpty {
            self.backend.setConsent(consent)
        }
    }

    /// Merges default parameters; NSNull removes a key and nil clears all defaults.
    @objc public func setDefaultEventParameters(_ parameters: [String: Any]?) {
        self.backend.setDefaultParameters(parameters)
    }

    /// Clears local Analytics data and resets the app instance identifier.
    @objc public func resetAnalyticsData() {
        self.backend.resetData()
    }

    /// Sets the inactivity timeout in seconds.
    @objc public func setSessionTimeoutInterval(_ interval: TimeInterval) {
        self.backend.setSessionTimeout(interval)
    }

    /// Retrieves the session identifier or an error on Firebase's callback queue.
    /// NSNumber preserves the Int64 identifier while allowing nil on failure.
    @objc public func sessionID(completion: @escaping (NSNumber?, NSError?) -> Void) {
        self.backend.sessionID { sessionID, error in
            if let error = error {
                completion(nil, error as NSError)
            } else {
                completion(NSNumber(value: sessionID), nil)
            }
        }
    }

    /// Returns the app instance identifier, or nil when unavailable.
    @objc public func appInstanceID() -> String? {
        return self.backend.appInstanceID()
    }
}
