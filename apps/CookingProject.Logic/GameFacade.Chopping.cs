using CookingProject.Logic.Features.Chopping;
using CookingProject.Logic.Features.Chopping.Commands;

namespace CookingProject.Logic;

/// <summary>
/// Chopping feature command handlers for GameFacade.
/// Delegates command processing to ChoppingSystem.
/// </summary>
public partial class GameFacade
{
    private ChoppingSystem? _choppingSystem;

    private void HandleStartChopping(StartChoppingCommand command)
    {
        _choppingSystem?.ProcessCommand(command);
    }

    private void HandleCancelChopping(CancelChoppingCommand command)
    {
        _choppingSystem?.ProcessCommand(command);
    }
}
