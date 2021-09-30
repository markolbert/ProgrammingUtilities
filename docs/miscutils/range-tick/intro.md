# Range Tick Calculator

While developing a WPF app that utilized [SyncFusion's SfRangeSlider](https://help.syncfusion.com/wpf/range-slider) I ran into an interesting challenge: how do you pick the major and minor tick intervals so that they are "reasonable" when you don't know ahead of time the values that define the range?

Before discussing the algorithm I implemented I need to point out that using the `IRangeCalculator` API really benefits from using dependency injection techniques. That's because there are a number of classes you need to configure for even the defaults to work properly. The library contains an [Autofac](https://autofac.org/) module you can use to ensure things are set up properly.

## Standardized Major/Minor Tick Intervals

The `IRangeCalculator` API provides an answer to that question. The approach I took was conceptually simple. I started by defining major and minor tick intervals independent of the range they'll be applied to.

An example, for a value range based on "regular" numbers (i.e., base 10), might be the following:
|Minor Tick Size|Minor Ticks per Major Tick|
|:-:|:-:|
|1|10|
|2|5|
|5|2|
|25|4|

For a value range based on months you might instead use:
|Minor Tick Size|Number of Minor Ticks per Major Tick|
|:-:|:-:|
|1|12|
|2|3|
|6|2|
|12|1|

You could scale these to cover whatever size range you were analyzing by multiplying the values by a power of ten and evaluating them against the actual range you were seeking to cover. You would also generally have to adjust the starting and ending points of the range so that they coincided with a minor tick value.

Scaling is simply by powers of ten for "regular" numbers. For months the scaling first converts months to years, by dividing by 12, and then scales the years by powers of ten.

## Applying the Algorithm

Consider the range **-76 => 1307** as an example. The algorithm would produce the following alternative tick sizes:

|Minor Tick Size|Minor Ticks per Major Tick|Major Tick Size|Adjusted Range Start|Adjusted Range End|# of Major Ticks|
|:-:|:-:|:-:|:-:|:-:|:-:|
|10|10|100|-80|1310|14|
|20|5|100|-80|1320|14|
|50|2|100|-100|1350|15|
|250|4|1000|-250|1500|2|
|100|10|1000|-100|1400|2|
|200|5|1000|-200|1400|2|
|500|2|1000|-500|1500|2|
|1000|10|10000|-1000|2000|1|

There are of course an infinite number of other choices, involving smaller and larger minor tick sizes. The algorithm simply stops generating them at a certain point...which leaves you with having to find some way to choose between the alternatives.

They're not  all equally attractive. In particular some of them have what I refer to as large *inactive regions*, values below the smallest value of the range (-76) or above the largest value of the range (1307). I generally use a"smallest inactive regions" rule to pick the best alternative. The API has a static function to do just that:

```csharp
public static RangeParameters? BestByInactiveRegions( this List<RangeParameters> alternatives, int maxMajorTicks = int.MaxValue )
```

This filtering function also lets you ignore any alternative with more than `maxMajorTicks` in making a choice.

## In-depth Examples

- [range of regular numbers](docs/miscutils/regularnum.md)
- [range of months](docs/miscutils/monthnum.md)
