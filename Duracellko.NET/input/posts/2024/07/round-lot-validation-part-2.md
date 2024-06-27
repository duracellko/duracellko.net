Title: Round-lot validation (part 2)
Published: 2024-07-04
Tags:
- floating-point numbers
- numerical computing
- .NET
---
Recently I was implementing a validation checking that a number entered by user fits into a configured round-lot. And I decided to write few blog posts about it. In [previous post](../06/round-lot-validation-part-1) I described the problem and found out what is maximum difference between result of dividing a value by round-lot and nearest integer number. And in this post I will present the implementation.

Let me describe the problem again. We want to implement a validation function (C#):

```csharp
static bool IsValidRoundLot(double value, double roundLot)
{
    ...
}
```

The function, should return true, when there exists an integer number that is result of dividing the `value` by `roundLot`. In previous post I presented that it is not sufficient to simply divide the numbers and check, if the result is integer, because of rounding errors. I also presented that the result doesn't depend on sign of the numbers and it is possible and even better to work with absolute values of the numbers.

```csharp
value = Math.Abs(value);
roundLot = Math.Abs(roundLot);
```

Another condition is that the `value` must be equal or higher than the `roundLot`. Otherwise, it is clear that there is no integer value to be found.

```csharp
if (value < roundLot)
{
    return false;
}
```

Next step is to divide the 2 numbers and find the nearest integer next to the result.

```csharp
var ratio = value / roundLot;
var integerRatio = Math.Round(ratio);
```

[Math.Round](https://learn.microsoft.com/en-us/dotnet/api/system.math.round#system-math-round(system-double)) returns the nearest integer value. The function is available in all popular languages. .NET additionally supports different rounding strategies by specifying [MidpointRounding](https://learn.microsoft.com/en-us/dotnet/api/system.midpointrounding) value. However, it's fine to use the default value, because it has impact only on middle values (e.g. 0.5, 1.5, ...) and those are clearly too far from any integer.

And the last step is to compare, if those 2 numbers are near each other.

```csharp
return Math.Abs(ratio - integerRatio) <= acceptedError;
```

From the previous post it is known that `acceptedError` is $2^{e_r-k}$. $e_r$ is exponent of the floating point number and that can be extracted using [Math.ILogB](https://learn.microsoft.com/en-us/dotnet/api/system.math.ilogb) function. And $k$ is precision of [Double](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-double) and that is 52 bits.

So the full function is

```csharp
static bool IsValidRoundLot(double value, double roundLot)
{
    value = Math.Abs(value);
    roundLot = Math.Abs(roundLot);

    if (value < roundLot)
    {
        return false;
    }

    var ratio = value / roundLot;
    var integerRatio = Math.Round(ratio);

    var acceptedError = Math.Pow(2, Math.ILogB(ratio) - 52);
    return Math.Abs(ratio - integerRatio) <= acceptedError;
}
```

Same implementation can be done in other programming languages as functions `Abs`, `Round`, and `Pow` are pretty standard. And the language should use [IEEE 754](https://en.wikipedia.org/wiki/IEEE_754) floating-point number data type with known precision. The only non-standard function is `ILogB`. However, it is also possible to use:

```csharp
var acceptedError = Math.Pow(2, Math.Truncate(Math.Log2(ratio)) - 52);
```

or

```csharp
var acceptedError = Math.Pow(2, Math.Floor(Math.Log2(ratio)) - 52);
```

The function doesn't handle exceptional cases, for example:

- Value or round-lot is positive or negative infinity or [NaN](https://learn.microsoft.com/en-us/dotnet/api/system.double.nan#system-double-nan)
- Round-lot is 0 or very near zero.
- Ratio is positive infinity, because round-lot is very small number and value is very large number.

However, handling of these inputs depends on your application.

This post presented accurate implementation of round-lot validation. In next post I will have a look, how does this work with [Decimal](https://learn.microsoft.com/en-us/dotnet/api/system.decimal) data type.

**Note** Same as note in the previous post, it may be considered to use 1 bit lower precision for `acceptedError`.

```csharp
var acceptedError = Math.Pow(2, Math.ILogB(ratio) - 51);
```

This should cover platforms and architectures, where round-off error can be higher than just 1 bit. Although, I am not an expert on IEEE 754 standard and its implementations, so I am not sure if such implementation exists.
