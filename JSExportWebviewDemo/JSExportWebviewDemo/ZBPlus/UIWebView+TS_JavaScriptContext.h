//
//  UIWebView+TS_JavaScriptContext.h
//  JSExportWebviewDemo
//
//  Created by JZhou on 2019/7/5.
//  Copyright © 2019 ZBIntel. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <JavaScriptCore/JavaScriptCore.h>

@protocol TSWebViewDelegate <UIWebViewDelegate>

@optional

- (void)webView:(UIWebView *)webView didCreateJavaScriptContext:(JSContext*) ctx;

@end


@interface UIWebView (TS_JavaScriptContext)

@property (nonatomic, readonly) JSContext* ts_javaScriptContext;

@end
