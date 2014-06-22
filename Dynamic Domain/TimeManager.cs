namespace StarSimulator
{
    public class TimeManager : NotifyBase
    {
        private long _currentTick = 0;
        public long CurrentTick
        {
            get { return _currentTick; }
            set
            {
                if (_currentTick == value) return;
                _currentTick = value;
                OnPropertyChanged();
            }
        }

        public void SilentOverride(long tick)
        {
            _currentTick = tick;
        }

        public void Pulse()
        {
            OnPropertyChanged("CurrentTick");
        }
    }
}