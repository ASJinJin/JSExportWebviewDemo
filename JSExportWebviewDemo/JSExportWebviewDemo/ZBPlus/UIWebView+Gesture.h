//
//  UIWebView+Gesture.h
//  JSExportWebviewDemo
//
//  Created by JZhou on 2019/7/5.
//  Copyright © 2019 ZBIntel. All rights reserved.
//

#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

@interface UIWebView (Gesture)
- (void)setPanGestureRecognizerWithBlock: (void (^) (void))block;
@end

NS_ASSUME_NONNULL_END
