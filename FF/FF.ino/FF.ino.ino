#include <EEPROM.h>
#include <SimpleTimer.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include <SPI.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define OLED_RESET 4
Adafruit_SSD1306 display(OLED_RESET);

// Data wire is plugged into pin 2 on the Arduino
#define ONE_WIRE_BUS 2
 
// Setup a oneWire instance to communicate with any OneWire devices 
// (not just Maxim/Dallas temperature ICs)
OneWire oneWire(ONE_WIRE_BUS);

// the timer object
SimpleTimer timer;

// Pass our oneWire reference to Dallas Temperature.
DallasTemperature sensors(&oneWire);


//Debug stuff
boolean debug = false; //Obviously turn on for serial debugging.

//Fridge mode (in development)
int fridgeTemp = 7;
int fridgePin = 7;
int fridgeToggle = 0;


//Code for it:
//  fridgeToggle = digitalRead(fridgePin); 
//  digitalWrite(LED_BUILTIN, fridgeToggle);


//Variables
unsigned int incomingByte = 0;
boolean isConnected = false;
boolean toggle1 = false;
int relay1 = 4;
int relay2 = 3;




int currentTemp = 0;
int upperLimit = (int)EEPROM.read(0) + (int)EEPROM.read(1);
int lowerLimit = (int)EEPROM.read(0) - (int)EEPROM.read(1);



void setup(){
  //Sets the baud rate.
  Serial.begin(9600);           // set up Serial library at 9600 bps

  display.begin(SSD1306_SWITCHCAPVCC, 0x3C);  // initialize with the I2C addr 0x3D (for the 128x64)
  
  //Set all pins for operation.
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(relay1, OUTPUT);
  pinMode(relay2, OUTPUT);
  pinMode(fridgePin, INPUT);

  //Initialize relays to off.
  digitalWrite(relay1, HIGH);
  digitalWrite(relay2, HIGH);

  //Set timer interval and attach it to checking function.
  timer.setInterval((int)EEPROM.read(2) * 1000, TempChecker); //Example timer (timer in mili, function to call)  

  // Clear the buffer.
  display.clearDisplay();
  display.setTextSize(3);
  display.setTextColor(WHITE);
  
}//end setup

//Initialize serial connection for inputting values.
void SerialInit(void){
    while(Serial.available()){Serial.read();}
      Serial.write((char)EEPROM.read(0));
      Serial.write((char)EEPROM.read(1));
      Serial.write((char)EEPROM.read(2));
      isConnected = true;
      delay(150);
  }

//Serial loop behaviour.
void SerialMode(void){
  
    if(Serial.available() > 2){
        digitalWrite(12, LOW);
        EEPROM.write(0, Serial.read());
        EEPROM.write(1, Serial.read());
        EEPROM.write(2, Serial.read());
        delay(500);
        Serial.write((char)EEPROM.read(0));
        Serial.write((char)EEPROM.read(1));
        Serial.write((char)EEPROM.read(2));
        while(Serial.available() > 0){Serial.read();}
      }
  
}

//Check the temp at the user specified intervals.  
void TempChecker(void){
    sensors.requestTemperatures();
    delay(100);
    display.clearDisplay();
    display.setCursor(0,10);
    currentTemp = sensors.getTempCByIndex(0);
    display.print(currentTemp);
    display.display();
    if(debug){
      Serial.print("Current temp: ");
      Serial.print(currentTemp);
      Serial.print(". Current max: ");
      Serial.print(upperLimit);
      Serial.print(". Current min: ");
      Serial.print(lowerLimit);
      Serial.println("");
    }
  }

//If the temp is outside the specified range, change relay state.
void TempCorrection(void){
  
    //To stop fridge starting when the temp is yet to be read.
    while(currentTemp == 0){timer.run();}
    
    if(currentTemp > upperLimit){
        digitalWrite(relay1, LOW);
        digitalWrite(relay2, HIGH);
      }
    else if(currentTemp < lowerLimit){
        digitalWrite(relay2, LOW);
        digitalWrite(relay1, HIGH);
      }
    else{
        digitalWrite(relay1, HIGH);
        digitalWrite(relay2, HIGH);
      }
  }


void loop() {
  //Checks to see if a serial connection has been established and triggers a flag
  if(Serial.available() > 0){
      SerialInit();
      while(isConnected){
        SerialMode();
      }
    }
  
  timer.run();
  TempCorrection();



}


