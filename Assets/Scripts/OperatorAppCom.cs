using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tango;

using TechTweaking.Bluetooth;

public class OperatorAppCom : MonoBehaviour, ITangoLifecycle, ITangoDepth
{
	
  private BluetoothDevice device;
  public string bluetoothName = "P023";
  private const string UUID = "0acc9c7c-48e1-41d2-acaa-610d1a7b085e";

  public TangoPointCloud pointCloud;
  private TangoApplication tangoApplication;

  private bool findPlaneWaitingForDepth = false;
  public GameObject[] toPlace;

  private string content = "";
  private List<int>	toPlaceLater = new List<int>();
  private List<Vector3> toPlaceCoordinates = new List<Vector3>();
  // Use this for initialization
  void Start()
  {
    Debug.Log("BT operator start\n");
    BluetoothAdapter.enableBluetooth();//Force Enabling Bluetooth
    device = new BluetoothDevice();
    /*device.Name = bluetoothName;// TODO nom du bluetooth de l'operateur
		device.UUID = UUID;
		device.connect();
		if (device.IsConnected) {
			Debug.Log ("Ok BT op\n");
			device.send(System.Text.Encoding.ASCII.GetBytes("Hello\n"));
		}
		else
			Debug.Log("KO BT op\n");
		*/
    BluetoothAdapter.OnClientRequest += HandleOnClientRequest;//listen to client remote devices trying to connect to your device
    BluetoothAdapter.startServer(UUID);
    tangoApplication = FindObjectOfType<TangoApplication>();
    tangoApplication.Register(this);
    StartCoroutine(PlaceLater());

  }

  void Update()
  {
    /*
		if (!device.IsConnected) {
			Debug.Log ("Trying to reconnect...");
			device.connect ();
			if (device.IsConnected) {
				Debug.Log ("Ok BT op\n");
				device.send (System.Text.Encoding.ASCII.GetBytes ("Hello\n"));
			}
		}
		if (device.IsReading) {
			byte [] msg = device.read ();
			if (msg != null ) {
				content += System.Text.ASCIIEncoding.ASCII.GetString (msg);
				if (content.Contains("a")) {
					PlaceObject ();
					content = "";
				}
			} 
		}
		*/
  }

  private IEnumerator PlaceObject(Vector3 position, int idObject)
  {
    position.y = 0; 
    findPlaneWaitingForDepth = true;

    // Turn on the camera and wait for a single depth update.
    tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
    while (findPlaneWaitingForDepth)
    {
      yield return null;
    }
    tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
    // Find the plane.
    Camera cam = Camera.main;
    Vector3 planeCenter, screenPoint;
    Plane plane;

    screenPoint = cam.WorldToScreenPoint(position);
    device.send(System.Text.Encoding.ASCII.GetBytes("placing: " + position + " -> " + screenPoint + " " + cam.transform.position + "\n\n"));
    planeCenter = new Vector3();
    if (//screenPoint.x < 800 || screenPoint.x > 3000 || screenPoint.y < 400 || screenPoint.y > 2000 ||
      !pointCloud.FindPlane(cam, cam.WorldToScreenPoint(position), out planeCenter, out plane) ||
      Vector3.Distance(position, new Vector3(planeCenter.x, 0, planeCenter.z)) > 0.5f)
    {
      toPlaceLater.Add(idObject);
      toPlaceCoordinates.Add(position);
      device.send(System.Text.Encoding.ASCII.GetBytes("Found: " + planeCenter + position + " distance:" + Vector3.Distance(position, planeCenter) + " failed\n\n"));
    }
    else
    {
      // Ensure the location is always facing the camera.  This is like a LookRotation, but for the Y axis.
      planeCenter.y += 0.1f;
      Vector3 up = plane.normal;
      Vector3 forward;
      if (Vector3.Angle(plane.normal, cam.transform.forward) < 175)
      {
        Vector3 right = Vector3.Cross(up, cam.transform.forward).normalized;
        forward = Vector3.Cross(right, up).normalized;
      }
      else
      {
        // Normal is nearly parallel to camera look direction, the cross product would have too much
        // floating point error in it.
        forward = Vector3.Cross(up, cam.transform.right);
      }
      device.send(System.Text.Encoding.ASCII.GetBytes("objet ok: " + planeCenter + "\n"));
      Instantiate(toPlace[idObject], planeCenter, Quaternion.LookRotation(forward, up));
    }
  }

  /// <summary>
  /// This is called when the permission granting process is finished.
  /// </summary>
  /// <param name="permissionsGranted"><c>true</c> if permissions were granted, otherwise <c>false</c>.</param>
  public void OnTangoPermissions(bool permissionsGranted)
  {
  }

  /// <summary>
  /// This is called when successfully connected to the Tango service.
  /// </summary>
  public void OnTangoServiceConnected()
  {
    tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
  }

  /// <summary>
  /// This is called when disconnected from the Tango service.
  /// </summary>
  public void OnTangoServiceDisconnected()
  {
  }

  public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
  {
    // Don't handle depth here because the PointCloud may not have been updated yet.  Just
    // tell the coroutine it can continue.
    findPlaneWaitingForDepth = false;
  }

  void HandleOnClientRequest(BluetoothDevice device)
  {
    this.device = device;


    //Assign the 'Coroutine' that will handle your reading Functionality, this will improve your code style
    //Other way would be listening to the event Bt.OnReadingStarted, and starting the courotine from there
    this.device.ReadingCoroutine = ManageConnection;
    BluetoothAdapter.OnClientRequest -= HandleOnClientRequest;

    //Any device passed by the event OnClientRequest to this handler will has the same UUID, so we will connect to it directly.
    this.device.connect();
    Debug.Log("connection");
  }

  //############### Reading Data  #####################
  IEnumerator  ManageConnection(BluetoothDevice device)
  {
    //Manage Reading Coroutine
    while (device.IsReading)
    {
      byte[] msg = device.read();

      if (msg != null)
      {
        content += System.Text.ASCIIEncoding.ASCII.GetString(msg);
        Debug.Log("msg : " + content);
        // Format de commande [ID] [x] [y]
        if (content.EndsWith("\n") && int.Parse(content.Split(" ".ToCharArray(), 3)[0]) < toPlace.Length)
        {
          Vector3 position = new Vector3(float.Parse(content.Split(" ".ToCharArray(), 3)[1]), 0, float.Parse(content.Split(" ".ToCharArray(), 3)[2]));
          device.send(System.Text.Encoding.ASCII.GetBytes("msg : " + content + "position: " + Camera.main.transform.position.ToString() + "\n"));
          yield return StartCoroutine(PlaceObject(position, int.Parse(content.Split(" ".ToCharArray(), 3)[0])));
          device.send(System.Text.Encoding.ASCII.GetBytes("sorti\n"));
          content = "";
        }
      }
      yield return null;
    }
  }

  IEnumerator		PlaceLater()
  {
    while (true)
    {
      if (device.IsConnected)
        device.send(System.Text.Encoding.ASCII.GetBytes("coroutine\n"));
      while (toPlaceLater.Count > 0)
      {
        yield return StartCoroutine(PlaceObject(toPlaceCoordinates[0], toPlaceLater[0]));
        toPlaceCoordinates.RemoveAt(0);
        toPlaceLater.RemoveAt(0);
        yield return new WaitForSeconds(0.1f);
      }
      yield return new WaitForSeconds(1);
    }
  }
}

