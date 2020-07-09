var str = CodeInfo.FileText;//读取的文件的文本字符串信息
var strArr = str.split('\n');
if(strArr.length <150) return;
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
                var errStr = "函数代码块超过150行:" + "从第" + (first + 1).toString() + "行" + "至第"+(i + 1).toString() + "行\n";
                var lastRowStr = strArr[first-1].replace(/\s*/,'').replace(/\r/g,'').replace(/\n/g,'');
                var firstRowStr = strArr[first].replace(/\s*/,'').replace(/\r/g,'').replace(/\n/g,'');
                
                //根据"{"的上一行最后字符是否是")"字符串判断函数
                if (lastRowStr.substr(lastRowStr.length - 1 ,1) === ")")
                {
                    CodeInfo.AddResult(first+1,errStr,1);
                }else{
                    var patt = /\)\s*\{/g;
                    //根据"{"所在行是否包含")"字符串判断函数
                    if((patt.test(firstRowStr)))
                    {
                        CodeInfo.AddResult(first+1,errStr,1);
                    }
                }
            }
        }
    }
}

    
