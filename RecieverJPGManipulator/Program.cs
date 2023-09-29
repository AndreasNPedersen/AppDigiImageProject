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
            
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "Digi-exchange", type: ExchangeType.Topic);
            
            // declare a server-named queue
            channel.QueueDeclare("Digi",durable:true,exclusive:false,autoDelete:false, new Dictionary<string, object> { { "x-dead-letter-exchange", "Dlx-exchange" },
                    { "x-dead-letter-routing-key", "Digi_pictures"}, { "x-message-ttl", 20000 } });

                channel.QueueBind(queue: "Digi",
                                  exchange: "Digi-exchange",
                                  routingKey: "Datatype.Picture.#");
            

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
                Console.WriteLine($" [x] Received '{routingKey}':'{recivedImage.Email}'");
                using var image = new MagickImage(recivedImage.FilePath);
                image.Posterize(10);
                //image.Write("image.png"); for saving on local storage

                MemoryStream stream = new MemoryStream();
                image.Write(stream);
                emailService.Send(stream,recivedImage.Email);
                stream.Close();
                stream.Dispose();

            };
            channel.BasicConsume(queue: "Digi",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}