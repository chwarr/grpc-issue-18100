# Failure to start a Server leaves some ports in zombie state 

This repository demonstrated gRPC C# [issue #18100][18100], "Failure to
start a Server leaves some ports in zombie state". As of gRPC C# 2.32.0,
this issue has been fixed.

As of September 14, 2020, this repository is archived.

## What version of gRPC and what language are you using?

Tested with 

* C# 1.19.0-pre1
* C# 2.27.0
* C# 2.28.1

## What operating system (Linux, Windows, …) and version?

Windows 10

## What runtime / compiler are you using (e.g. python version or version of gcc)

Tested with 

* netcoreapp2.1 (1.19.0-pre1)
* netcoreapp3.1 (2.27.0)
* netcoreapp3.1 (2.28.1)

## What did you do?

Attempted to start a server with two ports: one that could be bound and one
that was unable to be bound (due in this case to invalid TLS certificates,
but can also occur if the port is already in use).

## What did you expect to see?

Neither of the two ports should be open after the call to `Server.Start()`.
Requests to either one would fail to connect at the TCP layer.

I'd be OK if the ports that didn't fail to bind responded to every call with
`UNIMPLEMENTED` or `UNAVAILABLE`.

## What did you see instead?

The first port was opened successfully, can be connected to, but never
responded to gRPC messages.

## Mitigation

Explicitly calling `Server.ShutdownAsync()` or `Server.KillAsync()` shutdowns
down the zombie port.

## Possible fix

I think the fix is to add call to `Server.ShutdownInternalAsync()` around when
the port binding failure is detected.

## Details

The program in this repository reproduces the problem. Run it with `dotnet
run`.

Things to notice:

* The TLS failure is reported as expected.
* `Server.Start()` throws an IOException.
    * This program intentionally tries to connect after seeing that exception.
* The client can connect to the bound port.
* All the client's writes are successful.

[18100]: https://github.com/grpc/grpc/issues/18100
