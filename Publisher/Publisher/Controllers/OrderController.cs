using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Publisher.Domain;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace Publisher.Controllers
{
    [Route("api/")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        [HttpPost("order")]
        public IActionResult PostOrder(Order order)
        {
            try
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

                    string message = JsonSerializer.Serialize(order);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "orderQueue",
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine("Pedido enviado com sucesso! Aguarde o processamento.");

                    return Accepted(order);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao tentar criar um novo pedido", ex);

                return new StatusCodeResult(500);
            }
        }
    }
}
