/**
 @Name：业务系统 封装ajax使用
 @Author：胡伟
 @Site：
 @License：
 */

layui.define(['jquery'], function (exports) {
    var _$ = layui.jquery;

    /**
     * @param {ajax 的配置参数 请参照参ajax数表对照} options                 
     * @param {成功回调参数} success
     * @param {失败回调参数} error
     */
    var _request = function (options, success, error) {

        delete options.success;
        delete options.error;
        //loading 设置
        var _loading = layer.msg("加载中......", { icon: 16, shade: 0.01 });
        //ajax请求
        return _$.ajax(_$.extend({
            async: false,
            timeout: 30000,
            type:"GET",
            success: function (result) {
                layer.close(_loading);
                try {
                    success(result)
                } catch (_e) {
                    layer.msg(_e);
                }
                
            },
            error: function (e) {
                layer.close(_loading);
                try {
                    error(e)
                } catch (_e) {
                    layer.msg(_e);
                }
                
            }
        }, options))
    }

    //对外暴露一个接口
    exports('myajax', {
        req: _request
    });
});