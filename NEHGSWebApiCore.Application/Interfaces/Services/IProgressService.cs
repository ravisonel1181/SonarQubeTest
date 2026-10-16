using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEHGSWebApiCore.Application.Interfaces.Services
{
    public interface IProgressService
    {
        void Start(string taskId, int total);
        void Step(string taskId);
        void SetMessage(string taskId, string message);
        int GetPercentComplete(string taskId);
        string GetMessage(string taskId);
    }
}
