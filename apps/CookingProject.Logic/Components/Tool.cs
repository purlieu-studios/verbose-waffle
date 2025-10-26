namespace CookingProject.Logic.Components;

/// <summary>
/// Tag component identifying an entity as a tool (knife, press, etc.).
/// Used for filtering and querying tool entities.
/// </summary>
public struct Tool
{
    /// <summary>
    /// The type of tool (e.g., "Knife", "Press", "SharpeningStone").
    /// </summary>
    public string ToolType;
}
