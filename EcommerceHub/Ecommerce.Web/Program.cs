


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Named HttpClient banate hain jo hamesha API ka base address use karega
builder.Services.AddHttpClient("EcommerceAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7045/");   // apna API ka exact port daalna
});
builder.Services.AddScoped<Ecommerce.Web.Services.ApiService>();

// Session use karne ke liye ye 2 services chahiye
// IHttpContextAccessor - current request ki info access karne ke liye
builder.Services.AddHttpContextAccessor();

// Session ko memory mein store karne ke liye (development ke liye theek hai)
builder.Services.AddDistributedMemoryCache();

// Session ki settings - kitni der tak yaad rakhega, cookie kaisi banegi
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);   // 60 min tak inactive rehne pe bhi login yaad rahega
    options.Cookie.HttpOnly = true;                     // JS se cookie access nahi ho sakti - security ke liye
    options.Cookie.IsEssential = true;                  // GDPR jaisi cookie-consent policies ke liye zaroori
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();   // Session middleware activate karo - ye zaroori hai varna session kaam nahi karega

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
