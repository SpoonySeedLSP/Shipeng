/*[js-md5]*/!function () { "use strict"; function t(t) { if (t) d[0] = d[16] = d[1] = d[2] = d[3] = d[4] = d[5] = d[6] = d[7] = d[8] = d[9] = d[10] = d[11] = d[12] = d[13] = d[14] = d[15] = 0, this.blocks = d, this.buffer8 = l; else if (a) { var r = new ArrayBuffer(68); this.buffer8 = new Uint8Array(r), this.blocks = new Uint32Array(r) } else this.blocks = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]; this.h0 = this.h1 = this.h2 = this.h3 = this.start = this.bytes = this.hBytes = 0, this.finalized = this.hashed = !1, this.first = !0 } var r = "input is invalid type", e = "object" == typeof window, i = e ? window : {}; i.JS_MD5_NO_WINDOW && (e = !1); var s = !e && "object" == typeof self, h = !i.JS_MD5_NO_NODE_JS && "object" == typeof process && process.versions && process.versions.node; h ? i = global : s && (i = self); var f = !i.JS_MD5_NO_COMMON_JS && "object" == typeof module && module.exports, o = "function" == typeof define && define.amd, a = !i.JS_MD5_NO_ARRAY_BUFFER && "undefined" != typeof ArrayBuffer, n = "0123456789abcdef".split(""), u = [128, 32768, 8388608, -2147483648], y = [0, 8, 16, 24], c = ["hex", "array", "digest", "buffer", "arrayBuffer", "base64"], p = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".split(""), d = [], l; if (a) { var A = new ArrayBuffer(68); l = new Uint8Array(A), d = new Uint32Array(A) } !i.JS_MD5_NO_NODE_JS && Array.isArray || (Array.isArray = function (t) { return "[object Array]" === Object.prototype.toString.call(t) }), !a || !i.JS_MD5_NO_ARRAY_BUFFER_IS_VIEW && ArrayBuffer.isView || (ArrayBuffer.isView = function (t) { return "object" == typeof t && t.buffer && t.buffer.constructor === ArrayBuffer }); var b = function (r) { return function (e) { return new t(!0).update(e)[r]() } }, v = function () { var r = b("hex"); h && (r = w(r)), r.create = function () { return new t }, r.update = function (t) { return r.create().update(t) }; for (var e = 0; e < c.length; ++e) { var i = c[e]; r[i] = b(i) } return r }, w = function (t) { var e = eval("require('crypto')"), i = eval("require('buffer').Buffer"), s = function (s) { if ("string" == typeof s) return e.createHash("md5").update(s, "utf8").digest("hex"); if (null === s || void 0 === s) throw r; return s.constructor === ArrayBuffer && (s = new Uint8Array(s)), Array.isArray(s) || ArrayBuffer.isView(s) || s.constructor === i ? e.createHash("md5").update(new i(s)).digest("hex") : t(s) }; return s }; t.prototype.update = function (t) { if (!this.finalized) { var e, i = typeof t; if ("string" !== i) { if ("object" !== i) throw r; if (null === t) throw r; if (a && t.constructor === ArrayBuffer) t = new Uint8Array(t); else if (!(Array.isArray(t) || a && ArrayBuffer.isView(t))) throw r; e = !0 } for (var s, h, f = 0, o = t.length, n = this.blocks, u = this.buffer8; f < o;) { if (this.hashed && (this.hashed = !1, n[0] = n[16], n[16] = n[1] = n[2] = n[3] = n[4] = n[5] = n[6] = n[7] = n[8] = n[9] = n[10] = n[11] = n[12] = n[13] = n[14] = n[15] = 0), e) if (a) for (h = this.start; f < o && h < 64; ++f)u[h++] = t[f]; else for (h = this.start; f < o && h < 64; ++f)n[h >> 2] |= t[f] << y[3 & h++]; else if (a) for (h = this.start; f < o && h < 64; ++f)(s = t.charCodeAt(f)) < 128 ? u[h++] = s : s < 2048 ? (u[h++] = 192 | s >> 6, u[h++] = 128 | 63 & s) : s < 55296 || s >= 57344 ? (u[h++] = 224 | s >> 12, u[h++] = 128 | s >> 6 & 63, u[h++] = 128 | 63 & s) : (s = 65536 + ((1023 & s) << 10 | 1023 & t.charCodeAt(++f)), u[h++] = 240 | s >> 18, u[h++] = 128 | s >> 12 & 63, u[h++] = 128 | s >> 6 & 63, u[h++] = 128 | 63 & s); else for (h = this.start; f < o && h < 64; ++f)(s = t.charCodeAt(f)) < 128 ? n[h >> 2] |= s << y[3 & h++] : s < 2048 ? (n[h >> 2] |= (192 | s >> 6) << y[3 & h++], n[h >> 2] |= (128 | 63 & s) << y[3 & h++]) : s < 55296 || s >= 57344 ? (n[h >> 2] |= (224 | s >> 12) << y[3 & h++], n[h >> 2] |= (128 | s >> 6 & 63) << y[3 & h++], n[h >> 2] |= (128 | 63 & s) << y[3 & h++]) : (s = 65536 + ((1023 & s) << 10 | 1023 & t.charCodeAt(++f)), n[h >> 2] |= (240 | s >> 18) << y[3 & h++], n[h >> 2] |= (128 | s >> 12 & 63) << y[3 & h++], n[h >> 2] |= (128 | s >> 6 & 63) << y[3 & h++], n[h >> 2] |= (128 | 63 & s) << y[3 & h++]); this.lastByteIndex = h, this.bytes += h - this.start, h >= 64 ? (this.start = h - 64, this.hash(), this.hashed = !0) : this.start = h } return this.bytes > 4294967295 && (this.hBytes += this.bytes / 4294967296 << 0, this.bytes = this.bytes % 4294967296), this } }, t.prototype.finalize = function () { if (!this.finalized) { this.finalized = !0; var t = this.blocks, r = this.lastByteIndex; t[r >> 2] |= u[3 & r], r >= 56 && (this.hashed || this.hash(), t[0] = t[16], t[16] = t[1] = t[2] = t[3] = t[4] = t[5] = t[6] = t[7] = t[8] = t[9] = t[10] = t[11] = t[12] = t[13] = t[14] = t[15] = 0), t[14] = this.bytes << 3, t[15] = this.hBytes << 3 | this.bytes >>> 29, this.hash() } }, t.prototype.hash = function () { var t, r, e, i, s, h, f = this.blocks; this.first ? r = ((r = ((t = ((t = f[0] - 680876937) << 7 | t >>> 25) - 271733879 << 0) ^ (e = ((e = (-271733879 ^ (i = ((i = (-1732584194 ^ 2004318071 & t) + f[1] - 117830708) << 12 | i >>> 20) + t << 0) & (-271733879 ^ t)) + f[2] - 1126478375) << 17 | e >>> 15) + i << 0) & (i ^ t)) + f[3] - 1316259209) << 22 | r >>> 10) + e << 0 : (t = this.h0, r = this.h1, e = this.h2, r = ((r += ((t = ((t += ((i = this.h3) ^ r & (e ^ i)) + f[0] - 680876936) << 7 | t >>> 25) + r << 0) ^ (e = ((e += (r ^ (i = ((i += (e ^ t & (r ^ e)) + f[1] - 389564586) << 12 | i >>> 20) + t << 0) & (t ^ r)) + f[2] + 606105819) << 17 | e >>> 15) + i << 0) & (i ^ t)) + f[3] - 1044525330) << 22 | r >>> 10) + e << 0), r = ((r += ((t = ((t += (i ^ r & (e ^ i)) + f[4] - 176418897) << 7 | t >>> 25) + r << 0) ^ (e = ((e += (r ^ (i = ((i += (e ^ t & (r ^ e)) + f[5] + 1200080426) << 12 | i >>> 20) + t << 0) & (t ^ r)) + f[6] - 1473231341) << 17 | e >>> 15) + i << 0) & (i ^ t)) + f[7] - 45705983) << 22 | r >>> 10) + e << 0, r = ((r += ((t = ((t += (i ^ r & (e ^ i)) + f[8] + 1770035416) << 7 | t >>> 25) + r << 0) ^ (e = ((e += (r ^ (i = ((i += (e ^ t & (r ^ e)) + f[9] - 1958414417) << 12 | i >>> 20) + t << 0) & (t ^ r)) + f[10] - 42063) << 17 | e >>> 15) + i << 0) & (i ^ t)) + f[11] - 1990404162) << 22 | r >>> 10) + e << 0, r = ((r += ((t = ((t += (i ^ r & (e ^ i)) + f[12] + 1804603682) << 7 | t >>> 25) + r << 0) ^ (e = ((e += (r ^ (i = ((i += (e ^ t & (r ^ e)) + f[13] - 40341101) << 12 | i >>> 20) + t << 0) & (t ^ r)) + f[14] - 1502002290) << 17 | e >>> 15) + i << 0) & (i ^ t)) + f[15] + 1236535329) << 22 | r >>> 10) + e << 0, r = ((r += ((i = ((i += (r ^ e & ((t = ((t += (e ^ i & (r ^ e)) + f[1] - 165796510) << 5 | t >>> 27) + r << 0) ^ r)) + f[6] - 1069501632) << 9 | i >>> 23) + t << 0) ^ t & ((e = ((e += (t ^ r & (i ^ t)) + f[11] + 643717713) << 14 | e >>> 18) + i << 0) ^ i)) + f[0] - 373897302) << 20 | r >>> 12) + e << 0, r = ((r += ((i = ((i += (r ^ e & ((t = ((t += (e ^ i & (r ^ e)) + f[5] - 701558691) << 5 | t >>> 27) + r << 0) ^ r)) + f[10] + 38016083) << 9 | i >>> 23) + t << 0) ^ t & ((e = ((e += (t ^ r & (i ^ t)) + f[15] - 660478335) << 14 | e >>> 18) + i << 0) ^ i)) + f[4] - 405537848) << 20 | r >>> 12) + e << 0, r = ((r += ((i = ((i += (r ^ e & ((t = ((t += (e ^ i & (r ^ e)) + f[9] + 568446438) << 5 | t >>> 27) + r << 0) ^ r)) + f[14] - 1019803690) << 9 | i >>> 23) + t << 0) ^ t & ((e = ((e += (t ^ r & (i ^ t)) + f[3] - 187363961) << 14 | e >>> 18) + i << 0) ^ i)) + f[8] + 1163531501) << 20 | r >>> 12) + e << 0, r = ((r += ((i = ((i += (r ^ e & ((t = ((t += (e ^ i & (r ^ e)) + f[13] - 1444681467) << 5 | t >>> 27) + r << 0) ^ r)) + f[2] - 51403784) << 9 | i >>> 23) + t << 0) ^ t & ((e = ((e += (t ^ r & (i ^ t)) + f[7] + 1735328473) << 14 | e >>> 18) + i << 0) ^ i)) + f[12] - 1926607734) << 20 | r >>> 12) + e << 0, r = ((r += ((h = (i = ((i += ((s = r ^ e) ^ (t = ((t += (s ^ i) + f[5] - 378558) << 4 | t >>> 28) + r << 0)) + f[8] - 2022574463) << 11 | i >>> 21) + t << 0) ^ t) ^ (e = ((e += (h ^ r) + f[11] + 1839030562) << 16 | e >>> 16) + i << 0)) + f[14] - 35309556) << 23 | r >>> 9) + e << 0, r = ((r += ((h = (i = ((i += ((s = r ^ e) ^ (t = ((t += (s ^ i) + f[1] - 1530992060) << 4 | t >>> 28) + r << 0)) + f[4] + 1272893353) << 11 | i >>> 21) + t << 0) ^ t) ^ (e = ((e += (h ^ r) + f[7] - 155497632) << 16 | e >>> 16) + i << 0)) + f[10] - 1094730640) << 23 | r >>> 9) + e << 0, r = ((r += ((h = (i = ((i += ((s = r ^ e) ^ (t = ((t += (s ^ i) + f[13] + 681279174) << 4 | t >>> 28) + r << 0)) + f[0] - 358537222) << 11 | i >>> 21) + t << 0) ^ t) ^ (e = ((e += (h ^ r) + f[3] - 722521979) << 16 | e >>> 16) + i << 0)) + f[6] + 76029189) << 23 | r >>> 9) + e << 0, r = ((r += ((h = (i = ((i += ((s = r ^ e) ^ (t = ((t += (s ^ i) + f[9] - 640364487) << 4 | t >>> 28) + r << 0)) + f[12] - 421815835) << 11 | i >>> 21) + t << 0) ^ t) ^ (e = ((e += (h ^ r) + f[15] + 530742520) << 16 | e >>> 16) + i << 0)) + f[2] - 995338651) << 23 | r >>> 9) + e << 0, r = ((r += ((i = ((i += (r ^ ((t = ((t += (e ^ (r | ~i)) + f[0] - 198630844) << 6 | t >>> 26) + r << 0) | ~e)) + f[7] + 1126891415) << 10 | i >>> 22) + t << 0) ^ ((e = ((e += (t ^ (i | ~r)) + f[14] - 1416354905) << 15 | e >>> 17) + i << 0) | ~t)) + f[5] - 57434055) << 21 | r >>> 11) + e << 0, r = ((r += ((i = ((i += (r ^ ((t = ((t += (e ^ (r | ~i)) + f[12] + 1700485571) << 6 | t >>> 26) + r << 0) | ~e)) + f[3] - 1894986606) << 10 | i >>> 22) + t << 0) ^ ((e = ((e += (t ^ (i | ~r)) + f[10] - 1051523) << 15 | e >>> 17) + i << 0) | ~t)) + f[1] - 2054922799) << 21 | r >>> 11) + e << 0, r = ((r += ((i = ((i += (r ^ ((t = ((t += (e ^ (r | ~i)) + f[8] + 1873313359) << 6 | t >>> 26) + r << 0) | ~e)) + f[15] - 30611744) << 10 | i >>> 22) + t << 0) ^ ((e = ((e += (t ^ (i | ~r)) + f[6] - 1560198380) << 15 | e >>> 17) + i << 0) | ~t)) + f[13] + 1309151649) << 21 | r >>> 11) + e << 0, r = ((r += ((i = ((i += (r ^ ((t = ((t += (e ^ (r | ~i)) + f[4] - 145523070) << 6 | t >>> 26) + r << 0) | ~e)) + f[11] - 1120210379) << 10 | i >>> 22) + t << 0) ^ ((e = ((e += (t ^ (i | ~r)) + f[2] + 718787259) << 15 | e >>> 17) + i << 0) | ~t)) + f[9] - 343485551) << 21 | r >>> 11) + e << 0, this.first ? (this.h0 = t + 1732584193 << 0, this.h1 = r - 271733879 << 0, this.h2 = e - 1732584194 << 0, this.h3 = i + 271733878 << 0, this.first = !1) : (this.h0 = this.h0 + t << 0, this.h1 = this.h1 + r << 0, this.h2 = this.h2 + e << 0, this.h3 = this.h3 + i << 0) }, t.prototype.hex = function () { this.finalize(); var t = this.h0, r = this.h1, e = this.h2, i = this.h3; return n[t >> 4 & 15] + n[15 & t] + n[t >> 12 & 15] + n[t >> 8 & 15] + n[t >> 20 & 15] + n[t >> 16 & 15] + n[t >> 28 & 15] + n[t >> 24 & 15] + n[r >> 4 & 15] + n[15 & r] + n[r >> 12 & 15] + n[r >> 8 & 15] + n[r >> 20 & 15] + n[r >> 16 & 15] + n[r >> 28 & 15] + n[r >> 24 & 15] + n[e >> 4 & 15] + n[15 & e] + n[e >> 12 & 15] + n[e >> 8 & 15] + n[e >> 20 & 15] + n[e >> 16 & 15] + n[e >> 28 & 15] + n[e >> 24 & 15] + n[i >> 4 & 15] + n[15 & i] + n[i >> 12 & 15] + n[i >> 8 & 15] + n[i >> 20 & 15] + n[i >> 16 & 15] + n[i >> 28 & 15] + n[i >> 24 & 15] }, t.prototype.toString = t.prototype.hex, t.prototype.digest = function () { this.finalize(); var t = this.h0, r = this.h1, e = this.h2, i = this.h3; return [255 & t, t >> 8 & 255, t >> 16 & 255, t >> 24 & 255, 255 & r, r >> 8 & 255, r >> 16 & 255, r >> 24 & 255, 255 & e, e >> 8 & 255, e >> 16 & 255, e >> 24 & 255, 255 & i, i >> 8 & 255, i >> 16 & 255, i >> 24 & 255] }, t.prototype.array = t.prototype.digest, t.prototype.arrayBuffer = function () { this.finalize(); var t = new ArrayBuffer(16), r = new Uint32Array(t); return r[0] = this.h0, r[1] = this.h1, r[2] = this.h2, r[3] = this.h3, t }, t.prototype.buffer = t.prototype.arrayBuffer, t.prototype.base64 = function () { for (var t, r, e, i = "", s = this.array(), h = 0; h < 15;)t = s[h++], r = s[h++], e = s[h++], i += p[t >>> 2] + p[63 & (t << 4 | r >>> 4)] + p[63 & (r << 2 | e >>> 6)] + p[63 & e]; return t = s[h], i += p[t >>> 2] + p[t << 4 & 63] + "==" }; var _ = v(); f ? module.exports = _ : (i.md5 = _, o && define(function () { return _ })) }();
function gRandomStr(length) {
    var text = "";
    var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < length; i++)
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    return text;
}
function gUrlRandomStr() {
    return gRandomStr(10);
}
// 表单值转换为集合
function form2map(formId, in_map) {
    var paramArray = $('#' + formId).serializeArray();
    var map = (typeof (in_map) === "undefined") ? {} : in_map;
    $(paramArray).each(function () {
        map[this.name] = this.value;
    });
    return map;
    //return JSON.stringify(map);
}
function getInputByName(name) {
    document.getElementsByTagName("input");
    for (i = 0; i < list.length; i++) {
        if (list[i].name == name)
            return list[i];
    }
    return null;
}
function getSelectByName(formId, name) {
    var form = document.getElementById(formId);
    var list;
    if (!form)
        list = document.getElementsByTagName("select");
    else
        list = form.getElementsByTagName("select");
    for (i = 0; i < list.length; i++) {
        if (list[i].name == name)
            return list[i];
    }
    return null;
}
/**
 * select-options初始化
 * @param {any} addAll
 * @param {any} formId
 * @param {any} name
 * @param {any} list:[{"Value":"value值1","Text":"text值1"},{"Value":"value值2","Text":"text值2"}, ...]
 * @param {any} selectedKey
 */
