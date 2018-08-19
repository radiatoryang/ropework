using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Yarn.Unity;

namespace Ropework {
    public class RopeworkManager : MonoBehaviour
    {
		[Header("Manually Load Assets"), Tooltip("you can manually assign various assets here if you don't want to use /Resources/ folder")]
		public Sprite[] loadSprites;
		public AudioClip[] loadAudio;

		[Header("UI settings")] // UI tuning variables and references
		public Color highlightTint;
		public Color defaultTint;

		[Header("Object references"), Tooltip("umm don't change these")]
		public RectTransform spriteGroup; // used for screenshake
		public Image bgImage;
		public Image genericSprite; // local prefab, used for instantiating sprites
		public AudioSource myAudioSource; // local prefab, used for instantiating sounds

		// internal stuff...
		List<AudioSource> sounds = new List<AudioSource>(); // big list of all instantiated sounds
		List<Image> sprites = new List<Image>(); // big list of all instantianted sprites
		[HideInInspector] public Dictionary<string,Image> actors = new Dictionary<string,Image>(); // "actors" are sprites with name IDs

		string[] separ = new string[] {","}; // stores the separator value, usually a comma

		void Awake () {
			// always rename this gameObject to "@" so that Yarn commands will work
			this.name = "@";
		}

		void Update () {

		}

		#region YarnCommands

		// changes background image
		[YarnCommand("Scene")]
		public void DoSceneChange(string spriteName) {
			bgImage.sprite = FetchAsset<Sprite>(spriteName);
		}

		// SetActor(actorName,spriteName,positionX,positionY)
		// main function for moving / adjusting characters
		[YarnCommand("Act")]
		public void SetActor(params string[] parameters) {
			// get parameter data
			var par = CleanParams( parameters );
			var actorName = par[0];
			var spriteName = "";
			if ( par.Length > 1 ) {
				spriteName = par[1];
			} else {
				Debug.LogErrorFormat(this, "Ropework tried to <<SetActor {0}>> but there aren't enough parameters to work with; it needs at least 2, like <<SetActor @ actorName, spriteName>>", par[0] );
				return;
			}

			// have to use SetSprite() because par[2] and par[3] might be keywords (e.g. "left", "right")
			var newActor = SetSprite( string.Format("{0},{1},{2}", spriteName, par.Length > 2 ? par[2] : "", par.Length > 3 ? par[3] : "" ) );

			// if the actor is using a sprite already, then maybe clone position, and destroy it
			if ( actors.ContainsKey(actorName)) {
				// if any missing position params, assume the actor position should stay the same
				var newPos = newActor.rectTransform.anchoredPosition;
				if ( par.Length == 2 ) { // missing 2 params, override both x and y
					newPos = actors[actorName].rectTransform.anchoredPosition;
				} else if ( par.Length == 3) { // missing 1 param, override y
					newPos.y = actors[actorName].rectTransform.anchoredPosition.y;
				}
				newActor.rectTransform.anchoredPosition = newPos;
				// clean-up
				Destroy( actors[actorName].gameObject );
				actors.Remove(actorName);
			}

			actors.Add( actorName, newActor );
		}

		// SetSprite(spriteName,positionX,positionY)
		// generic function for sprite drawing
		[YarnCommand("Show")]
		public Image SetSprite(params string[] parameters) {
			var par = CleanParams( parameters );

			// set sprite
			var spriteName = par[0];

			// position sprite
			var pos = new Vector2(0.5f, 0.5f);
			if ( par.Length > 1 ) {
				pos.x = ConvertCoordinates(par[1]);
			}
			if ( par.Length > 2 ) {
				pos.y = ConvertCoordinates(par[2]);
			}

			// actually instantiate and draw sprite now
			return SetSpriteActual( spriteName, pos );
		}

