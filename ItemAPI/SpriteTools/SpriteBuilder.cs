using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alexandria.DungeonAPI;
using Alexandria.Misc;
using UnityEngine;

namespace Alexandria.ItemAPI
{
	public static class SpriteBuilder
	{
		public static tk2dSpriteCollectionData itemCollection = PickupObjectDatabase.GetById(155).sprite.Collection;
		public static tk2dSpriteCollectionData ammonomiconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
		private static tk2dSprite baseSprite = PickupObjectDatabase.GetById(155).GetComponent<tk2dSprite>();
		public static tk2dSpriteAnimationClip AddAnimation(tk2dSpriteAnimator animator, tk2dSpriteCollectionData collection, List<int> spriteIDs,
			string clipName, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, float fps = 15)
		{
			var clip = Shared.CreateAnimation(collection, spriteIDs, clipName, wrapMode, fps);

			if (animator.Library == null)
			{
				animator.Library = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
				animator.Library.clips = new tk2dSpriteAnimationClip[0];
				animator.Library.enabled = true;
			}

			Array.Resize(ref animator.Library.clips, animator.Library.clips.Length + 1);
			animator.Library.clips[animator.Library.clips.Length - 1] = clip;

			return clip;
		}
		
		/// <summary>
		/// Returns an object with a tk2dSprite component with the 
		/// texture of a file in the sprites folder
		/// </summary>
		public static GameObject SpriteFromFile(string spriteName, GameObject obj = null)
		{
			string filename = spriteName.Replace(".png", "");

			var texture = ResourceExtractor.GetTextureFromFile(filename);
			if (texture == null) return null;

			return SpriteFromTexture(texture, spriteName, obj);
		}
		/// <summary>
		/// Returns an object with a tk2dSprite component with the 
		/// texture of an embedded resource
		/// </summary>
		public static GameObject SpriteFromResource(string spriteName, GameObject obj = null, Assembly assembly = null)
		{
			string extension = !spriteName.EndsWith(".png") ? ".png" : "";
			string resourcePath = spriteName + extension;

			var texture = ResourceExtractor.GetTextureFromResource(resourcePath, assembly ?? Assembly.GetCallingAssembly());
			if (texture == null) return null;

			return SpriteFromTexture(texture, resourcePath, obj);
		}

		/// <summary>
		/// Returns an object with a tk2dSprite component with the texture provided
		/// </summary>
		public static GameObject SpriteFromTexture(Texture2D texture, string spriteName, GameObject obj = null)
		{
			if (obj == null)
			{
				obj = new GameObject();
			}
			tk2dSprite sprite;
			sprite = obj.AddComponent<tk2dSprite>();

			int id = AddSpriteToCollection(texture, itemCollection, spriteName);
			sprite.SetSprite(itemCollection, id);
			sprite.SortingOrder = 0;
			sprite.IsPerpendicular = true;

			obj.GetComponent<BraveBehaviour>().sprite = sprite;

			return obj;
		}


		public static int AddSpriteToCollection(Texture2D texture, tk2dSpriteCollectionData collection, string name = "")
		{
			var definition = ConstructDefinition(texture); //Generate definition
			if (string.IsNullOrEmpty(name))
			{
				definition.name = texture.name; //naming the definition is actually extremely important 
			}
			else
			{
				definition.name = name; //naming the definition is actually extremely important 
			}
			return AddSpriteToCollection(definition, collection);
		}

		public static int AddSpriteToCollection(string resourcePath, tk2dSpriteCollectionData collection, string name, Assembly assembly = null)
		{
			string extension = !resourcePath.EndsWith(".png") ? ".png" : "";
			resourcePath += extension;
			var texture = ResourceExtractor.GetTextureFromResource(resourcePath, assembly ?? Assembly.GetCallingAssembly()); //Get Texture
			var definition = ConstructDefinition(texture); //Generate definition
			if (string.IsNullOrEmpty(name))
			{
				definition.name = texture.name; //naming the definition is actually extremely important 
			}
			else
			{
				definition.name = name; //naming the definition is actually extremely important 
			}
			return AddSpriteToCollection(definition, collection);
		}

		public static int AddSpriteToCollection2(Texture2D texture, tk2dSpriteCollectionData collection, string name = "")
		{
			var definition = ConstructDefinition2(texture, collection); //Generate definition
			if (string.IsNullOrEmpty(name))
			{
				definition.name = texture.name; //naming the definition is actually extremely important 
			}
			else
			{
				definition.name = name; //naming the definition is actually extremely important 
			}
			return AddSpriteToCollection(definition, collection);
		}


		public static int AddSpriteToCollection2(string resourcePath, tk2dSpriteCollectionData collection, string name = "", Assembly assembly = null)
		{
			string extension = !resourcePath.EndsWith(".png") ? ".png" : "";
			resourcePath += extension;
			var texture = ResourceExtractor.GetTextureFromResource(resourcePath, assembly ?? Assembly.GetCallingAssembly()); //Get Texture
			var definition = ConstructDefinition2(texture, collection); //Generate definition
			if (string.IsNullOrEmpty(name))
			{
				definition.name = texture.name; //naming the definition is actually extremely important 
			}
			else
			{
				definition.name = name; //naming the definition is actually extremely important 
			}
			return AddSpriteToCollection(definition, collection);
		}

