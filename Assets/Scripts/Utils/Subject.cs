using System.Collections.Generic;
using UnityEngine;

public abstract class Subject : MonoBehaviour
{
    private List<IObserver> _subscribers = new List<IObserver>();

    public void Subscribe(IObserver observer)
    {
        _subscribers.Add(observer);
    }

    public void Subscribe(IObserver[] observers)
    {
        _subscribers.AddRange(observers);
    }

    public void Unsubscribe(IObserver observer)
    {
        _subscribers.Remove(observer);
    }

    public void Notify(EventsEnum evt)
    {
        _subscribers.ForEach((_observer) => {_observer.OnNotify(evt);});
    }
}