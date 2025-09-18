using DirectFerries.Models;
using DirectFerries.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace DirectFerries.Pages;

//Would ideally use ms identity for authentication if using database or federation. Using session for simplicity here
//Add the authorization set up in program.cs to use cookie authentication scheme so the cookie automatically gets added when using the built in UserManager loginAsync 
//[Authorize]
public class DashboardModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(IApiService apiService, ILogger<DashboardModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public List<Product>? Smartphones { get; set; }
    public LoginResponse? CurrentUser { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;
    public bool IsLoading { get; set; }
    public bool IsUpdating { get; set; }

    [BindProperty]
    public decimal Percentage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = HttpContext.Session.GetString("Token");
        var userJson = HttpContext.Session.GetString("User");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userJson))
        {
            return RedirectToPage("/Login");
        }

        try
        {
            CurrentUser = JsonSerializer.Deserialize<LoginResponse>(userJson);
            IsLoading = true;

            _logger.LogInformation("Loading smartphones for user: {Username}", CurrentUser?.Username);
            Smartphones = await _apiService.GetSmartphonesAsync(token);

            if (Smartphones?.Any() != true)
            {
                ErrorMessage = "No smartphones found or unable to load data.";
                _logger.LogWarning("No smartphones found for user: {Username}", CurrentUser?.Username);
            }
            else
            {
                _logger.LogInformation("Loaded {Count} smartphones for user: {Username}", Smartphones.Count, CurrentUser?.Username);
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = "Connection error. Please check your internet connection.";
            _logger.LogError(ex, "Network error loading smartphones for user: {Username}", CurrentUser?.Username);
        }
        catch (Exception ex)
        {
            ErrorMessage = "An unexpected error occurred while loading data.";
            _logger.LogError(ex, "Unexpected error loading smartphones for user: {Username}", CurrentUser?.Username);
        }
        finally
        {
            IsLoading = false;
        }

        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        var token = HttpContext.Session.GetString("Token");
        var userJson = HttpContext.Session.GetString("User");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userJson))
        {
            return RedirectToPage("/Login");
        }
       
        try
        {
            CurrentUser = JsonSerializer.Deserialize<LoginResponse>(userJson);

            if (Percentage < 0 || Percentage > 100)
            {
                ErrorMessage = "Percentage must be between 0 and 100.";
                await LoadSmartphones(token);
                return Page();
            }

            IsUpdating = true;
            _logger.LogInformation("User {Username} updating prices by {Percentage}%", CurrentUser?.Username, Percentage);

            // Load current smartphones
            await LoadSmartphones(token);

            if (Smartphones?.Any() != true)
            {
                ErrorMessage = "No smartphones to update.";
                return Page();
            }

            // Update each smartphone price concurrently - try catch, so if one fails the others continue
            var updateTasks = Smartphones.Select(async phone =>
            {
                try
                {
                    var newPrice = phone.Price * (1 + Percentage / 100);
                    var updatedProduct = await _apiService.UpdateProductPriceAsync(phone.Id, newPrice, token);
                    return new { phone, updatedProduct };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update product {PhoneId}", phone.Id);
                    return new { phone, updatedProduct = (Product?)null };
                }
            });

            var results = await Task.WhenAll(updateTasks);
            var updatedCount = 0;

            foreach (var result in results)
            {
                if (result.updatedProduct != null)
                {
                    result.phone.Price = result.updatedProduct.Price;
                    updatedCount++;
                    _logger.LogInformation("Updated product {PhoneId} price to {Price}", result.phone.Id, result.updatedProduct.Price);
                }
            }

            if (updatedCount > 0)
            {
                SuccessMessage = $"Successfully updated {updatedCount} smartphone prices by {Percentage}%.";
                _logger.LogInformation("Successfully updated {UpdatedCount} prices for user: {Username}", updatedCount, CurrentUser?.Username);
            }
            else
            {
                ErrorMessage = "Failed to update any prices. Please try again.";
                _logger.LogWarning("Failed to update any prices for user: {Username}", CurrentUser?.Username);
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = "Connection error during price update. Please try again.";
            _logger.LogError(ex, "Network error updating prices for user: {Username}", CurrentUser?.Username);
        }
        catch (Exception ex)
        {
            ErrorMessage = "An unexpected error occurred during price update.";
            _logger.LogError(ex, "Unexpected error updating prices for user: {Username}", CurrentUser?.Username);
        }
        finally
        {
            IsUpdating = false;
        }

        return Page();
    }

    private async Task LoadSmartphones(string token)
    {
        try
        {
            Smartphones = await _apiService.GetSmartphonesAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading smartphones");
            Smartphones = new List<Product>();
        }
    }
}
