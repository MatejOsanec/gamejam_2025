using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace HMUI {

    public class GradientImage : UnityEngine.UI.Image {

        [SerializeField] Color _color0;
        [SerializeField] Color _color1;

        public Color color0 {
            get => _color0;
            set {
                if (SetPropertyUtility.SetColor(ref _color0, value)) SetAllDirty();
            }
        }

        public Color color1 {
            get => _color1;
            set {
                if (SetPropertyUtility.SetColor(ref _color1, value)) SetAllDirty();
            }
        }

         private static readonly Vector2 kVec2Zero = new Vector2(0.0f, 0.0f);
         private static readonly Vector3 kVec3Zero = new Vector3(0.0f, 0.0f, 0.0f);
         private static readonly Vector4 kVec4Zero = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        private readonly CurvedCanvasSettingsHelper _curvedCanvasSettingsHelper = new CurvedCanvasSettingsHelper();

        protected override void OnPopulateMesh(VertexHelper toFill) {

            if (overrideSprite == null) {
                base.OnPopulateMesh(toFill);
                return;
            }

            var curvedCanvasSettings = _curvedCanvasSettingsHelper.GetCurvedCanvasSettings(canvas);
            float curvedUIRadius = curvedCanvasSettings == null ? 0.0f : curvedCanvasSettings.radius;

            switch (type) {
                default:
                case Type.Simple:
                    GenerateSimpleSprite(toFill, preserveAspect, curvedUIRadius);
                    break;
                case Type.Sliced:
                    GenerateSlicedSprite(toFill, curvedUIRadius);
                    break;
                case Type.Tiled:
                    GenerateTiledSprite(toFill);
                    break;
                case Type.Filled:
                    GenerateFilledSprite(toFill, preserveAspect);
                    break;
            }
        }

        void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect, float curvedUIRadius) {

            vh.Clear();

            Vector4 v = GetDrawingDimensions(lPreserveAspect);
            var uv = (overrideSprite != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;

            var c0 = color * _color0;
            var c1 = color * _color1;

            var curvedUIRadiusVec = new Vector2(curvedUIRadius, 0.0f);

            var elements = Mathf.CeilToInt(Mathf.Abs(v.z - v.x) / CurvedCanvasSettings.kMaxElementWidth);
            if (elements < 1) {
                elements = 1;
            }
            for (int i = 0; i < elements + 1; i++) {
                var t = (float) i / elements;
                var posX = Mathf.Lerp(v.x, v.z, t);
                var uvX = Mathf.Lerp(uv.x, uv.z, t);
                var c = Color32.Lerp(c0, c1, t);
                vh.AddVert(new Vector3(posX, v.w), c, new Vector2(uvX, uv.w), new Vector2(1.0f, 0.0f), curvedUIRadiusVec, kVec2Zero, kVec3Zero, kVec4Zero);
                vh.AddVert(new Vector3(posX, v.y), c, new Vector2(uvX, uv.y), new Vector2(0.0f, 0.0f), curvedUIRadiusVec, kVec2Zero, kVec3Zero, kVec4Zero);
            }

            for (int i = 0; i < elements; i++) {
                var offset = i * 2;
                vh.AddTriangle(0 + offset, 1 + offset, 2 + offset);
                vh.AddTriangle(2 + offset, 3 + offset, 1 + offset);
            }

        }

         static readonly Vector2[] s_VertScratch = new Vector2[4];

         static readonly Vector2[] s_UVScratch = new Vector2[4];

         static readonly Color[] s_ColorScratch = new Color[4];

        private void GenerateSlicedSprite(VertexHelper vh, float curvedUIRadius) {

            if (!hasBorder) {
                GenerateSimpleSprite(vh, lPreserveAspect: false, curvedUIRadius);
                return;
            }

            Vector4 outer, inner, padding, border;

            if (overrideSprite != null) {
                outer = UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite);
                inner = UnityEngine.Sprites.DataUtility.GetInnerUV(overrideSprite);
                padding = UnityEngine.Sprites.DataUtility.GetPadding(overrideSprite);
                border = overrideSprite.border;
            }
            else {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();
            border = GetAdjustedBorders(border / pixelsPerUnit, rect);
            padding = padding / pixelsPerUnit;

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = border.x;
            s_VertScratch[1].y = border.y;
            s_VertScratch[2].x = rect.width - border.z;
            s_VertScratch[2].y = rect.height - border.w;

            for (int i = 0; i < 4; ++i) {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            s_UVScratch[0] = new Vector2(outer.x, outer.y);
            s_UVScratch[1] = new Vector2(inner.x, inner.y);
            s_UVScratch[2] = new Vector2(inner.z, inner.w);
            s_UVScratch[3] = new Vector2(outer.z, outer.w);

            var c0 = _color0 * color;
            var c1 = _color1 * color;
            var startX = s_VertScratch[0].x;
            var width = s_VertScratch[3].x - s_VertScratch[0].x;

            s_ColorScratch[0] = c0;
            s_ColorScratch[1] = Color.Lerp(c0, c1, (s_VertScratch[1].x - startX) / width);
            s_ColorScratch[2] = Color.Lerp(c0, c1, (s_VertScratch[2].x - startX) / width);
            s_ColorScratch[3] = c1;

            vh.Clear();

            float elementWidthScale = transform.localScale.x;

            for (int x = 0; x < 3; ++x) {
                int x2 = x + 1;

                for (int y = 0; y < 3; ++y) {
                    if (!fillCenter && x == 1 && y == 1)
                        continue;

                    int y2 = y + 1;

                    AddQuad(vh,
                        new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                        new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                        s_ColorScratch[x],
                        s_ColorScratch[x2],
                        new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                        new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y),
                        elementWidthScale,
                        curvedUIRadius
                    );
                }
            }
        }

        void GenerateTiledSprite(VertexHelper toFill) {
            
            Vector4 outer, inner, border;
            Vector2 spriteSize;

            if (overrideSprite != null) {
                outer = UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite);
                inner = UnityEngine.Sprites.DataUtility.GetInnerUV(overrideSprite);
                border = overrideSprite.border;
                spriteSize = overrideSprite.rect.size;
            }
            else {
                outer = Vector4.zero;
                inner = Vector4.zero;
                border = Vector4.zero;
                spriteSize = Vector2.one * 100;
            }

            Rect rect = GetPixelAdjustedRect();
            float tileWidth = (spriteSize.x - border.x - border.z) / pixelsPerUnit;
            float tileHeight = (spriteSize.y - border.y - border.w) / pixelsPerUnit;
            border = GetAdjustedBorders(border / pixelsPerUnit, rect);

            var uvMin = new Vector2(inner.x, inner.y);
            var uvMax = new Vector2(inner.z, inner.w);

            var v = UIVertex.simpleVert;
            v.color = color;

            // Min to max max range for tiled region in coordinates relative to lower left corner.
            float xMin = border.x;
            float xMax = rect.width - border.z;
            float yMin = border.y;
            float yMax = rect.height - border.w;

            toFill.Clear();
            var clipped = uvMax;

            // if either with is zero we cant tile so just assume it was the full width.
            if (tileWidth == 0)
                tileWidth = xMax - xMin;

            if (tileHeight == 0)
                tileHeight = yMax - yMin;

            if (fillCenter) {
                for (float y1 = yMin; y1 < yMax; y1 += tileHeight) {
                    float y2 = y1 + tileHeight;
                    if (y2 > yMax) {
                        clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                        y2 = yMax;
                    }

                    clipped.x = uvMax.x;
                    for (float x1 = xMin; x1 < xMax; x1 += tileWidth) {
                        float x2 = x1 + tileWidth;
                        if (x2 > xMax) {
                            clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                            x2 = xMax;
                        }
                        AddQuad(toFill, new Vector2(x1, y1) + rect.position, new Vector2(x2, y2) + rect.position, color, uvMin, clipped);
                    }
                }
            }

            if (hasBorder) {
                clipped = uvMax;
                for (float y1 = yMin; y1 < yMax; y1 += tileHeight) {
                    float y2 = y1 + tileHeight;
                    if (y2 > yMax) {
                        clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                        y2 = yMax;
                    }
                    AddQuad(toFill,
                        new Vector2(0, y1) + rect.position,
                        new Vector2(xMin, y2) + rect.position,
                        color,
                        new Vector2(outer.x, uvMin.y),
                        new Vector2(uvMin.x, clipped.y));
                    AddQuad(toFill,
                        new Vector2(xMax, y1) + rect.position,
                        new Vector2(rect.width, y2) + rect.position,
                        color,
                        new Vector2(uvMax.x, uvMin.y),
                        new Vector2(outer.z, clipped.y));
                }

                // Bottom and top tiled border
                clipped = uvMax;
                for (float x1 = xMin; x1 < xMax; x1 += tileWidth) {
                    float x2 = x1 + tileWidth;
                    if (x2 > xMax) {
                        clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                        x2 = xMax;
                    }
                    AddQuad(toFill,
                        new Vector2(x1, 0) + rect.position,
                        new Vector2(x2, yMin) + rect.position,
                        color,
                        new Vector2(uvMin.x, outer.y),
                        new Vector2(clipped.x, uvMin.y));
                    AddQuad(toFill,
                        new Vector2(x1, yMax) + rect.position,
                        new Vector2(x2, rect.height) + rect.position,
                        color,
                        new Vector2(uvMin.x, uvMax.y),
                        new Vector2(clipped.x, outer.w));
                }

                // Corners
                AddQuad(toFill,
                    new Vector2(0, 0) + rect.position,
                    new Vector2(xMin, yMin) + rect.position,
                    color,
                    new Vector2(outer.x, outer.y),
                    new Vector2(uvMin.x, uvMin.y));
                AddQuad(toFill,
                    new Vector2(xMax, 0) + rect.position,
                    new Vector2(rect.width, yMin) + rect.position,
                    color,
                    new Vector2(uvMax.x, outer.y),
                    new Vector2(outer.z, uvMin.y));
                AddQuad(toFill,
                    new Vector2(0, yMax) + rect.position,
                    new Vector2(xMin, rect.height) + rect.position,
                    color,
                    new Vector2(outer.x, uvMax.y),
                    new Vector2(uvMin.x, outer.w));
                AddQuad(toFill,
                    new Vector2(xMax, yMax) + rect.position,
                    new Vector2(rect.width, rect.height) + rect.position,
                    color,
                    new Vector2(uvMax.x, uvMax.y),
                    new Vector2(outer.z, outer.w));
            }
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs) {
            int startIndex = vertexHelper.currentVertCount;

            for (int i = 0; i < 4; ++i)
                vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax) {
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color0, Color32 color1, Vector2 uv0Min, Vector2 uv0Max, float elementWidthScale, float curvedUIRadius) {

            var elements = Mathf.CeilToInt(Mathf.Abs(posMin.x - posMax.x) * elementWidthScale / CurvedCanvasSettings.kMaxElementWidth);
            if (elements < 1) {
                elements = 1;
            }

            int startIndex = vertexHelper.currentVertCount;
            var curvedUIRadiusVec = new Vector2(curvedUIRadius, 0.0f);

            for (int i = 0; i < elements + 1; i++) {
                var t = (float) i / elements;
                var posX = Mathf.Lerp(posMin.x, posMax.x, t);
                var uv0X = Mathf.Lerp(uv0Min.x, uv0Max.x, t);
                var c = Color.Lerp(color0, color1, t);
                vertexHelper.AddVert(new Vector3(posX, posMin.y), c, new Vector2(uv0X, uv0Min.y), kVec2Zero, curvedUIRadiusVec, kVec2Zero, kVec3Zero, kVec4Zero);
                vertexHelper.AddVert(new Vector3(posX, posMax.y), c, new Vector2(uv0X, uv0Max.y), kVec2Zero, curvedUIRadiusVec, kVec2Zero, kVec3Zero, kVec4Zero);
            }

            for (int i = 0; i < elements; i++) {
                var offset = i * 2 + startIndex;
                vertexHelper.AddTriangle(0 + offset, 1 + offset, 2 + offset);
                vertexHelper.AddTriangle(2 + offset, 3 + offset, 1 + offset);
            }
        }

        Vector4 GetAdjustedBorders(Vector4 border, Rect rect) {
            for (int axis = 0; axis <= 1; axis++) {
                // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
                // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
                float combinedBorders = border[axis] + border[axis + 2];
                if (rect.size[axis] < combinedBorders && combinedBorders != 0) {
                    float borderScaleRatio = rect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }

         static readonly Vector3[] s_Xy = new Vector3[4];
         static readonly Vector3[] s_Uv = new Vector3[4];

        void GenerateFilledSprite(VertexHelper toFill, bool preserveAspect) {
            toFill.Clear();

            if (fillAmount < 0.001f)
                return;

            Vector4 v = GetDrawingDimensions(preserveAspect);
            Vector4 outer = overrideSprite != null ? UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
            UIVertex uiv = UIVertex.simpleVert;
            uiv.color = color;

            float tx0 = outer.x;
            float ty0 = outer.y;
            float tx1 = outer.z;
            float ty1 = outer.w;

            // Horizontal and vertical filled sprites are simple -- just end the Image prematurely
            if (fillMethod == FillMethod.Horizontal || fillMethod == FillMethod.Vertical) {
                if (fillMethod == FillMethod.Horizontal) {
                    float fill = (tx1 - tx0) * fillAmount;

                    if (fillOrigin == 1) {
                        v.x = v.z - (v.z - v.x) * fillAmount;
                        tx0 = tx1 - fill;
                    }
                    else {
                        v.z = v.x + (v.z - v.x) * fillAmount;
                        tx1 = tx0 + fill;
                    }
                }
                else if (fillMethod == FillMethod.Vertical) {
                    float fill = (ty1 - ty0) * fillAmount;

                    if (fillOrigin == 1) {
                        v.y = v.w - (v.w - v.y) * fillAmount;
                        ty0 = ty1 - fill;
                    }
                    else {
                        v.w = v.y + (v.w - v.y) * fillAmount;
                        ty1 = ty0 + fill;
                    }
                }
            }

            s_Xy[0] = new Vector2(v.x, v.y);
            s_Xy[1] = new Vector2(v.x, v.w);
            s_Xy[2] = new Vector2(v.z, v.w);
            s_Xy[3] = new Vector2(v.z, v.y);

            s_Uv[0] = new Vector2(tx0, ty0);
            s_Uv[1] = new Vector2(tx0, ty1);
            s_Uv[2] = new Vector2(tx1, ty1);
            s_Uv[3] = new Vector2(tx1, ty0);

            {
                if (fillAmount < 1f && fillMethod != FillMethod.Horizontal && fillMethod != FillMethod.Vertical) {
                    if (fillMethod == FillMethod.Radial90) {
                        if (RadialCut(s_Xy, s_Uv, fillAmount, fillClockwise, fillOrigin))
                            AddQuad(toFill, s_Xy, color, s_Uv);
                    }
                    else if (fillMethod == FillMethod.Radial180) {
                        for (int side = 0; side < 2; ++side) {
                            float fx0, fx1, fy0, fy1;
                            int even = fillOrigin > 1 ? 1 : 0;

                            if (fillOrigin == 0 || fillOrigin == 2) {
                                fy0 = 0f;
                                fy1 = 1f;
                                if (side == even) {
                                    fx0 = 0f;
                                    fx1 = 0.5f;
                                }
                                else {
                                    fx0 = 0.5f;
                                    fx1 = 1f;
                                }
                            }
                            else {
                                fx0 = 0f;
                                fx1 = 1f;
                                if (side == even) {
                                    fy0 = 0.5f;
                                    fy1 = 1f;
                                }
                                else {
                                    fy0 = 0f;
                                    fy1 = 0.5f;
                                }
                            }

                            s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = fillClockwise ? fillAmount * 2f - side : fillAmount * 2f - (1 - side);

                            if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), fillClockwise, ((side + fillOrigin + 3) % 4))) {
                                AddQuad(toFill, s_Xy, color, s_Uv);
                            }
                        }
                    }
                    else if (fillMethod == FillMethod.Radial360) {
                        for (int corner = 0; corner < 4; ++corner) {
                            float fx0, fx1, fy0, fy1;

                            if (corner < 2) {
                                fx0 = 0f;
                                fx1 = 0.5f;
                            }
                            else {
                                fx0 = 0.5f;
                                fx1 = 1f;
                            }

                            if (corner == 0 || corner == 3) {
                                fy0 = 0f;
                                fy1 = 0.5f;
                            }
                            else {
                                fy0 = 0.5f;
                                fy1 = 1f;
                            }

                            s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = fillClockwise ? fillAmount * 4f - ((corner + fillOrigin) % 4) : fillAmount * 4f - (3 - ((corner + fillOrigin) % 4));

                            if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), fillClockwise, ((corner + 2) % 4)))
                                AddQuad(toFill, s_Xy, color, s_Uv);
                        }
                    }
                }
                else {
                    AddQuad(toFill, s_Xy, color, s_Uv);
                }
            }
        }

        static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner) {
            // Nothing to fill
            if (fill < 0.001f) return false;

            // Even corners invert the fill direction
            if ((corner & 1) == 1) invert = !invert;

            // Nothing to adjust
            if (!invert && fill > 0.999f) return true;

            // Convert 0-1 value into 0 to 90 degrees angle in radians
            float angle = Mathf.Clamp01(fill);
            if (invert) angle = 1f - angle;
            angle *= 90f * Mathf.Deg2Rad;

            // Calculate the effective X and Y factors
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            RadialCut(xy, cos, sin, invert, corner);
            RadialCut(uv, cos, sin, invert, corner);
            return true;
        }

        static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner) {
            int i0 = corner;
            int i1 = ((corner + 1) % 4);
            int i2 = ((corner + 2) % 4);
            int i3 = ((corner + 3) % 4);

            if ((corner & 1) == 1) {
                if (sin > cos) {
                    cos /= sin;
                    sin = 1f;

                    if (invert) {
                        xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i2].x = xy[i1].x;
                    }
                }
                else if (cos > sin) {
                    sin /= cos;
                    cos = 1f;

                    if (!invert) {
                        xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i3].y = xy[i2].y;
                    }
                }
                else {
                    cos = 1f;
                    sin = 1f;
                }

                if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
            }
            else {
                if (cos > sin) {
                    sin /= cos;
                    cos = 1f;

                    if (!invert) {
                        xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i2].y = xy[i1].y;
                    }
                }
                else if (sin > cos) {
                    cos /= sin;
                    sin = 1f;

                    if (invert) {
                        xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i3].x = xy[i2].x;
                    }
                }
                else {
                    cos = 1f;
                    sin = 1f;
                }

                if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
            }
        }

        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect) {

            var padding = overrideSprite == null ? Vector4.zero : UnityEngine.Sprites.DataUtility.GetPadding(overrideSprite);
            var size = overrideSprite == null ? Vector2.zero : new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);

            Rect r = GetPixelAdjustedRect();

            int spriteW = Mathf.RoundToInt(size.x);
            int spriteH = Mathf.RoundToInt(size.y);

            var v = new Vector4(
                padding.x / spriteW,
                padding.y / spriteH,
                (spriteW - padding.z) / spriteW,
                (spriteH - padding.w) / spriteH);

            if (shouldPreserveAspect && size.sqrMagnitude > 0.0f) {
                var spriteRatio = size.x / size.y;
                var rectRatio = r.width / r.height;

                if (spriteRatio > rectRatio) {
                    var oldHeight = r.height;
                    r.height = r.width * (1.0f / spriteRatio);
                    r.y += (oldHeight - r.height) * rectTransform.pivot.y;
                }
                else {
                    var oldWidth = r.width;
                    r.width = r.height * spriteRatio;
                    r.x += (oldWidth - r.width) * rectTransform.pivot.x;
                }
            }

            v = new Vector4(
                r.x + r.width * v.x,
                r.y + r.height * v.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
            );

            return v;
        }
    }
}