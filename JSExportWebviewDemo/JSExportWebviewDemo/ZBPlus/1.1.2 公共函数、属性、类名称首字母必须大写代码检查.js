var str = CodeInfo.FileText;//读取的文件的文本字符串信息
var rows = str.split('\n');

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
               if(!(new RegExp("^[A-Z]+.*$").test(fncNames[0]))){
                   var errStr = "公共函数名称不是以大写字母开头" + "位置：第" + (i+1).toString() + "行\n";
                   CodeInfo.AddResult(i+1,errStr,1);
               }
            }
        }
        
        let reg2 = /\s{0,1}(class ){1}.*/g;//类正则
        if(reg2.test(tailSubStr))//匹配到类的行
        {
            var className =  tailSubStr.substr(tailSubStr.indexOf("class ")+6,);
            var capitalizedStr = className.substr(0,1);
            if(!(new RegExp("^[A-Z]$").test(capitalizedStr))){
                var errStr = "公共类名称不是以大写字母开头" + "位置：第" + (i+1).toString() + "行\n";
                CodeInfo.AddResult(i+1,errStr,1);
            }
       }
        
        let reg3 = /(set\s*\{)+|(get\s*\{)+/g;
        if(reg3.test(tailSubStr))//匹配到属性的行
        {
            let r = /[A-Za-z0-9_]*\{.*/;
            var fncNames = tailSubStr.match(r);
            if(fncNames.length >0){
               if(!(new RegExp("^[A-Z]+.*$").test(fncNames[0]))){
                   var errStr = "公共属性名称不是以大写字母开头" + "位置：第" + (i+1).toString() + "行\n";
                   CodeInfo.AddResult(i+1,errStr,1);
               }
            }
        }
    }
}
