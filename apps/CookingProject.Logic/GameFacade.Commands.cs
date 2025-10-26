using CookingProject.Logic.Core.Commands;
using CookingProject.Logic.Features.Cooking.Commands;
using CookingProject.Logic.Features.Sharpening.Commands;

namespace CookingProject.Logic;

/// <summary>
/// Command routing for GameFacade.
/// Routes incoming commands to appropriate handler methods.
/// </summary>
public partial class GameFacade
{
    /// <summary>
    /// Processes a command from Godot (player input/intent).
    /// Commands are routed to the appropriate handler methods.
    /// </summary>
    /// <param name="command">The command to process.</param>
    public void ProcessCommand(IGameCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_isInitialized)
        {
            throw new InvalidOperationException("GameFacade must be initialized before ProcessCommand()");
        }

        switch (command)
        {
            // Sharpening commands
            case StartSharpeningCommand sharpenCmd:
                HandleStartSharpening(sharpenCmd);
                break;

            case CancelSharpeningCommand cancelCmd:
                HandleCancelSharpening(cancelCmd);
                break;

            // Cooking commands
            case SetHeatLevelCommand setHeatCmd:
                HandleSetHeatLevel(setHeatCmd);
                break;

            case PlaceFoodOnBurnerCommand placeCmd:
                HandlePlaceFoodOnBurner(placeCmd);
                break;

            case RemoveFoodFromBurnerCommand removeCmd:
                HandleRemoveFoodFromBurner(removeCmd);
                break;

            default:
                throw new NotSupportedException($"Command type {command.GetType().Name} is not supported");
        }
    }
}
