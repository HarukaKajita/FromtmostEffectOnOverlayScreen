using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;
using UniRx.Triggers;
using UnityEngine.UI;

public class EffectManager : MonoBehaviour {

	[SerializeField] private GameObject _captureSetPrefab; //パーティクル撮影用カメラとパーティクルを子に持つEmptyObjectのプレハブ
	[SerializeField] private GameObject _rawImagePrefab; //撮影して出来た映像を写すRawImageのプレハブ
	
	//RenderTextureの映像を写すRawImageをまとめる親オブジェクト
	[SerializeField] private GameObject _rawImagesParentPrefab;//プレハブ
	private Transform _rawImagesParent;//Instantiate後のインスタンスのトランスフォーム
	
	//最前面に来るCanvasをアタッチして下さい。
	[SerializeField] private Transform _mostFrontCanvas;
	
	//プール
	private readonly List<EffectSet> _effectPool = new List<EffectSet>();
	[SerializeField] private uint _prePoolingNum = 3;//常時プールするエフェクトの数

	//shrinkInterval秒毎に余分なエフェクトを1つずつ削除していきます。
	public int ShrinkInterval = 10;
	
	private void Start()
	{
		if (_mostFrontCanvas == null) {
			var canvasObject = new GameObject("EffectCanvas");
			var canvas = canvasObject.AddComponent<Canvas>();
			canvasObject.AddComponent<GraphicRaycaster>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 30000;
			
			Instantiate(canvasObject);
			_mostFrontCanvas = canvasObject.transform;
		}

		SetRawImagesParent();
		PoolDefaultEffects();

		//クリックでエフェクトを再生するストリーム
		Observable.EveryUpdate()
			.Where(_ => Input.GetMouseButtonDown(0))
			.Subscribe(_ => PlayEffect()).AddTo(this);

		//定期的に余分なエフェクトを削除するストリーム
		Observable
			.Interval(TimeSpan.FromSeconds(ShrinkInterval))
			.Where(_ => _effectPool.Count > 3)
			.Select(_ => _effectPool[_effectPool.Count -1])
			.Where(e => e.IsPlaying == false)
			.Subscribe(ShrinkPool).AddTo(this);
			
	}

	private void SetRawImagesParent()
	{
		
		_rawImagesParent = Instantiate(_rawImagesParentPrefab,_mostFrontCanvas).transform;
		_rawImagesParent.transform.SetAsLastSibling();
	}

	private void PoolDefaultEffects()
	{
		//prePooligNum個エフェクトを予め作る(ここで作られたエフェクトは常にプールされ削除されない)
		for(var i = 0; i < _prePoolingNum; i++){
			var e = CreateInstance ();
			_effectPool.Add (e);
		}
	}

	private EffectSet CreateInstance(){

		//エフェクトに関する物の参照をまとめるEffectSetクラスを生成
		var effectSet = new EffectSet (_captureSetPrefab, _rawImagePrefab, transform, _rawImagesParent);

		//他のカメラに他のパーティクルが写り込まないように位置を調整する
		effectSet._effectCaptureSet.transform.localPosition = new Vector3 (_effectPool.Count * 10f,0f,0f);

		return effectSet;
	}


	private void PlayEffect(){
		var effect = SearchPlayableEffect ();
		effect.PlayOn ();

	}


	private EffectSet SearchPlayableEffect(){
		foreach(var e in _effectPool){
			if(e.IsPlaying == false){
				return e;
			}
		}

		//再生できるエフェクトがない場合は新たにプールに追加
		var newSakuraEffect = CreateInstance ();
		//プールに追加
		_effectPool.Add(newSakuraEffect);
		return newSakuraEffect;
	}

	
	 private void ShrinkPool(EffectSet e){
		e.Delete ();
		_effectPool.Remove (e);
	}

}