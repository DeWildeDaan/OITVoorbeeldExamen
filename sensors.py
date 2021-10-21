import paho.mqtt.client as mqtt
import paho.mqtt.publish as publish
import json
import requests
import sys
import random
global eprijs
global wprijs

def on_connect(client, userdata, flags, rc):
    print("Connected with result code " +str(rc))

def on_message(client, userdata, msg):
    # print(msg.topic + " " + str(msg.payload))
    try:
        data = json.loads(msg.payload.decode("utf-8"))
        print(data)
    except:
        print("An error has occured!")

def get_prices():
    global eprijs
    global wprijs
    #response = requests.get("")
    #print(response.json())
    eprijs = 1.0
    wprijs = 1.0
 
def menu(keuze):
    global eprijs
    global wprijs
    
    aantal = 0
    if keuze == 9:
        sys.exit()
    elif keuze == 1:
        aantal = random.randrange(100)
        prijs = aantal * wprijs
        payload = {
            "Sensor": "water",
            "Amount": aantal,
            "Price": prijs,
            "EMail": email
        }
        publish.single("/daandewilde", json.dumps(payload).encode("utf-8") , hostname="13.81.105.139")
    elif keuze == 2:
        aantal = random.randrange(100)
        prijs = aantal * eprijs
        payload = {
            "Sensor": "electricity",
            "Amount": aantal,
            "Price": prijs,
            "EMail": email
        }
        publish.single("/daandewilde", json.dumps(payload).encode("utf-8") , hostname="13.81.105.139")
 
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
    email = input("Geef je e-mail in om verder te gaan:\n")
    get_prices()
    run()