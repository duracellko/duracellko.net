Title: Cooperative vs Preemptive Multitasking
Published: 2026-02-05
Tags:
- Parallel algorithms
---
Few weeks back I wrote about ideas behind origin of [async/await](/posts/2026/01/async-await) constructs in modern programming languages. And it reminded me that it's also an old pattern being revived back. The main goal of async/await frameworks is to provide a way to split a procedure into small tasks that can be scheduled on available processors. And soon after the first preview it was clear that a task does not have to be only a piece of code running on available processor. It can be a code running on another process, device, or even on another machine. For example, a Remote Procedure Call (RPC), or a database query, or write request to a file system. In the end these kinds of tasks are even more common than CPU-bound tasks. And all these tasks are scheduled by a scheduler provided by the async/await framework. The question is, what is a need for this extra scheduler? Every modern operating system already has a scheduler that schedules running tasks or threads. Could that be used instead?

To answer that, let's look at some older operating systems. In early 90s, or in times before Mac OS was considered good for developers, first operating systems supporting multitasking were introduced. This multitasking was relatively simple. It worked like this:

1. Operating system started a program.
2. Program then told the operating system:
    1. Please, create a new window for me.
    2. When anything happens to the window (for example, user clicks a button, ot resizes the window), call this callback function.
3. Program then returns control to the operating system.
4. On any event related to the window, operating system calls the callback function provided by the program.
5. The function handles the event, and when done, returns control back to the operating system.

![Cooperative Multitasking](/images/posts/2026/02/cooperative-multitasking.svg)

This type of multitasking is called *cooperative multitasking*. The operating system relies on the program to return control back to it. It may not look complicated today, but it was big change of thinking for programmers, who were used to have complete control over the computer at those times. Advantage of cooperative multitasking is that it has low overhead, and thus is very efficient. The disadvantage is that if a program does not return control back to the operating system, the whole system becomes unresponsive. For example, if a program enters an infinite loop, the operating system will never get control back, and thus cannot handle any other programs. And same like today, programs had bugs that could cause such situations.

To solve this problem, *preemptive multitasking* was introduced. In preemptive multitasking, the operating system has a timer that interrupts running programs after a certain time slice. When interrupted, the operating system saves the state of the program, and then switches to another program. This way, even if a program does not return control back to the operating system, the operating system can still regain control after the time slice expires. This makes the system more robust and responsive. However, preemptive multitasking has higher overhead due to context switching and state saving.

![Preemptive Multitasking](/images/posts/2026/02/preemptive-multitasking.svg)

Now let's go back to async/await frameworks. Most of the applications using async/await frameworks trust that the tasks are short-running and do not get stuck running infinitelly. Of course, there can be still bugs in the code and it is not possible to guarantee that a task would not get stuck. However, it is not expected that developers would write some routines to handle such situations. In such case, usual approach is to restart the whole application. Thus, the async/await frameworks can use cooperative multitasking approach, where tasks are expected to return control back to the scheduler. This makes the async/await frameworks more efficient, as they do not have to deal with context switching and state saving.

It is interesting that a use case (cooperative multitasking) that was considered problematic in early operating systems and replaced by more robust approach (preemptive multitasking) is now being used in modern async/await frameworks. This shows that sometimes, old ideas can be revived and adapted to new contexts, providing efficient solutions to modern problems.

Also similar use cases can be seen in the real life scenarios. Constant meetings make software development less efficient because of the context switching. However, if the stakeholders think that developers could get stuck on some tasks, then the robust development process is worth the cost of context switching.
