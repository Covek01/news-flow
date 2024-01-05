using NewsFlowAPI.Models;

namespace NewsFlowAPI.Services
{
    public class NewsService : INewsService
    {
        private readonly string _trendingNewsKey = "trendingnews";

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
    }
}
