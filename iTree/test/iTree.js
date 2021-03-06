

var iTree = {};

(function(){

    iTree.create = create;

    function create(root_origin, root_target, initData, loader){

        if(!root_origin || !root_target || !initData)
            return null;

        $(root_origin).html("").append("<ul/>").addClass("itree_root");
        $(root_target).html("").append("<ul/>").addClass("itree_root");

        var tree = {
            toggle: function(){
                if(selectedNodes.length == 0){
                    return;
                }
                var roots = $(root_origin).add(root_target);
                var target = roots.not(roots.has(selectedNodes[0]));

                $(selectedNodes).each(function(){
                    $(this).removeClass("selected");
                    addChild(target, this);
                });
                selectedNodes.length = 0;
            },
            loader: loader,
            setLoader: function(loader){
                this.loader = loader;
            },
            /**
             * 获取选中目标data组成的树结构
             * @param root
             */
            getSelectedTree: function(root){
                root = $(root);
                var _this = this;
                var result = [];
                if(!root.length)
                    root = $(root_target);
                root.children("ul").children("li").each(function(){
                    var data = this.data;
                    result.push(data);
                    if($(this).children("ul").children("li").length){
                        data.children = _this.getSelectedTree(this);
                    }
                });
                return result;
            },
            /**
             * 获取选中目标data组成的数组结构
             * @returns {Array}
             */
            getSelectedList: function(){
                var result = [];
                $(root_target).find("li").each(function(){
                    result.push(this.data);
                });
                return result;
            }
        };

        var selectedNodes = [];
        //每个层级id的初始值是1
        var level_nodeidList = [];


        load(root_origin, initData);

        //loadNode = load;
        //加载子节点
        //@node 将节点加载到此节点下
        //@data 加载数据数组
        function load(node, data, callback){
            node = $(node)[0];
            if(node.loaded){
                // if(callback)
                //     callback();
                return;
            }

            if(data.length > 0){
                $(data).each(function(i){
                    var child = createNode(this, node);
                    addChild(null, child, node);

                    if(this.loaded === true)
                        child[0].loaded = true;
                    if(this.children && this.children.length)
                        load(child, this.children, callback);
                });
            }

            node.loaded = true;
            if(callback)
                callback();
        }

        function choosing(node){
            $(node).addClass("selected");
            selectedNodes.push(node);
        }
        function dechoosing(node){
            $(node).removeClass("selected");
            var index = -1;
            for(var i = 0; i < selectedNodes.length; i++){
                if(selectedNodes[i] === node){
                    index = i;
                    break;
                }
            }
            if(index >= 0)
                selectedNodes.splice(index, 1);
        }

        //被点击的元素是节点下的 a 标签
        function click_choose(e){
            e = e || event;
            var node = this.parentNode;
            if(e.ctrlKey){
                if($(node).is(".selected")){
                    dechoosing(node);
                    return;
                }
                //无法同时选中不同层级的目标
                if(selectedNodes.length > 0){
                    if(selectedNodes[0].parentNode !== node.parentNode){
                        return;
                    }
                }
                choosing(node);
            }else if(e.shiftKey){
                if($(node).is(".selected") || selectedNodes.length != 1)
                    return;
                var ahead = selectedNodes[0], after;
                if(ahead.parentNode !== node.parentNode)
                    return;
                if($(ahead).index() > $(node).index()){
                    after = ahead;
                    ahead = node;
                }else{
                    after = node;
                }
                selectedNodes.length = 0;
                $(ahead).nextAll().not($(after).nextAll()).add(ahead).each(function(){
                    choosing(this);
                });
            }else{
                //没有按下ctrl时
                if(selectedNodes.length > 0){
                    $(selectedNodes).each(function(){
                        $(this).removeClass("selected");
                    });
                    selectedNodes.length = 0;
                }
                choosing(node);
            }
        }

        //节点下的 i 标签触发
        function click_spread(){
            if($(this).parent().is(".spreading")){
                $(this).parent().removeClass("spreading");
            }else{
                $(this).parent().addClass("spreading");
            }
            var node = $(this).parent()[0];
            if(tree.loader){
				var loadData = tree.loader(node.data, node, function(data){load(node, data)});
				if(loadData)
					load(node, loadData);
            }
        }

        /**
         * 添加节点
         */
        function addChild(root, child, parent){
            root = $(root);
            child = $(child);
            parent = $(parent);

            //如果目标中以有此子节点,则只添加此节点的子节点到此对应的子节点中
            var child2;
            if(parent.length){
                child2 = parent.find("li[nodeid='" + child.attr("nodeid") + "']");
            }else{
                child2 = root.find("li[nodeid='" + child.attr("nodeid") + "']");
            }

            if(child2.length){
                child.children("ul").children("li").each(function(){
                    addChild(null, this, child2);
                });

                while(true){
                    var temp = child.parent().parent("li");
                    child.remove();
                    if(!temp.length || temp.children("ul").children().length){
                        break;
                    }
                    child = temp;
                }
                return;
            }

            if(parent.length){
                child.appendTo(parent.children("ul"));
                return;
            }

            var origin_pnode = child.parent().parent("li");
            //用户在处理之后，如果父节点空了，方便清除之
            var origin_pnode2 = origin_pnode;
            while(true){
                if(origin_pnode.length > 0){
                    var targetNode = root.find("li[nodeid='" + origin_pnode.attr("nodeid") + "']");
                    //目标树中不存在对应父节点，复制之
                    if(targetNode.length === 0){
                        var target_pnode = cloneNode(origin_pnode);
                        child.appendTo(target_pnode.children("ul"));
                        child = target_pnode;
                        origin_pnode = origin_pnode.parent().parent("li");
                        continue;
                    }
                }else{
                    targetNode = root;
                }
                child.appendTo(targetNode.children("ul"));
                break;
            }
            while(origin_pnode2.length > 0){
                if(origin_pnode2.children("ul").children().length == 0){
                    var temp = origin_pnode2.parent().parent("li");
                    origin_pnode2.remove();
                    origin_pnode2 = temp;
                }else{
                    break;
                }
            }
        }

        /**
         * 克隆一个节点，不复制支节点
         * 操作和返回都为 jquery 节点对象
         */
        function cloneNode(node){
            node = $(node);
            var newNode = node.clone(true);
            newNode.children("ul").empty();
            newNode[0].data = node[0].data;
            newNode[0].loaded = node[0].loaded;
            return newNode;
        }

        /**
         * 创建节点
         * 操作及返回节点是 jquery 节点
         */
        function createNode(data, parent){
            parent = $(parent);
            var node = $("<li/>");
            var nodeid;
            //如果父节点是根
            if(!parent.length || !parent.attr("nodeid")){
                level_nodeidList[0] = ~~level_nodeidList[0] + 1;
                nodeid = level_nodeidList[0] + "";
            }else{
                var pnodeid = parent.attr("nodeid");
                var id_index = pnodeid.split("_").length;
                level_nodeidList[id_index] = ~~level_nodeidList[id_index] + 1;
                nodeid = pnodeid + "_" + level_nodeidList[id_index];
            }
            node.attr("nodeid", nodeid);
            node[0].data = data.data;
            //TODO 对于name的取值不能通用
            node.append($("<i/>").on("click", click_spread))
                .append($("<a/>").on("click", click_choose).html(data.name))
                .append($("<ul/>"));
            return node;
        }

        return tree;
    }
})();



