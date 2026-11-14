namespace HastaneRandevu.Data.Repositories
{
    /// <summary>
    /// Generic Repository arayüzü.
    /// OOP: Interface (Soyutlama) + Generic (Tip Güvenliği) örneği.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> TumunuGetirAsync();
        Task<T?> IdIleGetirAsync(int id);
        Task<T> EkleAsync(T entity);
        Task GuncelleAsync(T entity);
        Task SilAsync(int id);
        Task<bool> VarMiAsync(int id);
        Task<int> KaydetAsync();
    }
}
