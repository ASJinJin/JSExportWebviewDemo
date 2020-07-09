//
//  ZBJSExport.h
//  Demo_JSExport
//
//  Created by JZhou on 2019/6/18.
//  Copyright © 2019 纠结伦. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <JavaScriptCore/JavaScriptCore.h>
#import <UIKit/UIKit.h>
#import "ZBWebView.h"
NS_ASSUME_NONNULL_BEGIN

@protocol ZBJSExport <JSExport, NSObject>
//@property (nonatomic,strong) Class webview;
@property (nonatomic,strong) ZBWebView *webview;
@property (nonatomic,strong) NSString *codeString;
@end

NS_ASSUME_NONNULL_END
