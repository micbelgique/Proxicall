const app = require('express')()


const onInboundCall = (request, response) => {
    //const from = request.query.from
    //const fromSplitIntoCharacters = from.split('').join(' ')
  
    const ncco = [{
      action: 'talk',
      text: "I am ProxiCall"
    }]
  
    response.json(ncco)
  }
  
  app.get('/webhooks/answer', onInboundCall);
  
  app.listen(44344, function () {
    console.log("Express server listening on port 44344");
    });