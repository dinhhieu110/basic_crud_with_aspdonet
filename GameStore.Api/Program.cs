using GameStore.Api.DTOs;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

List<GameDTO> games = [
     new (1, "League Of Legends", "ESport", 69.99M, new DateOnly(2025,3,27)),
     new (2, "CF", "Fighting", 50.99M, new DateOnly(2025,3,27)),
     new (3, "Zing Speed", "Racing", 40.99M, new DateOnly(2025,3,27)),
];

app.MapGet("/games", () => games);
app.MapGet("/games/{id}", (int id) => games.Find(game=>game.Id == id));


app.Run();
