using Arch.Core;
using Arch.Core.Extensions;
using CookingProject.Logic.Components;
using CookingProject.Logic.Events;

namespace CookingProject.Logic.Systems;

/// <summary>
/// System that handles cooking logic including temperature changes,
/// cooking progress, and burning detection.
/// </summary>
public class CookingSystem : IGameSystem
{
    private readonly World _world;
    private readonly GameFacade _facade;
    private readonly QueryDescription _cookingQuery;

    // Cooking constants
    private const float HeatRiseRate = 15f; // Degrees per second at full heat
    private const float CoolDownRate = 5f; // Degrees per second
    private const float BurnTemperature = 150f; // Temperature at which burning occurs
    private const float OptimalTemperature = 100f; // Ideal cooking temperature

    public CookingSystem(World world, GameFacade facade)
    {
        _world = world;
        _facade = facade;

        // Query for entities that are being cooked
        _cookingQuery = new QueryDescription()
            .WithAll<Ingredient, Temperature, CookingProgress>()
            .WithNone<Burned>(); // Don't process already burned items
    }

    public void Update(float deltaTime)
    {
        // Process all cooking entities
        _world.Query(in _cookingQuery, (ref Ingredient ingredient, ref Temperature temp, ref CookingProgress progress) =>
        {
            // Update temperature based on heat level
            if (temp.HeatLevel > 0f)
            {
                temp.Current += temp.HeatLevel * HeatRiseRate * deltaTime;
            }
            else
            {
                // Cool down when no heat is applied
                temp.Current -= CoolDownRate * deltaTime;
                temp.Current = Math.Max(temp.Current, 20f); // Room temperature minimum
            }

            // Update cooking progress if actively cooking
            if (progress.IsCooking)
            {
                // Progress faster at optimal temperature
                float tempEfficiency = CalculateCookingEfficiency(temp.Current);
                float progressRate = tempEfficiency / progress.RequiredTime;

                progress.TimeCooked += deltaTime;
                progress.Progress += progressRate * deltaTime;

                // Check if cooking is complete
                if (progress.Progress >= 1.0f)
                {
                    progress.IsCooking = false;
                    progress.Progress = 1.0f;

                    // In a real game, this might trigger RecipeCompletedEvent
                    // For now, just emit a cooking progress event
                }
            }

            // Check for burning (this would add the Burned component in a real implementation)
            if (temp.Current >= BurnTemperature)
            {
                // Mark as burned
                _facade.EmitEvent(new IngredientBurnedEvent(0, ingredient.Name)); // Entity ID would come from query
                progress.IsCooking = false;
            }
        });

        // Emit progress events for UI updates
        EmitProgressEvents();
    }

    private void EmitProgressEvents()
    {
        // Query entities to emit their cooking progress
        _world.Query(in _cookingQuery, (ref Entity entity, ref Temperature temp, ref CookingProgress progress) =>
        {
            if (progress.IsCooking)
            {
                _facade.EmitEvent(new CookingProgressEvent(
                    entity.Id,
                    progress.Progress,
                    temp.Current
                ));
            }
        });
    }

    private static float CalculateCookingEfficiency(float temperature)
    {
        // Cooking is most efficient near optimal temperature
        float diff = Math.Abs(temperature - OptimalTemperature);

        if (diff < 10f)
        {
            return 1.0f; // Perfect temperature
        }
        else if (diff < 30f)
        {
            return 0.7f; // Decent temperature
        }
        else
        {
            return 0.3f; // Suboptimal temperature, slow cooking
        }
    }
}
