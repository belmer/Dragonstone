Dragonstone
===========

Simple Web Server using .Net HttpClient
Simple Chat Client that will demonstrate POST and GET method using JQuery

Running or Testing
=====================================================
I included a compiled files inside "Chat app and Server". Open or Double Click "Dragonstone.exe" to start the web server.
The default port is 3000, so make sure that port is not used by other application.
If you have issue running the web server. Try running it with administrator privilige. (Right click Dragonstone.exe and choose "Run as Administrator"). Once the web server is running, navigate to "http://localhost:3000". You should be able to get the Chat App

=====================================================

Simple Web Server
	- Accepts http requests (e.g POST, GET)
	- Accepts multiple connection
	- Accepts to any request going to port 3000 (this is configurable on App.Config file)
	- However this will only process to the following url:
		- http://localhost
		- http://localhost/client.js
		- http://ace.min.css
			- Host the Simple Client Chat files
			- Client.html
			- client.js
			- ace.min.css
		- http://localhost/connect
			- To connect the webserver
		- http://localhost/send/
			- To broadcast or send message to all connected user
		- http://localhost/dequeue/
			- Get message from the client queue
		- http://localhost/disconnect/
			- Disconnect from the server

Web Server
	- Base URL (http://localhost)
		- FileRequest
		- Respond the file being requested. As for the host url it will locate the file Client.html, read the file and output
		- Client.html needed the following supporting files
			- client.js
			- ace.min.css
			- Whenver the server get this requests, it will locate the file, read and output it to the requesting client
	- Connect (http://localhost/connect)
		- Add client/user to the collection
		- To be able to demonstrate our chat client app, the web server has to keep all the clients in memory using .net Generic collection.
	- Send/AddMessageToClientQueue (http://localhost/send/)
		- This method will process the send message request from the client.
		- The message will then be stored to all the clients queue or collection.
		- Each message is identified by the message guid
	- Dequeue/DequeueClientMessages (http://localhost/dequeue/)
		- Retrieve messages from the requesting client
		- Once the message has been dequeue, this will then be deleted from the collection to avoid duplicates
	- Disconnect
		- Disconnect user from the server.
		- Once this method is called, the client will then be removed from the client collection.
Chat Client
	The client chat app will enable user to communicate to each other via the web server. There will be "Connect" button that will enable you to be connected to the server and start chatting. Once you click "Connect" button, this will create a guid and stored it into client cookie(clientID). This is used to identify each client connected to the server. The "client_guid" will then be sent to the server through (http://localhost/connect). Once succesfully connected, will store the client state to the cookie as "connected'. However, whenever you refresh your browser your cookie will be deleted and need to reconnect. (This is ofcourse can be configured to retain your session).

	Once connected to the server, the client will also start getting messages from the server via ajax request. This is done every 2 seconds. If there is a message received from the server or other client. This will then be added to the DOM on the message area. The "message_guid" will be used as the "element id" to avoid duplication. Another notable update when you are connected is the "Connect" button will be updated to "Disconnect". This will enable you to disconnect from server and stop getting messages.

	Send Button
		The send button at the lower right will send all the text that is in the text area.
