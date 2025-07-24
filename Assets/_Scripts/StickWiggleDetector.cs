using UnityEngine;

namespace _Scripts
{
    public class StickWiggleDetector
    {
        private WiggleState _state = WiggleState.Neutral;
        private const float Threshold = 0.55f;
        public int WiggleCount { get; private set; }

        public void Update(float horizontalInput)
        {
            if (Mathf.Abs(horizontalInput) > Threshold)
            {
                if (_state == WiggleState.Neutral)
                {
                    WiggleCount++;
                    _state = horizontalInput > 0 ? WiggleState.MovedRight : WiggleState.MovedLeft;
                }
                else if (_state == WiggleState.MovedLeft && horizontalInput > Threshold)
                {
                    // From left to right
                    WiggleCount++;
                    _state = WiggleState.MovedRight;
                }
                else if (_state == WiggleState.MovedRight && horizontalInput < -Threshold)
                {
                    // From right to left
                    WiggleCount++;
                    _state = WiggleState.MovedLeft;
                }
            }
            else
            {
                // Stick near center
                _state = WiggleState.Neutral;
            }
        }
        
        private enum WiggleState
        {
            Neutral,
            MovedLeft,
            MovedRight
        }
    }
}