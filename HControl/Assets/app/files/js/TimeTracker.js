class TimeTracker {
    static STR_PLACEHOLDER_TIME = "${time}";

    static #nMinTimeTrigger = 120000;
    static #nMaxTimeTrigger = 600000;
    static #arrStrMessages = [
        `Oh look, it's been ${TimeTracker.STR_PLACEHOLDER_TIME} minutes!`,
        `${TimeTracker.STR_PLACEHOLDER_TIME} minutes already, keep it up!`,
        `You've been at it for ${TimeTracker.STR_PLACEHOLDER_TIME} minutes so far!`,
        `${TimeTracker.STR_PLACEHOLDER_TIME} minutes since you started`,
        `Let's check the time... ${TimeTracker.STR_PLACEHOLDER_TIME} minutes now.`
    ];

    #nStartTime = 0;
    #timeoutTimerDone;

    constructor()
    {
        this.#nStartTime = Math.floor(performance.now());

        // Every 2 to 10 minutes, trigger a message showing the elapsed time.
        this.#timeoutTimerDone = setTimeout(this.#onTimerDone.bind(this), getRandInteger(TimeTracker.#nMinTimeTrigger, TimeTracker.#nMaxTimeTrigger));
    }

    #onTimerDone()
    {
        // Get the ms since the start.
        let nTimeElapsed = Math.floor(performance.now()) - this.#nStartTime;

        // 1 minute is 60000 ms.
        let nTimeElapsedMinutes = Math.floor(nTimeElapsed / 60000);

        // Display the message if the option is enabled.
        if (document.getElementById("cbShowTimerMessages").checked)
        {
            // Pick a random message from the list, apply formatting and display it.
            let strMessage = TimeTracker.#arrStrMessages[getRandInteger(0, TimeTracker.#arrStrMessages.length - 1)];
            strMessage = strMessage.replace(TimeTracker.STR_PLACEHOLDER_TIME, String(nTimeElapsedMinutes));
            NotifMessage.displayCharText(strMessage);
        }

        // Retrigger this in a loop.
        this.#timeoutTimerDone = setTimeout(this.#onTimerDone.bind(this), getRandInteger(TimeTracker.#nMinTimeTrigger, TimeTracker.#nMaxTimeTrigger));
    }

    disable()
    {
        clearTimeout(this.#timeoutTimerDone);
    }
}
