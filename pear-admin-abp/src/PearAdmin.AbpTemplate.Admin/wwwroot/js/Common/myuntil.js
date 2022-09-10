/**
 @Name：业务系统 工具函数
 @Author：胡伟
 @Site：
 @License：
 */

layui.define(['jquery','form'], function (exports) {
    var _$ = layui.jquery;
    var _form = layui.form;

    /**
     * 根据参数名获取参数值
     * @param {参数名} key
     */
    var _getUrlParam = function (key) {
        var reg = new RegExp("(^|&)" + key + "=([^&]*)(&|$)");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) return unescape(r[2]); return null;

    }


    /**
     * 渲染选项框
     * @param {配置信息} options
     * @param {选项框DOM ID-#id} options.id
     * @param {框架过滤ID} options.layId
     * @param {显示} options.dispalyText
     * @param {值} options.dispalyValue
     * @param {传入数据值 List} options.data
     * @param {默认显示} options.defaultText
     * @param {默认值} options.defaultValue
     * @param {开启默认 Boolen} options.isDefault 
     * @param {选项事件监听 Func} options.fn
     */
    var _renderSelect = function (options) {
        try {
            //配置参数项
            var _options = _$.extend({
                id: "#id",
                layId: "layId",
                dispalyText: "text",
                dispalyValue: "value",
                data: [],
                defaultText: "请选择",
                defaultValue: "",
                isDefault: false,
                fn: null
            }, options);

            _$(_options["id"]).empty();

            //创建模板
            var _tem = "<option value='{0}'>{1}</option>";
            var _create = "";
            //有默认值
            if (_options["isDefault"]) {
                _tem = _tem.replace("{0}", _options["defaultValue"])
                    .replace("{1}", _options["defaultText"]);
                _create += _tem;
            }

            //创建选项框
            if (_options["data"].length > 0) {
                _options["data"].forEach(function (_val, _index, _array) {
                    _tem = "<option value='{0}'>{1}</option>";
                    _create += _tem.replace("{0}", _val[_options["dispalyValue"]])
                        .replace("{1}", _val[_options["dispalyText"]]);
                });
                _$(_options["id"]).append(_create);
                _form.render('select');

            } else {
                console.log("数据为空不进行初始化");
            }

            if (_options["fn"] != null && _options["fn"] != undefined) {
                //选项框监听
                _form.on('select({0})'.replace("{0}", _options["layId"]), function (_data) {
                    _options["fn"](_data.value);
                });
            }

            

        } catch (e) {
            //console.log(e);
            console.log("程序有错我他妈什么都不想做")
        }

    }

    //对外暴露一个接口
    exports('myuntil', {
        getUrlParam: _getUrlParam,
        renderSelect: _renderSelect
    });
});