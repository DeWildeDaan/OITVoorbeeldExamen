import paho.mqtt.client as mqtt
import json
import urllib.request
import sys
import random

def on_connect(client, userdata, flags, rc):
    pass
    #print("Connected with result code " +str(rc))

def on_message(client, userdata, msg):
    # print(msg.topic + " " + str(msg.payload))
    try:
        data = json.loads(msg.payload.decode("utf-8"))
        print(data)
    except:
        print("An error has occured!")
 
def menu(keuze):
    aantal = 0
    if keuze == 9:
        sys.exit()
    elif keuze == 1:
        aantal = random.randrange(100)
        print(f"sensor: water \taantal: {aantal} \tprijs:  \temail: {email}")
    elif keuze == 2:
        aantal = random.randrange(100)
        print(f"sensor: water \taantal: {aantal} \tprijs:  \temail: {email}")
 

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
    client.connect("13.81.105.139", 1883, 60)
    client.subscribe("/daandewilde", qos=0)
    client.loop_start()
    email = input("Geef je e-mail in om ver6der te gaan:\n")
    run()