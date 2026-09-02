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

    // MARK: - Methods

    /// Adds diagnostic context to reports; this does not record an error itself.
    @objc public func log(message: String) {
        Crashlytics.crashlytics().log(message)
    }

    /// Records a non-fatal native error.
    @objc public func record(error: NSError) {
        Crashlytics.crashlytics().record(error: error)
    }

    /// Records a non-fatal native error with event-specific metadata.
    @objc public func record(error: NSError, userInfo: [String: Any]?) {
        Crashlytics.crashlytics().record(error: error, userInfo: userInfo)
    }

    /// Records an exception from another runtime, such as .NET.
    /// Supply frames in order from the throw site to the outermost caller.
    @objc public func recordException(name: String,
                                      reason: String,
                                      stackTrace: [CrashlyticsStackFrame]) {
        let exception = ExceptionModel(name: name, reason: reason)
        exception.stackTrace = stackTrace.map { StackFrame(symbol: $0.symbol, file: $0.file, line: $0.line) }
        Crashlytics.crashlytics().record(exceptionModel: exception)
    }

    /// Sets diagnostic context shared by subsequent reports.
    @objc public func setCustomValue(_ value: Any?, forKey key: String) {
        Crashlytics.crashlytics().setCustomValue(value, forKey: key)
    }

    /// Merges diagnostic context shared by subsequent reports.
    @objc public func setCustomKeysAndValues(_ values: [String: Any]) {
        Crashlytics.crashlytics().setCustomKeysAndValues(values)
    }

    /// Associates reports with an identifier, or clears it when nil.
    @objc public func setUserID(_ userID: String?) {
        Crashlytics.crashlytics().setUserID(userID)
    }

    /// Sets Firebase's persisted automatic-collection override.
    /// Configure Info.plist to disable collection from the first app launch.
    @objc public func setCrashlyticsCollectionEnabled(_ enabled: Bool) {
        Crashlytics.crashlytics().setCrashlyticsCollectionEnabled(enabled)
    }

    /// Returns Firebase's automatic-collection setting.
    @objc public func isCrashlyticsCollectionEnabled() -> Bool {
        return Crashlytics.crashlytics().isCrashlyticsCollectionEnabled()
    }

    /// Reports whether the previous execution ended in a crash.
    @objc public func didCrashDuringPreviousExecution() -> Bool {
        return Crashlytics.crashlytics().didCrashDuringPreviousExecution()
    }

    /// Checks for pending reports when automatic collection is disabled.
    /// Call once per launch; Firebase controls the callback queue and timing.
    @objc public func checkForUnsentReports(completion: @escaping (Bool) -> Void) {
        Crashlytics.crashlytics().checkForUnsentReports(completion: completion)
    }

    /// Requests upload of pending reports when automatic collection is disabled.
    @objc public func sendUnsentReports() {
        Crashlytics.crashlytics().sendUnsentReports()
    }

    /// Deletes pending local reports when automatic collection is disabled.
    @objc public func deleteUnsentReports() {
        Crashlytics.crashlytics().deleteUnsentReports()
    }
}
