namespace CookingProject.Logic.Core.Systems;

/// <summary>
/// Base interface for all game systems.
/// Systems contain game logic and operate on entities with specific components.
/// </summary>
public interface IGameSystem
{
    /// <summary>
    /// Updates the system. Called once per frame by GameFacade.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame in seconds.</param>
    void Update(float deltaTime);
}
