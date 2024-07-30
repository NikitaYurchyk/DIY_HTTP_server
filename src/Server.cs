using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var msg = Encoding.ASCII.GetBytes(@"HTTP/1.1 200 OK\r\n\r\n");
var client = server.AcceptSocket(); // wait for client
client.Send(msg);


