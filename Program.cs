using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Hubs;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.Momo;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services.AIservices;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;


var builder = WebApplication.CreateBuilder(args);

//Connect MomoAPI
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

//Connected sql
builder.Services.AddDbContext<DataContext>(options =>
{
	options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
});
//Add Email Sender
builder.Services.AddTransient<IEmailSender, EmailSender>();
// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.IsEssential = true;
});

//Khai bao Identity
builder.Services.AddIdentity<AppUserModel, IdentityRole>()
	.AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
	//options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	//options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	//options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
	options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
	options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
});

builder.Services.AddRazorPages();

builder.Services.Configure<IdentityOptions>(options =>
{
	// Password settings.
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequiredLength = 4;

	// User settings.
	options.User.RequireUniqueEmail = false;
});

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(
		builder =>
		{
			builder.WithOrigins("http://localhost:5172")
				.AllowAnyHeader()
				.WithMethods("GET", "POST")
				.AllowCredentials();
		});
});

//chatbot AI
builder.Services.AddSingleton<AIService>();


//Add SignalR
builder.Services.AddSignalR();

//Connect VNPay API
builder.Services.AddScoped<IVnPayService, VnPayService>();

var app = builder.Build();

//page 404 Error
app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

//session
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
// UseCors must be called before MapHub.
app.UseCors("CorsPolicy");
//Set Signal Hub


app.MapControllerRoute(
	name: "category",
	pattern: "/category/{Slug?}",
	defaults: new { controller = "Category", action = "Index" });

app.MapControllerRoute(
	name: "brand",
	pattern: "/brand/{Slug?}",
	defaults: new { controller = "Brand", action = "Index" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
	name: "Areas",
	pattern: "{areas:exists}/{controller=Product}/{action=Index}/{id?}");

//seeding data
var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedingData(context);


// Seed role và admin user (async)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUserModel>>();
    await SeedIdentityData.SeedRolesAndAdminAsync(roleManager, userManager);
}

// UseCors must be called before MapHub.
app.MapHub<ChatHub>("/Realtime/Index");

app.Run();
