using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    public interface IServiceCollection
    {
        IServiceCollection AddSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService;
        IServiceCollection AddSingleton<TService>(Func<IServiceProvider, TService> factory) where TService : class;
        IServiceCollection AddSingleton<TService>()where TService : class;
    }
}
