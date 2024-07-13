Title: Round-lot validation (part 2)
Published: 2024-07-04
Tags:
- floating-point numbers
- numerical computing
- .NET
---
Recently I was implementing a validation checking that a number entered by user fits into a configured round-lot. And I decided to write few blog posts about it. In [the previous post](../06/round-lot-validation-part-1) I described the problem and created naive implementation that didn't work in some cases. And I presented implementation that should handle those cases, but there is unanswered question about `acceptedError`.

## Error of floating-point numbers

It was explained that a floating-point number may represent a nearby value. For example, binary floating-point number $1.00011001100$ may represent decimal number 1.1. Actually there exist multiple real numbers, which would be represented by the same floating-point number with specific precision, because there is much more real numbers than floating-point numbers. It is possible to define relationship between the floating point numbers and real numbers that floating-point significand $s$ represents interval of real numbers

$$
[ s - 2^{-k-1},  s + 2^{-k-1} ]
$$

where $k$ is number of bits used to store the significand. We can also say that a floating-point number represents any real number $v$ that satisfies following inequation

$$
s - 2^{-k-1} \leq v \leq s + 2^{-k-1}
$$

And including exponent it would be

$$
\left( s - 2^{-k-1} \right) 2^e \leq v \leq \left(s + 2^{-k-1} \right) 2^e
$$


## Error of division

Now after understanding of relationship between floating-point numbers and real numbers we can get back to our problem. The problem is to find out if there exists any integer number $r$ that

$$
\frac{u}{c} - \epsilon \leq r \leq \frac{u}{c} + \epsilon
$$

But what is the $\epsilon$? Is it possible to calculate it or estimate it? Yes, it is. Before that, however, I have to present a lemma.

**Lemma 1**: For any positive real numbers $x$, $a$, $b$, and integer $n$, when $a \geq 2^{n}b > 0$ then

$$
\frac{x}{a+b} \geq \frac{x}{a} - \frac{1}{2^n} \cdot \frac{x}{a+b}
$$

Proof:

$$
\frac{x}{a+b} \geq \frac{x}{a} - \frac{1}{2^n} \cdot \frac{x}{a+b}
$$

$$
\left( 1 + \frac{1}{2^n} \right) \frac{x}{a+b} \geq \frac{x}{a}
$$

$$
\left( 1 + \frac{1}{2^n} \right) \frac{1}{a+b} \geq \frac{1}{a}
$$

$$
\left( 1 + \frac{1}{2^n} \right) a \geq a+b
$$

$$
\frac{1}{2^n} a \geq b
$$

$$
a \geq 2^{n}b
$$

And that is the precondition. So following the implications in reverse order is the proof of Lemma 1.

With this small lemma in our toolset, it's possible to find out the $\epsilon$ we are looking for. At first let's summarize, what we know about the numbers.

1. Real number $u$ is represented by floating-point number $u_f = s_u 2^{e_u}$
2. Real number $c$ is represented by floating-point number $c_f = s_c 2^{e_c}$
3. $1 \leq s_u < 2$
4. $1 \leq s_c < 2$
5. $ \left( s_u - 2^{-k-1} \right) 2^{e_u} \leq u \leq \left( s_u + 2^{-k-1} \right) 2^{e_u} $
6. $ \left( s_c - 2^{-k-1} \right) 2^{e_c} \leq c \leq \left( s_c + 2^{-k-1} \right) 2^{e_c} $
7. $u \geq c > 0$; if $u$ would be lower than $c$ it's certain that there is no integer $m$ that $u = mc$

And let's define $r = \frac{u}{c}$. Then following inequation is true

$$
\frac{\left( s_u - 2^{-k-1} \right) 2^{e_u}}{\left( s_c + 2^{-k-1} \right) 2^{e_c}} \leq
r \leq
\frac{\left( s_u + 2^{-k-1} \right) 2^{e_u}}{\left( s_c - 2^{-k-1} \right) 2^{e_c}}
$$

This is because the lowest possible $r$ is the lowest possible $u$ divided by the highest possible $c$. And the same way the highest possible $r$ is the highest possible $u$ divided by the lowest possible $c$.

Let's focus on the left side of the inequation.

$$
r \geq \frac{\left( s_u - 2^{-k-1} \right) 2^{e_u}}{\left( s_c + 2^{-k-1} \right) 2^{e_c}} =
$$

$$
\frac{s_u 2^{e_u}}{s_c 2^{e_c} + 2^{-k-1} 2^{e_c}} -
\frac{2^{e_u-k-1}}{s_c 2^{e_c} + 2^{-k-1} 2^{e_c}}
$$

Now we use Lemma 1 from our toolset. We substitute: $x = s_u 2^{e_u}$, $a = s_c 2^{e_c}$, $b = 2^{-k-1} 2^{e_c}$, and $n = k+1$. Keep in mind that $s_c \geq 1$, and therefore $a \geq 2^{k+1}b$.