function selectInitData(addAll, formId, name, list, selectedKey) {
    var sel = getSelectByName(formId, name);
    if (addAll)
        sel.options.add(new Option("全部", ""));
    for (var i = 0; i < list.length; i++) {
        map = list[i];
        sel.options.add(new Option(map["Text"], map["Value"], false, map["Value"] == selectedKey));
    }
}
function ajaxPost(settings) {
    if (typeof (settings.async) === "undefined")
        settings.async = true;

    settings.type = 'post';
    settings.contentType = "application/json;charset=utf-8";
    settings.dataType = 'json';
    settings.error = function (data) {
        if (typeof (layer) != "undefined") {
            //layer.msg('操作异常，请与管理员联系！！', { icon: 5 });
            layer.msg(data, { icon: 5 });
        }
        else
            alert('操作异常，请与管理员联系！！');
    }
    $.ajax(settings);
}
function safePost(settings) {
    if (!TOKEN) console.log("Missing TOKEN!");
    settings.beforeSend = function (XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader("Auth", TOKEN);
    }
    ajaxPost(settings);
}
function ajaxGet(settings) {
    if (typeof (settings.async) === "undefined")
        settings.async = true;

    settings.type = 'get';
    settings.async = true;
    settings.dataType = 'json';
    settings.contentType = "application/json;charset=utf-8";

    if (settings.error !== undefined && typeof (settings.error) == "undefined") {
        settings.error = function (e) {
            if (typeof (layer) != "undefined" && layer != null) {
                //layer.msg('操作异常，请与管理员联系！！', { icon: 5 });\
                layer.msg('操作异常，请与管理员联系！！', { icon: 5 });
            }
            else {
                alert('操作异常，请与管理员联系！！');
            }
        }
    }
    $.ajax(settings);
}
var _UnauthorizedHint = false;
function safeGet(settings) {
    if (!TOKEN) console.log("Missing TOKEN!");
    settings.beforeSend = function (XMLHttpRequest) {
        XMLHttpRequest.setRequestHeader("Auth", TOKEN);
    };
    settings.error = function (e) {
        //如果带了TOKEN还返回401，则提示登录。
        if ((e.statusText == "Unauthorized" || e.status == '401') && !_UnauthorizedHint) {
            _UnauthorizedHint = true;
            layer.confirm('登录超时，是否重新登录？', {
                btn: ['重新登录', '取消'] //按钮
            }, function () {
                //layer.msg('的确很重要', { icon: 1 });
                window.top.location.href = gPath + "Login";
            }, function () {
                _UnauthorizedHint = false;
            });
        } else {
            if (typeof (layer) != "undefined" && layer != null)
                //layer.msg('操作异常，请与管理员联系！！', { icon: 5 });
                layer.msg(e, { icon: 5 });
            else
                alert('操作异常，请与管理员联系！！');
        }
    }
    ajaxGet(settings);
}
function fillSelect(selector, rows) {
    var sel = $(selector);
    sel.empty();
    for (var i = 0; i < rows.length; i++) {
        var r = rows[i];
        sel.append("<option value='" + r.val + "' title='" + r.txt + "'>" + r.txt + "</option>");
    }
}
function loadSelect(selector, url, param, callback) {
    var sel = $(selector);
    ajaxGet({
        url: url,
        data: param,
        success: function (data) {
            var rows = data;
            sel.empty();
            for (var i = 0; i < rows.length; i++) {
                var r = rows[i];
                sel.append("<option value='" + r.val + "' title='" + r.txt + "'>" + r.txt + "</option>");
            }
            if (typeof (callback) != "undefined")
                callback();
        }
    });
}
//ztree操作，刷新选择的节点
function refreshSelectedNode(treeId, addChild) {
    var zTree = $.fn.zTree.getZTreeObj(treeId),
        nodes = zTree.getSelectedNodes();
    nodes[0].isParent = addChild;   //是否新增子节点
    zTree.reAsyncChildNodes(nodes[0], "refresh", false);
    //if (addChild) {
    //    //如果添加子节点，则选中新的子节点
    //    var childNode = zTree.getNodeByParam("id", childNodeId);
    //    console.log(childNode);
    //    zTree.selectNode(childNode, false, true);
    //}
    //else
    //    zTree.selectNode(nodes[0], true);
    return nodes[0].id;
}
//ztree操作，未整理
function openFirstTreenode() {
    // 获取树对象
    var treeObj = $.fn.zTree.getZTreeObj("treeDemo");
    /* 获取所有树节点 */
    var nodes = treeObj.transformToArray(treeObj.getNodes());
    //展开第一级树
    treeObj.expandNode(nodes[0], true);
}
//ztree操作，未整理
function selectNode(treeId, nodeId) {
    var treeObj = $.fn.zTree.getZTreeObj(treeId);
    var a = treeObj.getNodeByParam("id", nodeId);

    treeObj.selectNode(a, true);
    return a;
}
/** 
 *ztree 刷新当前选择节点的父节点，未整理
 */
