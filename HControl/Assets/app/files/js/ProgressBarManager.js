var ProgressBarManager = {
	timeoutHandle: null,
	timeoutSlideProgress: null,
	isPaused: false,
	
	setTimeout: function(callback, timeout)
	{
		ProgressBarManager.showProgressAndCallback(callback, timeout, "divJerkbarFill");
	},

	setSlideTimeout: function(callback, timeout)
	{
		ProgressBarManager.backgroundSlideTick(callback, timeout);
	},
	
	setTimeoutCumbar: function(callback, timeout)
	{
		ProgressBarManager.showProgressAndCallback(callback, timeout, "divCumbarFill");
	},

	backgroundSlideTick: function(callback, timeout)
	{
		var timeLeft = timeout;
		var lastPassType = State.passType;
		var referenceTime = performance.now();
		var continueSlideProgress = function() {
			if (State.passType != lastPassType||State.viewingPrevImage) {
				timeLeft = timeout;
				lastPassType = State.passType
				State.viewingPrevImage = false;
			}

			// Freeze the progress circle if paused or not displaying a pic.
			if (ProgressBarManager.isPaused || ((State.passType == State.STATE_STOP || State.passType == State.STATE_CANCEL || State.passType == State.STATE_DONE) && !document.getElementById("sexyNoFap").checked))
			{
				referenceTime = performance.now();
				ProgressBarManager.timeoutSlideProgress = setTimeout(continueSlideProgress, 25);
				return;
			}
			
			timeLeft -= performance.now() - referenceTime;
			referenceTime = performance.now();
			var percent = (timeout - timeLeft) / timeout * 100;
			Visuals.updateNextImageCircle(percent);
			
			if (timeLeft <= 0)
			{
				if (State.optionGameMode == State.GAMEMODE_NORMAL)
				{
					GameModeNormal.slideshowSteps();
				}
				else if (State.optionGameMode == State.GAMEMODE_ENDURANCE)
				{
					GameModeEndurance.slideshowSteps();
				}
				callback();
			}
			else
			{
				ProgressBarManager.timeoutSlideProgress = setTimeout(continueSlideProgress, 25);
			}
		};
		
		continueSlideProgress();
	},
	
	// Calls the specified callback after the specified timeout,
	// displaying the progress on the specified progress bar.
	showProgressAndCallback: function(callback, timeout, strIdBar)
	{
		var timeLeft = timeout;
		var referenceTime = performance.now();
		var continueProgress = function() {
			if (ProgressBarManager.isPaused)
			{
				referenceTime = performance.now();
				ProgressBarManager.timeoutHandle = setTimeout(continueProgress, 25);
				return;
			}
			
			timeLeft -= performance.now() - referenceTime;
			referenceTime = performance.now();
			var percent = (timeout - timeLeft) / timeout * 100;
			document.getElementById(strIdBar).style.setProperty("width", percent + "%");
			
			if (timeLeft <= 0)
			{
				document.getElementById("divCumbarFill").style.setProperty("width", "0%");
				callback();
			}
			else
			{
				ProgressBarManager.timeoutHandle = setTimeout(continueProgress, 25);
			}
		};
		
		continueProgress();
	},
	
	togglePause: function()
	{
		ProgressBarManager.isPaused = !ProgressBarManager.isPaused;
	},
	
	cancelProgress: function()
	{
		if (!ProgressBarManager.timeoutHandle)
		{
			return this;
		}
		
		clearTimeout(ProgressBarManager.timeoutHandle);
		
		ProgressBarManager.timeoutHandle = null;
		return this;
	},

	cancelSlideProgress: function()
	{
		if (!ProgressBarManager.timeoutSlideProgress)
		{
			return this;
		}
		
		clearTimeout(ProgressBarManager.timeoutSlideProgress);
		
		ProgressBarManager.timeoutSlideProgress = null;
		return this;
	}
};
