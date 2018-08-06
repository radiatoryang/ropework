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

		public Image bgImage;
		public Image genericSprite;
		List<Image> sprites = new List<Image>();
		Dictionary<string,Image> actors = new Dictionary<string,Image>();

		string[] separ = new string[] {","};

		[YarnCommand("Scene")]
		public void DoSceneChange(string textureName) {
			bgImage.sprite = ValidateSprite(textureName);
		}

		[YarnCommand("Act")]
		public void SetActor(string parameters) {
			var par = parameters.Split( separ, System.StringSplitOptions.RemoveEmptyEntries);
			var textureName = par[1];
			if ( actors.ContainsKey(par[0])) {
				actors[par[0]].sprite = Resources.Load<Sprite>(par[1]);
			}
		}

		[YarnCommand("Show")]
		public void SetSprite(string parameters) {
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
			SetSpriteActual( spriteName, pos );
		}

		void SetSpriteActual(string spriteName, Vector2 position) {
			var newSpriteObject = Instantiate<Image>(genericSprite, genericSprite.transform.parent);
			sprites.Add(newSpriteObject);
			newSpriteObject.sprite = ValidateSprite( spriteName );
			newSpriteObject.SetNativeSize();
			newSpriteObject.rectTransform.anchoredPosition = Vector2.Scale( position, new Vector2( 1280f, 720f ) );
		}

		float ConvertCoordinates(string coordinate) {
			// first, let's see if they used a keyword
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
			var newSprite = Resources.Load<Sprite>(spriteName);
			if ( newSprite == null ) {
				Debug.LogErrorFormat( "Ropework can't find sprite [{0}]... maybe it is misspelled, or it isn't configured as a sprite?", spriteName );
				return null;
			} else {
				return newSprite;
			}
		}


    } // end class

} // end namespace
