var FlashManager = {
	MODE_CIRCLE: 1,
	MODE_BACKGROUND: 2,
	STATE_ON: "on",
	STATE_OFF: "off",
	isEnabled: true,
	nMode: 0,
	bSound: false,
	soundTick: new Audio('./files/sfx/Tick.wav'),
	
	setEnabled: function(enabled)
	{
		FlashManager.isEnabled = enabled;
	},
	
	setMode: function(mode)
	{
		FlashManager.nMode = mode;
	},
	
	setSound: function(enabled)
	{
		FlashManager.bSound = enabled;
	},
	
	// Calling this function will make the Flash square blink at the specified fps,
	// On the relevant screens.
	// Call with 0 to stop it.
	updateFlash: function(fps)
	{
		clearInterval(window.flashInterval);
		
		var flashControl = document.getElementById("flash");
		var flashBgControl = document.getElementById("flashBg");
		var flashControlImg = document.getElementById("flashImg");
		
		flashControl.style.setProperty("display", "none");
		flashBgControl.style.setProperty("display", "none");
		flashControlImg.style.setProperty("display", "none");
		
		Visuals.changeFlash("off");
		
		if (!fps)
		{
			debugConsole.log("Stopping beat.");
			flashControlImg.style.removeProperty("display");

			// Make the vibration tied to the flashing speed.
			// In case of a BPM of 0, we stop the vibration.
			ButtplugConnection.stopAllDevicesConnected();
			HandyControl.stop();
			return;
		}

		// Make the vibration tied to the flashing speed.
		// Even if the visuals are disabled, we enable vibrations if a vibrator is connected.
		// The max BPM is assumed to be 5 in messages, anything above will simply max out the power.
		ButtplugConnection.activateAllConnected(fps / 5.0);
		HandyControl.setVelocity(fps * 100 / 5);
		
		if (!FlashManager.isEnabled)
		{
			return;
		}
		
		debugConsole.log("Going at " + fps + "bps.");
		
		// Default mode is circle, nothing to do.
		var controlToUpdate = flashControl;
		
		// Background mode will change the background class instead of the circle class.
		if (FlashManager.nMode === FlashManager.MODE_BACKGROUND)
		{
			controlToUpdate = flashBgControl;
		}
		
		controlToUpdate.style.removeProperty("display");
		
		// Start at the end, so the loop attempts to remove the last one,
		// increments to the first, and adds the first class,
		// effectively starting from the beginning.
		var classes = [FlashManager.STATE_ON, FlashManager.STATE_OFF];
		var indexClass = classes.length - 1;
		
		var doUpdateFlash = function() {
			indexClass = (indexClass + 1) % classes.length;
			
			Visuals.changeFlash(classes[indexClass]);
			
			// Play a tick sound everytime it blinks, if feature is activated.
			if (FlashManager.bSound && classes[indexClass] == FlashManager.STATE_ON && !State.muted)
			{
				FlashManager.soundTick.currentTime=0;
				FlashManager.soundTick.play();
			}
		};
		
		var timeout = 1000 / fps / classes.length;
		window.flashInterval = setInterval(doUpdateFlash, timeout);
	},
};

FlashManager.soundTick.loop = false;
