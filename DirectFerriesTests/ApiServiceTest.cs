using DirectFerries.Models;
using DirectFerries.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DirectFerriesTests
{
    public class ApiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<ApiService>> _loggerMock;
        private readonly ApiService _apiService;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _loggerMock = new Mock<ILogger<ApiService>>();
            _apiService = new ApiService(_httpClient, _loggerMock.Object);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Fact]
        public async Task LoginAsync_SuccessfulLogin_ReturnsUser()
        {
            // Arrange
            var user = new User { Id = 1, Username = "testuser", Token = "test-token" };
            var responseContent = JsonSerializer.Serialize(user, _jsonOptions);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _apiService.LoginAsync("testuser", "password");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test-token", result.Token);
        }

        [Fact]
        public async Task LoginAsync_FailedLogin_ReturnsNull()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("Unauthorized", Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _apiService.LoginAsync("testuser", "wrongpassword");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_HttpException_ThrowsException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _apiService.LoginAsync("testuser", "password"));
        }

        [Fact]
        public async Task GetSmartphonesAsync_SuccessfulResponse_ReturnsTop3Smartphones()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "iPhone", Price = 1000, Category = "smartphones" },
                new Product { Id = 2, Title = "Samsung", Price = 800, Category = "smartphones" },
                new Product { Id = 3, Title = "Google Pixel", Price = 900, Category = "smartphones" },
                new Product { Id = 4, Title = "OnePlus", Price = 700, Category = "smartphones" }
            };
            var productsResponse = new ProductsResponse { Products = products };
            var responseContent = JsonSerializer.Serialize(productsResponse, _jsonOptions);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _apiService.GetSmartphonesAsync("test-token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(1000, result[0].Price); // Highest price first
            Assert.Equal(900, result[1].Price);
            Assert.Equal(800, result[2].Price);
        }

        [Fact]
        public async Task GetSmartphonesAsync_EmptyResponse_ReturnsEmptyList()
        {
            // Arrange
            var productsResponse = new ProductsResponse { Products = new List<Product>() };
            var responseContent = JsonSerializer.Serialize(productsResponse, _jsonOptions);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _apiService.GetSmartphonesAsync("test-token");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSmartphonesAsync_FailedResponse_ReturnsEmptyList()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad Request", Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _apiService.GetSmartphonesAsync("test-token");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateProductPriceAsync_SuccessfulUpdate_ReturnsUpdatedProduct()
        {
            // Arrange
            var updatedProduct = new Product { Id = 1, Title = "iPhone", Price = 1200, Category = "smartphones" };
            var responseContent = JsonSerializer.Serialize(updatedProduct, _jsonOptions);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _apiService.UpdateProductPriceAsync(1, 1200m, "test-token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1200m, result.Price);
        }

        [Fact]
        public async Task UpdateProductPriceAsync_FailedUpdate_ReturnsNull()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Product not found", Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _apiService.UpdateProductPriceAsync(999, 1200m, "test-token");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProductPriceAsync_HttpException_ThrowsException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _apiService.UpdateProductPriceAsync(1, 1200m, "test-token"));
        }
    }
}