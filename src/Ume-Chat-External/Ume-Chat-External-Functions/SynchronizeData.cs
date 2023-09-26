﻿using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Ume_Chat_External_Functions.Clients;

namespace Ume_Chat_External_Functions;

/// <summary>
///     Azure Function to synchronize data between website & database.
/// </summary>
public class SynchronizeData(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SynchronizeData>();

    [Function("SynchronizeData")]
    public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "synchronize-data")] HttpRequestData req)
        // public async Task Run([TimerTrigger("0 0 15 * * 1-5")] TimerInfo timer) // TODO: Should automatically run 17:00 Swedish time every weekday
    {
        try
        {
            var dataClient = await DataClient.CreateAsync(_logger);
            await dataClient.SynchronizeAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed running SynchronizeData function!");
            throw;
        }
    }
}
