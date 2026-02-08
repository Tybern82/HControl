using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HControl.Game.Endurance.EnduranceGameParameters;

namespace HControl.Game.Endurance {
    public class EnduranceCatalog : RoundCatalog {

        public static readonly GameRound[] GentleRounds = [
            GameRound.newRound("Good work, keep going like that!", 45, 60),
            GameRound.newRound("Rest a bit, and keep it up!", 45, 60),
            GameRound.newRound("Just a bit more, you can do it!", 45, 60)
            ];

        public static readonly GameRound[] NormalRounds = [
            GameRound.newRound("Nice work, I'm sure you can keep going for a while longer!", 45, 60),
            GameRound.newRound("Not done yet, I wanna see you edge more ;3", 45, 60)
            ];

        public static readonly GameRound[] EvilRounds = [
            GameRound.newRound("Nice, now get ready for more...", 30, 40),
            GameRound.newRound("Good, let's go for more, no stopping!", 30, 40),
            GameRound.newRound("I want more from you, so keep at it!", 30, 40),
            GameRound.newRound("How long will I make you edge for, I wonder...", 30, 40),
            GameRound.newRound("Hmm, I'm not sure you've been edging quite enough yet!", 30, 40)
            ];

        public static readonly GameRound[] LongRounds = [
            GameRound.newRound("Hehehe, you're in for a ride, I'm feeling pretty horny...", 20, 35),
            GameRound.newRound("If you wanna cum, you're gonna have to earn it.", 20, 35),
            GameRound.newRound("We're far from done, so get ready...", 20, 35)
            ];


        public static GameRound getEnduranceRound(GameMode mode, ushort currentRound, ushort totalRounds) {
            List<GameRound> options = [];
            if (totalRounds - currentRound >= 5) {
                // include long round messages
                options.AddRange(LongRounds);
            }
            switch (mode) {
                case GameMode.Evil:
                    options.AddRange(EvilRounds);
                    goto case GameMode.Normal;

                case GameMode.Normal:
                    options.AddRange(NormalRounds);
                    goto case GameMode.Gentle;

                case GameMode.Gentle:
                    options.AddRange(GentleRounds);
                    break;
            }
            return options[Random.Shared.Next(options.Count)];
        }
    }
}
