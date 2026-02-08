class SwipeDetection extends EventTarget {
    // Minimum distance required to count as a swipe.
    static #nMinMovement = 10;
    static #instance = null;
    static #initialX = null;
    static #initialY = null;

    static EVENT_SWIPE_LEFT = "swipeleft";
    static EVENT_SWIPE_RIGHT = "swiperight";
    static EVENT_SWIPE_UP = "swipeup";
    static EVENT_SWIPE_DOWN = "swipedown";


    /**
     * Do not instanciate this class.
     */
    constructor()
    {
        super();

        // Listen to the events we need to detect touch movement.
        document.body.addEventListener("touchstart", SwipeDetection.#startTouch, false);
        document.body.addEventListener("touchmove", SwipeDetection.#moveTouch, false);
    }

    /**
     * Singleton, only one instance alive.
     * @returns The instance of SwipeDetection.
     */
    static #getInstance()
    {
        if (!SwipeDetection.#instance)
        {
            SwipeDetection.#instance = new SwipeDetection();
        }

        return SwipeDetection.#instance;
    }

    static #startTouch(e)
    {
        SwipeDetection.#initialX = e.touches[0].clientX;
        SwipeDetection.#initialY = e.touches[0].clientY;
    }

    static #moveTouch(e)
    {
        if (SwipeDetection.#initialX === null)
        {
            return;
        }

        if (SwipeDetection.#initialY === null)
        {
            return;
        }
        
        let swipeEvent = null;

        // Get the horizontal and vertical distance travelled.
        var diffX = SwipeDetection.#initialX - e.touches[0].clientX;
        var diffY = SwipeDetection.#initialY - e.touches[0].clientY;

        // Priority to the axis with the most travelled distance.
        if (Math.abs(diffX) > Math.abs(diffY))
        {
            // sliding horizontally
            if (diffX > SwipeDetection.#nMinMovement)
            {
                swipeEvent = new CustomEvent(SwipeDetection.EVENT_SWIPE_LEFT);
            }
            else if (diffX < -1 * SwipeDetection.#nMinMovement)
            {
                swipeEvent = new CustomEvent(SwipeDetection.EVENT_SWIPE_RIGHT);
            }  
        }
        else
        {
            // sliding vertically
            if (diffY > SwipeDetection.#nMinMovement)
            {
                swipeEvent = new CustomEvent(SwipeDetection.EVENT_SWIPE_UP);
            }
            else if (diffY < -1 * SwipeDetection.#nMinMovement)
            {
                swipeEvent = new CustomEvent(SwipeDetection.EVENT_SWIPE_DOWN);
            }  
        }

        // Reset for the next swipe detection.
        SwipeDetection.#initialX = null;
        SwipeDetection.#initialY = null;

        // Fire the event if there was one.
        if (swipeEvent)
        {
            SwipeDetection.#getInstance().dispatchEvent(swipeEvent);
        }
        
        e.preventDefault();
    }

    static addEventListener(strEvent, callback)
    {
        SwipeDetection.#getInstance().addEventListener(strEvent, callback);
    }

    static removeHandler(strEvent, callback)
    {
        SwipeDetection.#getInstance().removeEventListener(strEvent, callback);
    }
}