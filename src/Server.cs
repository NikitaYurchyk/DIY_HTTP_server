using System.Net;
using System.Net.Sockets;
using System.Text;


 Console.WriteLine("Logs from your program will appear here!");
 TcpListener server = new(IPAddress.Any, 4221);
 try
 {
  server.Start();
  while (true)
  {
   var client = await server.AcceptTcpClientAsync();
   Task.Run(async () => await HttpClientHandler(client));
  }
 }
 finally
 {
  server.Stop();
 }



 async Task HttpClientHandler(TcpClient client)
 {
  byte[] buffer = new byte[1024];
  var recMsg = client.Client.Receive(buffer);
  string data = Encoding.UTF8.GetString(buffer);


  string[] listOfWords = data.Split("\r\n");

  var firstLine = listOfWords[0].Split(" ");


  if (firstLine[1] == "/")
  {
   byte[] theWholeResp =
    Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: 0\r\n\r\n");
   await client.Client.SendAsync(theWholeResp);
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
   await client.Client.SendAsync(encrypResp);
  }

  else if (firstLine[1].Contains("/user-agent"))
  {
   var partWithUserAgent = listOfWords[2].Split(" ");
   string resp =
    $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {partWithUserAgent[1].Length}\r\n\r\n{partWithUserAgent[1]}\r\n\r\n";
   await client.Client.SendAsync(Encoding.UTF8.GetBytes(resp));

  }
  else
  {
   await client.Client.SendAsync(Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n"));
  }
 }

 
 


