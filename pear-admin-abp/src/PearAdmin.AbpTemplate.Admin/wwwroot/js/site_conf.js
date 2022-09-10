var gPath = "/";  //站点虚拟目录

var TOKEN = "";
function UrlParamHash(url) {
    var params = [], h;
    var hash = url.slice(url.indexOf("?") + 1).split('&');
    //console.log(hash);
    for (var i = 0; i < hash.length; i++) {
        h = hash[i].split("="); //
        params[h[0]] = h[1];
        //console.log(h);
    }
    return params;
}
function wrapToken(url) {
    var request_url = window.location.href.split('?')[0];
    return (url + (url.indexOf("?") >= 0 ? "&" : "?") + "token=" + TOKEN + "&request_url=" + request_url);
}