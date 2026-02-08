var GameModeSlideshow = {
	minTime: 5,
	maxTime: 999,
	
	startGame: function()
	{
		FlashManager.setEnabled(false);
		document.getElementById("imageHead").style.setProperty("display", "none");
		
		GameModeSlideshow.minTime = parseInt(document.getElementById("nbSlideshowMinSpeed").value);
		GameModeSlideshow.maxTime = parseInt(document.getElementById("nbSlideshowMaxSpeed").value);
		
		// Check values are correct.
		if (isNaN(GameModeSlideshow.minTime) || isNaN(GameModeSlideshow.maxTime) || GameModeSlideshow.minTime < 5 || GameModeSlideshow.minTime > GameModeSlideshow.maxTime)
		{
			backToMenu();
			NotifMessage.displayError("Check the time values are correct, and try again.");
			return;
		}
		
		GameModeSlideshow.runStepFap();
	},
	
	runStepFap: function()
	{
		var passType = State.STATE_GO;
		
		updateMainDisplay(passType, null);
		
		var randomTime = (GameModeSlideshow.maxTime - GameModeSlideshow.minTime) * Math.random() + GameModeSlideshow.minTime;
		debugConsole.log("Time selected: " + randomTime);
		
		ProgressBarManager.setTimeout(GameModeSlideshow.runStepFap, randomTime * 1000);
	},
	
	onClickNext: function()
	{
		ProgressBarManager.cancelProgress();
		GameModeSlideshow.runStepFap();
	},
	
	onClickPrev: function()
	{
		ProgressBarManager.cancelProgress();
		
		ImageManager.displayPrevImage();
		
		
		var randomTime = (GameModeSlideshow.maxTime - GameModeSlideshow.minTime) * Math.random() + GameModeSlideshow.minTime;
		debugConsole.log("Time selected: " + randomTime);
		
		ProgressBarManager.setTimeout(GameModeSlideshow.runStepFap, randomTime * 1000);
	},
};
