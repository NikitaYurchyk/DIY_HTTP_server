using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var successMsg = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
var failMsg = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");

var client = server.AcceptSocket(); 
byte[] buffer = new byte[1024];
var recMsg = client.Receive(buffer);
string data = Encoding.UTF8.GetString(buffer);
int indexOfSlash = data.IndexOf('/');



if (data[indexOfSlash + 1] != ' ')
{
 client.Send(failMsg);
}
else
{
 client.Send(successMsg);
}
// Console.WriteLine($"RECEIVED DATA: {data}");

client.Shutdown(SocketShutdown.Both);
client.Close();


