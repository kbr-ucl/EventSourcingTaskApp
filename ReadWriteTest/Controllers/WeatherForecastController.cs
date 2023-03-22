using System.Text.Json;
using EventStore.Client;
using Microsoft.AspNetCore.Mvc;

namespace ReadWriteTest.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly EventStoreClient _client;

    public WeatherForecastController(EventStoreClient client)
    {
        _client = client;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var result = new List<WeatherForecast>();

        var streamResult = _client.ReadStreamAsync(Direction.Forwards,
            WeatherForecastRecorded.StreamName,
            StreamPosition.Start);
        await foreach (var item in streamResult)
        {
            //item.Event.EventType. // <-- use this to determine which class to serialise to
            var recorded = JsonSerializer.Deserialize(item.Event.Data.Span,
                typeof(WeatherForecastRecorded));

            result.Add(new WeatherForecast(recorded as WeatherForecastRecorded));
        }

        return result;
    }

    [HttpPost]
    public async Task<object> PostAsync(WeatherForecast data)
    {
        var weatherForecastRecordedEvent = new WeatherForecastRecorded
        {
            Date = DateTime.Now,
            Summary = data.Summary,
            TemperatureC = data.TemperatureC
        };

        var utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(weatherForecastRecordedEvent);

        var eventData = new EventData(Uuid.NewUuid(),
            nameof(WeatherForecastRecorded),
            utf8Bytes.AsMemory());

        var writeResult = await _client
            .AppendToStreamAsync(WeatherForecastRecorded.StreamName,
                StreamState.Any,
                new[] {eventData});

        return writeResult;
    }
}