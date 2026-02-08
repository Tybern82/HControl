using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HControl.Game {
    public class RoundCatalog {

        private static readonly GameRound First = GameRound.newRound(true, "Let's start, warm up and fap for a while, then stop when the EDGE bar reaches the end.", 30, 50).setBeatRate(2.5, 2.5).setHandyRate(40, 60);

        private static readonly GameRound[] Go = [
            GameRound.newRound(true, "Fap!", 20, 45).setBeatRate(2, 2.7).setHandyRate(40, 60),
            GameRound.newRound(true, "Stroke", 20, 45).setBeatRate(2.2, 2.7).setHandyRate(50, 70),
            GameRound.newRound(true, "Stroke lightly", 15, 45).setBeatRate(1.5, 2.5).setHandyRate(15, 40),
            GameRound.newRound(true, "Go SLOW and steady...", 45, 70).setBeatRate(1, 1).setHandyRate(10, 20),
            GameRound.newRound(true, "Go HARD and FAST", 8, 15).setBeatRate(4.5, 5).setHandyRate(80, 90)
            ];

        private static readonly GameRound[] Stop = [
            GameRound.newRound("STOP! Calm down...", 15, 30),
            GameRound.newRound("No touching!", 10, 30),
            GameRound.newRound("STOP! Quick break.", 7, 15),
            GameRound.newRound("Hands off!", 10, 25),
            GameRound.newRound("STOP", 10, 25)
            ];

        private static readonly GameRound[] Finish = [
            GameRound.newRound(true, "CUM! DO IT NOW!", 15, 20).setBeatRate(4, 5).setHandyRate(90, 100),
            GameRound.newRound(true, "CUM! Take your time, no rush ;3", 50, 70).setBeatRate(2, 3).setHandyRate(80, 100),
            GameRound.newRound(true, "CUM now!", 25, 35).setBeatRate(2.6, 2.6).setHandyRate(80, 100),
            GameRound.newRound(true, "CUM!", 25, 40).setBeatRate(2.6, 2.6).setHandyRate(80, 100),
            GameRound.newRound(true, "You can cum now. GO!", 20, 30).setBeatRate(3, 3).setHandyRate(85, 100)
            ];

        private static readonly GameRound FinishImmediate = GameRound.newRound(true, "You have 10 seconds to start cumming, otherwise it's game over!", 11, 11).setBeatRate(5, 5).setHandyRate(100, 100);
        private static readonly GameRound NoCum = GameRound.newRound("STOP! Sorry, no cumming for you.\nTry again, maybe you will get lucky next time...", 30, 30);

        public static GameRound getFirstRound() => First;
        public static GameRound getActiveRound(bool isIntense, bool isFinish) => isFinish ? getFinishRound(isIntense) : Go[Random.Shared.Next(Go.Length)];
        public static GameRound getPauseRound() => Stop[Random.Shared.Next(Stop.Length)];
        public static GameRound getDenyRound() => NoCum;

        public static GameRound getFinishRound(bool isIntense) {
            if (!isIntense) return Finish[Random.Shared.Next(Finish.Length)];
            List<GameRound> options = [.. Finish];
            options.Add(FinishImmediate);       // in Evil mode, also include option for Immediate
            return options[Random.Shared.Next(options.Count)];
        }
    }
}
