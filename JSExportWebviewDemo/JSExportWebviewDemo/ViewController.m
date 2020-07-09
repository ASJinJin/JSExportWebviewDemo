//
//  ViewController.m
//  JSExportWebviewDemo
//
//  Created by JZhou on 2019/6/24.
//  Copyright © 2019 ZBIntel. All rights reserved.
//

#import "ViewController.h"
#import "ZBAppCore.h"

@interface ViewController ()<UIWebViewDelegate>
@property (nonatomic,strong)ZBWebView *webView;
@end

@implementation ViewController

- (void)loadView{
    [super loadView];
    [ZBAppCore shareInstance].viewController = self;
//    NSString *str;
//    [str lowercaseString]
}

- (void)viewDidLoad {
    [super viewDidLoad];
    
    self.webView = [[ZBWebView alloc] initWithFrame:self.view.frame htmlUrl:@"index.html"];
    
//    self.webView.delegate = self;
    NSURL *htmlURL = [[NSBundle mainBundle] URLForResource:@"index.html" withExtension:nil];
    //    NSURL *htmlURL = [NSURL URLWithString:@"http://www.baidu.com"];
    NSURLRequest *request = [NSURLRequest requestWithURL:htmlURL];
    
    self.webView.backgroundColor = [UIColor clearColor];
    // UIWebView 滚动的比较慢，这里设置为正常速度
    self.webView.scrollView.decelerationRate = UIScrollViewDecelerationRateNormal;
    [self.webView loadRequest:request];
    [self.view addSubview:self.webView];

}



@end
