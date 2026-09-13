using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("EcommerceAPI");
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
        {

            if(!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }


            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode) return default;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }


        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, string? token = null)
        {
            // C# object ko JSON string mein convert karo (jo API ko chahiye)
            var json = JsonSerializer.Serialize(data);

            // JSON ko HTTP request body ke format mein wrap karo
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Agar token diya gaya hai (matlab user login hai), toh use header mein add karo
            // Isse protected endpoints (jaise Add to Cart) bhi call kar sakenge
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // POST request bhejo
            var response = await _httpClient.PostAsync(endpoint, content);

            // Response body nikaalo (chahe success ho ya error - dono cases mein useful info ho sakti hai)
            var responseJson = await response.Content.ReadAsStringAsync();

            // Agar request fail hui(400, 401, 500 etc.)
            if (!response.IsSuccessStatusCode)
                // Exception throw karo taaki controller isse "catch" karke user ko error dikha sake
                throw new Exception($"Status: {response.StatusCode} | Body: {responseJson}");

            // Success - response ko TResponse type mein convert karke wapas do
            return JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        // Generic PUT method - kisi existing record ko UPDATE karne ke liye
        public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data, string? token = null)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if(!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
           new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // PUT request bhejo - "update karo" wala HTTP method
            var response = await _httpClient.PutAsync(endpoint, content);

            // true/false return kar rahe hain - "successful hua ya nahi", data return karne ki zaroorat nahi
            // (kyunki PUT/DELETE mein API sirf "204 No Content" bhejta hai, koi body nahi)
            return response.IsSuccessStatusCode;
        }

        // Generic DELETE method - kisi record ko delete karne ke liye
        public async Task<bool> DeleteAsync(string endpoint, string? token = null)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
    }
}
