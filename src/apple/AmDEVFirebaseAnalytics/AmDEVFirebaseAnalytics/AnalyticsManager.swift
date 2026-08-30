//
//  AmDEVFirebaseAnalytics.swift
//  AmDEVFirebaseAnalytics
//
//  Created by Alessandro Morvillo on 30/08/2026.
//

import Foundation
import FirebaseCore
import FirebaseAnalytics

@objc public class AnalyticsManager
    : NSObject {
    
    //MARK: Methods
    
    @objc public func logEvent(name: String, parameters: [String: Any]? = nil) {
        Analytics.logEvent(name, parameters: parameters)
    }
}
