class SingleNotifMessage {
    divNotif = null;
    timeoutRemove = null;
    displayTime = 5;

    constructor (strMessage, textColor, fontStyle) {
        this.divNotif = null;
        this.timeoutRemove = null;
        this.displayTime = 5;
        this.display(strMessage, textColor, fontStyle);
    }

    remove()
    {
        clearTimeout(this.timeoutRemove);
        this.timeoutRemove = null;
        this.divNotif.removeEventListener('transitionend', this.remove.bind(this), false);
        this.divNotif.remove();
        this.divNotif = null;
    }

    fadeIn()
    {
        this.divNotif.style.setProperty("opacity", "1");
    }

    fadeOut()
    {
        this.divNotif.style.removeProperty("opacity");

        // Trigger the removal at the end of the fade out.
        this.divNotif.addEventListener('transitionend', this.remove.bind(this), false);
    }

    display(strMessage, color, fontStyle)
    {
        // Display the message in a self-removing NotifMessage.
        this.divNotif = document.createElement("div");

        this.divNotif.classList.add("divNotification");
        this.divNotif.textContent = strMessage;

        if (color)
        {
            this.divNotif.style.setProperty("color", color)
        }

        if (fontStyle)
        {
            this.divNotif.style.setProperty("font-style", fontStyle)
        }

        this.timeoutRemove = setTimeout(this.fadeOut.bind(this), 1000 * this.displayTime);
        this.divNotif.onclick = this.remove.bind(this);

        // Display the element completely transparent.
        document.getElementById("divNotificationContainer").appendChild(this.divNotif);

        // Trigger the fade in a bit later.
        // Waiting prevents the transition from misbehaving if we display during a fade out.
        setTimeout(this.fadeIn.bind(this), 50);
    }
}

class NotifMessage {
    static display(strMessage)
    {
        debugConsole.log(strMessage);
        let notifMessage = new SingleNotifMessage(strMessage);
    }

    static displayWarning(strMessage)
    {
        debugConsole.warn(strMessage);
        let notifMessage = new SingleNotifMessage(strMessage, "#cfbf1e");
    }

    static displayError(strMessage)
    {
        debugConsole.error(strMessage);
        let notifMessage = new SingleNotifMessage(strMessage, "#800000");
    }

    static displayCharText(strMessage)
    {
        let notifMessage = new SingleNotifMessage('"' + strMessage + '"', "#1D4A66", "italic");
    }
}
