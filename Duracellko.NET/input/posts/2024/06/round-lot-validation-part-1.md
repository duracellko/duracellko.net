Title: Round-lot validation (part 1)
Published: 2024-06-27
Tags:
- floating-point numbers
- numerical computing
---
Recently I was implementing a validation checking that a number entered by user fits into a configured round-lot. For example when the round-lot is 2 then only even numbers are allowed values. Other examples would be:

| Round-lot | Allowed values      | Not allowed values |
|-----------|---------------------|--------------------|
|         1 | 1, 2, 3, 4, ...     | 0.1, 0.5, 1.5, 9.9 |
|        10 | 10, 20, 30, 100     | 1, 9, 15, 99       |
|       0.1 | 0.1, 0.2, 1.9, 33.3 | 0.05, 0.75, 22.22  |
|       0.5 | 0.5, 1, 1.5, 9, 9.5 | 0.1, 0.2, 22.22    |

In more exact terms the validation defines input as valid, when a user's input number is $u$ and a configured round-lot is $c$, then there must exist an integer number $m$, so that $u = mc$; or slightly transformed $m = u/c$. In other words the following equation must be true: $u/c = \lfloor u/c \rfloor$. Based on this definition the first naive implementation is:

```csharp
static bool IsValidRoundLot(double value, double roundLot)
{
    var ratio = value / roundLot;
    var integerRatio = Math.Round(ratio);
    return ratio == integerRatio;
}
```

After running some tests it was found out that it works for some input values, e.g. value: 1 and round-lot: 0.5. However, the function returns `false`, when input is value: 9.7 and round-lot: 0.1. And that is not correct. If we want to understand this behaviour and know how to fix it, we have to understand how floating-point numbers work.

## Floating-point numbers

Mostly, when people define some formulas or calculations (e.g. $m=u/c$), they are mostly defined in space of real numbers or rational numbers. However, there is infinite number of real or rational numbers, but computers have only finite memory, so computers use floating-point numbers. So let's talk about difference between real numbers and floating-point numbers. For the rest of this post we can consider that all numbers are not negative. I think it is quite clear that it's possible to do the validation on absolute values of $u$ and $c$, so sign of the numbers is not relevant. Floating-point numbers are represented by a significand $s$ and an exponent $e$. Then the expressed number is $s2^e$. Significand is a rational number, but with limited precision. And the exponent is an integer. For example single precission floating-point numbers have significand of size 11 bits and exponent of size 4 bits. When bits of significand are $s_1, s_2, \dots s_{11}$ then the number represented by the bits is

$$
\left(1 + \sum_{i=1}^{11} s_{i}2^{-i} \right) 2^e
$$

Notice that $1 \leq s < 2$. This format of floating-point numbers is called normalized. Or it's said that a floating-point number is normalized, when it is stored in this format. Normalized floating point numbers ensure that there is exactly 1 representation of each number. For example in binary $0.011 \cdot 2^{1}$ and $1.1 \cdot 2^{-1}$ are the same numbers, but only the latter one is normalized. We will use this property later.

Let's have a look at decimal number $1.1$ for example. It can be easily expressed using exponent 0, but how would the significand look like?

$$
1 + 0 \cdot 2^{-1} + 0 \cdot 2^{-2} + 0 \cdot 2^{-3} + 1 \cdot 2^{-4} + 1 \cdot 2^{-5} + 0 \cdot 2^{-6} + 0 \cdot 2^{-7} + 1 \cdot 2^{-8} + 1 \cdot 2^{-9} + 0 \cdot 2^{-10} + 0 \cdot 2^{-11} =
$$

$$
1 + 0.0625 + 0.03125 + 0.00390625 + 0.001953125 = 1.099609375
$$

But that is not exactly $1.1$. So the first lesson learned is that it is not possible to convert decimal number $1.1$ to a binary floating point number exactly. And adding more bits does not help either. Binary representation with 11 bits is

$1.00011001100$

And with 22 bits it is

$1.0001100110011001100110$

It's possible to see that it is very likely that the pattern $0011$ would repeat itself to infinity. So it would be needed infinite number of bits to represent decimal number 1.1 in binary. And that is one of the issues that causes the naive implementation of the validation function returning incorrect result in some cases.

## Implementation

General practice working with floating-point numbers is to not consider them as exact numbers. Procedure implementations should consider that a floating-point number represents a nearby value. For example, binary floating-point number $1.00011001100$ may represent decimal number 1.1. Therefore comparing 2 floating-point numbers shouldn't be done exactly, but should allow a small difference. How should the implementation of the validation function be changed?

At first the input values should be converted to absolute values, because the result doesn't depend on sign of the numbers.

```csharp
value = Math.Abs(value);
roundLot = Math.Abs(roundLot);
```

Another condition is that the `value` must be higher or equal than the `roundLot`. Otherwise, it is clear that there is no integer value to be found.

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

[Math.Round](https://learn.microsoft.com/en-us/dotnet/api/system.math.round#system-math-round(system-double)) returns the nearest integer value. The function is available in all popular programming languages. .NET additionally supports different rounding strategies by specifying [MidpointRounding](https://learn.microsoft.com/en-us/dotnet/api/system.midpointrounding) value. However, it's fine to use the default value, because it has impact only on middle values (e.g. 0.5, 1.5, ...) and those are definitely too far from any integer.

And the last step is to compare, if those 2 numbers are near each other.

```csharp
return Math.Abs(ratio - integerRatio) <= acceptedError;
```

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

    var acceptedError = ...;
    return Math.Abs(ratio - integerRatio) <= acceptedError;
}
```

What is `acceptedError`? That will be explained in [the next part](../07/round-lot-validation-part-2).
