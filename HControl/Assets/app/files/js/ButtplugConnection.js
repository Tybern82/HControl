// https://github.com/buttplugio/buttplug-csharp - plugin available

class ButtplugConnection {
    // This will hold active connections.
    static #arrConnection = [];
    static #dMaxVibratePower = 1.0;
    static #dMaxOscillatePower = 1.0;
    static #dMaxRotatePower = 1.0;
    static #bConstrict = true;
    static #dMinMoveBps = 1.0;
    static #dMaxMoveBps = 5.0;
    static #dMaxMoveLength = 1.0;
    static #dLastPower = 0.0;
    static #bPaused = false;

    #client = new Buttplug.ButtplugClient("Heat Control " + VERSION);
    #connector = null;
    #callbackDisconnect = null;
    #arrCallbackMove = [];

    //#region static functions

    static setLastPower(dPower)
    {
        ButtplugConnection.#dLastPower = dPower;
    }

    static setMaxVibratePower(dPower)
    {
        if (isNaN(dPower))
        {
            debugConsole.error("Wrong parameter in setMaxVibratePower().");
            return;
        }

        // Safety cap the power.
        if (dPower < 0.0)
		{
			dPower = 0.0;
		}

		if (dPower > 1.0)
		{
			dPower = 1.0;
		}

        ButtplugConnection.#dMaxVibratePower = dPower;
    }

    static setMaxOscillatePower(dPower)
    {
        if (isNaN(dPower))
        {
            debugConsole.error("Wrong parameter in setMaxOscillatePower().");
            return;
        }

        // Safety cap the power.
        if (dPower < 0.0)
		{
			dPower = 0.0;
		}

		if (dPower > 1.0)
		{
			dPower = 1.0;
		}

        ButtplugConnection.#dMaxOscillatePower = dPower;
    }

    static setMaxRotatePower(dPower)
    {
        if (isNaN(dPower))
        {
            debugConsole.error("Wrong parameter in setMaxRotatePower().");
            return;
        }

        // Safety cap the power.
        if (dPower < 0.0)
		{
			dPower = 0.0;
		}

		if (dPower > 1.0)
		{
			dPower = 1.0;
		}

        ButtplugConnection.#dMaxRotatePower = dPower;
    }

    static setConstrictEnabled(bConstrict)
    {
        ButtplugConnection.#bConstrict = bConstrict;
    }

    static setMoveBpsRange(dMinBps, dMaxBps)
    {
        if (dMinBps > dMaxBps)
        {
            debugConsole.error("dMinBps can't be higher than dMaxBps.");
            return;
        }

        ButtplugConnection.#dMinMoveBps = dMinBps;
        ButtplugConnection.#dMaxMoveBps = dMaxBps;
    }

    static setMaxMoveLength(dLength)
    {
        if (isNaN(dLength))
        {
            debugConsole.error("Wrong parameter in setMaxMoveLength().");
            return;
        }

        // Safety cap the length.
        if (dLength < 0.2)
		{
			dLength = 0.2;
		}

		if (dLength > 1.0)
		{
			dLength = 1.0;
		}

        ButtplugConnection.#dMaxMoveLength = dLength;
    }

