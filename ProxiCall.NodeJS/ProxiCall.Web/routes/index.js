'use strict';
var express = require('express');
var router = express.Router();
var tts = require('../services/text-to-speech');


/* GET home page. */
router.get('/', function (req, res) {
    var audio = tts.runTextToSpeech();
    console.log(audio)
    res.render('index', { title: 'Express' });
});

module.exports = router;
