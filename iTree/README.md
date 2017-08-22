


#构建

iTree.create(origin, target, data, loader)

origin: HTMLElement

target: HTMLElement

origin和target都是DOM元素（也可以是jquery封装过的），初始时节点会被加载到origin内

data: Object 初始化节点需要的数据，结构为：
{
    name: String 节点名称,
    children: Array 子节点集合,
    data: Object 节点包含的附加数据,在动态加载节点时会传入,
    loaded: Boolean 表示此节点是否执行过动态加载（如果不希望此节点执行动态加载，可设置为true）
}

loader: function(data, node) 在动态加载时调用，加载依据此函数返回的数据，结构同 iTree.create 参数中的 data
    data: Object 此节点的附加数据
    node: HTMLElement 执行加载的节点


iTree.create 返回一个如下结构的对象，通过此对象可以执行相关的操作
{
    toggle: function()  交换选中节点,
    setLoader: function(loader)  设置动态加载函数,
    getSelectedTree: function() 获取 target 元素中的节点的附加数据组成的树形结构数据,
    getSelectedList: function() 获取 target 元素中的节点的附加数据组成的数组结构数据
}


#示例

var tree = iTree.create(origin, target, [
    {
        name: "1111",
        children: [
            {
                name: "1111-2222",
            }
        data: {fruit: "orange", other: "balabala"}
    },
    {
        name: "2222",
    }
], function(data, node){    //对于没有加载过的节点，点击展开后，都会加载一个名为 "apple" 的节点
    return {name: "apple"}
});

将选中的节点在指定的两个元素（origin, target）间交换
tree.toggle()




#Tip
jquery需要在iTree.js之前导入
节点选择操作支持 ctrl 和 shift 辅助多选




