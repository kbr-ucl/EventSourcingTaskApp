using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EventSourcingTaskApp.Core.Framework;
using EventStore.Client;

namespace EventSourcingTaskApp.Infrastructure;

public class AggregateRepository
{
    private readonly EventStoreClient _eventStore;

    public AggregateRepository(EventStoreClient eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task SaveAsync<T>(T aggregate) where T : Aggregate, new()
    {
        var events = aggregate.GetChanges()
            .Select(@event => new EventData(
                Uuid.NewUuid(),
                @event.GetType().Name,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)),
                Encoding.UTF8.GetBytes(@event.GetType().FullName ?? throw new InvalidOperationException())))
            .ToArray();

        if (!events.Any()) return;

        var streamName = GetStreamName(aggregate, aggregate.Id);

        await _eventStore.AppendToStreamAsync(streamName, StreamState.Any, events);
    }

    public async Task<T> LoadAsync<T>(Guid aggregateId) where T : Aggregate, new()
    {
        if (aggregateId == Guid.Empty)
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(aggregateId));

        var aggregate = new T();
        var streamName = GetStreamName(aggregate, aggregateId);

        var events = _eventStore.ReadStreamAsync(
            Direction.Forwards,
            streamName, StreamPosition.Start);

        if (events.ReadState.Result == ReadState.StreamNotFound)
        {
            return aggregate;
        }

        var loadedFromStore = await events.ToListAsync();

        if (!loadedFromStore.Any()) return aggregate;

        var loadedEvents = loadedFromStore.Select(@event =>
            JsonSerializer.Deserialize(Encoding.UTF8.GetString(@event.OriginalEvent.Data.ToArray()),
                Type.GetType(Encoding.UTF8.GetString(@event.OriginalEvent.Metadata.ToArray())) ?? typeof(object))
        );


        aggregate.Load(
            loadedFromStore.Select(a => a.Event).Last().EventNumber.ToInt64(),
            loadedEvents);

        return aggregate;
    }

    private string GetStreamName<T>(T type, Guid aggregateId)
    {
        return $"{type.GetType().Name}-{aggregateId}";
    }
}