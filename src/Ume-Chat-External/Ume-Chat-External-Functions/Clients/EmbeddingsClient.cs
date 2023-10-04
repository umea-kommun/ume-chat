﻿using System.Diagnostics;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions;

namespace Ume_Chat_External_Functions.Clients;

/// <summary>
///     Client for handling embeddings.
/// </summary>
[DebuggerDisplay("{URL}")]
public class EmbeddingsClient
{
    private readonly ILogger _logger;

    public EmbeddingsClient(ILogger logger)
    {
        try
        {
            _logger = logger;

            Key = Variables.Get("OPENAI_API_KEY");

            URL = Variables.Get("OPENAI_URL");
            EmbeddingsDeployment = Variables.Get("OPENAI_EMBEDDINGS_DEPLOYMENT");
            EmbeddingsBatchSize = Variables.GetInt("OPENAI_EMBEDDINGS_BATCH_SIZE");

            Client = new OpenAIClient(new Uri(URL), new AzureKeyCredential(Key));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed creation of OpenAIEmbeddingsClient!");
            throw;
        }
    }

    /// <summary>
    ///     Key to Azure OpenAI Service.
    /// </summary>
    private string Key { get; }

    /// <summary>
    ///     URL to Azure OpenAI Service.
    /// </summary>
    private string URL { get; }

    /// <summary>
    ///     Azure OpenAI embeddings deployment.
    /// </summary>
    private string EmbeddingsDeployment { get; }

    /// <summary>
    ///     Embeddings request batch limit.
    ///     2023-09-22: 16
    /// </summary>
    private int EmbeddingsBatchSize { get; }

    /// <summary>
    ///     Client for handling requests to Azure OpenAI Service.
    /// </summary>
    private OpenAIClient Client { get; }

    /// <summary>
    ///     Populate documents with embeddings based on it's content.
    /// </summary>
    /// <param name="documents">Documents to populate embeddings</param>
    /// <returns>List of documents populated with embeddings</returns>
    public List<Document> PopulateDocumentsWithEmbeddings(List<Document> documents)
    {
        _logger.LogInformation($"Retrieving embedding{Grammar.GetPlurality(documents.Count, "", "s")} for {{Count}} document{Grammar.GetPlurality(documents.Count, "", "s")}...",
                               documents.Count);

        try
        {
            // Batch documents in to required max embeddings batch size & convert to string
            var batches = documents.Chunk(EmbeddingsBatchSize).Select(c => c.Select(GetEmbeddingContent));

            // Run all batches synchronously
            var tasks = batches.Select(GetEmbeddingsAsync).ToList();

            // Wait for all embeddings to be retrieved
            Task.WaitAll(tasks.Cast<Task>().ToArray());

            var embeddings = tasks.SelectMany(t => t.Result.Value.Data).ToList();

            // Populate documents with embeddings
            for (var i = 0; i < embeddings.Count; i++)
                documents[i].Vector = embeddings[i].Embedding;

            _logger.LogInformation($"Retrieved embedding{Grammar.GetPlurality(documents.Count, "", "s")} for document{Grammar.GetPlurality(documents.Count, "", "s")}!");
            return documents;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of embeddings!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve a string containing Title, URL & Content of a provided document.
    /// </summary>
    /// <param name="document">Document to get string content from</param>
    /// <returns>String containing Title, URL & Content of provided document</returns>
    private static string GetEmbeddingContent(Document document)
    {
        return $"{document.Title}\n{document.URL}\n\n###\n\n{document.Content}";
    }

    /// <summary>
    ///     Retrieve embeddings for a batch of strings.
    /// </summary>
    /// <param name="batch">Batch to retrieve embeddings from</param>
    /// <returns>Response with Embeddings</returns>
    private async Task<Response<Embeddings>> GetEmbeddingsAsync(IEnumerable<string> batch)
    {
        return await Client.GetEmbeddingsAsync(EmbeddingsDeployment, new EmbeddingsOptions(batch));
    }
}