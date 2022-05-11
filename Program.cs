using Neve.Server.Hubs;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddTransient<MySqlConnection>(_=>new MySqlConnection(configuration["ConnectionStrings:Default"]));



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


app.MapRazorPages();
app.MapHub<NeveHub>("/nevehub");


app.Run();
