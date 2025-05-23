# Azure ServiceBus Queue

This project demonstrates a simple sender and receiver implementation for Azure Service Bus Queue using .NET 8 and Blazor.  
It includes two main components:
- **sbsender**: Sends messages to an Azure Service Bus Queue.
- **sbreceiver**: Receives and processes messages from the Azure Service Bus Queue.

The solution is designed to help developers understand how to integrate Azure Service Bus messaging into Blazor applications.


## Key Concepts

- **Queues**: Support FIFO (First-In-First-Out) delivery and allow only one consumer per message.
- **Topics**: Enable a publish-subscribe model, allowing multiple subscribers for a single topic.

### Queue Features

- **Multiple Queues**: You can create multiple queues in a Service Bus, each dedicated to a specific task.
- **Message Time To Live (TTL)**: Defines how long a message remains in the queue before it expires or is moved to the dead-letter queue.
- **Dead-letter Queue**: A special sub-queue for messages that cannot be delivered or processed successfully (e.g., expired TTL or exceeded max delivery attempts). Useful for debugging and inspection.
- **Max Queue Size**: Default is 1GB. Partitioned queues can increase this up to 80GB (at higher cost).
- **Max Delivery Count**: The number of times a message delivery can be attempted before moving to the dead-letter queue (default is 10).
- **Lock Duration**: When a message is received, it is locked for a configurable period (default 30 seconds). If not processed in time, the lock is released for another receiver to process.

### Topics vs. Queues

- **Queues**: One-to-one communication (single consumer per message).
- **Topics**: One-to-many communication (multiple subscribers per topic).

## Usage Notes

- When creating a queue, you can set the TTL for messages. Messages not consumed within this time are moved to the dead-letter queue.
- The dead-letter queue is used for messages that cannot be processed successfully, providing a way to inspect and debug problematic messages.
- Lock duration ensures that a message is temporarily unavailable to other receivers while being processed. If not completed within the lock duration, the message becomes available to other receivers until the max delivery count is reached.
- Topics: is like a pub sub model, where we can have multiple subscribers for a single topic, whereas in queue we can have only one subscriber(1-1)     

## Peek Lock
- Peek lock is a mode of receiving messages from a queue or subscription in Azure Service Bus. When a message is received in peek lock mode, it is not removed from the queue immediately. Instead, it is locked for a specified duration (lock duration) to allow the receiver to process the message without it being consumed by other receivers. If the message is successfully processed within the lock duration, it can be completed (removed from the queue). If not, the lock expires, and the message becomes available for other receivers to process.
- Peek lock is useful for scenarios where you want to ensure that a message is processed only once and allows for retries in case of processing failures. It helps prevent message loss and ensures that messages are not lost if the receiver fails to process them within the lock duration.
- Peek Lock is the default receive mode in Azure Service Bus. When a message is received, it is locked for a short period (lock duration, e.g., 30 seconds by default).
- If you call CompleteAsync, the message is deleted from the queue.
- If you do not call CompleteAsync (for example, due to an exception), the lock expires and the message becomes available for delivery again (retry).
- If the message is not processed successfully after a certain number of attempts (max delivery count), it is moved to the dead-letter queue for further inspection.
- **Note:** The lock duration is configurable, and you can set it according to your application's needs. The default lock duration is 30 seconds, but you can increase or decrease it based on your processing requirements.

## Message duplication
- Azure Service Bus handles message duplication using the MessageId property.
- **MessageId**: Each message sent to a queue or topic can have a MessageId set by the sender. Azure Service Bus uses this ID to identify duplicate messages.


**Summary:**  
The dead-letter queue is a special sub-queue for messages that cannot be delivered or processed successfully, such as those that exceed their TTL or max delivery count.
Lock duration ensures messages are temporarily unavailable to other receivers while being processed, and unprocessed messages are retried until the max delivery count is reached.
Max queue size is 1GB, if we want to increase the size, we can use partitioned queues, which will increase the size to 80GB, but it will be more expensive
Max delivery count: 10, if the message is not consumed within 10 times, it will be sent to dead letter queue
Lock duration: 30 seconds, if the message is not consumed within 30 seconds, it will be sent to dead letter queue. Lock durtation sets lock on the message ,ley say for 30 seconds(you can configure it). Recieiver will get the message and it will be locked for 30 seconds, if the receiver is not able to process the message within 30 seconds,
then it will pass on to next reader. Now next reader starts reading message it also fails to process the message, then it will be passed on to next reader.This will continue till the max delivery count is reached(eg 10). If the max delivery count is reached, then the message will be sent to dead letter queue. Dead letter queue is a special queue where we can send the messages which are not consumed within the time to live, so that we can check the messages later.
Dead letter queue is used for debugging purpose, so that we can check the messages which are not consumed within the time to live.
> The message is passed to the next receiver means that if you have multiple instances of your receiver application (for example, several deployments of your Blazor app or background service), only one instance will process a given message at a time.  
> **If the first instance fails to process the message within the lock duration, the message becomes available for another instance of the same consumer application to process. This is called the "competing consumers" pattern.**

