/**
 * Created by sine on 2017/11/28.
 */

(function(){

    $.fn.i_fill = function(data){
        if(!data) return;
        var jcol = this;
        jcol.filter("*[i-if]").add(jcol.find("*[i-if]")).each(function(){
            var exp = this.getAttribute("i-if");
            if(!accessProp(data, exp)){
                $(this).remove();
            }
        });

        var for_col = jcol.filter("*[i-for]").add(jcol.find("*[i-for]"));
        for_col = for_col.not(for_col.find("*[i-for]"));
        (function(data, for_col){
            var handler = arguments.callee;
            for_col.each(function(){
                var exp = this.getAttribute("i-for");
                this.removeAttribute("i-for");
                var temp;
                var var_exp, path_exp;
                temp = exp.split(" in ");
                var_exp = temp[0];
                path_exp = temp[1];
                if(!path_exp)
                    throw new Error("i-for expression parse error");
                var for_data = Object.create(data);
                var item_key, index_key;
                temp = var_exp.replace(/^\s*\(?([^()]*)\)?\s*$/, function(){
                    return arguments[1];
                }).split(",");
                item_key = temp[0];
                index_key = temp[1];
                if(!item_key)
                    throw new Error("i-for expression parse error");
                if(!index_key)
                    index_key = "$index";
                item_key = item_key.trim();
                index_key = index_key.trim();

                var collection;
                try{
                    collection = accessProp(data, path_exp);
                }catch(e){
                    console.error(e);
                    return;
                }
                if(collection.length > 0){
                    var template = this.outerHTML;
                    var res = "";
                    for(var i = 0; i < collection.length; i++){
                        for_data[item_key] = collection[i];
                        for_data[index_key] = i + 1;
                        var child_for_col = $(this).find("*[i-for]");
                        if(child_for_col.length){
                            var templateNode = $(this.cloneNode(true));
                            child_for_col = templateNode.find("*[i-for]");
                            child_for_col = child_for_col.not(child_for_col.find("*[i-for]"));
                            handler(for_data, child_for_col);
                            template = templateNode[0].outerHTML;
                        }
                        res += template.replace(/{{(.+?)}}/g, function(match, va){
                            return String(accessProp(for_data, va));
                        });
                    }
                    if(res){
                        this.insertAdjacentHTML("afterend", res);
                        $(this).remove();
                    }
                }
            });
        })(data, for_col);

        jcol.each(function(){
            this.outerHTML = this.outerHTML.replace(/{{(.+?)}}/g, function(match, va){
                va = va.trim();
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

    if(Object.defineProperty)
    $.fn.i_bind = function(data){
        if(!data) return;

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

        // this.filter("*[i-for]").add(this.find("*[i-for]")).each(function(){
        //
        // });

        (function(){
            var reg_var = /{{(.+?)}}/g;
            var attributes = [];
            jcol.add(jcol.find("*")).each(function(){
                attributes.append(this.attributes);
            });
            $(attributes).each(function(){
                var str = this.value;
                var paths = [];
                var index = 0;
                var template = str.replace(reg_var, function(match, pathStr){
                    pathStr = pathStr.trim();
                    if(pathStr){
                        paths.push(pathStr.split("."));
                    }
                    return "{" + index++ + "}";
                });
                var nodeObj = {node: this, paths: paths, template: template};
                paths.forEach(function(path){
                    refreshTree.add(path, nodeObj);
                });
            });
            jcol.findText().each(function(){
                var str = this.data;
                var paths = [];
                var index = 0;
                var template = str.replace(reg_var, function(match, pathStr){
                    pathStr = pathStr.trim();
                    if(pathStr){
                        paths.push(pathStr.split("."));
                    }
                    return "{" + index++ + "}";
                });
                var nodeObj = {node: this, paths: paths, template: template};
                paths.forEach(function(path){
                    refreshTree.add(path, nodeObj);
                });
            });
        })();

        var refer;  //参照，以检查脏值
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

        this.filter("*[i-model]").add(this.find("*[i-model]")).each(function(){
            if(this.value !== undefined){
                var path = this.getAttribute("i-model").trim().split(".");
                $(this).on("input", function(){
                    editProp(data, path, this.value);
                });
                this.setAttribute("value", this.value);
                var nodeObj = {node: this.getAttributeNode("value"), paths:[path], template: "{0}"};
                refreshTree.add(path, nodeObj);
            }
        });
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


    /**
     * 解析表达式
     * @param context 变量上下文
     * @param exp
     */
    function exp_parse(context, exp){
        //TODO
    }

    function accessProp(obj, exp){
        var path = exp instanceof Array ? exp : exp.trim().split(".");
        var temp = obj;
        for(var i = 0; i < path.length; i++){
            temp = temp[path[i]];
        }
        return temp;
    }

    function editProp(obj, exp, value){
        var path = exp instanceof Array ? exp : exp.trim().split(".");
        var temp = obj;
        for(var i = 0; i < path.length - 1; i++){
            temp = temp[path[i]];
        }
        temp[path[i]] = value;
        return path;
    }

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

            $(nodeObj.__$i_nodes__).each(function(){
                var _this = this;
                var node = this.node;
                var vkey = ({2: "value", 3: "data"})[node.nodeType];
                node[vkey] = this.template.replace(/{(\d+)}/g, function(match, index){
                    var path = _this.paths[index];
                    var value = data;
                    try{
                        $(path).each(function(){
                            value = value[this];
                        });
                    }catch(e){}
                    return String(value);
                });
                if(node.nodeName === "value")
                    node.ownerElement.value = node[vkey];
            });

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
    if(!String.prototype.trim)
    String.prototype.trim = function(){
        return this.replace(/^\s+/, "").replace(/\s+$/, "");
    };
    if(Object.create)
    Object.create = function(obj){
        var res = {};
        res.__proto__ = obj;
        return res;
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