		// hides a sprite... TODO: allow wildcards, e.g. HideSprite(Sally*) will hide a sprite named SallyIdle or Sally_Happy
		[YarnCommand("Hide")]
		public void HideSprite(string actorOrSpriteName) {
			// find the spriteObject with name "spriteName" and destroy it

			// is it an actor name?
			Image toDestroy = null;
			string keyToRemove = "";
			if ( actors.ContainsKey( actorOrSpriteName ) ) {
				keyToRemove = actorOrSpriteName;
				toDestroy = actors[actorOrSpriteName];
			} else { // if it isn't an actor, then let's just do this in a sloppy way for now, and also assume there's only one object like it
				foreach ( var spriteObject in sprites ) {
					if (spriteObject.name == actorOrSpriteName) {
						toDestroy = spriteObject;
						break;
					}
				}

				// if an actor is using the sprite reference, also remove reference to it
				foreach ( var kvp in actors ) {
					if ( kvp.Value == toDestroy ) {
						keyToRemove = kvp.Key;
						break;
					}
				}
			}

			// there's probably a better way to do this
			if ( keyToRemove.Length > 0 ) {
				actors.Remove(keyToRemove);
			}

			// don't forget to actually destroy the sprite object
			if ( toDestroy != null ) {
				CleanDestroy<Image>(toDestroy.gameObject);
			} else {
				Debug.LogWarningFormat(this, "Ropework tried to <<Hide {0}>> but it can't find any sprite named \"{0}\"... it was either misspelled, or it was already hidden", actorOrSpriteName );
			}
		}

		// hides all sprites (but doesn't clear the background image)
		[YarnCommand("HideAll")]
		public void HideAllSprites() {
			foreach ( var spr in sprites ) {
				HideSprite( spr.name );
			}
		}

		// PlayAudio( soundName,volume,"loop" )... PlayAudio(soundName,1.0) plays the sound once at 100% volume
		// "volume" is a number from 0.0 to 1.0
		// "loop" is the word "loop" (or "true"), which tells the sound to loop over and over
		[YarnCommand("PlayAudio")]
		public void PlayAudio(params string[] parameters) {
			var pars = CleanParams( parameters );

			var audioClip = FetchAsset<AudioClip>(pars[0]);
			// detect volume setting
			float volume = 1f;
			if ( pars.Length > 1 ) { // if parsing fails or second parameter isn't present, default to 100% volume
				if ( float.TryParse( pars[1], out volume ) == false ) {
					volume = 1f;
				} else if ( volume <= 0.01f ) {
					Debug.LogWarningFormat(this, "Ropework is playing sound {0} at very low volume ({1}), just so you know", pars[0], pars[1] );
				}
			}
			// detect loop setting
			bool shouldLoop = false;
			if ( pars.Length > 2 && (pars[2].Contains("loop") || pars[2].Contains("true") ) ) {
				shouldLoop = true;
			}
			
			// instantiate AudioSource and configure it (don't use AudioSource.PlayOneShot because we also want the option to use <<StopAudio>> and interrupt it)
			var newAudioSource = Instantiate<AudioSource>( myAudioSource, myAudioSource.transform.parent );
			newAudioSource.name = audioClip.name;
			newAudioSource.clip = audioClip;
			newAudioSource.volume *= volume;
			newAudioSource.loop = shouldLoop;
			newAudioSource.Play();
			sounds.Add(newAudioSource);

			// if it doesn't loop, let's set a max lifetime for this sound
			if ( shouldLoop == false ) {
				StartCoroutine( SetDestroyTime( newAudioSource, audioClip.length ) );
			}
		}

		// stops sound playback based on sound name, whether it's looping or not
		[YarnCommand("StopAudio")]
		public void StopAudio(string soundName) {
			// let's just do this in a sloppy way for now, and also assume there's only one object like it
			AudioSource toDestroy = null;
			foreach ( var audioObject in sounds ) {
				if (audioObject.name == soundName) {
					toDestroy = audioObject;
					break;
				}
			}

			// double-check there's any audioSource to destroy tho
			if ( toDestroy != null ) {
				CleanDestroy<AudioSource>( toDestroy.gameObject );
			} else {
				Debug.LogWarningFormat(this, "Ropework tried to <<StopAudio {0}>> but couldn't find any sound \"{0}\" currently playing. Double-check the name, or maybe it already stopped.", soundName );
			}
		}

