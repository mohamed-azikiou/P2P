using System;
using System.Collections.Generic;
using System.Text;

namespace P2PShare.Classes
{
    public class TransferState
    {
        public enum State { Sending, Receiving, Completed, Error }
        public string CurrentFileName;
        public double CurrentFileProgress;
        public State CurrentState;
        public int AverageSpeed;
        public int InstantSpeed;
        public double Time;
        public int CurrentFileIndex;
        public int FileCount;

        public TransferState()
        {
        }
        public string GetFormattedAverageSpeed()
        {
            return AverageSpeed + " Ko/s";
        }
        public string GetFormattedInstantSpeed()
        {
            return InstantSpeed + " Ko/s";
        }
    }
}
