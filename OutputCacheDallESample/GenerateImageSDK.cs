using Azure.AI.OpenAI;
using Azure;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using OpenAI.Images;

namespace OutputCacheDallESample
{
    public static class GenerateImageSDK
    {
        public static async Task GenerateImageSDKAsync(HttpContext context, string _prompt, IConfiguration _config)
        {
            string endpoint = _config["AZURE_OPENAI_ENDPOINT"];
            string key = _config["apiKey"];

            AzureOpenAIClient client = new(new Uri(endpoint), new AzureKeyCredential(key));

            ImageClient imageClient = client.GetImageClient("dall-e-3");
            GeneratedImage generatedImage = await imageClient.GenerateImageAsync(_prompt, new ImageGenerationOptions()
            {
                Size = GeneratedImageSize.W1024xH1024
            });

            // Image Generations responses provide URLs you can use to retrieve requested images
            string imageURL = generatedImage.ImageUri.AbsoluteUri;

            await context.Response.WriteAsync("<!DOCTYPE html><html><body> " +
            $"<img src=\"{imageURL}\" alt=\"Flowers in Chania\" width=\"460\" height=\"345\">" +
            " </body> </html>");
        }
    }
}
