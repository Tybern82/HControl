using System;

namespace HControl.Game {
    public class GameRound {

        public bool IsActiveRound { get; set; } = false;

        public string RoundContent { get; set; } = "";

        public TimeSpan RoundDuration { get; set; }

        public double BeatRate { get; set; } = 0;
        public ushort HandyRate { get; set; } = 0;

        private GameRound(bool isActiveRound, string content, ushort duration, double beatRate = 0, ushort handyRate = 0) : this(isActiveRound, content, TimeSpan.FromSeconds(duration), beatRate, handyRate) { }

        private GameRound(bool isActiveRound, string content, TimeSpan duration, double beatRate = 0, ushort handyRate = 0) { 
            IsActiveRound = isActiveRound;
            RoundContent = content;
            RoundDuration = duration;
            BeatRate = beatRate;
            HandyRate = handyRate;
        }

        public GameRound setBeatRate(double minBeat, double maxBeat) {
            if (minBeat == maxBeat) return new GameRound(IsActiveRound, RoundContent, RoundDuration, minBeat, HandyRate);
            if (maxBeat < minBeat) return setBeatRate(maxBeat, minBeat);
            double range = maxBeat - minBeat;
            return new GameRound(IsActiveRound, RoundContent, RoundDuration, minBeat + (Random.Shared.NextDouble() * range), HandyRate);
        }

        public GameRound setHandyRate(ushort minRate, ushort maxRate) {
            if (minRate == maxRate) return new GameRound(IsActiveRound, RoundContent, RoundDuration, BeatRate, minRate);
            if (minRate > maxRate) return setHandyRate(maxRate, minRate);
            ushort range = (ushort)(maxRate - minRate);
            return new GameRound(IsActiveRound, RoundContent, RoundDuration, BeatRate, (ushort)(minRate + (Random.Shared.NextDouble() * range)));
        }

        public static GameRound newRound(bool isActive, string content, ushort minTime, ushort maxTime) {
            if (minTime == maxTime) return new GameRound(isActive, content, minTime);
            if (minTime > maxTime) return newRound(isActive, content, maxTime, minTime);
            ushort range = (ushort)(maxTime - minTime);
            return new GameRound(isActive, content, (ushort)(minTime + (Random.Shared.NextDouble() * range)));
        }

        public static GameRound newRound(string content, ushort minTime, ushort maxTime) => newRound(false, content, minTime, maxTime);
    }
}
