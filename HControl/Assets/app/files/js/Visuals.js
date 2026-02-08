var Visuals = {
	// Store different elements colours for each state.
	backgroundColor: {done: 0x101010},
	progressColor: {done: 0x101010},
	textColor: {done: 0x101010},
	flashColorRGB: {off: {r: 0, g: 0, b: 0}},

	hexNumToString(color) {
		if (typeof color === 'number')
		{
			color = '#' + color.toString(16).padStart(6, '0');
		}
		return color;
	},

	hexToRgb(hex) {
		var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);

		if (!result)
		{
			debugConsole.warn("Regex not matching in hexToRgb: " + hex);
			return null;
		}

		return {
		  r: parseInt(result[1], 16),
		  g: parseInt(result[2], 16),
		  b: parseInt(result[3], 16)
		};
	},
	
	changeBackground(color) {
		color = this.hexNumToString(color);
		
		document.getElementById("gamewrapper").style.setProperty("background-color", color);
		return this;
	},
	
	changeProgressBarColor(color) {
		barColor = this.hexNumToString(color);
		
		document.getElementById("progress").style.setProperty("background-color", barColor);

		document.querySelectorAll(".containsCurrentImage").forEach(el => el.style.setProperty("background-color", barColor));

		document.getElementById("divCumbar").style.setProperty("background-color", barColor);

		if (this.progressColor.stop)
		{
			document.getElementById("divCumbarFill").style.setProperty("background-color", this.hexNumToString(this.progressColor.stop));
		}

		document.getElementById("divCumbar").style.setProperty("background", "linear-gradient(to right, "+this.hexNumToString(color)+" 0%, "+this.hexNumToString(this.calculateCumbarGradient(color))+" 100%)");

		return this;
	},

	changeFlash(mode) {
		var controlToUpdate = document.getElementById("flash");
		var colorTransparency = 1.0;

		if (FlashManager.nMode === FlashManager.MODE_BACKGROUND) {
			controlToUpdate = document.getElementById("flashBg");
			colorTransparency = 0.25;
		}

		if (mode == FlashManager.STATE_ON)
		{
			controlToUpdate.style.setProperty("background", "rgba(" + this.flashColorRGB[mode].r + ", " + this.flashColorRGB[mode].g + ", " + this.flashColorRGB[mode].b + ", " + colorTransparency + ")");
		}
		else
		{
			controlToUpdate.style.removeProperty("background");
		}
		
		return this;
	},

	changeTextColor(color) {
		color = this.hexNumToString(color);
		document.getElementById("labelJerkbar").style.setProperty("color", color);
		document.getElementById("labelCumbar").style.setProperty("color", color);
		return this;
	},
	
	progress(bool) {
		document.getElementById("progress").style.setProperty("display", (bool)?"block":"none");
		this.progressText(bool);
		return this;
	},
	
	progressText(bool) {
		document.getElementById("labelJerkbar").style.setProperty("display", (bool)?"block":"none");
		document.getElementById("labelCumbar").style.setProperty("display", (bool)?"block":"none");
		return this;
	},
	
	divGameControls(bool) {
		document.getElementById("divGameControls").style.setProperty("display", (bool)?"flex":"none");
		return this;
	},
	
	showCumbar(bool) {
		document.getElementById("divJerkbar").style.setProperty("width", (bool)?"75%":"100%");
		document.getElementById("divCumbar").style.setProperty("display", (bool)?"block":"none");
		return this;
	},

	showPictureChangeCircle(bool) {
		document.getElementById("nextImageCircle").style.setProperty("display", (bool)?"block":"none");
		return this;
	},
	
	reset() {
		this.progress(true).divGameControls(false).showCumbar(false);
		this.showPictureChangeCircle((State.optionPermaSlideshow && State.optionGameMode != State.GAMEMODE_SLIDESHOW));
		return this;
	},

	updateColors() {
		this.progressColor.go = this.progressColor.first = this.progressColor.finish = this.calculateBarColor(this.backgroundColor.go);

		if (State.optionGameMode != State.GAMEMODE_SLIDESHOW) {
			this.textColor.go = this.textColor.first = this.textColor.finish = this.invertTextColor(this.progressColor.go);
		}else {
			this.textColor.go = this.textColor.first = this.textColor.finish = "rgba(0,0,0,0)";
		}

		this.flashColorRGB.on = this.hexToRgb(this.hexNumToString(this.progressColor.go));

		this.progressColor.stop = this.progressColor.cancel = this.calculateBarColor(this.backgroundColor.stop);

		if (State.optionGameMode != State.GAMEMODE_SLIDESHOW) {
			this.textColor.stop = this.textColor.cancel = this.invertTextColor(this.progressColor.stop);
		}else {
			this.textColor.stop = this.textColor.cancel = "rgba(0,0,0,0)";
		}
	},

	calculateBarColor(backgroundValue, mult) {
		// Mult is a value between 0 and 1.
		// 0 makes the colour the same.
		// 1 makes it more contrasted.
		if (mult === undefined || mult === null) {
			// Default of halfway.
			mult = 0.5;
		}
		else if (mult < 0) {
			mult = 0;
		}
		else if (mult > 1) {
			mult = 1;
		}

		// Special case for pure black so we don't divide by 0.
		if (backgroundValue == 0)
		{
			var color = Math.floor(127 * mult);
			backgroundValue = (color << 16) + (color << 8) + color;
			return backgroundValue;
		}

		// Get the bar colour constrasted with the background colour.
		var colorR = (backgroundValue & 0xFF0000) >> 16;
		var colorG = (backgroundValue & 0xFF00) >> 8;
		var colorB = backgroundValue & 0xFF;

		// Find the highest colour component.
		var colorMax = colorR;
		if (colorG > colorMax)
		{
			colorMax = colorG;
		}
		
		if (colorB > colorMax)
		{
			colorMax = colorB;
		}
		
		// If any colour's intensity is more than half, the colour is light.
		var bIsLight = backgroundValue & 0x808080; 
		
		if (!bIsLight)
		{
			// Compute the multiply factor to get this highest value lighter.
			var targetMax = colorMax + Math.floor((255 - colorMax) * mult);
			var mulFactor = targetMax / colorMax;
		}
		else
		{
			// Compute the multiply factor to get this highest value darker.
			var targetMin = colorMax - Math.floor(colorMax * mult);
			var mulFactor = targetMin / colorMax;
		}
		
		// Apply the multiplier to each component.
		colorR = Math.floor(colorR * mulFactor);
		colorG = Math.floor(colorG * mulFactor);
		colorB = Math.floor(colorB * mulFactor);
		
		backgroundValue = colorR;
		backgroundValue <<= 8;
		backgroundValue += colorG;
		backgroundValue <<= 8;
		backgroundValue += colorB;

		return backgroundValue;
	},

	calculateCumbarGradient(color) {
		return this.calculateBarColor(color, 0.9);
	},

	invertTextColor(barColor) {
		// Estimate colour intensity.
		// Light returns black and dark returns white.
		return barColor & 0x808080? 0x000000: 0xffffff;
	},

	updateNextImageCircle(percent) {
		var angle = percent * 3.6;

		if (angle < 0)
		{
			debugConsole.warn("Wrong parameter passed to updateNextImageCircle: " + percent);
			angle = 0;
		}

		if (angle > 360)
		{
			debugConsole.warn("Wrong parameter passed to updateNextImageCircle: " + percent);
			angle = 360;
		}

		document.getElementById("quadrant1").style.setProperty("transform", "rotate(270deg) skew(" + Math.max((90 - angle),0) + "deg)");
		document.getElementById("quadrant2").style.setProperty("transform", "rotate(180deg) skew("+((angle > 270)?Math.max((360 - angle),0):90)+"deg)");
		document.getElementById("quadrant3").style.setProperty("transform", "rotate(90deg) skew("+((angle > 180)?Math.max((270 - angle),0):90)+"deg)");
		document.getElementById("quadrant4").style.setProperty("transform", "rotate(0deg) skew("+((angle > 90)?Math.max((180 - angle),0):90)+"deg)");
	},
	
	changeSegmentColor(color) {
		color=this.hexNumToString(color);
		document.getElementById("quadrant1").style.setProperty("background-color", color);
		document.getElementById("quadrant2").style.setProperty("background-color", color);
		document.getElementById("quadrant3").style.setProperty("background-color", color);
		document.getElementById("quadrant4").style.setProperty("background-color", color);
	}
};
