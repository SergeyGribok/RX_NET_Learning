using System.Reactive.Linq;
using TryRx;

/*
IObservable<long> ticks = Observable.Timer(
    dueTime: TimeSpan.Zero,
    period: TimeSpan.FromSeconds(1));

ticks.Subscribe(
    tick => Console.WriteLine($"Tick {tick}"));
*/

var numbers = new MySequenceOfNumbers();
numbers.Subscribe(
    number => Console.WriteLine($"Received value: {number}"),
    () => Console.WriteLine("Sequence terminated"));

Console.ReadLine();