using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class ApiCallRoutine
{
    private HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    public string _accessToken;
    private string HostUrl = "";
   
    public ApiCallRoutine(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        HostUrl= _configuration.GetValue("AppSettings:HostURL", "");
    }

    public string GetAPI(string controller, string methodName, IDictionary<string, string> headers = null, IDictionary<string, string> queryParams = null)
    {
        _httpClient = new HttpClient();
        // Refresh token if needed
        if (string.IsNullOrEmpty(_accessToken))
        {
            RefreshTokenAsync();
        }

        // Set the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        
        var url = $"{HostUrl}{controller}/{methodName}";

        if (queryParams != null && queryParams.Any())
        {
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            url = $"{url}?{queryString}";
        }

        // Add headers if provided
        if (headers != null)
        {
            foreach (var header in headers)
            {
                if(!_httpClient.DefaultRequestHeaders.Any(x=>x.Key== header.Key))
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);

                }
            }
        }

        // Make the GET request
        var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
        
        var jsonString = response.Content.ReadAsStringAsync().Result;
        return jsonString;
    }

    public string PostAPI(
    string controller,
    string methodName,
    object body,
    IDictionary<string, string> headers = null,
    IDictionary<string, string> queryParams = null,
    IFormFileCollection files = null)
    {
        // Refresh token if needed
        if (string.IsNullOrEmpty(_accessToken))
        {
            RefreshTokenAsync();
        }

        // Set the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var urlBuilder = new StringBuilder($"{HostUrl}{controller}/{methodName}");

        if (queryParams != null && queryParams.Any())
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var param in queryParams)
            {
                query[param.Key] = param.Value;
            }
            urlBuilder.Append($"?{query}");
        }

        // Create HttpRequestMessage to avoid potential threading issues
        var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder.ToString());

        // Add headers if provided
        if (headers != null)
        {
            foreach (var header in headers)
            {
                if (!request.Headers.Contains(header.Key))
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
        }

        // Determine if the request should include files
        if (files != null && files.Any())
        {
            // Use MultipartFormDataContent for file uploads
            var multipartContent = new MultipartFormDataContent();

            // Add files to the multipart content
            foreach (var file in files)
            {
                var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                multipartContent.Add(fileContent, "_documents", file.FileName); // Match the server-side parameter name
            }

            // Add JSON body if provided
            if (body != null&&body!="")
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                multipartContent.Add(jsonContent, "data");
            }

            request.Content = multipartContent;
        }

        else
        {
            // Use JSON content if no files are provided
            if (body != null)
            {
                request.Content = new StringContent(body.ToString(), Encoding.Default, "application/json");
            }
        }

        try
        {
            // Make the POST request
            var response = _httpClient.Send(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = response.Content.ReadAsStringAsync().Result;
                return jsonString;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                return "";
            }
        }
        catch (HttpRequestException e)
        {
            // Handle request exception
            Console.WriteLine($"Request error: {e.Message}");
            throw;
        }
    }

    public void RefreshTokenAsync()
    {

        var tokenEndpoint = HostUrl+_configuration.GetValue("AppSettings:TokenEndpoint", "");
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);

        try
        {
            // Send the POST request
            HttpResponseMessage res =  _httpClient.Send(request);

            // Ensure the request was successful, and throw an exception if it wasn't
            res.EnsureSuccessStatusCode();

            // Read and display the response body if necessary
            string responseBody = res.Content.ReadAsStringAsync().Result;

            _accessToken = responseBody;
            
           
        }
        catch (HttpRequestException e)
        {
            // Handle any errors that may have occurred
            Console.WriteLine($"Request error: {e.Message}");
        }
    }

    private class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
