namespace SpellChargingPlugin.Utilities
{
    public sealed class SimpleTimer
    {
        private float _elapsedSeconds = 0f;

        public bool Enabled { get; set; } = true;

        public void Update(float elapsedSeconds)
        {
            if (!Enabled)
                return;
            _elapsedSeconds += elapsedSeconds;
        }

        /// <summary>
        /// Check if a certain amount of time has passed since this method has been called and reset the timer
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="elapsed">The current value of the internal timer</param>
        /// <returns></returns>
        public bool HasElapsed(float seconds, out float elapsed, bool reset = true)
        {
            elapsed = _elapsedSeconds;
            if (!Enabled || _elapsedSeconds < seconds)
                return false;
            if (reset)
                Reset();
            return true;
        }

        public void Reset()
        {
            _elapsedSeconds = 0f;
        }
    }
}
