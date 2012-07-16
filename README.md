OpenBus
=======

A publish/subscribe bus for the .NET platform.

Architecture:
- It follows a centralized message-broker model with the adapters residing locally at the application side. Therefore it is rather a MessageBus than Hub-And-Spoke.

Communication:
- Communication can be asynchronous or synchronous (request/response) and takes place via MSMQ over WCF services - thus it is transactional and persistent.
- Subscriptions are type-based with support for content-based filtering done at the Bus-side.
- Messages are in the form of objects (both generic and normal classes are supported).
- Uses custom serialization so more than the usual types are supported.
- Uses X.509 certificates for both encryption and authentication.

Libraries:
- OpenBus.Bus.dll contains the Bus. Is run as a service.
- OpenBus.BusWorker.dll contains the integration layer for applications to use.
- OpenBus.Messages.dll contains the messages. All messages that inherit (directly or indirectly) from BusMessage is a valid message to send. Both the bus and the application needs to reference this library.
- OpenBus.Common.dll contains helper classes and functions.


To do:
- Serialization of predicates for content-based filtering.
- Support for multiple instances of the bus application for better safety (i.e. replication facilitates resistance to failure).
