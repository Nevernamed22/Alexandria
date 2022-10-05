using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Alexandria.ItemAPI
{

    public static class GunTools
    {
        public static Texture2D DesheetTexture(this tk2dSpriteDefinition definition)
        {
            if (definition?.material?.mainTexture != null && definition.material.mainTexture is Texture2D tex)
            {
                var sheet = tex.GetRW();
                var sheetWidth = sheet.width;
                var sheetHeight = sheet.height;
                var uv = definition.uvs;
                if (uv.Length >= 4)
                {
                    var x = Mathf.RoundToInt(uv[0].x * sheetWidth);
                    var y = Mathf.RoundToInt(uv[0].y * sheetHeight);
                    var width = Mathf.RoundToInt((uv[3].x - uv[0].x) * sheetWidth);
                    var height = Mathf.RoundToInt((uv[3].y - uv[0].y) * sheetHeight);
                    var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    texture.SetPixels(sheet.GetPixels(x, y, width, height));
                    texture.Apply(false, false);
                    texture.name = definition.name;
                    return texture;
                }
            }
            return null;
        }
        public static void TrimGunSpritesForSpecificAnims(this Gun gun, params string[] anims)
        {
            List<KeyValuePair<tk2dSpriteCollectionData, int>> ids = new List<KeyValuePair<tk2dSpriteCollectionData, int>>();

            anims.ToList().ForEach(x => gun.TryTrimGunAnimation(x, ids));

            var defaultId = gun.sprite.spriteId;
            var defaultDefinition = gun.sprite.Collection.spriteDefinitions[defaultId];
            var globalOffset = new Vector2(-defaultDefinition.position0.x, -defaultDefinition.position0.y);
            foreach (var x in ids)
            {
                x.Key?.spriteDefinitions[x.Value]?.AddOffset(globalOffset);
                var attach = x.Key?.GetAttachPoints(x.Value);
                if (attach == null)
                {
                    continue;
                }
                foreach (var attachPoint in attach)
                {
                    attachPoint.position += globalOffset.ToVector3ZUp(0f);
                }
            };
        }
        public static void TrimGunSprites(this Gun gun)
        {
            List<KeyValuePair<tk2dSpriteCollectionData, int>> ids = new List<KeyValuePair<tk2dSpriteCollectionData, int>>();
            gun.TryTrimGunAnimation(gun.shootAnimation, ids);
            gun.TryTrimGunAnimation(gun.reloadAnimation, ids);
            gun.TryTrimGunAnimation(gun.emptyReloadAnimation, ids);
            gun.TryTrimGunAnimation(gun.idleAnimation, ids);
            gun.TryTrimGunAnimation(gun.chargeAnimation, ids);
            gun.TryTrimGunAnimation(gun.dischargeAnimation, ids);
            gun.TryTrimGunAnimation(gun.emptyAnimation, ids);
            gun.TryTrimGunAnimation(gun.introAnimation, ids);
            gun.TryTrimGunAnimation(gun.finalShootAnimation, ids);
            gun.TryTrimGunAnimation(gun.enemyPreFireAnimation, ids);
            gun.TryTrimGunAnimation(gun.outOfAmmoAnimation, ids);
            gun.TryTrimGunAnimation(gun.criticalFireAnimation, ids);
            gun.TryTrimGunAnimation(gun.dodgeAnimation, ids);
            gun.TryTrimGunAnimation(gun.alternateIdleAnimation, ids);
            gun.TryTrimGunAnimation(gun.alternateReloadAnimation, ids);
            gun.TryTrimGunAnimation(gun.alternateShootAnimation, ids);
            var defaultId = gun.sprite.spriteId;
            var defaultDefinition = gun.sprite.Collection.spriteDefinitions[defaultId];
            var globalOffset = new Vector2(-defaultDefinition.position0.x, -defaultDefinition.position0.y);
            foreach (var x in ids)
            {
                x.Key?.spriteDefinitions[x.Value]?.AddOffset(globalOffset);
                var attach = x.Key?.GetAttachPoints(x.Value);
                if (attach == null)
                {
                    continue;
                }
                foreach (var attachPoint in attach)
                {
                    attachPoint.position += globalOffset.ToVector3ZUp(0f);
                }
            };
            gun.barrelOffset.localPosition += globalOffset.ToVector3ZUp(0f);
        }
        public static void TryTrimGunAnimation(this Gun gun, string animation, List<KeyValuePair<tk2dSpriteCollectionData, int>> ids)
        {
            if (!string.IsNullOrEmpty(animation) && gun.spriteAnimator != null)
            {
                var clip = gun.spriteAnimator.GetClipByName(animation);
                if (clip != null)
                {
                    foreach (var frame in clip.frames)
                    {
                        if (frame.spriteCollection?.spriteDefinitions != null && frame.spriteId >= 0 && frame.spriteId < frame.spriteCollection.spriteDefinitions.Length)
                        {
                            var definition = frame.spriteCollection.spriteDefinitions[frame.spriteId];
                            ETGMod.Assets.TextureMap.TryGetValue("sprites/" + frame.spriteCollection.name + "/" + definition.name, out var texture);
                            if (texture != null && definition != null)
                            {
                                var pixelOffset = texture.TrimTexture();
                                RuntimeAtlasSegment ras = ETGMod.Assets.Packer.Pack(texture); //pack your resources beforehand or the outlines will turn out weird

                                Material material = new Material(definition.material);
                                material.mainTexture = ras.texture;
                                definition.uvs = ras.uvs;
                                definition.material = material;
                                if (definition.materialInst != null)
                                {
                                    definition.materialInst = new Material(material);
                                }
                                float num = texture.width * 0.0625f;
                                float num2 = texture.height * 0.0625f;
                                definition.position0 = new Vector3(0f, 0f, 0f);
                                definition.position1 = new Vector3(num, 0f, 0f);
                                definition.position2 = new Vector3(0f, num2, 0f);
                                definition.position3 = new Vector3(num, num2, 0f);
                                definition.boundsDataCenter = definition.untrimmedBoundsDataCenter = new Vector3(num / 2f, num2 / 2f, 0f);
                                definition.boundsDataExtents = definition.untrimmedBoundsDataExtents = new Vector3(num, num2, 0f);
                                definition.AddOffset(pixelOffset.ToVector2() / 16f);
                                ids.Add(new KeyValuePair<tk2dSpriteCollectionData, int>(frame.spriteCollection, frame.spriteId));
                            }
                        }
                    }
                }
            }
        }
        public static void AddOffset(this tk2dSpriteDefinition def, Vector2 offset, bool changesCollider = false)
        {
            float xOffset = offset.x;
            float yOffset = offset.y;
            def.position0 += new Vector3(xOffset, yOffset, 0);
            def.position1 += new Vector3(xOffset, yOffset, 0);
            def.position2 += new Vector3(xOffset, yOffset, 0);
            def.position3 += new Vector3(xOffset, yOffset, 0);
            def.boundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.boundsDataExtents += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataExtents += new Vector3(xOffset, yOffset, 0);
            if (def.colliderVertices != null && def.colliderVertices.Length > 0 && changesCollider)
            {
                def.colliderVertices[0] += new Vector3(xOffset, yOffset, 0);
            }
        }
        public static IntVector2 TrimTexture(this Texture2D orig)
        {
            RectInt bounds = orig.GetTrimmedBounds();
            Color[][] pixels = new Color[bounds.width][];

            for (int x = bounds.x; x < bounds.x + bounds.width; x++)
            {
                for (int y = bounds.y; y < bounds.y + bounds.height; y++)
                {
                    if (pixels[x - bounds.x] == null)
                    {
                        pixels[x - bounds.x] = new Color[bounds.height];
                    }
                    pixels[x - bounds.x][y - bounds.y] = orig.GetPixel(x, y);
                }
            }

            orig.Resize(bounds.width, bounds.height);

            for (int x = 0; x < bounds.width; x++)
            {
                for (int y = 0; y < bounds.height; y++)
                {
                    orig.SetPixel(x, y, pixels[x][y]);
                }
            }
            orig.Apply(false, false);
            return new IntVector2(bounds.x, bounds.y);
        }
        public static RectInt GetTrimmedBounds(this Texture2D t)
        {

            int xMin = t.width;
            int yMin = t.height;
            int xMax = 0;
            int yMax = 0;

            for (int x = 0; x < t.width; x++)
            {
                for (int y = 0; y < t.height; y++)
                {
                    if (t.GetPixel(x, y).a > 0)
                    {
                        if (x < xMin) xMin = x;
                        if (y < yMin) yMin = y;
                        if (x > xMax) xMax = x;
                        if (y > yMax) yMax = y;
                    }
                }
            }

            return new RectInt(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1);
        }

        public static tk2dSpriteDefinition Copy(this tk2dSpriteDefinition other)
        {
            tk2dSpriteDefinition result = new tk2dSpriteDefinition
            {
                boundsDataCenter = other.boundsDataCenter,
                boundsDataExtents = other.boundsDataExtents,
                colliderConvex = other.colliderConvex,
                colliderSmoothSphereCollisions = other.colliderSmoothSphereCollisions,
                colliderType = other.colliderType,
                colliderVertices = other.colliderVertices,
                collisionLayer = other.collisionLayer,
                complexGeometry = other.complexGeometry,
                extractRegion = other.extractRegion,
                flipped = other.flipped,
                indices = other.indices,
                material = new Material(other.material),
                materialId = other.materialId,
                materialInst = new Material(other.materialInst ?? other.material),
                metadata = other.metadata,
                name = other.name,
                normals = other.normals,
                physicsEngine = other.physicsEngine,
                position0 = other.position0,
                position1 = other.position1,
                position2 = other.position2,
                position3 = other.position3,
                regionH = other.regionH,
                regionW = other.regionW,
                regionX = other.regionX,
                regionY = other.regionY,
                tangents = other.tangents,
                texelSize = other.texelSize,
                untrimmedBoundsDataCenter = other.untrimmedBoundsDataCenter,
                untrimmedBoundsDataExtents = other.untrimmedBoundsDataExtents,
                uvs = other.uvs,                
            };
            return result;
        }

        public static tk2dSpriteDefinition CopyDefinitionFrom(this tk2dSpriteDefinition other)
        {
            if (other.boundsDataCenter == null) Debug.LogWarning("FUUUUUUUUUUUUUUUUUUUUUCK");
            if (other.boundsDataExtents == null) Debug.LogWarning("FUUUUUUUUUUUUUUUUUUUUUCK2");
            tk2dSpriteDefinition result = new tk2dSpriteDefinition
            {
                boundsDataCenter = new Vector3
                {
                    x = other.boundsDataCenter.x,
                    y = other.boundsDataCenter.y,
                    z = other.boundsDataCenter.z
                },
                boundsDataExtents = new Vector3
                {
                    x = other.boundsDataExtents.x,
                    y = other.boundsDataExtents.y,
                    z = other.boundsDataExtents.z
                },
                colliderConvex = other.colliderConvex,
                colliderSmoothSphereCollisions = other.colliderSmoothSphereCollisions,
                colliderType = other.colliderType,
                colliderVertices = other.colliderVertices,
                collisionLayer = other.collisionLayer,
                complexGeometry = other.complexGeometry,
                extractRegion = other.extractRegion,
                flipped = other.flipped,
                indices = other.indices,              
                material = new Material(other.material),
                materialId = other.materialId,
                materialInst = new Material(other.materialInst ?? other.material),
                metadata = other.metadata,
                name = other.name,
                normals = other.normals,
                physicsEngine = other.physicsEngine,
                position0 = new Vector3
                {
                    x = other.position0.x,
                    y = other.position0.y,
                    z = other.position0.z
                },
                position1 = new Vector3
                {
                    x = other.position1.x,
                    y = other.position1.y,
                    z = other.position1.z
                },
                position2 = new Vector3
                {
                    x = other.position2.x,
                    y = other.position2.y,
                    z = other.position2.z
                },
                position3 = new Vector3
                {
                    x = other.position3.x,
                    y = other.position3.y,
                    z = other.position3.z
                },
                regionH = other.regionH,
                regionW = other.regionW,
                regionX = other.regionX,
                regionY = other.regionY,
                tangents = other.tangents,
                texelSize = new Vector2
                {
                    x = other.texelSize.x,
                    y = other.texelSize.y
                },
                untrimmedBoundsDataCenter = new Vector3
                {
                    x = other.untrimmedBoundsDataCenter.x,
                    y = other.untrimmedBoundsDataCenter.y,
                    z = other.untrimmedBoundsDataCenter.z
                },
                untrimmedBoundsDataExtents = new Vector3
                {
                    x = other.untrimmedBoundsDataExtents.x,
                    y = other.untrimmedBoundsDataExtents.y,
                    z = other.untrimmedBoundsDataExtents.z
                }
            };
            List<Vector2> uvs = new List<Vector2>();
            foreach (Vector2 vector in other.uvs)
            {
                uvs.Add(new Vector2
                {
                    x = vector.x,
                    y = vector.y
                });
            }
            result.uvs = uvs.ToArray();
            List<Vector3> colliderVertices = new List<Vector3>();
            foreach (Vector3 vector in other.colliderVertices)
            {
                colliderVertices.Add(new Vector3
                {
                    x = vector.x,
                    y = vector.y,
                    z = vector.z
                });
            }
            result.colliderVertices = colliderVertices.ToArray();
            return result;
        }
        public static tk2dSpriteDefinition SetupDefinitionForProjectileSprite(string name, int id, int pixelWidth, int pixelHeight, bool lightened = true, int? overrideColliderPixelWidth = null, int? overrideColliderPixelHeight = null,
            int? overrideColliderOffsetX = null, int? overrideColliderOffsetY = null, Projectile overrideProjectileToCopyFrom = null)
        {
            if (overrideColliderPixelWidth == null)
            {
                overrideColliderPixelWidth = pixelWidth;
            }
            if (overrideColliderPixelHeight == null)
            {
                overrideColliderPixelHeight = pixelHeight;
            }
            if (overrideColliderOffsetX == null)
            {
                overrideColliderOffsetX = 0;
            }
            if (overrideColliderOffsetY == null)
            {
                overrideColliderOffsetY = 0;
            }
            float thing = 16;
            float thing2 = 16;
            float trueWidth = (float)pixelWidth / thing;
            float trueHeight = (float)pixelHeight / thing;
            float colliderWidth = (float)overrideColliderPixelWidth.Value / thing2;
            float colliderHeight = (float)overrideColliderPixelHeight.Value / thing2;
            float colliderOffsetX = (float)overrideColliderOffsetX.Value / thing2;
            float colliderOffsetY = (float)overrideColliderOffsetY.Value / thing2;
            tk2dSpriteDefinition def = ETGMod.Databases.Items.ProjectileCollection.inst.spriteDefinitions[(overrideProjectileToCopyFrom ??
                    (PickupObjectDatabase.GetById(lightened ? 47 : 12) as Gun).DefaultModule.projectiles[0]).GetAnySprite().spriteId].CopyDefinitionFrom();
            def.boundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.boundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.untrimmedBoundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.untrimmedBoundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.texelSize = new Vector2(1 / 16f, 1 / 16f);
            def.position0 = new Vector3(0f, 0f, 0f);
            def.position1 = new Vector3(0f + trueWidth, 0f, 0f);
            def.position2 = new Vector3(0f, 0f + trueHeight, 0f);
            def.position3 = new Vector3(0f + trueWidth, 0f + trueHeight, 0f);
            def.colliderVertices = new Vector3[2];
            def.colliderVertices[0] = new Vector3(colliderOffsetX, colliderOffsetY, 0f);
            def.colliderVertices[1] = new Vector3(colliderWidth / 2, colliderHeight / 2);
            def.name = name;

            def.materialInst.mainTexture = ETGMod.Databases.Items.ProjectileCollection.inst.spriteDefinitions[id].materialInst.mainTexture;
            def.uvs = ETGMod.Databases.Items.ProjectileCollection.inst.spriteDefinitions[id].uvs.ToArray();

            ETGMod.Databases.Items.ProjectileCollection.inst.spriteDefinitions[id] = def;

            return def;
        }
        public static tk2dSpriteDefinition SetProjectileSpriteRight(this Projectile proj, string name, int pixelWidth, int pixelHeight, bool lightened = true, tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.LowerLeft, int? overrideColliderPixelWidth = null, int? overrideColliderPixelHeight = null, bool anchorChangesCollider = true,
            bool fixesScale = false, int? overrideColliderOffsetX = null, int? overrideColliderOffsetY = null, Projectile overrideProjectileToCopyFrom = null)
        {
            try
            {
                proj.GetAnySprite().SetSprite(ETGMod.Databases.Items.ProjectileCollection.inst, ETGMod.Databases.Items.ProjectileCollection.inst.GetSpriteIdByName(name));
                tk2dSpriteDefinition def = SetupDefinitionForProjectileSprite(name, proj.GetAnySprite().spriteId, pixelWidth, pixelHeight, lightened, overrideColliderPixelWidth, overrideColliderPixelHeight, overrideColliderOffsetX,
                    overrideColliderOffsetY, overrideProjectileToCopyFrom);
                def.ConstructOffsetsFromAnchor(anchor, def.position3, fixesScale, anchorChangesCollider);
                proj.GetAnySprite().scale = new Vector3(1f, 1f, 1f);
                proj.transform.localScale = new Vector3(1f, 1f, 1f);
                proj.GetAnySprite().transform.localScale = new Vector3(1f, 1f, 1f);
                proj.AdditionalScaleMultiplier = 1f;
                return def;
            }
            catch (Exception ex)
            {
                ETGModConsole.Log("Ooops! Seems like something got very, Very, VERY wrong. Here's the exception:");
                ETGModConsole.Log(ex.ToString());
                return null;
            }
        }
        public static void MakeOffset(this tk2dSpriteDefinition def, Vector2 offset, bool changesCollider = false)
        {
            float xOffset = offset.x;
            float yOffset = offset.y;
            def.position0 += new Vector3(xOffset, yOffset, 0);
            def.position1 += new Vector3(xOffset, yOffset, 0);
            def.position2 += new Vector3(xOffset, yOffset, 0);
            def.position3 += new Vector3(xOffset, yOffset, 0);
            def.boundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.boundsDataExtents += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataExtents += new Vector3(xOffset, yOffset, 0);
            if (def.colliderVertices != null && def.colliderVertices.Length > 0 && changesCollider)
            {
                def.colliderVertices[0] += new Vector3(xOffset, yOffset, 0);
            }
        }
        public static void ConstructOffsetsFromAnchor(this tk2dSpriteDefinition def, tk2dBaseSprite.Anchor anchor, Vector2? scale = null, bool fixesScale = false, bool changesCollider = true)
        {
            if (!scale.HasValue)
            {
                scale = new Vector2?(def.position3);
            }
            if (fixesScale)
            {
                Vector2 fixedScale = scale.Value - def.position0.XY();
                scale = new Vector2?(fixedScale);
            }
            float xOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.UpperCenter)
            {
                xOffset = -(scale.Value.x / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                xOffset = -scale.Value.x;
            }
            float yOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.MiddleLeft)
            {
                yOffset = -(scale.Value.y / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                yOffset = -scale.Value.y;
            }
            def.MakeOffset(new Vector2(xOffset, yOffset), false);
            if (changesCollider && def.colliderVertices != null && def.colliderVertices.Length > 0)
            {
                float colliderXOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.LowerLeft || anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.UpperLeft)
                {
                    colliderXOffset = (scale.Value.x / 2f);
                }
                else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    colliderXOffset = -(scale.Value.x / 2f);
                }
                float colliderYOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.LowerLeft || anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.LowerRight)
                {
                    colliderYOffset = (scale.Value.y / 2f);
                }
                else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    colliderYOffset = -(scale.Value.y / 2f);
                }
                def.colliderVertices[0] += new Vector3(colliderXOffset, colliderYOffset, 0);
            }
        }
        public static void SetFields(this Component comp, Component other, bool includeFields = true, bool includeProperties = true)
        {
            if (comp != null && other != null)
            {
                Type type = comp.GetType();
                if (type != other.GetType())
                {
                    ETGModConsole.Log(" type mis-match");
                    return;
                } // type mis-match
                if (includeProperties)
                {
                    PropertyInfo[] pinfos = type.GetProperties();
                    foreach (var pinfo in pinfos)
                    {
                        if (pinfo.CanWrite)
                        {
                            try
                            {
                                pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                            }
                            catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                        }
                    }
                }
                if (includeFields)
                {
                    FieldInfo[] finfos = type.GetFields();
                    foreach (var finfo in finfos)
                    {
                        try
                        {
                            finfo.SetValue(comp, finfo.GetValue(other));
                        }
                        catch { }
                    }
                }
            }
        }
        public static bool IsCurrentGun(this Gun gun)
        {
            if (gun && gun.CurrentOwner)
            {
                if (gun.CurrentOwner.CurrentGun == gun) return true;
                else return false;
            }
            else return false;
        }
        public static PlayerController GunPlayerOwner(this Gun bullet)
        {
            if (bullet && bullet.CurrentOwner && bullet.CurrentOwner is PlayerController) return bullet.CurrentOwner as PlayerController;
            else return null;
        }
        public static ProjectileModule AddProjectileModuleToRawVolley(this Gun gun, ProjectileModule projectile)
        {
            gun.RawSourceVolley.projectiles.Add(projectile);
            return projectile;
        }
        public static ProjectileModule AddProjectileModuleToRawVolleyFrom(this Gun gun, Gun other, bool cloned = true)
        {
            ProjectileModule defaultModule = other.DefaultModule;
            if (!cloned)
            {
                return gun.AddProjectileModuleToRawVolley(defaultModule);
            }
            ProjectileModule projectileModule = ProjectileModule.CreateClone(defaultModule, false, -1);
            projectileModule.projectiles = new List<Projectile>(defaultModule.projectiles.Capacity);
            for (int i = 0; i < defaultModule.projectiles.Count; i++)
            {
                projectileModule.projectiles.Add(defaultModule.projectiles[i]);
            }
            return gun.AddProjectileModuleToRawVolley(projectileModule);
        }
        public static ProjectileModule RawDefaultModule(this Gun self)
        {
            if (self.RawSourceVolley)
            {
                if (self.RawSourceVolley.ModulesAreTiers)
                {
                    for (int i = 0; i < self.RawSourceVolley.projectiles.Count; i++)
                    {
                        ProjectileModule projectileModule = self.RawSourceVolley.projectiles[i];
                        if (projectileModule != null)
                        {
                            int num = (projectileModule.CloneSourceIndex < 0) ? i : projectileModule.CloneSourceIndex;
                            if (num == self.CurrentStrengthTier)
                            {
                                return projectileModule;
                            }
                        }
                    }
                }
                return self.RawSourceVolley.projectiles[0];
            }
            return self.singleModule;
        }
        public static void AddStatToGun(this Gun item, PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            StatModifier modifier = new StatModifier
            {
                amount = amount,
                statToBoost = statType,
                modifyType = method
            };

            if (item.passiveStatModifiers == null)
                item.passiveStatModifiers = new StatModifier[] { modifier };
            else
                item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
        }
        public static void RemoveStatFromGun(this Gun item, PlayerStats.StatType statType)
        {
            var newModifiers = new List<StatModifier>();
            for (int i = 0; i < item.passiveStatModifiers.Length; i++)
            {
                if (item.passiveStatModifiers[i].statToBoost != statType)
                    newModifiers.Add(item.passiveStatModifiers[i]);
            }
            item.passiveStatModifiers = newModifiers.ToArray();
        }
    }  
}
