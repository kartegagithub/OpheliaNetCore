# Subtitle Library

A flexible and extensible .NET Core library for extracting subtitles from video/audio files and translating them into different languages. Built with Dependency Injection principles, this library allows easy integration of different speech-to-text engines and translation services.

## Features

- 🎥 **Audio Extraction**: Extract audio from video files using FFmpeg
- 🎤 **Speech Recognition**: Generate subtitles using Whisper AI (extensible to other engines)
- 🌍 **Translation**: Translate subtitles to different languages
- 🔧 **Fully Customizable**: Audio encoding parameters (codec, sample rate, channels) are configurable
- 🏗️ **Dependency Injection**: Built with DI principles for easy testing and extension
- 📦 **Interface-based**: Each component can be easily replaced with alternative implementations

## Architecture

The library is built around four main interfaces:

- **IAudioExtractor**: Handles audio extraction from video/audio files
- **ISubtitleGenerator**: Generates subtitles from audio (Whisper, Vosk, Azure Speech, etc.)
- **ISubtitleTranslator**: Translates subtitle text between languages
- **ISrtFileHandler**: Parses and writes SRT subtitle files

## Installation

### Prerequisites

1. **FFmpeg**: Must be installed and accessible from command line
   ```bash
   # Windows (using Chocolatey)
   choco install ffmpeg
   
   # macOS
   brew install ffmpeg
   
   # Linux
   sudo apt-get install ffmpeg
   ```

2. **Whisper**: Install OpenAI Whisper
   ```bash
   pip install openai-whisper
   ```

3. **NuGet Packages**:
   ```bash
   dotnet add package Microsoft.Extensions.DependencyInjection
   ```

## Quick Start

### Basic Usage (Manual DI)

```csharp
using WhisperSrtLibrary;

// Initialize components
var audioExtractor = new FFmpegAudioExtractor();
var subtitleGenerator = new WhisperSubtitleGenerator();
var translator = new PlaceholderTranslator();
var srtHandler = new SrtFileHandler();

// Create service
var service = new SubtitleService(
    audioExtractor,
    subtitleGenerator,
    translator,
    srtHandler
);

// Generate subtitles
var srtFile = await service.CreateSubtitlesAsync(
    inputFile: "video.mp4",
    audioOptions: new AudioExtractionOptions
    {
        SampleRate = 16000,
        Channels = 1,
        Codec = "pcm_s16le"
    },
    generationOptions: new SubtitleGenerationOptions
    {
        Model = "base",
        Language = "en",
        OutputPath = "output.srt"
    }
);

Console.WriteLine($"Subtitles created: {srtFile}");

// Translate subtitles
var translatedSrt = await service.TranslateSubtitlesAsync(
    srtFile: srtFile,
    targetLanguage: "es",
    outputPath: "output_es.srt"
);

Console.WriteLine($"Translated subtitles: {translatedSrt}");
```

### Using Microsoft.Extensions.DependencyInjection

```csharp
using Microsoft.Extensions.DependencyInjection;
using WhisperSrtLibrary;

// Configure services
var services = new ServiceCollection();

services.AddSubtitleServices(options =>
{
    options.FFmpegPath = "ffmpeg";
    options.WhisperPath = "whisper";
    options.TempDirectory = @"C:\Temp\Subtitles";
});

var serviceProvider = services.BuildServiceProvider();

// Get service and use it
var subtitleService = serviceProvider.GetService<SubtitleService>();

var result = await subtitleService.CreateSubtitlesAsync("video.mp4");
```

## Configuration Options

### AudioExtractionOptions

Configure how audio is extracted from video files:

```csharp
var audioOptions = new AudioExtractionOptions
{
    OutputPath = "output.wav",      // Optional: specify output path
    Codec = "pcm_s16le",             // Audio codec
    SampleRate = 16000,              // Sample rate in Hz (16000 recommended for Whisper)
    Channels = 1,                    // Number of audio channels (1 = mono)
    Format = "wav"                   // Output format
};
```

### SubtitleGenerationOptions

Configure subtitle generation:

```csharp
var generationOptions = new SubtitleGenerationOptions
{
    OutputPath = "subtitles.srt",
    Model = "base",                  // Whisper model: tiny, base, small, medium, large
    Language = "en",                 // Source language (optional)
    OutputFormat = "srt",            // Output format
    AdditionalParameters = new Dictionary<string, string>
    {
        { "task", "transcribe" },    // or "translate"
        { "verbose", "True" }
    }
};
```

### Whisper Models

| Model  | Parameters | Required VRAM | Relative Speed |
|--------|-----------|---------------|----------------|
| tiny   | 39 M      | ~1 GB         | ~32x           |
| base   | 74 M      | ~1 GB         | ~16x           |
| small  | 244 M     | ~2 GB         | ~6x            |
| medium | 769 M     | ~5 GB         | ~2x            |
| large  | 1550 M    | ~10 GB        | 1x             |

## Advanced Usage

### Implementing Custom Audio Extractor

```csharp
public class CustomAudioExtractor : IAudioExtractor
{
    public async Task<string> ExtractAudioAsync(
        string inputFile, 
        AudioExtractionOptions options)
    {
        // Your custom implementation
        // Could use different tools or cloud services
        
        return outputFilePath;
    }
}

// Use in DI
services.AddSingleton<IAudioExtractor, CustomAudioExtractor>();
```

