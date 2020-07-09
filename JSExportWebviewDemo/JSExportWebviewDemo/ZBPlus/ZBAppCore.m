//
//  ZBWebViewCore.m
//  OCJSDemo
//
//  Created by JZhou on 2019/4/25.
//  Copyright © 2019年 Dong. All rights reserved.
//

#import "ZBAppCore.h"
@interface ZBAppCore(){
//    ZBWebViewManager *_webviewManager;
}
@end

@implementation ZBAppCore
+(instancetype)shareInstance
{
    static ZBAppCore *_single = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _single = [[super allocWithZone:NULL] init];
    });
    return _single;
}

+ (instancetype)allocWithZone:(struct _NSZone *)zone
{
    return [self shareInstance];
}

- (id)copyWithZone:(NSZone *)zone
{
    return self;
}

- (id)mutableCopyWithZone:(NSZone *)zone
{
    return self;
}

- (void)setViewController:(UIViewController *)viewController{
    if (viewController) {
        _appWindow = viewController.view;
    }
    _viewController = viewController;
}

- (NSMutableArray *)allWebviews{
    if (_allWebviews == nil) {
        _allWebviews = [NSMutableArray array];
    }
    return _allWebviews;
}
//- (ZBWebViewManager *)webviewManager{
//    if (_webviewManager == nil) {
//        _webviewManager = [ZBWebViewManager new];
//    }
//    return _webviewManager;
//}

@end
