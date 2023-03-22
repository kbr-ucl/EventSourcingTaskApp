namespace ReadWriteTest;

public class WeatherForecast
{
    public WeatherForecast(WeatherForecastRecorded? recorded)
    {
        if (recorded == null) throw new ArgumentNullException(nameof(recorded));
        Date = DateOnly.FromDateTime(recorded.Date);
        TemperatureC = recorded.TemperatureC;
        Summary = recorded.Summary;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public DateOnly? Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);

    public string? Summary { get; set; }
}

public class WeatherForecastRecorded
{
    public static string StreamName = "WeatherForecast";

    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}