		// stops all currently playing sounds
		[YarnCommand("StopAudioAll")]
		public void StopAudioAll() {
			foreach (var audioSrc in sounds ) {
				StopAudio( audioSrc.name );
			}
		}

		// shakes actorName or spriteName at X strength
		[YarnCommand("Shake")]
		public void Shake(params string[] parameters) {
			var pars = CleanParams( parameters );

			// detect shakeStrength setting
			float shakeStrength = 0.5f;
			if ( pars.Length > 1 ) {
				if ( float.TryParse( pars[1], out shakeStrength ) == false ) {
					shakeStrength = 0.5f;
				}
			}

			// actually shake the thing now
			var findShakeTarget = FindActorOrSprite( pars[0] );
			if ( findShakeTarget != null ) {
				StartCoroutine( SetShake( findShakeTarget.rectTransform, shakeStrength ) );
			}
			
		}

		#endregion



		#region Utility

		// called by ClassicDialogueUI to highlight a sprite when it's talking
		public void HighlightSprite (Image sprite) {
			StopCoroutine( "HighlightSpriteCoroutine" ); // use StartCoroutine(string) overload so that we can Stop and Start the coroutine (it doesn't work otherwise?)
			StartCoroutine( "HighlightSpriteCoroutine", sprite );
		}

		// called by HighlightSprite
		IEnumerator HighlightSpriteCoroutine (Image highlightedSprite) {
			float t = 0f;
			// over time, gradually change sprites to be "normal" or "highlighted"
			while ( t < 1f ) {
				t += Time.deltaTime / 2f;
				foreach ( var spr in sprites ) {
					if ( spr != highlightedSprite) { // set back to normal
						spr.transform.localScale = Vector3.MoveTowards( spr.transform.localScale, Vector3.one, Time.deltaTime );
						spr.color = Color.Lerp( spr.color, defaultTint, Time.deltaTime * 5f );
					} else { // a little bit bigger / brighter
						spr.transform.localScale = Vector3.MoveTowards( spr.transform.localScale, Vector3.one * 1.1f, Time.deltaTime );
						spr.color = Color.Lerp( spr.color, highlightTint, Time.deltaTime * 5f );
					}
				}
				yield return 0;
			}
		}

		Image SetSpriteActual(string spriteName, Vector2 position) {
			var newSpriteObject = Instantiate<Image>(genericSprite, genericSprite.transform.parent);
			sprites.Add(newSpriteObject);
			newSpriteObject.name = spriteName;
			newSpriteObject.sprite = FetchAsset<Sprite>( spriteName );
			newSpriteObject.SetNativeSize();
			newSpriteObject.rectTransform.anchoredPosition = Vector2.Scale( position, new Vector2( 1280f, 720f ) );
			return newSpriteObject;
		}

		// TODO: change to Image[] and grab all valid results
		Image FindActorOrSprite(string actorOrSpriteName) {
			if ( actors.ContainsKey( actorOrSpriteName ) ) {
				return actors[actorOrSpriteName];
			} else { // or is it a generic sprite?
				foreach ( var sprite in sprites ) { // lazy sprite name search
					if ( sprite.name == actorOrSpriteName ) {
						return sprite;
					}
				}
				Debug.LogErrorFormat(this, "Ropework couldn't find an actor or sprite with name \"{0}\", maybe it was misspelled or the sprite was hidden / destroyed already", actorOrSpriteName );
				return null;
			}
		}

