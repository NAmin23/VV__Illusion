using UnityEngine;
using System;
using System.IO.Ports;

public class gvsController : MonoBehaviour
{
  SerialPort sp;
  string msg;


  [Range(0, 10)] [SerializeField] int portNum;
  [Range(0, 40)] [SerializeField] int currentAdjuster;
  int maxCurrent;

  int polarity;
  string commandString;
  string port;


  //bool state = false;

  // Use this for initialization
  void Start()
  {
    port = "";
    // the_com = "/dev/tty.usbmodem14101"; // Serial
    port = "COM" + portNum.ToString(); // Serial1

    // Init variables
    currentAdjuster = 8;
    maxCurrent = 25 * currentAdjuster;

    sp = new SerialPort(port, 115200);
    if (!sp.IsOpen)
    {
      print("Opening " + port + ", baud 115200");
      sp.Open();
      sp.ReadTimeout = 15;
      sp.Handshake = Handshake.None;
      sp.DtrEnable = true;

      if (sp.IsOpen) { print("Open"); }
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (!sp.IsOpen)
    {
      sp.Open();
      print("opened sp again...");
    }
    if (sp.IsOpen)
    {

      if (Input.GetAxis("Axis 3") < 0) // left
      {
        polarity = 1;
        maxCurrent = 25 * currentAdjuster;
        Debug.Log("left");
      }
      else if (Input.GetAxis("Axis 3") > 0) // right
      {
        polarity = 2;
        maxCurrent = 25 * currentAdjuster;
        Debug.Log("right");
      }
      else
      {
        polarity = 0;
        maxCurrent = 0;
        print("OFF SIGNAL");
      }

      commandString = maxCurrent.ToString() + "," + polarity.ToString();

      sp.WriteLine(commandString);
      print("GVS Command: " + commandString);
    }



    /*
        if (sp.IsOpen)
        {
          if (Input.GetKeyDown(posButton) || Input.GetKeyDown(negButton))
          {
            state = !state;
            if (state)
            {
              // Do some finagling of the input command
              polarity = Input.GetKeyDown(posButton) ? 1 : 0;

              // polarity = GVSPolarity ? 1 : 0;
              commandString = EMSPulseFrequency + "," + MaxCurrent +
                  "," + PulseWidthMicroseconds + "," + polarity;


              print("ON TIME: " + Time.time +
                  "\n\tEMS Command: " + commandString);
              sp.WriteLine(commandString);
              sp.WriteLine("start");

              // msg = sp.ReadLine();
              // print("Read1: " + msg);
              // state = !state;
            }
            else
            {
              print("OFF SIGNAL");
              sp.WriteLine("stop");
            }
          }
        }
    */
  }
}