$$
\frac{s_u 2^{e_u}}{s_c 2^{e_c} + 2^{-k-1} 2^{e_c}} -
\frac{2^{e_u-k-1}}{s_c 2^{e_c} + 2^{-k-1} 2^{e_c}} \geq
$$

$$
\frac{s_u 2^{e_u}}{s_c 2^{e_c}} -
\frac{1}{2^{k+1}} \cdot \frac{s_u 2^{e_u}}{s_c 2^{e_c} + 2^{-k-1} 2^{e_c}} -
\frac{2^{e_u-k-1}}{s_c 2^{e_c} + 2^{-k-1} 2^{e_c}} =
$$

$$
\frac{u_f}{c_f} -
\frac{2^{-k-1} s_u 2^{e_u} - 2^{e_u-k-1}}{s_c 2^{e_c} + 2^{-k-1} 2^{e_c}} =
$$

$$
\frac{u_f}{c_f} -
\frac{s_u 2^{e_u-k-1} - 2^{e_u-k-1}}{\left( s_c + 2^{-k-1} \right) 2^{e_c}} =
$$

$$
\frac{u_f}{c_f} -
\frac{\left( s_u - 1 \right) 2^{e_u-k-1}}{\left( s_c + 2^{-k-1} \right) 2^{e_c}}
$$

And we know that $s_u - 1 < 1$ and also $s_c + 2^{-k-1} > 1$. Therefore,

$$
\frac{u_f}{c_f} -
\frac{\left( s_u - 1 \right) 2^{e_u-k-1}}{\left( s_c + 2^{-k-1} \right) 2^{e_c}} >
$$

$$
\frac{u_f}{c_f} -
\frac{1 \cdot 2^{e_u-k-1}}{1 \cdot 2^{e_c}} =
$$

$$
\frac{u_f}{c_f} - 2^{e_u-e_c-k-1}
$$

We just proved that

$$
r \geq \frac{u_f}{c_f} - 2^{e_u-e_c-k-1}
$$

And in very similar way it is possible to prove that

$$
r \leq \frac{u_f}{c_f} + 2^{e_u-e_c-k-1}
$$

As some of my professors used to say: “We leave this proof as a homework for reader.”

Some of you may already see that, but to be clear, let's expand this expression.

$$
\frac{u_f}{c_f} \pm 2^{e_u-e_c-k-1} =
$$

$$
\frac{s_u 2^{e_u}}{s_c 2^{e_c}} \pm 2^{e_u-e_c-k-1} =
$$

$$
\frac{s_u}{s_c} 2^{e_u-e_c} \pm 2^{e_u-e_c-k-1} =
$$

$$
\left( \frac{s_u}{s_c} \pm 2^{-k-1} \right) 2^{e_u-e_c}
$$

Now it seems like dividing of two floating point numbers does not loose any precision. That it's possible to simply divide $u_f$ by $u_c$ and check if it is integer or not, exactly like we did with real numbers. However, that is not exactly true.

## Rounding off error

$\frac{s_u}{s_c}$ is rational number. It may require more than $k$ bits to be expressed exactly. Actually as rational number it may require infinite number of bits to be expressed exactly. But we have only $k$ bits. And that is the point, where the precision is lost. But how much precision can be lost? Let's have a look at floating-point result of expression $\frac{u_f}{u_c}$. The result should be

$$
r_f = f \left( \frac{s_u}{s_c} \right) 2^{e_u-e_c}
$$

where $f$ is a function that converts rational number to close enough floating-point number with significand of $k$ bits. And what is precision of that function? That depends on the implementation of the function. The best scenario is that the result is rounded to the nearest value of $k$ bits.

$$
f(x) = \mathrm{round}(x, k) = \frac{\bigl\lfloor x2^k + \frac{1}{2} \bigl\rfloor}{2^k}
$$

Or similar option is

$$
f(x) = \frac{\bigl\lceil x2^k - \frac{1}{2} \bigl\rceil}{2^k}
$$

Then the lost of precision is less than $2^{-k-1}$

$$
|f(x)-x| \leq 2^{-k-1}
$$

$$
\left| \frac{\bigl\lfloor x2^k + \frac{1}{2} \bigl\rfloor}{2^k} - x \right| \leq 2^{-k-1}
$$

$$
\left| \Bigl\lfloor x2^k + \frac{1}{2} \Bigl\rfloor - x2^k \right| \leq 2^{-1}
$$

There are 2 cases. When $ \lfloor x2^k + 2^{-1} \rfloor = \lfloor x2^k \rfloor $ then difference between $ \lfloor x2^k + 2^{-1} \rfloor $ and $ x2^k $ is less than $\frac{1}{2}$. And when $ \lfloor x2^k + 2^{-1} \rfloor = \lfloor x2^k + 1 \rfloor $ then difference between $ \lfloor x2^k + 2^{-1} \rfloor $ and $ x2^k $ is also less than $\frac{1}{2}$.

