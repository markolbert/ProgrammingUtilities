# J4JSoftware.WPFUtilities

This library targets Net 7 and has nullability enabled.

The library repository is available on [github](https://github.com/markolbert/ProgrammingUtilities/blob/master/WPFUtilities/docs/readme.md).

The change log is [available here](changes.md).

This assembly contains several utility classes and objects that I've found useful in WPF programs:

|Class/Object|Type|Description|
|------------|:--:|-----------|
|`MediaDrawing`|extension method class|static methods for converting between `System.Windows.Media.Color` and `System.Drawing.Color` objects|
|`CustomComboBox`|XAML|Customized WPF combobox|
|`SizeObserver`|static DependencyProperty class|Supports live binding between a WPF element's Size and a viewmodel|
|`DebounceDispatcher`|class|Suppresses excessive WPF property events caused by human "noisyness" in dealing with a UI|

## MediaDrawing

There are two different **Color** objects inside of the C# libraries, *System.Drawing.Color* and *System.Windows.Media.Color*. The static class `MediaDrawing` provides methods for converting from `System.Windows.Media.Color` to `System.Drawing.Color`.

## CustomComboxBox.xaml

I use this custom XAML to work around a limitation of WPF combobox controls, I believe having to do with not displaying error templates properly.

I did not develop this code, but found it online on someone's website. It did not, as I recall, have any license constraints on its use so I'm treating it as open source.

I apologize to the original author for not giving him/her credit. I'd be happy to do so if she/he contacts me.

## SizeObserver

WPF doesn't natively provide a way of linking the size of a display element to a viewmodel property so the property's value is "live".

The `SizeObserver` class developed by Ken Boogaart and Athari adds this capability. You use it by referencing the `WPFUtilities` assembly and including the following in the control whose size you want to bind to a viewmodel. Note that your details may differ depending on how you've implemented your viewmodel:

```xml
<UserControl ...
    SizeObserver.Observe="True"
    SizeObserver.ObservedWidth="{Binding Width, Mode=OneWayToSource}"
    SizeObserver.ObservedHeight="{Binding Height, Mode=OneWayToSource}"
```

For details, please consult their [StackOverflow answer](https://stackoverflow.com/questions/1083224/pushing-read-only-gui-properties-back-into-viewmodel/1083733#1083733).

## DebounceDispatcher

I sometimes run into problems where a WPF visual element sends "too many" events to the viewmodel properties it's bound to. This is similar to the "key bounce" problem that occurs when you monitor low-level keystrokes: what appears to you and me as a single key stroke can actually be composed of a number of keyup/keydown events.

`DebounceDispatcher`, developed by Rick Strahl, addresses this problem by defining a customizable timer which consolidates multiple events occuring within a definable time window into a single event.

The typical pattern I use with `DebounceDispatcher` involves bound viewmodel properties (code drawn from Rick's site and lightly edited):

```csharp
private readonly DebounceDispatcher debounceTopicsFilter = new DebounceDispatcher();

private string _topicsFilter;

public string TopicsFilter
{
    get => _topicsFilter;

    set
    {
        if (value == _topicsFilter) return;
        _topicsFilter = value;
        OnPropertyChanged();

        // debounce the tree filter change notifications
        debounceTopicsFilter.Debounce(500, e => OnPropertyChanged(nameof(FilteredTopicTree)));
    }
}
```

For more information please consult [Rick's website](https://weblog.west-wind.com/posts/2017/jul/02/debouncing-and-throttling-dispatcher-events).
