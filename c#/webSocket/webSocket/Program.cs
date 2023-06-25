using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace webSocket
{
    class Server
    {
        public static void Main()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);
            server.Start();

            Console.WriteLine("Server has started on http://127.0.0.1:80.\nWaiting for a connection...");

            TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("A client connected.");

            NetworkStream stream = client.GetStream();

            while (true)
            {
                while (!stream.DataAvailable) ;

                Byte[] bytes = new Byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);

                String data = Encoding.UTF8.GetString(bytes);

                if (new Regex("^GET").IsMatch(data))
                {
                    Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                    + "Connection: Upgrade" + Environment.NewLine
                    + "Upgrade: websocket" + Environment.NewLine + 
                    "Sec-WebSocket-Accept: " + Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))) 
                    + Environment.NewLine
                    + Environment.NewLine);

                    stream.Write(response, 0, response.Length);
                    
                    string htmlResponse = BuildHtmlResponse(data);

                    string responseHeaders = "HTTP/1.1 200 OK" + Environment.NewLine
                        + "Content-Type: text/html" + Environment.NewLine
                        + "Content-Length: " + htmlResponse.Length + Environment.NewLine
                        + Environment.NewLine;

                    byte[] responseHeadersBytes = Encoding.UTF8.GetBytes(responseHeaders);
                    byte[] htmlResponseBytes = Encoding.UTF8.GetBytes(htmlResponse);

                    // Send the response
                    stream.Write(responseHeadersBytes, 0, responseHeadersBytes.Length);
                    stream.Write(htmlResponseBytes, 0, htmlResponseBytes.Length);
                    stream.Flush();
                }
            }
        }

        public static string BuildHtmlResponse(string data)
        {
            string so = "";
            string browser = "";
            char[] dataArray = data.ToCharArray();

            if (data.Contains("sec-ch-ua-platform:"))
            {
                int index = data.IndexOf("sec-ch-ua-platform:");

                for (int i = index; dataArray[i] != '\n'; i++)
                {
                    so += dataArray[i];
                }

                so = so.Split(":")[1];
            }

            if (data.Contains("sec-ch-ua:"))
            {
                int index = data.IndexOf("sec-ch-ua:");

                for (int i = index; dataArray[i] != '\n'; i++)
                {
                    browser += dataArray[i];
                }

                browser = browser.Split(":")[1];
            }

            return string.Format(@"
                    <!DOCTYPE html>
                    <html lang=""en"">
                    <head>
                      <meta charset=""UTF-8"">
                      <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                      <title>Document</title>
                    </head>
                        <body>
                            <h1>Hi, my friend!</h1>
                            <p>Your infos:</p>
                            <p>so: {0}</p>
                            <p>browser infos: {1}</p>
                        </body>
                    </html>
                ", so, browser);
        }
    }
}