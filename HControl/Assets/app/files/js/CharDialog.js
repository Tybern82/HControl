class characterDialogue {
	constructor(edgeMessages, enduranceMessages) {
		this.messages = {}
		messages.edgeMessages = edgeMessages;
		messages.enduranceMessages = enduranceMessages;
	}

	appendMessage (message, type, passType) {
		this.messages[type][passType].push(message);
	}
}

class message {
	constructor (msg, minTime, maxTime, beatRate) {
		this.msg = msg;
		this.minTime = minTime;
		this.maxTime = maxTime;
		this.beatRate = beatRate;
	}
}

var messages = {
	"first" : [
		{ msg: "Let's start, warm up and fap for a while, then stop when the EDGE bar reaches the end.", minTime: 30, maxTime: 50, beatRate: 2.5}
	],
	"go" : [
		{ msg: "Fap!", minTime: 20, maxTime: 45, beatRate: 2.5},
		{ msg: "Go HARD and FAST", minTime: 8, maxTime: 15, beatRate: 5},
		{ msg: "Go HARD and FAST", minTime: 8, maxTime: 15, beatRate: 5},
		{ msg: "Fap!", minTime: 20, maxTime: 45, beatRate: 2.2},
		{ msg: "Fap!", minTime: 20, maxTime: 45, beatRate: 2.7},
		{ msg: "Stroke", minTime: 20, maxTime: 45, beatRate: 2.7},
		{ msg: "Fap!", minTime: 20, maxTime: 45, beatRate: 2},
		{ msg: "Stroke", minTime: 20, maxTime: 45, beatRate: 2.2},
		{ msg: "Stroke", minTime: 20, maxTime: 45, beatRate: 2.5},
		{ msg: "Go SLOW and steady...", minTime: 45, maxTime: 70, beatRate: 1},
		{ msg: "Use your other hand!", minTime: 20, maxTime: 45, beatRate: 2.5},
		{ msg: "Use your other hand!", minTime: 20, maxTime: 45, beatRate: 3},
		{ msg: "Stroke lightly", minTime: 15, maxTime: 30, beatRate: 2.5},
		{ msg: "Stroke lightly", minTime: 25, maxTime: 45, beatRate: 1.5},
		{ msg: "Fap!", minTime: 20, maxTime: 45, beatRate: 2.2},
		{ msg: "Fap!", minTime: 20, maxTime: 45, beatRate: 2.5}
	],
	"stop" : [
		{ msg: "STOP! Calm down...", minTime: 15, maxTime: 30},
		{ msg: "No touching!", minTime: 20, maxTime: 30},
		{ msg: "STOP! Quick break.", minTime: 7, maxTime: 15},
		{ msg: "STOP! Quick break.", minTime: 7, maxTime: 15},
		{ msg: "Hands off!", minTime: 10, maxTime: 25},
		{ msg: "Hands off!", minTime: 10, maxTime: 25},
		{ msg: "Hands off!", minTime: 10, maxTime: 25},
		{ msg: "Hands off!", minTime: 10, maxTime: 25},
		{ msg: "STOP", minTime: 10, maxTime: 20},
		{ msg: "STOP", minTime: 10, maxTime: 25},
		{ msg: "STOP", minTime: 10, maxTime: 15},
		{ msg: "STOP", minTime: 10, maxTime: 20},
		{ msg: "No touching!", minTime: 10, maxTime: 20}
	],
	"finish": [
		{ msg: "CUM! DO IT NOW!", minTime: 15, maxTime: 20, beatRate: 4},
		{ msg: "CUM! Take your time, no rush ;3", minTime: 50, maxTime: 70, beatRate: 2},
		{ msg: "CUM now!", minTime: 25, maxTime: 35, beatRate: 2.6},
		{ msg: "CUM!", minTime: 25, maxTime: 40, beatRate: 2.6},
		{ msg: "You have 10 seconds to start cumming, otherwise it's game over!", minTime: 11, maxTime: 11, beatRate: 5},
		{ msg: "You can cum now. GO!", minTime: 20, maxTime: 30, beatRate: 3}
	],
	"noCum": [
		{ msg: "STOP! Sorry, no cumming for you.\r\nTry again, maybe you will get lucky next time...", minTime: 30, maxTime: 30}
	]
};

var messagesEndurance = {
	"average" : [
		{ msg: "Good work, keep going like that!", minTime: 45, maxTime: 60 },
		{ msg: "Rest a bit, and keep it up!", minTime: 45, maxTime: 60 },
		{ msg: "Nice work, I'm sure you can keep going for a while longer!", minTime: 45, maxTime: 60 },
		{ msg: "Not done yet, I wanna see you edge more ;3", minTime: 45, maxTime: 60 },
	],
	"gentle" : [
		{ msg: "Good work, keep going like that!", minTime: 45, maxTime: 60 },
		{ msg: "Rest a bit, and keep it up!", minTime: 45, maxTime: 60 },
		{ msg: "Just a bit more, you can do it!", minTime: 45, maxTime: 60 },
	],
	"evil" : [
		{ msg: "Not done yet, I wanna see you edge more ;3", minTime: 30, maxTime: 40 },
		{ msg: "Nice, now get ready for more...", minTime: 30, maxTime: 40 },
		{ msg: "Good, let's go for more, no stopping!", minTime: 30, maxTime: 40 },
		{ msg: "I want more from you, so keep at it!", minTime: 30, maxTime: 40 },
		{ msg: "How long will I make you edge for, I wonder...", minTime: 30, maxTime: 40 },
		{ msg: "Hmm, I'm not sure you've been edging quite enough yet!", minTime: 30, maxTime: 40 },
	]
};

var messagesEnduranceEvilLong = [
	{ msg: "Hehehe, you're in for a ride, I'm feeling pretty horny...", minTime: 20, maxTime: 35 },
	{ msg: "If you wanna cum, you're gonna have to earn it.", minTime: 20, maxTime: 35 },
	{ msg: "We're far from done, so get ready...", minTime: 20, maxTime: 35 },
];
