using System;
using System.Collections.Generic;
using System.Threading;

namespace OSCP5
{
    class Process
    {
        int Priority;
        int ID;
        int Runtime;
        int CompleteLevel;
        string Name;
        ProcessStates State;
        public Process(string vName, List<Process> Processes, int prPriority)
        {
            Random random = new Random();

            Priority = prPriority;

            Thread.Sleep(30); // Когда Runtime рандомится слишком быстро, он берет старые значения. Для обхода проблемы замедляем поток.
            Runtime = random.Next(100, 300);
            CompleteLevel = 0;
            Name = vName;
            State = ProcessStates.birth;

            int nID;
            while (true)
            {
                nID = random.Next(5, 10000);
                if (IDComparison(nID, Processes) == true)
                {
                    ID = nID;
                    break;
                }
            }
        }
        bool IDComparison(int nID, List<Process> Processes)
        {
            for (int i = 0; i < Processes.Count; i++)
                if (Processes[i].GetID() == nID)
                    return false;
            return true;
        }
        public void SetCompleteLevel(int vCompleteLevel)
        {
            CompleteLevel = vCompleteLevel;
        }
        public int GetCompleteLevel()
        {
            return CompleteLevel;
        }
        public void SetState(ProcessStates vState)
        {
            State = vState;
        }
        public ProcessStates GetState()
        {
            return State;
        }
        public int GetID()
        {
            return ID;
        }
        public int GetRuntime()
        {
            return Runtime;
        }
        public string GetName()
        {
            return Name;
        }
        public int GetPriority()
        {
            return Priority;
        }
    }
}
