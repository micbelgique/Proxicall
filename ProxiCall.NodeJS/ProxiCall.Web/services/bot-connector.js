'use strict';

// Requires dotenv to read .env
const dotenv = require('dotenv');
dotenv.config();

const { DirectLine } = require('botframework-directlinejs');

class BotConnector {
    constructor() {
        this.directLine = new DirectLine({
            secret : process.env.DIRECTLINE_SECRET
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