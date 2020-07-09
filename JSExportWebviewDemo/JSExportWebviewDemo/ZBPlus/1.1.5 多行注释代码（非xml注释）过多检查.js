var str = CodeInfo.FileText;
var rows = str.split('\n');
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
                    var errStr = "非xml注释代码过长：从第" + (i - nn2 + 1).toString() + "行至第" + (i+1).toString() +"行\n";
                    CodeInfo.AddResult(i - nn2 + 1,errStr,1);
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
            var errStr = "非xml注释代码过长：从第" + (i - nn + 1).toString() + "行至第" + i.toString() +"行\n";
            CodeInfo.AddResult(i - nn + 1,errStr,1);
            nn = 0;
        }else{
            nn = 0;
        }
    }
}

