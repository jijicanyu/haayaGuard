/// <reference path="lib/jquery-1.8.2.min.js" />
var rtc = null;
var videos = document.getElementById("videos");
var pageObject = {
    session: "",
    loginBtn: $("#loginBtn"),
    url: "http://u.xieqj.cn/UserVaildate.asmx/CrossLogin",
    init: function () {
        this.loginBtn.on("click", this.login);
    },
    rtcInit: function () {
        rtc.on("connected", function (socket) {
            //创建本地视频流
            rtc.createStream({
                "video": true,
                "audio": true
            });
        });
        //创建本地视频流成功
        rtc.on("stream_created", function (stream) {
            document.getElementById('me').src = URL.createObjectURL(stream);
            document.getElementById('me').play();
        });
        //创建本地视频流失败
        rtc.on("stream_create_error", function () {
            alert("create stream failed!");
        });
        //接收到其他用户的视频流
        rtc.on('pc_add_stream', function (stream, socketId) {
            var newVideo = document.createElement("video"),
                id = "other-" + socketId;
            newVideo.setAttribute("class", "other");
            newVideo.setAttribute("autoplay", "autoplay");
            newVideo.setAttribute("id", id);
            videos.appendChild(newVideo);
            rtc.attachStream(stream, id);
        });
        //删除其他用户
        rtc.on('remove_peer', function (socketId) {
            var video = document.getElementById('other-' + socketId);
            if (video) {
                video.parentNode.removeChild(video);
            }
        });
       
    },
    login: function () {
        var loginName = $("#loginName").val(), pwd = $("#pwd").val();
        var iceName = $("#iceName").val(), icepwd = $("#icePwd").val();
        $.ajax({
            url: pageObject.url,
            data: { loginName: loginName, pwd: pwd },
            type: 'post',
            cache: false,
            contentType: "application/json; charset=utf-8",
            dataType: 'jsonp',
            jsonp: 'jsoncallback',
            success: function (data) {
                var r = data;
                if (r.ActionResult) {
                    rtc = SkyRTC(iceName, icepwd);
                    pageObject.rtcInit();
                    pageObject.session = r.Data;
                   // alert(pageObject.session);
                    //连接WebSocket服务器
                    rtc.connect("ws:http://v.xieqj.cn:3000", "");
                }
               
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {               
                //alert(XMLHttpRequest.status);
                //alert(XMLHttpRequest.readyState);
                alert(textStatus);
            },
            complete: function (XMLHttpRequest, textStatus) {

            }
        });
    }
};
pageObject.init();