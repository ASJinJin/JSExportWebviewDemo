var str = CodeInfo.FileText;
var rows = str.split('\n');
for(var i=0;i<rows.length;i++){
    var rowStr = rows[i].replace(/^\s*|\s*$/,'');
    if(rowStr.indexOf("public") >= 0){
        if(i-2 >= 0){
            if(rows[i-2].indexOf("///") == -1 && rows[i-1].indexOf("///") == -1){
                var errStr = "公共函数、属性、类未加xml注释，位置：第" + (i+1).toString() + "行\n";
                CodeInfo.AddResult(i+1,errStr,1);
            }
        }else if(i-1 >= 0){
            if(rows[i-1].indexOf("///") == -1){
                var errStr = "公共函数、属性、类未加xml注释，位置：第" + (i+1).toString() + "行\n";
                CodeInfo.AddResult(i+1,errStr,1);
            }
        }else{
            if(rowStr.indexOf("///") == -1){
                var errStr = "公共函数、属性、类未加xml注释，位置：第" + (i+1).toString() + "行\n";
                CodeInfo.AddResult(i+1,errStr,1);
            }
        }
    }
}
