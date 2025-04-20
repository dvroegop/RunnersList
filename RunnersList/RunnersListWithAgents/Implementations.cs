namespace RunnersListWithAgents;

internal class Implementations
{
    public string GetCurrentWeatherAtLocation(string location, string temperatureUnit = "f")
    {
        return location switch
        {
            "Seattle, WA" => temperatureUnit == "f" ? "70f" : "21c",
            "Amsterdam" => temperatureUnit == "f" ? "60f" : "15c",
            _ => throw new NotImplementedException()
        };
    }

    public async Task<string> GetUserFavoriteCity()
    {
        return await Task.FromResult("Amsterdam");
    }

    public string GetCityNickName(string location)
    {
        return location switch
        {
            "Amsterdam" => "The Venice of the North",
            "Paris" => "The City of Light",
            "London" => "The Big Smoke",
            "Seattle" => "The Emerald City",
            _ => "Unknown"
        };
    }


    public string GetWeather(string city)
    {
        return city switch
        {
            "Amsterdam" => "20",
            "Paris" => "25",
            "London" => "15",
            "Seattle" => "18",
            _ => "0"
        };
    }

    public string GetWeather(string city, string unit)
    {
        return city switch
        {
            "Amsterdam" => unit == "c" ? "20" : "68",
            "Paris" => unit == "c" ? "25" : "77",
            "London" => unit == "c" ? "15" : "59",
            "Seattle" => unit == "c" ? "18" : "64",
            _ => unit == "c" ? "0" : "32"
        };
    }
}