using System;
using GameStore.Api.Data;
using GameStore.Api.DTOs;
using GameStore.Api.Entities;

namespace GameStore.Api.Endpoints;

public static class GameEndpoints
{
    const string GetGameEndPointName = "GetSpecificGame";

    private static readonly List<GameDTO> games = [
         new (1, "League Of Legends", "ESport", 69.99M, new DateOnly(2025,3,27)),
     new (2, "CF", "Fighting", 50.99M, new DateOnly(2025,3,27)),
     new (3, "Zing Speed", "Racing", 40.99M, new DateOnly(2025,3,27)),
];

    public static RouteGroupBuilder MapGameEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("games").WithParameterValidation();

        group.MapGet("/", () => games);

        group.MapGet("/{id}", (int id) =>
            {
                GameDTO? game = games.Find(game => game.Id == id);
                return game is null ? Results.NotFound() : Results.Ok(game);
            }).WithName(GetGameEndPointName);

        group.MapPost("/", (CreateGameDTO newGame, GameStoreContext dbContext) =>
        {
            // GameDTO game = new(
            //     games.Count + 1,
            //     newGame.Name,
            //     newGame.Genre,
            //     newGame.Price,
            //     newGame.ReleaseDate
            //     );
            Game game = new()
            {
                Name = newGame.Name,
                Genre = dbContext.Genres.Find(newGame.GenreId),
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };

            dbContext.Games.Add(game);
            dbContext.SaveChanges();

            GameDTO gameToUI = new(
                game.Id,
                game.Name,
                game.Genre!.Name,
                game.Price,
                game.ReleaseDate
            );

            return Results.CreatedAtRoute(GetGameEndPointName, new { id = game.Id }, gameToUI);
        });

        group.MapPut("/{id}", (int id, UpdateGameDTO updatedGame) =>
        {
            var i = games.FindIndex(game => game.Id == id);
            if (i == -1)
            {
                return Results.NotFound();
            }
            games[i] = new GameDTO(
                id,
                updatedGame.Name,
                updatedGame.Genre,
                updatedGame.Price,
                updatedGame.ReleaseDate
            );
            return Results.NoContent();
        });

        group.MapDelete("/{id}", (int id) =>
        {
            var removedCount = games.RemoveAll(game => game.Id == id);
            if (removedCount == 0)
            {
                return Results.NotFound();
            }
            return Results.NoContent();
        });
        return group;
    }
}


