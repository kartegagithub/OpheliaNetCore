using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// Microsoft.Extensions.DependencyInjection için extension metodları
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSubtitleServices(
            this IServiceCollection services,
            Action<SubtitleServiceOptions> configure = null)
        {
            var options = new SubtitleServiceOptions();
            configure?.Invoke(options);

            services.AddSingleton<IAudioExtractor>(sp =>
                new FFmpegAudioExtractor(options.FFmpegPath, options.TempDirectory));

            services.AddSingleton<ISubtitleGenerator>(sp =>
                new WhisperSubtitleGenerator(options.WhisperPath));

            services.AddSingleton<ISubtitleTranslator, PlaceholderTranslator>();
            services.AddSingleton<ISrtFileHandler, SrtFileHandler>();
            services.AddSingleton<SubtitleService>();

            return services;
        }
    }
}
