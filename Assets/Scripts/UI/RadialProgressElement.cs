using UnityEngine;
using UnityEngine.UIElements;

namespace AnimalsDream.UI
{
    public class RadialProgressElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<RadialProgressElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_Progress = new UxmlFloatAttributeDescription { name = "progress", defaultValue = 0 };
            UxmlColorAttributeDescription m_FillColor = new UxmlColorAttributeDescription { name = "fill-color", defaultValue = new Color(0, 0, 0, 0.7f) };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as RadialProgressElement;
                ate.Progress = m_Progress.GetValueFromBag(bag, cc);
                ate.FillColor = m_FillColor.GetValueFromBag(bag, cc);
            }
        }

        private float m_Progress; // 0 to 1. 1 = Full (Start of Cooldown), 0 = Empty (Ready)
        private Color m_FillColor = new Color(0, 0, 0, 0.7f);

        public float Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = Mathf.Clamp01(value);
                MarkDirtyRepaint();
            }
        }

        public Color FillColor
        {
            get => m_FillColor;
            set
            {
                m_FillColor = value;
                MarkDirtyRepaint();
            }
        }

        public RadialProgressElement()
        {
            generateVisualContent += GenerateVisualContent;
        }

        void GenerateVisualContent(MeshGenerationContext mgc)
        {
            if (m_Progress <= 0) return;

            var paint2D = mgc.painter2D;
            paint2D.fillColor = m_FillColor;
            
            float width = contentRect.width;
            float height = contentRect.height;
            Vector2 center = new Vector2(width / 2, height / 2);
            float radius = Mathf.Min(width, height) / 2;

            float elapsedRatio = 1f - m_Progress;
            float startAngle = -90f + (360f * elapsedRatio);
            float endAngle = 270f; // -90 + 360

            // If almost full, just draw full circle to avoid artifacts?
            if (m_Progress >= 0.99f)
            {
                paint2D.BeginPath();
                paint2D.Arc(center, radius, 0, 360);
                paint2D.ClosePath();
                paint2D.Fill();
            }
            else
            {
                paint2D.BeginPath();
                paint2D.MoveTo(center);
                paint2D.Arc(center, radius, startAngle, endAngle);
                paint2D.LineTo(center);
                paint2D.ClosePath();
                paint2D.Fill();
            }
        }
    }
}