### Implementing Custom Subtitle Generator

```csharp
public class AzureSpeechGenerator : ISubtitleGenerator
{
    public string GeneratorName => "Azure Speech";
    
    public async Task<string> GenerateSubtitlesAsync(
        string audioFile, 
        SubtitleGenerationOptions options)
    {
        // Implementation using Azure Speech Service
        // Use Microsoft.CognitiveServices.Speech SDK
        
        return srtFilePath;
    }
}

// Use in DI
services.AddSingleton<ISubtitleGenerator, AzureSpeechGenerator>();
```

### Implementing Custom Translator

Example with Google Translate:

```csharp
public class GoogleTranslator : ISubtitleTranslator
{
    private readonly TranslationClient _client;
    
    public string TranslatorName => "Google Translate";
    
    public GoogleTranslator(string apiKey)
    {
        _client = TranslationClient.CreateFromApiKey(apiKey);
    }
    
    public async Task<string> TranslateAsync(
        string text, 
        string sourceLanguage, 
        string targetLanguage)
    {
        var response = await _client.TranslateTextAsync(
            text, 
            targetLanguage, 
            sourceLanguage
        );
        
        return response.TranslatedText;
    }
}

// Use in DI
services.AddSingleton<ISubtitleTranslator>(sp => 
    new GoogleTranslator("YOUR_API_KEY"));
```

Example with DeepL:

```csharp
public class DeepLTranslator : ISubtitleTranslator
{
    private readonly Translator _translator;
    
    public string TranslatorName => "DeepL";
    
    public DeepLTranslator(string apiKey)
    {
        _translator = new Translator(apiKey);
    }
    
    public async Task<string> TranslateAsync(
        string text, 
        string sourceLanguage, 
        string targetLanguage)
    {
        var result = await _translator.TranslateTextAsync(
            text, 
            sourceLanguage, 
            targetLanguage
        );
        
        return result.Text;
    }
}
```

## API Reference

### SubtitleService

Main service class that orchestrates subtitle generation and translation.

#### Methods

**CreateSubtitlesAsync**
```csharp
Task<string> CreateSubtitlesAsync(
    string inputFile,
    AudioExtractionOptions audioOptions = null,
    SubtitleGenerationOptions generationOptions = null
)
```
Extracts audio from video/audio file and generates subtitles.

**TranslateSubtitlesAsync**
```csharp
Task<string> TranslateSubtitlesAsync(
    string srtFile,
    string targetLanguage,
    string sourceLanguage = null,
    string outputPath = null
)
```
Translates an existing SRT file to a different language.

### IAudioExtractor

Interface for audio extraction implementations.

```csharp
Task<string> ExtractAudioAsync(string inputFile, AudioExtractionOptions options);
```

### ISubtitleGenerator

Interface for subtitle generation implementations.

```csharp
Task<string> GenerateSubtitlesAsync(string audioFile, SubtitleGenerationOptions options);
string GeneratorName { get; }
```

### ISubtitleTranslator

Interface for translation implementations.

```csharp
Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage);
string TranslatorName { get; }
```

### ISrtFileHandler

Interface for SRT file operations.

```csharp
List<SubtitleEntry> ParseSrtFile(string srtFile);
void WriteSrtFile(string outputPath, List<SubtitleEntry> subtitles);
```

## Error Handling

```csharp
try
{
    var srtFile = await service.CreateSubtitlesAsync("video.mp4");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Best Practices

1. **Audio Encoding**: Use 16kHz sample rate and mono channel for Whisper for optimal performance
2. **Model Selection**: Start with `base` model for balance between speed and accuracy
3. **Language Specification**: Always specify language when known for better accuracy
4. **Temp Files**: The library automatically cleans up temporary audio files
5. **Translation**: Implement a real translation service (Google, DeepL, Azure) for production use

## Performance Tips

- Use smaller Whisper models (`tiny` or `base`) for faster processing
- Process multiple files in parallel using `Task.WhenAll`
- Consider using GPU-accelerated Whisper models for large-scale processing
- Cache audio extraction results when generating multiple subtitle versions

## Troubleshooting

### FFmpeg not found
```
Error: FFmpeg hatası: 'ffmpeg' is not recognized...
```
**Solution**: Ensure FFmpeg is installed and added to system PATH.

### Whisper not found
```
Error: Whisper hatası: 'whisper' is not recognized...
```
**Solution**: Install Whisper using `pip install openai-whisper`.

### Out of memory
```
Error: CUDA out of memory
```
**Solution**: Use a smaller Whisper model or process on CPU.

## Contributing

Contributions are welcome! Please feel free to submit pull requests with:
- New subtitle generator implementations (Vosk, Azure Speech, etc.)
- New translation service integrations
- Bug fixes and improvements
- Documentation enhancements

## License

This project is licensed under the MIT License.

## Acknowledgments

- [OpenAI Whisper](https://github.com/openai/whisper) for speech recognition
- [FFmpeg](https://ffmpeg.org/) for audio/video processing

## Support

For issues and questions, please open an issue on GitHub.

---

Made with ❤️ for the subtitle processing community