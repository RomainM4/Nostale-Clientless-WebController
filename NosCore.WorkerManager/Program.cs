using System;
using System.Text;
using WatsonTcp;

WatsonTcpServer server = new WatsonTcpServer("127.0.0.1", 9000);
server.Events.ClientConnected += ClientConnected;
server.Events.ClientDisconnected += ClientDisconnected;
server.Events.MessageReceived += MessageReceived;
server.Callbacks.SyncRequestReceivedAsync = SyncRequestReceived;
server.Start();

// list clients
IEnumerable<ClientMetadata> clients = server.ListClients();

// send a message
server.Send([guid], "Hello, client!");

// send a message with metadata
Dictionary<string, object> md = new Dictionary<string, object>();
md.Add("foo", "bar");
server.Send([guid], "Hello, client!  Here's some metadata!", md);

// send async!
await server.SendAsync([guid], "Hello, client!  I'm async!");

// send and wait for a response
try
{
    SyncResponse resp = server.SendAndWait(
        [guid],
        5000,
        "Hey, say hello back within 5 seconds!");

    Console.WriteLine("My friend says: " + Encoding.UTF8.GetString(resp.Data));
}
catch (TimeoutException)
{
    Console.WriteLine("Too slow...");
}

static void ClientConnected(object sender, ConnectionEventArgs args)
{
    Console.WriteLine("Client connected: " + args.Client.ToString());
}

static void ClientDisconnected(object sender, DisconnectionEventArgs args)
{
    Console.WriteLine(
        "Client disconnected: "
        + args.Client.ToString()
        + ": "
        + args.Reason.ToString());
}

static void MessageReceived(object sender, MessageReceivedEventArgs args)
{
    Console.WriteLine(
        "Message from "
        + args.Client.ToString()
        + ": "
        + Encoding.UTF8.GetString(args.Data));
}

static async Task<SyncResponse> SyncRequestReceived(SyncRequest req)
{
    return new SyncResponse(req, "Hello back at you!");
}