//
//  UIView+AdjustFrame.m  Adjust：调整
//  Hello
//
//  Created by HEYANG on 16/1/20.
//  Copyright © 2016年 HEYANG. All rights reserved.
//

#import "UIView+AdjustFrame.h"

@implementation UIView (AdjustFrame)

#pragma mark - adjust_x
-(void)setX:(CGFloat)adjust_x{
    CGRect frame = self.frame;
    frame.origin.x = adjust_x;
    self.frame = frame;
}

-(CGFloat)x{
    return self.frame.origin.x;
}

#pragma mark - adjust_y
-(void)setY:(CGFloat)adjust_y{
    CGRect frame = self.frame;
    frame.origin.y = adjust_y;
    self.frame = frame;
}

- (CGFloat)y
{
    return self.frame.origin.y;
}

#pragma mark - adjust_width
-(void)setWidth:(CGFloat)adjust_width{
    CGRect frame = self.frame;
    frame.size.width = adjust_width;
    self.frame = frame;
}
- (CGFloat)width
{
    return self.frame.size.width;
}

#pragma mark - adjust_height
-(void)setHeight:(CGFloat)adjust_height{
    CGRect frame = self.frame;
    frame.size.height = adjust_height;
    self.frame = frame;
}
- (CGFloat)height
{
    return self.frame.size.height;
}

#pragma mark - adjust_size
-(void)setSize:(CGSize)adjust_size{
    CGRect frame = self.frame;
    frame.size = adjust_size;
    self.frame = frame;
}
- (CGSize)size
{
    return self.frame.size;
}

#pragma mark - adjust_origin
-(void)setOrigin:(CGPoint)adjust_origin{
    CGRect frame = self.frame;
    frame.origin = adjust_origin;
    self.frame = frame;
}
- (CGPoint)origin
{
    return self.frame.origin;
}

- (CGFloat)lf_left {
    return self.frame.origin.x;
}

- (void)setLf_left:(CGFloat)x {
    CGRect frame = self.frame;
    frame.origin.x = x;
    self.frame = frame;
}

- (CGFloat)lf_top {
    return self.frame.origin.y;
}

- (void)setLf_top:(CGFloat)y {
    CGRect frame = self.frame;
    frame.origin.y = y;
    self.frame = frame;
}

- (CGFloat)lf_right {
    return self.frame.origin.x + self.frame.size.width;
}

- (void)setLf_right:(CGFloat)right {
    CGRect frame = self.frame;
    frame.origin.x = right - frame.size.width;
    self.frame = frame;
}

- (CGFloat)lf_bottom {
    return self.frame.origin.y + self.frame.size.height;
}

- (void)setLf_bottom:(CGFloat)bottom {
    CGRect frame = self.frame;
    frame.origin.y = bottom - frame.size.height;
    self.frame = frame;
}

- (UIView *)borderForColor:(UIColor *)color borderWidth:(CGFloat)borderWidth borderType:(UIBorderSideType)borderType {
    
    if (borderType == UIBorderSideTypeAll) {
        self.layer.borderWidth = borderWidth;
        self.layer.borderColor = color.CGColor;
        return self;
    }
    
    
    /// 左侧
    if (borderType & UIBorderSideTypeLeft) {
        /// 左侧线路径
        [self.layer addSublayer:[self addLineOriginPoint:CGPointMake(0.f, 0.f) toPoint:CGPointMake(0.0f, self.frame.size.height) color:color borderWidth:borderWidth]];
    }
    
    /// 右侧
    if (borderType & UIBorderSideTypeRight) {
        /// 右侧线路径
        [self.layer addSublayer:[self addLineOriginPoint:CGPointMake(self.frame.size.width, 0.0f) toPoint:CGPointMake( self.frame.size.width, self.frame.size.height) color:color borderWidth:borderWidth]];
    }
    
    /// top
    if (borderType & UIBorderSideTypeTop) {
        /// top线路径
        [self.layer addSublayer:[self addLineOriginPoint:CGPointMake(0.0f, 0.0f) toPoint:CGPointMake(self.frame.size.width, 0.0f) color:color borderWidth:borderWidth]];
    }
    
    /// bottom
    if (borderType & UIBorderSideTypeBottom) {
        /// bottom线路径
        [self.layer addSublayer:[self addLineOriginPoint:CGPointMake(0.0f, self.frame.size.height) toPoint:CGPointMake( self.frame.size.width, self.frame.size.height) color:color borderWidth:borderWidth]];
    }
    
    return self;
}

- (CAShapeLayer *)addLineOriginPoint:(CGPoint)p0 toPoint:(CGPoint)p1 color:(UIColor *)color borderWidth:(CGFloat)borderWidth {
    
    /// 线的路径
    UIBezierPath * bezierPath = [UIBezierPath bezierPath];
    [bezierPath moveToPoint:p0];
    [bezierPath addLineToPoint:p1];
    
    CAShapeLayer * shapeLayer = [CAShapeLayer layer];
    shapeLayer.strokeColor = color.CGColor;
    shapeLayer.fillColor  = [UIColor clearColor].CGColor;
    /// 添加路径
    shapeLayer.path = bezierPath.CGPath;
    /// 线宽度
    shapeLayer.lineWidth = borderWidth;
    return shapeLayer;
}

@end
