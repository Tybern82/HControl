var GameModeEndurance = {
    mood: "average",
    maxRounds: 0,
    selectedRounds: 0,
    currentRound: 0,
    timeTracker: null,

    startGame: function () {
        // Set the speed modifiers from settings.
        baseMultiplier = parseFloat(
            document.getElementById("rangeStepSpeedGo").value
        );
        pauseMultiplier = parseFloat(
            document.getElementById("rangeStepSpeedStop").value
        );

        var controlStroke = document.getElementById(
            "optionStrokeControl"
        ).value;
        if (controlStroke) {
            FlashManager.setEnabled(true);
            FlashManager.setSound(
                document.getElementById("cbStrokeControlTick").checked
            );

            var flashMode = parseInt(controlStroke);

            if (!isNaN(flashMode)) {
                FlashManager.setMode(flashMode);
            }
        } else {
            FlashManager.setEnabled(false);
        }

        GameModeEndurance.mood = document.getElementById("selectMood").value;

        // Default first target duration is 30 min.
        targetDuration = 30 * 60 * getRandInInterval(1.0 - 0.2, 1.0 + 0.2);

        // Each round is about 10 minutes.
        // Default max number of rounds is 8. (About 1h20, on top of initial 30 minutes)
        GameModeEndurance.maxRounds = 8;

        if (GameModeEndurance.mood === "gentle") {
            // Max about 30 min, on top of initial 30 minutes.
            GameModeEndurance.maxRounds = 3;
        } else if (GameModeEndurance.mood === "evil") {
            // Initial target duration is 1 hour.
            targetDuration = 60 * 60 * (Math.random() + 0.2);

            // Max about 2h30 hours on top of initial 1 hour.
            GameModeEndurance.maxRounds = 15;
        }

        // Randomly pick number of rounds.
        GameModeEndurance.selectedRounds = Math.floor(
            Math.random() * (GameModeEndurance.maxRounds + 1)
        );

        debugConsole.log(
            "Selected " + GameModeEndurance.selectedRounds + " rounds."
        );

        startTime = new Date().getTime();
        GameModeEndurance.runStepFirstFap();
        if (State.optionPermaSlideshow && State.passType !== "") {
            GameModeEndurance.slideshowSteps();
        }

        // Send messages about elapsed time periodically.
        GameModeEndurance.timeTracker = new TimeTracker();
    },

    endGame: function () {
        setCharHead("pause");
        FlashManager.updateFlash(0);
        ImageManager.displayNoImage();
        ProgressBarManager.cancelSlideProgress();

        let divMessage = document.getElementById("message");

        divMessage.innerText =
            "Well done, let's do this again when you feel ready :3";
        if (GameModeEndurance.mood == "evil") {
            divMessage.innerText =
                "Good job, I'll be waiting for our next session when you're ready!";

            if (GameModeEndurance.selectedRounds <= 1) {
                setCharHead("deny");
                divMessage.innerText =
                    "I went pretty easy on you this time, be ready for next time, I may not feel as generous then.";
            } else if (
                GameModeEndurance.maxRounds -
                    GameModeEndurance.selectedRounds <=
                2
            ) {
                divMessage.innerText =
                    "You've done very well, I hope that climax was rewarding. Let's do this again later~";
            }
        }

        State.updatePassType(State.STATE_DONE);
    },

    runStepFirstFap: function () {
        var multiplier = baseMultiplier;
        var passType = State.STATE_FIRST;
        var randomMessage = Object.assign({}, messages[passType][0]);

        if (Math.floor(Math.random() * 2) === 0) {
            setCharHead("fap");
        } else {
            setCharHead("fap2");
        }

        if (GameModeEndurance.mood == "gentle") {
            randomMessage.msg += " Don't worry, I'll go easy on you <3";
        } else if (GameModeEndurance.mood == "evil") {
            randomMessage.msg =
                "Get ready, fap until the EDGE bar reaches the end. I want to see you hard and needy!";
        }

        updateMainDisplay(passType, randomMessage);

        var randomTime =
            (randomMessage.maxTime - randomMessage.minTime) * Math.random() +
            randomMessage.minTime;
        randomTime /= multiplier;
        debugConsole.log("Time selected: " + randomTime);

        let fNextStep = GameModeEndurance.runStepPause;

        // Next step is another fap step if stop steps are disabled.
        if (!document.getElementById("cbStopSteps").checked) {
            fNextStep = GameModeEndurance.runStepFap;
        }

        ProgressBarManager.setTimeout(fNextStep, randomTime * 1000);
    },

    onClickNext: function () {
        if (State.optionPermaSlideshow && State.passType !== "") {
            GameModeEndurance.slideshowSteps();
        }
    },

    slideshowSteps: function () {
        ProgressBarManager.cancelSlideProgress();

        let nSecondsToDisplay = document.getElementById(
            "nbPictureChangeSpeed"
        ).value;

        if (State.passType === State.STATE_FINISH) {
            nSecondsToDisplay = document.getElementById(
                "nbPictureChangeSpeedCum"
            ).value;

            // Don't reactivate the automatic picture switch if a cum picture was set.
            if (ImageManager.imageContainerCum.src) {
                return;
            }
        }

        ProgressBarManager.setSlideTimeout(
            ImageManager.displayNextImage,
            nSecondsToDisplay * 1000
        );
    },

    runStepFap: function () {
        if (Math.floor(Math.random() * 2) === 0) {
            setCharHead("fap");
        } else {
            setCharHead("fap2");
        }

        var duration = (new Date().getTime() - startTime) / 1000;
        debugConsole.log("Duration: " + duration);
        var multiplier = baseMultiplier;
        var passType = State.STATE_GO;

        if (duration > (targetDuration / 4) * 3) {
            multiplier = multiplier * 1.5;
            debugConsole.log("1.5x speed.");
        } else if (duration > targetDuration / 2) {
            multiplier = multiplier * 1.25;
            debugConsole.log("1.25x speed.");
        }

        var randomMessage =
            messages[passType][
                Math.floor(Math.random() * messages[passType].length)
            ];

        // Chance to bait by showing the cumbar during second half if not in gentle mood,
        // or for real if next is finish.

        // Always show if end of game.
        var showCumbar =
            duration > targetDuration &&
            GameModeEndurance.currentRound >= GameModeEndurance.selectedRounds;

        if (!showCumbar) {
            // Never bait if gentle.
            showCumbar = GameModeEndurance.mood !== "gentle";

            if (showCumbar) {
                // Chance to bait if second half of game.
                showCumbar =
                    duration > targetDuration / 2 && Math.random() < 0.1;
            }
        }

        updateMainDisplay(passType, randomMessage);

        if (showCumbar) {
            Visuals.showCumbar(true);
        }

        var randomTime =
            (randomMessage.maxTime - randomMessage.minTime) * Math.random() +
            randomMessage.minTime;
        randomTime /= multiplier;
        debugConsole.log("Time selected: " + randomTime);

        var nextStepCallback = GameModeEndurance.runStepPause;

        // Next step is another fap step if stop steps are disabled.
        if (!document.getElementById("cbStopSteps").checked) {
            nextStepCallback = GameModeEndurance.runStepFap;
        }

        if (duration > targetDuration) {
            // Go to next round.
            nextStepCallback = GameModeEndurance.runStepNextRound;

            if (
                GameModeEndurance.currentRound >=
                GameModeEndurance.selectedRounds
            ) {
                // Next will be the finish.
                nextStepCallback = GameModeEndurance.runStepFinish;
            }
        }

        ProgressBarManager.setTimeout(nextStepCallback, randomTime * 1000);
    },

    runStepPause: function () {
        setCharHead("pause");

        var duration = (new Date().getTime() - startTime) / 1000;
        debugConsole.log("Duration: " + duration);
        var multiplier = pauseMultiplier;
        var passType = State.STATE_STOP;

        if (duration > (targetDuration / 4) * 3) {
            multiplier = multiplier * 1.5;
            debugConsole.log("1.5x speed.");
        } else if (duration > targetDuration / 2) {
            multiplier = multiplier * 1.25;
            debugConsole.log("1.25x speed.");
        }

        var randomMessage =
            messages[passType][
                Math.floor(Math.random() * messages[passType].length)
            ];

        updateMainDisplay(passType, randomMessage);

        var randomTime =
            (randomMessage.maxTime - randomMessage.minTime) * Math.random() +
            randomMessage.minTime;
        randomTime /= multiplier;
        debugConsole.log("Time selected: " + randomTime);

        ProgressBarManager.setTimeout(
            GameModeEndurance.runStepFap,
            randomTime * 1000
        );
    },

    runStepNextRound: function () {
        setCharHead("deny");

        ++GameModeEndurance.currentRound;

        // Reset target duration to 10 minutes for next round.
        targetDuration = 10 * 60 * getRandInInterval(1.0 - 0.5, 1.0 + 0.5);
        startTime = new Date().getTime();

        debugConsole.log(
            "Round " +
                GameModeEndurance.currentRound +
                "/" +
                GameModeEndurance.selectedRounds
        );
        debugConsole.log("Next round target duration: " + targetDuration);
        var multiplier = pauseMultiplier;
        var passType = State.STATE_STOP;

        var possibleMessages = messagesEndurance[GameModeEndurance.mood];

        if (
            GameModeEndurance.mood == "evil" &&
            GameModeEndurance.selectedRounds - GameModeEndurance.currentRound >=
                6
        ) {
            // Extra messages possible at beginning of long evil sessions.
            possibleMessages = possibleMessages.concat(
                messagesEnduranceEvilLong
            );
        }

        var randomMessage =
            possibleMessages[
                Math.floor(Math.random() * possibleMessages.length)
            ];

        if (
            GameModeEndurance.mood == "gentle" &&
            GameModeEndurance.selectedRounds == GameModeEndurance.currentRound
        ) {
            // Last round of gentle mode, special message.
            randomMessage = getMessageCopy(randomMessage);
            randomMessage.msg =
                "Don't worry, I won't keep you going for too much longer <3";
        }

        updateMainDisplay(passType, randomMessage);

        var randomTime =
            (randomMessage.maxTime - randomMessage.minTime) * Math.random() +
            randomMessage.minTime;
        randomTime /= multiplier;
        debugConsole.log("Time selected: " + randomTime);

        ProgressBarManager.setTimeout(
            GameModeEndurance.runStepFap,
            randomTime * 1000
        );
    },

    runStepFinish: function () {
        GameModeEndurance.timeTracker.disable();

        document.getElementById("labelCumbar").textContent = "CUM!";
        setCharHead("cum");

        // Cum allowed.
        var passType = State.STATE_FINISH;
        var randomMessage =
            messages[passType][
                Math.floor(Math.random() * messages[passType].length)
            ];

        randomMessage.msg = "You can cum now!";

        if (GameModeEndurance.mood == "gentle") {
            randomMessage.msg = "Cum for me now <3";
        } else if (GameModeEndurance.mood == "evil") {
            if (GameModeEndurance.selectedRounds <= 2) {
                setCharHead("fap");
                randomMessage.msg =
                    "I'm feeling very generous right now. Cum, and be thankful.";
            } else if (
                GameModeEndurance.maxRounds -
                    GameModeEndurance.selectedRounds <=
                4
            ) {
                randomMessage.msg =
                    "Cum now, you've earned it. Make a big mess!";
            }
        }

        updateMainDisplay(passType, randomMessage);

        if (document.getElementById("cbFinishStepMaxVibe").checked) {
            // Override the intensity set by the message to max it out.
            ButtplugConnection.activateAllConnected(1.0);
        }

        var randomTime =
            (randomMessage.maxTime - randomMessage.minTime) * Math.random() +
            randomMessage.minTime +
            20;
        debugConsole.log("Time selected: " + randomTime);

        ProgressBarManager.setTimeoutCumbar(
            GameModeEndurance.endGame,
            randomTime * 1000
        );

        if (State.optionPermaSlideshow && State.passType !== "") {
            GameModeEndurance.slideshowSteps();
        }
    },
};
