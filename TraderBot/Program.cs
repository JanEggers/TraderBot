var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTraderBot(Secrets.ApiKey);
builder.Services.AddMvc();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.MapRazorPages();
app.MapStaticAssets();
app.MapSwagger();
app.UseSwaggerUI();

await app.RunAsync();