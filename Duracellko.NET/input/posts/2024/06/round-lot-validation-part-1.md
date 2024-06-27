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

In more exact terms the validation defines input as valid, when a user's input number is $u$ and a configured round-lot is $c$, then there must exist an integer number $m$, so that $u = mc$; or slightly transformed $m = u/c$. In other words the following equation must be true: $u/c = \lfloor u/c \rfloor$. While this is simple and satisfying definition in the space of real numbers, it does not hold in space of floating-point numbers.

## Floating-point numbers

At first let's talk about difference between real numbers and floating-point numbers. For the rest of this post we can consider that all numbers are not negative. I think it is quite clear that it's possible to do the validation on absolute values of $u$ and $c$, so sign of the numbers is not relevant. Floating-point numbers are represented by a significand $s$ and an exponent $e$. Then the expressed number is $s2^e$. Significand is a rational number, but with limited precision. And the exponent is an integer. For example single precission floating-point numbers have significand of size 11 bits and exponent of size 4 bits. When bits of significand are $s_1, s_2, \dots s_{11}$ then the number represented by the bits is

$$
\left(1 + \sum_{i=1}^{11} s_{i}2^{-i} \right) 2^e
$$

Notice that $1 \leq s < 2$. This format of floating-point numbers is called normalized. Or it's said that a floating-point number is normalized, when it is stored in this format. Normalized floating point numbers ensure that there is exactly 1 representation of each number. For example $0.011 \cdot 2^{1}$ and $1.1 \cdot 2^{-1}$ are the same numbers, but only the latter one is normalized. We will use this property later.

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

It's possible to see that it is very likely that the pattern $0011$ would repeat itself to infinity. So it would be needed infinite number of bits to represent decimal number 1.1 in binary. It should be clear that there exist multiple real numbers, which would be represented by the same floating-point number with specific precision. It is possible to define relationship between the floating point numbers and real numbers. The relationship is that floating-point significand $s$ represents interval of real numbers

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

Now after some understanding of floating-point numbers and their relationship to real numbers we can get back to our problem. The problem is to find out if there exists any integer number $r$ that

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

## Rounding off

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

## Conclusion

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

Now we know, what is the range that we have to look for our integer. In the next part I will show, how to use this knowledge to implement the validation function.

**Note**: It is also possible that the rounding function simply truncates bits that do not fit into the significand. Then the rounding function would be something like

$$
f(x) = \mathrm{round}(x, k) = \frac{\bigl\lfloor x2^k \bigl\rfloor}{2^k}
$$

Then precision of $s_r$ would be $2^{-k}$. And final range for our integer would be

$$
|r_f - r| \leq 2^{e_r-k+1}
$$
