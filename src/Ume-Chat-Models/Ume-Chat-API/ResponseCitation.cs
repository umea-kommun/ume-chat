﻿using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_Models.Ume_Chat_API;

/// <summary>
///     Citation data transfer object
/// </summary>
[DebuggerDisplay("{Title}")]
public class ResponseCitation
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string URL { get; set; } = string.Empty;
}