$(document).ready(function()
{
    // Make sure we all it all starts clean
    // Disconnect all connection and delete cookies
    disconnect();

	$("#connect").click(function()
	{
	    
	    if (getCookie("state") == "disconnected" || getCookie("state")==null)
	    {
	        connect(function ()
	        {
	            // After connection is established, start getting Messages
	            console.log("Start getting messages");
	            startAjaxPolling();
	        });
	        
	    }
	    else if(getCookie("state")=="connected")
	        disconnect();
	});

	$("#sendChat").click(function () {
	    send();
	});

	$("#chatBox").keydown(function (event) {
	    if (event.which == 13)
	    {
	        send();
	        event.preventDefault();
	    }
	});
});
var interval_id;

function disconnect()
{
    var url = "http://localhost:3000/disconnect/";

    // Disconnects client
    if (getCookie("clientID") == null)
    {
        delCookie("state");
    }
    else
    {
        AjaxRequest("POST", url, { id: "x" }, function (result) {
            delCookie("clientID");
            delCookie("state");
        });
    }

    switchButton(false);
    clearInterval(interval_id);
}

function send()
{
    var data = {};
    var url = "http://localhost:3000/send/";
    
    if (!isClientConnected()) {
        alert("You need to connect to the server before you can send a message.");
        return;
    }
    data.message = $("#chatBox").val();
    $("#chatBox").val("");

    if (data.message != "")
    {
        AjaxRequest("POST", url, data, function (result) {
            if (result == "false")
                return;
            else {
                // Append Message to mesages area
                console.log("Message Sent: " + data.message);
                addToMessageArea(data.message,result, "You");
            }

        });
    }
}

var dequeueMessage=function()
{
    var url = "http://localhost:3000/dequeue";

    if (!isClientConnected())
    {
        alert("You are disconnected from the server.");
        disconnect();
    }
    else
    {
        AjaxRequest("GET", url, { id: "x" }, function (message) {
            if (message.length > 0) {
                // Get Message Details from result
                var message_id = message[0].message_guid;
                var message_sender = message[0].message_sender;
                var message_timestamp = message[0].message_timestamp;
                var message_text = message[0].message_text;

                // Don't Add message to DOM if it is already added
                if ($("#" + message_id).length <= 0) {
                    addToMessageArea(message_text, message_id, message_sender);
                }
            }
        });
    }
}

function startAjaxPolling() {
    interval_id= setInterval(dequeueMessage, 2000);
}

function addToMessageArea(message, message_id, from)
{
    // Check if message already added to DOM
    var obj_message = $("#" + message_id);
    var css_class = "alert-info";

    if (from != "You") {
        from = "Client: " + from;
        css_class = "alert-success";
    }

    var htmlMessage = "<div class=\"alert "+css_class+"\" style=\"margin-bottom:5px;\" id=\""+message_id+"\"><b><small>" + from + "</small></b><small>:&nbsp;" + message + "</small></div>";
    $(htmlMessage).appendTo("#messages");
}

function connect(callback)
{

	//	Get cookie clientID
	var clientGuid=getCookie("clientID");
	console.log('GUID: '+clientGuid);

	if(clientGuid==null)
	{
		console.log("Generate UUID....");
		// Generate cookie id
		var uuid=guid();
		console.log(uuid);
		// Set cookie values
		setCookie("clientID", uuid, 1);
		setCookie("state", "connected", 1);
	}
	else
	{
		console.log("Client UUID: "+clientGuid);
	}

    // Connect to webserver
	var url = "http://localhost:3000/connect/";

	AjaxRequest("POST", url, { id: clientGuid }, function (result) {
	    console.log(result);
	    if (result == "True") {
	        switchButton(true);
	        callback(result);
	    } else {
	        switchButton(false);
	    }

	});
};

function setCookie(c_name,value,exdays)
{
	var exdate=new Date();
	exdate.setDate(exdate.getDate() + exdays);
	var c_value=escape(value) + ((exdays==null) ? "" : "; expires="+exdate.toUTCString());
	document.cookie=c_name + "=" + c_value;
};

function getCookie(c_name)
{
	var c_value = document.cookie;
	var c_start = c_value.indexOf(" " + c_name + "=");
	if (c_start == -1)
	  {
	  	c_start = c_value.indexOf(c_name + "=");
	  }
	if (c_start == -1)
	  {
	  	c_value = null;
	  }
	else
  	{
		  c_start = c_value.indexOf("=", c_start) + 1;
		  var c_end = c_value.indexOf(";", c_start);
		  if (c_end == -1)
	  {
		c_end = c_value.length;
		}
		c_value = unescape(c_value.substring(c_start,c_end));
	}
	return c_value;
};

function delCookie(name) {
    document.cookie = name + '=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}

function s4()
{
	return Math.floor((1 + Math.random()) * 0x10000)
             .toString(16)
             .substring(1);
};

function AjaxRequest(method,url,data, callback)
{
    console.log(method+": "+url);
    if (method == "GET")
    {
        $.ajax({
            url: url
            , type: method
            , data: data
            , dataType:'json'
            , success: function (res) {
                callback(res)
            }
            , error: function (xhr, ajaxOptions, thrownError) {
                console.log("Error Occured!\n");
                console.log(xhr.status);
                console.log(thrownError);
                alert("Oops! Something is wrong, please try to reconnect");
                // Cleanup
                switchButton(false);
                clearInterval(interval_id);
            }
        });
    }
    else
    {
        $.ajax({
            url: url
            , type: method
            , data: data
            , success: function (data) {
                callback(data)
            }
            , error: function (xhr, ajaxOptions, thrownError) {
                console.log("Error Occured!\n");
                console.log(xhr.status);
                console.log(thrownError);
                alert("Oops! Something is wrong, please try to reconnect");
                // Cleanup
                switchButton(false);
                clearInterval(interval_id);
            }
        });
    }
}

function guid()
{
   // For longer guid names
  //return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
    //       s4() + '-' + s4() + s4() + s4();

    // In this App we only need short names
    return s4();
};

function switchButton(connected)
{
    if (connected)
    {
        $("#connect").removeClass("btn-success");
        $("#connect").addClass("btn-warning");
        $("#connect").text("Disconnect");
    }
    else
    {
        $("#connect").removeClass("btn-warning");
        $("#connect").addClass("btn-success");
        $("#connect").text("Connect");
    }
}

function showErrorMessage()
{
    $("#alert-contaner").addClass("alert-error");
    $("#alert-contaner").text("Something went wrong! You got disconnected from server");
    $("#alert-contaner").show();
}

function isClientConnected()
{
    var state = getCookie("state");

    if (state == "connected") {
        return true;
    } else { return false; }
}