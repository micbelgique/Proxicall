'use strict';
var express = require('express');
var router = express.Router();

/* Answer url for nexmo */
router.get('/answer', function (req, res) {
    const ncco = [{
        action: 'talk',
        text: "I am a node js web application in need of love"
    }];

    res.send(ncco);
});

/* Event url for nexmo */
router.post('/event', function (req, res) {
    //TODO do something with the received events
    res.status(200).send();
});

module.exports = router;
