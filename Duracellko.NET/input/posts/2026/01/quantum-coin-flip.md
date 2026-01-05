Title: Quantum coin flipping
Published: 2026-01-29
Tags:
- Quantum computing
- Computer networks
---
[The previous post](./quantum-cryptography) showed that quantum networks can be as interesting as quantum computers. The main difference from classical networks is the ability to transmit qubits instead of classical bits. This post explores another interesting problem in network communication: Coin flipping.

The problem of coin flipping is to make a fair random decision between two choices. For example, Alice and Bob want to see a movie. Alice prefers the latest DC movie and Bob prefers the latest Marvel movie. They agree to flip a coin to decide which movie to watch. The problem is that they are in different parts of the city, so they cannot flip a physical coin together. Instead, they need to communicate over a network to agree on the coin flip result. The issue is that either Alice or Bob could cheat. For example, Alice could flip a coin and Bob announces _head_ as his guess. Alice could then simply say _tail_ even if the coin landed on _head_. Or, if Alice announces the coin flip result first, Bob could claim that he guessed it correctly no matter what Alice announced.

A solution could be to involve a trusted third party, Charlie. Alice and Bob can both send their guesses to Charlie, who flips a coin and announces the result. However, a trusted third party is not always available, or may not be trustworthy.

## Random choice

Before looking at a coin flipping protocol, let's consider a simpler problem: random choice or a random function. In the real world, it is possible to flip a coin and have it randomly land on head or tail, ideally with a 50% probability for each side. In other words, we want a function that randomly returns either 0 or 1. However, mathematically, all functions are deterministic. This means that for a given input, the function always returns the same output. A solution is to use a secret counter as input for the random function. The counter is incremented for each call, so the input is always different. Since the counter is secret, the output appears random to an outside observer. The function should be complex enough that an observer cannot deduce anything about the counter from previous outputs to predict future outputs. However, as the function has a finite number of instructions, there is a certain number of outputs after which it becomes possible to predict future outputs. Of course, it is desirable for this number to be large enough to be practically unreachable. Therefore, such functions are called pseudorandom functions or pseudorandom number generators.

Another solution is to introduce some randomness from outside the function. For example, today's CPUs have special hardware instructions that return random numbers based on physical processes inside the CPU. Such functions are called true random functions or true random number generators. These systems use physical noise that comes from outside the computer system, such as thermal or electrical noise.

Even for this simpler problem, quantum mechanics can help. A qubit can be prepared at a 45° angle (↗️). Then it is [measured](/posts/2025/10/quantum-computing-measurement) in the standard basis: x-axis (90°) and y-axis (0°). The measurement returns 0° or 90° with 50% probability each. We can assign bit 0 to 0° and bit 1 to 90°. Quantum measurement then returns a truly random bit.

## Classical coin flipping

As mentioned, if Alice reveals her coin flip result first, Bob can cheat by claiming that he guessed it correctly. If Bob reveals his guess first, Alice can cheat by announcing the opposite of Bob's guess. Therefore, Alice should reveal _something_ about the coin flip result. That _something_ must hide the actual result, so that Bob cannot deduce it and cheat. However, it must also be possible for Bob to verify that Alice did not change her mind when she reveals the actual result. In other words, Alice has to **commit to a value**. That _something_ may be, for example, a cryptographic hash function like [SHA-3](https://en.wikipedia.org/wiki/SHA-3). So the protocol can look like this:

1. Alice generates a large random number $x$ with at least as many bits as the output size of **SHA-3** (for example, 256 bits).
2. Alice sends the hash value $ h = \mathrm{SHA3}(x) $ to Bob.
3. Bob guesses if $x$ is even or odd and sends his guess to Alice. Notice that he cannot tell anything about $x$, because it is hard to compute the inverse of the hash function.
4. Alice sends $x$ to Bob. Bob computes the hash value and compares it with the value $h$ received in step 2. This way, he can verify that Alice did not change her mind.
5. If Bob's guess matches the parity of $x$, Bob wins; otherwise, Alice wins.

This protocol is secure as long as the hash function is secure. This means that it is hard to compute the inverse of the hash function and hard to find collisions (two different inputs that produce the same output). There may be other commitment functions based on other hard computational problems.

## Quantum coin flipping

