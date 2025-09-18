using DirectFerries.Models;
using System.Text;
using System.Text.Json;
using System.Web;

namespace DirectFerries.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string BaseUrl = "https://dummyjson.com";

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// LoginAsync - authenticates user and returns user details including token
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>uesr details and access token</returns>
        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username and password cannot be null or empty");
            }

            var sanitizedUsername = System.Web.HttpUtility.HtmlEncode(username);
            var sanitizedPassword = System.Web.HttpUtility.HtmlEncode(password);

            try
            {
                _logger.LogInformation("Attempting login for user: {Username}", sanitizedUsername);

                var loginRequest = new Models.LoginRequest { Username = sanitizedUsername, Password = sanitizedPassword };
                var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(BaseUrl + "/auth/login", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Login API response status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<User>(responseContent, _jsonOptions);
                    _logger.LogInformation("Login successful");
                    return loginResponse;
                }

                _logger.LogWarning("Login failed. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", sanitizedUsername);
                throw;
            }
        }

        /// <summary>
        /// GetSmartphonesAsync - fetches products in smartphones category, sorts by price desc and returns top 3
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>List of products/returns>
        public async Task<List<Product>> GetSmartphonesAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be null or empty");
            }
            
            try
            {
                _logger.LogInformation("Fetching smartphones with token");

                //retry logic for errors - haven't done it on all calls - proof of concept
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        //get all products in smartphones category
                        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/auth/products/category/smartphones");
                        request.Headers.Add("Authorization", $"Bearer {token}");
                        var response = await _httpClient.SendAsync(request);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        _logger.LogInformation("Products API response status: {StatusCode}", response.StatusCode);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var productsResponse = JsonSerializer.Deserialize<ProductsResponse>(responseContent, _jsonOptions);

                            //belt and braces on category, filtering, sort by price desc and take top 3
                            var smartphones = productsResponse?.Products
                                .Where(p => p.Category.Equals("smartphones", StringComparison.OrdinalIgnoreCase))
                                .OrderByDescending(p => p.Price)
                                .Take(3)
                                .ToList() ?? new List<Product>();

                            _logger.LogInformation("Found {Count} smartphones", smartphones.Count);
                            return smartphones;
                        }

                        _logger.LogWarning("Failed to fetch products. Status: {StatusCode}", response.StatusCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Network error fetching smartphones - try again!");
                        if (i == 2) throw; // throw after final attempt
                    }
                }
 
                return new List<Product>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching smartphones");
                throw;
            }
        }

        /// <summary>
        /// UpdateProductPriceAsync - updates product price by productId
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="newPrice"></param>
        /// <param name="token"></param>
        /// <returns>product</returns>
        public async Task<Product?> UpdateProductPriceAsync(int productId, decimal newPrice, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be null or empty");
            }

            if (productId < 1)
            {
                throw new ArgumentException("Product Id must be greater than 0");
            }

            if (newPrice < 0)
            {
                throw new ArgumentException("Price cannot be negative");
            }

            try
            {
                _logger.LogInformation("Updating product {ProductId} price to {NewPrice}", productId, newPrice);

                var updateRequest = new ProductUpdateRequest { Price = newPrice };
                var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Put, BaseUrl + $"/auth/products/{productId}");
                request.Headers.Add("Authorization", $"Bearer {token}");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Update product API response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var updatedProduct = JsonSerializer.Deserialize<Product>(responseContent, _jsonOptions);
                    _logger.LogInformation("Successfully updated product");
                    return updatedProduct;
                }

                _logger.LogWarning("Failed to update product {ProductId}. Status: {StatusCode}", productId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product for {ProductId}", productId);
                throw;
            }
        }
    }
}
