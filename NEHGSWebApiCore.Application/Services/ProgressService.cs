using NEHGSWebApiCore.Application.Interfaces.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Services
{
    public class ProgressService : IProgressService
    {
        private class ProgressState { public int Total; public int StepCount; public string Message; }
        private readonly ConcurrentDictionary<string, ProgressState> _store = new ConcurrentDictionary<string, ProgressState>();

        public void Start(string taskId, int total)
        {
            _store[taskId] = new ProgressState { Total = total, StepCount = 0, Message = string.Empty };
        }

        public void Step(string taskId)
        {
            if (_store.TryGetValue(taskId, out var s)) s.StepCount++;
        }

        public void SetMessage(string taskId, string message)
        {
            if (_store.TryGetValue(taskId, out var s)) s.Message = message;
        }

        public int GetPercentComplete(string taskId)
        {
            if (!_store.TryGetValue(taskId, out var s)) return 0;
            if (s.Total <= 0) return s.StepCount >= 1 ? 100 : 0;
            var pct = (int)((s.StepCount / (double)s.Total) * 100);
            return pct > 100 ? 100 : pct;
        }

        public string GetMessage(string taskId)
        {
            if (!_store.TryGetValue(taskId, out var s)) return string.Empty;
            return s.Message;
        }
    }
}
