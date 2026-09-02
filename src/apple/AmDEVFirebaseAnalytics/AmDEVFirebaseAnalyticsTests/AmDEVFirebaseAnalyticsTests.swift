import Foundation
import FirebaseAnalytics
import FirebaseCrashlytics
import Testing
@testable import AmDEVFirebaseAnalytics

struct AmDEVFirebaseAnalyticsTests {
    // MARK: - Methods

    @Test func analyticsPreservesEventPayloadAndNil() {
        var backend = AnalyticsBackend.live
        var names = [String]()
        var payloads = [[String: Any]?]()
        backend.logEvent = { names.append($0); payloads.append($1) }
        let manager = AnalyticsManager(backend: backend)
        manager.logEvent(name: "purchase", parameters: ["value": 12, "items": [["item_id": "sku"]]])
        manager.logEvent(name: "open")
        #expect(names == ["purchase", "open"])
        #expect(payloads[0]?["value"] as? Int == 12)
        #expect((payloads[0]?["items"] as? [[String: String]])?.first?["item_id"] == "sku")
        #expect(payloads[1] == nil)
    }

    @Test func analyticsIdentityCollectionDefaultsAndReset() {
        var backend = AnalyticsBackend.live
        var users = [String?]()
        var properties = [(String?, String)]()
        var collection = [Bool]()
        var defaults = [[String: Any]?]()
        var resets = 0
        var timeout: TimeInterval = 0
        backend.setUserID = { users.append($0) }
        backend.setUserProperty = { properties.append(($0, $1)) }
        backend.setCollectionEnabled = { collection.append($0) }
        backend.setDefaultParameters = { defaults.append($0) }
        backend.resetData = { resets += 1 }
        backend.setSessionTimeout = { timeout = $0 }
        backend.appInstanceID = { "instance" }
        let manager = AnalyticsManager(backend: backend)
        manager.setUserID("user")
        manager.setUserID(nil)
        manager.setUserProperty("gold", forName: "tier")
        manager.setUserProperty(nil, forName: "tier")
        manager.setAnalyticsCollectionEnabled(false)
        manager.setAnalyticsCollectionEnabled(true)
        manager.setDefaultEventParameters(["remove": NSNull()])
        manager.setDefaultEventParameters(nil)
        manager.resetAnalyticsData()
        manager.setSessionTimeoutInterval(12.5)
        #expect(users == ["user", nil])
        #expect(properties.count == 2 && properties[0].0 == "gold" && properties[1].0 == nil)
        #expect(properties.allSatisfy { $0.1 == "tier" })
        #expect(collection == [false, true])
        #expect(defaults[0]?["remove"] is NSNull && defaults[1] == nil)
        #expect(resets == 1 && timeout == 12.5)
        #expect(manager.appInstanceID() == "instance")
        backend.appInstanceID = { nil }
        #expect(AnalyticsManager(backend: backend).appInstanceID() == nil)
    }

    @Test func consentOmitsUnchangedCategoriesAndPreservesEnumABI() {
        var backend = AnalyticsBackend.live
        var calls = 0
        backend.setConsent = { consent in
            calls += 1
            #expect(consent.count == 3)
            #expect(consent[.analyticsStorage] == .granted)
            #expect(consent[.adStorage] == nil)
            #expect(consent[.adUserData] == .denied)
            #expect(consent[.adPersonalization] == .granted)
        }
        let manager = AnalyticsManager(backend: backend)
        manager.setConsent()
        #expect(calls == 0)
        manager.setConsent(analyticsStorage: .granted, adUserData: .denied, adPersonalization: .granted)
        #expect(calls == 1)
        #expect(AnalyticsConsentStatus.unchanged.rawValue == 0)
        #expect(AnalyticsConsentStatus.granted.rawValue == 1)
        #expect(AnalyticsConsentStatus.denied.rawValue == 2)
    }

    @Test func consentIncludesAllDeniedCategories() {
        var backend = AnalyticsBackend.live
        var calls = 0
        backend.setConsent = { consent in
            calls += 1
            #expect(consent.count == 4)
            #expect(consent.values.allSatisfy { $0 == .denied })
        }
        AnalyticsManager(backend: backend).setConsent(analyticsStorage: .denied,
                                                    adStorage: .denied,
                                                    adUserData: .denied,
                                                    adPersonalization: .denied)
        #expect(calls == 1)
    }

    @Test func sessionCompletionPreservesInt64AndNativeError() {
        var backend = AnalyticsBackend.live
        var callback: ((Int64, Error?) -> Void)?
        var calls = 0
        backend.sessionID = { callback = $0 }
        AnalyticsManager(backend: backend).sessionID { identifier, error in
            calls += 1
            #expect(identifier?.int64Value == Int64.max)
            #expect(error == nil)
        }
        #expect(calls == 0)
        callback?(Int64.max, nil)
        #expect(calls == 1)
        let failure = NSError(domain: "test", code: 42, userInfo: ["detail": "disabled"])
        AnalyticsManager(backend: backend).sessionID { identifier, error in
            calls += 1
            #expect(identifier == nil)
            #expect(error === failure)
        }
        callback?(123, failure)
        #expect(calls == 2)
    }

