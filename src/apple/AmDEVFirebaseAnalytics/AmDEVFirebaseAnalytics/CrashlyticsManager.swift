//
//  Crashlytics.swift
//
//
//  Created by Alessandro Morvillo on 30/08/2026.
//

import Foundation
import FirebaseCrashlytics

/// Exposes Firebase Crashlytics through an Objective-C-compatible API.
/// Configure Firebase before using this manager.
@objc public class CrashlyticsManager
    : NSObject {

    // MARK: - Properties

    private let backend: CrashlyticsBackend

    // MARK: - Initialization

    public override init() {
        self.backend = .live
        super.init()
    }

    internal init(backend: CrashlyticsBackend) {
        self.backend = backend
        super.init()
    }

    // MARK: - Methods

    /// Adds diagnostic context to reports; this does not record an error itself.
    @objc public func log(message: String) {
        self.backend.log(message)
    }

    /// Records a non-fatal native error.
    @objc public func record(error: NSError) {
        self.backend.recordError(error)
    }

    /// Records a non-fatal native error with event-specific metadata.
    @objc public func record(error: NSError, userInfo: [String: Any]?) {
        self.backend.recordErrorWithInfo(error, userInfo)
    }

    /// Records an exception from another runtime, such as .NET.
    /// Supply frames in order from the throw site to the outermost caller.
    @objc public func recordException(name: String,
                                      reason: String,
                                      stackTrace: [CrashlyticsStackFrame]) {
        let exception = self.backend.makeException(name, reason)
        exception.stackTrace = stackTrace.map { self.backend.makeFrame($0.symbol, $0.file, $0.line) }
        self.backend.recordException(exception)
    }

    /// Sets diagnostic context shared by subsequent reports.
    @objc public func setCustomValue(_ value: Any?, forKey key: String) {
        self.backend.setCustomValue(value, key)
    }

    /// Merges diagnostic context shared by subsequent reports.
    @objc public func setCustomKeysAndValues(_ values: [String: Any]) {
        self.backend.setCustomKeys(values)
    }

    /// Associates reports with an identifier, or clears it when nil.
    @objc public func setUserID(_ userID: String?) {
        self.backend.setUserID(userID)
    }

    /// Sets Firebase's persisted automatic-collection override.
    /// Configure Info.plist to disable collection from the first app launch.
    @objc public func setCrashlyticsCollectionEnabled(_ enabled: Bool) {
        self.backend.setCollectionEnabled(enabled)
    }

    /// Returns Firebase's automatic-collection setting.
    @objc public func isCrashlyticsCollectionEnabled() -> Bool {
        return self.backend.isCollectionEnabled()
    }

    /// Reports whether the previous execution ended in a crash.
    @objc public func didCrashDuringPreviousExecution() -> Bool {
        return self.backend.didCrash()
    }

    /// Checks for pending reports when automatic collection is disabled.
    /// Call once per launch; Firebase controls the callback queue and timing.
    @objc public func checkForUnsentReports(completion: @escaping (Bool) -> Void) {
        self.backend.checkReports(completion)
    }

    /// Requests upload of pending reports when automatic collection is disabled.
    @objc public func sendUnsentReports() {
        self.backend.sendReports()
    }

    /// Deletes pending local reports when automatic collection is disabled.
    @objc public func deleteUnsentReports() {
        self.backend.deleteReports()
    }
}
