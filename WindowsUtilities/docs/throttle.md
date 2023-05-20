# Throttling Updates

*This code comes from [Rick Strahl's website](https://weblog.west-wind.com/posts/2017/jul/02/debouncing-and-throttling-dispatcher-events). Thanx, Rick, for solving an important WinUI problem in a simple way!*

It's common, when writing UI code, to have to deal with situations where a bunch of UI properties get updated as a group in rapid succession. In fact, this happens at least once for every UI I'm familiar with, when the app launches and initial values get set from storage.

If some or all of those updates trigger "long running" code the result can be a laggy UI. Even if the triggered code isn't long running, often times you want UI code to run after all related property changes are made, not when each individual change is made.

The trick is to "throttle" when the update code gets triggered. You wait for a short period of time, until the dust settles and all the relevant UI properties have assumed their new values, and then you trigger the update code.

That's what `ThrottleDispatcher` does. It has only one method:

|Method|Arguments|Description|
|------|---------|-----------|
|`Throttle`|`int` interval|the window to wait, in milliseconds|
||`Action<object?>` action|the action to take after the throttle period passes, which can involve an optional parameter|
||`object?` optParam = `null`|the optional parameter to pass to the action|

You use `ThrottleDispatcher` like this:

```csharp
private readonly ThrottleDispatcher _throttle = new();

public string SomeUIProperty
{
    get => _someUIProperty;
    set => _throttle.Throttle(100, ()=> UpdateUI() );
}

public string SomeOtherUIProperty
{
    get => _someOtherUIProperty;
    set => _throttle.Throttle(100, ()=> UpdateUI() );
}
```

[return to readme](readme.md)
