using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    public class GridLayoutAssetItemDrawer : AssetItemDrawer,IDisposable
    {
        public override event Action<AssetItem, Object> Clicked;
        public override event Action<AssetItem, Object> DoubleClicked;
        public override event Action<AssetItem, Object> LabelClicked;
        // public override event Action<AssetItem, Object> DragStarted;
        // public override event Action<AssetItem, Object,bool> Dragged;

        private readonly GUIStyle _selectedLabelStyle;
        private readonly Texture2D _selectedTexture;
        private readonly GUIStyle _highlightedLabelStyle;
        private readonly Texture2D _highlightedTexture;
        // private readonly Texture2D _subAssetButtonReverseIcon;
        // private readonly Texture2D _subAssetButtonNormalIcon;
        public override Action Repaint { get; set; }

        public GridLayoutAssetItemDrawer()
        {
            //  _selectedTexture = new Texture2D(1, 1);
            // _selectedTexture.SetPixel(0, 0, new Color(52/255f,88/255f,119/255f));
            // _selectedTexture.Apply();
            _selectedLabelStyle = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    textColor = Color.white,
                    // background =  _selectedTexture,
                },
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                overflow = new RectOffset(5, 5, 0, 0),
                border = new RectOffset(5,5,5,5),
            };
            
            _highlightedTexture = new Texture2D(1, 1);
            _highlightedTexture.SetPixel(0, 0, Color.green);
            _highlightedTexture.Apply();
            
            _highlightedLabelStyle = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    textColor = Color.green,
                    background = null// _highlightedTexture,
                    
                },
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                overflow = new RectOffset(5, 5, 0, 0),
                border = new RectOffset(5,5,5,5),
            };

            // _subAssetButtonNormalIcon = Clone((Texture2D)EditorGUIUtility.IconContent("d_Animation Icon").image);
            // _subAssetButtonReverseIcon = FlipTexture((Texture2D)EditorGUIUtility.IconContent("d_Animation Icon").image);

        }

        private static Texture2D Clone(Texture2D texture)
        {
            var tex = new Texture2D(texture.width,texture.height);
            tex.SetPixels(texture.GetPixels());
            tex.Apply();

            return tex;
        }
        
        public static Texture2D FlipTexture(Texture2D texture)
        {
            var tex = Clone(texture);
            
            var pixels = tex.GetPixels();
            Array.Reverse(pixels);
            tex.SetPixels(pixels);

            return tex;
        }

        public void Dispose()
        {
            Object.DestroyImmediate(_highlightedTexture);
            Object.DestroyImmediate(_selectedTexture);
            // Object.DestroyImmediate(_subAssetButtonNormalIcon);
            // Object.DestroyImmediate(_subAssetButtonReverseIcon);
        }

        public override IEnumerable<(AssetItem,ItemWithLabel[])> DrawAssets(IEnumerable<AssetItem> assetsItems)
        {
            var assets = assetsItems.ToList();
            const float tileWidth = 120f;
            const float spacing = 25f;
            // Debug.Log(nameof(DrawAsset)+string.Join(",",assets.Where(a=>a.Expanded).Select(i=>i.Object?.name)));

            var widths = assets.Select(p =>
                    Enumerable.Range(0, 1 + (p.Expanded ? p.SubAssets.Count() : 0))).SelectMany(ps => ps)
                .Select(_ => tileWidth).ToList();

            var rects = new List<Rect>();
            
            
            
            EditorGUILayout.BeginVertical();
            while (rects.Count < widths.Count)
            {
            
                GUILayout.Space(5);
                var rs = DrawHorizontal(widths, Width, tileWidth + 21
                    , spacing, 15).ToArray();
                rects.AddRange(rs);
                GUILayout.Space(5);

            
            
                if (rs.Length == 0)
                    break;
            }
            EditorGUILayout.EndVertical();
            var rectIndex = 0;
            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                var currentRects = rects.Skip(rectIndex).Take(asset.Expanded ? asset.SubAssets.Count() + 1 : 1).Select(r=>new ItemWithLabel
                    {
                        TotalRect = r,
                        Label = new Rect(r.x,r.y+r.width+5, r.width,r.height - r.width-5)
                    })
                    .ToArray();
                
                // foreach (var itemWithLabel in currentRects)
                // {
                //     EditorGUI.DrawRect(itemWithLabel.TotalRect,Color.gray);
                //     // EditorGUI.DrawRect(itemWithLabel.Label,Color.red);
                // }
                DrawAsset(asset, currentRects);
                yield return (asset, currentRects);
                    rectIndex += currentRects.Length;
            }
            yield break;
        }

        private void DrawAsset(AssetItem item, IReadOnlyList<ItemWithLabel> rects)
        {
            // Debug.Log($"{item.Object?.name}:{item.Path}");
            var expanded = item.Expanded;
            DrawPrimaryAsset(item,
                rects.First());

            // Debug.Log($"{item.SubAssets.Count()}:{rects.Count}");
            
            // HandleDragEvent(item,item.Object,rects.First());
            
            if (expanded)
            {
                var assets = item.SubAssets.ToArray();
                DrawSubAssetBg(rects.Skip(1).Select(r =>
                {
                    var rect = new Rect(r.TotalRect);
                    rect.height = rect.width;
                    return rect;
                }).ToArray());
                for (var i = 0; i < assets.Length; i++)
                {
                    var asset = assets[i];
                    DrawSubAsset(item,asset, AssetPreview.GetAssetPreview(asset) ?? AssetDatabase.GetCachedIcon(item.Path),
                        Selection.objects.Any(o => o == asset), rects[i + 1]);
                    // HandleDragEvent(item,assets[i],rects[i+1]);

                }
            }
        }

        // private void HandleDragEvent(AssetItem item, Object itemObject,Rect rect)
        // {
        //     HandleDraggingEvent(item,itemObject,rect);
        //     // HandleDragStartedEvent(item,itemObject,rect);
        // }
        
        // private void HandleDragStartedEvent(AssetItem item, Object itemObject,Rect rect)
        // {
        //     if (Event.current is { type: EventType.MouseDrag } && rect.Contains(Event.current.mousePosition))
        //     {
        //         DragStarted?.Invoke(item, itemObject);
        //         Event.current.Use();
        //     }
        // }
        
        // private void HandleDraggingEvent(AssetItem item, Object itemObject,Rect rect)
        // {
        //
        //     if (Event.current is { type: EventType.DragUpdated })
        //     {
        //         // Debug.Log(item.Path+":"+rect.Contains(Event.current.mousePosition));
        //         Dragged?.Invoke(item, itemObject,rect.Contains(Event.current.mousePosition));
        //         // Event.current.Use();
        //     }
        // }

        private void DrawSubAssetBg(IReadOnlyList<Rect> rects)
        {
            if (!rects.Any())
                return;

            var currentYIndex = 0;
            var currentY = rects.First().y;

            for (var i = 0; i < rects.Count; i++)
            {
                var rect = rects[i];
                // Debug.Log(Mathf.Abs(rect.y - currentY));

                if (Mathf.Abs(rect.y - currentY) > 0)
                {
                    DrawSubBackground(new Rect(rects[currentYIndex].x, rects[currentYIndex].y,
                        rects[i - 1].x + rects[i - 1].width - rects[currentYIndex].x, rect.height));
                    currentYIndex = i;
                    Debug.Log(i);
                    currentY = rect.y;
                }
            }

            // Debug.Log($"{currentYIndex}:{rects.Count}");
            if (currentYIndex < rects.Count)
                DrawSubBackground(new Rect(rects[currentYIndex].x, rects[currentYIndex].y,
                    rects.Last().x + rects.Last().width - rects[currentYIndex].x, rects.Last().height));
        }

        private void DrawSubBackground(Rect rect, bool continuing = false)
        {
            Debug.Log($"{nameof(DrawSubBackground)}:{rect}");
            // var texture = new Texture2D(1, 1);
            // texture.SetPixel(0, 0, new Color(0.1f,0.1f,0.1f));
            // texture.Apply();
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0,
                new Color(0.15f, 0.15f, 0.15f), 0, 10f);
            // DestroyImmediate(texture);
        }

        private void DrawPrimaryAsset(AssetItem item, ItemWithLabel itemWithRect)
        {
            var selected = Selection.objects.Any(o => o == item.Object);

            var rect = itemWithRect.TotalRect;
            var iconRect = new Rect(rect.position, new Vector2(rect.width, rect.width));
            var labelRect = itemWithRect.Label;
            var lastColor = GUI.color;
            GUI.color = selected?new Color(0.9f, 0.9f, 1):Color.white;
            var offsetIcon = rect.width / 6f;

            GUI.DrawTexture(
                new Rect(iconRect.x + offsetIcon, iconRect.y + offsetIcon, iconRect.width - 2 * offsetIcon,
                    iconRect.height - 2 * offsetIcon), item.Icon, ScaleMode.ScaleToFit);
            GUI.color = lastColor;
           


            var style = selected ? _selectedLabelStyle :
                item.IsHighlighting ? _highlightedLabelStyle : EditorStyles.label;
            if (!item.IsRenaming && !item.IsCreating)
            {
                var text = GetClippingText(item.Name, labelRect,GUI.skin.label);
                var labelWidth = style.CalcSize(new GUIContent(text)).x;
                labelRect.x += (labelRect.width - labelWidth) / 2;
                labelRect.width = labelWidth;

                if(selected)
                {
                    var labelBgRect = new Rect(labelRect.x - 5, labelRect.y, labelRect.width + 10, labelRect.height);
                    GUI.DrawTexture(labelBgRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0,
                        new Color(52/255f,88/255f,119/255f), 0, 4f);
                }

                GUI.Label(labelRect, text, style);
            }
            else if (item.Object)
            {
                item.RenamingText = GUI.TextField(labelRect, item.RenamingText);
            }


            const float subIconWidth = 30f;
            var subIconButtonRect = new Rect(rect.x + rect.width - subIconWidth / 2,
                rect.y + rect.height / 2 - subIconWidth / 2, subIconWidth, subIconWidth);
            // var texture = item.Expanded ? _subAssetButtonReverseIcon : _subAssetButtonNormalIcon;
            if (item.SubAssets.Any())
            {
                
                if (GUI.Button(subIconButtonRect, ""))
                {
                    item.Expanded = !item.Expanded;
                    Repaint?.Invoke();
                    return;
                }

                var subIconRect = new Rect(subIconButtonRect.x + 3, subIconButtonRect.y + 3,
                    subIconButtonRect.width - 6, subIconButtonRect.height - 6);
                GUI.DrawTextureWithTexCoords(subIconRect,EditorGUIUtility.IconContent("d_Animation Icon").image,new Rect(item.Expanded?1:0,0,item.Expanded?-1:1,1));
            }



            // if (Event.current is { type: EventType.MouseUp, clickCount: 1 })
            // {
            //     if (rect.Contains(Event.current.mousePosition))
            //     {
            //         Event.current.Use();
            //         Clicked?.Invoke(item, item.Object);
            //     }
            //
            //     if (labelRect.Contains(Event.current.mousePosition) && selected)
            //     {
            //         LabelClicked?.Invoke(item, item.Object);
            //         Event.current.Use();
            //     }
            // }
            //
            //
            // if (Event.current is { type: EventType.MouseDown, clickCount: 2 } &&
            //     rect.Contains(Event.current.mousePosition))
            // {
            //     // Debug.Log("Doubled Clicked:" + item.Object.name + " Path:" + item.Path);
            //     Event.current.Use();
            //     DoubleClicked?.Invoke(item, item.Object);
            // }
        }

        private void DrawSubAsset(AssetItem item, Object asset, Texture icon, bool selected, ItemWithLabel itemWithLabel)
        {
            var rect = itemWithLabel.TotalRect;
            var iconRect = new Rect(rect.position, new Vector2(rect.width, rect.width));
            var labelRect = itemWithLabel.Label;
            var lastColor = GUI.color;
                        GUI.color = selected?new Color(0.9f, 0.9f, 1):Color.white;
                        var offsetIcon = rect.width / 6f;

            GUI.DrawTexture(
                new Rect(iconRect.x + offsetIcon, iconRect.y + offsetIcon, iconRect.width - 2 * offsetIcon,
                    iconRect.height - 2 * offsetIcon), icon, ScaleMode.ScaleToFit);
            GUI.color = lastColor;
           
            var style = selected ? _selectedLabelStyle :
                item.IsHighlighting ? _highlightedLabelStyle : EditorStyles.label;


            if (asset)
            {
                var text = GetClippingText(asset.name, labelRect,GUI.skin.label);
                var labelWidth = style.CalcSize(new GUIContent(text)).x;
                labelRect.x += (labelRect.width - labelWidth) / 2;
                labelRect.width = labelWidth;

                if(selected)
                {
                    var labelBgRect = new Rect(labelRect.x - 5, labelRect.y, labelRect.width + 10, labelRect.height);
                    GUI.DrawTexture(labelBgRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0,
                        new Color(52/255f,88/255f,119/255f), 0, 4f);
                }
                GUI.Label(labelRect, text, style);

            }

         
        }

        public static string GetClippingText(string text, Rect area,GUIStyle style)
        {
            const string ellipsis = "...";
            if (style.CalcSize(new GUIContent(text)).x > area.width)
            {
                var clippedText = ellipsis;
                var characters = text.ToCharArray();
                for (int i = 0; i < characters.Length; i++)
                {
                    clippedText = clippedText.Insert(i, characters[i].ToString());
                    if (style.CalcSize(new GUIContent(clippedText)).x >= area.width)
                    {
                        break;
                    }
                }

                return clippedText;
            }

            return text;
        }

        // ReSharper disable once TooManyArguments
        private static IEnumerable<Rect> DrawHorizontal(IEnumerable<float> widths, float availableWidth, float height,
            float spacing = 0, float padding = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(padding);
            GUILayout.Space(-1);

            var leftWidth = availableWidth - 2 * padding;
            foreach (var width in widths)
            {
                if (leftWidth >= width)
                {
                    leftWidth -= width;
                    yield return EditorGUILayout.GetControlRect(false, height, GUILayout.Width(width));
                    GUILayout.Space(-5);
                    GUILayout.Space(spacing);
                    leftWidth -= spacing;
                }
                else
                    break;
            }
            // GUILayout.Space(3);
            GUILayout.Space(-spacing);
            GUILayout.Space(padding);

            EditorGUILayout.EndHorizontal();
        }
    }
}