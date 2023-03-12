using System;

namespace Ophelia
{
    public static class IObservableExtensions
    {
        public static void Subscribe<T>(this IObservable<T> observable, params IObserver<T>[] args)
        {
            foreach (var observer in args)
                observable.Subscribe(observer);
        }
    }
}
