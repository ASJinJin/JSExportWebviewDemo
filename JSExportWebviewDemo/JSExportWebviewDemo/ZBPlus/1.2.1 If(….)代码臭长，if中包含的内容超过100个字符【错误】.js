var str = CodeInfo.FileText;//读取的文件的文本字符串信息
var rows = str.split('\n');

for(var i=0;i<rows.length;i++)
{
    var rowStr = rows[i].replace(/^\s*|\s*$/,'');//替换前后空格
    if((new RegExp("^(if \()(.*)\)").test(rowStr)) || (new RegExp("^(if\()(.*)\)").test(rowStr)))
    {
        var tailStr = rowStr.substr(rowStr.indexOf("("),);
        var n=0;
        var m=0;
        var expressionStr = '';
        
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
            var errStr = "if(…)条件表达式超长," + "第" + (i+1).toString() + "行\n"
            CodeInfo.AddResult(i+1,errStr,1);
        }
    }
}