		/// <summary>
		/// Adds a sprite (from a resource) to a collection
		/// </summary>
		/// <returns>The spriteID of the defintion in the collection</returns>
		public static int AddSpriteToCollection(string resourcePath, tk2dSpriteCollectionData collection, Assembly assembly = null)
		{
			string extension = !resourcePath.EndsWith(".png") ? ".png" : "";
			resourcePath += extension;
			var texture = ResourceExtractor.GetTextureFromResource(resourcePath, assembly ?? Assembly.GetCallingAssembly()); //Get Texture

			var definition = ConstructDefinition(texture); //Generate definition
			definition.name = texture.name; //naming the definition is actually extremely important 

			return AddSpriteToCollection(definition, collection);
		}

		/// <summary>
		/// Adds a sprite from a definition to a collection
		/// </summary>
		/// <returns>The spriteID of the defintion in the collection</returns>
		public static int AddSpriteToCollection(tk2dSpriteDefinition spriteDefinition, tk2dSpriteCollectionData collection)
		{
			//Add definition to collection
			var defs = collection.spriteDefinitions;
			var newDefs = defs.Concat(new tk2dSpriteDefinition[] { spriteDefinition }).ToArray();
			collection.spriteDefinitions = newDefs;

			//Reset lookup dictionary
			if (collection.spriteNameLookupDict == null)
				collection.InitDictionary();
			else
				collection.spriteNameLookupDict[spriteDefinition.name] = newDefs.Length - 1;
			return newDefs.Length - 1;
		}

		/// <summary>
		/// Adds a sprite definition to the Ammonomicon sprite collection
		/// </summary>
		/// <returns>The spriteID of the defintion in the ammonomicon collection</returns>
		public static int AddToAmmonomicon(tk2dSpriteDefinition spriteDefinition, string prefix = "")
		{
			//var newDef = spriteDefinition.Copy();
			//newDef.name = prefix + newDef.name;
			return AddSpriteToCollection(spriteDefinition, ammonomiconCollection);
		}

		public static tk2dSpriteAnimationClip AddAnimation(tk2dSpriteAnimation animaton, tk2dSpriteCollectionData collection, List<int> spriteIDs,
			string clipName, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, float fps = 15)
		{
			var clip = Shared.CreateAnimation(collection, spriteIDs, clipName, wrapMode, fps);

			Array.Resize(ref animaton.clips, animaton.clips.Length + 1);
			animaton.clips[animaton.clips.Length - 1] = clip;
			return clip;
		}

		public static tk2dSpriteAnimationClip AddAnimation(tk2dSpriteAnimator animator, tk2dSpriteCollectionData collection, List<string> spritePaths,
	string clipName, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, int fps = 15)
		{
			var clip = Assembly.GetCallingAssembly().CreateAnimation(collection, spritePaths, clipName, wrapMode, fps);

			if (animator.Library == null)
			{
				animator.Library = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
				animator.Library.clips = new tk2dSpriteAnimationClip[0];
				animator.Library.enabled = true;
			}

			Array.Resize(ref animator.Library.clips, animator.Library.clips.Length + 1);
			animator.Library.clips[animator.Library.clips.Length - 1] = clip;

			return clip;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000238C File Offset: 0x0000058C
		public static SpeculativeRigidbody SetUpSpeculativeRigidbody(this tk2dSprite sprite, IntVector2 offset, IntVector2 dimensions)
		{
			SpeculativeRigidbody body = sprite.gameObject.GetOrAddComponent<SpeculativeRigidbody>();
			body.AddCollider(CollisionLayer.EnemyCollider, offset, dimensions);
			return body;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002404 File Offset: 0x00000604
		public static tk2dSpriteDefinition ConstructDefinition(Texture2D texture)
		{
			return Shared.ConstructDefinition(texture: texture, overrideMat: null, apply: false, useOffset: false);
		}

		public static tk2dSpriteDefinition ConstructDefinition2(Texture2D texture, tk2dSpriteCollectionData collection)
		{
			return Shared.ConstructDefinition(texture: texture, overrideMat: null, apply: false, useOffset: true);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000026EC File Offset: 0x000008EC
		public static tk2dSpriteCollectionData ConstructCollection(GameObject obj, string name, bool destroyOnLoad = false)
		{
			tk2dSpriteCollectionData tk2dSpriteCollectionData = obj.AddComponent<tk2dSpriteCollectionData>();
			if (!destroyOnLoad) UnityEngine.Object.DontDestroyOnLoad(tk2dSpriteCollectionData);
			tk2dSpriteCollectionData.assetName = name;
			tk2dSpriteCollectionData.spriteCollectionGUID = name;
			tk2dSpriteCollectionData.spriteCollectionName = name;
			tk2dSpriteCollectionData.spriteDefinitions = new tk2dSpriteDefinition[0];
			tk2dSpriteCollectionData.InitDictionary();
			return tk2dSpriteCollectionData;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002738 File Offset: 0x00000938
		public static T CopyFrom<T>(this Component comp, T other) where T : Component
		{
			Type type = comp.GetType();
			T result;
			if (type != other.GetType())
			{
				result = default(T);
			}
			else
			{
				PropertyInfo[] properties = type.GetProperties();
				foreach (PropertyInfo propertyInfo in properties)
				{
					if (propertyInfo.CanWrite)
					{
						try
						{
							propertyInfo.SetValue(comp, propertyInfo.GetValue(other, null), null);
						}
						catch
						{
						}
					}
				}
				FieldInfo[] fields = type.GetFields();
				foreach (FieldInfo fieldInfo in fields)
				{
					fieldInfo.SetValue(comp, fieldInfo.GetValue(other));
				}
				result = (comp as T);
			}
			return result;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x0000282C File Offset: 0x00000A2C
		public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
		{
			return go.AddComponent<T>().CopyFrom(toAdd);
		}
	}
}
