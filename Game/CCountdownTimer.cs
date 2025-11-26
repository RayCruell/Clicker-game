using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class CCountdownTimer
    {
        private double targetTime;
        private double currentTime;

        public CCountdownTimer(double targetTime)
        {
            this.targetTime = targetTime;
            this.currentTime = targetTime;
        }

        public double getTime()
        {
            return currentTime;
        }

        public void update(double delta)
        {
            if (currentTime > 0)
                currentTime -= delta;
        }

        public bool IsFinished()
        {
            return currentTime <= 0;
        }

        public void Start(double newTargetTime)
        {
            targetTime = newTargetTime;
            currentTime = targetTime;
        }

        public void SetDuration(double newDuration)
        {
            targetTime = newDuration;
        }

        public double GetDuration() => targetTime;
    }
}
