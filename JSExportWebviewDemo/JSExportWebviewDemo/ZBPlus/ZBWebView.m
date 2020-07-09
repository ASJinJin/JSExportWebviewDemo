//
//  ZBWebView.m
//  Demo_JSExport
//
//  Created by JZhou on 2019/6/4.
//  Copyright © 2019 纠结伦. All rights reserved.
//

#import "ZBWebView.h"
#import "ZBPlus.h"
#import "ZBWebViewJSExport.h"
#import "UIWebView+TS_JavaScriptContext.h"
#import "UIWebView+Gesture.h"
#import "ZBAppCore.h"

static NSString *const ZBWebViewEventClose = @"close";
static NSString *const ZBWebViewEventError = @"error";
static NSString *const ZBWebViewEventShow = @"show";
static NSString *const ZBWebViewEventHide = @"hide";
static NSString *const ZBWebViewEventLoading = @"loading";
static NSString *const ZBWebViewEventLoaded = @"loaded";
static NSString *const ZBWebViewEventPopGesture = @"popGesture";

@interface ZBWebView () <ZBWebViewJSExport,TSWebViewDelegate>
{
     NSInteger _count;
}

@end


@implementation ZBWebView
@synthesize url = _url;
@synthesize id = _id;
@synthesize count = _count;
@synthesize style = _style;
@synthesize visible = _visible;
@synthesize clsoeJSEvent = _clsoeJSEvent, errorJSEvent = _errorJSEvent, showJSEvent= _showJSEvent, hideJSEvent = _hideJSEvent, loadedJSEvent = _loadedJSEvent, loadingJSEvent = _loadingJSEvent, popJSEvent = _popJSEvent;


//@synthesize webviewEvents = _webviewEvents;


-(instancetype)initWithStyle:(ZBWebViewStyle *)style htmlUrl:(NSString *)url{
//    self = [self initWithFrame:CGRectMake(style.left, style.top, style.width, style.height) htmlUrl:url];
    __block ZBWebView* weakSelf = [ZBWebView new];
     dispatch_async(dispatch_get_main_queue(), ^{
        weakSelf = [[ZBWebView alloc] initWithFrame:CGRectMake(10, 10, 400, 400) htmlUrl:url];

        weakSelf.style = style;
        
        weakSelf.scrollView.scrollsToTop = style.scrollsToTop;
        //    self.alpha = style.opacity;
        
        weakSelf.layer.masksToBounds = YES;
       
     });
    return weakSelf;
}


- (void)setParentWebView:(ZBWebView *)parentWebivew{
    _parentWebView = parentWebivew;
}

//- (NSMutableDictionary *)webviewEvents{
//    if (_webviewEvents == nil) {
//        _webviewEvents = [NSMutableDictionary dictionary];
//    }
//    return _webviewEvents;
//}

-(instancetype)initWithFrame:(CGRect)frame htmlUrl:(NSString *)url{
    
    self = [super initWithFrame:frame];
    self.delegate = self;
    _url = url;
    
    return self;
}

- (void)setStyle:(ZBWebViewStyle *)style{
    _style = style;
    self.frame = CGRectMake(_style.left, _style.top, _style.width, _style.height);
    self.opaque = NO;
    self.backgroundColor = _style.background;
    
}

- (void)loadRequest{
    NSString *path = [[[NSBundle mainBundle] bundlePath]  stringByAppendingPathComponent:self.url];
    NSURLRequest *request = [NSURLRequest requestWithURL:[NSURL fileURLWithPath:path]];
    
    if ([self.url hasPrefix:@"http"]) {
        request = [NSURLRequest requestWithURL:[NSURL URLWithString:self.url]];
    }
    
    [self loadRequest: request];
}


-(void)webView:(UIWebView *)webView didCreateJavaScriptContext:(JSContext *)ctx{
    ZBPlus *plus = [[ZBPlus alloc] initWithWebView:(ZBWebView *)webView];
//    ZBPlus *plus = [[ZBPlus alloc] initWithWebView:[ZBWebView class]];

//    ctx[@"Plus"] = [ZBPlus class];
//    NSLog(@"%@",ctx);
    ctx[@"Plus"] = plus;
//    [ctx evaluateScript:@"Plus."];
}

- (void)webViewDidStartLoad:(UIWebView *)webView{
    [self.loadingJSEvent.value callWithArguments:@[@"loading!!!"]];
//    ZBWebView *jsWebview = (ZBWebView *)webView;
//    JSManagedValue * jsmv = [jsWebview.webviewEvents objectForKey:ZBWebViewEventLoading];
//    [jsmv.value callWithArguments:@[@"success!!!!"]];
}

- (void)webView:(UIWebView *)webView didFailLoadWithError:(NSError *)error{
    [self.errorJSEvent.value callWithArguments:@[@"error!!!"]];
//    ZBWebView *jsWebview = (ZBWebView *)webView;
//    JSManagedValue * jsmv = [jsWebview.webviewEvents objectForKey:ZBWebViewEventError];
//    [jsmv.value callWithArguments:@[@"success!!!!"]];
}

