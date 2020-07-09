//
//  ZBPlus.h
//  Demo_JSExport
//
//  Created by JZhou on 2019/6/18.
//  Copyright © 2019 纠结伦. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "ZBWebView.h"

NS_ASSUME_NONNULL_BEGIN

@interface ZBPlus : NSObject
//- (instancetype)initWithWebView:(Class)webview;
- (instancetype)initWithWebView:(ZBWebView *)webview;
@end

NS_ASSUME_NONNULL_END
