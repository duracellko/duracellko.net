Title: async/await question
Published: 2025-10-24
Tags:
- Parallel algorithms
---
When interviewing a candidate, I usually ask to tell me something about async/await as we know it in modern languages like C#, TypeScript. What does it do? Why do we use it? And people usually talks about technical details and threads. Some of the details are correct, some of them not. However, I have never heard anyone talked about theory or problem driving the inception of async/await concept.

At university one of the modules I studied was Parallel Algorithms. The module covered questions "What kind of problems can be solved more efficiently on multiple processors?" and "How can we design efficient parallel algorithm?" And the first lecture came up with question "Does number of available processors have impact on design of parallel algorithm?" This question si very important, because when tasked with designing parallel algorithm, we usually don't know how many processors will be available. And the answer to the question is "No". The number of processors doesn't have impact on design of parallel algorithm. How does that work? When designing parallel algorithm, we split the computation work into small tasks. Unfortunately those task may have dependencies one on another.

For example, consider algorithm to sum up numbers in array with length $N$. Then the algorithm creates $N$ tasks and each task adds value at specific index to previous task starting with value at index 0. $ T(i) = T(i-1) + A[i] $ where $ T(0) = A[0] $. Then final result it $ T(N-1) $. The problem is that each task depends on previous one. So there are no 2 tasks that can run in parallel. Following image presents tasks for array with length 5. The arrows represent dependencies between tasks.

![Sequential algorithm to sum up numbers in array](/images/posts/2025/10/sum-algorithm-sequential.png)

To take advantage of multiple processors, we need to change dependencies of the tasks. We can simply do that to make algorithm work in rounds. In the first round each task adds 2 values from the array. Then in the second round each task adds 2 values from results of previous round. And so on. The following image presents tasks for array with length 10. The arrows represent dependencies between tasks.

![Parallel algorithm to sum up numbers in array](/images/posts/2025/10/sum-algorithm-parallel.png)

Now how does this work with different number of processors? To run the algorithm, we need a scheduler that assigns tasks to processors. The scheduler can work quite simply. It takes tasks in order $ T(0), T(1), \ldots $ and assign each task to next available processor. However, task is scheduled only after all dependencies are finished. If there are $N$ processors available, then there is always a processor available for each task. If there are $m$ processors available, where $m<N$, then the scheduler assigns first $m$ tasks to processors. And as soon as any task is finished, it takes next task and assigns it to the available processor. This way we don't have to design parallel algorithm for specific number of processors. The scheduler maps any algorithm to specific number of processors.

And that is exactly what async/await frameworks do. They provide a programming language construct to split a procedure/function/method into small tasks. And then the framework implements a scheduler that assigns tasks to available processors or technically threads. So the framework provides abstraction for engineers to write parallel procedures without worrying about number of available processors.

Historically, processors used to have single core only. At some point in time this changed and multi-core processors became commercially available. So there was need for some framework to bring those multiple cores to software developers without knowing how many cores would the target system have. And that was motivation for creating async/await frameworks.
