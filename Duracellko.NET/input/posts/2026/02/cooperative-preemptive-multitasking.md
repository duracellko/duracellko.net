Title: Cooperative vs Preemptive Multitasking
Published: 2026-02-05
Tags:
- Parallel algorithms
---
A few weeks ago I wrote about the ideas behind the origin of [async/await](/posts/2026/01/async-await) constructs in modern programming languages, and it reminded me that async/await revives an old pattern. The main goal of async/await frameworks is to provide a way to split a procedure into small tasks that can be scheduled on available processors. Soon after the first preview, it became clear that a task does not have to be only a piece of code running on a processor; it can be code running in another process, on another device, or even on another machine. Examples include a Remote Procedure Call (RPC), a database query, or a write request to a file system. In practice, these kinds of tasks are often more common than CPU-bound tasks, and async/await frameworks schedule them using a framework-level scheduler. This raises a question: why do we need an extra scheduler when modern operating systems already provide schedulers for threads and processes?

To answer that, let's look at older operating systems. In the early 1990s, before Mac OS was considered favorite choice for developers, the first operating systems supporting multitasking were introduced. That multitasking model was relatively simple and worked like this:

1. The operating system started a program.
2. The program then told the operating system:
    1. Please create a new window for me.
    2. When anything happens to the window (for example, the user clicks a button or resizes the window), call this callback function.
3. The program returned control to the operating system.
4. When an event occurred, the operating system invoked the program's callback function.
5. The callback handled the event and returned control to the operating system.
6. The operating system scheduled the next task.

![Cooperative Multitasking](/images/posts/2026/02/cooperative-multitasking.svg)

This type of multitasking is called *cooperative multitasking*. The operating system relies on the program to return control back to it. It may not look complicated today, but it was a big change in thinking for programmers who were used to having complete control over the computer at that time. The advantage of cooperative multitasking is that it has low overhead and is therefore very efficient. The disadvantage is that if a program does not return control to the operating system, the whole system becomes unresponsive. For example, if a program enters an infinite loop, the operating system will never get control back, and thus cannot handle any other programs. Same as today, programs had bugs that could cause such situations.

To solve this problem, *preemptive multitasking* was introduced. In preemptive multitasking, the operating system has a timer that interrupts running programs after a certain time slice. When interrupted, the operating system saves the state of the program, and then switches to another program. This way, even if a program does not return control back to the operating system, the operating system can still regain control after the time slice expires. This makes the system more robust and responsive. However, preemptive multitasking has higher overhead due to context switching and state saving.

![Preemptive Multitasking](/images/posts/2026/02/preemptive-multitasking.svg)

Keep in mind that the same programming model with window event handlers is still used on modern operating systems with preemptive multitasking. The difference is that the OS can interrupt the program at any time, not just when the program yields.

Now let's go back to async/await frameworks. Most applications using async/await frameworks assume that tasks are short-running and do not get stuck running indefinitely. Of course, there can still be bugs in the code and it is not possible to guarantee that a task will not get stuck. However, it is not expected that developers would write routines to handle such situations. In such cases, the usual approach is to restart the whole application. Thus, async/await frameworks can use a cooperative multitasking approach, where tasks are expected to return control back to the scheduler. This makes async/await frameworks more efficient, as they do not have to deal with frequent context switching and state saving.

It is interesting that a use case (cooperative multitasking) that was considered problematic in early operating systems and replaced by a more robust approach (preemptive multitasking) is now being used in modern async/await frameworks. This shows that sometimes, old ideas can be revived and adapted to new contexts, providing efficient solutions to modern problems.

Also, similar use cases can be seen in real-life scenarios. Constant meetings make software development less efficient because of the context switching. However, if stakeholders think that developers could get stuck on some tasks, then a more robust development process is worth the cost of context switching.
