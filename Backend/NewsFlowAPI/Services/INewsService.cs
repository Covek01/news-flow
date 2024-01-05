using Microsoft.AspNetCore.Mvc;
using NewsFlowAPI.Models;

namespace NewsFlowAPI.Services
{
    public interface INewsService
    {
        public Task InsertNews(string title, string summary, string text, string imgUrl, int idAuthor, Location? location);
        public Task DeleteNews(int idNews);
        public Task<List<News>> GetNewsByAuthor(int idAuthor);
        public Task<List<News>> GetNewsByTags(List<Tag> tags);
        public Task<List<News>> GetTrendingNews();
    }
}
