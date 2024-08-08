using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;


class HttpServer
{
    private readonly int port;
    
    public HttpServer(int port)
    {
        this.port = port;
    }

    public async Task StartAsync()
    {
        TcpListener server = new(IPAddress.Any, port);
        try
        {
            server.Start();
            Console.WriteLine("Server started");
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
    }

    private async Task HttpClientHandler(TcpClient client)
    {
        byte[] buffer = new byte[1024];
        var recMsg = client.Client.Receive(buffer);
        string data = Encoding.UTF8.GetString(buffer);
        string[] listOfWords = data.Split("\r\n");

        foreach (var i in listOfWords)
        {
            Console.WriteLine($"-{i}");
        }

        var firstLine = listOfWords[0].Split(" ");
        var encoding = listOfWords[2].Split(" ");
        if (firstLine[0] == "GET")
        {
            if (firstLine[1] == "/")
            {
                byte[] theWholeResp = Encoding.UTF8.GetBytes(
                    $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: 0\r\n\r\n");
                await client.Client.SendAsync(theWholeResp);
            }
            else if (firstLine[1].Contains("/echo/"))
            {
                int index = firstLine[1].IndexOf('o');
                string body = firstLine[1].Substring(index + 2);

                bool checkValidEncoding = encoding.Any(i => i.Contains("gzip"));

                if (encoding.Length > 1 && checkValidEncoding)
                {
                    byte[] compressedBody = await CompressString(body);

                    string resp = $"HTTP/1.1 200 OK\r\nContent-Encoding: gzip\r\nContent-Type: text/plain\r\nContent-Length:{compressedBody.Length}\r\n\r\n";
                    byte[] encrypResp = Encoding.UTF8.GetBytes(resp);
                    byte[] fullResponse = ConcatenateByteArrays(encrypResp, compressedBody);

                    await client.Client.SendAsync(fullResponse);
                }
                else
                {
                    string resp = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {body.Length}\r\n\r\n{body}";
                    byte[] encrypResp = Encoding.UTF8.GetBytes(resp);
                    await client.Client.SendAsync(encrypResp);
                }
            }
            else if (firstLine[1].Contains("/user-agent"))
            {
                var partWithUserAgent = listOfWords[2].Split(" ");
                string resp =
                    $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {partWithUserAgent[1].Length}\r\n\r\n{partWithUserAgent[1]}\r\n\r\n";
                await client.Client.SendAsync(Encoding.UTF8.GetBytes(resp));
            }
            else if (firstLine[1].Contains("/files"))
            {
                var listOfDirectories = firstLine[1].Split("/");
                string res = await ReadFile(listOfDirectories[2]);
                if (res == "")
                {
                    await client.Client.SendAsync(
                        Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n"));
                }
                else
                {
                    string resp =
                        $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {res.Length}\r\n\r\n{res}";
                    await client.Client.SendAsync(Encoding.UTF8.GetBytes(resp));
                }
            }
            else
            {
                await client.Client.SendAsync(
                    Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n"));
            }
        }
        else if (firstLine[0] == "POST")
        {
            if (firstLine[1].Contains("/files"))
            {
                var listOfDirectories = firstLine[1].Split("/");
                string[] contentLenght = listOfWords[2].Split(" ");
                int len = int.Parse(contentLenght[1]);
                string parsedBody = listOfWords[5].Substring(0, len);
                await WriteToFile(listOfDirectories[2], parsedBody);
                await client.Client.SendAsync(
                    Encoding.UTF8.GetBytes("HTTP/1.1 201 Created\r\n\r\n"));
            }
            else
            {
                await client.Client.SendAsync(
                    Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n"));
            }
        }
    }

    private async Task<string> ReadFile(string fileName)
    {
        string path = $"{Environment.GetCommandLineArgs()[2]}/{fileName}";
        if (!File.Exists(path))
        {
            return "";
        }
        string[] text = await File.ReadAllLinesAsync(path);
        string res = String.Join("", text);
        return res;
    }

    private async Task WriteToFile(string fileName, string text)
    {
        string path = $"{Environment.GetCommandLineArgs()[2]}/{fileName}";
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            await writer.WriteAsync(text);
        }
    }

    private async Task<byte[]> CompressString(string text)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(text);
        using (var memoryStream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                await gzipStream.WriteAsync(byteArray, 0, byteArray.Length);
            }
            return memoryStream.ToArray();
        }
    }

    private byte[] ConcatenateByteArrays(byte[] arr1, byte[] arr2)
    {
        byte[] result = new byte[arr1.Length + arr2.Length];
        Buffer.BlockCopy(arr1, 0, result, 0, arr1.Length);
        Buffer.BlockCopy(arr2, 0, result, arr1.Length, arr2.Length);
        return result;
    }

    static async Task Main(string[] args)
    {
        var server = new HttpServer(4221);
        await server.StartAsync();
    }
}

