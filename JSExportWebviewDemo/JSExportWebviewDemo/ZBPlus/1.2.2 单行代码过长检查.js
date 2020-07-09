var str = CodeInfo.FileText;//读取的文件的文本字符串信息
var rows = str.split('\n');

for(var i=0;i<rows.length;i++){
    var rowStr = rows[i].replace(/\s+/g,'');
    if(rowStr.length > 200){//单行超过100个字符
        var errStr = "单行代码超长:第" + (i+1).toString() + "行字符长度：" + rowStr.length.toString()+"\n";
        CodeInfo.AddResult(i + 1,errStr,1);
    }
}

