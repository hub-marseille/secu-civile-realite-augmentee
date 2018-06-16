#include <SoftwareSerial.h>

SoftwareSerial bt(3,2); // Arduino RX, TX
char c = ' ';
boolean NL = true;

void setup () 
{
  bt.begin(9600);
  Serial.begin(9600);
}


void loop() 
{
    // Read from the Bluetooth module and send to the Arduino Serial Monitor
    if (bt.available())
    {
        c = bt.read();
        Serial.write(c);
    }
 
 
    // Read from the Serial Monitor and send to the Bluetooth module
    if (Serial.available())
    {
        c = Serial.read();
        bt.write(c);   
 
        // Echo the user input to the main window. The ">" character indicates the user entered text.
        if (NL) { Serial.print(">");  NL = false; }
        Serial.write(c);
        if (c==10) { NL = true; }
    }
}
