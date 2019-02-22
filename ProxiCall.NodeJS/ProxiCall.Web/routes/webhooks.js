'use strict';
const express = require('express');
const router = express.Router();

const botConnector = require('../services/bot-connector');
const nexmoConnector = require('../services/nexmo-connector');

var socket;

/* Answer url for nexmo */
router.get('/answer', function (req, res) {
    const ncco = [{
        "action": "connect",
        "endpoint": [
            {
                "type": "websocket",
                "uri": "wss://e0a192e8.ngrok.io/webhooks/socket",
                "content-type": "audio/l16;rate=16000", 
                "headers": {
                    "language": "fr-FR",
                    "callerID": `${req.param('from')}`
                }
           }
        ]
    }];

    res.json(ncco);
});

/* Event url for nexmo */
router.post('/event', function (req, res) {
    //TODO do something with the received events
    res.status(200).send();
});

router.ws('/socket', function(ws, req) {
    ws.on('message', function(msg) {
        //TODO handle received message
    });
    
    //Start bot conversation
    var bc = new botConnector();

    function onBotReply(botReply) {
        //Do something with bot response
        //TODO convert text to speech
        //ws.send(botReply);
    }
    bc.listenToBot(onBotReply);
});

module.exports = router;
