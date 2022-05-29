using UnityEngine;
using System.Collections;
using System;
using System.IO.Ports;

public class gvsUpdatedController : MonoBehaviour
{
  SerialPort sp;
  string msg;


  [Range(0, 10)] [SerializeField] int portNum;
  [Range(0, 40)] [SerializeField] int currentAdjuster;
  int maxCurrent;

  int polarity;
  string commandString;
  string port;

  //Coefficients of torque
  [Range(0, 1)] [SerializeField] int a;
  private int b;

  // Mass and acceleration
  public GameObject hand;
  public GameObject head;
  public float massInGrams;
  public GameObject mass;
  private float handAcceleration;
  private Vector3 currHandPosition;
  private Vector3 lastHandPosition;
  private float massAcceleration;
  private Vector3 currMassPosition;
  private Vector3 lastMassPosition;

  // Booleans
  private bool contact;
  private bool grounded;

  // Head to arm radius
  public float shoulderOffset;
  private float length;

  // Torque values
  private float dynamicTorque;
  private float staticTorque;
  private float totalTorque;

  // Use this for initialization
  void Start()
  {
    // Instantiate variables for acceleration calculation
    Vector3 currHandPosition = Vector3.zero;
    Vector3 lastHandPosition = Vector3.zero;
    Vector3 currMassPosition = Vector3.zero;
    Vector3 lastMassPosition = Vector3.zero;

    // Instantiate torque coefficients
    b = 1 - a;

    // Port stuff

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
    } // Open sp
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
    } // Left right GVS

  }

  void FixedUpdate()
  {
    // Calculate acceleration of hand
    currHandPosition = hand.transform.position;
    handAcceleration = Mathf.Pow((currHandPosition.y - lastHandPosition.y), 2) / Mathf.Pow(Time.fixedDeltaTime, 2);
    lastHandPosition = currHandPosition;
    //Debug.Log("hand acceleration is " + handAcceleration);

    //Calculate acceleration of mass
    currMassPosition = mass.transform.position;
    massAcceleration = Mathf.Pow((currMassPosition.y - lastMassPosition.y), 2) / Mathf.Pow(Time.fixedDeltaTime, 2);
    lastMassPosition = currMassPosition;
    //Debug.Log("mass acceleration is " + massAcceleration);

    // Tracking head to hand
    length = head.transform.position.x - hand.transform.position.x + shoulderOffset;

    // Torque values
    dynamicTorque = massInGrams * handAcceleration * length;
    staticTorque = massInGrams * massAcceleration * length;
    totalTorque = a * dynamicTorque + b * staticTorque;
    Debug.Log("total torque is " + totalTorque);
  }
}
