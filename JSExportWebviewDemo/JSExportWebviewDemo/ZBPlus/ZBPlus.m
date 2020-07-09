#import "ZBPlus.h"
#import "ZBJSExport.h"
@interface ZBPlus () <ZBJSExport>{
    
    NSString *s1;
    NSString *s2;
    NSString *s3;
}

@end

@implementation ZBPlus
@synthesize webview=_webview;
@synthesize codeString = _codeString;

//- (instancetype)initWithWebView:(Class)webview{
//    self = [super init];
//    if (self) {
//        _webview = webview;
//    }
//    return self;
//}

- (instancetype)initWithWebView:(ZBWebView*)webview{
    self = [super init];
    if (self) {
        _webview = webview;
    }
    
    switch (1) {
        case 0:
            {
                NSLog(@"");
            }
            break;
            
        default:
            break;
    }
    return self;
}

-(NSString *)codeString{
//    NSString *csPath = [[NSBundle mainBundle] pathForResource:@"RegisterList" ofType:@"cs"];
    NSString *csPath = [[NSBundle mainBundle] pathForResource:@"RegistersAdd" ofType:@"cs"];
    NSString *csString = [NSString stringWithContentsOfFile:csPath encoding:NSUTF8StringEncoding error:nil];
//    NSLog(@"----%@",csString);
    return csString;
}
@end
