//
//  ZBWebViewJSExport.h
//  Demo_JSExport
//
//  Created by JZhou on 2019/6/18.
//  Copyright © 2019 纠结伦. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <JavaScriptCore/JavaScriptCore.h>
#import "ZBWebView.h"

NS_ASSUME_NONNULL_BEGIN
typedef void(^JSFunctionBlock)(id);

@protocol ZBWebViewJSExport <JSExport, NSObject>
@property(nonatomic,strong)NSString *url;
@property(nonatomic,strong)NSString *id;
@property(nonatomic,assign,readonly)NSInteger count;
@property(nonatomic,strong,getter=getStyle)NSDictionary *style;
@property(nonatomic,getter=isVisible,setter=setVisible:) BOOL visible;
@property (nonatomic,strong)JSManagedValue *clsoeJSEvent;
@property (nonatomic,strong)JSManagedValue *errorJSEvent;
@property (nonatomic,strong)JSManagedValue *showJSEvent;
@property (nonatomic,strong)JSManagedValue *hideJSEvent;
@property (nonatomic,strong)JSManagedValue *loadingJSEvent;
@property (nonatomic,strong)JSManagedValue *loadedJSEvent;
@property (nonatomic,strong)JSManagedValue *popJSEvent;
//@property (nonatomic,strong)NSMutableDictionary *webviewEvents;

JSExportAs(create, -(nullable ZBWebView *)createUrl:(NSString *)url idStr:(nullable NSString *)idStr styles:(nullable NSDictionary*)styles extras:(NSDictionary*)extras);
JSExportAs(open, -(nullable ZBWebView *)openUrl:(NSString *)url idStr:(nullable NSString *)idStr styles:(nullable NSDictionary*)styles extras:(nullable NSDictionary*)extras jsFunction:(nullable JSValue *)successBlock);
JSExportAs(show,-(void)show:(nullable id)id_webviewObj animation:(nullable NSNumber*)animation duration:(nullable NSNumber*)duration jsFunction:(nullable JSValue *)successBlock);
JSExportAs(hide, -(void)hide:(nullable id)id_webviewObj);
JSExportAs(close, -(void)close:(nullable id)id_webviewObj);
JSExportAs(getWebviewById, -(ZBWebView *)getWebviewById:(nullable NSString *)webviewId);

JSExportAs(addEventListener, -(void)addEvent:(NSString *)event listener:(JSValue *)listener);
JSExportAs(removeEventListener, -(void)removeEvent:(NSString *)event listener:(JSValue *)listener);

JSExportAs(append, -(void)append:(nullable id)webview);
JSExportAs(appendJsFile, -(void)appendJsFile:(NSString *)file);
JSExportAs(evalJS, -(void)evalJS:(NSString *)jsString);


- (NSArray *)all;
- (ZBWebView *)currentWebview;
- (ZBWebView *)getLaunchWebview;
- (ZBWebView *)getTopWebview;
- (ZBWebView *)opener;
- (UIView *)parent;

- (void)setStyle:(NSDictionary * _Nonnull)style;
- (NSDictionary *)getStyle;
- (NSString *)getURL;
- (BOOL)isVisible;

@end

NS_ASSUME_NONNULL_END
