MISS: Mindstorms Integration Server for Snap!
=============================================

MISS is a system that integrates [http://snap.berkeley.edu/](Snap! environment) and Lego Mindstorms. It's going to consist of a HTTP server that connects to Mindstorms brick and a set of block that will serve as an extension to Snap!.

MISS is not ready yet; however, a [https://docs.google.com/document/d/1PIJKJBwxoZyq1vh7xxN6PZ6N5k2XbjYJ5npDYOm086E/edit?usp=sharing](design document) exists for those who are curious. Feel free to leave a comment inside. They will be appreciated (unless they are not about MISS, but about your cat).

Here is a brief excerpt from the design document that recaps what I'm trying to achieve:

* Enable Snap! users to control motors and read sensors.
* Support for Mindstorms EV3 on standard firmware.
* Support at least Mac OS and Windows, preferably also Linux.
* Future-proof API: Snap! programs created with older version of the blocks library should be able to run with newer versions of the MISS server (at least up to some point).
* Stable API that may be used with or without a supported set of Snap! blocks.
