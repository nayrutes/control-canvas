using System;
using System.Collections.Generic;
using UniRx;

namespace ControlCanvas.Editor.Extensions
{
    public static class ReactiveCollectionExtensions
    {
        public static IDisposable SubscribeAndProcessExisting<T>(this ReactiveCollection<T> collection,
            Action<T> action)
        {
            // Process existing items
            foreach (var item in collection)
            {
                action(item);
            }

            // Subscribe to new items
            return collection.ObserveAdd().Subscribe(x => action(x.Value));
        }


        public static IObservable<TResult> CombineWithPrevious<TSource, TResult>(this IObservable<TSource> source,
            Func<TSource, TSource, TResult> resultSelector)
        {
            return source.Scan(
                    Tuple.Create(default(TSource), default(TSource)),
                    (previous, current) => Tuple.Create(previous.Item2, current))
                .Select(t => resultSelector(t.Item1, t.Item2));
        }


        public static IObservable<T> DoWithLast<T>(this IObservable<T> source, Action<T> doOnLast)
        {
            T previous = default(T);
            return Observable.Create<T>(observer =>
                source.Subscribe(value =>
                {
                    if (doOnLast != null && !EqualityComparer<T>.Default.Equals(previous, default(T)))
                    {
                        doOnLast(previous);
                    }

                    previous = value;
                    observer.OnNext(value);
                }, observer.OnError, observer.OnCompleted)
            );
        }
        

    }
}