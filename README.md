# Proxicall

This project is a system allowing to communicate with an application using its voice as a medium. This application will, among other things,search for a number, contact someone and record a meeting report.

## Contributors

[Xavier Vercruysse](https://github.com/xvercruysse)
[Mélissa Fontesse](https://github.com/MissRedfreak)

## ProxiCall CRM

### Database

Create a new sqlserver database on Azure.
![database view](https://github.com/micbelgique/ProxiCall/blob/master/pictures/database.png)

When it is created, retrieve the connection string from the Overview page, insert your username and password and save it for later.

### Sendgrid

If you don’t have one already, create an account on [Sendgrid](https://sendgrid.com) and follow the instructions to obtain an api key. Save it for later.

### Azure Active Directory

In your Azure Active Directory, register a new app. Put the url of your website in the redirect URL followed by '/signin-microsoft'
![azure Active Directory view](https://github.com/micbelgique/ProxiCall/blob/master/pictures/azureAD.png)

When the app is registered, go on the Certificates & Secrets tab and create a new Client secret. Copy the newly created secret and save it for later, you won’t be able to see it afterwards.

### Web app

Create a web app resource on Azure.
![web app view](https://github.com/micbelgique/ProxiCall/blob/master/pictures/webapp.png)

Fill in the following settings in your appsettings.json, as well as in the Configuration tab of the CRM web app on Azure.
The UserSettings section represents the default admin account of the CRM.
![web app user settings view](https://github.com/micbelgique/ProxiCall/blob/master/pictures/webappusersettings.png)

Next, execute the Update-Database command in the Package Manager Console.
Don't forget to put your web app in https only inside TLS/SSL Settings.
You can now publish the project on Azure.

## ProxiCall Bot

### Luis

Create a new Language Understanding (LUIS) resource. When it is created, save the api key for later and go on [luis.ai](https://luis.ai) and sign-in with your Azure account.
Create a new luis application for each culture you want to support (only the en-US and fr-FR cultures are supported as of right now).
When the application is created, go on the Manage tab, then Keys and Enpoints and assign the azure resource you created earlier.
On the Application Information tab, save the ApplicationId for later
To add the model in the application, go on the Versions tab and import the file proxicall-luis-LANGUAGE_CODE-model.json.
Finally, train and publish the app.

### Web app bot

Create a new Web App Bot resource on azure.
![web app bot view](https://github.com/micbelgique/ProxiCall/blob/master/pictures/webappbot.png)

Pick the Echo Bot template and select the Auto Create option in the Microsoft App ID and password field. Save the MicrosoftAppId and MicrosoftAppPassword added as Application Settings for later.
Go on the Web app bot, in the Channel tab and add the Directline channel. Save the Directline secret for later. You can also add the Teams channel for messaging.
Fill in the following settings in your appsettings.json, as well as in the Configuration tab of the CRM web app on Azure.
![web app bot user settings view](https://github.com/micbelgique/ProxiCall/blob/master/pictures/webappbotusersettings.png)

You can now publish the bot on Azure.

### Microsoft Teams

Go on the Teams App Store and install App Studio. When it is installed, go on the Manifest Editor tab and create a new app. Fill in the different fields, then go the bot tab and add an existing bot.
Add the Microsoft App Id that you saved earlier and select the personal scope.
Finally, scroll to the Test and distribute tab and install the app.
You can now chat with ProxiCall on Teams.
N.B.: You need to login at least once with your Microsoft Teams account on your CRM to be authenticated on the Teams bot.

## ProxiCall Directline

### Cognitive Speech

Create a new Speech resource on Azure. When it is created, retrieve your api key and save it for later, as well as the region.

### Twilio

If you don’t have one already, create a new account on [Twilio](https://twilio.com) and create a new project with a Phone Number product.
Finally, buy a phone number with calling capabilities and save it for later, along with your Twilio SID and Twilio Password (auth token).

### Web app

Create a new Web App on Azure.
![web app Proxicall view](https://github.com/micbelgique/ProxiCall/blob/master/pictures/webappproxicall.png)

Fill in the following settings in your appsettings.json, as well as in the Configuration tab of the web app.
![web app Proxicall user settings view](https://github.com/micbelgique/ProxiCall/blob/master/pictures/webappproxicallusersettings.png)

You can now publish the web app.
