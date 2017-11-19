const MessageTypes = require('./message_types');
const SendTypes = require('./send_types');
const assert = require('assert');

function createSingleReply(data, state, rinfo) {
    return {
        sendType : SendTypes.Single.value,
        state : state,
        data : data,
        rinfo : rinfo
    };
};

function createBroadcastReply(data, state, rinfo) {
    return {
        sendType : SendTypes.Broadcast.value,
        state: state,
        data : data,
        rinfo : rinfo
    };
};

const handlers = {
    handleJoinRoomRequest : function(data, state, rinfo, worker, db) {
        const { userName } = data;
        const {clients, players } = state;

        if (clients[userName]) return;

        clients[userName] = {
            workerPid : worker.pid,
            rinfo : rinfo
        };

        if (players.length < 4) {
            players.push(userName);

            // Start to send heartbeat to client
            let heartBeat = {
                msgType : MessageTypes.Heartbeat.value,
                jsonData : JSON.stringify({
                    ackId : 0,
                    timestamp : Date.now()
                })
            };
            worker.send(createSingleReply(heartBeat, state, rinfo));

            // Broadcast join success
            let joinRoomResponse = {
                msgType : MessageTypes.JoinRoomResponse.value,
                jsonData :JSON.stringify({
                    isSuccess: true
                })
            };
            worker.send(createBroadcastReply(joinRoomResponse, state, rinfo));

            // Create player 
            let createPlayer = {
                msgType : MessageTypes.CreatePlayer.value,
                jsonData :JSON.stringify({
                    userName : userName,
                    playerIndex : players.indexOf(userName)
                })
            };
            worker.send(createBroadcastReply(createPlayer, state, rinfo));


            db.collection('roomStates').insertOne({
                user : userName,
                users : players,
                isPlayer : true,
                playerIndex : players.indexOf(userName),
                createTime : Date.now()
            });
        } else {
            // Reply to client
            let joinRoomResponse = {
                msgType : MessageTypes.JoinRoomResponse.value,
                jsonData :JSON.stringify({
                    isSuccess: false,
                    playerCount : players.length,
                    playerIndex : -1
                })
            };
            worker.send(createSingleReply(joinRoomResponse, state, rinfo));

            db.collection('roomStates').insertOne({
                user : userName,
                users : players,
                isPlayer : false,
                playerIndex : -1,
                createTime : Date.now()
            });
        }

        // Restore player spawn state
        const playerStatesCollection = db.collection('playerStates');
        for (let counter = 0; counter < players.length; counter++) {
            var opponent = clients[players[counter]];

            if (opponent && userName != players[counter]) {
                // Spawn
                let createOpponent = {
                    msgType : MessageTypes.CreatePlayer.value,
                    jsonData :JSON.stringify({
                        userName : players[counter],
                        playerIndex : counter
                    })
                };

                worker.send(createSingleReply(createOpponent, state, rinfo));

                // Player State
                playerStatesCollection.find({user : players[counter]})
                    .limit(1)
                    .sort({ createTime : -1 })
                    .toArray((err, playerStates) => {
                        if (err != null) {
                            console.log(err);
                            return;
                        }
                        console.log(playerStates);
                        if (playerStates.length > 0) {
                            let playerState = {
                                msgType : MessageTypes.PlayerState.value,
                                jsonData : JSON.stringify(playerStates[0].state)
                            };
                            worker.send(createSingleReply(playerState, state, rinfo));
                        }
                    });
            }
        }
    },

    handlePlayerStateRequest : function(data, state, rinfo, worker, db) {
        const { userName } = data;
        const {clients, players } = state;
        if (clients[userName].workerPid != worker.pid) return;

        let playerState = {
            msgType : MessageTypes.PlayerState.value,
            jsonData : JSON.stringify(data)
        };
        worker.send(createBroadcastReply(playerState, state, rinfo));


        db.collection('playerStates').insertOne({
            user : userName,
            state : data,
            createTime : Date.now()
        });
    },

    handleHeartbeat : function(data, state, rinfo, worker, db) {
    }
};

module.exports = handlers;