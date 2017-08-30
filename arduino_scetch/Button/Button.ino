

// constants won't change. They're used here to
// set pin numbers:
const int buttonPin = 2;     // the number of the pushbutton pin
const int ledPin =  13;      // the number of the LED pin

int oldbuttonState = -1;

// variables will change:
int buttonState = 0;         // variable for reading the pushbutton status

void setup() {
  // initialize the LED pin as an output:
  pinMode(ledPin, OUTPUT);
  // initialize the pushbutton pin as an input:
  pinMode(buttonPin, INPUT_PULLUP);

  Serial.begin(9600);
}

void loop() {
  // read the state of the pushbutton value:
  buttonState = debounce(oldbuttonState);
  //Serial.println(buttonState);

  if (oldbuttonState != buttonState){
    oldbuttonState = buttonState;
    if (buttonState == HIGH) {
    // turn LED on:
    digitalWrite(ledPin, HIGH);
    Serial.println("btnHIGH");
    } else {
      // turn LED off:
      digitalWrite(ledPin, LOW);
      Serial.println("btnLOW");
    }
  }

  // check if the pushbutton is pressed.
  // if it is, the buttonState is HIGH:
}


// Процедура определения нажатия кнопки без дребезга:
int debounce(int last) { 
  int current = digitalRead(buttonPin); // считываем текущее состояние кнопки
  if (last != current) { // если состояние изменилось
    delay(20); // делаем задержку на 5 мсек, пока уляжется дребезг
    current = digitalRead(buttonPin); // и считываем снова
  }
  return current; // возвращаем текущее состояние кнопки
}