function refreshParentNode() {
    var zTree = $.fn.zTree.getZTreeObj("scriptTree"),
        type = "refresh",
        silent = false,
        nodes = zTree.getSelectedNodes();
    /*根据 zTree 的唯一标识 tId 快速获取节点 JSON 数据对象*/
    var parentNode = zTree.getNodeByTId(nodes[0].parentTId);
    /*选中指定节点*/
    zTree.selectNode(parentNode);
    zTree.reAsyncChildNodes(parentNode, type, silent);
}
/**
 *字符串补全0，num传入的数字，n需要的字符长度
 */
function StrPadLeft(num, n) {
    return (Array(n).join(0) + num).slice(-n);
}
function FormatedDateTimeStr() {
    var d = new Date();
    return d.getFullYear() + StrPadLeft(d.getMonth() + 1, 2) + StrPadLeft(d.getDate(), 2) + StrPadLeft(d.getHours(), 2) + StrPadLeft(d.getMinutes(), 2) + StrPadLeft(d.getSeconds(), 2) + StrPadLeft(d.getMilliseconds(), 3);
    //获取当前月份(0-11,0代表1月)
    //myDate.getDate(); //获取当前日(1-31)
    //myDate.getDay(); //获取当前星bai期X(0-6,0代表星期天)
    //myDate.getTime(); //获取当前时间(从1970.1.1开始的毫秒数)
    //myDate.getHours(); //获取当前小时数(0-23)
    //myDate.getMinutes(); //获取当前分钟数(0-59)
    //myDate.getSeconds(); //获取当前秒数(0-59)
    //myDate.getMilliseconds(); //获取当前毫秒数(0-999)
}
///弹出下载页面
function downLoad(src) {
    layer.load("正在下载中......");
    var a = document.createElement('a');
    a.id = 'expertFile'
    a.href = src;
    document.body.append(a);
    a.click();
    document.getElementById('expertFile').remove();
    layer.closeAll();
}
//下载文件
function downLoadFile(src) {
    $.ajax({
        type: "GET",
        contentType: false,
        processData: false,
        url: src,
        success: function (data) {
            //恢复按钮可用
            $("#export").attr("disabled", true);
            if (typeof window.chrome !== 'undefined') {
                //谷歌
                var a = document.createElement('a');
                a.id = 'expertFile'
                a.href = window.URL.createObjectURL(data);
                document.body.append(a);
                a.click();
                document.getElementById('expertFile').remove();
            } else if (typeof window.navigator.msSaveBlob !== 'undefined') {
                //ie
                var blob = new Blob([data], { type: 'application/force-download' });
                window.navigator.msSaveBlob(blob, filename);
            } else {
                //火狐
                var file = new File([data], filename, { type: 'application/force-download' });
                window.open(URL.createObjectURL(file));
            }
        },
        error: function () {
            $("#export").attr("disabled", true);
            alert("数据下载异常，请重试！");
        }
    });
}

