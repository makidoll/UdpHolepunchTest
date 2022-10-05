import socket
import signal
import sys

port = 5971  # SQRL like squirrels haha

socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
socket.bind(("0.0.0.0", port))

closing = False


def interupt_signal(signal, frame=None):
    global closing
    if closing:
        return
    closing = True
    print("closing socket...")
    socket.close()
    sys.exit(0)


signal.signal(signal.SIGINT, interupt_signal)
if sys.platform == "win32":
    import win32api
    win32api.SetConsoleCtrlHandler(interupt_signal, True)

print("udp server listening at *:" + str(port))

# { [instance_id]: address }
hosts = {}


def address_to_bytes(address):
    return str.encode(address[0] + " " + str(address[1]))


while True:
    if closing:
        break

    message, address = socket.recvfrom(1024)

    if message == b'\x00':
        print("heartbeat " + str(address))

    elif message.startswith(b'host'):
        try:
            split = message.split(b' ')
            instance_id = split[1].decode("utf-8")
            print("new host for " + instance_id + " " + str(address))

            hosts[instance_id] = address

#             print("> sending host, host info...")
#             socket.sendto(
#                 address_to_bytes(address),
#                 address,
#             )

        except Exception as e:
            print("error: host failed")
            print(e)
            pass

    elif message.startswith(b'client'):
        try:
            split = message.split(b' ')
            instance_id = split[1].decode("utf-8")
            print("new client for " + instance_id + " " + str(address))

#             print("> sending client, client info...")
#             socket.sendto(
#                 address_to_bytes(address),
#                 address,
#             )

            print("> sending host, client info...")
            socket.sendto(
                address_to_bytes(address),
                hosts[instance_id],
            )

            print("> sending client, host info...")
            socket.sendto(
                address_to_bytes(hosts[instance_id]),
                address,
            )

        except Exception as e:
            print("error: client failed")
            print(e)
            pass
