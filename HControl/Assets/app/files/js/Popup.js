class Popup {
    #divModalPopup;
    #divOptionContainer;
    #callbackOnClick;

    constructor(strMessage)
    {
        // Make the object accessible to the callback.
        let popup = this;

        this.#callbackOnClick = function onClick(event) {
            popup.#close();
        }

        // Dark overlay.
        this.#divModalPopup = document.createElement("div");
        this.#divModalPopup.classList.add("modalPopup");

        // The main popup.
        this.divPopup = document.createElement("div");
        this.#divModalPopup.appendChild(this.divPopup);

        this.divPopup.classList.add("popup");
        this.divPopup.innerText = strMessage;

        // Add zone in the popup for buttons.
        this.#divOptionContainer = document.createElement("div");
        this.#divOptionContainer.classList.add("popupOptionContainer");
        this.divPopup.appendChild(this.#divOptionContainer);

        // Clicking on the popup or its background closes it.
        this.#divModalPopup.addEventListener("click", this.#callbackOnClick);
        this.#divModalPopup.style.cursor = "pointer";
        this.divPopup.addEventListener("click", this.#callbackOnClick);
        this.divPopup.style.cursor = "pointer";

        // Display the popup.
        document.getElementById("divPopupContainer").appendChild(this.#divModalPopup);
    }

    #close()
    {
        this.#divModalPopup.remove();
    }

    addOption(strOption, fnCallback)
    {
        // If adding a button, don't close the popup by clicking anywhere.
        this.divPopup.removeEventListener("click", this.#callbackOnClick);
        this.divPopup.style.removeProperty("cursor");
        this.#divModalPopup.removeEventListener("click", this.#callbackOnClick);
        this.#divModalPopup.style.removeProperty("cursor");

        let divOption = document.createElement("div");
        
        divOption.classList.add("popupOption");
        divOption.textContent = strOption;
        this.#divOptionContainer.appendChild(divOption);

        if (fnCallback)
        {
            // Perform a custom action on click.
            divOption.addEventListener("click", fnCallback);
        }

        // Close the popup after performing the action.
        divOption.addEventListener("click", this.#callbackOnClick);

        return divOption;
    }
}
