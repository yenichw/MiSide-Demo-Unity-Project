// based on the original game.Yen Chezky(yenichw)
using System;
using UnityEngine;

[Serializable]
public class FootMaterial
{
	public AudioClip[] soundsFoot;

	public GameObject particleFoot;

	public GameObject particleFootRun;

	[Range(0f, 1f)]
	public float volume;
}