An attentive reader would notice that we didn't talk about normalization and its impact on the precision. Good thing is that it doesn't have any negative impact. When $s_u \geq s_c$ then $\frac{s_u}{s_c} \geq 1$, and thus the result is already normalized. When $s_u < s_c$ then $ 1 > s_r \geq \frac{1}{2} $ and it is possible to add one more bit to $s_r$ with precision $2^{-k-2}$. We can also look at it another way. Value of rounded significand is $ f \left( 2 \frac{s_u}{s_c} \right) $ and value of exponent is $e_u-e_c-1$. Then the precision is $ 2^{e_u-e_c-k-2} = 2^{e_r-k-1} $. 

$$
f \left( 2 \frac{s_u}{s_c} \right) 2^{e_u-e_c-1} - 2^{e_u-e_c-k-2} \leq \frac{s_u}{s_c} 2^{e_u-e_c} \leq f \left( 2 \frac{s_u}{s_c} \right) 2^{e_u-e_c-1} + 2^{e_u-e_c-k-2}
$$

$$
\left( f \left( 2 \frac{s_u}{s_c} \right) - 2^{-k-1} \right) 2^{e_u-e_c-1} \leq 2 \frac{s_u}{s_c} 2^{e_u-e_c-1} \leq \left( f \left( 2 \frac{s_u}{s_c} \right) + 2^{-k-1} \right) 2^{e_u-e_c-1}
$$

$$
f \left( 2 \frac{s_u}{s_c} \right) - 2^{-k-1} \leq 2 \frac{s_u}{s_c} \leq f \left( 2 \frac{s_u}{s_c} \right) + 2^{-k-1}
$$

And that is true, because for any $x$ (including $ 2 \frac{s_u}{s_c} $) following inequation is true

$$
f \left( x \right) - 2^{-k-1} \leq x \leq f \left( x \right) + 2^{-k-1}
$$

## Final error

And finally we should have a look at the relationship between floating-point result of division and integer that should exist, when the input is valid. This is, what we know about the floating-point result of division:

$$
\frac{u_f}{c_f} - 2^{e_u-e_c-k-1} \leq r_f \leq \frac{u_f}{c_f} + 2^{e_u-e_c-k-1}
$$

And we want to find an integer $r$ that satisfies

$$
\frac{u_f}{c_f} - 2^{e_u-e_c-k-1} \leq r \leq \frac{u_f}{c_f} + 2^{e_u-e_c-k-1}
$$

We can take the highest possible $r$ and the lowest possible $r_f$ (or the other way around the lowest possible $r$ and the highest possible $r_f$). Then it is easy to deduce that

$$
|r_f - r| \leq 2^{e_u-e_c-k} = 2^{e_r-k}
$$

## Implementation

We finally calculated that `acceptedError` is $2^{e_r-k}$. $e_r$ is exponent of the floating point number and that can be extracted using [Math.ILogB](https://learn.microsoft.com/en-us/dotnet/api/system.math.ilogb) function. And $k$ is precision of [Double](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-double) and that is 52 bits.

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

The function doesn't handle some exceptional cases, for example:

- Value or round-lot is positive or negative infinity or [NaN](https://learn.microsoft.com/en-us/dotnet/api/system.double.nan#system-double-nan)
- Round-lot is 0 or very near zero.
- Ratio is positive infinity, because the round-lot is very small number and the value is very large number.

However, handling of these inputs depends on your application.

This post presented accurate implementation of the round-lot validation. In the next post I will have a look, how does this work with [Decimal](https://learn.microsoft.com/en-us/dotnet/api/system.decimal) data type.

**Note**: It is also possible that the rounding function simply truncates bits that do not fit into the significand. Then the rounding function would be something like

$$
f(x) = \mathrm{round}(x, k) = \frac{\bigl\lfloor x2^k \bigl\rfloor}{2^k}
$$

Then precision of $s_r$ would be $2^{-k}$. And final range for our integer would be

$$
|r_f - r| \leq 2^{e_r-k+1}
$$

In such case `acceptedError` would be

```csharp
var acceptedError = Math.Pow(2, Math.ILogB(ratio) - 51);
```

This should cover platforms and architectures, where round-off error can be higher than just 1 bit. Especially in cases, when your application can accept lower precision. Although, I am not an expert on IEEE 754 standard and its implementations, so I am not sure if such problematic implementation exists.

**Note 2**: This post defined the problem in space of real numbers and explained the relationship between real numbers and floating-point numbers. It didn't use any root operations or any functions not compatible with rational numbers. And thus, the post applies also for definition of the problem in space of rational numbers and for this problem the same relationship applies to rational numbers and floating-point numbers.