Is it possible to use the secrecy of quantum states to implement a coin flipping protocol without relying on hard computational problems? The answer is yes, it is possible. The communication should follow this protocol:

1. Alice randomly chooses a single basis: either horizontal/vertical (HV) or diagonal/antidiagonal (DA).
2. Alice randomly chooses a string of bits and converts each bit to a qubit using the chosen basis.

| Secret bit | Basis     | Qubit direction        |
|------------|-----------|------------------------|
| 0          | 0 (HV) ➡️⬆️ | Horizontal (0°) ➡️     |
| 1          | 0 (HV) ➡️⬆️ | Vertical (90°) ⬆️      |
| 0          | 1 (DA) ↗️↘️ | Diagonal (45°) ↗️       |
| 1          | 1 (DA) ↗️↘️ | Anti-diagonal (135°) ↘️ |

3. Alice sends the qubits to Bob.
4. Bob randomly chooses a basis (HV or DA) for each qubit and measures them.
5. Bob guesses the basis used by Alice (HV or DA) and sends his guess to Alice. If the guess is correct, Bob wins; otherwise, Alice wins.
6. Alice reveals the basis she used and the original bits.
7. Bob takes the measurements from step 4 where his basis was the same as Alice's basis. He compares the measurement results with the original bits revealed by Alice. If they match, Bob accepts the result; otherwise, he accuses Alice of cheating.

The following example shows how the protocol works:

1. Alice chooses the horizontal/vertical basis (HV) - ➡️⬆️.
2. Alice chooses random bits 1, 0, 1, 1, 0 and encodes them to qubits: ⬆️➡️⬆️⬆️➡️.
3. Alice sends the qubits to Bob.
4. Bob measures the qubits in random bases:

| Row | Qubit | Bob's basis | Bob's measurement | Probability |
|-----|-------|-------------|-------------------|-------------|
|   1 | ⬆️     | HV ➡️⬆️      | 1                 | 100%        |
|   2 | ➡️    | HV ➡️⬆️      | 0                 | 100%        |
|   3 | ⬆️     | DA ↗️↘️       | 0                 | 50%         |
|   4 | ⬆️     | HV ➡️⬆️      | 1                 | 100%        |
|   5 | ➡️    | DA ↗️↘️       | 1                 | 50%         |

5. Bob guesses basis DA.
6. Alice announces that Bob guessed incorrectly and sends him the original bits.
7. Bob now takes his measurements from rows 1, 2, and 4, where his basis matched Alice's basis. He compares them with the original bits 1, 0, and 1. They match, so Bob accepts the result.

If Bob wants to cheat, he would need to find the actual angle of a qubit. For example, if Bob measures 1, the qubit could be either ⬆️ (0°) or ↗️ (45°). There is no way for Bob to distinguish between the two possibilities with a single measurement. Therefore, Bob cannot reliably guess Alice's basis.

Alice may have an even harder job cheating. If she wants to change her basis after Bob sends his guess, she would need to know all of Bob's measurements. For example, if Alice sent qubit ➡️ (90°) and wanted to convince Bob that it was diagonal ↗️ or ↘️, Bob would measured it in the DA basis and he could have obtained 0 or 1 with 50% probability. There is no way for Alice to know which one Bob obtained. Therefore, she cannot reliably change her basis and original bits to match Bob's measurements.

## Network routing

I presented the problem of sending a secret message and coin flipping over a quantum network. I could present the protocols using a simple angle representation of qubits without needing knowledge about vector spaces or matrices. However, you may notice that both protocols require a direct line to send qubits between the parties. This is rarely the case with classical networks, where messages are routed through multiple intermediate nodes, e.g., through a WiFi router, ISP, Internet backbone, or mail server. It is possible to route qubits through multiple intermediate nodes as well. The intermediate nodes would need to perform [quantum teleportation](https://en.wikipedia.org/wiki/Quantum_teleportation) to forward the qubits to the next node. This is out of scope for this post, as it requires understanding of [quantum entanglement](https://en.wikipedia.org/wiki/Quantum_entanglement). The problem is that quantum teleportation requires transferring classical bits that hold certain information about the qubit being teleported. I suspect that this information could leak some details about the qubit, which could compromise the security of the protocols.
