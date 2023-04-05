# J4JSoftware.VisualUtilities

The change log is [available here](changes.md).

The library repository is available on [github](https://github.com/markolbert/ProgrammingUtilities/blob/master/VisualUtilities/docs/readme.md).

This assembly is focused on functions needed to work with various kinds of media objects.

This assembly targets Net 7 and has nullability enabled.

## Table of contents

- [WebColors](#webcolors)
- Rectangle2D
  - [Constructors](#constructors)
  - [Properties](#properties)
  - [Methods](#methods)

## WebColors

The `WebColors` class contains various static methods supporting frequently-used transformations between "web-encoded" colors and colors as defined in the Microsoft/Windows environment (the difference relates to how RGB colors are stored within a 32 bit integer value). Using the Google Earth API within a Windows app is an example of where this difference comes into play.

[return to table of contents](#table-of-contents)

## Rectangle2D

### Constructors

`Rectangle2D` is a class which allows you to create two dimensional rectangles and then compare how they relate to each other (i.e., is Rectangle1 inside or outside or coterminus with Rectangle2?).

There are two public constructors and a private one to support creating copies of a `Rectangle2D` object.

```csharp
public Rectangle2D(
    float height,
    float width,
    float rotation = 0,
    Vector3? center = null,
    CoordinateSystem2D coordinateSystem = CoordinateSystem2D.Cartesian,
    float comparisonTolerance = DefaultComparisonTolerance )
```

Notice that you can define a **rotation value** to change the orientation of the rectangle in the two dimensional plane. The rotation is assumed to be in degrees. Positive values correspond to clockwise rotations.

You can also specify a **center** point for the rotation. If you don't, the midpoint of the rectangle is used.

The **coordinateSystem** parameter lets you define where the origin of the Cartesian plane is, and what increases in Y values mean geometrically:

- `CoordinateSystem2D.Cartesian` puts the origin in the center of the rectangle. It also implies *increases* in Y values move you **up**.
- `CoordinateSystem2D.Display` puts the origin in the upper left corner of the rectangle. It also implies *increases* in Y values move you **down**.

The **comparisonTolerance** parameter defines what it means for two floating point values to be considered equal. Because `Rectangle2D` is based on floating point math various calculations (e.g., a 360 degree rotation) can, through rounding errors, result in two points which *should* be the same being considered distinct. **comparisonTolerance** defines the difference between two floating point values which should still result in them being considered equal.

```csharp
public Rectangle2D(
    Vector3[] points,
    CoordinateSystem2D coordinateSystem = CoordinateSystem2D.Cartesian,
    float comparisonTolerance = DefaultComparisonTolerance
)
```

This second constructor allows you to define a `Rectangle2D` object using its four corner points. No attempt is made to normalize the provided points, so it's possible the resulting `Rectangle2D` object won't be a geometric rectangle.

[return to table of contents](#table-of-contents)

### Properties

|Property|Value Type|Description|
|--------|:--------:|-----------|
|ComparisonTolerance|`float`|the maximum absolute value by which two floating point numbers can differ and still be considered the same|
|CoordinateSystem|`CoordinateSystem2D`|an enum defining the origin of the rectangle and how Y values are interpreted (see the [constructor](#constructors) discussion above)|
|LowerLeft|`Vector3`|the lower left corner of the rectangle|
|UpperLeft|`Vector3`|the upper left corner of the rectangle|
|UpperRight|`Vector3`|the upper right corner of the rectangle|
|LowerRight|`Vector3`|the lower right corner of the rectangle|
|Center|`Vector3`|the center of the rectangle|
|Height|`float`|the height of the rectangle|
|Width|`float`|the width of the rectangle|
|BoundingBox|`Rectangle2D`|the rectangle that encloses the rectangle (which can be larger than the actual rectangle if it is rotated)|

[return to table of contents](#table-of-contents)

### Methods

#### `RelativePosition2D` Contains( `Rectangle2D` toCheck )

Indicates the containment relationship between the rectangle and another rectangle. See below for the possible values of [RelativePosition2D](#commonly-used-entities).

#### `RelativePosition2D` Contains( `Vector3` point )

Indicates the containment relationship between the rectangle and a point. See below for the possible values of [RelativePosition2D](#commonly-used-entities).

#### `Vector3` this[ `int` idx ]

Returns one of the rectangle's corners, which are numbered from the lower left corner, starting at **0** and then proceeding clockwise.

**idx** values less than 0 or greater than 3 will throw an `IndexOutOfRangeException`.

#### `IEnumerable<Edge2D>` GetEdges()

Iterates over the edges of the rectangle, starting from the lower left corner and proceeding clockwise.

[return to table of contents](#table-of-contents)

### Commonly Used Entities

`RelativePosition2D`

- `Inside`: the rectangle being evaluated is *inside* the rectangle
- `Edge`: the rectangle being evaluated is coterminus with the rectangle
- `Outside`: the rectangle being evaluated is outside the rectangle

`Edge2D` is a record containing the two `Vector3` points which define an edge. *Point1* and *Point2* are defined in a clockwise sense.

```csharp
public record Edge2D( Vector3 Point1, Vector3 Point2 );
```

[return to table of contents](#table-of-contents)
