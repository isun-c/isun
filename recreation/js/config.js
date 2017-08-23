/**
 * Created by sine on 2017/1/9.
 */


var $ = function(qstr){
    return document.querySelector(qstr);
};

var $all = function(qstr){
    return document.querySelectorAll(qstr);
};

var $class = function(className){
    return document.getElementsByClassName(className);
};

var $id = function(id){
    return document.getElementById(id);
};
