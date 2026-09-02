import Foundation
import FirebaseCrashlytics

/// Keeps singleton lookup at call time, while allowing tests without a Firebase app.
struct CrashlyticsBackend {
    // MARK: - Properties

    var log: (String) -> Void
    var recordError: (NSError) -> Void
    var recordErrorWithInfo: (NSError, [String: Any]?) -> Void
    var makeException: (String, String) -> ExceptionModel
    var makeFrame: (String, String, Int) -> StackFrame
    var recordException: (ExceptionModel) -> Void
    var setCustomValue: (Any?, String) -> Void
    var setCustomKeys: ([String: Any]) -> Void
    var setUserID: (String?) -> Void
    var setCollectionEnabled: (Bool) -> Void
    var isCollectionEnabled: () -> Bool
    var didCrash: () -> Bool
    var checkReports: (@escaping (Bool) -> Void) -> Void
    var sendReports: () -> Void
    var deleteReports: () -> Void

    static var live: CrashlyticsBackend {
        CrashlyticsBackend(log: { Crashlytics.crashlytics().log($0) },
                           recordError: { Crashlytics.crashlytics().record(error: $0) },
                           recordErrorWithInfo: { Crashlytics.crashlytics().record(error: $0, userInfo: $1) },
                           makeException: { ExceptionModel(name: $0, reason: $1) },
                           makeFrame: { StackFrame(symbol: $0, file: $1, line: $2) },
                           recordException: { Crashlytics.crashlytics().record(exceptionModel: $0) },
                           setCustomValue: { Crashlytics.crashlytics().setCustomValue($0, forKey: $1) },
                           setCustomKeys: { Crashlytics.crashlytics().setCustomKeysAndValues($0) },
                           setUserID: { Crashlytics.crashlytics().setUserID($0) },
                           setCollectionEnabled: { Crashlytics.crashlytics().setCrashlyticsCollectionEnabled($0) },
                           isCollectionEnabled: { Crashlytics.crashlytics().isCrashlyticsCollectionEnabled() },
                           didCrash: { Crashlytics.crashlytics().didCrashDuringPreviousExecution() },
                           checkReports: { Crashlytics.crashlytics().checkForUnsentReports(completion: $0) },
                           sendReports: { Crashlytics.crashlytics().sendUnsentReports() },
                           deleteReports: { Crashlytics.crashlytics().deleteUnsentReports() })
    }
}
