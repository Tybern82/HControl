class HoldableButton {
    #button;
    #callback;
    #callbackOnMouseDown;
    #timeoutHoldButton;
    #intervalHoldButton;

    constructor(button, callback)
    {
        // Button must be an input of type button.
        if (button === null)
        {
            throw new TypeError("Wrong argument: 'button' not a button input.");
        }

        if (typeof button !== "object")
        {
            throw new TypeError("Wrong argument: 'button' not a button input.");
        }

        if (button.tagName !== "INPUT")
        {
            throw new TypeError("Wrong argument: 'button' not a button input.");
        }

        if (button.type !== "button")
        {
            throw new TypeError("Wrong argument: 'button' not a button input.");
        }

        // Callback must be a function.
        if (typeof callback !== "function")
        {
            throw new TypeError("Wrong argument: 'callback' not a function.");
        }

        this.button = button;
        this.callback = callback;

        // Give callbacks access to this object.
        let holdableButton = this;

        function onMouseDown(event)
        {
            document.body.addEventListener(EVENT_MOUSEUP, onMouseUp);
            holdableButton.#onMouseDown();
        }

        function onMouseUp(event)
        {
            document.body.removeEventListener(EVENT_MOUSEUP, onMouseUp);
            holdableButton.#onMouseUp();
        }

        this.callbackOnMouseDown = onMouseDown;

        // A click triggers an action and rapid fires if held.
        button.addEventListener(EVENT_MOUSEDOWN, onMouseDown);
    }

    destroy()
    {
        // Stop any ongoing click and disable click event.
        this.#onMouseUp();
        this.button.removeEventListener(EVENT_MOUSEDOWN, this.callbackOnMouseDown);
    }

    #onMouseDown(event)
    {
        this.callback();

        // Give access to this object in callback.
        let holdableButton = this;

        // Trigger rapid fire.
        function onTimeout()
        {
            holdableButton.intervalHoldButton = setInterval(onTick, 50);
        }

        // Rapid fire function called every interval tick.
        function onTick()
        {
            holdableButton.callback();
        }

        // Some time after holding, trigger rapid fire.
        this.timeoutHoldButton = setTimeout(onTimeout, 200);
    }

    #onMouseUp(event)
    {
        // Stop all timeouts, return to base state listening for a click.
        clearTimeout(this.timeoutHoldButton);
        clearInterval(this.intervalHoldButton);
        this.timeoutHoldButton = null;
        this.intervalHoldButton = null;
    }
}
