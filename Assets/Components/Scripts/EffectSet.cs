using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using Object = UnityEngine.Object;

public class EffectSet{

	public  GameObject _effectCaptureSet; //パーティクル撮影用カメラとパーティクルを子に持つEmptyObject
	private readonly GameObject _effectRawImage; //撮影して出来たRenderTextureを写すRawImage

	public bool IsPlaying;//パーティクル再生中であるかどうか


	public EffectSet(GameObject captureSetPrefab, GameObject rawImagePrefab, Transform captureSetParent, Transform rawImageParent){
		
		_effectCaptureSet = Object.Instantiate (captureSetPrefab);
		_effectRawImage = Object.Instantiate (rawImagePrefab);

		//RenderTextureの設定
		var rt = new RenderTexture (64,64,0);
		var camera = _effectCaptureSet.transform.GetComponentInChildren<Camera> ();
		camera.targetTexture = rt;
		_effectRawImage.GetComponent<RawImage> ().texture = rt;

		//ヒエラルキーを整える為親を設定
		_effectCaptureSet.transform.SetParent (captureSetParent);
		_effectRawImage.transform.SetParent (rawImageParent);

		IsPlaying = false;
	}

	//エフェクト再生
	public void PlayOn(){
		IsPlaying = true;
		//クリック位置にエフェクトが表示されるようにRawImageの位置を調整
		//位置を変えたい場合はここをいじって下さい
		Vector2 imagePos = Input.mousePosition;
		imagePos.y -= _effectRawImage.GetComponent<RectTransform> ().rect.height / 2;
		_effectRawImage.transform.position = imagePos;
		_effectRawImage.GetComponent<RawImage> ().enabled = true;
		var ps = _effectCaptureSet.transform.GetComponentInChildren<ParticleSystem> ();
		ps.Play ();
		Observable.Timer (TimeSpan.FromSeconds (ps.main.duration + ps.main.startLifetime.constant)).First ().Subscribe (_ => FinishPaling());
	}

	private void FinishPaling(){
		IsPlaying = false;
		_effectRawImage.GetComponent<RawImage> ().enabled = false;
	}

	public void Delete(){
		Object.Destroy (_effectCaptureSet);
		Object.Destroy (_effectRawImage);
	}
}
