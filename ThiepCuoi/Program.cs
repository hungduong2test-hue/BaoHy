using ThiepCuoi.Pages;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("Config/00-loai-thiep.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Config/01-thong-tin-trang-dau.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Config/02-thong-tin-tiec-cuoi.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Config/03-dia-diem-tiec-cuoi.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Config/04-thong-tin-le-cuoi.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Config/05-hop-mung-cuoi.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Config/06-cau-hinh-hieu-ung.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<WeddingPageData>(builder.Configuration.GetSection("Wedding"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
