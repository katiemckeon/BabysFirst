using UnityEditor;
using UnityEngine;

namespace BabysFirst.EditorTools
{
    /// <summary>
    /// Import settings for audio we make ourselves, matched by folder name anywhere under
    /// Assets/_Project so a new episode needs no change here. Background music (any
    /// Resources/Music folder) is streamed and Vorbis-compressed so a long WAV stays small;
    /// narration (any Audio/Narration folder) is Vorbis kept compressed in memory so each line
    /// starts without a delay.
    /// </summary>
    sealed class AudioImportSettings : AssetPostprocessor
    {
        const string Root = "Assets/_Project/";

        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(Root)) return;
            var importer = (AudioImporter)assetImporter;

            // Narration: short clips, kept compressed in memory so each line starts instantly.
            if (assetPath.Contains("/Audio/Narration/"))
            {
                importer.defaultSampleSettings = new AudioImporterSampleSettings
                {
                    loadType = AudioClipLoadType.CompressedInMemory,
                    compressionFormat = AudioCompressionFormat.Vorbis,
                    quality = 0.7f,
                    sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate,
                };
                return;
            }

            if (!assetPath.Contains("/Resources/Music/")) return;
            importer.loadInBackground = true;
            importer.defaultSampleSettings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.Streaming,
                compressionFormat = AudioCompressionFormat.Vorbis,
                quality = 0.6f,
                sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate,
            };
        }
    }
}
