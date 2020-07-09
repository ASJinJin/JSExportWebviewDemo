var str = CodeInfo.FileText;//读取的文件的文本字符串信息
var rows = str.split('\n');
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
                CodeInfo.AddResult((rows.length - subArr.length + k + 1),errStr,1);
            }
            
            if(n == m && n>0 && m>0)
            {
                break;
            }
        }
    }
}

    

