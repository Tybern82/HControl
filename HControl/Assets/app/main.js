const {app, BrowserWindow, ipcMain, dialog, shell} = require('electron');
const path = require('path');
const fs = require('fs');

var mainWindow;

function createWindow () {
	// Create the browser window.
	mainWindow = new BrowserWindow({
		show: false,
		width: 800,
		height: 600,
		webPreferences: {
			preload: path.join(__dirname, 'preload.js')
		}
	});
	  
	mainWindow.removeMenu();
	mainWindow.maximize();
	mainWindow.show();

	// Load main page.
	mainWindow.loadFile('index.html')
  
	// Open URLs in default browser.
	mainWindow.webContents.setWindowOpenHandler(({ url }) => {
		shell.openExternal(url);
		return { action: 'deny' };
	});
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.whenReady().then(() => {
	createWindow();

	app.on('activate', function () {
		// On macOS it's common to re-create a window in the app when the
		// dock icon is clicked and there are no other windows open.
		if (BrowserWindow.getAllWindows().length === 0)
		{
			createWindow();
		}
	});
	
	// Set handlers for app shortcuts.
	mainWindow.webContents.on("before-input-event", (event, input) => {
		if (input.type ==  "keyDown" && !input.isAutoRepeat)
		{
			if (input.key == "F11")
			{
				mainWindow.setFullScreen(!mainWindow.isFullScreen());
			}
			else if (input.key == "F12")
			{
				// Open the DevTools.
				mainWindow.webContents.openDevTools();
			}
			else if (input.control)
			{
				// Set zoom when using ctrl +/-
				if (input.key == "="||input.key == "+")
				{
					var currentZoom = mainWindow.webContents.getZoomFactor();
					currentZoom += 0.1;
					
					if (currentZoom <= 5.0)
					{
						mainWindow.webContents.zoomFactor = currentZoom;
					}
					
				}
				else if (input.key == "-"||input.key == "_")
				{
					var currentZoom = mainWindow.webContents.getZoomFactor();
					currentZoom -= 0.1;
					
					if (currentZoom > 0.2)
					{
						mainWindow.webContents.zoomFactor = currentZoom;
					}
				}else if (input.key == "0") {
					mainWindow.webContents.zoomFactor = 1;
				}else if (input.key == "r") {
					mainWindow.webContents.reloadIgnoringCache();
				}
			}
		}
	});
	
	// Set zoom when using ctrl mousewheel.
	mainWindow.webContents.on("zoom-changed", (event, zoomDirection) => {
		var currentZoom = mainWindow.webContents.getZoomFactor();
		
		if (zoomDirection == "in")
		{
			currentZoom += 0.1;
		}
		else
		{
			currentZoom -= 0.1;
			
		}
		
		if (currentZoom > 0.2 && currentZoom <= 5.0)
		{
			mainWindow.webContents.zoomFactor = currentZoom;
		}
	});
})

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') app.quit()
})


// Get files in a directory.
function walkSync(dir, filelist) {
    var path = path || require('path');
    var fs = fs || require('fs'),
        files = fs.readdirSync(dir);
		
    filelist = filelist || [];
	
    files.forEach(function(file) {
        if (fs.statSync(path.join(dir, file)).isDirectory())
		{
            filelist = walkSync(path.join(dir, file), filelist);
        }
        else
		{
            filelist.push(path.join(dir, file));
        }
    });
	
    return filelist;
};

function loadFolderContent(strPath, bIncludeSubfolders)
{
	let result = {};

	// The first element of the result list is the directory name.
	let filelist = [strPath];

	result.bIncludeSubfolders = bIncludeSubfolders;

	// Recursive call to include subfolders.
	if (bIncludeSubfolders)
	{
		result.filelist = walkSync(strPath, filelist);
		return result;
	}
	
	// Ignore folders.
	let files=fs.readdirSync(strPath);
	
	files.forEach(function(file) {
        if (!fs.statSync(path.join(strPath, file)).isDirectory())
		{
            filelist.push(path.join(strPath, file));
        }
    });
	
	result.filelist = filelist;
	return result;
};

function showDialog(bIncludeSubfolders)
{
	let strPath = dialog.showOpenDialogSync({
		properties: ['openDirectory']
	});
	
	// Check for cancel.
	if (typeof strPath === 'undefined')
	{
		return;
	}

	return loadFolderContent(strPath[0], bIncludeSubfolders);
};


ipcMain.handle("showDialog", (e, bIncludeSubfolders) => { return showDialog(bIncludeSubfolders); });
ipcMain.handle("loadFolderContent", (e, data) => { return loadFolderContent(data.strPath, data.bIncludeSubfolders); });