function verifyDownLoad(src, url) {
    layer.load("正在处理中......");
    $.ajax({
        url: url,
        type: "get",
        dataType: "json",
        success: function (r) {
            if (r.code == 0) {
                layer.closeAll();
                layer.msg("正在下载请稍后", { icon: 1 });
                downLoad(src);
            }
            else if (r.code == -1) {
                layer.closeAll();
                layer.msg(r.message, { icon: 1 });
            }
            else {
                layer.closeAll();
                layer.msg(r.message, { icon: 5 });
            }
        },
        error: function () {
            layer.closeAll();
            layer.msg("出错了!", { icon: 5 });
        }
    });
}

function verifyDownLoad2(src, url, res) {
    let index = layer.load("正在处理中......");
    $.ajax({
        url: url,
        type: "get",
        dataType: "json",
        success: function (r) {
            if (r.code == 0) {
                layer.close(index);
                layer.msg("正在下载请稍后", { icon: 1 });
                downLoad(src);
                res(r)
            }
            else if (r.code == -1) {
                layer.closeAll();
                layer.msg(r.message, { icon: 1 });
                res(false)
            }
            else {
                layer.closeAll();
                layer.msg(r.message, { icon: 5 });
                res(false)
            }
        },
        error: function () {
            layer.closeAll();
            layer.msg("出错了!", { icon: 5 });
            res(false)
        }
    });
}

