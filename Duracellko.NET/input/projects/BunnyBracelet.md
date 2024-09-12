Title: Bunny Bracelet
Image: images/screenshots/BunnyBracelet.png
Order: 25
ShouldOutput: false
---
Bunny Bracelet program relays messages between multiple [RabbitMQ](https://www.rabbitmq.com/) instances over HTTP protocol.

![Bunny Bracelet example setup](images/bunny-bracelet-example-setup.svg)

Previous example shows setup of 2 RabbitMQ instances and 2 Bunny Bracelet instances. Program `Bunny Bracelet A` consumes messages from `RabbitMQ A` and forwards them to `Bunny Bracelet B` over HTTP protocol. Then program `Bunny Bracelet B` publishes the messages to `RabbitMQ B`.

Single Bunny Bracelet program can relay messages to multiple instances or from multiple instances.

**Project**: [https://github.com/duracellko/bunny-bracelet](https://github.com/duracellko/bunny-bracelet)
