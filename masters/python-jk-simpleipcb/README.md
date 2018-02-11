jk_simpleipcb
=============

Introduction
------------

This module provides a way to attach other processes (possibly written in other programming languages) to the current python process in order to use functionality provided in these other processes.

Communication is performed by exchanging data frames over pipes. Processes attached can receive these frames, perform some tasks and then respond with a response data frame. As these frames can be of almost arbitrary size even exchanging larger amount of data is well supported.

Information about this module can be found here:

* [github.org](https://github.com/jkpubsrc/python-module-jk-simpleipcb)
* [pypi.python.org](https://pypi.python.org/pypi/jk_simpleipcb)

How to use this module
----------------------

### Import

To import this module use the following statement:

```python
import jk_simpleipcb
```

### Attaching a process

Before data can be exchanged the remote process needs to be launched. This is done invoking the method `launchComponentProcess()` which additionally requires a user defined name and information about the sending and receiving data formats expected:

```python
b = SimpleInterProcessCommunicationBridge()
b.launchComponentProcess("MyFancyComponent", "/path/to/other/executable", "str", "str")
```

The following data types are supported:

* `bin` : send/receive binary data
* `str` : send/receive a text string of arbitrary length (encoded in UTF-8)
* `json` : send/receive JSON serialized data

### Invoking a subroutine in the attached processed

If the communication link has been established we now can communicate with the attached process - here "MyFancyComponent" - and call a specific command provided by the atached process:

```python
ret = b.send("MyFancyComponent", "echo", "ThisIsATextExample")
```

On `send()` a single request frame is sent to the remote process and a single response frame is expected. As defined in `launchComponentProcess()` in this specific cas a JSON object is required as argument and a JSON object is expected to be returned by the command invoked within the attached process.

Example in C#
-------------

The main program rountine must instantiate the class `InterProcessCommunicationInterface` which will attach to STDIN and STDOUT and will from now on deal with all I/O including serialization and deserialization. A command handler must be registered, in this case providing the command "echo", which will receive and send UTF8 based data. The implementation will later on be done in a method named `HandleUTFEcho()`:

```C#
InterProcessCommunicationInterface ipci = new InterProcessCommunicationInterface();
ipci.RegisterHandler("echo", typeof(UTF8DataPacket), typeof(UTF8DataPacket), new MessageHandlerDelegate(HandleUTF8Echo));
ipci.Run();
```

Additionally an implementation for this echo-command must be provided:

```C#
public AbstractDataPacket HandleUTF8Echo(AbstractDataPacket pkg)
{
	// do nothing in this specific case; just return the data received for test purposes;
	return pkg;
}
```

The communication framework will deal with the data received and deserialize it into a packet of type `UTF8DataPacket` as defined in method `RegisterHandler()`. The method handling the command call issued by a controlling process will therefor receive an instance of `UTF8DataPacket` regardless of the message signature.

Performance
-----------

On a standard Linux system with a quad core i5-6600 at 3.30GHz the following performance can be achieved:

* Duration of a single request-response: ca. 0.0225 ms
* Number of request-response per second: ca. 44200

Communication is sufficiently fast for most computational purposes. It will primarily depend on the computation time within the attached processes.


General notes
-------------

Processes compatible to this kind of communication must be simple command line programs capable of reading binary data from STDIN and sending binary data using STDOUT in response. For convenience unpacking or packing data is wrapped in a simple library so that developers can focus on implementing functionality not data handling code.

Basically every kind of program that has this simple communication mechanism implemented can be attached to a controlling (python) process.

Current state of development
----------------------------

The software currently available is considerd to be in BETA stage. That means: It is fully operational in general but might still have some minor bugs or still lacks a little bit of small features here and there. It is perfectly safe to use the current code in projects but you might possibly be required to do some very small modifications in the final release.

Here is an overview about the current state of development:

| Programming language           | Controlling process                    | Attached process                       |
|--------------------------------|----------------------------------------|----------------------------------------|
| Python                         | yes - jk_simpleipcb (bin, str, json)   | no - planned                           |
| .Net/C#                        | no - planned                           | yes - LibSimpleIPCB (bin, str)         |
| Java                           | no                                     | no - planned                           |
| JavaScript                     | no                                     | no - planned                           |

Additional notes about the state of development:

* The communication protocol is considered to be stable.
* Some identifiers might be renamed in the future. Particularly:
    * mybe rename data format "str" to "utf8"
	* the name of the controller "server" code component and the name of the attached "client" code component
* The C# module does not yet support JSON
* The C# module does not yet autodetect calling conventions from signature
* No conventions yet how to gracefully shut down of an attached process
* Complete Java client port missing
* Complete JavaScript client port missing
* Maybe a general convention is adviced how to deal with exceptions in the attached components
* The current code is quite fine as it is very small but over the time more having better unit tests is adviced
* More documentation needed
* Currently argument and return value formats are defined on client module level. This should be specific to a method invoked, ideally by having the attached process providing the required information directly.

How to contribute
-----------------

The current functionality can basically be ported to any larger programming language, maybe even to some scripting languages. If you require a port in a new programming language feel free to port it yourself: This is not very difficult as it is only a very small piece of code and you will get support by the author(s) of this project to assist you.

Your advice is appriciated. Feel free to perform a code review and/or discuss the architecture, concepts, naming schemas, etc. with the author(s) of this project. We gladly welcome you to discuss all details in order to find ways for improvement.

Furthermore the author(s) of this project will be happy to learn about your experience in using this software in your own projects. Feel free to cooperate any time and let us know about things you find great as well as troubles you run into as well as features you would desire in the future.

If you have experience with open source projects and building communities around them and would like to support this project feel free to contact any of the author(s) as well.

Contact Information
-------------------

This is Open Source code. That not only gives you the possibility of freely using this code it also
allows you to contribute. Feel free to contact the author(s) of this software listed below, either
for comments, collaboration requests, suggestions for improvement or reporting bugs:

* Jürgen Knauth: jknauth@uni-goettingen.de, pubsrc@binary-overflow.de

License
-------

This software is provided under the following license:

* Apache Software License 2.0



