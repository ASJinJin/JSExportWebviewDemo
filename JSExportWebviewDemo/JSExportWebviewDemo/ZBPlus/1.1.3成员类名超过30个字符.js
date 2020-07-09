var str = CodeInfo.FileText;//读取的文件的文本字符串信息
var rows = str.split('\n');

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
            for(var j = 0;j<fncNames.length;j++){
                if(fncNames[j].length > 30){
                    var errStr = "成员、类等名称超过30个,"+"位置：第" + (i + 1).toString() + "行:" + "超长字符:"+ fncNames[j] + "\n";
                    CodeInfo.AddResult(i+1,errStr,1);
                }
            }
        }
    }
}

  
