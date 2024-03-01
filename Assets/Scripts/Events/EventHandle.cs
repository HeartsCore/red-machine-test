using System;
using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class EventHandle<T> : EventHandleCore
    {
        private readonly List<Action<T>> _actions = new(100);
        private readonly List<Action<T>> _removed = new(100);

        public void Subscribe(object watcher, Action<T> action)
        {
            if (_removed.Contains(action))
            {
                _removed.Remove(action);
            }

            if (!_actions.Contains(action))
            {
                _actions.Add(action);
                EnsureWatchers();
                _watchers.Add(watcher);
            }
            
            else if (LogsEnabled)
            {
                Debug.LogFormat("{0} tries to subscribe to {1} again.", watcher, action);
            }
        }

        public void Unsubscribe(Action<T> action)
        {
            SafeUnsubscribe(action);
        }

        private void SafeUnsubscribe(Action<T> action)
        {
            var num = _actions.IndexOf(action);
            SafeUnsubscribe(num);
            
            if (num < 0 && LogsEnabled)
            {
                Debug.LogFormat("Trying to unsubscribe action {0} without watcher.", action);
            }
        }

        private void SafeUnsubscribe(int index)
        {
            if (index >= 0)
            {
                _removed.Add(_actions[index]);
            }
        }

        private void FullUnsubscribe(int index)
        {
            if (index >= 0)
            {
                _actions.RemoveAt(index);
                if (index < _watchers.Count)
                {
                    _watchers.RemoveAt(index);
                }
            }
        }

        private void FullUnsubscribe(Action<T> action)
        {
            var index = _actions.IndexOf(action);
            FullUnsubscribe(index);
        }

        public void Fire(T arg)
        {
            foreach (var action in _actions)
            {
                if (!_removed.Contains(action))
                {
                    action(arg);
                }
            }

            CleanUp();
            
            if (AllFireLogs)
            {
                Debug.LogFormat("[{0}] fired (Listeners: {1})", typeof(T).Name, _watchers.Count);
            }
        }

        public override void CleanUp()
        {
            using var enumerator = _removed.GetEnumerator();
            
            while (enumerator.MoveNext())
            {
                FullUnsubscribe(enumerator.Current);
            }

            _removed.Clear();
        }

        public override bool FixWatchers()
        {
            CleanUp();
            int num = 0;
            EnsureWatchers();
            for (var i = 0; i < _watchers.Count; i++)
            {
                var obj = _watchers[i];
                if (obj is MonoBehaviour && !(obj as MonoBehaviour))
                {
                    SafeUnsubscribe(i);
                    num++;
                }
            }

            if (num > 0)
            {
                CleanUp();
            }

            if (num > 0 && LogsEnabled)
            {
                Debug.LogFormat("{0} destroyed scripts subscribed to event {1}.", num, typeof(T));
            }

            return num == 0;
        }
    }
}