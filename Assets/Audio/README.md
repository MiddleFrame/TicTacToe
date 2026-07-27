# Audio palette

`Resources/Audio/SoundLibrary.asset` is the single place for clip assignment,
volume, pitch variation, cooldown, and simultaneous-voice limits.

- Replacing a WAV in place preserves all Unity references.
- Run `Tools/Audio/generate_audio.py` to regenerate the deterministic source
  palette, then use `Tools > TicTacToe > Audio > Rebuild Sound Library`.
- Buttons receive `ButtonSoundEmitter` automatically at runtime. To override one
  button, add that component explicitly, disable **Use Default Preset**, and type
  any preset ID from the library. No AudioClip needs to be dragged onto a button.
- The `Cell` prefab exposes **Figure Placement Animation**. `Fill` and `Scale`
  automatically use their matching sound presets.
