//
//  ZBWebView.h
//  Demo_JSExport
//
//  Created by JZhou on 2019/6/4.
//  Copyright © 2019 纠结伦. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "ZBWebViewStyle.h"
NS_ASSUME_NONNULL_BEGIN

@interface ZBWebView : UIWebView


@property(nonatomic,strong)NSString *webview_id;

@property (nonatomic,strong)ZBWebViewStyle *style;
@property (nonatomic,strong)ZBWebView *parentWebView;

//@property (nonatomic,strong)NSMutableDictionary *webviewEvents;



-(instancetype)initWithStyle:(ZBWebViewStyle *)style htmlUrl:(NSString *)url;

-(instancetype)initWithFrame:(CGRect)frame htmlUrl:(NSString *)url;

-(void)popGestureEvent;

@end





NS_ASSUME_NONNULL_END
