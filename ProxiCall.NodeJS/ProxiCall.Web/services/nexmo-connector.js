'use strict';

const express = require('express');
const router = express.Router();
const ws = require('ws');

class NexmoConnector {
    constructor() {
        this.wss = new WebSocket.Server({router});
    }

    startWebsocketServer() {
        this.wss.on('connection', function connection(ws) {
            this.ws = ws;
            this.ws.on('message', function incoming(message) {
                console.log('received: %s', message);
            });
        });
    }

    sendToNexmo(message) {
        this.ws.send(message);
    }
}

module.exports = NexmoConnector;