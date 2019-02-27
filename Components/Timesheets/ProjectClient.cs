using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Timesheets
{
    public class ProjectClient : IProjectClient
    {
        private Dictionary<long, ProjectInfo> _projectCache = new Dictionary<long, ProjectInfo>();
        private readonly HttpClient _client;
        private readonly ILogger<ProjectClient> _logger;

        public ProjectClient(HttpClient client, ILogger<ProjectClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ProjectInfo> Get(long projectId)
        {
            return await new GetProjectCommand(DoGet, DoGetFromCache, projectId).ExecuteAsync();
        }

        public async Task<ProjectInfo> DoGet(long projectId)
        {
            _client.DefaultRequestHeaders.Accept.Clear();
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