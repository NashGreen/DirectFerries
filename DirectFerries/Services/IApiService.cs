using DirectFerries.Models;

namespace DirectFerries.Services
{
    public interface IApiService
    {
        Task<User?> LoginAsync(string username, string password);

        Task<List<Product>> GetSmartphonesAsync(string token);

        Task<Product?> UpdateProductPriceAsync(int productId, decimal newPrice, string token);

    }
}
