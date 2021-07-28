using System.Collections;
using UnityEngine;

public class BlackBarHandler : MonoBehaviour
{
	[Range(0.01f, 0.5f)]
	public float finalStateAmountOfScreen = 0.22f;
	public float transitionDuration = 2f;

	private Material material;
	private static readonly int shaderPropAmountOfScreen = Shader.PropertyToID("_AmountOfScreen");
	private float _currentAmountOfScreen;
	
	private float _t;
	private float _amountOfPixelsToMove;

	private void Awake ()
	{
		material = new Material(Shader.Find("Hidden/BlackBarShader"));
		material.SetFloat(shaderPropAmountOfScreen, 0f);
	}
	
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_currentAmountOfScreen < 0.00001f)
		{
			Graphics.Blit(source, destination);
			return;
		}

		Graphics.Blit(source, destination, material);
	}

	public void Animate(bool reel)
	{
		float screenHeight = Screen.height;
		_amountOfPixelsToMove = finalStateAmountOfScreen * 0.5f * screenHeight;

		StartCoroutine(AnimateRoutine(reel));
	}

	private IEnumerator AnimateRoutine(bool reel)
	{
		float toAdd = 1f / _amountOfPixelsToMove;
		float toWait = transitionDuration / _amountOfPixelsToMove;
		
		while (_t < 1f)
		{
			if (reel) _currentAmountOfScreen = Mathf.Lerp(0f, finalStateAmountOfScreen, _t);
			else _currentAmountOfScreen = Mathf.Lerp(finalStateAmountOfScreen, 0f, _t);

			material.SetFloat(shaderPropAmountOfScreen, _currentAmountOfScreen);
			_t += toAdd;
			yield return new WaitForSeconds(toWait);
		}
		
		_t = 0f;
	}
}