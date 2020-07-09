//
//  ZBWebViewStyles.m
//  OCJSDemo
//
//  Created by JZhou on 2019/4/17.
//  Copyright © 2019年 Dong. All rights reserved.
//

#import "ZBWebViewStyle.h"

@implementation ZBWebViewStyle
-(instancetype)initWithStyles:(NSDictionary *)styles{
    self = [self init];
    if (self) {
        _left = [styles[@"left"] floatValue] * KMainWidth;
        _right = [styles[@"right"] floatValue] * KMainWidth;
        _top = [styles[@"top"] floatValue] * KMainHeight;
        _bottom = [styles[@"bottom"] floatValue] * KMainHeight;
        _width = [styles[@"width"] floatValue] * KMainWidth;
        _height = [styles[@"height"] floatValue] *KMainHeight;
        _opacity = [styles[@"opacity"] floatValue];
        
        _zindex = [styles[@"zindex"] integerValue];
        _background = [self colorFromString:styles[@"background"]];
        if ([styles[@"popGesture"] isEqualToString:@"none"]) {
            _popGesture = ZBWebViewPopGestureNone;
        }else if ([styles[@"popGesture"] isEqualToString:@"close"] || [styles[@"popGesture"] isEqualToString:@"auto"]){
            _popGesture = ZBWebViewPopGestureClose;
        }else if ([styles[@"popGesture"] isEqualToString:@"hide"]){
            _popGesture = ZBWebViewPopGestureHide;
        }
        
        _animation = ZBWebViewAnimationTypeNone;
        
    }
    return self;
}


- (UIColor *)colorFromString:(NSString *)colorName{
    
    if ([UIColor respondsToSelector:NSSelectorFromString([NSString stringWithFormat:@"%@Color",colorName])]) {
        return [UIColor performSelector:NSSelectorFromString([NSString stringWithFormat:@"%@Color",colorName]) withObject:nil];
    }
    
    if (colorName.length >= 6) {
        return [self colorWithHexString:colorName alpha:1];
    }
    
    return [UIColor clearColor];
}

- (UIColor *)colorWithHexString:(NSString *)color alpha:(CGFloat)alpha
{
    //删除字符串中的空格
    
    NSString *cString = [[color stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]] uppercaseString];
    
    // String should be 6 or 8 characters
    
    if ([cString length] < 6)
    
    {
        
        return [UIColor clearColor];
        
    }
    
    // strip 0X if it appears
    
    //如果是0x开头的，那么截取字符串，字符串从索引为2的位置开始，一直到末尾
    
    if ([cString hasPrefix:@"0X"])
    
    {
        
        cString = [cString substringFromIndex:2];
        
    }
    
    //如果是#开头的，那么截取字符串，字符串从索引为1的位置开始，一直到末尾
    
    if ([cString hasPrefix:@"#"])
    
    {
        
        cString = [cString substringFromIndex:1];
        
    }
    
    if ([cString length] != 6)
    
    {
        
        return [UIColor clearColor];
        
    }
    
    
    
    // Separate into r, g, b substrings
    
    NSRange range;
    
    range.location = 0;
    
    range.length = 2;
    
    //r
    
    NSString *rString = [cString substringWithRange:range];
    
    //g
    
    range.location = 2;
    
    NSString *gString = [cString substringWithRange:range];
    
    //b
    
    range.location = 4;
    
    NSString *bString = [cString substringWithRange:range];
    
    
    
    // Scan values
    
    unsigned int r, g, b;
    
    [[NSScanner scannerWithString:rString] scanHexInt:&r];
    
    [[NSScanner scannerWithString:gString] scanHexInt:&g];
    
    [[NSScanner scannerWithString:bString] scanHexInt:&b];
    
    return [UIColor colorWithRed:((float)r / 255.0f) green:((float)g / 255.0f) blue:((float)b / 255.0f) alpha:alpha];
}
@end
