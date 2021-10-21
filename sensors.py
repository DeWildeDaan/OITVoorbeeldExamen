import paho.mqtt.client as mqtt
import json
import urllib.request
import sys
import random

 
def menu(keuze):

 
    if keuze == 9:
        sys.exit()

 

def run():
    keuze = 0
    while keuze  != 9:      
        print("Maak uw keuze:")
        print("1. Test Water Sensor")
        print("2. Test Electriciteits Sensor")
        print("9. Exit")
        keuze = int(input())
        menu(keuze)
    
if __name__ == '__main__':
 
    client = mqtt.Client()
    client.on_connect = on_connect
    client.on_message = on_message
    client.connect("", 1883, 60)
    client.subscribe("")
    client.loop_start()
    run()