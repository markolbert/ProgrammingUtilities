# J4JSoftware.VisualUtilities

The change log is [available here](changes.md).

The library repository is available on [github](https://github.com/markolbert/ProgrammingUtilities/blob/master/VisualUtilities/docs/readme.md).

This assembly is focused on functions needed to work with various kinds of media objects.

This assembly targets Net 7 and has nullability enabled.

## WebColors

The `WebColors` class contains various static methods supporting frequently-used transformations between "web-encoded" colors and colors as defined in the Microsoft/Windows environment (the difference relates to how RGB colors are stored within a 32 bit integer value). Using the Google Earth API within a Windows app is an example of where this difference comes into play.

## Vector-based and Coordinate System Utilities

I find working with the Windows Vector-based API difficult. It lacks built-in support for important capabilities I frequently need (e.g., creating rectangles, finding perpendiculars).

At the same time, I find dealing with Windows' display coordinate system confusing, because of the oddball coordinate system it uses, with (0,0) being the upper-left corner of the screen and increasing values of the Y coordinate taking you **down** the screen, rather than up. Any API manipulating a (real) world coordinate system, like Google Maps, thus creates a (mind-bending) conflict when integrating such an API into a Windows/Microsoft program.

The vector-based and coordinate system utilties in this library attempt to address these issues.

`VectorPolygon` creates polygons from arrays of `Vector2` objects via its static `Create()` method. It also lets you create rectangles from upper-left corner (X,Y) coordinates, a width and a height via its static `CreateRectangle()` method. `VectorPolygon` has no public constructor.

`VectorPolygon` provides a number of useful properties and methods:

|Name|Type|Description|
|----|:--:|-----------|
|`Vertices`|property|read-only collection of `Vector2` objects defining the polygon's vertices|
|`Center`|property|a `Vector2` object defining the center of the polygon|
|`IsConvex`|property|true if the polygon is convex, false if it is concave|
|`Edges`|enumerator|an enumerator (`IEnumerable<Vector2>` of the polygon's edges|
|`Minimum`|property|a tuple (X,Y) of the upper-left corner of the polygon's bounding box|
|`Maximum`|property|a tuple (X,Y) of the lower-right corner of the polygon's bounding box|
|`BoundingRectangle`|property|a tuple (Width, Height) of the polygon's bounding box|

The `VectorExtensions` static class contains a number of methods (some are extension methods) that I've found useful:

|Name|Type|Description|
|----|:--:|-----------|
|`Perpendicular`|extension method (`Vector2`)|calculates the perpendicular to a `Vector2`, optionally normalizing it|
|`Translate`|extension method (`VectorPolygon`)|moves/translates a `VectorPolygon` along a provided `Vector2` or X/Y deltas|
|`Rotate`|extension method (`VectorPolygon`)|rotates a `VectorPolygon` by a number of degrees, around an optional center point|
|`GetPerpendiculars`|static enumerator|enumerates `Vector2` perpendiculars based on a collection of one or more `VectorPolygon`s|
|`GetNormalizedPerpendiculars`|static enumerator|similar to `GetPerpendiculars`, but normalizes the calculated perpendiculars|
|`Intersects`|static method|returns true if two supplied `VectorPolygon`s intersect, false otherwise, optionally throwing an exception if the supplied polygons are not each convex|
|`Inside`|static method|returns true if one of the two supplied `VectorPolygon`s is inside the other, false otherwise, optionally throwing an exception if the supplied polygons are not each convex|
|`ChangeCoordinateSystem`|converts a `VectorPolygon` from one coordinate system to another|
