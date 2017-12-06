/**
 * Created by sine on 2017/11/28.
 */

(function(){

    $.fn.i_fill = function(data){
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
        return this;
    };

    $.fn.i_bind = function(data){
        var jcol = this;    //jquery collection

        var refreshTree = {};           //{pathNode: ..., __$i_nodes__: [{node, paths, template}]}
        Object.defineProperty(refreshTree, "add", {
            configurable: false,
            enumerable: false,
            writable: false,
            value: function(path, nodeObj){
                if(nodeObj){
                    var temp = this;
                    $(path).each(function(){
                        if(!temp.hasOwnProperty(this)){
                            temp[this] = (function(){
                                var o = {};
                                Object.defineProperty(o, "__$i_nodes__", {
                                    enumerable: false,
                                    configurable: false,
                                    writable: false,
                                    value: []
                                });
                                return o;
                            })();
                        }
                        temp = temp[this];
                    });
                    temp.__$i_nodes__.push(nodeObj);
                }
            }
        });
        (function(){
            var reg_var = /{{(.+?)}}/g, reg_ps = /^\s+/, reg_fs = /\s+$/;
            var attributes = [];
            jcol.add(jcol.find("*")).each(function(){
                attributes.append(this.attributes);
            });

            $(attributes).each(function(){
                var str = this.value;
                // var node = this;
                var paths = [];
                var index = 0;
                var template = str.replace(reg_var, function(match, pathStr){
                    pathStr = pathStr.replace(reg_ps, "").replace(reg_fs, "");
                    if(pathStr){
                        paths.push(pathStr.split("."));
                    }
                    return "{" + index++ + "}";
                });
                var nodeObj = {node: this, paths: paths, template: template};
                paths.forEach(function(path){
                    refreshTree.add(path, nodeObj);
                });

                // if(reg_var.test(this.value)){
                //     var pathStr = this.value.replace(reg_ps, "").replace(reg_fs, "");
                //     var path = pathStr.split(".");
                //     // refreshTree.add(path, {node: this, path: path});
                //     refreshTree.add(path, this);
                // }
            });
            jcol.findText().each(function(){
                var str = this.data;
                // var node = this;
                var paths = [];
                var index = 0;
                var template = str.replace(reg_var, function(match, pathStr){
                    pathStr = pathStr.replace(reg_ps, "").replace(reg_fs, "");
                    if(pathStr){
                        paths.push(pathStr.split("."))
                    }
                    // var path = pathStr.split(".");
                    // refreshTree.add(path, node);
                    return "{" + index++ + "}";
                });
                var nodeObj = {node: this, paths: paths, template: template};
                paths.forEach(function(path){
                    refreshTree.add(path, nodeObj);
                });
                // if(reg_var.test(this.data)){
                //     var pathStr = this.data.replace(reg_ps, "").replace(reg_fs, "");
                //     var path = pathStr.split(".");
                //     // refreshTree.add(path, {node: this, path: path});
                //     refreshTree.add(path, this);
                // }
            });
        })();

        var refer;  //参照，以检查脏值
        if(data){
            refer = deepCopy(data);
            (function(obj, path){
                var f = arguments.callee;
                Object.keys(obj).forEach(function(key){
                    (function(key){
                        var value = obj[key];
                        Object.defineProperty(obj, key, {
                            enumerable: true,
                            configurable: true,
                            get: function(){
                                return value;
                            },
                            set: function(v){
                                value = v;
                                if(v instanceof Object){
                                    var _path = path.slice();
                                    _path.push(key);
                                    f(v, _path);
                                }
                                //脏值检查，刷新涉及的节点
                                try{
                                    var oldValue = refer;
                                    for(var i = 0; i < path.length; i++){
                                        oldValue = oldValue[path[i]];
                                    }
                                    var oldObj = oldValue;
                                    oldValue = oldValue[key];
                                    if(oldValue !== v){
                                        //执行刷新
                                        var _path = path.slice();
                                        _path.push(key);
                                        refresh(refreshTree, data, _path);
                                        oldObj[key] = v;
                                    }
                                }catch(e){ }
                            }
                        });
                    })(key);
                });
            })(data, []);

            refresh(refreshTree, data, []);

            // this.add().each(function(){
            //     var $i_data = {}; //将附加到元素上的数据
            //     //提取特征
            //
            //
            //
            //     //根据特征填充数据
            // });
        }
    };

    $.fn.findText = function(){
        var collection = [];
        this.add(this.find("*")).each(function(){
            var len = this.childNodes.length, node;
            for(var i = 0; i < len; i++){
                node = this.childNodes[i];
                if(node.nodeType === 3)
                    collection.push(node);
            }
        });
        return $(collection);
    };
    $.fn.textChildren = function(){
        var collection = [];
        this.add().each(function(){
            var len = this.childNodes.length, node;
            for(var i = 0; i < len; i++){
                node = this.childNodes[i];
                if(node.nodeType === 3)
                    collection.push(node);
            }
        });
        return $(collection);
    };


    function deepCopy(obj){
        if(obj instanceof Object){
            var res;
            if(obj instanceof Function)
                res = (function(){});
            else if(obj instanceof Array)
                res = [];
            else
                res = {};
            Object.keys(obj).forEach(function(key){
                res[key] = deepCopy(obj[key]);
            });
            return res;
        }else{
            return obj;
        }
    }

    function refresh(nodeTree, data, path){
        try{
            var nodeObj = nodeTree;
            $(path).each(function(){
                nodeObj = nodeObj[this];
            });
            if(!nodeObj) return;

            // var value = data;
            // $(path).each(function(){
            //     value = data[this];
            // });
            //T/ODO if value is a function

            // var textValue = String(value);
            $(nodeObj.__$i_nodes__).each(function(){
                var _this = this;
                var node = this.node;
                var vkey = ({2: "value", 3: "data"})[node.nodeType];
                // node[vkey] = textValue ;
                node[vkey] = this.template.replace(/{(\d)}/g, function(match, index){
                    var path = _this.paths[index];
                    var value = data;
                    try{
                        $(path).each(function(){
                            value = value[this];
                        });
                    }catch(e){}
                    return String(value);
                });
            });

            // $(Object.keys(nodeObj)).each(function(){
            //     refresh(nodeObj, value, [this]);
            // });

            $(Object.keys(nodeObj)).each(function(){
                refresh(nodeObj, data, [this]);
            });
        }catch(e){}
    }

    if(!Array.prototype.append)
    Array.prototype.append = function(){
        for(var i = 0; i < arguments.length; i++){
            var arr = arguments[i];
            if(isNaN(arr.length)){
                return;
            }else{
                for(var j = 0; j < arr.length; j++){
                    this.push(arr[j]);
                }
            }
        }
    };
})();




var i_module = {
    parseAsCss: function(obj){
        var s = "";
        for(var key in obj){
            s += key + ": " + obj[key] + ";";
        }
        return s.replace(/[A-Z]/g, function(match){
            return "-" + String.fromCharCode(match.charCodeAt(0) | 32);
        });
    }
};
if(!$i)
    var $i = i_module;