function downLoad2(src, data_params, filename) {
    layer.load(2);
    //要请求的Url和携带的参数
    var xhr = new XMLHttpRequest();
    xhr.open("GET", src + "&data_params=" + data_params, true);
    // xhr.onreadystatechange = processResponse;//设置处理响应的回调函数
    //xhr.responseType = 'blob';
    //POST方式需要自己设置http的请求头
    xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
    //xhr.setRequestHeader("Content-type", "application/json");    
    //设置响应类型为blob类型
    xhr.responseType = "blob";
    //POST方式发送数据
    xhr.send(data_params);//发送请求
    xhr.onload = function () {//HTTP响应已经完全接收
        console.log(this)
        if (this.status === 200) {
            console.log(this.response)
            //var content = this.response;
            //var blob = new Blob([content]);
            //var reader = new FileReader();
            //reader.readAsDataURL(blob);//转换为base64，可以直接放入a表情href
            //reader.onload = function (e) {
            //const url = window.URL || window.webkitURL || window.moxURL;
            //const href = url.createObjectURL(blob);
            //// 转换完成，创建一个a标签用于下载
            //var a = document.createElement('a');
            //filename = filename + new Date().toString("yyyyMMddHHmmssfff") + '.xlsx';
            //a.download = filename;
            //a.href = href;
            //a.style.display = 'none';
            ////a.setAttribute('download', fileName);
            //$("body").append(a); //修复firefox中无法触发click
            //a.click();//启动a标签点击事件下载
            //$(a).remove();//下载完成移除元素
            //window.URL.revokeObjectURL(href); //下载完成进行释放
            //}
            reader.onerror = function (e) {

            }
            layer.closeAll();
        }
        else {
            layer.msg("下载失败", { icon: 5 });
            layer.closeAll();
        }
    };
}

//扩展Date的format方法
Date.prototype.format = function (format) {
    var o = {
        "M+": this.getMonth() + 1,//获取当前月份(0-11,0代表1月)
        "d+": this.getDate(),//获取当前日(1-31)
        "h+": this.getHours(),//获取当前小时数(0-23)
        "m+": this.getMinutes(),//获取当前分钟数(0-59)
        "s+": this.getSeconds(),//获取当前秒数(0-59)
        "q+": Math.floor((this.getMonth() + 3) / 3),
        "S": this.getMilliseconds()
    }
    if (/(y+)/.test(format)) {
        format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(format)) {
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
        }
    }
    return format;
}