- (void)webViewDidFinishLoad:(UIWebView *)webView
{
     [self.loadedJSEvent.value callWithArguments:@[@"loaded!!!"]];
//    ZBWebView *jsWebview = (ZBWebView *)webView;
//    JSManagedValue * jsmv = [jsWebview.webviewEvents objectForKey:ZBWebViewEventLoaded];
//    [jsmv.value callWithArguments:@[@"success!!!!"]];
}

@end


@interface ZBWebView (JSMethodImplementation)
-(ZBWebView *)createUrl:(NSString *)url idStr:(NSString *)idStr styles:(NSDictionary *)styles extras:(NSDictionary *)extras;

@end

@implementation ZBWebView (JSMethodImplementation)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wobjc-protocol-method-implementation"

-(NSInteger)count{
    return [[ZBAppCore shareInstance].allWebviews count];
}


- (ZBWebView *)createWebview:(NSString *)url styles:(NSDictionary *)styles{
    ZBWebView*  webview = [[ZBWebView alloc] init];
    webview.style = [[ZBWebViewStyle alloc] initWithStyles:styles];
    webview.url = url;
    webview.backgroundColor = [UIColor redColor];
    webview.frame = CGRectMake(0, 0, KMainWidth, KMainHeight);
    webview.delegate =webview;
    return webview;
}

-(ZBWebView *)createUrl:(NSString *)url idStr:(NSString *)idStr styles:(NSDictionary *)styles extras:(NSDictionary *)extras{
    
  
//    dispatch_semaphore_t signal = dispatch_semaphore_create(0);
//    __block ZBWebView *webview;
//
//    dispatch_async(dispatch_get_main_queue(), ^{
//        webview = [ZBWebView new];
//        webview.style = [[ZBWebViewStyle alloc] initWithStyles:styles];
//        webview.url = url;
//        webview.backgroundColor = [UIColor redColor];
//        webview.frame = CGRectMake(0, 0, KMainWidth, KMainHeight);
//        webview.delegate =webview;
//        dispatch_semaphore_signal(signal);
//    });
//
//    dispatch_semaphore_wait(signal, DISPATCH_TIME_FOREVER);
//    webview.scrollView.scrollEnabled= NO;
//    webview.scrollView.showsVerticalScrollIndicator = NO;
//    return webview;
    
    return [self createWebview:url styles:styles];
}


-(ZBWebView *)openUrl:(NSString *)url idStr:(NSString *)idStr styles:(NSDictionary *)styles extras:(NSDictionary *)extras jsFunction:(JSValue *)successBlock{
    ZBWebView *webview =  [self createWebview:url styles:styles];//[self createUrl:url idStr:idStr styles:styles extras:extras];
    [webview show:webview animation:@1 duration:@1 jsFunction:nil];
    
    if (successBlock) {
        JSValue *e = [successBlock callWithArguments:@[@"success!!!!!"]];
        NSLog(@"%@",e);
    }
    return webview;
}

- (void)show:(nullable id)id_webviewObj animation:(nullable NSNumber*)animation duration:(nullable NSNumber*)duration jsFunction:(JSValue *)successBlock{
    ZBWebView *webview;
    if (id_webviewObj) {
        if ([id_webviewObj isKindOfClass:[NSString class]]) {
            for (ZBWebView *obj in [ZBAppCore shareInstance].allWebviews) {
                if ([obj.webview_id isEqualToString:id_webviewObj]) {
                    webview = obj;
                    break;
                }
            }
        }
        
        if ([id_webviewObj isKindOfClass:[ZBWebView class]]) {
            webview = id_webviewObj;
        }
        
    }
    if(webview == nil){
        webview = self;
    }
    
    dispatch_async(dispatch_get_main_queue(), ^{
        if (!id_webviewObj) {
            webview.hidden = NO;
        }
        NSLog(@"%@",[ZBAppCore shareInstance].appWindow);
        [webview loadRequest];
        [[ZBAppCore shareInstance].appWindow addSubview:webview];
        if ([animation boolValue]) {
            webview.frame = CGRectMake(KMainWidth, 0, webview.width, webview.height);
            [UIView animateWithDuration:0.35 delay:0 options:UIViewAnimationOptionCurveEaseIn animations:^{
                webview.frame = CGRectMake(0, 0, webview.width, webview.height);
            } completion:^(BOOL finished) {
                [self.showJSEvent.value callWithArguments:@[@"show!!!"]];
//                JSManagedValue * jsmv = [webview.webviewEvents objectForKey:ZBWebViewEventShow];
//                [jsmv.value callWithArguments:@[@"success!!!!"]];
            }];
        }
    });
    if (successBlock) {
        JSValue *e = [successBlock callWithArguments:@[@"success!!!!!"]];
        NSLog(@"%@",e);
    }
}

