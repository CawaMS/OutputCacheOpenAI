using Microsoft.AspNetCore.OutputCaching;
using OutputCacheDallESample;

var builder = WebApplication.CreateBuilder(args);

// add services
builder.Services.AddOutputCache(options => {
    // optional: named output-cache profiles
});
builder.Services.AddStackExchangeRedisOutputCache(options => {
    
    options.Configuration = "Your_Redis_Cache_ConnectionString";
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("/nocache", GenerateImage.generateImage);
app.MapGet("/cached", GenerateImage.generateImage).CacheOutput();

app.UseOutputCache();
app.Run();