    /** 
    * Makes all devices from all active connections activate.
    * They vibrate rotate and oscillate depending on features.
    * @param {number} dPower 
    * The action intensity, between 0.0 and 1.0.
    */
    static activateAllConnected(dPower)
    {
        ButtplugConnection.#bPaused = false;
        ButtplugConnection.#dLastPower = dPower;
        ButtplugConnection.#arrConnection.forEach((buttplugConnection) => {
            buttplugConnection.vibrateAll(dPower);
            buttplugConnection.oscillateAll(dPower);
            buttplugConnection.rotateAll(dPower);
            buttplugConnection.constrictAll(dPower);
            buttplugConnection.moveAll(dPower);
        });
    }

    /** 
    * Stops all devices from all active connections.
    */
    static pauseAll()
    {
        ButtplugConnection.#bPaused = true;
        ButtplugConnection.#arrConnection.forEach((buttplugConnection) => buttplugConnection.stopAllDevices());
    }

    /** 
    * Makes all devices from all active connections resume their initial action,
    * using the last used intensity.
    */
    static unpauseAll()
    {
        // Calling activateAllConnected() will unset the pause flag.
        ButtplugConnection.activateAllConnected(ButtplugConnection.#dLastPower);
    }

    /** 
    * Stops any vibration from all devices from all active connections.
    * Unpausing will also not trigger vibrations either.
    */
    static stopAllDevicesConnected()
    {
        ButtplugConnection.#dLastPower = 0.0;
        ButtplugConnection.#arrConnection.forEach((buttplugConnection) => buttplugConnection.stopAllDevices());
    }

    //#endregion static functions
    
    constructor(strUrl)
    {
        // Prepare connection to the requested URL.
        this.#connector = new Buttplug.ButtplugBrowserWebsocketClientConnector(strUrl);

        // Make this object instance available to callbacks.
        let connection = this;

        function onDeviceAdded(device)
        {
            connection.#onDeviceAdded(device);
        }

        function onDeviceRemoved(device)
        {
            connection.#onDeviceRemoved(device);
        }

        function onDisconnect()
        {
            connection.#onDisconnect();
        }

        // Add event handlers.
        this.#client.addListener("deviceadded", onDeviceAdded);
        this.#client.addListener("deviceremoved", onDeviceRemoved);
        this.#client.addListener("disconnect", onDisconnect);
    }

    //#region private

    #onDeviceAdded(device)
    {
        // We just list available devices in the console.
        debugConsole.log(`Device Connected: ${device.name}`);
        debugConsole.log("Client currently knows about these devices:");
        debugConsole.log(this.#client.devices);
        this.#client.devices.forEach((device) => debugConsole.log(`- ${device.name}`));
    }

    #onDeviceRemoved(device)
    {
        debugConsole.log(`Device Removed: ${device.name}`);
    }

    #onDisconnect()
    {
        // The connection was interrupted by the server or connectivity was lost.
        // Perform cleanup.
        this.disconnect();
    }

    async #vibrate(device, dPower)
    {
        if (device.vibrateAttributes.length == 0)
        {
            // This device can't vibrate.
            return;
        }

        // Safety cap the power.
        if (dPower < 0.0)
		{
			dPower = 0.0;
		}

		if (dPower > 1.0)
		{
			dPower = 1.0;
		}

        // Reduce power by provided setting.
        dPower *= ButtplugConnection.#dMaxVibratePower;

        debugConsole.log(`Vibrating device ${device.name} at power ${dPower}`);

        try
        {
            await device.vibrate(dPower);
        }
        catch (e)
        {
            if (e instanceof Buttplug.ButtplugClientConnectorException)
            {
                debugConsole.warn("Connection lost while attempting to run device.vibrate()");
            }
            else
            {
                // Other error.
                debugConsole.error(e);
            }
        }
    }

    async #oscillate(device, dPower)
    {
        if (device.oscillateAttributes.length == 0)
        {
            // This device can't oscillate.
            return;
        }

        // Safety cap the power.
        if (dPower < 0.0)
		{
			dPower = 0.0;
		}

		if (dPower > 1.0)
		{
			dPower = 1.0;
		}

        // Reduce power by provided setting.
        dPower *= ButtplugConnection.#dMaxOscillatePower;

        debugConsole.log(`Oscillating device ${device.name} at power ${dPower}`);

        try
        {
            await device.oscillate(dPower);
        }
        catch (e)
        {
            if (e instanceof Buttplug.ButtplugClientConnectorException)
            {
                debugConsole.warn("Connection lost while attempting to run device.oscillate()");
            }
            else
            {
                // Other error.
                debugConsole.error(e);
            }
        }
    }

    async #rotate(device, dPower)
    {
        if (device.rotateAttributes.length == 0)
        {
            // This device can't rotate.
            return;
        }

        // Safety cap the power.
        if (dPower < 0.0)
		{
			dPower = 0.0;
		}

		if (dPower > 1.0)
		{
			dPower = 1.0;
		}

        // Reduce power by provided setting.
        dPower *= ButtplugConnection.#dMaxRotatePower;

        debugConsole.log(`Rotating device ${device.name} at power ${dPower}`);

        try
        {
            await device.rotate(dPower);
        }
        catch (e)
        {
            if (e instanceof Buttplug.ButtplugClientConnectorException)
            {
                debugConsole.warn("Connection lost while attempting to run device.rotate()");
            }
            else
            {
                // Other error.
                debugConsole.error(e);
            }
        }
    }

    async #constrict(device, dPower)
    {
        // Do not constrict if option is disabled.
        if (!ButtplugConnection.#bConstrict)
        {
            return;
        }

        // Safety cap the power.
        if (dPower < 0.0)
        {
            dPower = 0.0;
        }

        if (dPower > 1.0)
        {
            dPower = 1.0;
        }

        // A device has a constrict feature if we find a ScalarCmd with Constrict actuator.
        let availableMessages = device._deviceInfo.DeviceMessages;
        if (typeof availableMessages.ScalarCmd === "undefined")
        {
            return;
        }

        debugConsole.log(`Constricting device ${device.name} at power ${dPower}`);

        for (let i = 0; i < availableMessages.ScalarCmd.length; ++i)
        {
            let cmd = availableMessages.ScalarCmd[i];

            if (cmd.ActuatorType === Buttplug.ActuatorType.Constrict)
            {
                let constrictSubcommand = new Buttplug.ScalarSubcommand(cmd.Index, dPower, Buttplug.ActuatorType.Constrict);
                
                try
                {
                    await device.scalar(constrictSubcommand);
                }
                catch (e)
                {
                    if (e instanceof Buttplug.ButtplugClientConnectorException)
                    {
                        debugConsole.warn("Connection lost while attempting to run device.scalar()");
                    }
                    else
                    {
                        // Other error.
                        debugConsole.error(e);
                    }
                }
            }
        }
    }

    async #move(device, dPower)
    {
        if (device.linearAttributes.length == 0 && typeof device.messageAttributes["LinearCmd"] === "undefined")
        {
            // This device can't move.
            return;
        }

        // When performing a new move command, cancel the timeout loop associated to this device.
        if (this.#arrCallbackMove[device.index])
        {
            clearTimeout(this.#arrCallbackMove[device.index]);
        }

        // Safety cap the power.
        if (dPower < 0.0)
		{
			dPower = 0.0;
		}

		if (dPower > 1.0)
		{
			dPower = 1.0;
		}

        // The min BPS value is for power level 1, default 1BPS.
        // The max BPS value is for power level 5, default 5BPS.
        // With these custom speeds, get the step to go from one power level to the next.
        let dBpsStep = (ButtplugConnection.#dMaxMoveBps - ButtplugConnection.#dMinMoveBps) / 4.0;

        // We get the theoretical BPS for a power level of 0.
        let dZeroPowerBps = ButtplugConnection.#dMinMoveBps - dBpsStep;

        // From the power level 0 (min) and power level 5 (max) values,
        // we can just scale our dPower which is from 0 to 1.
        // This becomes a BPS between 0 and 5.
        dPower = (ButtplugConnection.#dMaxMoveBps - dZeroPowerBps) * dPower + dZeroPowerBps;

        // Finally, translate the BPS value back to a 0 to 1 power scale.
        dPower /= 5.0;

        debugConsole.log(`Moving device ${device.name} at power ${dPower}`);


        // The length factor goes from p=0.4 -> l=1 to p=1 -> l=0.2 proportionally.
        // Under 0.4, it maxes out. In other words go full length for low powers.
        let dLengthFactor = ((1 - dPower) * 4.0 ) / 3.0 + 0.2;

        

        // Cap the used length according to settings.
        if (dLengthFactor > ButtplugConnection.#dMaxMoveLength)
        {
            dLengthFactor = ButtplugConnection.#dMaxMoveLength;
            debugConsole.log(`Length factor capped at: ${dLengthFactor}`);
        }

        if (dLengthFactor < 0.2)
        {
            dLengthFactor = 0.2;
            debugConsole.log(`Computed power > 1, length factor capped at 0.2`);
        }

        debugConsole.log(`Using length ${dLengthFactor}`);

        // Time needed to go one full length is proportional to power.
        // p=0.2 -> 500ms
        // p=1.0 -> 100ms
        let nTimeToReachTarget = parseInt(100.0 / dPower);
        
        debugConsole.log(`Move time: ${nTimeToReachTarget} ms`);

        // The time to travel should however never be below 80ms,
        // Messages to Intiface get too fast at this rate.
        if (nTimeToReachTarget < 80)
        {
            debugConsole.error(`TOO FAST, capping to 80ms.`);
            nTimeToReachTarget = 80;
        }

        // By reducing the length factor,
        // we bring the start position and end position closer together.
        // 1 uses all the length, 0.2 uses the 20% at the middle.
        let dPositionStart = 0.5 - (dLengthFactor / 2.0);
        let dPositionEnd = 0.5 + (dLengthFactor / 2.0);

        // Count the time spent to trigger the safety stop if loop is too fast.
        let nTimeStart = performance.now();

        // Allow callbacks to access this.
        let thisDevice = this;

        // When reaching the start position, go to the end position.
        function onReachStart()
        {
            let nTimeSpent = performance.now() - nTimeStart;
            nTimeStart = performance.now();

            if (nTimeSpent < 10.0)
            {
                // Safety precaution if promises are fulfilled too fast.
                debugConsole.error(`Move loop going too fast, aborting. Time taken: ${nTimeSpent}`);
                return;
            }

            // Stop moving if we're paused or stopped.
            if (ButtplugConnection.#bPaused || ButtplugConnection.#dLastPower == 0)
            {
                return;
            }

            device.linear(dPositionEnd, nTimeToReachTarget);
            thisDevice.#arrCallbackMove[device.index] = setTimeout(onReachEnd, nTimeToReachTarget);
        }

        // When reaching the end position, go to the start position.
        function onReachEnd()
        {
            // Stop moving if we're paused.
            if (ButtplugConnection.#bPaused || ButtplugConnection.#dLastPower == 0)
            {
                return;
            }

            device.linear(dPositionStart, nTimeToReachTarget);
            thisDevice.#arrCallbackMove[device.index] = setTimeout(onReachStart, nTimeToReachTarget);
        }

        // Start the movement chain.
        device.linear(dPositionEnd, nTimeToReachTarget);
        this.#arrCallbackMove[device.index] = setTimeout(onReachEnd, nTimeToReachTarget);
    }

    //#endregion private

    /** 
    * Sets a function to be called when the connection gets disconnected.
    */
    setHandlerDisconnect(callback)
    {
        if (typeof callback === "function")
        {
            this.#callbackDisconnect = callback;
        }
    }

    /** 
     * After creating the object, call this function to try connecting to the server.
     * @returns The URL of the server.
     * @throws {Error}
     * If the connection to the server fails.
     */
    async connect()
    {
        if (this.#client.connected)
        {
            debugConsole.warn("Client already connected.");
            return;
        }

        await this.#client.connect(this.#connector);

        // Add the successful connection to the list of active connections.
        ButtplugConnection.#arrConnection.push(this);

        return this.#connector._url;
    }

    /** 
     * After creating the object, call this function to try connecting to the server.
     * @throws {Error}
     * If somehow the connection disconnected
     * between the connect check and the attempt at disconnecting.
     */
    async disconnect()
    {
        // Remove the connection from the list of active connections.
        const iConnection = ButtplugConnection.#arrConnection.indexOf(this);
        if (iConnection != -1)
        {
            ButtplugConnection.#arrConnection.splice(iConnection, 1);
        }

        // Run the callback when disconnecting.
        if (this.#callbackDisconnect)
        {
            this.#callbackDisconnect();
            this.#callbackDisconnect = null;
        }

        if (!this.#client.connected)
        {
            debugConsole.log("Client already disconnected.");
            return;
        }

        await this.#client.disconnect();

        debugConsole.log("Client disconnected.");
    }

    /** 
     * Attempts to make all connected devices vibrate.
     * @param {number} dVibrationPower 
     * Number between 0.0 and 1.0.
     * The higher the number, the harder the vibration.
     */
    vibrateAll(dVibrationPower)
    {
        if (!this.#client.connected)
        {
            NotifMessage.displayWarning("Can't vibrate: client disconnected.");
            return;
        }

        if (isNaN(dVibrationPower))
        {
            debugConsole.error("Wrong parameter in vibrateAll().");
            return;
        }

        // Try to make any device that can vibrate, vibrate.
        this.#client.devices.forEach((device) => this.#vibrate(device, dVibrationPower));
    }

    /** 
     * Attempts to make all connected devices oscillate.
     * @param {number} dOscillationPower 
     * Number between 0.0 and 1.0.
     * The higher the number, the harder the movement.
     */
    oscillateAll(dOscillationPower)
    {
        if (!this.#client.connected)
        {
            NotifMessage.displayWarning("Can't oscillate: client disconnected.");
            return;
        }

        if (isNaN(dOscillationPower))
        {
            debugConsole.error("Wrong parameter in oscillateAll().");
            return;
        }

        // Try to make any device that can oscillate, oscillate.
        this.#client.devices.forEach((device) => this.#oscillate(device, dOscillationPower));
    }

    /** 
     * Attempts to make all connected devices rotate.
     * @param {number} dRotationPower 
     * Number between 0.0 and 1.0.
     * The higher the number, the harder the rotation.
     */
    rotateAll(dRotationPower)
    {
        if (!this.#client.connected)
        {
            NotifMessage.displayWarning("Can't rotate: client disconnected.");
            return;
        }

        if (isNaN(dRotationPower))
        {
            debugConsole.error("Wrong parameter in rotateAll().");
            return;
        }

        // Try to make any device that can rotate, rotate.
        this.#client.devices.forEach((device) => this.#rotate(device, dRotationPower));
    }

    /** 
     * Attempts to make all connected devices constrict.
     * @param {number} dConstrictPower 
     * Number between 0.0 and 1.0.
     * The higher the number, the harder the constriction.
     */
    constrictAll(dConstrictPower)
    {
        if (!this.#client.connected)
        {
            NotifMessage.displayWarning("Can't constrict: client disconnected.");
            return;
        }

        if (isNaN(dConstrictPower))
        {
            debugConsole.error("Wrong parameter in constrictAll().");
            return;
        }

        // Try to make any device that can constrict, constrict.
        this.#client.devices.forEach((device) => this.#constrict(device, dConstrictPower));
    }

    /** 
     * Attempts to make all connected devices move.
     * @param {number} dRotationPower 
     * Number between 0.0 and 1.0.
     * The higher the number, the more intense the move.
     */
    moveAll(dMovePower)
    {
        if (!this.#client.connected)
        {
            NotifMessage.displayWarning("Can't move: client disconnected.");
            return;
        }

        if (isNaN(dMovePower))
        {
            debugConsole.error("Wrong parameter in moveAll().");
            return;
        }

        // Try to make any device that can move, move.
        this.#client.devices.forEach((device) => this.#move(device, dMovePower));
    }

    /** 
     * Stop all connected devices.
     */
    stopAllDevices()
    {
        if (!this.#client.connected)
        {
            NotifMessage.displayWarning("Can't stop: client disconnected.");
            return;
        }

        debugConsole.log("Stopping all devices.");

        this.#client.stopAllDevices();
    }
}
