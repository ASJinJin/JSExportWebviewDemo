//
//  ZBWebViewCore.h
//  OCJSDemo
//
//  Created by JZhou on 2019/4/25.
//  Copyright © 2019年 Dong. All rights reserved.
//

#import <Foundation/Foundation.h>
//#import "ZBWebViewManager.h"
#import "ZBWebView.h"


NS_ASSUME_NONNULL_BEGIN

@interface ZBAppCore : NSObject
+ (instancetype)shareInstance;

// 应用视图控制器
@property (nonatomic,strong)UIViewController *viewController;
// 应用根窗口
@property (nonatomic, readonly)UIView *appWindow;
// 应用首页面
@property (nonatomic, readonly)ZBWebView *mainFrame;

@property (nonnull, strong)NSMutableArray *allWebviews;
// 应用页面管理
//@property (nonatomic, readonly)ZBWebViewManager *webviewManager;


@end

NS_ASSUME_NONNULL_END
