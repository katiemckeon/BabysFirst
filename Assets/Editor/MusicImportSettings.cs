using UnityEditor;
using UnityEngine;

namespace BabysFirst.EditorTools
{
    /// <summary>
    /// Import settings for background music (anything under Assets/Resources/Music): streamed and
    /// Vorbis-compressed so a long WAV stays small in the build and doesn't sit decoded in memory.
    /// </summary>
    sealed class MusicImportSettings : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith("Assets/Resources/Music/")) return;

            var importer = (AudioImporter)assetImporter;
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
