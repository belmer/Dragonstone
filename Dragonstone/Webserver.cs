using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Dragonstone
{
    public class Webserver
    {
        public delegate void RequestReceivedHanlder(object sender, HttpListenerContext context);
        public event RequestReceivedHanlder RequestReceived;
        public static List<string> FileRequest, AvailableServices, Clients;
        public static Dictionary<string,List<Message>> ListOfClientQueue;

        private readonly HttpListener _listener;
        private bool _running;
        private readonly Thread _connectionThread;

        public Webserver(string prefix)
        {
            ListOfClientQueue = new Dictionary<string, List<Message>>();
            // Add Files that is available on the server
            FileRequest = new List<string>() { 
                ""
                ,"client.js"
                ,"ace.min.css"
            };

            // Add Service Request that is Available
            AvailableServices = new List<string>() { 
                "connect"
                ,"send"
                ,"dequeue"
                ,"disconnect"
            };

            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _connectionThread = new Thread(ConnectionThread);
        }

        private void ConnectionThread()
        {
            try
            {
                _running = true;
                _listener.Start();
                while (_running)
                    ProcessRequest();
            }
            catch (HttpListenerException) { }
            catch (ThreadAbortException) { }
            catch (Exception){}
        }

        private void ProcessRequest()
        {
            IAsyncResult result = _listener.BeginGetContext(ListenerCallback, _listener);
            result.AsyncWaitHandle.WaitOne();
        }

        protected void ListenerCallback(IAsyncResult result)
        {
            if (_listener == null || !_listener.IsListening) return;
            var context = _listener.EndGetContext(result);
            OnRequestReceived(context);
        }

        protected void OnRequestReceived(HttpListenerContext context)
        {
            if (RequestReceived != null) 
                RequestReceived(this, context);
        }

        public void Start()
        {
            _connectionThread.Start();
        }

        public void Stop()
        {
            _running = false;
            _listener.Stop();
        }

        // Method to locate and read a file to serve
        public static string GetFile(string filename)
        {
            string html = "";

            try
            {
                var filePath = String.Concat(Directory.GetCurrentDirectory(), @"\Client\"+filename);

                if (File.Exists(filePath))
                {
                    html = File.ReadAllText(filePath);
                }
                else
                {
                    html = "<div class=\"alert alert-block\">Client File not Found!</div>";
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return html;
        }

        // Method to add client to collection
        public static bool Connect(HttpListenerContext context)
        {
            bool connected = false;
            try
            {
                string client = context.Request.Cookies["clientID"].Value;

                if (String.IsNullOrEmpty(client))
                    return false;

                string client_guid = client.Replace("-", "");

                if (!ListOfClientQueue.ContainsKey(client_guid))
                {
                    // Add client to collection
                    ListOfClientQueue.Add(client_guid, new List<Message>());
                    connected = true;
                    Console.WriteLine("New Client: {0} connected.", client_guid);
                }
                else // Client is already on the list
                    connected = true;

            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
            return connected;
        }

        // Method to disconnect client from server
        public static bool Disconnect(HttpListenerContext context)
        {
            bool disconnected = true;
            try
            {
                string client = context.Request.Cookies["clientID"].Value;

                if (String.IsNullOrEmpty(client))
                    return false;

                string client_guid = client.Replace("-", "");

                if (ListOfClientQueue.ContainsKey(client_guid))
                {
                    // Remove client and all of its queues from the collection
                    ListOfClientQueue.Remove(client_guid);
                    disconnected = true;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return disconnected;
        }

        // Method to Add Message to Client Queue
        public static string AddMessageToClientQueue(HttpListenerContext context)
        {
            string response = "false";
            string sender_message;
            try
            {
                string client = context.Request.Cookies["clientID"].Value;
                string client_guid = client.Replace("-", "");
                Guid messageGuid = Guid.NewGuid();
                string guid = messageGuid.ToString().Replace("-", "");

                // Get Posted Data Message
                using (var reader=new StreamReader(context.Request.InputStream,context.Request.ContentEncoding))
                {
                    var data_text = reader.ReadToEnd();
                    sender_message=HttpUtility.UrlDecode(data_text);
                }

                if (ListOfClientQueue.Count > 0)
                {
                    // Add Message to Clients Queue
                    foreach (var item in ListOfClientQueue)
                    {
                        Message message_data = new Message { 
                            message_guid=guid,
                            message_sender = client_guid,
                            message_timestamp=DateTime.Now.ToShortTimeString(),
                            message_text=sender_message.Split('=')[1]
                        };

                        item.Value.Add(message_data);
                    }

                    response = guid;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return "false";
            }
            return response;
        }

        // Method to Retreive client messages
        public static List<Message> DequeueClientMessages(HttpListenerContext context)
        {
            List<Message> client_messages = new List<Message>();
            try
            {
                string client = context.Request.Cookies["clientID"].Value;
                
                if (String.IsNullOrEmpty(client))
                    return client_messages;

                string client_guid = client.Replace("-", "");

                // Make sure the collection is not empty, else it will return an exception
                if (ListOfClientQueue.Count>0)
                {
                    // Get Messages from the Client Queue
                    client_messages = ListOfClientQueue[client_guid];

                    // Remove messages after Dequeueing
                    ListOfClientQueue[client_guid] = new List<Message>(); 
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return client_messages;
        }

        private static void LogException(Exception ex)
        {
            Console.WriteLine("Error: "+ex);
        }
    }
}
