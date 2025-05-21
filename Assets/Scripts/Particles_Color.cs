using UnityEngine;

public class Particles_Color : MonoBehaviour
{
	public Color[] colors;

	public void SetColor(int x)
	{
		ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
		main.startColor = colors[x];
	}
}
