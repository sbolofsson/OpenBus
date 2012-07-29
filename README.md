OpenBus
=======

A publish/subscribe bus for the .NET platform.

Architecture:
- Centralized message-bus.

Communication:
- Communication can be asynchronous or synchronous (request/response).
- Communication takes place via MSMQ over WCF services (transactional and persistent).
- Subscriptions can both be type- and content-based. The type-based filtering is done at the bus-side whereas the content-based filtering is done client-side.
- Messages are in the form of objects (generics are also supported).
- Uses custom serialization so more than the usual types are supported.
- Uses X.509 certificates for both encryption and authentication.

Libraries:
- OpenBus.Bus.dll contains the bus.
- OpenBus.BusWorker.dll contains the integration layer for applications to use.
- OpenBus.Messages.dll contains the messages. All messages that inherit (directly or indirectly) from BusMessage is a valid message to send. Both the bus and the application needs to reference this library.
- OpenBus.Common.dll contains helper classes and functions.


To do:
- Support for multiple instances of the bus application for better safety.
