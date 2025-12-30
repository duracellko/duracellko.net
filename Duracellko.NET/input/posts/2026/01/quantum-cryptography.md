Title: Quantum cryptography
Published: 2026-01-22
Tags:
- Quantum computing
- Computer networks
- Public Key infrastructure
---
In [the previous post](/posts/2025/10/quantum-computing-problems), I presented some computational problems that can be solved more efficiently by quantum computers. Today, however, computers are not only used for computations but also for solving various types of problems. In fact, most computer applications today perform minimal computation and primarily store, transfer, and transform data. Are there any non-computational problems that are difficult for classical computers but can be solved efficiently by quantum computers? In following posts, I will present examples of such problems in the field of computer networks.

## Qubit secret properties

Before presenting specific problems, let's examine some properties of qubits. As mentioned, a [qubit](/posts/2025/10/quantum-computing-qubit) is a rotation angle, which can also be represented by a unit vector.

![Qubit direction](/images/posts/2025/10/qubit-direction.svg)

[Measuring a qubit](/posts/2025/10/quantum-computing-measurement) requires specifying a basis or direction in which to measure the qubit. For example, in the picture above, the direction is specified by the x-axis and y-axis. The measurement will return either the x-axis or y-axis with certain probabilities. Thus, it is possible to observe one of two possible values. Interestingly, it is physically impossible to obtain the actual angle that represents the qubit's value. This is impossible for two reasons:

1. After measurement, the qubit's value collapses to either the x-axis or y-axis direction. The original value is lost, and it is not possible to perform further measurements to recover the original value.
2. Even if multiple measurements were possible, the angle is a real number with infinite precision. Therefore, an infinite number of measurements would be required to obtain the exact value.

This means a qubit can store a secret value that cannot be retrieved. Can this property be used to transfer secret information securely over a network?

## Quantum cryptography

