const Enum = require('enum');

module.exports  = new Enum({
    'Heartbeat' : 0,
    'JoinRoomRequest' : 1,
    'JoinRoomResponse' : 2,
    'CreatePlayer' : 3,
    'PlayerState' : 4,
    'LeaveRoom' : 5
});