﻿
using NetMQ;
using NetMQ.Sockets;
using System.Threading.Tasks.Dataflow;


namespace Genie.Adapters.Brokers.ZeroMQ;

// https://stackoverflow.com/questions/34437298/how-can-i-read-messages-from-a-queue-in-parallel

public sealed class ZeroMQPump<T>
{

    /// <summary>
    /// Creates a <see cref="ZeroMQPump"/> and immediately starts pumping.
    /// </summary>
    public static ZeroMQPump<T> Run(DealerSocket consumer, Func<byte[], Task> processMessage, int maxDegreeOfParallelism,
        CancellationToken ct = default)
    {


        ArgumentNullException.ThrowIfNull(consumer, nameof(consumer));
        ArgumentNullException.ThrowIfNull(processMessage, nameof(processMessage));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxDegreeOfParallelism, 0, nameof(maxDegreeOfParallelism));
        
        ct.ThrowIfCancellationRequested();

        return new(consumer, processMessage, maxDegreeOfParallelism, ct);
    }

    private readonly TaskCompletionSource<bool> stop = new();

    private readonly AutoResetEvent latch = new(false);

    /// <summary>
    /// <see cref="Task"/> which completes when this instance
    /// stops due to a <see cref="Stop"/> or cancellation request.
    /// </summary>
    public Task Completion { get; }

    /// <summary>
    /// Maximum number of parallel message processors.
    /// </summary>
    public int MaxDegreeOfParallelism { get; }

    /// <summary>
    /// <see cref="MessageQueue"/> that is pumped by this instance.
    /// </summary>

    public DealerSocket Consumer { get; }

    public TaskCompletionSource<bool> Stop1 => stop;

    /// <summary>
    /// Creates a new <see cref="KafkaMessagePump"/> instance.
    /// </summary>
    private ZeroMQPump(DealerSocket consumer, Func<byte[], Task> processMessage, int maxDegreeOfParallelism, CancellationToken ct)
    {
        Consumer = consumer;
        MaxDegreeOfParallelism = maxDegreeOfParallelism;

        // Kick off the loop.
        Completion = RunAsync(processMessage, ct);
    }

    /// <summary>
    /// Soft-terminates the pump so that no more messages will be pumped.
    /// Any messages already removed from the message queue will be
    /// processed before this instance fully completes.
    /// </summary>
    public void Stop()
    {
        // Multiple calls to Stop are fine.
        Stop1.TrySetResult(true);
        latch.Set();
    }

    /// <summary>
    /// Pump implementation.
    /// </summary>
    private async Task RunAsync(Func<byte[], Task> processMessage, CancellationToken ct = default)
    {


        using (CancellationTokenSource producerCTS = ct.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(ct) : new())
        {
            // This CancellationToken will either be signaled
            // externally, or if our consumer errors.
            ct = producerCTS.Token;

            // Handover between producer and consumer.
            DataflowBlockOptions bufferOptions = new()
            {
                // There is no point in dequeuing more messages than we can process,
                // so we'll throttle the producer by limiting the buffer capacity.
                BoundedCapacity = MaxDegreeOfParallelism,
                CancellationToken = ct
            };

            BufferBlock<byte[]> buffer = new(bufferOptions);

            //var (routingKey, more) = await Connection.ReceiveRoutingKeyAsync();
            //var (message, _) = await Connection.ReceiveFrameBytesAsync();


            Task producer = Task.Run(async () =>
            {
                try
                {
                    var autoResetEvent = new AutoResetEvent(false);
                    new NetMQProactor(Consumer, async (socket, message) =>
                    {
                        autoResetEvent.Set();
                        foreach (var m in message)
                        {
                            await buffer.SendAsync(m.Buffer);
                        }

                    });

                    while (Stop1.Task.Status != TaskStatus.RanToCompletion)
                    {
                        //var message = Connection.ReceiveFrameBytes();

                        //var (routingKey, more) = await Connection.ReceiveRoutingKeyAsync();
                        //var (message, _) = await Connection.ReceiveFrameBytesAsync();



                        autoResetEvent.WaitOne();
                        ct.ThrowIfCancellationRequested();
                    }
                }
                catch(Exception ex)
                {
                    await File.WriteAllTextAsync(@"c:\temp\error.log", ex.ToString());
                }
                finally
                {
                    buffer.Complete();
                }
            },
            ct);

            // Wire up the parallel consumers.
            ExecutionDataflowBlockOptions executionOptions = new()
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                SingleProducerConstrained = true, // We don't require thread safety guarantees.
                BoundedCapacity = MaxDegreeOfParallelism,
            };

            ActionBlock<byte[]> consumer = new(async message =>
            {
                ct.ThrowIfCancellationRequested();

                await processMessage(message).ConfigureAwait(false);
            },
            executionOptions);

            buffer.LinkTo(consumer, new DataflowLinkOptions { PropagateCompletion = true });

            if (await Task.WhenAny(producer, consumer.Completion).ConfigureAwait(false) == consumer.Completion)
            {
                // If we got here, consumer probably errored. Stop the producer
                // before we throw so we don't go dequeuing more messages.
                producerCTS.Cancel();
            }

            // Task.WhenAll checks faulted tasks before checking any
            // canceled tasks, so if our consumer threw a legitimate
            // execption, that's what will be rethrown, not the OCE.
            await Task.WhenAll(producer, consumer.Completion).ConfigureAwait(false);
        }
    }
}