var State = {
	STATE_DONE: "done",
	STATE_GO: "go",
	STATE_FIRST: "first",
	STATE_FINISH: "finish",
	STATE_CANCEL: "cancel",
	STATE_STOP: "stop",

	GAMEMODE_NORMAL: "gameModeNormal",
	GAMEMODE_ENDURANCE: "gameModeEndurance",
	GAMEMODE_SLIDESHOW: "gameModeSlideshow",

	optionGameMode: "",
	optionPermaSlideshow: false,
	optionFastQuality: false,

	passType: "",
	viewingPrevImage: false,
	justBlacklisted: false,
	muted: false,

	imageBrowserListName: null,

	applySettings: function()
	{
		if (document.getElementById("rbGameModeNormal").checked)
		{
			this.optionGameMode = this.GAMEMODE_NORMAL;
		}
		else if (document.getElementById("rbGameModeEndurance").checked)
		{
			this.optionGameMode = this.GAMEMODE_ENDURANCE;
		}
		else if (document.getElementById("rbGameModeSlideshow").checked)
		{
			this.optionGameMode = this.GAMEMODE_SLIDESHOW;
		}

		this.optionPermaSlideshow = document.getElementById("permaSlideshow").checked;
		this.optionFastQuality = document.getElementById("cbFastQuality").checked;
	},

	updatePassType(passType) {
		Visuals.reset();
		this.passType=passType
		switch (passType) {
			case State.STATE_DONE:
				Visuals.progress(false);
				break;
			case State.STATE_GO:
			case State.STATE_FIRST:
				Visuals.divGameControls(true);
				break;
			case State.STATE_FINISH:
				Visuals.divGameControls(true)
				.showCumbar(true);
				break;
			case State.STATE_CANCEL:
				Visuals.progress(false);
				break;
			case State.STATE_STOP:
				Visuals.progressText(false);
				if (document.getElementById("sexyNoFap").checked) {
					Visuals.divGameControls(true);
				}
				break;
			default:
				debugConsole.error("Error: invalid passType")
				return;
		}

		// Get the colours for the current pass type.
		var backgroundValue = Visuals.backgroundColor[passType];
		var progressBarValue = Visuals.progressColor[passType];
		var textValue = Visuals.textColor[passType];
		
		Visuals.changeBackground(backgroundValue);
		Visuals.changeProgressBarColor(progressBarValue);
		Visuals.changeTextColor(textValue);
		Visuals.changeSegmentColor(progressBarValue);
	}
};
