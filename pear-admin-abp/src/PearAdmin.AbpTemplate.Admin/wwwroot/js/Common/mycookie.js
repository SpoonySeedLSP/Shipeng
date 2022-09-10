/**

 @Name：业务系统 封装cookie使用
 @Author：胡伟
 @Site：
 @License：

 */

layui.define('jquery', function (exports) {
    var _$ = layui.jquery;
    var _defaultEx = 1;
    //console.log(1111);

    /**
     * 写入cookie的值
     * @param {键} key
     * @param {值} value
     * @param {时效} expires
     */
    var _set = function (key, value,path, expires) {
        _$.cookie(key, value, {
            expires: (expires == null || expires == undefined) ? _defaultEx : expires,
            path: (path == null || path == undefined) ? '/' : path});
    }

    /**
     * 通过键获取当前cookie的值 返回一个有值或者null对象
     * @param {键} key
     */
    var _get = function (key) {
        return _$.cookie(key) == undefined ? null : _$.cookie(key);
    }

    /**
     * 通过键删除cookie的值
     * @param {any} key
     */
    var _delete = function (key) {
        _$.cookie(key, null);
    }

    //对外暴露一个接口
    exports('mycookie', {
        set: _set,
        get: _get,
        delete: _delete
    });
})