Date.prototype.toString = function (format) {
    var time = {};
    time.Year = this.getFullYear();
    time.TYear = ("" + time.Year).substr(2);
    time.Month = this.getMonth() + 1;
    time.TMonth = time.Month < 10 ? "0" + time.Month : time.Month;
    time.Day = this.getDate();
    time.TDay = time.Day < 10 ? "0" + time.Day : time.Day;
    time.Hour = this.getHours();
    time.THour = time.Hour < 10 ? "0" + time.Hour : time.Hour;
    time.hour = time.Hour < 13 ? time.Hour : time.Hour - 12;
    time.Thour = time.hour < 10 ? "0" + time.hour : time.hour;
    time.Minute = this.getMinutes();
    time.TMinute = time.Minute < 10 ? "0" + time.Minute : time.Minute;
    time.Second = this.getSeconds();
    time.TSecond = time.Second < 10 ? "0" + time.Second : time.Second;
    time.Millisecond = this.getMilliseconds();
    var oNumber = time.Millisecond / 1000;
    if (format != undefined && format.replace(/\s/g, "").length > 0) {
        format = format
            .replace(/yyyy/ig, time.Year)
            .replace(/yyy/ig, time.Year)
            .replace(/yy/ig, time.TYear)
            .replace(/y/ig, time.TYear)
            .replace(/MM/g, time.TMonth)
            .replace(/M/g, time.Month)
            .replace(/dd/ig, time.TDay)
            .replace(/d/ig, time.Day)
            .replace(/HH/g, time.THour)
            .replace(/H/g, time.Hour)
            .replace(/hh/g, time.Thour)
            .replace(/h/g, time.hour)
            .replace(/mm/g, time.TMinute)
            .replace(/m/g, time.Minute)
            .replace(/ss/ig, time.TSecond)
            .replace(/s/ig, time.Second)
            .replace(/fff/ig, time.Millisecond)
            .replace(/ff/ig, oNumber.toFixed(2) * 100)
            .replace(/f/ig, oNumber.toFixed(1) * 10);
    }
    else {
        format = time.Year + "-" + time.Month + "-" + time.Day + " " + time.Hour + ":" + time.Minute + ":" + time.Second;
    }
    return format;
}

function formatTime(val) {
    var re = /-?\d+/;
    var m = re.exec(val);
    var d = new Date(parseInt(m[0]));
    // 按【2012-02-13 09:09:09】的格式返回日期
    return d.format("yyyy-MM-dd hh:mm:ss");
}
function formatDate(val) {
    var re = /-?\d+/;
    var m = re.exec(val);
    var d = new Date(parseInt(m[0]));
    // 按【2012-02-13】的格式返回日期
    return d.format("yyyy-MM-dd");
}

 //日期转yyyy-MM-dd
function vertDate(obj) {  
    var date = new Date(obj)
    var y = 1900 + date.getYear()
    var m = '0' + (date.getMonth() + 1)
    var d = '0' + date.getDate()
    return y + '-' + m.substring(m.length - 2, m.length) + '-' + d.substring(d.length - 2, d.length)
}

//将标准时间转换成任意格式的函数
function formatDate(date, format) {
    if (/(y+)/.test(format)) {
        format = format.replace(RegExp.$1, (date.getFullYear() + '').substr(4 - RegExp.$1.length));
        var o = {
            "M+": date.getMonth() + 1,
            "d+": date.getDate(),
            "h+": date.getHours(),
            "m+": date.getMinutes(),
            "s+": date.getSeconds(),
            "q+": Math.floor((date.getMonth() + 3) / 3),
            "S": date.getMilliseconds()
        };
        for (var k in o) {
            if (new RegExp("(" + k + ")").test(format)) {
                var str = o[k] + '';
                format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? str : ("00" + str).substr(str.length));
            }
        }
        return format;
    }
}

