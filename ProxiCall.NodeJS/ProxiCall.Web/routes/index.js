'use strict';
var express = require('express');
var router = express.Router();

const botConnector = require('../services/bot-connector');

/* GET home page. */
router.get('/', function (req, res) {

    //Directline test
    var test = new botConnector();
    test.listenToBot(onBotReply);

    res.render('index', { title: 'Express' });
});

function onBotReply(msg) { 
    console.log(`This is a bot reply : ${msg}`);
}

module.exports = router;
