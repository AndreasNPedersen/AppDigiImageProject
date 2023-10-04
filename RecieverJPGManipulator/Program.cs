using ImageMagick;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RecieverJPGManipulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EmailService emailService = new EmailService();
            HttpClient httpClient = new HttpClient();
            
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "Digi-exchange", type: ExchangeType.Topic);
            
            // declare a server-named queue
            channel.QueueDeclare("Digi",durable:true,exclusive:false,autoDelete:false, new Dictionary<string, object> { 
                { "x-dead-letter-exchange", "Dlx-exchange" }, { "x-dead-letter-routing-key", "Digi_pictures"}, 
                { "x-message-ttl", 20000 } });

                channel.QueueBind(queue: "Digi",
                                  exchange: "Digi-exchange",
                                  routingKey: "Datatype.Picture.#"); //get's all picture types
            

            Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Image recivedImage;
                try
                {
                    recivedImage = JsonSerializer.Deserialize<Image>(message);

                    if (recivedImage == null) { throw new ArgumentNullException(); }
                }catch (Exception ex) {
                    return;
                }
                
                var routingKey = ea.RoutingKey;
                Console.WriteLine($" [x] Received '{routingKey}' to'{recivedImage.Email}'");
             
                var gottenImage = StorageHelper.GetImageAsync(httpClient, recivedImage.FilePath).Result;
                gottenImage.Position = 0;

                using var image = new MagickImage(gottenImage);
                
                //things done to the image
                image.Posterize(20);


                //image.Write("image.png"); for saving on local storage
                MemoryStream stream = new MemoryStream();
                image.Write(stream);
                emailService.Send(stream,recivedImage.Email);

                //always remember to close streams
                gottenImage.Close();
                gottenImage.Dispose();
                stream.Close();
                stream.Dispose();

                // deletes the used image on storage server
                StorageHelper.DeleteImageAsync(httpClient, recivedImage.FilePath); 
            };
            channel.BasicConsume(queue: "Digi",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}