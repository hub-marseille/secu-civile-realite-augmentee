using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TechTweaking.Bluetooth;

public class ArduinoCommunicationBT : MonoBehaviour 
{
	
	private BluetoothDevice device;
	public string bluetoothName = "HC-05";
	private float lastUpdate = 0;

	// Use this for initialization
	void Awake () 
	{
		Debug.Log("BT awake\n");
	}

	void Start ()
	{
		Debug.Log("BT start\n");
		BluetoothAdapter.enableBluetooth();//Force Enabling Bluetooth
		device = new BluetoothDevice();
		device.Name = "HC-05";//bluetoothName;
		device.connect();
		if (device.IsConnected) 
			Debug.Log("Ok BT\n");
		else
			Debug.Log("KO BT\n");
	}

	private double getHotDistance()
	{
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag("Hot");
		double distance = Mathf.Infinity;
		Vector3 position = transform.position;
		if (GameObject.FindGameObjectsWithTag ("MainCamera").Length > 0)
			position = GameObject.FindGameObjectsWithTag ("MainCamera") [0].transform.position;
		foreach (GameObject go in gos)
		{
			Vector3 diff = go.transform.position - position;
			float curDistance = diff.sqrMagnitude;
			if (curDistance < distance)
				distance = curDistance;
		}
		return distance;
	}

	private void sendTemperature() 
	{
		double	temperature = 0;
		double distance = Mathf.Infinity;

		distance = getHotDistance();
		if (distance < .5)
			distance = .5;
		temperature = 20 + (30 / distance);
		if (device != null)
			device.send (System.Text.Encoding.ASCII.GetBytes (temperature.ToString ("0.0") + "\n"));
	}

	void Update()
	{
		if (Time.time > lastUpdate + 0.5f) 
		{
			sendTemperature ();
			lastUpdate = Time.time;
		}
	}
}

