//
//  UIWebView+Gesture.m
//  JSExportWebviewDemo
//
//  Created by JZhou on 2019/7/5.
//  Copyright © 2019 ZBIntel. All rights reserved.
//

#import "UIWebView+Gesture.h"
#import "ZBWebView.h"
#import <objc/runtime.h>
#import "ZBAppCore.h"

static char kZBActionHandlerPanGestureKey;
static char kZBActionHandlerPanBlockKey;
@implementation UIWebView (Gesture)
-(void)setPanGestureRecognizerWithBlock:(void (^)(void))block{
    UIScreenEdgePanGestureRecognizer *gesture = objc_getAssociatedObject(self, &kZBActionHandlerPanGestureKey);
    if (!gesture) {
        gesture = [[UIScreenEdgePanGestureRecognizer alloc] initWithTarget:self action:@selector(__handlerActionPanGesture:)];
        gesture.edges = UIRectEdgeLeft;
        gesture.delegate = self;
        [self addGestureRecognizer:gesture];
        objc_setAssociatedObject(self, &kZBActionHandlerPanGestureKey, gesture, OBJC_ASSOCIATION_RETAIN);
    }
    
    objc_setAssociatedObject(self, &kZBActionHandlerPanBlockKey, block, OBJC_ASSOCIATION_COPY);
}

-(void)__handlerActionPanGesture:(UIPanGestureRecognizer *)gesture{
    if([[ZBAppCore shareInstance].allWebviews count] < 2) return;
    
    if (gesture.state == UIGestureRecognizerStateBegan){
        self.scrollView.scrollEnabled = NO;
    }
    
    // 获取平移手势移动后, 在相对坐标中的偏移量
    CGPoint point = [gesture translationInView:self];
    // 声明xNew变量用point.x赋值
    CGFloat xNew = point.x;
    // 改变mainView的frame
    self.frame = CGRectMake(xNew + self.frame.origin.x, 0, self.frame.size.width, self.frame.size.height);
    // 设置手势移动后的point
    [gesture setTranslation:CGPointZero inView:gesture.view];
    
    [self addShadowToView:self withColor:[UIColor blackColor]];
    
    if (gesture.state == UIGestureRecognizerStateEnded || gesture.state == UIGestureRecognizerStateCancelled) {
        
        if (self.frame.origin.x > [UIScreen mainScreen].bounds.size.width * 0.3) {
            [UIView animateWithDuration:0.35 delay:0 options:UIViewAnimationOptionCurveEaseIn animations:^{
                self.frame = CGRectMake([UIScreen mainScreen].bounds.size.width, 0, [UIScreen mainScreen].bounds.size.width, [UIScreen mainScreen].bounds.size.height);
            } completion:^(BOOL finished) {
                ZBWebView *jsWebview = (ZBWebView *)self;
                [jsWebview popGestureEvent];
                
//                JSManagedValue * jsmv = [jsWebview.webviewEvents objectForKey:@"popGesture"];
//                [jsmv.value callWithArguments:@[@"success!!!!"]];
                
                [self removeFromSuperview];
            }];
        }else{
            [UIView animateWithDuration:0.35 animations:^{
                self.frame = CGRectMake(0, 0, self.bounds.size.width, self.bounds.size.height);
            }];
        }
       self.scrollView.scrollEnabled = YES;
        NSLog(@"FlyElephant---视图拖动结束");
    }
}


- (void)addShadowToView:(UIView *)theView withColor:(UIColor *)theColor {
    
    theView.layer.shadowColor = theColor.CGColor;
    theView.layer.shadowOffset = CGSizeMake(0,0);
    theView.layer.shadowOpacity = 0.5;
    theView.layer.shadowRadius = 5;
    // 单边阴影 顶边
    float shadowPathWidth = theView.layer.shadowRadius;
    CGRect shadowRect = CGRectMake(0-shadowPathWidth/2.0, 0,shadowPathWidth,theView.bounds.size.height);
    UIBezierPath *path = [UIBezierPath bezierPathWithRect:shadowRect];
    theView.layer.shadowPath = path.CGPath;
    
}

- (BOOL)gestureRecognizer:(UIGestureRecognizer *)gestureRecognizer shouldRecognizeSimultaneouslyWithGestureRecognizer:(UIGestureRecognizer *)otherGestureRecognizer
{
    return YES;
    
}

- (void)willMoveToSuperview:(nullable UIView *)newSuperview{
    [super willMoveToSuperview:newSuperview];
    
    if (newSuperview) {
        [[ZBAppCore shareInstance].allWebviews addObject:self];
        if ([[ZBAppCore shareInstance].allWebviews count]>1) {
            [self setPanGestureRecognizerWithBlock:^{
                //添加手势
            }];
        }
    }else{
        [[ZBAppCore shareInstance].allWebviews removeObject:self];
    }
 
}


@end
