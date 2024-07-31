using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

var failMsg = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");


var client = server.AcceptSocket(); 
byte[] buffer = new byte[1024];
var recMsg = client.Receive(buffer);
string data = Encoding.UTF8.GetString(buffer);


string[] listOfWords = data.Split("\r\n");
Console.WriteLine("Check request");
foreach (var word in listOfWords)
{
 Console.WriteLine(word);
}




var firstLine = listOfWords[0].Split(" ");
if (firstLine[1] == "/")
{
 byte[] theWholeResp = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: 0\r\n\r\n");
 client.Send(theWholeResp);
}
else if (firstLine[1].Contains("/echo/"))
{
 int index = firstLine[1].IndexOf('o');
 string body = "";
 for (int i = index + 2; i < firstLine[1].Length; i++)
 {
  body += firstLine[1][i];
 }
 string resp = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {body.Length}\r\n\r\n{body}";
 byte[] encrypResp = Encoding.UTF8.GetBytes(resp);
 client.Send(encrypResp);
}
else if (firstLine[1].Contains("/user-agent"))
{
 var partWithUserAgent = listOfWords[3].Split(" ");
 string resp = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {partWithUserAgent[1].Length}\r\n\r\n{partWithUserAgent[1]}";
 client.Send(Encoding.UTF8.GetBytes(resp));

}
else
{
 client.Send(failMsg);
}


client.Shutdown(SocketShutdown.Both);
client.Close();


