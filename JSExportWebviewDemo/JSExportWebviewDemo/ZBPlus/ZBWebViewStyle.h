//
//  ZBWebViewStyles.h
//  OCJSDemo
//
//  Created by JZhou on 2019/4/17.
//  Copyright © 2019年 Dong. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "UIView+AdjustFrame.h"

typedef NS_ENUM(NSUInteger, ZBWebViewPopGesture) {
    ZBWebViewPopGestureAuto = 0,
    ZBWebViewPopGestureNone ,
    ZBWebViewPopGestureClose,
    ZBWebViewPopGestureHide
};

typedef NS_ENUM(NSUInteger,ZBWebViewAnimationType){
    ZBWebViewAnimationTypeAuto = 0,
    ZBWebViewAnimationTypeNone,
    ZBWebViewAnimationTypeSlideInRight,
    ZBWebViewAnimationTypeSlideInLeft,
    ZBWebViewAnimationTypeSlideInTop,
    ZBWebViewAnimationTypeSlideInBottom,
    ZBWebViewAnimationTypeFadeIn,
    ZBWebViewAnimationTypePopIn
};

NS_ASSUME_NONNULL_BEGIN

@interface ZBWebViewStyle : NSObject
-(instancetype)initWithStyles:(NSDictionary *)styles;

@property (assign, nonatomic)ZBWebViewAnimationType animation;

@property (assign, nonatomic, readonly) CGFloat left;
@property (assign, nonatomic, readonly) CGFloat top;
@property (assign, nonatomic, readonly) CGFloat width;
@property (assign, nonatomic, readonly) CGFloat height;
@property (assign, nonatomic, readonly) CGFloat right;
@property (assign, nonatomic, readonly) CGFloat bottom;

@property (assign, nonatomic, readonly) CGFloat opacity;

/*zindex: (Number 类型 )窗口的堆叠顺序值拥有更高堆叠顺序的窗口总是会处于堆叠顺序较低的窗口的前面，拥有相同堆叠顺序的窗口后调用show方法则在前面。*/
@property (assign, nonatomic, readonly) NSInteger zindex;

/*Color 颜色值可取值： "#RRGGBB"格式字符串，如"#FF0000"表示红色； "rgba(R,G,B,A)"，其中R/G/B分别代表红色值/绿色值/蓝色值，正整数类型，取值范围为0-255，A为透明度，浮点数类型，取值范围为0-1（0为全透明，1为不透明），如"rgba(255,0,0,0.5)"，表示红色半透明。 默认值为透明。*/
//窗口的背景颜色
@property (strong, nonatomic, readonly) UIColor *background;
//)窗口顶部背景颜色值
@property (strong, nonatomic, readonly) UIColor *backgroundColorTop;
//窗口底部背景颜色
@property (strong, nonatomic, readonly) UIColor *backgroundColorBottom;
//窗口回弹效果区域的背景
@property (strong, nonatomic, readonly) UIColor *bounceBackground;

/*margin: (String 类型 )窗口的边距
 用于定位窗口的位置，支持auto，auto表示居中。若设置了left、right、top、bottom则对应的边距值失效*/
@property (strong, nonatomic, readonly) NSString *margin;

/*popGesture: (String 类型 )窗口的侧滑返回功能
 可取值： "none"-无侧滑返回功能； "close"-侧滑返回关闭Webview窗口； "hide"-侧滑返回隐藏webview窗口*/
@property (assign, nonatomic, readonly) ZBWebViewPopGesture popGesture;

/*scalable: (Boolean 类型 )窗口是否可缩放
 窗口设置为可缩放（scalable:true）时，用户可通过双指操作放大或缩小页面，此时html页面可通过meta节点设置“name="viewport" content="user-scalable=no"”来限制页面不可缩放。 窗口设置为不可缩放（scalable:false）时，用户不可通过双指操作放大或缩小页面，即使页面中的meta节点也无法开启可缩放功能。 默认值为false，即不可缩放*/
@property (assign, nonatomic, readonly) BOOL scalable;

/*scrollsToTop: (Boolean 类型 )点击设备的状态栏时是否滚动返回至顶部
true表示点击设备的状态栏可以滚动返回至顶部，false表示点击设备的状态栏不可以，默认值为true。 此功能仅iOS平台支持，在iPhone上有且只有一个Webview窗口的scrollsToTop属性值为true时才生效，所以在显示和关闭Webview窗口时需动态更新所有Webview的scrollsToTop值，已确保此功能生效。*/
@property (assign, nonatomic, readonly) BOOL scrollsToTop;

/*position: (WebviewPosition 类型 )Webview窗口的排版位置
 当Webview窗口添加到另外一个窗口中时，排版位置才会生效，排版位置决定子窗口在父窗口中的定位方式。 可取值："static"，控件在页面中正常定位，如果页面存在滚动条则随窗口内容滚动；"absolute"，控件在页面中绝对定位，如果页面存在滚动条不随窗口内容滚动；"dock"，控件在页面中停靠，停靠的位置由dock属性值决定。 默认值为"absolute"。*/
@property (strong, nonatomic, readonly) NSString *position;

/*userSelect: (Boolean 类型 )用户是否可选择内容
可取值： true - 表示可选择内容，用户可通过长按来选择页面内容，如文本内容选择后可以弹出系统复制粘贴菜单； false - 表示不可选择内容，用户不可通过长按来选择页面内容。 默认值为true。 注意：在Web页面中可通过CSS的user-select对单个页面元素进行控制，前提是Webview对象的userSelect设置为true，否则CSS设置的user-select失效。*/
@property (assign, nonatomic, readonly) BOOL userSelect;

/*videoFullscreen: (String 类型 )视频全屏播放时的显示方向
 可取值： "auto": 自动适配，如果当前页面竖屏，则竖屏显示；如果当前页面横盘显示，则横屏；如果当前页面自动感应，则自动感应横竖屏切换。 "portrait-primary": 竖屏正方向； "portrait-secondary": 竖屏反方向，屏幕正方向按顺时针旋转180°； "landscape-primary": 横屏正方向，屏幕正方向按顺时针旋转90°； "landscape-secondary": 横屏方向，屏幕正方向按顺时针旋转270°； "landscape": 横屏正方向或反方向，根据设备重力感应器自动调整； 默认值为“auto”。*/
@property (strong, nonatomic, readonly) NSString *videoFullscreen;

/*Webview窗口自动处理返回键逻辑
 当Webview窗口在显示栈顶，并且Webview窗口中没有调用JS监听返回键（plus.key.addEventListener('backbutton',...)）时按下返回键响应行为。 可取值： "hide" - 隐藏Webview窗口，隐藏动画与上一次调用显示时设置的动画类型相对应（如“slide-in-right”对应的关闭动画为“slid-out-right”）； "close" - 关闭Webview窗口，关闭动画与上一次调用显示时设置的动画类型相对应（如“slide-in-right”对应的关闭动画为“slid-out-right”） ； "none" - 不做操作，将返回键传递给下一Webview窗口处理； "quit" - 退出应用。*/
@property (strong, nonatomic, readonly) NSString *backButtonAutoControl;




@property (strong, nonatomic, readonly) NSString *hardwareAccelerated;


@end

NS_ASSUME_NONNULL_END
