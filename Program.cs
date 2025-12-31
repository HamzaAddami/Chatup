<<<<<<< HEAD
﻿using System.Text;
=======
using System.Text;
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
using Chatup.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
<<<<<<< HEAD
using sib_api_v3_sdk.Client;
=======
using Scalar.AspNetCore;
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

<<<<<<< HEAD
var mongoClient = new MongoClient(builder.Configuration.GetConnectionString("MongoDb"));
builder.Services.AddSingleton<IMongoDatabase>(mongoClient.GetDatabase("chatup"));


var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// Brevo
Configuration.Default.ApiKey.Add("api-key", builder.Configuration["Brevo:ApiKey"]);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173") 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});

=======
var mongoClient = new MongoClient(builder.Configuration.GetConnectionString("MongoDB"));
builder.Services.AddSingleton<IMongoDatabase>(mongoClient.GetDatabase("chatup"));

var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

builder.Services.AddScoped<AuthService>();
builder.Services.AddControllers();
builder.Services.AddScoped<UserService>();
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"]
        };
<<<<<<< HEAD

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ConversationService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddControllers();
builder.Services.AddSignalR();


=======
    });

>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapScalarApiReference(options =>
    {
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });
}

<<<<<<< HEAD
app.UseCors("CorsPolicy");
// app.UseHttpsRedirection();
=======
app.UseHttpsRedirection();
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

<<<<<<< HEAD
app.MapHub<Chatup.Hubs.ChatHub>("/hubs/chat");



app.Run();
=======
app.Run();
>>>>>>> fc15b791a7b488ee839afa2b10116487d7d9f5be
