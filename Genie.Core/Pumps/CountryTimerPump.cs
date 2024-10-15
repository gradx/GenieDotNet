using Genie.Core.Persistence;
using ParquetSharp;
using System.Threading.Tasks.Dataflow;

namespace Genie.Core.Pumps;

public sealed class CountryTimerPump
{
    private readonly TaskCompletionSource<bool> stop = new();

    public Task Completion { get; }

    public int MaxDegreeOfParallelism { get; }

    public long Duration { get; set; }


    public static CountryTimerPump Run(long duration, Func<CountryPostalCode, Task> processMessage, int maxDegreeOfParallelism, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(processMessage, nameof(processMessage));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxDegreeOfParallelism, 0, nameof(maxDegreeOfParallelism));

        ct.ThrowIfCancellationRequested();

        return new(duration, processMessage, maxDegreeOfParallelism, ct);
    }

    private CountryTimerPump(long duration, Func<CountryPostalCode, Task> processMessage, int maxDegreeOfParallelism, CancellationToken ct)
    {
        Duration = duration;
        MaxDegreeOfParallelism = maxDegreeOfParallelism;

        Completion = RunAsync(processMessage, ct);
    }

    public void Stop()
    {
        stop.TrySetResult(true);
    }

    private async Task RunAsync(Func<CountryPostalCode, Task> processMessage, CancellationToken ct = default)
    {

        using (CancellationTokenSource producerCTS = ct.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(ct) : new())
        {
            ct = producerCTS.Token;

            DataflowBlockOptions bufferOptions = new()
            {
                BoundedCapacity = MaxDegreeOfParallelism,
                CancellationToken = ct
            };

            BufferBlock<CountryPostalCode> buffer = new(bufferOptions);

            Task producer = Task.Run(async () =>
            {
                try
                {

                    var file1 = new ParquetFileReader(@"c:\temp\geonames-postal-code.parquet");


                    long counter = 0;

                    bool completed = false;

                    var timer = new System.Timers.Timer(Duration * 1000)
                    {
                        Enabled = true,
                    };

                    timer.Elapsed += (sender, e) =>
                    {
                        completed = true;
                    };

                    timer.Start();

                    while (!completed)
                    {
                        for (var i = 0; i < file1.FileMetaData.NumRowGroups; i++)
                        {
                            var rowGroupReader = file1.RowGroup(i);

                            var country_code = rowGroupReader.Column(0).LogicalReader<string>().ToList();
                            var postal_code = rowGroupReader.Column(1).LogicalReader<string>().ToList();
                            var place_name = rowGroupReader.Column(2).LogicalReader<string>().ToList();
                            var latitude = rowGroupReader.Column(9).LogicalReader<double?>().ToList();
                            var longitude = rowGroupReader.Column(10).LogicalReader<double?>().ToList();

                            for (int a = 0; a < country_code.Count; a++)
                            {
                                counter++;

                                if (completed)
                                    return;

                                await buffer.SendAsync(new CountryPostalCode
                                {
                                    Id = counter,
                                    CountryCode = country_code[a],
                                    PostalCode = postal_code[a],
                                    PlaceName = place_name[a],
                                    Latitude = latitude[a],
                                    Longitude = longitude[a]
                                }).ConfigureAwait(false);

                                ct.ThrowIfCancellationRequested();
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    await File.WriteAllTextAsync(@"c:\temp\error.log", ex.ToString());
                }
                finally
                {
                    buffer.Complete();
                }
            },
            ct);

            ExecutionDataflowBlockOptions executionOptions = new()
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                SingleProducerConstrained = true, // We don't require thread safety guarantees.
                BoundedCapacity = MaxDegreeOfParallelism,
            };

            ActionBlock<CountryPostalCode> consumer = new(async message =>
            {
                ct.ThrowIfCancellationRequested();

                await processMessage(message).ConfigureAwait(false);
            },
            executionOptions);

            buffer.LinkTo(consumer, new DataflowLinkOptions { PropagateCompletion = true });

            if (await Task.WhenAny(producer, consumer.Completion).ConfigureAwait(false) == consumer.Completion)
            {
                producerCTS.Cancel();
            }

            await Task.WhenAll(producer, consumer.Completion).ConfigureAwait(false);
        }
    }
}