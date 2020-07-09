//
//  UIView+AdjustFrame.h   Adjust：调整
//  Hello
//
//  Created by HEYANG on 16/1/20.
//  Copyright © 2016年 HEYANG. All rights reserved.
//

#import <UIKit/UIKit.h>

#define KMainWidth ([UIScreen mainScreen].bounds.size.width)
#define KMainHeight ([UIScreen mainScreen].bounds.size.height)

typedef NS_OPTIONS(NSUInteger, UIBorderSideType) {
    UIBorderSideTypeAll  = 0,
    UIBorderSideTypeTop = 1 << 0,
    UIBorderSideTypeBottom = 1 << 1,
    UIBorderSideTypeLeft = 1 << 2,
    UIBorderSideTypeRight = 1 << 3,
};


@interface UIView (AdjustFrame)

//类别可以拓展属性，但是不能生成set和get方法
@property (assign, nonatomic) CGFloat x;
@property (assign, nonatomic) CGFloat y;
@property (assign, nonatomic) CGFloat width;
@property (assign, nonatomic) CGFloat height;
@property (assign, nonatomic) CGSize size;
@property (assign, nonatomic) CGPoint origin;

@property (nonatomic, setter=setLf_left:, getter=lf_left) CGFloat left;    ///< Shortcut for frame.origin.x.
@property (nonatomic, setter=setLf_top:, getter=lf_top) CGFloat top;     ///< Shortcut for frame.origin.y
@property (nonatomic, setter=setLf_right:, getter=lf_right) CGFloat right;   ///< Shortcut for frame.origin.x + frame.size.width
@property (nonatomic, setter=setLf_bottom:, getter=lf_bottom) CGFloat bottom;  ///< Shortcut for frame.origin.y + frame.size.height

- (UIView *)borderForColor:(UIColor *)color borderWidth:(CGFloat)borderWidth borderType:(UIBorderSideType)borderType;

@end
