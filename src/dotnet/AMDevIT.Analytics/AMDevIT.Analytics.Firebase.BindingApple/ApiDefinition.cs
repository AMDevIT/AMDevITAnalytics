using System;
using Foundation;
using ObjCRuntime;

namespace AMDevIT.Analytics.Firebase.BindingApple {
	// @interface AnalyticsManager : NSObject
	[BaseType (typeof (NSObject), Name = "_TtC22AmDEVFirebaseAnalytics16AnalyticsManager")]
	interface AnalyticsManager {
		// -(void)logEventWithName:(NSString * _Nonnull)name parameters:(NSDictionary<NSString *,id> * _Nullable)parameters;
		[Export ("logEventWithName:parameters:")]
		void LogEventWithName (string name, [NullAllowed] NSDictionary<NSString, NSObject> parameters);

		// -(void)setUserID:(NSString * _Nullable)userID;
		[Export ("setUserID:")]
		void SetUserID ([NullAllowed] string userID);

		// -(void)setUserProperty:(NSString * _Nullable)value forName:(NSString * _Nonnull)name;
		[Export ("setUserProperty:forName:")]
		void SetUserProperty ([NullAllowed] string value, string name);

		// -(void)setAnalyticsCollectionEnabled:(BOOL)enabled;
		[Export ("setAnalyticsCollectionEnabled:")]
		void SetAnalyticsCollectionEnabled (bool enabled);

		// -(void)setConsentWithAnalyticsStorage:(enum AnalyticsConsentStatus)analyticsStorage adStorage:(enum AnalyticsConsentStatus)adStorage adUserData:(enum AnalyticsConsentStatus)adUserData adPersonalization:(enum AnalyticsConsentStatus)adPersonalization;
		[Export ("setConsentWithAnalyticsStorage:adStorage:adUserData:adPersonalization:")]
		void SetConsentWithAnalyticsStorage (AnalyticsConsentStatus analyticsStorage, AnalyticsConsentStatus adStorage, AnalyticsConsentStatus adUserData, AnalyticsConsentStatus adPersonalization);

		// -(void)setDefaultEventParameters:(NSDictionary<NSString *,id> * _Nullable)parameters;
		[Export ("setDefaultEventParameters:")]
		void SetDefaultEventParameters ([NullAllowed] NSDictionary<NSString, NSObject> parameters);

		// -(void)resetAnalyticsData;
		[Export ("resetAnalyticsData")]
		void ResetAnalyticsData ();

		// -(void)setSessionTimeoutInterval:(NSTimeInterval)interval;
		[Export ("setSessionTimeoutInterval:")]
		void SetSessionTimeoutInterval (double interval);

		// -(void)sessionIDWithCompletion:(void (^ _Nonnull)(NSNumber * _Nullable, NSError * _Nullable))completion;
		[Export ("sessionIDWithCompletion:")]
		void SessionIDWithCompletion (Action<NSNumber, NSError> completion);

		// -(NSString * _Nullable)appInstanceID __attribute__((warn_unused_result("")));
		[NullAllowed, Export ("appInstanceID")]
		string AppInstanceID { get; }
	}

	// @interface CrashlyticsManager : NSObject
	[BaseType (typeof (NSObject), Name = "_TtC22AmDEVFirebaseAnalytics18CrashlyticsManager")]
	interface CrashlyticsManager {
		// -(void)logWithMessage:(NSString * _Nonnull)message;
		[Export ("logWithMessage:")]
		void LogWithMessage (string message);

		// -(void)recordWithError:(NSError * _Nonnull)error;
		[Export ("recordWithError:")]
		void RecordWithError (NSError error);

		// -(void)recordWithError:(NSError * _Nonnull)error userInfo:(NSDictionary<NSString *,id> * _Nullable)userInfo;
		[Export ("recordWithError:userInfo:")]
		void RecordWithError (NSError error, [NullAllowed] NSDictionary<NSString, NSObject> userInfo);

		// -(void)recordExceptionWithName:(NSString * _Nonnull)name reason:(NSString * _Nonnull)reason stackTrace:(NSArray<CrashlyticsStackFrame *> * _Nonnull)stackTrace;
		[Export ("recordExceptionWithName:reason:stackTrace:")]
		void RecordExceptionWithName (string name, string reason, CrashlyticsStackFrame [] stackTrace);

		// -(void)setCustomValue:(id _Nullable)value forKey:(NSString * _Nonnull)key;
		[Export ("setCustomValue:forKey:")]
		void SetCustomValue ([NullAllowed] NSObject value, string key);

		// -(void)setCustomKeysAndValues:(NSDictionary<NSString *,id> * _Nonnull)values;
		[Export ("setCustomKeysAndValues:")]
		void SetCustomKeysAndValues (NSDictionary<NSString, NSObject> values);

		// -(void)setUserID:(NSString * _Nullable)userID;
		[Export ("setUserID:")]
		void SetUserID ([NullAllowed] string userID);

		// -(void)setCrashlyticsCollectionEnabled:(BOOL)enabled;
		[Export ("setCrashlyticsCollectionEnabled:")]
		void SetCrashlyticsCollectionEnabled (bool enabled);

		// -(BOOL)isCrashlyticsCollectionEnabled __attribute__((warn_unused_result("")));
		[Export ("isCrashlyticsCollectionEnabled")]
		bool IsCrashlyticsCollectionEnabled { get; }

		// -(BOOL)didCrashDuringPreviousExecution __attribute__((warn_unused_result("")));
		[Export ("didCrashDuringPreviousExecution")]
		bool DidCrashDuringPreviousExecution { get; }

		// -(void)checkForUnsentReportsWithCompletion:(void (^ _Nonnull)(BOOL))completion;
		[Export ("checkForUnsentReportsWithCompletion:")]
		void CheckForUnsentReportsWithCompletion (Action<bool> completion);

		// -(void)sendUnsentReports;
		[Export ("sendUnsentReports")]
		void SendUnsentReports ();

		// -(void)deleteUnsentReports;
		[Export ("deleteUnsentReports")]
		void DeleteUnsentReports ();
	}

	// @interface CrashlyticsStackFrame : NSObject
	[BaseType (typeof (NSObject), Name = "_TtC22AmDEVFirebaseAnalytics21CrashlyticsStackFrame")]
	[DisableDefaultCtor]
	interface CrashlyticsStackFrame {
		// @property (readonly, copy, nonatomic) NSString * _Nonnull symbol;
		[Export ("symbol")]
		string Symbol { get; }

		// @property (readonly, copy, nonatomic) NSString * _Nonnull file;
		[Export ("file")]
		string File { get; }

		// @property (readonly, nonatomic) NSInteger line;
		[Export ("line")]
		nint Line { get; }

		// -(instancetype _Nonnull)initWithSymbol:(NSString * _Nonnull)symbol file:(NSString * _Nonnull)file line:(NSInteger)line __attribute__((objc_designated_initializer));
		[Export ("initWithSymbol:file:line:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string symbol, string file, nint line);
	}

	// @interface FirebaseCoreManager : NSObject
	[BaseType (typeof (NSObject), Name = "_TtC22AmDEVFirebaseAnalytics19FirebaseCoreManager")]
	interface FirebaseCoreManager {
		// +(void)initializeFirebase;
		[Static]
		[Export ("initializeFirebase")]
		void InitializeFirebase ();
	}
}
