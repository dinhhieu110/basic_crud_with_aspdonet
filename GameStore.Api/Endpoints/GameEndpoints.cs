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

        group.MapGet("/", async (GameStoreContext dbContext) => await dbContext.Games
        // Because list games is Id, cant map to GameSumMaryDTO
        .Include(game => game.Genre)
        .Select(game => game.ToGameSummaryDTO())
        .AsNoTracking()
        .ToListAsync()
        );

        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
            {
                Game? game = await dbContext.Games.FindAsync(id);
                return game is null
                ? Results.NotFound()
                : Results.Ok(game.ToGameDetailsDTO());
            }).WithName(GetGameEndPointName);

        group.MapPost("/", async (CreateGameDTO newGame, GameStoreContext dbContext) =>
        {
            // Create entity for server
            Game game = newGame.ToEntity();

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(GetGameEndPointName,
             new { id = game.Id }, game.ToGameDetailsDTO());
        });

        group.MapPut("/{id}", async (int id, UpdateGameDTO updatedGame, GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);
            if (existingGame is null)
            {
                return Results.NotFound();
            }
            dbContext.Entry(existingGame).CurrentValues.SetValues(updatedGame.ToEntity(id));
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
             .Where(game => game.Id == id)
             .ExecuteDeleteAsync();
            return Results.NoContent();
        });
        return group;
    }
}


