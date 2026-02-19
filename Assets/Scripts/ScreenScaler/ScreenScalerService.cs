using ScreenScaler.Interfaces;
using UnityEngine;

namespace ScreenScaler
{

    public class ScreenScalerService : IScreenScaler
    {
        private readonly Vector2 _screenDefault = new(1080, 1920);

        public Vector2 GetScreenDefault()
        {
            return _screenDefault;
        }

        public Vector2 GetVector(Vector2 vector)
        {
            return new Vector2(GetWidth(vector.x), GetHeight(vector.y));
        }


        public float GetHeight(float h)
        {
            return h * GetHeightRatio();
        }

        public float GetWidth(float w)
        {
            return w * GetWidthRatio();
        }

        public float GetHeightRatio()
        {

            return (Camera.main != null) ? Camera.main.pixelHeight / _screenDefault.y : 0;
        }


        public float GetWidthRatio()
        {
            return (Camera.main != null) ? Camera.main.pixelWidth / _screenDefault.x : 0;
        }

    }
}