Title: Quantum Computing - qubit
Published: 2025-10-07
Tags:
- Quantum computing
---
When I asked the question, "What makes quantum computation more powerful?" or "What kinds of problems can quantum computation solve efficiently?", the usual answer was, "It can run computations in parallel." While this is perhaps the most accurate single-sentence answer, I find it both inaccurate and misleading. A few years ago, I began my journey studying quantum computing. My background is in computer science, so my main interest was to understand the problem space that can be solved more efficiently by quantum computation. I did not study physics beyond high school, and my goal was not to delve deeply into quantum physics. In the next few blog posts, I will attempt to provide a better explanation to the question, "What kinds of problems can quantum computation solve efficiently?"

I began my self-study of quantum computing with the book [Quantum Computing for Everyone](https://direct.mit.edu/books/book/4186/Quantum-Computing-for-Everyone) by Chris Bernhardt. The book is a good, light introduction. It covers quantum states and quantum operations. However, it did not provide the insight I was seeking. Then I found another book, [Quantum Computation and Quantum Information](https://www.cambridge.org/highereducation/books/quantum-computation-and-quantum-information/01E10196D0A682A6AEFFEA52D53BE9AE) by Michael A. Nielsen and Isaac L. Chuang. This book was exactly what I needed to gain the insights I was looking for.

## Qubit

In this first blog post, I will discuss the qubit. The first book explained it quite well using a physical experiment with electron spin. I will use a different experiment with photons for explanation. Advantage of the photons experiment is that you can try it yourself with some inexpensive polarization filters. When you look through a polarization filter, it filters part of the light (some photons). The actual filtered amount depends on the source of light and the environment, but for simplicity, let's say it filters 50% of the light (50% of the photons). When you place the same polarization filter behind the first one, it does not filter any additional light. However, when you rotate that filter by 90°, the scene becomes black and all light is filtered. When you rotate the filter only 45°, then 50% of the light that passed through the first filter is filtered. The effect is shown in the following images.

![2 polarization filters with the same orientation](/images/posts/2025/10/polarization-filter-1.svg)
![2 polarization filters with 90° rotation](/images/posts/2025/10/polarization-filter-2.svg)
![2 polarization filters with 45° rotation](/images/posts/2025/10/polarization-filter-3.svg)

A very surprising part of the experiment is to rotate the second filter by 90°. That makes the scene black. Then, try inserting another polarization filter between the two and rotate it 45°. The surprising result is that it passes 25% of the light through. This is counterintuitive, because by adding the additional filter, less light is filtered and more photons pass through.

![3 polarization filters with 45° rotation](/images/posts/2025/10/polarization-filter-4.svg)

Let us return to the two-filter scenario. In this case, the more you rotate the filters toward 90°, the more photons are filtered and the scene becomes darker. The more you rotate toward 0°, the more photons pass through and the scene becomes lighter. The angle between the filters determines how much light is passed through and how much is filtered. That angle represents the **qubit**.

How does this work? The experiment with electron spin would be physically more accurate, but I will continue with photons, even though it is not strictly correct. The first filter allows only photons polarized in a certain direction to pass through. Recall the saying, "There is no up in space." That is what the second filter does: it defines where the "up" direction is. The following image should clarify this.

![Qubit direction](/images/posts/2025/10/qubit-direction.svg)

The blue arrow is the direction of polarization set by the first filter. The $x$ and $y$ axes represent the direction of the second filter. It is important to realize that it is possible to rotate the first filter, which means rotating the blue arrow in the $x$, $y$ coordinates. Alternatively, it is possible to rotate the second filter, which means rotating the $x$ and $y$ axes around the blue arrow. The result is the same, because the final amount of light passed through depends on the angle $\alpha$.

The angle $\alpha$ is significant. It determines how much light is passed through and how much is filtered. The probability that a photon passes through the second filter is $ \cos^2\alpha $. The probability that a photon is filtered out is $ \sin^2\alpha $. Note that $ \cos^2\alpha + \sin^2\alpha = 1 $ by Pythagorean theorem. Thus, it is certain that a photon is either filtered or passed through. The amount of light passed through is $ \cos^2\alpha $.

For example, when the filter is rotated 30°, then $ \alpha = \frac{\pi}{6} $ and the probability of a photon passing through is

$$
\cos^2\frac{\pi}{6} = \left(\frac{\sqrt{3}}{2}\right)^2 = \frac{3}{4}
$$

A qubit can be represented as a real number between $0$ and $2\pi$ (0° to 360°). This means that a qubit can have an infinite number of values, compared to a bit, which can have only two (0 or 1). Notice that the value can be greater than $\pi$, and thus $ \sin\alpha $ and $ \cos\alpha $ can be negative. However, the probabilities of passing and filtering ($ \cos^2\alpha $ and $ \sin^2\alpha $) are always positive numbers between 0 and 1.

![Qubit direction with angle higher than PI](/images/posts/2025/10/qubit-direction-2.svg)

## Qubit operations

Now the question is: what kinds of operations can be performed on a qubit? An important rule in quantum mechanics is that it should be possible to revert the operation. There are two types of operations that allow reverting. The first is **rotation**. This means adding any constant angle to a qubit. For example, the following image shows a qubit rotated by angle $\gamma$. In other words, it adds a constant $\gamma$ to qubit $\alpha$.

![Qubit rotation](/images/posts/2025/10/rotation.svg)

It should be clear that the operation can be reverted by subtracting the same angle.

The second type of operation is **reflection**. This means reflecting the direction line by another given direction line. For example, the following images present reflection by the y-axis, x-axis, and the diagonal line.

![Qubit reflection by y-axis](/images/posts/2025/10/reflection-y.svg)
![Qubit reflection by x-axis](/images/posts/2025/10/reflection-x.svg)
![Qubit reflection by diagonal line](/images/posts/2025/10/reflection.svg)

The revert operation is even simpler: repeating the same operation returns the qubit to its original state.

What kinds of operations cannot be reverted? For example, consider the conditional operation:

$$
f(x) =
\begin{cases}
  x+\pi, & \text{if } x<\pi\\
  x, & \text{otherwise}
\end{cases}
$$

The problem is that it is not possible to revert the value $ \frac{3}{2}\pi $, because

$$
f\left(\frac{3}{2}\pi\right) = f\left(\frac{\pi}{2}\right) = \frac{3}{2}\pi
$$

So there would be two possible results for the revert operation.

Even simple multiplication of the angle by a constant is not revertible. For example, multiplication by 2 cannot be reverted either.

$$
2\cdot\frac{\pi}{4} = 2\cdot\frac{5}{2}\pi = \frac{\pi}{2}
$$

in the space of values between $0$ and $2\pi$.

## Vectors

I used the angle representation of a qubit because it is very illustrative. However, the usual representation of a qubit is as a two-dimensional vector. That is, a tuple of two values, specifically $ (\sin\alpha, \cos\alpha) $. In the previous image, we presented the qubit as an arrow showing direction. However, the arrow shows only the direction; it does not have any significant length. So we can standardize all arrows to the same length of a single unit. Thus, a qubit can be represented by a point on the unit circle. The coordinates of such point are $ (\sin\alpha, \cos\alpha) $. That is the vector.

The vector representation of a qubit is very useful when combining multiple qubits. I will discuss that in the next post. I will try to avoid vectors as much as possible, as my goal is to explain quantum computing using elementary math.

## 3-dimensional space

The first book, "Quantum Computing for Everyone," explains the qubit as a direction in two-dimensional space. An advantage of this approach is its ease of illustration. However, we live in three-dimensional space, and thus a qubit is actually a direction in three-dimensional space. So we can think of it as a point on the unit sphere, instead of the circle. This point can be represented in different forms:
- Three-dimensional coordinates $ (x, y, z) $
- Two angles known as polar coordinates - you can think of these as GPS coordinates for points on Earth.
- Two-dimensional vector of complex numbers $C^2$

![Bloch sphere](/images/posts/2025/10/bloch-sphere.svg)

As you might expect, the most common representation of a qubit is the last one: a vector in $C^2$. Three-dimensional space also means that there are more possibilities for rotations and reflections, and thus more complex qubit operations.
