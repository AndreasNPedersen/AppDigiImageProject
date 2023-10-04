using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfAppDigi
{
    /// <summary>
    /// Send the file path from the local computer to the receiver
    /// If the server should be more separated, an API should take the filestream from the 
    /// Local client, and send it to a storage, where from it can be picked up by the receiver  
    /// </summary>
    public class RabbitSender
    {
        public async Task Send(string fileName, string email)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "Digi-exchange", type: ExchangeType.Topic);

            //this is very specific for every file extension with only 3 letters, use regex for more variable input
            //this should also be in another class:
            var routingKey = "";
            switch (fileName.Substring(fileName.Length - 4))
            {
                case ".jpg":
                    routingKey = "Datatype.Picture.jpg";
                    break;
                case ".png":
                    routingKey = "Datatype.Picture.png";
                    break;
                case ".jpeg":
                    routingKey = "Datatype.Picture.jpeg";
                    break;
                case ".gif":
                    routingKey = "Datatype.Gif.gif";
                    break;
                case ".pdf":
                    routingKey = "Datatype.PDF.pdf";
                    break;
            }

            FileInfo fileInfo = new FileInfo(fileName);


            string storageURL = "https://localhost:44303/api/Storage/";
            HttpClient httpClient = new HttpClient();
            using var stream = File.OpenRead(fileName);
            using var content = new MultipartFormDataContent
            {
                { new StreamContent(stream), "file", fileInfo.Name }
            };
            var response = await httpClient.PostAsync(storageURL,content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Image imageInfo = new Image() { Email = email, FilePath = responseContent };
                string message = JsonSerializer.Serialize(imageInfo);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "Digi-exchange",
                                     routingKey: routingKey,
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}
