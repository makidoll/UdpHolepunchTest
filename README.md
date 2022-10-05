# UdpHolepunchTest

Testing UDP hole punching for https://github.com/tivolispace/tivolispace/

Host can connect to multiple clients, whilst client can only connect to one host

-   Run `server.py` on external server with open port `5971`

-   Modify `UdpHolepunchTest/UdpHolepunchTest.cs` to use your server

-   Run `dotnet run host instancename` on one machine

-   Run `dotnet run client instancename` on the other machine

## Todo

- Add relay system

- If host/client IP is default gateway, then get server's public IP

## Observations

- Cellular network does not work. Should it? Using a VPN is a good way to test for now.
