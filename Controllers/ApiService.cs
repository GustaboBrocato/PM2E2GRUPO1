using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PM2E2GRUPO1.Controllers
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        //private string apiURL = "https://pm2e2grupo1aspcore20240315131853.azurewebsites.net"; //Azure
        private string apiURL = "https://aspnetclusters-166055-0.cloudclusters.net"; //Cloudclusters

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<T> GetDataAsync<T>(string endpoint)
        {
            try
            {
                string url = $"{apiURL}{endpoint}";
                var response = await _httpClient.GetStringAsync(url);
                return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                return default;
            }
        }

        public async Task<TResponse> PostDataAsync<TResponse>(string endpoint, object data)
        {
            try
            {
                string url = $"{apiURL}{endpoint}";
                var jsonData = JsonSerializer.Serialize(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostDataAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PostSuccessAsync(string endpoint, object data)
        {
            try
            {
                string url = $"{apiURL}{endpoint}";
                var jsonData = JsonSerializer.Serialize(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostSuccessAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDataAsync(string endpoint, int id)
        {
            try
            {
                string url = $"{apiURL}{endpoint}/{id}";

                var response = await _httpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();

                // If the response status code is 204 (No Content), return true
                return response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error in DeleteDataAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateDataAsync(string endpoint, int id, object data)
        {
            try
            {
                string url = $"{apiURL}{endpoint}/{id}";
                var jsonData = JsonSerializer.Serialize(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();

                // Si la respuesta es buena, regresa true
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateDataAsync: {ex.Message}");
                return false;
            }
        }
    }
}
