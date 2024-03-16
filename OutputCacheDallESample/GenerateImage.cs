using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Text;
using System.Text.Json;
using Redis.OM;
using Redis.OM.Vectorizers;

namespace OutputCacheDallESample; 

public static class GenerateImage
{
    public static HttpClient client = new HttpClient();
    public const int AOAIDeploymentDimension = 10;

    public static async Task GenerateImageAsync(HttpContext context, string _prompt, IConfiguration _config, RedisConnectionProvider _provider )
    {
        //add semantic cache
        var cache = _provider.AzureOpenAISemanticCache(_config["apiKey"], _config["AOAIResourceName"], _config["AOAIEmbeddingDeploymentName"],AOAIDeploymentDimension);
        

        // Add custom headers
        client.DefaultRequestHeaders.Add("api-key", _config["apiKey"]);

        try
        {
            // Create a JSON payload
            var requestBody = new
            {
                // user defined prompt
                prompt = _prompt,
                size = "1024x1024",
                n = 1
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(_config["apiUrl"], content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
                var status = "";
                var location = "";
                foreach (var h in response.Headers)
                {
                    if (h.Key == "operation-location")
                    {
                        location = h.Value.First();
                    }
                }

                // retrieve image
                HttpResponseMessage imageResponse = await client.GetAsync(location);
                string imageResponseBody = await imageResponse.Content.ReadAsStringAsync();
                while (getStatus(imageResponseBody) != "succeeded")
                {
                    Thread.Sleep(1000);
                    imageResponse = await client.GetAsync(location);
                    imageResponseBody = await imageResponse.Content.ReadAsStringAsync();
                }
                string imageURL = retrieveImageURL(imageResponseBody);
                await context.Response.WriteAsync($"<img src=\"{imageURL}\"/>");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        
    }

    private static string getStatus(string imageResponseBody)
    {
        var jsonDocument = JsonDocument.Parse(imageResponseBody);
        var root = jsonDocument.RootElement;
        return root.GetProperty("status").GetString();

    }
    private static string retrieveImageURL(string imageResponseBody)
    {
        var jsonDocument = JsonDocument.Parse(imageResponseBody);
        var root = jsonDocument.RootElement;
        return root.GetProperty("result").GetProperty("data")[0].GetProperty("url").ToString();
    }
}
