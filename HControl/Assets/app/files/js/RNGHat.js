// RNG that acts like picking numbers out of a hat, and putting them back in the hat immediately.
var RNGHat = {
	getRand: function(max)
	{
		return Math.floor(Math.random() * (max + 1));
	}
};
