Title: Round-lot validation (part 4)
Published: 2024-08-21
Tags:
- decimal
- numerical computing
- .NET
---
In my [previous blog post](../07/round-lot-validation-part-3) I was writing about round-lot validation on data type [decimal](https://learn.microsoft.com/en-us/dotnet/api/system.decimal) in .NET. I presented that the implementation using simple division of the numbers does not return any false-negatives. It means that the function never returns false, when a value fits into a round lot. However, it can return false-positives. It means that the function may return true, when the value doesn't fit into the round-lot. I also presented a small update to mitigate false-positives, but we didn't eliminate them completely.

In this blog post I will show, how to eliminate false-positives using [BigInteger](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.biginteger). The function may not be that useful by itself. If your application is concerned about false-positive scenarios, then it is likely that `decimal` data type is not the right one for your values. Therefore, we will have look at another data type. And that is the goal of this blog post.

## False-positive

Let's have a look at an example that gives false-positive scenario. False positive happens, (not only) when $ \left| \frac{u+1}{c} - \frac{u}{c} \right| \leq 10^{-29} $. It means, when a small change of the value divided by round-lot changes only at 29th decimal digit. Precision of this operation is lost, because `decimal` type can store at most 28 digits. Following example presents numbers that are problematic:

- Round-lot: 1.0000000000000000000000000001
- Value: 20000000000000000000000000002 - does fit into the round-lot and the validation function returns true.
- Value: 20000000000000000000000000003 - does not fit into the round-lot, but the validation function returns true.

Let's have a look at the format of `decimal` data type again. A decimal number has format $ d = s 10^{-e} $. Then dividing of numbers $u$ and $c$ is

$$
\frac{u}{c} = \frac{s_u 10^{-e_u}}{s_c 10^{-e_c}}
$$

In special case, when $e_u = e_c$, then

$$
\frac{u}{c} = \frac{s_u 10^{-e}}{s_c 10^{-e}} = \frac{s_u}{s_c}
$$

 And both $s_u$ and $s_c$ are integers, so the validation function should return true if, and only if
 
 $$
 0 \equiv s_u \pmod{s_c}
 $$
 
 Is it possible to use the same exponent for both numbers all the time? Yes, it is. Let's say:

$$
e_{max} = \max(e_u, e_c)
$$

Then

$$
\frac{u}{c} = \frac{u 10^{e_{max}}}{c 10^{e_{max}}}
$$

It should be clear that both $u 10^{e_{max}}$ and $c 10^{e_{max}}$ are integers, because $e_{max}-e_u \geq 0$ and $e_{max}-e_c \geq 0$. Therefore, our validation function can check if

$$
0 \equiv u 10^{e_{max}} \pmod{c 10^{e_{max}}}
$$

Or more explicitly

$$
0 \equiv s_u 10^{e_{max}-e_u} \pmod{s_c 10^{e_{max}-e_c}}
$$

Unfortunately, multiplying numbers $u$ or $c$ by a number higher than 1 may not fit into the `decimal` data type. But it should fit into [BigInteger](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.biginteger).

## Big integer

[BigInteger](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.biginteger) data type can store an unlimited integer value. Well, there are probably some limitations, but we are definitely not going to hit them. So let's get straight to the implementation.

At first we need 2 helper functions. The first function extracts significand (the integer part) of the `decimal` data type and returns `BigInteger`. Decimal type has [GetBits](https://learn.microsoft.com/en-us/dotnet/api/system.decimal.getbits) function that returns 4 integers. The first 3 integers are low, medium and high 32 bits of the 96-bit integer significand. As the first integer is low, then it means that the 96-bit integer is stored in [Little-endian](https://en.wikipedia.org/wiki/Endianness). The function to extract significand out of decimal is:

```csharp
private static BigInteger GetDecimalSignificand(decimal value)
{
    Span<int> decimalBits = stackalloc int[4];
    decimal.GetBits(value, decimalBits);

    Span<byte> bits = stackalloc byte[12];
    BinaryPrimitives.WriteInt32LittleEndian(bits, decimalBits[0]);
    BinaryPrimitives.WriteInt32LittleEndian(bits.Slice(4), decimalBits[1]);
    BinaryPrimitives.WriteInt32LittleEndian(bits.Slice(8), decimalBits[2]);

    return new BigInteger(bits, isUnsigned: true, isBigEndian: false);
}
```

The function treats the significand as unsigned integer. It is, because sign of the decimal number is not included in those 96 bits. Also our round-lot validation function does not care if the number is positive or negative, so we don't have to take care of the sign bit.

Next function extracts exponent of the `decimal` type, of course. The exponent is stored in bits 16 to 23 of the last integer returned by `GetBits` function. So the extracting function is following:

```csharp
private static int GetDecimalExponent(decimal value)
{
    Span<int> decimalBits = stackalloc int[4];
    decimal.GetBits(value, decimalBits);
    return (decimalBits[3] >> 16) & 0x1F;
}
```

Then the round-lot validation function is simple. It calculates $e_{max}$ and then checks if
$$
0 \equiv s_u 10^{e_{max}-e_u} \pmod{s_c 10^{e_{max}-e_c}}
$$

```csharp
private static bool IsValidRoundLot(decimal value, decimal roundLot)
{
    value = Math.Abs(value);
    roundLot = Math.Abs(roundLot);

    if (value < roundLot)
    {
        return false;
    }

    var bigValue = GetDecimalSignificand(value);
    var bigRoundLot = GetDecimalSignificand(roundLot);
    var valueExponent = GetDecimalExponent(value);
    var roundLotExponent = GetDecimalExponent(roundLot);

    var maxExponent = Math.Max(valueExponent, roundLotExponent);
    bigValue *= BigInteger.Pow(10, maxExponent - valueExponent);
    bigRoundLot *= BigInteger.Pow(10, maxExponent - roundLotExponent);

    return bigValue % bigRoundLot == 0;
}
```

## Conclusion

This blog post presented, how to convert `decimal` data type to `BigInteger` and how it can be used to do exact round-lot validation. And this conversion can be used in much more scenarios than just validation function.