﻿using System.Text.Json.Serialization;

namespace RunnersListLibrary.Spotify.DTO.SpotifyDataObjects;

public class Image
{
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}