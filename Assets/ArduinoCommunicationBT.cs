using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TechTweaking.Bluetooth;

public class ArduinoCommunicationBT : MonoBehaviour {
	
	private BluetoothDevice device;
	public string bluetoothName = "HC-05";

	// Use this for initialization
	void Awake () {
		Debug.Log ("BT start\n");
		BluetoothAdapter.enableBluetooth ();//Force Enabling Bluetooth
		device = new BluetoothDevice ();
		device.Name = bluetoothName;
	}

	void Start ()
	{
		device.connect();
		if (device.IsConnected)
			Debug.Log ("Ok BT\n");
		else
			Debug.Log ("KO BT\n");
	}

	private void sendHello() {
		if (device != null) {
			/*
			 * Send and Read works only with bytes. You need to convert everything to bytes.
			 * Different devices with different encoding is the reason for this. You should know what encoding you're using.
			 * In the method call below I'm using the ASCII encoding to send "Hello" + a new line.
			 */
			device.send (System.Text.Encoding.ASCII.GetBytes ("Hello\n"));
		}
	}


	void Update() {
		Debug.Log ("Update BT");
		sendHello ();
	}
}
