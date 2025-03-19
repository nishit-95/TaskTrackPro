using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Core.Models;
using Nest;

namespace MyApp.Core.Services
{
    public class ElasticSearchService
    {
        private readonly ElasticClient _client;
    private const string IndexName = "tasks";

    public ElasticSearchService()
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex(IndexName);
        _client = new ElasticClient(settings);
    }

    public async Task<List<TaskModel>> SearchTasks(string query)
    {
        var response = await _client.SearchAsync<TaskModel>(s => s
            .Index(IndexName)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Title)
                    .Query(query)
                )
            )
            .Highlight(h => h
                .Fields(f => f
                    .Field(p => p.Title)
                )
                .PreTags("<mark>")
                .PostTags("</mark>")
            )
        );

        var results = new List<TaskModel>();
        foreach (var hit in response.Hits)
        {
            var task = hit.Source;
            task.HighlightedTitle = hit.Highlight.ContainsKey("title") ? string.Join(" ", hit.Highlight["title"]) : task.Title;
            results.Add(task);
        }
        return results;
    }
    }
}