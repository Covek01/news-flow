using Neo4jClient;
using NewsFlowAPI.DTOs;
using NewsFlowAPI.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NewsFlowAPI.Services
{
    public class NewsService : INewsService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBoltGraphClient _neo4j;

        public NewsService(
            IConnectionMultiplexer redis,
            IBoltGraphClient neo4j)
        {
            _neo4j = neo4j;
            _redis = redis;
        }

        private readonly string _newestNewsKey = "newestnews";

        public Task DeleteNews(int idNews)
        {
            throw new NotImplementedException();
        }

        public Task<List<News>> GetNewsByAuthor(int idAuthor)
        {
            throw new NotImplementedException();
        }

        public Task<List<News>> GetNewsByTags(List<Tag> tags)
        {
            throw new NotImplementedException();
        }

        public Task<List<News>> GetTrendingNews()
        {
            throw new NotImplementedException();
        }

        public Task InsertNews(string title, string summary, string text, string imgUrl, int idAuthor, Location? location)
        {
            throw new NotImplementedException();
        }

        public async Task<List<News>> GetNewsUnfiltered()
        {
            var db = _redis.GetDatabase();

            var news = await db.ListRangeAsync(_newestNewsKey);
            var newsDeserialized = new List<News>();

            foreach (var n in news)
            {
                newsDeserialized.Add(JsonConvert.DeserializeObject<News>(n));
            }

            var newsReturnTasks = newsDeserialized.Select(async p =>
            {

                var author = (await _neo4j.Cypher
                                .Match("(u:User)")
                                .Where((User u) => u.Id == p.AuthorId)
                                .Return(u => u.As<User>().Name)
                                .ResultsAsync).FirstOrDefault();

                return new NewsReturnDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Text = p.Text,
                    Summary = p.Summary,
                    ImageUrl = p.ImageUrl,
                    locationId = p.LocationId,
                    ViewsCount = p.ViewsCount,
                    LikeCount = p.LikeCount,
                    authorId = p.AuthorId,
                    PostTime = p.PostTime,
                    authorName = author
                };
            });

            var newsReturnResults = await Task.WhenAll(newsReturnTasks);

            return null;
        }

        public async Task<List<News>> GetNewsFiltered(List<long> tagIds, long locationId, long writerId)
        {
            var db = _redis.GetDatabase();

            var news = await db.ListRangeAsync(_newestNewsKey);
            var newsDeserialized = new List<News>();

            foreach (var n in news)
            {
                newsDeserialized.Add(JsonConvert.DeserializeObject<News>(n));
            }

            var newsReturnTasks = newsDeserialized.Select(async p =>
            {

                var author = (await _neo4j.Cypher
                                .Match("(u:User)")
                                .Where((User u) => u.Id == p.AuthorId)
                                .Return(u => u.As<User>().Name)
                                .ResultsAsync).FirstOrDefault();

                return new NewsReturnDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Text = p.Text,
                    Summary = p.Summary,
                    ImageUrl = p.ImageUrl,
                    locationId = p.LocationId,
                    ViewsCount = p.ViewsCount,
                    LikeCount = p.LikeCount,
                    authorId = p.AuthorId,
                    PostTime = p.PostTime,
                    authorName = author
                };
            });

            var newsReturnResults = await Task.WhenAll(newsReturnTasks);

            return null;
        }
    }
}
