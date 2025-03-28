using System;
using GameStore.Api.Data;
using GameStore.Api.DTOs;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GameEndpoints
{
    const string GetGameEndPointName = "GetSpecificGame";

    public static RouteGroupBuilder MapGameEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("games").WithParameterValidation();

        group.MapGet("/", (GameStoreContext dbContext) => dbContext.Games
        // Because list games is Id, cant map to GameSumMaryDTO
        .Include(game => game.Genre)
        .Select(game => game.ToGameSummaryDTO())
        .AsNoTracking()
        );

        group.MapGet("/{id}", (int id, GameStoreContext dbContext) =>
            {
                Game? game = dbContext.Games.Find(id);
                return game is null
                ? Results.NotFound()
                : Results.Ok(game.ToGameDetailsDTO());
            }).WithName(GetGameEndPointName);

        group.MapPost("/", (CreateGameDTO newGame, GameStoreContext dbContext) =>
        {
            // Create entity for server
            Game game = newGame.ToEntity();

            dbContext.Games.Add(game);
            dbContext.SaveChanges();

            return Results.CreatedAtRoute(GetGameEndPointName,
             new { id = game.Id }, game.ToGameDetailsDTO());
        });

        group.MapPut("/{id}", (int id, UpdateGameDTO updatedGame, GameStoreContext dbContext) =>
        {
            var existingGame = dbContext.Games.Find(id);
            if (existingGame is null)
            {
                return Results.NotFound();
            }
            dbContext.Entry(existingGame).CurrentValues.SetValues(updatedGame.ToEntity(id));
            dbContext.SaveChanges();
            return Results.NoContent();
        });

        group.MapDelete("/{id}", (int id, GameStoreContext dbContext) =>
        {
            dbContext.Games
            .Where(game => game.Id == id)
            .ExecuteDelete();
            return Results.NoContent();
        });
        return group;
    }
}


