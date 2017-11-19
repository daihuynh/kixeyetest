const cluster = require('cluster');
const dgram = require('dgram');
const numCPUs = require('os').cpus().length;
const clusterCount = Math.min(2, numCPUs);
const host = "192.168.1.48";
const port = 43210;

const mongoClient = require('mongodb').MongoClient;
const assert = require('assert');
const Enum = require('enum');

const MessageTypes = require('./message_types');
const MessageHandlers = require('./message_handlers');

const SendTypes = require('./send_types');


// Cluster init
if (cluster.isMaster) {
    // Open db connection
    mongoClient.connect('mongodb://localhost:27017/mygame', (err, db) => {
        assert.equal(null, err);

        console.log("Connected to mongodb");

        initMasterCluster(db);
    });
} else if (cluster.isWorker) {
    initWorkerCluster();
}

function initMasterCluster(db) {
    console.log(`Master ${process.pid} is running with ${clusterCount} workers`);

    for (let counter = 0; counter < clusterCount; counter++) {
        cluster.fork();
    }

    // Register message handlers
    let msgHandlers = {};
    msgHandlers[MessageTypes.JoinRoomRequest.value.toString()] = MessageHandlers.handleJoinRoomRequest;
    msgHandlers[MessageTypes.PlayerState.value.toString()] = MessageHandlers.handlePlayerStateRequest;

    // Server state
    let clients = {};
    let players = [];

    let state = {
        clients : clients,
        players : players
    };


    for (const id in cluster.workers) {
        const worker = cluster.workers[id];
        worker.on('message', (msg) => {
            try {
                const { rinfo, clientMsg} = msg;
                const { msgType, jsonData } = clientMsg;
                let data = JSON.parse(jsonData);

                const handler = msgHandlers[msgType];
                if (handler) {
                    handler(data, state, rinfo, worker, db);
                } else {
                    console.log(`======= Handler not found ======== ${MessageTypes.get(msgType)}`);
                }
            } catch (err) {
                console.log(`======= RECEIVE MSG ERROR ======= ${err.stack}`);
            }
        });
    }
}

function initWorkerCluster(db) {
    const socket = dgram.createSocket('udp4');
    socket.on('listening', () => {
        console.log(`Worker ${process.pid}'s socket is listening on :${port}`);
        socket.setBroadcast(true);
    });
    socket.on('error', (err) => {
        console.log(`Worker ${process.pid}'s socket error ${err.stack}`);
    });
    socket.on('message', (msg, rinfo) => {
        console.log(`Worker ${process.pid}'s socket receive ${msg} from ${rinfo.address}:${rinfo.port}`);

        try {
            let clientMsg = JSON.parse(msg);
            process.send({
                rinfo : rinfo,
                clientMsg : clientMsg
            });
        } catch (err) {
            console.log(`======= PARSING MSG ERROR ======= ${err.stack}`);
        }

        // var buffer = Buffer.from("OK");
        // socket.send(buffer, rinfo.port, rinfo.address);
    });
    socket.bind(port);

    process.on('message', (msg) => {
        try {
            const { sendType, data, state, rinfo } = msg;

            let jsonMessage = JSON.stringify(data);
            let buffer = Buffer.from(jsonMessage);

            if (sendType == SendTypes.Single.value) {
                socket.send(buffer, rinfo.port, rinfo.address);
            } else {
                const { clients } = state;
                for (var userName in clients) {
                    var client = clients[userName];
                    socket.send(buffer, client.rinfo.port, client.rinfo.address);
                }
            }
            // console.log(`Worker ${process.pid}'s socket send ${jsonMessage}`);
        } catch (err) {
            console.log(`======= SEND MSG ERROR ======= ${err.stack}`);
        }
    });
}