    @Test func crashErrorsLogsAndMetadataAreForwarded() {
        var backend = CrashlyticsBackend.live
        let error = NSError(domain: "test", code: 7)
        var calls = [String]()
        backend.log = { #expect($0 == "breadcrumb"); calls.append("log") }
        backend.recordError = { #expect($0 === error); calls.append("error") }
        backend.recordErrorWithInfo = {
            #expect($0 === error)
            #expect($1?["attempt"] as? Int == 2)
            calls.append("info")
        }
        let manager = CrashlyticsManager(backend: backend)
        manager.log(message: "breadcrumb")
        manager.record(error: error)
        manager.record(error: error, userInfo: ["attempt": 2])
        #expect(calls == ["log", "error", "info"])
        backend.recordErrorWithInfo = { #expect($0 === error && $1 == nil); calls.append("nil") }
        CrashlyticsManager(backend: backend).record(error: error, userInfo: nil)
        #expect(calls.last == "nil")
    }

    @Test func managedExceptionPreservesNameReasonAndFrameOrder() {
        var backend = CrashlyticsBackend.live
        let makeException = backend.makeException
        let makeFrame = backend.makeFrame
        var symbols = [String]()
        var files = [String]()
        var lines = [Int]()
        var recordings = 0
        backend.makeException = { name, reason in
            #expect(name == "Managed.Error" && reason == "failure")
            return makeException(name, reason)
        }
        backend.makeFrame = { symbol, file, line in
            symbols.append(symbol)
            files.append(file)
            lines.append(line)
            return makeFrame(symbol, file, line)
        }
        backend.recordException = { exception in
            recordings += 1
            #expect(exception.stackTrace.count == (recordings == 1 ? 2 : 0))
        }
        let manager = CrashlyticsManager(backend: backend)
        manager.recordException(name: "Managed.Error",
                                reason: "failure",
                                stackTrace: [CrashlyticsStackFrame(symbol: "Throw", file: "Test.cs", line: 42),
                                             CrashlyticsStackFrame(symbol: "Caller", file: "", line: 0)])
        manager.recordException(name: "Managed.Error", reason: "failure", stackTrace: [])
        #expect(symbols == ["Throw", "Caller"])
        #expect(files == ["Test.cs", ""] && lines == [42, 0])
        #expect(recordings == 2)
    }

    @Test func crashContextAndCollectionControls() {
        var backend = CrashlyticsBackend.live
        var users = [String?]()
        var values = [Any?]()
        var enabled = [Bool]()
        var keysCalls = 0
        backend.setUserID = { users.append($0) }
        backend.setCustomValue = { #expect($1 == "key"); values.append($0) }
        backend.setCustomKeys = { #expect($0["attempt"] as? Int == 3); keysCalls += 1 }
        backend.setCollectionEnabled = { enabled.append($0) }
        backend.isCollectionEnabled = { false }
        backend.didCrash = { true }
        let manager = CrashlyticsManager(backend: backend)
        manager.setUserID("user")
        manager.setUserID(nil)
        manager.setCustomValue(3, forKey: "key")
        manager.setCustomValue(nil, forKey: "key")
        manager.setCustomKeysAndValues(["attempt": 3])
        manager.setCrashlyticsCollectionEnabled(false)
        manager.setCrashlyticsCollectionEnabled(true)
        #expect(users == ["user", nil] && values.count == 2)
        #expect(values[0] as? Int == 3 && values[1] == nil)
        #expect(keysCalls == 1 && enabled == [false, true])
        #expect(!manager.isCrashlyticsCollectionEnabled() && manager.didCrashDuringPreviousExecution())
    }

    @Test(arguments: [false, true]) func pendingReportCallbacksAndActions(pending: Bool) {
        var backend = CrashlyticsBackend.live
        var callback: ((Bool) -> Void)?
        var result: Bool?
        var sent = 0
        var deleted = 0
        backend.checkReports = { callback = $0 }
        backend.sendReports = { sent += 1 }
        backend.deleteReports = { deleted += 1 }
        let manager = CrashlyticsManager(backend: backend)
        manager.checkForUnsentReports { result = $0 }
        #expect(result == nil)
        callback?(pending)
        #expect(result == pending)
        manager.sendUnsentReports()
        manager.deleteUnsentReports()
        #expect(sent == 1 && deleted == 1)
    }

    @Test func initializerDelegatesExactlyOncePerInvocation() {
        var calls = 0
        FirebaseCoreManager.initializeFirebase(configure: { calls += 1 })
        #expect(calls == 1)
        // Idempotence belongs to the managed startup gate, not the native ABI.
        FirebaseCoreManager.initializeFirebase(configure: { calls += 1 })
        #expect(calls == 2)
    }
}
