/**
 * Created by sine on 2017/11/28.
 */


$.fn.dataFill = function(data){
    if(data)
    this.each(function(){
        this.outerHTML = this.outerHTML.replace(/{{(.+)}}/g, function(match, va){
            va = va.replace(/^\s+/, "").replace(/\s+$/, ""); //去首尾空白
            try{
                if(!va) throw 1;
                va = va.split(".");
                var temp = data;
                for(var i = 0; i < va.length; i++){
                    temp = temp[va[i]];
                }
                return String(temp);
            }catch(e){
                console.error(e);
                return match;
            }
        });
    });
};