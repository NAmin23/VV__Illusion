using UnityEngine;
using System.Collections;
using System;
using System.IO.Ports;

public class gvsUpdatedController : MonoBehaviour
{
  // Placed on hand
  SerialPort sp;
  string msg;


  [Range(0, 10)] [SerializeField] int portNum;
  [Range(0, 40)] [SerializeField] int currentAdjuster;
  int maxCurrent;

  int polarity;
  string commandString;
  string port;

  //Coefficients of torque
  [Range(0, 1)] [SerializeField] float a;

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

  // NEW mass and acceleration
  private Vector3 prevPosition;
  private float prevVelocity;

  // Head to arm radius
  public float shoulderOffset;
  private float length;

  // Torque values
  private float dynamicTorque;
  private float staticTorque;
  private float totalTorque;
  [Range(0, 2f)] [SerializeField] float k;
  [Range(0, 1000)] [SerializeField] int minThreshold;

  // Use this for initialization
  void Start()
  {

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
      //else if (Input.GetAxis("Axis 3") > 0) // right
      else if (Input.GetAxis("Axis 3") > 0) // right
      {
        polarity = 2;
        maxCurrent = Mathf.RoundToInt(totalTorque * k);
        if(maxCurrent >= 1000)
        {
          maxCurrent = 1000;
        }
        else if (maxCurrent < minThreshold)
        {
          maxCurrent = 0;
        }
        Debug.Log("right " + maxCurrent);
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
    if(mass != null)
    {
      prevVelocity = calculateVelocity(mass, prevPosition);
      massAcceleration = calculateAcceleration(calculateVelocity(mass, mass.transform.position), prevVelocity);

      // Tracking head to hand
      length = hand.transform.position.x - head.transform.position.x + shoulderOffset;

      // Torque values
      staticTorque = massInGrams * 9.8f * length; // Convert 9.8 to Unity units
      dynamicTorque = massInGrams * massAcceleration * length;
      totalTorque = a * dynamicTorque + (1 - a) * staticTorque;
      Debug.Log("static torque is " + (1 - a) * staticTorque + " and dynamic torque is " + a * dynamicTorque);
      //Debug.Log("total torque is " + totalTorque);

      // Update position and velocity
      prevPosition = calculatePosition(mass);
    }
  }

  Vector3 calculatePosition(GameObject contactObject)
  {
    return contactObject.transform.position;
  }

  float calculateVelocity(GameObject contactObject, Vector3 previousPosition)
  {
    return ((contactObject.transform.position.y - previousPosition.y) / Time.deltaTime);
  }

  float calculateAcceleration(float currentVelocity, float previousVelocity)
  {
    return ((currentVelocity - previousVelocity) / Time.deltaTime);
  }

  void OnTriggerEnter(Collider other)
  {
    if(other.GetComponent<objectTracker>().ungrounded == true)
     {
        mass = other.gameObject;
        other.GetComponent<objectTracker>().contact = true;
        massInGrams += other.GetComponent<objectTracker>().massInGrams;
     }
  }

  void OnTriggerExit(Collider other)
  {
    if (other.GetComponent<objectTracker>().contact == true)
    {
      mass = other.gameObject;
      other.GetComponent<objectTracker>().contact = false;
      massInGrams -= other.GetComponent<objectTracker>().massInGrams;
    }
  }
}
