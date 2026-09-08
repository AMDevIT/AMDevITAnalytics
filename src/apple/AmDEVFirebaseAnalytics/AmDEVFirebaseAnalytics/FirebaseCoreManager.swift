//
//  FirebaseCore.swift
//  AmDEVFirebaseAnalytics
//
//  Created by Alessandro Morvillo on 30/08/2026.
//

import FirebaseCore

@objc public class FirebaseCoreManager
    : NSObject {

    // MARK: - Methods

    /// Initializes Firebase for the application
    @objc public static func initializeFirebase() {
        self.initializeFirebase(configure: { FirebaseApp.configure() })
    }

    internal static func initializeFirebase(configure: () -> Void) {
        configure()
    }
}
