Title: Quantum Computing measurement
Published: 2025-10-14
Tags:
- Quantum computing
---
This is the second post in my journey to discover "What kinds of problems can quantum computation solve efficiently?" In the [first post](./quantum-computing-qubit), I explained what a [qubit](./quantum-computing-qubit) is and how it can be represented. In this post, I will cover measurement in (not only) quantum mechanics. You have probably heard that when measuring a qubit, it collapses to one of the measured states and loses its actual value. This led me to some natural questions: What is the difference between measurement and any other operation? How does a qubit know it is being measured?

I already presented an example of a measurement experiment in the previous post, without explicitly mentioning it. Do you remember the experiment where we rotate two polarization filters 45° relative to each other? Only 50% of the light that passed through the first filter passes through the second filter. So what actually happens? Let us say the first polarization filter sets the polarization direction, and thus our qubit, to the blue arrow. The second filter then "measures" the qubit in the $x$ and $y$ axes.

![Qubit direction](/images/posts/2025/10/qubit-direction.svg)

What happens to the photon after passing through the second polarization filter? It changes its polarization to either the direction of the y-axis or the x-axis. The probability it is changed to the y-axis is $ \cos^2\alpha $, and the probability of changing to the x-axis is $ \sin^2\alpha $. In reality, the photon that is changed to the x-axis never passes through the filter and is destroyed. Therefore, the experiment with electron spin is more accurate, as the electron is not destroyed and the outcome is actually one or the other direction. However, photons are easier to observe by eyes.

Now it is possible to explain the experiment with three polarization filters. As mentioned, the first polarization filter sets polarization to the vertical direction, which is aligned with the y-axis. The second polarization filter measures polarization at a 45° rotation.

![Diagonal measurement](/images/posts/2025/10/diagonal-measurement.svg)

This second filter changes the polarization of 50% of the photons to the direction of $ y^\prime $ and the other 50% to $ x^\prime $, which does not pass through the filter. Now we have 50% of the light passing through the first filter and 50% of that passing through the second filter. That means 25% of the light passes through both filters, and all of this light has the polarization direction of the $ y^\prime $ axis. The third filter then measures again in the horizontal x-axis and vertical y-axis, similar to the first image. The outcome is that 50% of the photons reaching the third filter change polarization to the y-axis and 50% to the x-axis, which is again filtered out. Therefore, the total light passing through all three filters is 12.5%.

![3 polarization filters measurement](/images/posts/2025/10/3-filters-measurement.svg)

## Multiple qubits

With classical bits of values 0 or 1, it is possible to combine them to represent other numbers or values. For example, it is possible to combine 8 bits to represent an integer between 0 and 255. Is it possible to combine multiple qubits in the same way? The answer is "yes." But do we even need it? If a qubit can be any real number between $0$ and $2\pi$, then it can easily represent a number from a finite set of integers. For example, an integer $ i \in \{ 0, \ldots, 255 \} $ can be represented by a qubit as

$$
i \rightarrow \frac{i}{256} \cdot 2\pi
$$

Interestingly, a similar approach is used in classical computing. Most people say that computers communicate in 0s and 1s. However, that is not exactly true, although it is the best abstraction of the communication signal. While 0s (low voltage) and 1s (high voltage) are commonly used in processors and memory units (likely because it works well with transistors), network communication over cable or air uses more complicated encoding. Just like a qubit value, voltage can also be any real number. So it is possible to use multiple voltage levels to represent more numbers than just 0 and 1. This is especially important in communication. If the communication is at a certain frequency, e.g., 1 kHz, then it would be possible to transmit a maximum of 1000 bits per second with 2 levels. But introducing multiple energy/voltage levels increases the number of bits transmitted per second.

Why do we not use the same approach with qubits? The first reason is similar to classical computing: there are limited operations we can perform with a single qubit. It is possible to add numbers and perform reflections, but it is not possible to perform multiplications. The second reason is the limitation of measurement. As mentioned above, a measurement of a single qubit can yield one of two possible outcomes, and not an explicit value from a set of multiple integers.

## Schrödinger's cat

We have shown that a qubit measurement aligns the qubit with either the x-axis or y-axis. We can assign the value 0 to the x-axis and 1 to the y-axis. Thus, it is possible to think of a qubit as something between 0 and 1, and measurement reveals exactly one of the values 0 or 1.

Many people are probably familiar with the [Schrödinger's cat](https://en.wikipedia.org/wiki/Schr%C3%B6dinger's_cat) thought experiment, thanks to The Big Bang Theory sitcom. The thought experiment describes a cat in a box with poison. It is not possible to know if it is dead or alive without opening the box. The idea is to consider the cat both dead and alive at the same time before opening the box.

The same perception can be applied to a qubit. Instead of considering it as something between 0 and 1, we can consider that a qubit is both 0 and 1, each with a certain probability. This property of a qubit is called superposition of 0 and 1. This is another reason to combine multiple qubits. If we have 2 qubits, each in superposition of 0 and 1, then those 2 qubits are in superposition of values 0, 1, 2, 3. In general, $n$ qubits can be in superposition of numbers $ 0 \ldots 2^n-1 $. This is the reason why it is often explained that quantum computers can run computations in parallel. The problem, however, is that measurement always reveals only a single number. When running the same computation multiple times, the measurement reveals a different number each time. Therefore, I find the statement "quantum computers can run operations in parallel" a bit misleading.

Superposition is also the reason that a qubit is usually represented by a two-dimensional vector, instead of just a single angle. There is also a natural way to combine two-dimensional vectors into multi-dimensional vectors, so they can represent the superposition of multiple qubits.

## What is measurement?

Quantum mechanics describes measurement of a single qubit as a set of two operations on the qubit, where exactly one operation is chosen with a certain probability and that is applied. The difference from a regular qubit operation is that measurement is not a reversible operation. This partially answers the question, "How does the qubit know it is being measured?" or "What is the difference between measurement and a regular operation?" However, that still does not explain how it works.

The book [Quantum Computation and Quantum Information](https://www.cambridge.org/highereducation/books/quantum-computation-and-quantum-information/01E10196D0A682A6AEFFEA52D53BE9AE) provided me with valuable insight into this. I realized it has connections outside of quantum mechanics. There is [Goodhart's Law](https://en.wikipedia.org/wiki/Goodhart%27s_law): "When a measure becomes a target, it ceases to be a good measure." For example, when unit test code coverage is being measured, code coverage may become more important than code quality. So a measurement always impacts the system being measured. In some cases, the impact is significant, such as with the unit test code coverage metric. In other cases, the impact is negligible. For example, a car speedometer has almost no impact on the speed of the car, but it is still a non-zero impact. It is always a bit of an art, requiring a lot of experience, to design measurements that have minimal impact on the measured system. It is likely almost impossible in the quantum world, which is so small that even the introduction of a small system change required for measurement probably has a large impact. It is like a variation of [the 3rd Newton's law](https://en.wikipedia.org/wiki/Newton%27s_laws_of_motion#Third_law): For every action, there is some reaction.

Interestingly, quantum mechanics explains this very well. By measuring, we are usually adding something to the quantum system. For example, measuring a photon may require an electron that produces electric current when hit by the photon. So we can consider that a quantum system with measurement is like a system with additional qubit(s). When we combine possible measurement outcomes with changes on the added qubit(s), the combined operation is actually a regular operation that is revertible and does not seem counterintuitive.

It is extraordinary how something negligible in larger systems can explain the counterintuitive behavior of quantum measurement.