Consider the following problem: Alice wants to send a secret message to Bob over an insecure network. An eavesdropper, Eve, can read all messages sent over the network. How can Alice and Bob communicate securely so that Eve cannot read the secret message? This problem is typically solved by [public key cryptography](/posts/2025/10/quantum-computing-problems#public-key-cryptography). Classical public key cryptography transforms the secret message using a function that is easy to compute, but whose inverse is hard to compute. A well-known example is the [RSA](https://en.wikipedia.org/wiki/RSA_cryptosystem) algorithm, which relies on the difficulty of factoring large numbers. Unfortunately, this problem is easy for quantum computers, as explained in the [Problems for Quantum Computing](/posts/2025/10/quantum-computing-problems) post. Therefore, the security of such cryptosystems is threatened by quantum computers. Additionally, there is no formal proof that the number factorization problem is truly hard. It is possible that future advancements in computer science may yield an efficient algorithm for classical computers, breaking the security of the cryptosystem.

Is it possible to send a secret message securely by transmitting qubits instead of classical bits? Before answering, note that public key cryptography is not used directly to send secret messages. Instead, it is used to exchange a secret key, which is then used to encrypt and decrypt actual messages using symmetric key cryptography, such as [AES](https://en.wikipedia.org/wiki/Advanced_Encryption_Standard). The real problem is securely exchanging a secret key over an insecure network. Public key cryptography algorithms are much slower and have limitations on message size (e.g., 125 bytes). Quantum cryptography also has limitations that prevent direct use of qubits for sending secret messages.

## BB84

The first quantum cryptography protocol was introduced in 1984 by Charles Bennett and Gilles Brassard, and is known as [BB84](https://en.wikipedia.org/wiki/BB84). Let's start with a single bit. Alice wants to send a random secret bit (0 or 1) to Bob. She generates another random bit to specify the basis rotation: either horizontal/vertical (HV) or diagonal (DA). She then prepares a qubit according to the following table:

| Secret bit | Basis bit | Qubit direction        |
|------------|-----------|------------------------|
| 0          | 0 (HV)    | Horizontal (90°) ➡️     |
| 1          | 0 (HV)    | Vertical (0°) ⬆️       |
| 0          | 1 (DA)    | Diagonal (45°) ↗️       |
| 1          | 1 (DA)    | Anti-diagonal (135°) ↘️ |

She sends the prepared qubit to Bob over the insecure network. Bob also randomly chooses a basis bit (HV or DA) and measures the received qubit using his chosen basis. If Bob's basis bit matches Alice's, he will obtain the correct secret bit with 100% probability. If the basis bits do not match, Bob will obtain either 0 or 1 with 50% probability. The following table summarizes the possible cases:

| Secret bit | Alice's Basis | Qubit | Bob's Basis | Measured Bit | Probability |
|------------|---------------|-------|-------------|--------------|-------------|
| 0          | 0 (HV)        | ➡️    | 0 (HV) ➡️⬆️  | 0            | 100%        |
| 0          | 0 (HV)        | ➡️    | 1 (DA) ↗️↘️   | 0 or 1       | 50% each    |
| 1          | 0 (HV)        | ⬆️     | 0 (HV) ➡️⬆️  | 1            | 100%        |
| 1          | 0 (HV)        | ⬆️     | 1 (DA) ↗️↘️   | 0 or 1       | 50% each    |
| 0          | 1 (DA)        | ↗️     | 0 (HV) ➡️⬆️  | 0 or 1       | 50% each    |
| 0          | 1 (DA)        | ↗️     | 1 (DA) ↗️↘️   | 0            | 100%        |
| 1          | 1 (DA)        | ↘️     | 0 (HV) ➡️⬆️  | 0 or 1       | 50% each    |
| 1          | 1 (DA)        | ↘️     | 1 (DA) ↗️↘️   | 1            | 100%        |

After Bob confirms his measurement, Alice sends her basis bit to Bob over the classical network. Bob compares his basis bit with Alice's. If the basis bits match, both keep the measured bit as part of the secret key. If not, they discard the measured bit. After repeating this process multiple times, Alice and Bob will have a shared secret key consisting of bits where their basis bits matched.

What if Eve intercepts and measures the qubits sent from Alice to Bob? She faces the same situation as Bob and must randomly choose a basis for measurement. For example, if Alice sends a horizontal qubit ➡️ and Eve measures it in the HV basis, she obtains the correct secret bit 0 and the qubit remains unchanged. However, if she measures it in the DA basis, she obtains either 0 or 1 with 50% probability, and the qubit changes to either diagonal ↗️ or anti-diagonal ↘️. When Eve then sends the qubit to Bob, there is a chance that Bob's measurement will not match Alice's original secret bit, even if his basis matches Alice's. This effect is explained in the experiment with three polarization filters in the [Quantum computing measurement](/posts/2025/10/quantum-computing-measurement) post.

After exchanging some secret bits via qubits, Alice and Bob should randomly select some bits and compare them over the classical network. If there is no Eve, all compared bits should match. If there is an eavesdropper, some bits will not match due to Eve's interference. If the number of mismatched bits exceeds a certain threshold, Alice and Bob can conclude that there is an eavesdropper and discard the entire key. Otherwise, they can use the remaining bits as their shared secret key.

Which bits to surrender and compare should be decided randomly after the key exchange to prevent Eve from predicting which bits to measure and which bits to leave untouched.

Compared to classical public key cryptography, BB84 allows exchanging secret keys of any length. If Alice and Bob want to exchange $N$ secret bits, they must exchange approximately $4N$ qubits: $2N$ qubits because half the time Bob's basis will not match Alice's, and another $2N$ qubits to surrender for eavesdropper detection. Additionally, they need to exchange $4N$ bits over the classical network to share basis bits and another $4N$ bits to compare the surrendered bits. This is a reasonable cost for securely exchanging secret keys of any length.

This means Alice and Bob can exchange a secret key of the same length as the message they want to send securely. They can then use [one-time pad](https://en.wikipedia.org/wiki/One-time_pad) symmetric key cryptography, which is very simple and theoretically unbreakable.

## Limitations

One important limitation is that the BB84 protocol requires online communication between Alice and Bob. Unlike public key cryptography, where Alice can encrypt a message and send it to Bob's mailbox for later decryption, BB84 requires both parties to be online simultaneously to exchange qubits and classical bits.

Another limitation is the need for authenticity of the classical bits. When Bob receives Alice's basis bits and the surrendered bits for comparison, he must be able to verify they are actually from Alice and not from Eve. Otherwise, Eve could impersonate Alice and send false bases and comparison bits, making it impossible for Bob to detect her presence. This problem can be solved by using classical public key cryptography to sign the classical messages. Digital signatures may be required anyway, so Bob can verify that the secret message is actually from Alice, and vice versa.

## Summary

I presented the problem of sending a secret message over an insecure network. This problem is reasonably solved by public key cryptography today. However, public key cryptography is not formally proven to be secure. Using quantum communication, it is possible to exchange a secret message that is formally proven to be theoretically unbreakable.
