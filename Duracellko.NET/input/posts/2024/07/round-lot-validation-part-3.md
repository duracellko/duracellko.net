Title: Round-lot validation (part 3)
Published: 2024-07-16
Tags:
- decimal
- numerical computing
- .NET
---
Recently I was implementing a validation checking that a number entered by user fits into a configured round-lot. And I decided to write few blog posts about it. In [the previous post](../07/round-lot-validation-part-2) I presented implementation of the function for data type [double](https://learn.microsoft.com/en-us/dotnet/api/system.double). And in this post I will focus on [decimal](https://learn.microsoft.com/en-us/dotnet/api/system.decimal) data type.

## Decimal data type

Decimal data type is also a floating-point number, but in quite different format. In this post I will ignore negative numbers again as they don't have impact on the round-lot validation. Format of decimal numbers is $s10^{-e}$. There is, however, much more differences than just base of 10 instead of 2. Big difference is that $s$ is integer. And another difference is that the exponent range is much smaller. In general, when the type uses $k$ bits for storing the integer part $s$, then maximum value of $s$ is $2^k-1$ and range of exponent is $ 0 \dots \lfloor log_{10} \left( 2^k-1 \right) \rfloor $. However, keep in mind that the exponent is multiplied by $-1$, so the final exponent is always negative.

According to documentation of [GetBits](https://learn.microsoft.com/en-us/dotnet/api/system.decimal.getbits) function decimal type in .NET uses 128 bits. 96 bits is reserved for the integer $s$. That makes the range of integer values $0 \dots 79,228,162,514,264,337,593,543,950,335$ and range of the exponent is $0 \dots -28$. Notice that the maximum number has 29 digits in decimal notation. So the exponent can put decimal point anywhere between the digits, but not further. Probably you noticed that only 5 bits is needed to store values between 0 and 28. So most of the bits out of 32 bits is unused.

Another diference of the decimal data type is that there is no definition of normalized representation. For example $1 \cdot 10^0$ is the same number as $10 \cdot 10^{-1}$. Or in binary format

$$
\left( 1 \cdot 2^0 \right) 10^0 = \left( 1 \cdot 2^3 + 0 \cdot 2^2 + 1 \cdot 2^1 + 0 \cdot 2^0 \right) 10^{-1}
$$

As you can see, the 2 numbers have quite different binary representation. And that is probably reason why .NET does not normalize these numbers. Normalization operation may not be cheap as it is not simple bit-shift operation. And it is hard to estimate, what would be the best exponent for next operation.

## Applications

There is lot of applications that don't work with real numbers, but with decimal numbers. By decimal numbers, I mean rational numbers that can be expressed by formula $ \sum_{i=k}^{l} a_i10^i $ for some integer $k$ and $l$. Most common example of such applications are financial applications. And the reason is that no bakery is going to sell a pie for EUR π. Although that seems interesting idea. Also you would never see price EUR $\frac{10}{3}$ in any shop. And thus, lot of applications that work with human produced numbers use decimal numbers. These applications usually define, how to deal with rounding issues. For example, dividing 100 to 3 parts is not $\frac{100}{3}$, but it may be defined as 33%, 33%, and 34%.

Huge advantage af using decimal data type in such applications is that the application can assume exact precision. In [the first blog post](../06/round-lot-validation-part-1) I mentioned that decimal number $0.1$ cannot be converted to binary notation with finite precision. Therefore, already entering and storing numbers in binary floating-point data type is introducing error that has to be considered in further operations. However, decimal numbers can be entered and stored in decimal datatype without any precision lost. And thus no error has to be considered later on.

## Implementation

First version of the implementation is simple. It is same as the first naive implementation for `double` data type. And it has additional check to handle case, when `value < roundLot`. It was mentioned before, when $u < c$, then there is no integer $m$ that $u = mc$.

```csharp
static bool IsValidRoundLot(decimal value, decimal roundLot)
{
    value = Math.Abs(value);
    roundLot = Math.Abs(roundLot);

    if (value < roundLot)
    {
        return false;
    }

    var ratio = value / roundLot;
    var integerRatio = Math.Round(ratio);
    return ratio == integerRatio;
}
```

Is this implementation accurate? What kind of error does it introduce or tollerate?

## Round-off error

It was established that there is no input error for this validation function. What about round-off error? Of course, result of division operation can be a number that is not decimal number. For example dividing number 10 by 3 is $3.333\dots$ and that is not a decimal number. For simpler notation let's define function $\mathrm{dec}(x)$ that converts any rational number $x$ to the nearest decimal number. Let's have a look at the rounding error closer. The numbers $u$ and $c$ are stored in following format.

$$
u = s_u 10^{-e_u}
$$

$$
c = s_c 10^{-e_c}
$$

Then result of $\mathrm{dec}\left(\frac{u}{c}\right)$ is stored in format $s_r 10^{-e_r}$. As described previously we can look at the decimal data type as a string of decimal digits and the exponent just defines position of decimal point. So the highest precision of $\mathrm{dec}\left(\frac{u}{c}\right)$ can be achieved by storing the highest decimal digit as the first digit in the string. Then exponent $e_r$ is

$$
e_r = l - \biggl\lfloor \max \left( \log_{10} \frac{u}{c} , 0 \right) \biggr\rfloor
$$

where $l$ is the maximum possible exponent. In .NET decimal type $l = 28$. Here are few examples to understand it better. To make the examples more readable, I present them on decimal type with maximum exponent 6 and maximum number of digits 7. That would require 24 bits for the integer part.

| $\frac{u}{c}$ | $s_r$   | $e_r$ |
|---------------|---------|-------|
|             1 | 1000000 |     6 |
|            10 | 1000000 |     5 |
|           0.1 | 0100000 |     6 |
|      0.000001 | 0000001 |     6 |
|       1000000 | 1000000 |     0 |
|       1234567 | 1234567 |     0 |
|      123456.7 | 1234567 |     1 |
|      1.234567 | 1234567 |     6 |
|      999.9999 | 9999999 |     4 |

We defined precondition that $u \geq c$, and thus the previous formula can be simplified.

$$
e_r = l - \biggl\lfloor \log_{10} \frac{u}{c} \biggr\rfloor
$$


I didn't find this in documentation, but I assume that .NET uses standard decimal rounding to store the result of decimal operations. That means the result is calculated with higher precision than 29 digits (or more precisely higher precision than 96 bits), and then following rounding function is applied to round the integer part.

$$
f(x) = \mathrm{round}(x) = \lfloor x + 0.5 \rfloor
$$

Therefore rounding error of the integer part is $0.5$ and the error of the number including exponent is $\frac{1}{2} 10^{-e_r}$.

$$
\left| \frac{u}{c} - \mathrm{dec}\left(\frac{u}{c}\right) \right| \leq \frac{1}{2} \cdot 10^{\bigl\lfloor \log_{10} \frac{u}{c} \bigr\rfloor - l}
$$

However, we have to consider the mismatch between decimal and binary format of the numbers. Maximum .NET decimal type value is $\approx 79 \cdot 10^{27}$. Therefore, it's not possible to store decimal number with 29 digits 9:

$$
\sum_{i=0}^{29} 9 \cdot 10^{i}
$$

So in certain cases the decimal value can use only 28 digits. And thus, in general the error would be 10-times bigger. So the round-off error is

$$
\left| \frac{u}{c} - \mathrm{dec}\left(\frac{u}{c}\right) \right| \leq 5 \cdot 10^{\bigl\lfloor \log_{10} \frac{u}{c} \bigr\rfloor - l}
$$

## Conclusion

The round-off error is the only error that affects result $r=\frac{u}{c}$. However, function $\mathrm{dec}$ has nice property: When $r$ is integer, then $\mathrm{dec}(r)=r$ is integer too. So there are 3 possible cases for values $u$ and $c$.

1. There exists integer $m$, so that $u=mc$. Then $m=\mathrm{dec}\left(\frac{u}{c}\right)$.
2. $\frac{u}{c}$ is not integer, but there exists integer $m$ within range of round-off error of $\frac{u}{c}$. That means $ \left| \frac{u}{c} - m \right| \leq 5 \cdot 10^{\lfloor \log_{10} m \rfloor - l} $. Then $m=\mathrm{dec}\left(\frac{u}{c}\right)$.
3. There does not exist any integer within range of round-off error of $\frac{u}{c}$.

In cases 1 and 3 the round-lot validation function returns correct result. In case 1 it correctly identifies that the value fits into the round-lot. And in case 3 it correctly identifies that the value does not fit into the round-lot. Only in case 2 the function returns incorrect result. It identifies that the value fits into the round-lot, but it does not. This means that the implementation of the round-lot validation of `decimal` data type has false-positives, but it does not have false-negatives. This was not case for the validation function of `double` data type. The naive implementation had both false-negatives and false-positives. We had to improve the implementation to have only false-positives.

Unfortunately, it is not possible to eliminate false-positive results in the round-lot validation function of `decimal` data type. However, there is a technique to reduce them. False-positive result happens, when $m=\mathrm{dec}\left(\frac{u}{c}\right)$ is integer, but $\frac{u}{c}$ is not integer. Therefore, $u \neq mc$. So the function simply checks, if this is true or not.

```csharp
static bool IsValidRoundLot(decimal value, decimal roundLot)
{
    value = Math.Abs(value);
    roundLot = Math.Abs(roundLot);

    if (value < roundLot)
    {
        return false;
    }

    var ratio = value / roundLot;
    var integerRatio = Math.Round(ratio);
    if (ratio != integerRatio)
    {
        return false;
    }

    return value == ratio * roundLot;
}
```

Unfortunately, multiplication of `ratio` and `roundLot` has also round-off error, and thus it does not completely eliminate false-positive results.

Good thing is that, unlike `double`, `decimal` data type does not define special values, like $\infty$ or $-\infty$. However, there is still special case, when `value` is too high and `roundLot` is too low. In such case `value / roundLot` throws `RuntimeException`, because the result overflows the maximum `decimal` value. This is another difference from `double` data type.
