using UnityEditor;
using UnityEngine;

namespace BabysFirst.EditorTools
{
    /// <summary>
    /// Import settings for audio we make ourselves. Background music (Assets/Resources/Music) is
    /// streamed and Vorbis-compressed so a long WAV stays small; narration (Assets/Audio/Narration)
    /// is Vorbis kept compressed in memory so each line starts without a delay.
    /// </summary>
    sealed class MusicImportSettings : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            var importer = (AudioImporter)assetImporter;

            // Narration: short clips, kept compressed in memory so each line starts instantly.
            if (assetPath.StartsWith("Assets/Audio/Narration/"))
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

            if (!assetPath.StartsWith("Assets/Resources/Music/")) return;
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
