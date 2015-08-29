/// <reference path="lib/jquery-1.8.2.min.js" />
var userVaildateProvider = {
    session: "",
    url:"",
    login: function (loginName, pwd) {
        $.ajax({
            url: userVaildateProvider.url,
            data: JSON.stringify({ loginName: loginName, pwd: pwd }),
            type: 'post',
            cache: false,
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                var r = data.d;
                if (r.ActionResult) {
                    userVaildateProvider.session = r.Data;
                    //连接WebSocket服务器
                    rtc.connect("ws:" + window.location.href.substring(window.location.protocol.length).split('#')[0], window.location.hash.slice(1));
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
var videos = document.getElementById("videos");
var sendBtn = document.getElementById("sendBtn");
var msgs = document.getElementById("msgs");
var sendFileBtn = document.getElementById("sendFileBtn");
var files = document.getElementById("files");
var rtc = SkyRTC();
//成功创建WebSocket连接
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
//接收到文字信息
rtc.on('data_channel_message', function (channel, socketId, message) {
    var p = document.createElement("p");
    p.innerText = socketId + ": " + message;
    msgs.appendChild(p);
});