//将对象转换为可安全存储在属性中的字符串
function Obj2Str(obj) {
    return escape(JSON.stringify(obj));
}
function Str2Obj(s) {
    return JSON.parse(unescape(s));
}
//JSON转URL参数
var Json2Param = function (param, key) {
    var paramStr = "";
    if (param instanceof String || param instanceof Number || param instanceof Boolean) {
        paramStr += "&" + key + "=" + encodeURIComponent(param);
    } else {
        $.each(param, function (i) {
            var k = key == null ? i : key + (param instanceof Array ? "[" + i + "]" : "." + i);
            paramStr += '&' + Json2Param(this, k);
        });
    }
    return paramStr.substr(1);
};
function maskName(s) {
    return s.substring(0, 1).padEnd(s.length, '*');
}
function EscapeBM(s) {
    return escape(s) + "_BM";
}
function getPrevMonth(splitter) {
    //  1    2    3    4    5    6    7    8    9   10    11   12月
    var date = new Date();
    var daysInMonth = [0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    var strYear = date.getFullYear();
    var strDay = date.getDate();
    var strMonth = date.getMonth() + 1;
    //1、解决闰年平年的二月份天数   //平年28天、闰年29天//能被4整除且不能被100整除的为闰年,或能被100整除且能被400整除
    if (((strYear % 4) === 0) && ((strYear % 100) !== 0) || ((strYear % 400) === 0)) {
        daysInMonth[2] = 29;
    }
    if (strMonth - 1 === 0) //2、解决跨年问题
    {
        strYear -= 1;
        strMonth = 12;
    }
    else {
        strMonth -= 1;
    }
    //  strDay = daysInMonth[strMonth] >= strDay ? strDay : daysInMonth[strMonth];
    strDay = Math.min(strDay, daysInMonth[strMonth]);//3、前一个月日期不必定和今天同一号，例如3.31的前一个月日期是2.28；9.30前一个月日期是8.30

    if (strMonth < 10)//给个位数的月、日补零
    {
        strMonth = "0" + strMonth;
    }
    if (strDay < 10) {
        strDay = "0" + strDay;
    }
    var datastr = strYear + splitter + strMonth;// + "-" + strDay;
    return datastr;
}
function getPrevDay(splitter) {
    //  1    2    3    4    5    6    7    8    9   10    11   12月
    //var arr = date.split('-');
    //var year = arr[0]; //获取当前日期的年份  
    //var month = arr[1]; //获取当前日期的月份  
    //var day = arr[2]; //获取当前日期的日  
    //var days = new Date(year, month, 0); 
    var date = new Date();
    var daysInMonth = [0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    var strYear = date.getFullYear();
    var strDay = date.getDate();
    var strMonth = date.getMonth() + 1;
    //1、解决闰年平年的二月份天数   //平年28天、闰年29天//能被4整除且不能被100整除的为闰年,或能被100整除且能被400整除
    if (((strYear % 4) === 0) && ((strYear % 100) !== 0) || ((strYear % 400) === 0)) {
        daysInMonth[2] = 29;
    }
    if (strMonth - 1 === 0) //2、解决跨年问题
    {
        strYear -= 1;
        strMonth = 12;
    }
    else {
        strMonth -= 1;
    }
    //  strDay = daysInMonth[strMonth] >= strDay ? strDay : daysInMonth[strMonth];
    strDay = Math.min(strDay, daysInMonth[strMonth]);//3、前一个月日期不必定和今天同一号，例如3.31的前一个月日期是2.28；9.30前一个月日期是8.30

    if (strMonth < 10)//给个位数的月、日补零
    {
        strMonth = "0" + strMonth;
    }
    if (strDay < 10) {
        strDay = "0" + strDay;
    }
    var datastr = strYear + splitter + strMonth + splitter + strDay;
    return datastr;
}

//金额小写转大写
function MoneyToUpper(value) {
    var returnText = "";
    //如果输入的不是数字，则将其设置为空
    value = value.replace(/[^\d\.]/g, '').replace(/^0/, '');
    var Num = value;
    if (Num == "") {
        //输入框删减为空时，将大写金额的内容值设为原始状态，当然也可以根据需求进行修改
        returnText = "零元整";
        return returnText;
    }
    part = String(Num).split(".");
    newchar = "";
    for (i = part[0].length - 1; i >= 0; i--) {
        if (part[0].length > 10) {
            returnText = "金额位数过大，无法计算";//前面如果有验证位数的，此处判断可去掉
            return returnText;
        }
        tmpnewchar = ""
        perchar = part[0].charAt(i);
        switch (perchar) {
            case "0": tmpnewchar = "零" + tmpnewchar; break;
            case "1": tmpnewchar = "壹" + tmpnewchar; break;
            case "2": tmpnewchar = "贰" + tmpnewchar; break;
            case "3": tmpnewchar = "叁" + tmpnewchar; break;
            case "4": tmpnewchar = "肆" + tmpnewchar; break;
            case "5": tmpnewchar = "伍" + tmpnewchar; break;
            case "6": tmpnewchar = "陆" + tmpnewchar; break;
            case "7": tmpnewchar = "柒" + tmpnewchar; break;
            case "8": tmpnewchar = "捌" + tmpnewchar; break;
            case "9": tmpnewchar = "玖" + tmpnewchar; break;
        }
        switch (part[0].length - i - 1) {
            case 0: tmpnewchar = tmpnewchar + "元"; break;
            case 1: if (perchar != 0) tmpnewchar = tmpnewchar + "拾"; break;
            case 2: if (perchar != 0) tmpnewchar = tmpnewchar + "佰"; break;
            case 3: if (perchar != 0) tmpnewchar = tmpnewchar + "仟"; break;
            case 4: tmpnewchar = tmpnewchar + "万"; break;
            case 5: if (perchar != 0) tmpnewchar = tmpnewchar + "拾"; break;
            case 6: if (perchar != 0) tmpnewchar = tmpnewchar + "佰"; break;
            case 7: if (perchar != 0) tmpnewchar = tmpnewchar + "仟"; break;
            case 8: tmpnewchar = tmpnewchar + "亿"; break;
            case 9: tmpnewchar = tmpnewchar + "拾"; break;
        }
        newchar = tmpnewchar + newchar;
    }
    if (Num.indexOf(".") != -1) {
        if (part[1].length > 2) {
            part[1] = part[1].substr(0, 2)
        }
        for (i = 0; i < part[1].length; i++) {
            tmpnewchar = ""
            perchar = part[1].charAt(i)
            switch (perchar) {
                case "0": tmpnewchar = "零" + tmpnewchar; break;
                case "1": tmpnewchar = "壹" + tmpnewchar; break;
                case "2": tmpnewchar = "贰" + tmpnewchar; break;
                case "3": tmpnewchar = "叁" + tmpnewchar; break;
                case "4": tmpnewchar = "肆" + tmpnewchar; break;
                case "5": tmpnewchar = "伍" + tmpnewchar; break;
                case "6": tmpnewchar = "陆" + tmpnewchar; break;
                case "7": tmpnewchar = "柒" + tmpnewchar; break;
                case "8": tmpnewchar = "捌" + tmpnewchar; break;
                case "9": tmpnewchar = "玖" + tmpnewchar; break;
            }
            if (i == 0) tmpnewchar = tmpnewchar + "角";
            if (i == 1) tmpnewchar = tmpnewchar + "分";
            newchar = newchar + tmpnewchar;
        }
    }
    while (newchar.search("零元") != -1) {
        newchar = newchar.replace("零零", "零");
        newchar = newchar.replace("零亿", "亿");
        newchar = newchar.replace("亿万", "亿");
        newchar = newchar.replace("零万", "万");
        newchar = newchar.replace("零元", "元");
        newchar = newchar.replace("零角", "");
        newchar = newchar.replace("零分", "");
    }
    if (newchar.charAt(newchar.length - 1) == "元" || newchar.charAt(newchar.length - 1) == "角") {
        newchar = newchar + "整";
    }
    returnText = newchar;
    return returnText;
}

//金额输入验证
function moneyInput(value) {
    //修复第一个字符是小数点的情况.
    var fa = '';
    if (value !== '' && value.substr(0, 1) === '.') {
        value = "";
    }
    value = value.replace(/^0*(0\.|[1-9])/, '$1');//解决粘贴不生效
    value = value.replace(/[^\d.]/g, "");  //清除“数字”和“.”以外的字符
    value = value.replace(/\.{2,}/g, "."); //只保留第一个.清除多余的
    value = value.replace(".", "$#$").replace(/\./g, "").replace("$#$", ".");
    value = value.replace(/^(\-)*(\d+)\.(\d\d).*$/, '$1$2.$3'); //只能输入两个小数
    if (value.indexOf(".") < 0 && value !== "") { //以上已经过滤，此处控制的是如果没有小数点，首位不能为类似于01、02的金额
        if (value.substr(0, 1) === '0' && value.length === 2) {
            value = value.substr(1, value.length);
        }
    }
    value = fa + value;
    return value;
}

var uploadFileData = {};
var randnum = "";
var site_load;
//进行分片处理，拿到上传目标文件，然后通过slice方法进行分片，在分片处理之前定义缓冲区大小（默认为8兆），
//然后循环遍历文件大小，然后将分片数据塞入分片数组，最后利用循环或者队列先进先出机制获取数组分片元素上传
//file.文件 root.存放目录，不传默认存放在wwwwroot-upload-BurstUploads下，格式如“/upload/report_file”，也支持“/UploadFile/RoutinePower/{id}”但是必须传id
//存储的文件名格式“yyyyMMddHHmmssfff@@文件名”，需要截取文件名按“@@”截取就可以了，也支持直接存储原文件名但是editName必须传1
function BurstUploadFile(file, root = "", id = "", editName = "") {
    uploadFileData = {};
    randnum = "";
    site_load = layer.load(2, {
        shade: [0.1, '#fff'],
        content: '<span class="loadtip">正在进行分片处理</span>',
        success: function (layer) {
            layer.find('.layui-layer-content').css({
                'padding-top': '40px',
                'width': '100px'
            });
            layer.find('.loadtip').css({
                'font-size': '20px',
                'margin-left': '-18px'
            });
        }
    });
    // 创建上传文件分片缓冲区
    var fileChunks = [];
    //// 目标文件
    //var file = targetFile[0];
    // 设置分片缓冲区大小
    var maxFileSizeMB = 8;
    var bufferChunkSize = maxFileSizeMB * (1024 * 1024);
    // 读取文件流起始位置
    var fileStreamPos = 0;
    // 设置下一次读取缓冲区初始大小
    var endPos = bufferChunkSize;
    // 文件大小
    var size = file.size;
    // 将文件进行循环分片处理塞入分片数组
    while (fileStreamPos < size) {
        var fileChunkInfo = {
            file: file.slice(fileStreamPos, endPos),
            start: fileStreamPos,
            end: endPos
        }
        fileChunks.push(fileChunkInfo);
        fileStreamPos = endPos;
        endPos = fileStreamPos + bufferChunkSize;
    }
    // 获取上传文件分片总数量
    var totalParts = fileChunks.length;
    var partCount = 0;
    randnum = Math.round(Math.random() * 10);
    // 循环调用上传每一片
    while (chunk = fileChunks.shift()) {
        partCount++;
        chunk.filePartName = file.name;
        // 上传文件命名约定
        var name = file.name + ".partNumber-" + partCount;
        chunk.name = name;
        // url参数
        var url = 'partNumber=' + partCount + '&chunks=' + totalParts + '&size=' + bufferChunkSize + '&start=' + chunk.start + '&end=' + chunk.end + '&total=' + size;
        chunk.urlParameter = url;
        chunk.maxChunk = totalParts;
        chunk.index = partCount;
        // 上传文件
        UploadFileChunk(chunk, root, id, editName);
    }
    layer.close(site_load);
}

//将每一片文件命名先进行一个约定（文件名+“.partNumber” + 分片号），
//以便所有分片上传完成后获取按照文件名中的分片号对其进行排序合并，这也就是合并文件的依据
//接下来就是上传每一片文件
function UploadFileChunk(chunk, root, id, editName) {
    let formData = new FormData();
    formData.append('file', chunk.file);
    formData.append('name', chunk.name);
    formData.append('fileName', chunk.filePartName);
    formData.append('chunk', chunk.index);
    formData.append('maxChunk', chunk.maxChunk);
    formData.append('randnum', randnum);
    formData.append('root', root);
    formData.append('data_id', id);
    formData.append("editName", editName);

    var state = (chunk.index / chunk.maxChunk) * 100;
    layer.msg("正在处理第" + chunk.index + "片，总共" + chunk.maxChunk + "片，剩余" + (chunk.maxChunk - chunk.index) + "片", { icon: 1 });
    $.ajax({
        url: wrapToken(gPath + "FileUpload/Upload") + '&' + chunk.urlParameter,
        type: "post",
        cache: false,
        contentType: false,
        processData: false,
        data: formData,
        async: false,//同步
        //beforeSend: function () {//当一个Ajax请求开始时触发
        //    layer.msg("正在处理第" + chunk.index + "片，总共" +chunk.maxChunk+"片，剩余" + (chunk.maxChunk - chunk.index) + "片", { icon: 1 });    
        //},
        //complete: function () {
        //    layer.msg("已完成" + state + "%", { icon: 1 });
        //},
        success: function (r) {
            layer.msg("已完成" + state + "%", { icon: 1 });
            if (r.Completed && r.FileName != "" && r.Name != "" && r.FileDir != "" && r.FilePath != "" && r.RandNum != "") {
                uploadFileData.FileName = r.FileName;
                uploadFileData.Name = r.Name;
                uploadFileData.FileDir = r.FileDir;
                uploadFileData.FilePath = r.FilePath;
                uploadFileData.Completed = r.Completed;
                uploadFileData.RandNum = r.RandNum;
            }
        },
        error: function (r) {
            layer.msg("分片上传文件出错了!", { icon: 5 });
        }
    });
}