-(void)hide:(id)id_webviewObj{
    if (!id_webviewObj || [id_webviewObj isEqualToString:self.webview_id]) {
        self.hidden = YES;
    }else{
        
        [[ZBAppCore shareInstance].appWindow.subviews enumerateObjectsWithOptions:NSEnumerationReverse usingBlock:^(__kindof UIView * _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {
            if ([obj isKindOfClass:[ZBWebView class]]) {
                ZBWebView *wv = (ZBWebView *)obj;
                if ([wv.webview_id isEqualToString:id_webviewObj]) {
                    [self.hideJSEvent.value callWithArguments:@[@"hide!!!"]];
//                    JSManagedValue * jsmv = [self.webviewEvents objectForKey:ZBWebViewEventHide];
//                    [jsmv.value callWithArguments:@[@"success!!!!"]];
                    wv.hidden = YES;
                    *stop = YES;
                };
            }
        }];
    }
}

-(void)close:(id)id_webviewObj{
    
    if (!id_webviewObj || [id_webviewObj isEqualToString:self.webview_id]) {
        [self.clsoeJSEvent.value callWithArguments:@[@"close!!!"]];
        dispatch_async(dispatch_get_main_queue(), ^{
            [self.clsoeJSEvent.value callWithArguments:@[@"close!!!"]];
//            JSManagedValue * jsmv = [self.webviewEvents objectForKey:ZBWebViewEventClose];
//            [jsmv.value callWithArguments:@[@"close!!!!"]];
            
            [UIView animateWithDuration:0.35 delay:0 options:UIViewAnimationOptionCurveEaseIn animations:^{
                self.left = KMainWidth;
            } completion:^(BOOL finished) {
                [self removeFromSuperview];
            }];
        });
    }else{
    
        [[ZBAppCore shareInstance].appWindow.subviews enumerateObjectsWithOptions:NSEnumerationReverse usingBlock:^(__kindof UIView * _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {
            if ([obj isKindOfClass:[ZBWebView class]]) {
                ZBWebView *wv = (ZBWebView *)obj;
                if ([wv.webview_id isEqualToString:id_webviewObj]) {
                    [self.clsoeJSEvent.value callWithArguments:@[@"close!!!"]];
//                    JSManagedValue * jsmv = [wv.webviewEvents objectForKey:ZBWebViewEventClose];
//                    [jsmv.value callWithArguments:@[@"close!!!!"]];
                     [wv.clsoeJSEvent.value callWithArguments:@[@"close!!!"]];
                    
                    [UIView animateWithDuration:0.35 delay:0 options:UIViewAnimationOptionCurveEaseIn animations:^{
                        wv.left = KMainWidth;
                        
                    } completion:^(BOOL finished) {
                        
                        [wv removeFromSuperview];
    
                    }];
                    
                    *stop = YES;
                };
            }
        }];
    }
}

-(void)popGestureEvent{
    [self.popJSEvent.value callWithArguments:@[@"pop!!!!"]];
}

-(ZBWebView *)currentWebview{
    return self;
}

-(ZBWebView *)getWebviewById:(NSString *)webviewId{
    NSArray *subviews = [ZBAppCore shareInstance].appWindow.subviews;
    __block ZBWebView *webview;
    [subviews enumerateObjectsUsingBlock:^(id  _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {
        
        if ([obj isKindOfClass:[ZBWebView class]]) {
            ZBWebView *w = obj;
            if ([w.webview_id isEqualToString:webviewId]) {
                webview = w;
                *stop = YES;
            }
        }
    }];
    return webview;
}

-(ZBWebView *)getLaunchWebview{
    UIViewController *vc = [ZBAppCore shareInstance].viewController;
    NSArray *subviews = vc.view.subviews;
    ZBWebView *webview = [subviews firstObject];
    return webview;
}

-(ZBWebView *)getTopWebview{
    UIViewController *vc = [ZBAppCore shareInstance].viewController;
    NSArray *subviews = vc.view.subviews;
    ZBWebView *webview = [subviews lastObject];
    return webview;
}

- (ZBWebView *)opener{
    return self;
}

- (UIView *)parent{
    return [self superview];
}

- (NSArray *)all{
    return [NSArray arrayWithArray:[[ZBAppCore shareInstance] allWebviews]];
}

- (void)setStyle:(NSDictionary * _Nonnull)style{
    _style = style;
}

- (NSDictionary *)getStyle{
    return _style;
}

- (NSString *)getURL{
    return _url;
}

- (BOOL)isVisible{
    return _visible;
}

-(void)append:(id)webview{
    [self addSubview:webview];
}

-(void)addEvent:(NSString *)event listener:(JSValue *)listener{
    JSManagedValue *jsmv = [JSManagedValue managedValueWithValue:listener andOwner:self];
//    if (event) {
//        [self.webviewEvents setObject:jsmv forKey:event];
//    }
//    __autoreleasing id info = (__bridge_transfer id)JSObjectGetPrivate(listener.JSValueRef);
    
//    self.clsoeJSEvent = jsmv;
    
    [[self.ts_javaScriptContext virtualMachine] addManagedReference:self.clsoeJSEvent withOwner:self];
    
//    NSLog(@"%@---%@",[self.webviewEvents objectForKey:event],[self.webviewEvents objectForKey:event].value);
}

- (void)removeEvent:(NSString *)event listener:(JSValue *)listener{
//    [self.webviewEvents removeObjectForKey:event];
}
#pragma clang diagnostic pop

@end
