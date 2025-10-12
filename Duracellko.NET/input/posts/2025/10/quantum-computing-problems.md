Title: Problems for Quantum Computing
Published: 2025-10-28
Tags:
- Quantum computing
---
In [the previous post](./quantum-computing-nondeterminism), I explained nondeterministic Turing machines and how quantum computers can simulate them for certain problems. It was mentioned that simulation is possible using Quantum Fourier Transform when solutions are periodic. In this post, I will present some examples and other use cases of quantum computing.

The previous post already mentioned an example of a problem that can be efficiently solved by a quantum computer: **Order finding**. This is the problem of finding the smallest integer $r$ such that $ x^r = 1 \mod N $, where $x$ and $N$ are given integers. Probably the most famous application of order finding is [Integer factorization](https://en.wikipedia.org/wiki/Integer_factorization). This is the problem of finding prime factors for a given integer $N$, meaning finding prime numbers $ q_1, q_2, \ldots, q_n $ such that $ q_1 q_2 \ldots q_n = N $. There is a quantum algorithm called [Shor's algorithm](https://en.wikipedia.org/wiki/Shor%27s_algorithm) that can solve integer factorization efficiently by reducing it to order finding.

Another example of a problem that can be efficiently solved by a quantum computer is **Discrete logarithm**. This is the problem of finding an integer $k$ such that $ g^k = x \mod p $, where $g$, $x$, and $p$ are given integers, and $p$ is prime. This problem can be solved by a modification of Shor's algorithm.

## Public key cryptography

Shor's algorithm has a significant impact on information security, specifically on public key cryptography. The most common use of public key cryptography is [Transport Layer Security](https://en.wikipedia.org/wiki/Transport_Layer_Security) (TLS), which is used to secure communication over the Internet. Public key cryptography solves two main problems: secure key exchange and digital signatures. Secure key exchange provides confidentiality in information security by allowing two parties to agree on a shared secret key over an insecure channel. Digital signatures provide integrity by allowing one party to sign a message in a way that anyone can verify the signature, but only the signer can create it. It means that a sender of a message cannot deny having sent the message.

Currently, the most common cryptosystems in public key cryptography are [RSA](https://en.wikipedia.org/wiki/RSA_cryptosystem) and [Elliptic-curve cryptography](https://en.wikipedia.org/wiki/Elliptic-curve_cryptography). The security of RSA is based on the assumption that integer factorization is a hard problem, and elliptic-curve cryptography assumes that the discrete logarithm is a hard problem. While it is believed that these problems are hard to solve on classical computers, Shor's algorithm solves them efficiently using quantum computers. This means that current computer systems secured by public key cryptography are vulnerable to quantum computer attacks. At the moment, quantum computers do not have enough qubits to match the key sizes used by current cryptosystems. However, it is expected that quantum computers will become powerful enough in the future.

Therefore, there is an ongoing effort to develop and standardize new cryptographic algorithms that are secure against quantum computer attacks. This field is called [Post-quantum cryptography](https://en.wikipedia.org/wiki/Post-quantum_cryptography). [FIPS 203](https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.203.pdf) standard has been published and it specifies post-quantum cryptographic algorithms for both secure key exchange and digital signatures. These cryptosystems are based on module-lattice based problems. These problems were studied in the field of Machine Learning and are believed to be hard to solve even for quantum computers.

## Period finding

Order finding is a special case of a more general problem called **Period finding**. This is the problem of finding the period $r$ of a periodic function $f(x)$ such that $ f(x) = f(x + r) $ for all $x$. The function is given as a black box, so the only way to get its value is to call the black box with some input. The goal is to find the period $r$ using as few calls to the black box as possible. It should be clear that order finding is a special case of period finding, where the function is defined as $ f(x) = a^x \mod N $ for a given $a$.

Like order finding, period finding can also be solved efficiently using quantum computers. The algorithm using Quantum Fourier Transform can be generalized to achieve this.

A generalization of the period finding problem is the [Hidden subgroup problem](https://en.wikipedia.org/wiki/Hidden_subgroup_problem). Unfortunately, it is not known how to efficiently solve all instances of the hidden subgroup problem using quantum computers, and it is believed that some instances are hard to solve even for quantum computers. For example, [Learning with errors](https://en.wikipedia.org/wiki/Learning_with_errors) problem can be reduced to some instances of the hidden subgroup problem, and it is believed that learning with errors is hard to solve even for quantum computers. Learning with errors is the basis of some post-quantum cryptographic algorithms, and it likely would not be considered for post-quantum cryptography if it were not believed to be hard for quantum computers.

## Quantum search

In the previous post, I explained that a nondeterministic computing machine can branch its calculation into multiple paths. While classical computers have to evaluate all the paths, quantum computers can find the right path efficiently with high probability in certain cases.

![Nondeterministic computation](/images/posts/2025/10/nondeterministic-computation.png)

There is another approach to simulation of nondeterministic computing machines using quantum computers. This approach is called quantum search and it uses [Grover's algorithm](https://en.wikipedia.org/wiki/Grover%27s_algorithm). The result is not as impressive as using Quantum Fourier Transform for periodic problems, but it is still better than using classical computers. Quantum Fourier Transform reveals the right path and is equivalent to the computation of a single path. However, Grover's algorithm requires multiple runs and is equivalent to the computation of multiple paths. Specifically, it requires $\sqrt{N}$ computations, where $N$ is the number of all possible paths.

For example, consider the problem of finding a secret key for [Advanced Encryption Standard](https://en.wikipedia.org/wiki/Advanced_Encryption_Standard) (AES) with a key size of 256 bits. There are $2^{256}$ possible keys, and a classical computer would have to try on average half of them, which is $2^{255}$. A quantum computer using Grover's algorithm would require only $ \sqrt{2^{256}} = 2^{128} $ tries. So quantum computers effectively halve the size of the secret key. AES is still considered secure for post-quantum cryptography, but it is recommended to use AES with a key size of 256 bits or more.

Unfortunately, I did not find a simple illustration or analogy to present quantum search or Grover's algorithm. Books and the Wikipedia article has some geometric explanation, but that requires some knowledge of vector spaces.

## Summary

While a smart way has been found to simulate some nondeterministic Turing machines efficiently using quantum computers, it is believed that quantum computers cannot simulate all nondeterministic Turing machines efficiently in general, so there are hard problems that cannot be efficiently solved by quantum computers.

So far, the biggest success in quantum computing is showing that quantum computers can efficiently find the period of a periodic function, when finding the solution using classical computers would require calculating the function for almost all possible inputs. It is possible that there are other kinds of problems that can be efficiently solved by quantum computers, but they are not known yet. As quantum computations work quite counterintuitively, it requires a lot of ingenuity to find new approaches for quantum algorithms.
