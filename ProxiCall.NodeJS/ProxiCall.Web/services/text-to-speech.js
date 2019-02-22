'use strict';
// Requires request for HTTP requests
const request = require('request');
// Requires xmlbuilder to build the SSML body
const xmlbuilder = require('xmlbuilder');

// Requires dotenv to read .env
const dotenv = require('dotenv');
dotenv.config();

const subscriptionKey = process.env.SPEECH_KEY;
if (!subscriptionKey) {
    throw new Error('Environment variable for your subscription key is not set.')
};

const subscriptionRegion = process.env.SPEECH_Region;
if (!subscriptionRegion) {
    throw new Error('Environment variable for your subscription region is not set.')
};

var byteArray;

function textToSpeech(subscriptionKey, subscriptionRegion, saveAudio) {
    let options = {
        method: 'POST',
        uri: `https://westeurope.api.cognitive.microsoft.com/sts/v1.0/issueToken`,
        headers: {
            'Ocp-Apim-Subscription-Key': subscriptionKey
        }
    };
    // This function retrieve the access token and is passed as callback
    // to request below.
    function getToken(error, response, body) {
        console.log("Getting your token...\n")
        if (!error && response.statusCode == 200) {
            //This is the callback to our saveAudio function.
            // It takes a single argument, which is the returned accessToken.
            saveAudio(body)
        }
        else {
            throw new Error(error);
        }
    }
    request(options, getToken)
}

/* Make sure to update User-Agent with the name of your resource.
   You can also change the voice and output formats. See:
   https://docs.microsoft.com/azure/cognitive-services/speech-service/language-support#text-to-speech */
function saveAudio(accessToken) {
    // Create the SSML request.
    let xml_body = xmlbuilder.create('speak')
        .att('version', '1.0')
        .att('xml:lang', 'en-us')
        .ele('voice')
        .att('xml:lang', 'en-us')
        .att('name', 'Microsoft Server Speech Text to Speech Voice (en-US, Guy24KRUS)')
        .txt("test")
        .end();
    // Convert the XML into a string to send in the TTS request.
    let body = xml_body.toString();

    /* This sample assumes your resource was created in the WEST US region. If you
       are using a different region, please update the uri. */
    let options = {
        method: 'POST',
        baseUrl: 'https://westeurope.tts.speech.microsoft.com/',
        url: 'cognitiveservices/v1',
        headers: {
            'Authorization': 'Bearer ' + accessToken,
            'cache-control': 'no-cache',
            'User-Agent': 'YOUR_RESOURCE_NAME',
            'X-Microsoft-OutputFormat': 'riff-24khz-16bit-mono-pcm',
            'Content-Type': 'application/ssml+xml'
        },
        body: body
    };
    /* This function makes the request to convert speech to text.
       The speech is returned as the response. */
    function convertText(error, response, body) {
        if (!error && response.statusCode == 200) {
            console.log("Converting text-to-speech. Please hold...\n")
        }
        else {
            throw new Error(error);
        }
        console.log("Your file is ready.\n");
        var buf = Buffer.from(body);
        byteArray = buf.readUInt8(0);
    }
    request(options, convertText);
}

function runTextToSpeech() {
    textToSpeech(process.env.SPEECH_KEY, 'westeurope', saveAudio);
    return byteArray;
}

module.exports.runTextToSpeech = runTextToSpeech;