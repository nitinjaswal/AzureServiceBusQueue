using Microsoft.Azure.ServiceBus;
using SBShared.Models;
using System.Text;
using System.Text.Json;

namespace SBReceiver;
class Program
{
    const string connectionString = "";
    const string queueName = "";
    static IQueueClient queueClient;
    static async Task Main(string[] args)
    {
        queueClient = new QueueClient(connectionString, queueName);//queue client talks to message queue
        

        //If AutoComplete is true and you don't call CompleteAsync, the message will still be completed automatically if no exceptions occur. However, if an exception is thrown, the message will not be completed and will be retried or dead-lettered based on the queue's configuration.
        var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
        {
            MaxConcurrentCalls = 1,
            AutoComplete = false,
            
            //This means you need to manually call CompleteAsync to acknowledge the message. If you don't call this method, the message will remain in the queue and eventually become available for reprocessing after the lock expires.
        };
        //Registering the message handler
        queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

        Console.ReadLine();
        await queueClient.CloseAsync();
    }

    private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
    {
       var jsonString=Encoding.UTF8.GetString(message.Body);

        PersonModel person = JsonSerializer.Deserialize<PersonModel>(jsonString);
        // Simulate failure for a specific message
        if (person.FirstName == "DeadLetterTest")
        {
            throw new Exception("Simulated processing failure");
            // Or simply return without calling CompleteAsync
        }
        Console.WriteLine($"Message received: {person.FirstName} {person.LastName}");

        //This method is used to complete a message in Azure Service Bus. Completing a message tells the Service Bus that the message has been successfully processed and can be removed from the queue.
        //When a message is received from the queue, it is locked for a specific duration (lock duration). During this time, no other receiver can process the message.
        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
    }

    private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
    {
        Console.WriteLine($"Message handler exception: {args.Exception}");
        return Task.CompletedTask;
    }
}