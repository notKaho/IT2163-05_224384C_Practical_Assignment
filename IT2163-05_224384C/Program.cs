using IT2163_05_224384C.Model;
using Microsoft.AspNetCore.Identity;
using AspNetCore.ReCaptcha;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddIdentity<Member, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(config =>
{
    config.LoginPath = "/login";
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);
});

builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "MyCookieAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeAMember",
        policy => policy.RequireClaim("Membership", "Member"));
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;

});

builder.Services.AddReCaptcha(builder.Configuration.GetSection("GoogleRecaptcha"));


var app = builder.Build();

CreateMemberRole(app.Services).Wait();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/Error/{0}");


app.MapRazorPages();

app.Run();


async Task CreateMemberRole(IServiceProvider serviceProvider)
{
	using (var scope = serviceProvider.CreateScope())
	{
		var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

		// Check if the "Member" role exists
		var memberRoleExists = await roleManager.RoleExistsAsync("Member");

		// If the role doesn't exist, create it
		if (!memberRoleExists)
		{
			var memberRole = new IdentityRole("Member");
			await roleManager.CreateAsync(memberRole);
		}
	}
}