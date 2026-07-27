namespace AudioSystem
{
    /// <summary>
    /// Stable IDs used by animation code. The clips and playback tuning behind an ID
    /// live in SoundLibrary.asset, so test audio can be swapped without code changes.
    /// </summary>
    public static class SoundPresetIds
    {
        public const string UiClick = "ui.click";

        public const string FigurePlaceFill = "figure.place.fill";
        public const string FigurePlaceScale = "figure.place.scale";

        public const string DamageErase = "damage.erase";
        public const string DamageImpact = "damage.impact";
    }
}
