using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace CS.Reactive
{
    //http://stackoverflow.com/questions/34075006/rx-catchall-for-unobserved-messages
    public interface ITracked<out T>
    {
        T Value { get; }
        bool IsObserved { get; }
        T Observe();
    }

    public class Tracked<T> : ITracked<T>
    {
        public T Value { get; }
        public bool IsObserved { get; private set; }

        private readonly Action<T> _observeCallback;

        public Tracked(T value)
        {
            Value = value;
        }
        public Tracked(T value, Action<T> observeCallback) : this(value)
        {
            _observeCallback = observeCallback;
        }

        public T Observe()
        {
            IsObserved = true;
            _observeCallback?.Invoke(Value);
            return Value;
        }
    }

    public interface ITrackableObservable<out T> : IObservable<ITracked<T>>
    {
        IObservable<T> Unobserved { get; }
    }

    public class TrackableObservable<T> : ITrackableObservable<T>
    {
        public IObservable<T> Unobserved
        {
            get { return unobserved.AsObservable(); }
        }

        private readonly ISubject<T> unobserved = new Subject<T>();
        private readonly IObservable<ITracked<T>> source;

        public TrackableObservable(IObservable<T> source)
        {
            this.source = Observable
                .Create<ITracked<T>>(observer => source.Subscribe(
                    value =>
                    {
                        var trackedValue = new Tracked<T>(value);
                        observer.OnNext(trackedValue);
                        if (!trackedValue.IsObserved)
                        {
                            unobserved.OnNext(value);
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted))
                .Publish()
                .RefCount();
        }
        public TrackableObservable(IObservable<T> source, Action<T> observeCallback)
        {
            this.source = Observable
               .Create<ITracked<T>>(observer => source.Subscribe(
                   value =>
                   {
                       var trackedValue = new Tracked<T>(value, observeCallback);
                       observer.OnNext(trackedValue);
                       if (!trackedValue.IsObserved)
                       {
                           unobserved.OnNext(value);
                       }
                   },
                   observer.OnError,
                   observer.OnCompleted))
               .Publish()
               .RefCount();
        }

        public IDisposable Subscribe(IObserver<ITracked<T>> observer)
        {
            return source.Subscribe(observer);
        }
    }

    public static class TrackableObservableExtensions
    {
        public static ITrackableObservable<T> ToTrackableObservable<T>(this IObservable<T> source)
        {
            return new TrackableObservable<T>(source);
        }
        public static ITrackableObservable<T> ToTrackableObservable<T>(this IObservable<T> source, Action<T> observeCallback)
        {
            return new TrackableObservable<T>(source, observeCallback);
        }

        public static IObservable<T> Observe<T>(this IObservable<ITracked<T>> source)
        {
            return source.Do(x => x.Observe()).Select(x => x.Value);
        }
        public static IObservable<T> ObserveWhere<T>(this IObservable<ITracked<T>> source, Func<T, bool> predicate)
        {
            return source.Where(x => predicate(x.Value)).Observe();
        }
    }
}
