var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTraderBot(Secrets.ApiKey);
builder.Services.AddMvc();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.MapSwagger();
app.UseSwaggerUI();

await app.RunAsync();