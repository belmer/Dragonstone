using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dragonstone
{
    class Program
    {
        static void Main(string[] args)
        {
            string port = ConfigurationManager.AppSettings["listen_port"];
            string prefix = "http://*:" + port + "/";
           
            var ws = new Webserver(prefix);
            ws.RequestReceived += ws_RequestReceived;
            ws.Start();
            Console.WriteLine("Listening on " + prefix);
            Console.WriteLine("This will accept any request coming in to port: {0}",port);
            Console.WriteLine("But will only process to the following path: ");
            Console.WriteLine("\nhttp://localhost/connect/");
            Console.WriteLine("http://localhost/");
            Console.WriteLine("http://localhost/send/");
            Console.WriteLine("http://localhost/dequeue/");
            Console.WriteLine("http://localhost/disconnect/");
            Console.WriteLine("\nPress any key to stop.");
            Console.ReadLine();
            ws.Stop();
        }

        static void ws_RequestReceived(object sender, System.Net.HttpListenerContext context)
        {
            Console.WriteLine("Request Recieved: {0} {1}", context.Request.HttpMethod, context.Request.Url);

            // Append to Response Header all necessary permission for request
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.AppendHeader("Access-Control-Allow-Headers", "X-Requested-With,Content-Type,Accept");
            context.Response.AppendHeader("Access-Control-Allow-Methods", "GET,PUT,DELETE,OPTIONS,POST");

            // Initialize buffer, Incase a request comes in that is not available on collection print this message
            var buffer = System.Text.Encoding.Default.GetBytes("The Dog ate my page!!!");
            string requestedResource = context.Request.Url.AbsolutePath.Replace("/", "");

            if (Webserver.FileRequest.Contains(requestedResource))
            {
                // Process file request
                // Request is coming from the root host
                if (string.IsNullOrEmpty(requestedResource))
                    buffer = System.Text.Encoding.Default.GetBytes(Webserver.GetFile("Client.html"));
                else
                    buffer = System.Text.Encoding.Default.GetBytes(Webserver.GetFile(requestedResource));
            }
            else if (Webserver.AvailableServices.Contains(requestedResource))
            {
                // Process Service Request
                switch (requestedResource)
                {
                    case "connect": // Handle request for connecting
                        var conn_response=Webserver.Connect(context);
                        buffer = System.Text.Encoding.Default.GetBytes(conn_response.ToString());
                        break;
                    case "send": // Handle request to send message
                        var send_response = Webserver.AddMessageToClientQueue(context);
                        buffer = System.Text.Encoding.Default.GetBytes(send_response);
                        break;
                    case "dequeue": // Handle request to retrieve message
                        var dequeue_response = Webserver.DequeueClientMessages(context);
                        buffer = System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(dequeue_response));
                        break;
                    case "disconnect": // Handle request for disconnection
                        var disconnect_response = Webserver.Disconnect(context);
                        buffer = System.Text.Encoding.Default.GetBytes(disconnect_response.ToString());
                        break;
                    default:
                        break;
                }

            }

            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.OutputStream.Close();
        }
    }
}
