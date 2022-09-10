/**
 @Name：业务系统 创建系统树
 @Author：胡伟
 @Site：
 @License：
 */

layui.define(['jquery'], function (exports) {
    var _$ = layui.jquery;
    //声明一个loading变量
    var _loading = null;
    /**
     * 请求菜单树
     * @param {请求路径} url
     * @param {返回数据格式} type
     */
    var _reqMenu = function (url, type, pId) {
        _loading = layer.msg("加载中", { icon: 16, shade: 0.01 });
        var _str = "";

        _$.ajax({
            type: "GET",
            crossDomain: true,
            async: false,
            xhrFields: {
                withCredentials: true
            },
            dataType: type,
            url: url,
            success: function (result) {
                if (type == "text") {
                    result = JSON.parse(result);
                }
                //console.log(result);
                try {
                    var data = JSON.parse(result.Data);
                    if (data instanceof Array) {
                        layer.close(_loading);
                        //console.log(_creatSysMenu(result.resultdata, "0", "0"));
                        _str = _creatSysMenu(data, pId, pId, 1);
                    } else {
                        layer.close(_loading);
                        layer.msg("未获取菜单树");
                    }
                } catch (e) {
                    layer.close(_loading);
                    layer.msg("" + JSON.stringify(e));
                }
            },
            error: function (_e) {
                layer.close(_loading);
                layer.msg("" + JSON.stringify(_e));
            }
        });

        return _str;
    }


    /**
     * 获取菜单树
     * @param {数据列表} list
     * @param {父级标识} parentId
     * @param {根标识} rootId
     */
    var _creatSysMenu = function (list, parentId, rootId, level) {
        var _key = "PID";
        var _type = "TYPE";
        var _childList = new Array();
        var _str = "";
        list.forEach(function (_val, _index, _arr) {
            if (_val[_key] == parentId) {
                _childList.push(_val);
            }
        });

        if (_childList.length > 0) {
            _childList.forEach(function (_val, _index, _arr) {
                if (_val[_type] == "1") {
                    if (_val[_key] == rootId) {
                        _str += "<li data-name='{0}' code='{1}' class='layui-nav-item'>".replace("{0}", _val["NAME"]).replace("{1}", _val["CODE"]);
                        _str += "<a href='javascript:;' class='' >";
                        _str += "<i class='layui-icon {0}'></i><cite>{1}</cite>".replace("{0}", (_val["ICON"] == null) ?
                            "layui-icon-set-fill" : _val["ICON"]).replace("{1}", _val["NAME"]);
                        _str += "</a>";
                        _str += "<dl class='layui-nav-child'>";
                        _str += _creatSysMenu(list, _val["ID"], rootId, level+1);
                        _str += "</dl>";
                        _str += "</li>";
                    } else {
                        _str += "<dd data-name='{0}' code='{1}'>".replace("{0}", _val["NAME"]).replace("{1}", _val["CODE"]);
                        _str += "<a href='javascript:;' class=''>";
                        _str += "<i class='layui-icon {0}'></i><cite>{1}</cite>".replace("{0}", (_val["ICON"] == null) ?
                            "layui-icon-set-fill" : _val["ICON"]).replace("{1}", _val["NAME"]);
                        _str += "</a>";
                        _str += "<dl class='layui-nav-child'>";
                        _str += _creatSysMenu(list, _val["ID"], rootId, level+1);
                        _str += "</dl>";
                        _str += "</dd>";

                    }
                } else if (_val[_type] == "0") {
                    if (_val[_key] == rootId) {
                        _str += "<li data-name='{0}' code='{1}' class='layui-nav-item'>".replace("{0}", _val["NAME"]).replace("{1}", _val["CODE"]);
                        _str += "<a class='' " + (_val["TARGET"]=="_blank"?"target=\"_blank\"":" lay-")+"href='{0}' >".replace("{0}", (_val["PATH"] == null) ? "javascript:;" : _val["PATH"]);
                        _str += "<i class='layui-icon {0}'></i><cite>{1}</cite>".replace("{0}", (_val["ICON"] == null) ?
                            "layui-icon-set-fill" : _val["ICON"]).replace("{1}", _val["NAME"]);
                        _str += "</a>";
                        _str += "</li>";
                    } else {
                        _str += "<dd data-name='{0}' code='{1}'>".replace("{0}", _val["NAME"]).replace("{1}", _val["CODE"]);
                        if (_val["TARGET"] == "_blank")
                            _str += "<a class='menu_a"+level+"' target='_blank' href='{0}' >".replace("{0}", (_val["PATH"] == null) ? "javascript:;" : _val["PATH"]);
                        else
                            _str += "<a class='' lay-href='{0}' >".replace("{0}", (_val["PATH"] == null) ? "javascript:;" : wrapToken(gPath + _val["PATH"]));
                        _str += "<i class='layui-icon {0}'></i><cite>{1}</cite>".replace("{0}", (_val["ICON"] == null) ?
                            "layui-icon-set-fill" : _val["ICON"]).replace("{1}", _val["NAME"]);
                        _str += "</a>";
                        _str += "</dd>";
                    }
                }
            });
            return _str;
        } else {
            return _str;
        }
        return _str;
    }

    var createMenu = function (list) {
        var _key = "PID";
        var _type = "TYPE";
        var _str = "";
        var _val = list[0];
        _str += "<dd data-name='{0}' code='{1}'>".replace("{0}", _val["NAME"]).replace("{1}", _val["CODE"]);
        if (_val["TARGET"] == "_blank")
            _str += "<a class='menu_a1' target='_blank' href='{0}' >".replace("{0}", (_val["PATH"] == null) ? "javascript:;" : _val["PATH"]);
        else
            _str += "<a class='' lay-href='{0}' >".replace("{0}", (_val["PATH"] == null) ? "javascript:;" : wrapToken(gPath + _val["PATH"]));
        _str += "<i class='layui-icon {0}'></i><cite>{1}</cite>".replace("{0}", (_val["ICON"] == null) ?
            "layui-icon-set-fill" : _val["ICON"]).replace("{1}", _val["NAME"]);
        _str += "</a>";
        _str += "</dd>";
    }

    //对外暴露一个接口
    exports('mytree', {
        req: _reqMenu,
        createMenu: createMenu
    });

});