using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alexandria.DungeonAPI;
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
			if (animator.Library == null)
			{
				animator.Library = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
				animator.Library.clips = new tk2dSpriteAnimationClip[0];
				animator.Library.enabled = true;

			}

			List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
			for (int i = 0; i < spriteIDs.Count; i++)
			{
				tk2dSpriteDefinition sprite = collection.spriteDefinitions[spriteIDs[i]];
				if (sprite.Valid)
				{
					frames.Add(new tk2dSpriteAnimationFrame()
					{
						spriteCollection = collection,
						spriteId = spriteIDs[i]
					});
				}
			}

			var clip = new tk2dSpriteAnimationClip()
			{
				name = clipName,
				fps = fps,
				wrapMode = wrapMode,
			};
			Array.Resize(ref animator.Library.clips, animator.Library.clips.Length + 1);
			animator.Library.clips[animator.Library.clips.Length - 1] = clip;

			clip.frames = frames.ToArray();
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
			List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
			for (int i = 0; i < spriteIDs.Count; i++)
			{
				tk2dSpriteDefinition sprite = collection.spriteDefinitions[spriteIDs[i]];
				if (sprite.Valid)
				{
					frames.Add(new tk2dSpriteAnimationFrame()
					{
						spriteCollection = collection,
						spriteId = spriteIDs[i]
					});
				}
			}
			var clip = new tk2dSpriteAnimationClip()
			{
				name = clipName,
				fps = fps,
				wrapMode = wrapMode,
			};
			Array.Resize(ref animaton.clips, animaton.clips.Length + 1);
			animaton.clips[animaton.clips.Length - 1] = clip;

			clip.frames = frames.ToArray();
			return clip;
		}

		public static tk2dSpriteAnimationClip AddAnimation(tk2dSpriteAnimator animator, tk2dSpriteCollectionData collection, List<string> spritePaths,
	string clipName, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, int fps = 15)
		{
			if (animator.Library == null)
			{
				animator.Library = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
				animator.Library.clips = new tk2dSpriteAnimationClip[0];
				animator.Library.enabled = true;

			}
			List<int> spriteIDs = new List<int>();

			foreach(var FUCKINGDIE in spritePaths)
            {
				spriteIDs.Add(AddSpriteToCollection(FUCKINGDIE, collection, Assembly.GetCallingAssembly()));
			}

			List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
			for (int i = 0; i < spriteIDs.Count; i++)
			{
				tk2dSpriteDefinition sprite = collection.spriteDefinitions[spriteIDs[i]];
				if (sprite.Valid)
				{
					frames.Add(new tk2dSpriteAnimationFrame()
					{
						spriteCollection = collection,
						spriteId = spriteIDs[i]
					});
				}
			}

			var clip = new tk2dSpriteAnimationClip()
			{
				name = clipName,
				fps = fps,
				wrapMode = wrapMode,
			};
			Array.Resize(ref animator.Library.clips, animator.Library.clips.Length + 1);
			animator.Library.clips[animator.Library.clips.Length - 1] = clip;

			clip.frames = frames.ToArray();
			return clip;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000238C File Offset: 0x0000058C
		public static SpeculativeRigidbody SetUpSpeculativeRigidbody(this tk2dSprite sprite, IntVector2 offset, IntVector2 dimensions)
		{
			SpeculativeRigidbody orAddComponent = sprite.gameObject.GetOrAddComponent<SpeculativeRigidbody>();
			PixelCollider pixelCollider = new PixelCollider();
			pixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
			pixelCollider.CollisionLayer = CollisionLayer.EnemyCollider;
			pixelCollider.ManualWidth = dimensions.x;
			pixelCollider.ManualHeight = dimensions.y;
			pixelCollider.ManualOffsetX = offset.x;
			pixelCollider.ManualOffsetY = offset.y;
			orAddComponent.PixelColliders = new List<PixelCollider>
			{
				pixelCollider
			};
			return orAddComponent;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002404 File Offset: 0x00000604
		public static tk2dSpriteDefinition ConstructDefinition(Texture2D texture)
		{
			RuntimeAtlasSegment runtimeAtlasSegment = ETGMod.Assets.Packer.Pack(texture, false);
			Material material = new Material(ShaderCache.Acquire(PlayerController.DefaultShaderName));
			material.mainTexture = runtimeAtlasSegment.texture;
			int width = texture.width;
			int height = texture.height;
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)width / 16f;
			float num4 = (float)height / 16f;
			tk2dSpriteDefinition tk2dSpriteDefinition = new tk2dSpriteDefinition
			{
				normals = new Vector3[]
				{
					new Vector3(0f, 0f, -1f),
					new Vector3(0f, 0f, -1f),
					new Vector3(0f, 0f, -1f),
					new Vector3(0f, 0f, -1f)
				},
				tangents = new Vector4[]
				{
					new Vector4(1f, 0f, 0f, 1f),
					new Vector4(1f, 0f, 0f, 1f),
					new Vector4(1f, 0f, 0f, 1f),
					new Vector4(1f, 0f, 0f, 1f)
				},
				texelSize = new Vector2(0.0625f, 0.0625f),
				extractRegion = false,
				regionX = 0,
				regionY = 0,
				regionW = 0,
				regionH = 0,
				flipped = tk2dSpriteDefinition.FlipMode.None,
				complexGeometry = false,
				physicsEngine = tk2dSpriteDefinition.PhysicsEngine.Physics3D,
				colliderType = tk2dSpriteDefinition.ColliderType.None,
				collisionLayer = CollisionLayer.HighObstacle,
				position0 = new Vector3(num, num2, 0f),
				position1 = new Vector3(num + num3, num2, 0f),
				position2 = new Vector3(num, num2 + num4, 0f),
				position3 = new Vector3(num + num3, num2 + num4, 0f),
				material = material,
				materialInst = material,
				materialId = 0,
				uvs = runtimeAtlasSegment.uvs,
				boundsDataCenter = new Vector3(num3 / 2f, num4 / 2f, 0f),
				boundsDataExtents = new Vector3(num3, num4, 0f),
				untrimmedBoundsDataCenter = new Vector3(num3 / 2f, num4 / 2f, 0f),
				untrimmedBoundsDataExtents = new Vector3(num3, num4, 0f)
			};
			tk2dSpriteDefinition.name = texture.name;
			return tk2dSpriteDefinition;
		}

		public static tk2dSpriteDefinition ConstructDefinition2(Texture2D texture, tk2dSpriteCollectionData collection)
		{
			RuntimeAtlasSegment runtimeAtlasSegment = ETGMod.Assets.Packer.Pack(texture, false);
			Material material = new Material(ShaderCache.Acquire(PlayerController.DefaultShaderName));
			material.mainTexture = runtimeAtlasSegment.texture;
			float width = texture.width;
			float height = texture.height;
			float num3 = width / 16;
			float num4 = height / 16f;


			Vector2 anchor = tk2dSpriteGeomGen.GetAnchorOffset(tk2dBaseSprite.Anchor.LowerLeft, num3, num4);

			float scale = 1;

			Vector3 pos0 = new Vector3(0, -height * scale, 0.0f);
			Vector3 pos1 = pos0 + new Vector3(num3 * scale, height * scale, 0.0f);

			Rect trimRect = new Rect(0, 0, 0, 0);
			trimRect.Set(0, 0, num3, num4);

			Vector2 offset = new Vector2(trimRect.x - anchor.x, -trimRect.y + anchor.y);


			tk2dSpriteDefinition tk2dSpriteDefinition = new tk2dSpriteDefinition
			{
				normals = new Vector3[]
				{
					new Vector3(0f, 0f, -1f),
					new Vector3(0f, 0f, -1f),
					new Vector3(0f, 0f, -1f),
					new Vector3(0f, 0f, -1f)
				},
				tangents = new Vector4[]
				{
					new Vector4(1f, 0f, 0f, 1f),
					new Vector4(1f, 0f, 0f, 1f),
					new Vector4(1f, 0f, 0f, 1f),
					new Vector4(1f, 0f, 0f, 1f)
				},
				texelSize = new Vector2(0.0625f, 0.0625f),
				extractRegion = false,
				regionX = 0,
				regionY = 0,
				regionW = 0,
				regionH = 0,
				flipped = tk2dSpriteDefinition.FlipMode.None,
				complexGeometry = false,
				physicsEngine = tk2dSpriteDefinition.PhysicsEngine.Physics3D,
				colliderType = tk2dSpriteDefinition.ColliderType.None,
				collisionLayer = CollisionLayer.HighObstacle,

				position0 = new Vector3(pos0.x + offset.x, pos0.y + offset.y, 0),
				position1 = new Vector3(pos1.x + offset.x, pos0.y + offset.y, 0),
				position2 = new Vector3(pos0.x + offset.x, pos1.y + offset.y, 0),
				position3 = new Vector3(pos1.x + offset.x, pos1.y + offset.y, 0),

				material = material,
				materialInst = material,
				materialId = 0,
				uvs = runtimeAtlasSegment.uvs,
				boundsDataCenter = new Vector3(num3 / 2f, num4 / 2f, 0f),
				boundsDataExtents = new Vector3(num3, num4, 0f),
				untrimmedBoundsDataCenter = new Vector3(num3 / 2f, num4 / 2f, 0f),
				untrimmedBoundsDataExtents = new Vector3(num3, num4, 0f)
			};
			tk2dSpriteDefinition.name = texture.name;
			return tk2dSpriteDefinition;
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
