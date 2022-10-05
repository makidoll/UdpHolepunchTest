# UdpHolepunchTest

Testing UDP hole punching for @tivolispace

Host can connect to multiple clients, whilst client can only connect to one host

-   Run `server.py` on external server with open port `5971`

-   Modify `UdpHolepunchTest/UdpHolepunchTest.cs` to use your server

-   Run `dotnet run host instancename` on one machine
-
-   Run `dotnet run client instancename` on the other machine
