'use strict';

// Requires dotenv to read .env
const dotenv = require('dotenv');
dotenv.config();

const { DirectLine } = require('botframework-directlinejs');

class BotConnector {
    constructor() {
        this.directLine = new DirectLine({
            secret : process.env.DIRECTLINE_SECRET
            //secret = "Ng-CNh07_ys.PMF4jKQCP4mSjpWzqs2HXjliAl_flSPugcc3ZaiHts4"
        });
    }

    listenToBot(onBotReply) {
        this.directLine.activity$
            .filter(activity => activity.type === 'message' && activity.from.id === 'ProxiCallBot')
            .subscribe(
                message => onBotReply(message.text)
            );
    }
}

module.exports = BotConnector;