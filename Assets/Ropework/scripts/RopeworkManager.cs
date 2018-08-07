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
		[Header("Overrides"), Tooltip("you can manually assign various assets here if you don't want to use /Resources/ folder")]
		public Sprite[] loadSprites;
		public AudioClip[] loadAudio;

		[Header("UI")]
		public Image bgImage;
		public Image genericSprite;
		public Color highlightTint, defaultTint;
		List<Image> sprites = new List<Image>();
		[HideInInspector] public Dictionary<string,Image> actors = new Dictionary<string,Image>();

		string[] separ = new string[] {","};

		#region YarnCommands

		[YarnCommand("Scene")]
		public void DoSceneChange(string textureName) {
			bgImage.sprite = ValidateSprite(textureName);
		}

		[YarnCommand("Act")]
		public void SetActor(string parameters) {
			// get parameter data
			var par = parameters.Split( separ, System.StringSplitOptions.RemoveEmptyEntries);
			var actorName = par[0];
			var spriteName = par[1];

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

		[YarnCommand("Show")]
		public Image SetSprite(string parameters) {
			var par = parameters.Split( separ, System.StringSplitOptions.RemoveEmptyEntries);

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

		[YarnCommand("Hide")]
		public void HideSprite(string spriteName) {
			// find the spriteObject with name "spriteName" and destroy it
			// let's just do this in a sloppy way for now, and also assume there's only one object like it
			Image toDestroy = null;
			foreach ( var spriteObject in sprites ) {
				if (spriteObject.name == spriteName) {
					toDestroy = spriteObject;
					break;
				}
			}
			// if an actor is using it, also remove reference to it
			string keyToRemove = "";
			foreach ( var kvp in actors ) {
				if ( kvp.Value == toDestroy ) {
					keyToRemove = kvp.Key;
					break;
				}
			}
			// there's probably a better way to do this
			if ( keyToRemove.Length > 0 ) {
				actors.Remove(keyToRemove);
			}

			// don't forget to actually destroy the sprite object
			Destroy(toDestroy.gameObject);
			sprites.Remove(toDestroy);
		}

		#endregion

		#region Utility

		// called by ClassicDialogueUI
		public void HighlightSprite (Image sprite) {
			StopCoroutine( "HighlightSpriteCoroutine" ); // use StartCoroutine(string) overload so that we can Stop and Start the coroutine (it doesn't work otherwise?)
			StartCoroutine( "HighlightSpriteCoroutine", sprite );
		}

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
			newSpriteObject.sprite = ValidateSprite( spriteName );
			newSpriteObject.SetNativeSize();
			newSpriteObject.rectTransform.anchoredPosition = Vector2.Scale( position, new Vector2( 1280f, 720f ) );
			return newSpriteObject;
		}

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

			// next, let's try parsing it as a number
            float x;
            if (float.TryParse(coordinate, out x))
            {
                return x;
            }
            else
            {
                Debug.LogErrorFormat("Ropework couldn't convert position [{0}]... it must be an alignment (left, center, right, or top, middle, bottom) or a value (like 0.42 as 42%)", coordinate);
                return -1f;
            }

        }

		Sprite ValidateSprite( string spriteName ) {
			// first, check to see if it's a manually loaded sprite
			foreach ( var spr in loadSprites ) {
				if (spr.name == spriteName) {
					return spr;
				}
			}

			// otherwise, let's search Resources for it
			var newSprite = Resources.Load<Sprite>(spriteName);
			if ( newSprite == null ) {
				Debug.LogErrorFormat( "Ropework can't find sprite [{0}]... maybe it is misspelled, or it isn't configured as a sprite?", spriteName );
				return null;
			} else {
				return newSprite;
			}
		}

		#endregion


    } // end class

} // end namespace
