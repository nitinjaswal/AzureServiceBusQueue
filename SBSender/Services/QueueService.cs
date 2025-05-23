using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace SBSender.Services
{
    public class QueueService : IQueueService
    {
        private readonly IConfiguration _config;
        public QueueService(IConfiguration config)
        {
            _config = config;
        }
        //We can have lot of queues inside service bus, so we have to mention queue name as we each dedicated for each different task
        //Azure Service Bus supports multiple queues, each dedicated to a specific task.
        // - Queues: Use FIFO (First-In-First-Out) and support a single consumer per message.
        // - Topics: Use a publish-subscribe model, allowing multiple subscribers for a single topic.
        //
        // Key concepts:
        // - Message Time To Live (TTL): Specifies how long a message remains in the queue before expiring or being moved to the dead-letter queue.
        // - Dead-letter queue: Stores messages that cannot be delivered or processed successfully (e.g., expired TTL or exceeded max delivery attempts). Useful for debugging and later inspection.
        // - Max queue size: Default is 1GB. Partitioned queues can increase this up to 80GB (at higher cost).
        // - Max delivery count: Number of times a message delivery can be attempted before moving to the dead-letter queue (default is 10).
        // - Lock duration: When a message is received, it is locked for a configurable period (default 30 seconds). If not processed in time, the lock is released for another receiver to process.
        //

        //Topics: is like a pub sub model, where we can have multiple subscribers for a single topic, whereas in queue we can have only one subscriber(1-1)

        //While creating queue, we can set the time to live for the message, so that if the message is not consumed within that time, it will be deleted from the queue, dead letter queue is a special queue where we can send the messages which are not consumed within the time to live, so that we can check the messages later
        //Max queue size is 1GB, if we want to increase the size, we can use partitioned queues, which will increase the size to 80GB, but it will be more expensive
        //Max delivery count: 10, if the message is not consumed within 10 times, it will be sent to dead letter queue
        //Lock duration: 30 seconds, if the message is not consumed within 30 seconds, it will be sent to dead letter queue
        //lock durtation sets lock on the message ,ley say for 30 seconds(you can configure it). Recieiver will get the message and it will be locked for 30 seconds, if the receiver is not able to process the message within 30 seconds,
        //then it will pass on to next reader. Now next reader starts reading message it also fails t o process the message, then it will be passed on to next reader.
        //This will continue till the max delivery count is reached(eg 10). If the max delivery count is reached, then the message will be sent to dead letter queue. Dead letter queue is a special queue where we can send the messages which are not consumed within the time to live, so that we can check the messages later. Dead letter queue is used for debugging purpose, so that we can check the messages which are not consumed within the time to live.

        //what is dead letter queue: Special sub-queue for messages that cannot be delivered or processed successfully.
        //Message time to live: for how may days message can live in queue, before moving to dead letter queue or it is expired
        public async Task SendMessageAsync<T>(T serviceBusMessage, string queueName)
        {
            var queueClient = new QueueClient(_config.GetConnectionString("AzureServiceBus"), queueName);
            string messageBody = JsonSerializer.Serialize(serviceBusMessage);

            //Converting into byte array
            var message = new Message(Encoding.UTF8.GetBytes(messageBody))
            {
                MessageId = Guid.NewGuid().ToString(),
            };
            await queueClient.SendAsync(message);

        }
    }
}
