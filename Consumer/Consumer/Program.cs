using Consumer.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Consumer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "orderQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.Span;
                        var message = Encoding.UTF8.GetString(body);
                        var order = System.Text.Json.JsonSerializer.Deserialize<Order>(message);

                        Console.WriteLine($"Order: {order.OrderNumber}|{order.ItemName}|{order.Price:N2}");
                        Console.WriteLine("Mensagem lida com sucesso!");

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        //Logger
                        channel.BasicNack(ea.DeliveryTag, false, true);
                        Console.WriteLine("Ocorreu um erro ao processar esta mensagem, ela será devolvida à fila.");
                    }
                };
                channel.BasicConsume(queue: "orderQueue",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine("Aguardando novas mensagens. Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
