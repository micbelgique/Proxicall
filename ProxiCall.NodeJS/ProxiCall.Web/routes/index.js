'use strict';
var express = require('express');
var router = express.Router();

var tts = require('../services/text-to-speech');

/* GET home page. */
router.get('/', function (req, res) {
    res.render('index', { title: 'Express' });
});
tts.runTextToSpeech();
module.exports = router;
