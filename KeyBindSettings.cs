using UnityModManagerNet;

namespace EnhancedControls;

public class KeyBindSettings : UnityModManager.ModSettings
{
    public string NextCharacter = "Tab";

    public string NextTab = "%Tab";

    public string InventorySearch = "%F";

    public string HighlightToggle = "%R";

    public string SeparateEndTurn = "%Space";

    public string CollectAllAndClose = "F";

    public new void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);
}
