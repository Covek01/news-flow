namespace NewsFlow.Server.Services
{
    public interface IIdentifierService
    {
        Task<long> IdNext(string node);
        Task<long> UserNext();
        Task<long> NewsNext();
        Task<long> TagNext();
        Task<long> CommentNext();
        Task<long> LocationNext();
    }
}
