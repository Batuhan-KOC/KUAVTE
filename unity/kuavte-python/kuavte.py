import sys
import os
import msvcrt
import threading
import time
import pygame
import socket
import struct

i_throttle = 0.0
i_pitch = 0.0
i_yaw = 0.0
i_roll = 0.0
i_valid = False
i_done = False

data_sending_ip = "127.0.0.1"
data_sending_port = 12346
client_socket = None

data_lock = threading.Lock()
done_lock = threading.Lock()

# Simulation Controller'ın beklediği input tipi
class GymEnvironmentInput:
    def __init__(self, initialize, windActive, frequency, targetSizing, throttle, pitch, yaw, roll):
        self.initialize = initialize
        self.windActive = windActive
        self.frequency = frequency
        self.targetSizing = targetSizing
        self.throttle = throttle
        self.pitch = pitch
        self.yaw = yaw
        self.roll = roll

# Bu fonksiyon sadece örnek amacı ile joystick okumasını sağlamak içindir
def joystick_loop():
    global i_throttle, i_pitch, i_yaw, i_roll, i_valid, i_done
    pygame.init()
    
    joystick = None
    clock = pygame.time.Clock()

    while True:
        done_lock.acquire()
        if i_done:
            done_lock.release()
            break
        done_lock.release()
        
        # CHECK FOR EVENTS
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                done_lock.acquire()
                i_done = True
                done_lock.release()
            if event.type == pygame.JOYDEVICEADDED and joystick is None:
                joy = pygame.joystick.Joystick(event.device_index)
                joystick = joy
                print(f"Joystick {joy.get_instance_id()} connected")
            if event.type == pygame.JOYDEVICEREMOVED and joystick is not None:
                joystick = None
                print(f"Joystick {event.instance_id} disconnected")
                data_lock.acquire()
                i_valid = False
                data_lock.release()

        if joystick is not None:
            data_lock.acquire()
            i_roll = joystick.get_axis(0)  # ROLL
            i_pitch = joystick.get_axis(1)  # PITCH
            i_throttle = joystick.get_axis(2)  # THROTTLE
            i_yaw = joystick.get_axis(3)  # YAW
            i_valid = True
            data_lock.release()

        # LOCK TO 30 FPS
        clock.tick(30)
        
    print("Exiting joystick loop")
    
    pygame.quit()
    
def send_input(input_data) -> None:
    # Convert GymEnvironmentInput to bytes
    input_bytes = struct.pack("=IBf?ffff", input_data.initialize, input_data.windActive,
                              input_data.frequency, input_data.targetSizing,
                              input_data.throttle, input_data.pitch, input_data.yaw, input_data.roll)
    
    try:
        # Send the data to the server
        client_socket.sendto(input_bytes, (data_sending_ip, data_sending_port))
        print("Data sent successfully!")

    except Exception as e:
        print(f"Error sending data: {e}")
        
    return

if __name__ == "__main__":
    joystick_thread = threading.Thread(target=joystick_loop)
    joystick_thread.start()
    
    # Create a UDP socket
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    
    while True:
        # If ESC pressed close application
        if msvcrt.kbhit() and msvcrt.getch() == chr(27).encode():
            i_done = True
            break
        # If pressed "s" send simulation start data
        elif msvcrt.kbhit() and msvcrt.getch() == chr(115).encode():
            print("Starting data")
            input = GymEnvironmentInput (initialize = 1,
                                         windActive = 0,
                                         frequency = 26,
                                         targetSizing = False,
                                         throttle = 0,
                                         pitch = 0,
                                         roll = 0,
                                         yaw = 0)
            send_input(input)
            
            input.initialize = 0
            
            send_input(input)
    
    client_socket.close()
    joystick_thread.join()