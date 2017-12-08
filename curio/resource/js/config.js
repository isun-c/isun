/**
 * Created by sine on 2016/11/16.
 */



//初始化
!function(){
    var root = document.documentElement;
    root.style["font-size"] = root.offsetWidth / 750 * 100 + "px";
    window.addEventListener("resize", function(){
        root.style["font-size"] = root.offsetWidth / 750.* 100 + "px";
    });
}();
window.addEventListener("load", function(){
    $("._back").each(function(){
        this.onclick = function(){
            window.history.back();
        };
    });
});

var isun = {
    /**
     * @param el 被操作元素
     * @param callBack(d) 回调，传入相对上一触发点的位置偏移。 d: [dx, dy]
     * @param end_callBack(last_d) 在触摸事件结束时回调，最后二次move触发相对上一次的位置偏移 last_d: [dx, dy]
     */
    drag_measure: function(el, callBack, end_callBack){
        if(!callBack){
            return;
        }
        if(el.length){
            el = el[0];
        }

        el.drag_measure_func = {};

        var px, py;
        var last_d = [];

        // var sw = true;

        el.addEventListener("touchstart", el.drag_measure_func.start = function(e){
            var touch = e.touches[0];
            px = touch.pageX;
            py = touch.pageY;
        });
        el.addEventListener("touchmove", el.drag_measure_func.move = function(e){
            // if(sw){
            //     se = false;
            // }else{
            //     se = true;
            //     return;
            // }
            var touch = e.touches[0];
            var x = touch.pageX;
            var y = touch.pageY;
            var dx = x - px;
            var dy = y - py;
            if(dx || dy){
                last_d[0] = last_d[1];
                callBack(last_d[1] = [dx, dy]);
                px = x;
                py = y;
            }
        });
        el.addEventListener("touchend", el.drag_measure_func.end = function(e){
            if(end_callBack && last_d[0])
                end_callBack([(last_d[0][0] + last_d[1][0]) / 2, (last_d[0][1] + last_d[1][1]) / 2]);
            last_d = [];
        })
    },
    /**
     * 清除添加到元素上的触摸测量事件
     * @param el
     */
    clear_drag_measure: function(el){
        if(el.length){
            el = el[0];
        }
        if(!el.drag_measure_func){
            return;
        }
        el.removeEventListener("touchstart", el.drag_measure_func.start);
        el.removeEventListener("touchmove", el.drag_measure_func.move);
        el.removeEventListener("touchend", el.drag_measure_func.end);
    },
    /**
     * 触摸滑动一定距离触发事件
     * @param.up    向上滑动触发的事件
     * @param.down
     * @param.left
     * @param.right
     * @param.distance  触发所需滑动的距离
     * @param.el 处理元素
     */
    slide_crossly: function(param){
        if(!param || !param.el){
            return;
        }
        if(!param.distance){
            param.distance = 100;
        }
        param.distance *= Math.sqrt(window.innerWidth / 750);

        var el = param.el;
        if(el.length){
            el = el[0];
        }

        var px, py;
        el.slide_crossly_func = {};

        el.addEventListener("touchstart", el.slide_crossly_func.start = function(e){
            var touch = e.touches[0];
            px = touch.pageX;
            py = touch.pageY;
        });
        el.addEventListener("touchmove", el.slide_crossly_func.move = function(e){
            var touch = e.touches[0];
            var x = touch.pageX;
            var y = touch.pageY;
            if(param.up    && (py - y > param.distance)){
                param.up();
            }
            if(param.down  && (y - py > param.distance)){
                param.down();
            }
            if(param.left  && (px - x > param.distance)){
                param.left();
            }
            if(param.right && (x - px > param.distance)){
                param.right();
            }
        });
    },
    clear_slide_crossly: function(el){
        el.removeEventListener("touchstart", el.slide_crossly_func.start);
        el.removeEventListener("touchmove", el.slide_crossly_func.move);
    },
    flash: function(el, interval, range){
        if(!el){
            return;
        }
        if(!range || range instanceof Array){
            range = [0.5, 1];
        }

        if(!interval){
            interval = 500;
        }
        el = $(el);
        function a(){
            el.animate({
                opacity: range[0] + ""
            }, interval, b);
        }
        function b(){
            el.animate({
                opacity: range[1] + ""
            }, interval, a);
        }
        a();
    },
    /**
     * 手势判定并执行对应函数
     * @param param
     */
    gesture:function(param){

    },
    /**
     * 手势验证函数集合
     */
    gesture_checkings:{

    },
    /**
     * 使元素可被拖拽
     * @param param
     * @param param.el 被操作元素
     * @param param.callback(d) 在拖拽过程中的每一帧回调  d: [dx, dy]
     * @param param.end_callback(d, ) 在拖拽结束时回调
     *
     */
    dragable:function(param){

    },
    // page_onscrollbottom: function(callback){
    //     if(callback)
    //     window.addEventListener("scroll", function(){
    //         if(window.scrollY +  window.innerHeight == document.documentElement.offsetHeight){
    //             console.log("到底啦");
    //
    //         }
    //     });
    // }
};