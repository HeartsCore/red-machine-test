using System.Collections.Generic;

namespace Events
{
    public abstract class EventHandleCore
    {
        protected List<object> _watchers = new(100);

        protected bool LogsEnabled => false;

        protected bool AllFireLogs => LogsEnabled;

        public List<object> Watchers => _watchers;

        public virtual void CleanUp()
        {
        }

        public virtual bool FixWatchers()
        {
            return false;
        }

        protected void EnsureWatchers()
        {
            if (_watchers == null)
            {
                _watchers = new List<object>(100);
            }
        }
    }
}