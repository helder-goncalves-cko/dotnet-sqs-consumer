# dotnet-sqs-consumer

This boilerplate consists of a Console application designed to consume messages pushed to an [SQS](https://aws.amazon.com/sqs/) queue.

The application is built using [Proto.Actor](https://github.com/AsynkronIT/protoactor-dotnet) for super fast processing :)
It also logs to Serilog and has both Console and [Seq](http://getseq.net) sinks configured.


## Project contents

### Actors

- `Dequeuer.cs` - Actor responsible for dequeueing commands from the queue
- `Deleter.cs` - Actor responsible for deleting commands in the queue after they've been processed
- `Dispatcher.cs` - Sample actor responsible for processing a specific commmand (ie. send an HTTP request)

### Factories
- `ActorFactory.cs` - Class responsible for instantiating or getting an actor
- `MessageMapper.cs` - Class responsible for mapping an `SQSCommand` to a message that can be handled by an actor

## Quickstart

To bring up your containers:

```
docker-compose up --build
```

This will bring up an instance of Seq, alpine SQS and the Consumer.
