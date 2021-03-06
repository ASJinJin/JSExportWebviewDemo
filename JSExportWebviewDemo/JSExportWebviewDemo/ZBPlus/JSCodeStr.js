//检查 1.1.1公共【函数】、【属性】、【类】必须包含有效的XML注释
function checkPublicCode(){
    var str = Plus.codeString;//读取的文件的文本字符串信息
    var rows = str.split('\n');
    var errArray = [];//存放检查结果
    //alert("文本总行数："+rows.length.toString()+"----\n"); //打印文本总行数
    
    for(var i=0;i<rows.length;i++){
        var rowStr = rows[i].replace(/^\s*|\s*$/,'');
        if(rowStr.indexOf("public") >= 0){
            if(i-2 >= 0){
                if(rows[i-2].indexOf("///") == -1 && rows[i-1].indexOf("///") == -1){
                    var errStr = "公共代码未加xml注释⚠️⚠️⚠️⚠️：第" + (i+1).toString() + "行\n";
                    errArray.push(errStr);
                }
            }else if(i-1 >= 0){
                if(rows[i-1].indexOf("///") == -1){
                    var errStr = "公共代码未加xml注释⚠️⚠️⚠️⚠️：第" + (i+1).toString() + "行\n";
                    errArray.push(errStr);
                }
            }else{
                if(rowStr.indexOf("///") == -1){
                    var errStr = "公共代码未加xml注释⚠️⚠️⚠️⚠️：第" + (i+1).toString() + "行\n";
                    errArray.push(errStr);
                }
            }
        }
    }
    
    if(errArray.length>0){
        alert("=====" + errArray);//打印输出 "公共【函数】、【属性】、【类】必须包含有效的XML注释"未添加xml注释
    }
}

