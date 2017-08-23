


var isun = {
    /**
	 * 使目标元素可以被拖动
     * @param target 被拖动的目标
     * @param act 触发拖动的目标
	 * 如<div><p></p></div>，要通过p拖动div，则如此调用： drag(div, p)
     */
	drag: function (target, act){
		act || (act = target);
		var attachMethod = act.addEventListener ? function(el, type, fn){
			el.addEventListener(type, fn);
		} : function(el, type, fn){
			el.attachEvent("on" + type, fn);
		};
		var removeMethod = act.removeEventListener ? function(el, type, fn){
			el.removeEventListener(type, fn);
		} : function(el, type, fn){
			el.detachEvent("on" + type, fn);
		};

		var styler = target.currentStyle ? function(property){
			return target[property];
		} : function(property){
			return window.getComputedStyle(target, null)[property];
		};

		var ox, oy;
		var tox, toy;

		var html = document.documentElement;

		attachMethod(act, "mousedown", down);

		function down(e){
			e || (e = event);
			ox = e.screenX;
			oy = e.screenY;
			tox = parseInt(styler("left")) || 0;
			toy = parseInt(styler("top")) || 0;

			attachMethod(html, "mousemove", move);
			attachMethod(html, "mouseup", up);
		}
		function move(e){
			e || (e = event);
			var dx = e.screenX - ox, dy = e.screenY - oy;
			target.style["left"] = tox + dx + "px";
			target.style["top"] = toy + dy + "px";
		}
		function up(e){
			removeMethod(html, "mousemove", move);
			removeMethod(html, "mouseup", up);
		}
	},
	//// jq_based ///////////////////////////////////////////////////////////////////////////////////////////
	/**
	 * 创建轮播图
     * @param param
	 * 		param.pagi_list 切换按钮元素数组
     * 		param.sliders 被切换的元素数据  结构： [{direction: "horizontal|vertical", length: 滑动距离, wrapper: wrapper}, ...]
     * 		param.act_method 切换扭被激活的方式，默认 click
     * 		param.start_index 初始选中的下标
     * 		param.max_index 最大下标
	 * 		param.slide_time 自动切换间隔时间， 0 表示不自动切换。单位：毫秒
     */
	make_slider: function (param){
		var pagi_list = param.pagi_list;
		var sliders = param.sliders;
		var act_method = param.act_method;
		var start_index = param.start_index;
		var max_index = param.max_index;
		var slide_time = param.slide_time;

		act_method || (act_method = "click");
		max_index || (max_index = pagi_list.length - 1);
		if(max_index < 0)
			return;
		start_index = start_index || 0;
		var pre_index = start_index;

		$(pagi_list[start_index]).addClass("selected");

		if(pagi_list){
			$(pagi_list).each(function(index){
				$(this).on(act_method, function(){
					if(slide_interval){
						clearInterval(slide_interval);
					}
					slide(index);
					slide_interval = setInterval(function(){
						slide(pre_index + 1);
					}, slide_time);
				});
			});
		}

		$(sliders).each(function(){
			var wrapper = $(this.wrapper)[0];
			wrapper.slideTo = function(i){
				slide(i);
			};
			wrapper.preSlide = function(){
				slide(pre_index - 1);
			};
			wrapper.nextSlide = function(){
				slide(pre_index + 1);
			};
		});

		if(slide_time){
			var slide_interval = setInterval(function(){
				slide(pre_index + 1);
			}, slide_time);
		}

		function slide(index){
			if(pre_index == index || index < 0)
				return;
			while(index > max_index){
				index = start_index + index - max_index - 1;
			}
			pre_index = index;
			if(pagi_list)
				$(pagi_list).not($(pagi_list[index]).addClass("selected")).removeClass("selected");
			for(var i = 0; i < sliders.length; i++){
				!function(i){
					var slider = sliders[i];
					var len = parseFloat(slider.length);
					var unit = slider.length.slice((len + "").length);

					$(slider.wrapper).stop();
					if(slider.direction != "vertical"){
						$(slider.wrapper).animate({
							"margin-left": - len * index + unit
						});
					}else{
						$(slider.wrapper).animate({
							"margin-top": - len * index + unit
						});
					}
				}(i);
			}
		}
		slide(pre_index);
	},
    /**
	 * 切割指定空间的字符串
     * @param str
     * @param _len 空间长度
     * @param appendix 超过空间长度后添加的后缀
     * @returns {*}
     */
	text_space: function(str, _len, appendix){
        if(!str || typeof(str) != "string"){
            return str;
        }
        if(!appendix){
        	appendix = "";
		}
        var len = 0;
        for(var i = 0; i < str.length; i++){
            if(str.charCodeAt(i) <= 8000){
                len++;
            }else{
                len += 2;
            }
            if(len > _len){
                return str.substring(0, i) + appendix;
            }else if(len == _len){
                return str.substring(0, i + 1) + appendix;
            }
        }
        return str;
    },
    /**
	 * 绑定选项卡的标签与对应的窗体
     * @param labs
     * @param contents
     */
	nav_binding: function(labs, contents){

        if(labs.length != contents.length){
            return;
        }

        labs = $(labs);
        contents = $(contents);

        labs.each(function(i){
            this.onmouseover = function(){
                labs.not($(this).addClass("selected")).removeClass("selected");
                contents.not($(contents[i]).css("display", "block")).css("display", "none");
            };
        });
    }
};


