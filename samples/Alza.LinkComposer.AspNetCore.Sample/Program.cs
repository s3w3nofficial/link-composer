using Alza.LinkComposer.AspNetCore.Sample.Services;
using Alza.LinkComposer.Configuration;
using Alza.LinkComposer.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApiVersioning();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ILinkComposer, Alza.LinkComposer.AspNetCore.LinkComposer>();
builder.Services.AddSingleton<ILinkComposerBaseUriFactory, LinkComposerBaseUriFactory>();

builder.Services.Configure<LinkComposerSettings>(builder.Configuration.GetSection("LinkComposerSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
