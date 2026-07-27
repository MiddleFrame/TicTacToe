using System.Collections.Generic;
using AudioSystem;
using UnityEditor;
using UnityEngine;

public static class AudioContentBuilder
{
    private const string LibraryFolder = "Assets/Resources/Audio";
    private const string LibraryPath = LibraryFolder + "/SoundLibrary.asset";
    private const string SfxFolder = "Assets/Audio/SFX/";
    private const string MusicPath = "Assets/Audio/Music/minimal_background_loop.wav";

    [MenuItem("Tools/TicTacToe/Audio/Rebuild Sound Library")]
    public static void Build()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        EnsureFolder("Assets/Resources", "Audio");

        ConfigureSfxImporter(SfxFolder + "ui_click_01.wav");
        ConfigureSfxImporter(SfxFolder + "ui_click_02.wav");
        ConfigureSfxImporter(SfxFolder + "figure_fill_01.wav");
        ConfigureSfxImporter(SfxFolder + "figure_fill_02.wav");
        ConfigureSfxImporter(SfxFolder + "figure_scale_01.wav");
        ConfigureSfxImporter(SfxFolder + "figure_scale_02.wav");
        ConfigureSfxImporter(SfxFolder + "damage_erase_01.wav");
        ConfigureSfxImporter(SfxFolder + "damage_erase_02.wav");
        ConfigureSfxImporter(SfxFolder + "damage_impact_01.wav");
        ConfigureSfxImporter(SfxFolder + "damage_impact_02.wav");
        ConfigureSfxImporter(SfxFolder + "damage_impact_03.wav");
        ConfigureMusicImporter(MusicPath);

        SoundLibrary library = AssetDatabase.LoadAssetAtPath<SoundLibrary>(LibraryPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<SoundLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
        }

        List<SoundPreset> presets = new()
        {
            Preset(
                SoundPresetIds.UiClick,
                0.38f,
                0.99f,
                1.01f,
                0.025f,
                3,
                "ui_click_01.wav",
                "ui_click_02.wav"),
            Preset(
                SoundPresetIds.FigurePlaceFill,
                0.32f,
                0.97f,
                1.03f,
                0.035f,
                4,
                "figure_fill_01.wav",
                "figure_fill_02.wav"),
            Preset(
                SoundPresetIds.FigurePlaceScale,
                0.52f,
                0.97f,
                1.03f,
                0.035f,
                5,
                "figure_scale_01.wav",
                "figure_scale_02.wav"),
            Preset(
                SoundPresetIds.DamageErase,
                0.31f,
                0.96f,
                1.03f,
                0.08f,
                2,
                "damage_erase_01.wav",
                "damage_erase_02.wav"),
            Preset(
                SoundPresetIds.DamageImpact,
                0.46f,
                0.94f,
                1.06f,
                0.045f,
                4,
                "damage_impact_01.wav",
                "damage_impact_02.wav",
                "damage_impact_03.wav")
        };

        AudioClip music = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath);
        library.Configure(presets, music, 0.24f);
        EditorUtility.SetDirty(library);
        SetCellPlacementAnimationDefault();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Audio content built: {LibraryPath}");
    }

    public static void BuildFromCommandLine()
    {
        Build();
    }

    private static SoundPreset Preset(
        string id,
        float volume,
        float pitchMin,
        float pitchMax,
        float cooldown,
        int maxVoices,
        params string[] clipNames)
    {
        AudioClip[] clips = new AudioClip[clipNames.Length];
        for (int i = 0; i < clipNames.Length; i++)
        {
            clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(SfxFolder + clipNames[i]);
            if (clips[i] == null)
            {
                Debug.LogError($"Audio clip is missing: {SfxFolder + clipNames[i]}");
            }
        }

        return new SoundPreset
        {
            Id = id,
            Clips = clips,
            Volume = volume,
            PitchMin = pitchMin,
            PitchMax = pitchMax,
            Cooldown = cooldown,
            MaxSimultaneousVoices = maxVoices
        };
    }

    private static void ConfigureSfxImporter(string path)
    {
        if (AssetImporter.GetAtPath(path) is not AudioImporter importer) return;
        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.PCM;
        settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        settings.preloadAudioData = true;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = true;
        importer.loadInBackground = false;
        importer.SaveAndReimport();
    }

    private static void ConfigureMusicImporter(string path)
    {
        if (AssetImporter.GetAtPath(path) is not AudioImporter importer) return;
        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.Streaming;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.55f;
        settings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
        settings.preloadAudioData = false;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = false;
        importer.loadInBackground = true;
        importer.SaveAndReimport();
    }

    private static void SetCellPlacementAnimationDefault()
    {
        const string prefabPath = "Assets/Resources/Cell.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            Cell cell = root.GetComponent<Cell>();
            if (cell == null) return;

            SerializedObject serializedCell = new(cell);
            SerializedProperty animation =
                serializedCell.FindProperty("_figurePlacementAnimation");
            if (animation == null) return;

            animation.enumValueIndex = (int) FigurePlacementAnimationType.Scale;
            serializedCell.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