//1.1.2 公共函数、属性、类名称首字母必须大写代码检查
function checkPublicCodeName(){
    var str = Plus.codeString;//读取的文件的文本字符串信息
    var rows = str.split('\n');
    var errArray = [];//存放检查
    //alert("文本总行数："+rows.length.toString()+"----\n"); //打印文本总行数
    
    for(var i=0;i<rows.length;i++){
        var rowStr = rows[i].replace(/^\s*|\s*$/,'');//替换前后空格
        if(rowStr.startsWith("public ")){
            var tailSubStr = rowStr.substr(6,);
            let reg1 = /\s{0,1}[A-Za-z0-9_]*\(.*/;//函数正则
            if(reg1.test(tailSubStr))//匹配到函数行
            {
                let r = /[A-Za-z0-9_]*\(.*\)/;
                var fncNames = tailSubStr.match(r);
                if(fncNames.length >0){
                    //alert("函数的名称++" + fncNames);
                   if(new RegExp("^[A-Z]+.*$").test(fncNames[0])){
                       //alert("第" + (i+1).toString() + "行函数名称是以大写字母开头------" + fncNames[0]);
                   }else{
                       var errStr = "函数名称不是以大写字母开头=======" + "第" + (i+1).toString() + "行\n"
                       errArray.push(errStr);
                   }
                }
            }
            
            let reg2 = /\s{0,1}(class ){1}.*/g;//类正则
            if(reg2.test(tailSubStr))//匹配到类的行
            {
                var className =  tailSubStr.substr(tailSubStr.indexOf("class ")+6,);
                //alert("类名称className------" + className);
                var capitalizedStr = className.substr(0,1);
                if(new RegExp("^[A-Z]$").test(capitalizedStr)){
                    //alert("第" + (i+1).toString() + "行类名称是以大写字母开头------" + className);
                }else{
                    var errStr = "类名称不是以大写字母开头=======" + "第" + (i+1).toString() + "行\n"
                    errArray.push(errStr);
                }
 
           }
            
            let reg3 = /(set\s*\{)+|(get\s*\{)+/g;
            if(reg3.test(tailSubStr))//匹配到属性的行
            {
                //alert("0000"+ tailSubStr);
                let r = /[A-Za-z0-9_]*\{.*/;
                var fncNames = tailSubStr.match(r);
                if(fncNames.length >0){
                    //alert("属性的名称++" + fncNames);
                   if(new RegExp("^[A-Z]+.*$").test(fncNames[0])){
                       //alert("第" + (i+1).toString() + "行属性名称是以大写字母开头------" + fncNames[0]);
                   }else{
                       var errStr = "属性名称不是以大写字母开头=======" + "第" + (i+1).toString() + "行\n"
                       errArray.push(errStr);
                   }
                }
            }
        }
    }
    
    if(errArray.length>0)
    {
        alert('找到====' + errArray);//打印输出 "公共【函数】、【属性】、【类】必须包含有效的XML注释"未添加xml注释
    }
}

//1.1.3成员类名超过30个字符
function checkPublicCodeNameLength(){
    var str = Plus.codeString;//读取的文件的文本字符串信息
    var rows = str.split('\n');
    var errArray = [];//存放检查结果
    
    for(var i=0;i<rows.length;i++){
        var rowStr = rows[i].replace(/^\s*|\s*$/,'');//替换前后空格
        if(rowStr.startsWith("public ") || rowStr.startsWith("protected ") || rowStr.startsWith("private ")){
            var herfStr = rowStr;
            if(rowStr.indexOf(":") != -1){
                herfStr= herfStr.substr(0,herfStr.indexOf(":"));
            }
            if(herfStr.indexOf("(") != -1){
                herfStr= herfStr.substr(0,herfStr.indexOf("("));
            }
            if(herfStr.indexOf("{") != -1){
                herfStr= herfStr.substr(0,herfStr.indexOf("{"));
            }
            if(herfStr.indexOf("=") != -1){
                herfStr= herfStr.substr(0,herfStr.indexOf("="));
            }
            
            let r = /[A-Za-z0-9_]{1,}/g;
            var fncNames = herfStr.match(r);
            if(fncNames.length > 0){
                //alert(herfStr + "=============" + fncNames);
                for(var j = 0;j<fncNames.length;j++){
                    if(fncNames[j].length > 30){
                        //alert("超长名称"+fncNames[j]+"==="+rowStr);
                        var errStr = "第" + (i + 1).toString() + "行:" + "超长名称:"+ fncNames[j] + "\n";
                        errArray.push(errStr);
                    }
                }
            }
        }
        
    }
    alert("成员、类等名称超过了30个字符：\n" + errArray);
}

//1.1.4检查函数体内的代码块是否超过150行
function checkCodeFunctionLines(){
    var str = Plus.codeString;//读取的文件的文本字符串信息
    
    var strArr = str.split('\n');
    if(strArr.length <150)return;4

    var errArray = [];
    var rowNums = [];

    for(var i=1;i<strArr.length;i++)
    {
        var rowStr = strArr[i].replace(/^\s*|\s*$/,'').replace(/\r/g,'').replace(/\n/g,'');
      
        if(rowStr.indexOf('{') != -1){
            rowNums.push(i);//存放出现“{”字符的行号
        }

        if(rowStr.indexOf('}') != -1)//遍历到“}”
        {
            if(rowNums.length > 0)
            {
                var first = rowNums.pop();//遍历到“}”时从数组rowNums中取出与它匹配的"{"的行号
                if(i - first > 150)
                {
                    var errStr = "函数代码块超过15行=======" + "从第" + (first + 1).toString() + "行" + "至第"+(i + 1).toString() + "行\n";
                    var lastRowStr = strArr[first-1].replace(/\s*/,'').replace(/\r/g,'').replace(/\n/g,'');
                    var firstRowStr = strArr[first].replace(/\s*/,'').replace(/\r/g,'').replace(/\n/g,'');
                    
                    //根据"{"的上一行最后字符是否是")"字符串判断函数
                    if (lastRowStr.substr(lastRowStr.length - 1 ,1) === ")"){
                        errArray.push(errStr);
                    }else{
                        var patt = /\)\s*\{/g;
                        //根据"{"所在行是否包含")"字符串判断函数
                        if((patt.test(firstRowStr)))
                        {
                            errArray.push(errStr);
                        }
                    }
                        
                }
            }
        }
    }
    if(errArray.length >0)
    {
        alert("========"+errArray);
    }
}

//1.1.5 多行注释代码（非xml注释）过多检查
function checkManyLinesNoteCode(){
    var str = Plus.codeString;
    var rows = str.split('\n');
    var errArray = [];
    //alert("文本总行数："+rows.length.toString()+"----\n");
    
    var nn = 0;
    var nn2 = 0;
    for(var i=0;i<rows.length;i++){
        var rowStr = rows[i].replace(/^\s*|\s*$/,'');

        if(rowStr.indexOf("/*") >= 0){
            nn2++;
        }else{
            if(nn2>0){
                if(rowStr.indexOf("*/") >= 0){
                    if(nn2>=10){
                        var errStr = "注释代码过长警告⚠️⚠️⚠️⚠️：从第" + (i - nn2 + 1).toString() + "行至第" + (i+1).toString() +"行\n";
                        errArray.push(errStr);
                    }
                    nn2=0;
                }
                nn2++;
            }else{
                nn2 = 0;
            }
        }
        
        if((rowStr.indexOf("//") >= 0) && (rowStr.indexOf("///") == -1)){
            nn++;
        }else{
            if(nn>=10){
                var errStr = "注释代码过长警告⚠️⚠️⚠️⚠️：从第" + (i - nn + 1).toString() + "行至第" + i.toString() +"行\n";
                errArray.push(errStr);
                nn = 0;
            }else{
                nn = 0;
            }
        }
    }
    
    if(errArray.length>0){
        alert("========" + errArray);
    }
}

//1.1.6检查循环体中是否出现有 .GetTable或.GetValue 等函数
function checkLoopBody(){
    var str = Plus.codeString;//读取的文件的文本字符串信息
    
    var rows = str.split('\n');
    var errArray = [];//存放检查结果数据
    
    var loopBodys = [];
    for(var i=0;i<rows.length;i++){
        var rowStr = rows[i].replace(/^\s*|\s*$/,'');//替换前后空格
        if(rowStr.startsWith("foreach (") || rowStr.startsWith("for (") || rowStr.startsWith("while (") || rowStr.startsWith("foreach(") || rowStr.startsWith("for(") || rowStr.startsWith("while("))
        {
            if(rowStr.indexOf("{") !=-1)
            {
                loopBodys.push(rows.slice(i,));
            }else{
                loopBodys.push(rows.slice(i+1,));
            }
        }
    }
    if(loopBodys.length >0){
        for (var j = 0;j<loopBodys.length;j++)
        {
            var n=0;
            var m=0;
            var subArr = loopBodys[j];
            for(var k = 0;k<subArr.length;k++){
                
                if(subArr[k].indexOf('{') != -1)
                {
                    n++;
                }
                
                if(subArr[k].indexOf('}') != -1)
                {
                    m++;
                }
                
                if(subArr[k].indexOf('.GetTable') != -1 || subArr[k].indexOf('.GetValue') != -1)
                {
                    var errStr = "第" + (rows.length - subArr.length + k + 1).toString() + "行:" + "循环体中出现.GetTable或.GetValue函数\n";
                    errArray.push(errStr);
                }
                
                if(n == m && n>0 && m>0)
                {
                    break;
                }
            }
        }
    }
    
    alert("======" + errArray);
}

//1.2.1 If(….)代码臭长，if中包含的内容超过100个字符【错误】
function checkConditionalCode()
{
        var str = Plus.codeString;//读取的文件的文本字符串信息
        var rows = str.split('\n');
        var errArray = [];//存放检查 if中包含的内容超过100个字符
        //alert("文本总行数："+rows.length.toString()+"----\n"); //打印文本总行数
        
        for(var i=0;i<rows.length;i++)
        {
            var rowStr = rows[i].replace(/^\s*|\s*$/,'');//替换前后空格
            if((new RegExp("^(if \()(.*)\)").test(rowStr)) || (new RegExp("^(if\()(.*)\)").test(rowStr)))
            {
                 var tailStr = rowStr.substr(rowStr.indexOf("("),);
                 var n=0;
                 var m=0;
                 var expressionStr = '';
                 //alert('=====' + tailStr);
                 
                 for (var j = 0;j<tailStr.length;j++)
                 {
                     if(tailStr[j] == '(')
                     {
                         n++;
                     }
                     if(tailStr[j] == ')')
                     {
                         m++;
                     }
                     if(n == m)
                     {
                         expressionStr = tailStr.substr(1,j);
                         break;
                     }
                 }
                 
                 if(expressionStr.length > 100)
                 {
                     var errStr = "if条件表达式超长=======" + "第" + (i+1).toString() + "行\n"
                     //alert(errStr + expressionStr);
                     errArray.push(errStr);
                 }
            }
        }
        
        if(errArray.length > 0){
            alert(errArray)
        }
}

//1.2.2 单行代码过长检查
function checkSingleLineCode(){
    var str = Plus.codeString;//读取的文件的文本字符串信息
    
    var rows = str.split('\n');
    
    var errArray = [];//存放检查 1.2.2 单行代码超长， 超过了200字 【错误】
    //alert("文本总行数："+rows.length.toString()+"----\n"); //打印文本总行数
    
    for(var i=0;i<rows.length;i++){
        var rowStr = rows[i].replace(/\s+/g,'');
        if(rowStr.length > 200){//单行超过100个字符
            var errStr = "单行代码超长⚠️⚠️⚠️⚠️第" + (i+1).toString() + "行字符长度：" + rowStr.length.toString()+"\n";
            errArray.push(errStr);
        }
    }
    
    if(errArray.length >0){
        alert("======" + errArray);
    }
}
