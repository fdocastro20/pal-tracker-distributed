using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Headers;

namespace Backlog
{
    public class ProjectClient : IProjectClient
    {
        private Dictionary<long, ProjectInfo> _projectCache = new Dictionary<long, ProjectInfo>();
        private readonly HttpClient _client;
        private readonly ILogger<ProjectClient> _logger;
        private Func<Task<string>> _accessTokenFn;

        public ProjectClient(HttpClient client, ILogger<ProjectClient> logger, Func<Task<string>> accessToken)
        {
            _client = client;
            _logger = logger;
            _accessTokenFn = accessToken;
        }

        public async Task<ProjectInfo> Get(long projectId)
        {
            return await new GetProjectCommand(DoGet, DoGetFromCache, projectId).ExecuteAsync();
        }

        public async Task<ProjectInfo> DoGet(long projectId)
        {
            var token = await _accessTokenFn();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var streamTask = _client.GetStreamAsync($"project?projectId={projectId}");

            _logger.LogInformation($"Attempting to fetch projectId: {projectId}");
            var serializer = new DataContractJsonSerializer(typeof(ProjectInfo));
            ProjectInfo pi = (ProjectInfo)serializer.ReadObject(await streamTask);

            _projectCache.Add(projectId, pi);
            _logger.LogInformation($"Caching projectId: {projectId}");

            return pi;
        }

        public Task<ProjectInfo> DoGetFromCache(long projectId)
        {
            _logger.LogInformation($"Retrieving from cache projectId: {projectId}");
            return Task.FromResult(_projectCache[projectId]);
        }
    }
}