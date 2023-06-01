using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Caching;
using System.Threading.Tasks;
namespace SistemskoProgramiranjeLab2;

public class AsyncHttpServer
{
    private static Cache cache = new(1000);
    private static HttpListener socketListener = new();
    private static HttpClient client = new();

    public static async Task Start()
    {
        socketListener.Prefixes.Add("http://127.0.0.1:8080/");
        Console.WriteLine("Server has started listening on port 8080 at localhost address");
        socketListener.Start();

        while (socketListener.IsListening)
        {
            
            var context = await socketListener.GetContextAsync();
            Stopwatch stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var response = context.Response;
            var rawUrl = request.RawUrl;

            string responseData = await cache.GetCacheEntryOrDefault(rawUrl);
            Action action;
            if (responseData == String.Empty)
            {
                action = async () =>
                {
                    var url = $"https://api.deezer.com{rawUrl}";
                    var result = await client.GetAsync(url);
                    var resultBody = await result.Content.ReadAsStringAsync();
                    var json = JObject.Parse(resultBody);

                    JToken data = json["data"];
                    if (data == null)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        await SendResponse(HttpResourceNotFound(), response);
                    }
                    else
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("<html> <head> <meta charset=\"UTF-8\"></head> <body>");
                        stringBuilder.Append("<ul>");
                        foreach (var song in data)
                        {
                            stringBuilder.Append($"<li>{song["title"]}</li>");
                        }
                        stringBuilder.Append("</ul>");
                        stringBuilder.Append("</body>");
                        stringBuilder.Append("</html>");
                        responseData = stringBuilder.ToString();
                        response.StatusCode = (int)HttpStatusCode.OK;
                        await SendResponse(responseData, response);
                        stopwatch.Stop();
                        Console.WriteLine($"Time required : { stopwatch.ElapsedMilliseconds}ms");
                        await cache.InsertCacheEntry(rawUrl, responseData);
                    }
                };
            }
            else
            {
                action = async () =>
                {
                    response.StatusCode= (int)HttpStatusCode.OK;
                    await SendResponse(responseData, response);
                    stopwatch.Stop();
                    Console.WriteLine($"Time required : {stopwatch.ElapsedMilliseconds}ms");
                };
            }

            new Task(action).Start();   
            
        }
    }

    private static async Task SendResponse(string responseData, HttpListenerResponse response)
    {
        response.ContentType = "text/html";
        var contentBody = System.Text.Encoding.UTF8.GetBytes(responseData);
        using (var outputStream = response.OutputStream)
        {
            await outputStream.WriteAsync(contentBody, 0, contentBody.Length);
        }
    }
    private static string HttpResourceNotFound()
    {
        return "<html>" +
            "<head> </head>" +
            "<body>" +
            "<h3>ERROR 404: INVALID REQUEST</h3>" +
            "</body>" +
            "</html>";
    }

}
