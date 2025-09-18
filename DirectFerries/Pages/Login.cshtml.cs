using System.ComponentModel.DataAnnotations;
using DirectFerries.Models;
using DirectFerries.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace DirectFerries.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IApiService apiService, ILogger<LoginModel> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [BindProperty]
        [Required]
        public string Username { get; set; } = string.Empty;

        [BindProperty] 
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsLoading { get; set; }

        public IActionResult OnGet()
        {
            // Check if already logged in
            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                 return RedirectToPage("/Dashboard");
            }
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                IsLoading = true;
                _logger.LogInformation("Login attempt for username: {Username}", Username);

                var loginResponse = await _apiService.LoginAsync(Username, Password);

                if (loginResponse != null)
                {
                    // Store user data in session - firstname, lastname for the layout.cshtml welcome message, simplicity and secure in this scenario (razor pages, server side)
                    // if using our own database or federation would go with MS Identity, as this automatically uses and issues cookies/claims, use built in UserManager etc, JWT tokens another option,
                    // needs setup in Program.cs and use the [Authorize] attribute on pages/controllers
                    HttpContext.Session.SetString("FirstName", loginResponse.FirstName);
                    HttpContext.Session.SetString("LastName", loginResponse.LastName);
                    HttpContext.Session.SetString("Token", loginResponse.Token);
                    HttpContext.Session.SetString("User", JsonSerializer.Serialize(loginResponse));

                    _logger.LogInformation("Login successful for username: {Username}", Username);
                    return RedirectToPage("/Dashboard");
                }
                else
                {
                    ErrorMessage = "Invalid username or password. Please try again.";
                    _logger.LogWarning("Login failed for username: {Username}", Username);
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = "Connection error. Please check your internet connection and try again.";
                _logger.LogError(ex, "Network error during login for username: {Username}", Username);
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred. Please try again.";
                _logger.LogError(ex, "Unexpected error during login for username: {Username}", Username);
            }
            finally
            {
                IsLoading = false;
            }

            return Page();
        }
    }
}