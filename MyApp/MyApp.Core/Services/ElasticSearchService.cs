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
        private readonly IElasticClient _elasticClient;

        public ElasticSearchService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task IndexTask(t_task task)
        {
            var response = await _elasticClient.IndexDocumentAsync(task);
        }

        public async Task<List<t_task>> SearchTasks(string query, string status = null)
        {
            var searchResponse = await _elasticClient.SearchAsync<t_task>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            q.Match(m => m.Field(f => f.c_title).Query(query)),  // Search by title
                            q.Match(m => m.Field(f => f.c_description).Query(query)) // Search by description
                        )
                        .Filter(f => status != null ? f.Term(t => t.Field(f => f.c_status).Value(status)) : null) // Filter by status
                    )
                )
            );

            return searchResponse.Documents.ToList();
        }
    }
}