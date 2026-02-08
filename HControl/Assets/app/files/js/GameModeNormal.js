var GameModeNormal = {
	timeTracker: null,

	startGame: function()
	{
		targetDuration = parseInt(document.getElementById("rangeEdgeDuration").value);
		targetDuration = targetDuration * 60 * (getRandInInterval(1.0 - 0.2, 1.0 + 0.2));

		// Set the speed modifiers from settings.
		baseMultiplier = parseFloat(document.getElementById("rangeStepSpeedGo").value);
		pauseMultiplier = parseFloat(document.getElementById("rangeStepSpeedStop").value);

		// Make the game and steps much quicker if test mode is enabled.
		if (debugConsole.quickTestMode) {targetDuration = 10;baseMultiplier = 10;pauseMultiplier = 10;}

		cumFactor = parseFloat(document.getElementById("cumPercent").value) / 100.0;
		var controlStroke = document.getElementById("optionStrokeControl").value;
		if (controlStroke)
		{
			FlashManager.setEnabled(true);
			FlashManager.setSound(document.getElementById("cbStrokeControlTick").checked);
			
			var flashMode = parseInt(controlStroke);
			
			if (!isNaN(flashMode))
			{
				FlashManager.setMode(flashMode);
			}
		}
		else
		{
			FlashManager.setEnabled(false);
		}
		
		debugConsole.log("Target Duration: " + (targetDuration / 60));
		
		startTime = new Date().getTime();
		GameModeNormal.runStepFirstFap();
		if (State.optionPermaSlideshow && State.passType !== "") {
			GameModeNormal.slideshowSteps();
		}

		// Send messages about elapsed time periodically.
		GameModeNormal.timeTracker = new TimeTracker();
	},

	endGame: function()
	{
		FlashManager.updateFlash(0);
		setCharHead("pause");
		ImageManager.displayNoImage();
		ProgressBarManager.cancelSlideProgress();
		document.getElementById("message").innerText = "If you haven't cum yet, game over, STOP!\r\nIf you did, good job!";
		State.updatePassType(State.STATE_DONE);
	},
	
	runStepFirstFap: function()
	{
		var multiplier = baseMultiplier;
		var passType = State.STATE_FIRST;
		var randomMessage = messages[passType][Math.floor(Math.random() * messages[passType].length)];
		
		if (Math.floor(Math.random() * 2) === 0)
		{
			setCharHead("fap");
		}
		else
		{
			setCharHead("fap2");
		}
		
		updateMainDisplay(passType, randomMessage);
		
		var randomTime = (randomMessage.maxTime - randomMessage.minTime) * Math.random() + randomMessage.minTime;
		randomTime /= multiplier;
		debugConsole.log("Time selected: " + randomTime);

		let fNextStep = GameModeNormal.runStepPause;

		// Next step is another fap step if stop steps are disabled.
		if (!document.getElementById("cbStopSteps").checked)
		{
			fNextStep = GameModeNormal.runStepFap;
		}

		ProgressBarManager.setTimeout(fNextStep, randomTime * 1000);
	},

	onClickNext: function()
	{
		if (State.optionPermaSlideshow && State.passType !== "") {
			GameModeNormal.slideshowSteps();
		}
	},

	slideshowSteps: function() {
		ProgressBarManager.cancelSlideProgress();

		let nSecondsToDisplay = document.getElementById("nbPictureChangeSpeed").value;

		if (State.passType === State.STATE_FINISH)
		{
			nSecondsToDisplay = document.getElementById("nbPictureChangeSpeedCum").value;

			// Don't reactivate the automatic picture switch if a cum picture was set.
			if (ImageManager.imageContainerCum.src)
			{
				return;
			}
		}

		ProgressBarManager.setSlideTimeout(ImageManager.displayNextImage, nSecondsToDisplay * 1000);
	},

	runStepFap: function()
	{
		var duration = (new Date().getTime() - startTime) / 1000;
		debugConsole.log("Duration: " + duration);
		var multiplier = baseMultiplier;
		var passType = State.STATE_GO;
		
		if (Math.floor(Math.random() * 2) === 0)
		{
			setCharHead("fap");
		}
		else
		{
			setCharHead("fap2");
		}
		
		if (duration > targetDuration / 4 * 3)
		{
			multiplier = multiplier * 1.5;
			debugConsole.log('1.5x speed.');
		}
		else if (duration > targetDuration / 2)
		{
			multiplier = multiplier * 1.25;
			debugConsole.log('1.25x speed.');
		}
		
		var randomMessage = messages[passType][Math.floor(Math.random() * messages[passType].length)];
		
		// Chance to bait by showing the cumbar during second half,
		// or for real if next is finish.
		
		// Never show if chose not to cum.
		var showCumbar = cumFactor > 0.0;
		
		if (showCumbar)
		{
			// Always show if end of game.
			showCumbar = duration > targetDuration;
			
			if (!showCumbar)
			{
				// Chance to bait if second half of game.
				showCumbar = duration > targetDuration / 2 && Math.random() < 0.1;
			}
		}
		
		updateMainDisplay(passType, randomMessage);
		
		if (showCumbar)
		{
			Visuals.showCumbar(true);
		}
		
		var nextStepCallback = GameModeNormal.runStepPause;

		// Next step is another fap step if stop steps are disabled.
		if (!document.getElementById("cbStopSteps").checked)
		{
			nextStepCallback = GameModeNormal.runStepFap;
		}
		
		if (duration > targetDuration)
		{
			// Timer has run out, next will be the finish.
			nextStepCallback = GameModeNormal.runStepFinish;
		}
		
		var randomTime = (randomMessage.maxTime - randomMessage.minTime) * Math.random() + randomMessage.minTime;
		randomTime /= multiplier;
		debugConsole.log("Time selected: " + randomTime);

		
		ProgressBarManager.setTimeout(nextStepCallback, randomTime * 1000);

		if (State.optionPermaSlideshow && State.passType !== "") {
			GameModeNormal.slideshowSteps();
		}
	},

	runStepPause: function()
	{
		setCharHead("pause");
		
		var duration = (new Date().getTime() - startTime) / 1000;
		debugConsole.log("Duration: " + duration);
		var multiplier = pauseMultiplier;
		var passType = State.STATE_STOP;
		
		if (duration > targetDuration / 4 * 3)
		{
			multiplier = multiplier * 1.5;
			debugConsole.log('1.5x speed.');
		}
		else if (duration > targetDuration / 2)
		{
			multiplier = multiplier * 1.25;
			debugConsole.log('1.25x speed.');
		}
		
		var randomMessage = messages[passType][Math.floor(Math.random() * messages[passType].length)];
		
		updateMainDisplay(passType, randomMessage);
		
		var randomTime = (randomMessage.maxTime - randomMessage.minTime) * Math.random() + randomMessage.minTime;
		randomTime /= multiplier;
		debugConsole.log("Time selected: " + randomTime);
		
		ProgressBarManager.setTimeout(GameModeNormal.runStepFap, randomTime * 1000);
	},

	runStepFinish: function()
	{
		GameModeNormal.timeTracker.disable();

		document.getElementById("labelCumbar").textContent = "CUM!";
		
		if (cumFactor === 0.0)
		{
			// Chose not to cum.
			setCharHead("fap2");
			FlashManager.updateFlash(0);
			ImageManager.displayNoImage();
			ProgressBarManager.cancelSlideProgress();
			State.updatePassType(State.STATE_DONE);
			document.getElementById("message").innerText = "Congratulations, you have completed your edging session!\r\nLet's play again soon <3.";
			
			return;
		}
		
		cumFactor -= Math.random();
		
		if (cumFactor < 0.0)
		{
			// Cum not allowed.
			setCharHead("deny");
			FlashManager.updateFlash(0);
			ImageManager.displayNoImage();
			ProgressBarManager.cancelSlideProgress();
			State.updatePassType(State.STATE_CANCEL);
			document.getElementById("message").innerText = messages["noCum"][0].msg;
			
			return;
		}
		
		// Cum allowed.
		setCharHead("cum");
		
		var passType = State.STATE_FINISH;
		var randomMessage = messages[passType][Math.floor(Math.random() * messages[passType].length)];
		
		updateMainDisplay(passType, randomMessage);

		if (document.getElementById("cbFinishStepMaxVibe").checked)
		{
			// Override the intensity set by the message to max it out.
			ButtplugConnection.activateAllConnected(1.0);
		}
		
		var randomTime = (randomMessage.maxTime - randomMessage.minTime) * Math.random() + randomMessage.minTime;
		debugConsole.log("Time selected: " + randomTime);
		
		
		ProgressBarManager.setTimeoutCumbar(GameModeNormal.endGame, randomTime * 1000);

		if (State.optionPermaSlideshow && State.passType !== "") {
			GameModeNormal.slideshowSteps();
		}
	}
};