		// shakes a RectTransform (usually sprites)
		IEnumerator SetShake( RectTransform thingToShake, float shakeStrength = 0.5f ) {
			var startPos = thingToShake.anchoredPosition;
			while ( shakeStrength > 0f ) {
				shakeStrength -= Time.deltaTime;
				float shakeDistance = Mathf.Clamp( shakeStrength * 69f, 0f, 69f);
				float shakeFrequency = Mathf.Clamp( shakeStrength * 5f, 0f, 5f);
				thingToShake.anchoredPosition = startPos + shakeDistance * new Vector2( Mathf.Sin(Time.time * shakeFrequency), Mathf.Sin(Time.time * shakeFrequency + 17f) * 0.62f );
				yield return 0;
			}
			thingToShake.anchoredPosition = startPos;
		}

		// timed destroy... can't use Destroy( gameObject, timeDelay ) because it might get destroyed earlier via <<StopAudio>> or something, and we want to remove the reference from the list too
		IEnumerator SetDestroyTime(AudioSource destroyThis, float timeDelay) {
			float timer = timeDelay;
			while ( timer > 0f ) {
				if ( destroyThis.isPlaying ) {
					timer -= Time.deltaTime;
				}
				yield return 0;
			}
			if ( destroyThis != null ) { // it could've been destroyed already, so let's just make sure
				CleanDestroy<AudioSource>( destroyThis.gameObject );
			}
		}

		// CleanDestroy also removes any references to the gameObject from sprites or sounds
		void CleanDestroy<T>( GameObject destroyThis ) {
			if ( typeof(T) == typeof(AudioSource) ) {
				sounds.Remove( destroyThis.GetComponent<AudioSource>() );
			} else if ( typeof(T) == typeof(Image) ) {
				sprites.Remove( destroyThis.GetComponent<Image>() );
			}

			Destroy( destroyThis );
		}

		// utility function to clean and combine params
		// this is necessary because if the Yarn author forgets to separate parameters with a space, then they all get sent as 1 string that we'll have to split
		string[] CleanParams(string[] parameters) {
			var cleanPars = new List<string>();
			foreach (var par in parameters) {
				var fromCSV = par.Split( separ, System.StringSplitOptions.RemoveEmptyEntries);
				cleanPars.AddRange( fromCSV );
			}
			return cleanPars.ToArray();
		}

		// utility function to convert words like "left" or "right" into equivalent position numbers
		float ConvertCoordinates(string coordinate) {
			// first, let's see if they used a position keyword
			coordinate = coordinate.ToLower();
			switch ( coordinate ) {
				case "left":
				case "bottom":
				case "lower":
					return 0.25f;
				case "center":
				case "middle":
					return 0.5f;
				case "right":
				case "top":
				case "upper":
					return 0.75f;
			}

			// if none of those worked, then let's try parsing it as a number
            float x;
            if (float.TryParse(coordinate, out x))
            {
                return x;
            }
            else
            {
                Debug.LogErrorFormat(this, "Ropework couldn't convert position [{0}]... it must be an alignment (left, center, right, or top, middle, bottom) or a value (like 0.42 as 42%)", coordinate);
                return -1f;
            }

        }

		// utility function to find an asset, whether it's in \Resources\ or manually loaded via an array
		T FetchAsset<T>( string assetName ) where T : Object, new() {
			// first, check to see if it's a manully loaded asset, with manual array checks... it's messy but I can't think of a better way to do this
			if ( typeof(T) == typeof(Sprite) ) {
				foreach ( var spr in loadSprites ) {
					if (spr.name == assetName) {
						return spr as T;
					}
				}
			} else if ( typeof(T) == typeof(AudioClip) ) {
				foreach ( var ac in loadAudio ) {
					if ( ac.name == assetName ) {
						return ac as T;
					}
				}
			}

			// otherwise, let's search Resources for it
			var newAsset = Resources.Load<T>(assetName);
			if ( newAsset == null ) {
				Debug.LogErrorFormat(this, "Ropework can't find asset [{0}]... maybe it is misspelled, or isn't imported as {1}?", assetName, typeof(T).ToString() );
				return null;
			} else {
				return newAsset;
			}
		}


		#endregion


    } // end class

} // end namespace
