using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fluidTrickAnimation : MonoBehaviour {
	public int frame = 0;
	public float frameTime = 0.5f;
	private float m_currentTime = 0.0f;

	public bool 		loop;
	public GameObject[]	frameObject;
	private bool 		end;
	// Use this for initialization
	void Start ()
	{
		resetFrameObject ();
		frameObject [0].SetActive (true);
		loop = false;
		end = false;
	}

	// Update is called once per frame
	void Update ()
	{
		m_currentTime += Time.deltaTime;
		if (m_currentTime >= frameTime)
		{
			++frame;
			frame = frame % frameObject.Length;
			if (!loop && frame == 0)
				end = true;
			resetFrameObject ();
			if (!end)
				frameObject [frame].SetActive (true);
			else
				frameObject [frameObject.Length - (1 + frame % 3)].SetActive (true);
			m_currentTime %= frameTime;
		}
	}

	void resetFrameObject()
	{
		for (int i = 0; i < frameObject.Length; ++i)
			frameObject [i].SetActive (false);
	}
}
