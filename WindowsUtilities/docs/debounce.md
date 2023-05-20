# Debouncing Property Updates

*This code comes from [Rick Strahl's website](https://weblog.west-wind.com/posts/2017/jul/02/debouncing-and-throttling-dispatcher-events). Thanx, Rick, for solving an important WinUI problem in a simple way!*

Human fingers and hands are sloppy. In writing Windows UI code it's quite common to run into situations where a property appears to get updated multiple times in rapid succession but what's really happening is just "noise" as the property settles on its final, real value.

An example of this happens every time someone moves a mouse pointer. They think they've moved it to a particular point in one motion, but from the hardware's point of view they've moved it to a bunch of closely-related points until they let go of the mouse and motion stops.

The only property value change you want to act on is the very last value. The other, intermediate values are just noise. mouse oscillates around a particular point. If the update triggers a "long running" process the result can be a laggy UI.

The trick is to "debounce" the property changes. The term debounce comes from ancient experience with mechanical switches, which while from a human point of view seem to go from "open" to "closed" in a single move, actually open and close multiple times in rapid succession before settling down into their new state.

That's what `DebounceDispatcher` does. It has only one method:

|Method|Arguments|Description|
|------|---------|-----------|
|`Debounce`|`int` interval|the window during which multiple invocations should be merged, in milliseconds|
||`Action<object?>` action|the action to take after debouncing, which can involve an optional parameter|
||`object?` optParam = `null`|the optional parameter to pass to the action|

You use `DebounceDispatcher` like this:

```csharp
private readonly DebounceDispatcher _debounce = new();

public string SomeUIProperty
{
    get => _someUIProperty;
    set => _debounce.Debounce(50, ()=> _someUIProperty = value);
}
```

[return to readme](readme.md)
