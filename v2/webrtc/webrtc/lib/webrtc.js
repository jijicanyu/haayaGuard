var WebSocketServer = require('ws').Server;
var soap = require('soap');
var UUID = require('node-uuid');
var events = require('events');
var util = require('util');
var errorCb = function (rtc) {
    return function (error) {
        if (error) {
            rtc.emit("error", error);
        }
    };
};
function SkyRTC() {
    this.sockets = [];
    this.rooms = {};
    this.on('__join', function (data, socket) {
        var jointhat = this;
        console.log(data.session + 'will vaildate');
        soap.createClient("http://u.xieqj.cn/UserVaildate.asmx?WSDL", function (err, client) {
            client.SessionVaildate({ session: session }, function (err, result) {
                console.log("userVaildatelog:" + result);
                if (result.SessionVaildate) {
                    joinAction(data, socket);
                } else {
                    console.log(data.session + 'vaildate false');
                }
            });
        });
        function joinAction(data, socket) {
            console.log(jointhat.sockets.length);
            console.log('__join');
            var ids = [],
                i, m,
                room = data.room || "__default",
                curSocket,
                curRoom;    
            curRoom = jointhat.rooms[room] = jointhat.rooms[room] || [];

            for (i = 0, m = curRoom.length; i < m; i++) {
                curSocket = curRoom[i];
                if (curSocket.id === socket.id) {
                    continue;
                }
                ids.push(curSocket.id);
                curSocket.send(JSON.stringify({
                    "eventName": "_new_peer",
                    "data": {
                        "socketId": socket.id
                    }
                }), errorCb);
            }

            curRoom.push(socket);
            socket.room = room;

            socket.send(JSON.stringify({
                "eventName": "_peers",
                "data": {
                    "connections": ids,
                    "you": socket.id
                }
            }), errorCb);

            jointhat.emit('new_peer', socket, room);
        };
       
    });

    this.on('__ice_candidate', function (data, socket) {
        var soc = this.getSocket(data.socketId);
        console.log('__ice_candidate');
        if (soc) {
            soc.send(JSON.stringify({
                "eventName": "_ice_candidate",
                "data": {
                    "label": data.label,
                    "candidate": data.candidate,
                    "socketId": socket.id
                }
            }), errorCb);

            this.emit('ice_candidate', socket, data);
        }
    });

    this.on('__offer', function (data, socket) {
        var soc = this.getSocket(data.socketId);
        console.log('__offer');
        if (soc) {
            soc.send(JSON.stringify({
                "eventName": "_offer",
                "data": {
                    "sdp": data.sdp,
                    "socketId": socket.id
                }
            }), errorCb);
        }
        this.emit('offer', socket, data);
    });

    this.on('__answer', function (data, socket) {
        console.log('__answer');
        var soc = this.getSocket(data.socketId);
        if (soc) {
            soc.send(JSON.stringify({
                "eventName": "_answer",
                "data": {
                    "sdp": data.sdp,
                    "socketId": socket.id
                }
            }), errorCb);
            this.emit('answer', socket, data);
        }
    });
}

util.inherits(SkyRTC, events.EventEmitter);

SkyRTC.prototype.addSocket = function (socket) {
    console.log('addSocket');
    this.sockets.push(socket);
};

SkyRTC.prototype.removeSocket = function (socket) {
    console.log('removeSocket');
    var i = this.sockets.indexOf(socket),
		room = socket.room;
    this.sockets.splice(i, 1);
    if (room) {
        i = this.rooms[room].indexOf(socket);
        this.rooms[room].splice(i, 1);
        if (this.rooms[room].length === 0) {
            delete this.rooms[room];
        }
    }
};

SkyRTC.prototype.broadcast = function (data, errorCb) {
    console.log('broadcast');
    var i;
    for (i = this.sockets.length; i--;) {
        this.sockets[i].send(data, errorCb);
    }
};

SkyRTC.prototype.broadcastInRoom = function (room, data, errorCb) {
    console.log('broadcastInRoom');
    var curRoom = this.rooms[room],
		i;
    if (curRoom) {
        for (i = curRoom.length; i--;) {
            curRoom[i].send(data, errorCb);
        }
    }
};

SkyRTC.prototype.getRooms = function () {
    console.log('getRooms');
    var rooms = [],
		room;
    for (room in this.rooms) {
        rooms.push(room);
    }
    return rooms;
};

SkyRTC.prototype.getSocket = function (id) {
    console.log('getSocket');
    var i,
		curSocket;
    if (!this.sockets) {
        return;
    }
    for (i = this.sockets.length; i--;) {
        curSocket = this.sockets[i];
        if (id === curSocket.id) {
            return curSocket;
        }
    }
    return;
};

SkyRTC.prototype.init = function (socket) {
    var that = this;
    socket.id = UUID.v4();
    that.addSocket(socket);
    //为新连接绑定事件处理器
    socket.on('message', function (data) {
        var json = JSON.parse(data);
        if (json.eventName) {
            that.emit(json.eventName, json.data, socket);
        } else {
            that.emit("socket_message", socket, data);
        }
    });
    //连接关闭后从SkyRTC实例中移除连接，并通知其他连接
    socket.on('close', function () {
        var i, m,
			room = socket.room,
			curRoom;
        if (room) {
            curRoom = that.rooms[room];
            for (i = curRoom.length; i--;) {
                if (curRoom[i].id === socket.id) {
                    continue;
                }
                curRoom[i].send(JSON.stringify({
                    "eventName": "_remove_peer",
                    "data": {
                        "socketId": socket.id
                    }
                }), errorCb);
            }
        }

        that.removeSocket(socket);

        that.emit('remove_peer', socket.id, that);
    });
    that.emit('new_connect', socket);
};

module.exports.listen = function (server) {
    var SkyRTCServer;
    if (typeof server === 'number') {
        SkyRTCServer = new WebSocketServer({
            port: server
        });
    } else {
        SkyRTCServer = new WebSocketServer({
            server: server
        });
    }

    SkyRTCServer.rtc = new SkyRTC();
    errorCb = errorCb(SkyRTCServer.rtc);
    SkyRTCServer.on('connection', function (socket) {
        this.rtc.init(socket);
    });

    return SkyRTCServer;
};