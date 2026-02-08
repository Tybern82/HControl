function calcListValue (list) {
	return Object.keys(ListManager.lists[list]).length;
}

function updateIndividualPicValues () {
	let childrenElements = document.getElementById("divSelectedLists").children;
	for (let i=0; i<childrenElements.length; i++) {
		let selectedList = childrenElements[i].id.substring(15, childrenElements[i].id.length);
		let picValue = calcListValue(selectedList)
		childrenElements[i].children[1].children[1].textContent = selectedList + " - Pics: " + picValue
	}
}

var ImageBrowser = {
	gameMode: false,
	imageCache: [],
	imageURLsCache: [],
	imagesPerPage: 50,
	currentPage: 0,
	bIsBookmarkView: false,
	removeConfirmation: false,
	blacklistConfirmation: false,
	strUrlDeleted: "files/img/iconCancel.png",
	strUrlNotFound: "files/img/iconNotFound.png",

	editBlacklist: function()
	{
		State.imageBrowserCurrentList = blacklistedPictures;
		State.bIsBookmarkView = false;

		//Retrieves the property names of the blacklisted pictures
		ImageBrowser.imageCache = [];
		ImageBrowser.imageURLsCache = [];

		Object.keys(blacklistedPictures).forEach(element => {
			ImageBrowser.imageURLsCache.push(element);
		});

		ImageBrowser.currentPage = 0;
		ImageBrowser.gameMode = false;
		ImageBrowser.displayImageGrid();
	},

	editList: function(array)
	{
		State.imageBrowserListName = array;
		array = ListManager.lists[array];
		State.imageBrowserCurrentList = array;
		State.bIsBookmarkView = false;

		//Retrieves the property names of the pictures
		ImageBrowser.imageCache = [];
		ImageBrowser.imageURLsCache = [];
		
		Object.keys(array).forEach(element => {
			ImageBrowser.imageURLsCache.push(element);
		});

		ImageBrowser.currentPage = 0;
		ImageBrowser.gameMode = false;

		if (ImageBrowser.checkList())
		{
			ImageBrowser.displayImageGrid();
		}
	},

	viewBookmarkedImages: function()
	{
		State.imageBrowserListName = "bookmarked";
		State.imageBrowserCurrentList = ListManager.bookmarkedImages;
		ImageBrowser.imageCache = [];
		ImageBrowser.imageURLsCache = [];
		State.bIsBookmarkView = true;

		Object.keys(ListManager.bookmarkedImages).forEach(element => {
			ImageBrowser.imageURLsCache.push(element);
		});

		ImageBrowser.currentPage = 0;
		ImageBrowser.gameMode = false;

		if (ImageBrowser.checkList())
		{
			ImageBrowser.displayImageGrid();
		}
	},

	checkList: function()
	{
		let cache = ImageBrowser.imageCache;
		let tempObj = {};

		for (let i = 0; i < cache.length; ++i)
		{
			if (!tempObj[cache[i].src] != null && !tempObj[cache[i].src] != undefined)
			{
				tempObj[cache[i].src] = 1;
			}
			else
			{
				debugConsole.log("Corrupted borwser list. Reconstructing...")
				ImageBrowser.loadImages();
				return false;
			}
		}

		return true;
	},

	loadImages: function()
	{
		let newImageElment, indexUrl;
		for (let i = 0; i < ImageBrowser.imageURLsCache.length; ++i)
		{
			newImageElment = new Image;
			element = ImageBrowser.imageURLsCache[i];

			//Retrieve the thumbnail if the image is from e6. That thumbnail source is generated from the existing known image source
			if (element.substr(0, E621_IMAGES_URL.length) == E621_IMAGES_URL)
			{
				indexUrl = StringIndexOf(element, "data") + 5;
				newImageElment.src = E621_IMAGES_URL + "data/preview/" + element.substr(indexUrl, 38) + ".jpg";
			}
			else
			{
				newImageElment.src = element;
			}

            newImageElment.classList.add("imageBrowserImage");

			//Lazy loading is fun. Not necessary
            newImageElment.loading = "lazy";

			//Declare new variable to allow the click function to have some way to know what i really is. Just using i does not work and returns the largest number i ever was.
			let i1=i;

			//Set a property so we know how to update the click functionality if we need to later
			newImageElment.currentPosition = i1;

			//Give it some click functionality
            newImageElment.onclick = () => { ImageBrowser.display(i1); };

			//Error catch for 404
			newImageElment.onerror = () => { ImageBrowser.imageLoadingError(i1); };

			ImageBrowser.imageCache.push(newImageElment);
		}

		ImageBrowser.checkList();
	},

	imageLoadingError: function(index)
	{
		//ImageBrowser.imageURLsCache[index] = ImageBrowser.strUrlNotFound;
		ImageBrowser.imageCache[index].src = ImageBrowser.strUrlNotFound;
	},

	updateImageGrid: function()
	{
		document.getElementById("divImageBrowserGridWrapper").textContent = "";
		document.getElementById("divImageBrowserPageNumber").textContent = "Page: " + (ImageBrowser.currentPage+1) + "/" + Math.ceil(ImageBrowser.imageURLsCache.length / ImageBrowser.imagesPerPage);

		for (let i = ImageBrowser.currentPage * ImageBrowser.imagesPerPage; i < Math.min((ImageBrowser.currentPage + 1) * ImageBrowser.imagesPerPage, ImageBrowser.imageURLsCache.length); i++)
		{
			document.getElementById("divImageBrowserGridWrapper").appendChild(ImageBrowser.imageCache[i]);
		}

		ImageBrowser.fixGridButtonDisplay();
	},

	displayImageGrid: function()
	{
		ImageBrowser.loadImages()
		ImageBrowser.updateImageGrid()
		document.getElementById("imageBrowser").style.removeProperty("display");
		document.getElementById("choose").style.setProperty("display", "none");
		document.body.style.setProperty("height", "100%");
	},

	nextPage: function()
	{
		if ((ImageBrowser.currentPage + 1) * ImageBrowser.imagesPerPage < ImageBrowser.imageURLsCache.length)
		{
			++ImageBrowser.currentPage;
			setTimeout(ImageBrowser.updateImageGrid, 20);
		}
	},

	prevPage: function()
	{
		if (ImageBrowser.currentPage > 0)
		{
			--ImageBrowser.currentPage;
			setTimeout(ImageBrowser.updateImageGrid, 20);
		}
	},

	fixGridButtonDisplay: function()
	{
		let buttonGridNextPage = document.getElementById("buttonGridNextPage");
		let buttonGridPrevPage = document.getElementById("buttonGridPrevPage");

		if ((ImageBrowser.currentPage + 1) * ImageBrowser.imagesPerPage < ImageBrowser.imageURLsCache.length)
		{
			buttonGridNextPage.classList.remove("disabled");
			buttonGridNextPage.removeAttribute("disabled");
		}
		else
		{
			buttonGridNextPage.classList.add("disabled");
			buttonGridNextPage.setAttribute("disabled", "disabled");
		}

		if (ImageBrowser.currentPage>0)
		{
			buttonGridPrevPage.classList.remove("disabled");
			buttonGridPrevPage.removeAttribute("disabled");
		}
		else
		{
			buttonGridPrevPage.classList.add("disabled");
			buttonGridPrevPage.setAttribute("disabled", "disabled");
		}
	},

	setDisabledButtons: function()
	{
		let buttonBigImageRemoveList = document.getElementById("buttonBigImageRemoveList");
		let buttonBigImageBlacklist = document.getElementById("buttonBigImageBlacklist");

		buttonBigImageRemoveList.classList.remove("disabled");
		buttonBigImageRemoveList.removeAttribute("disabled");
		buttonBigImageBlacklist.classList.remove("disabled");
		buttonBigImageBlacklist.removeAttribute("disabled");

		if (ImageBrowser.imageURLsCache[State.bigImageIndex] == ImageBrowser.strUrlDeleted)
		{
			buttonBigImageRemoveList.classList.add("disabled");
			buttonBigImageRemoveList.setAttribute("disabled", "disabled");
			buttonBigImageBlacklist.classList.add("disabled");
			buttonBigImageBlacklist.setAttribute("disabled", "disabled");
		}

		if (ImageBrowser.gameMode)
		{
			buttonBigImageRemoveList.classList.add("disabled");
			buttonBigImageRemoveList.setAttribute("disabled", "disabled");
		}

		if (State.imageBrowserListName == "bookmarked")
		{
			buttonBigImageRemoveList.classList.add("disabled");
			buttonBigImageRemoveList.setAttribute("disabled", "disabled");
		}

		if (State.imageBrowserCurrentList == blacklistedPictures)
		{
			buttonBigImageBlacklist.classList.add("disabled");
			buttonBigImageBlacklist.setAttribute("disabled", "disabled");
		}
	},

	hideImageGrid: function()
	{
		document.getElementById("imageBrowser").style.setProperty("display", "none");
		document.getElementById("choose").style.removeProperty("display");
		State.imageBrowserListName = null;
		updateIndividualPicValues();
		document.body.style.setProperty("height", "fit-content");
	},

	display: function(index)
	{
		document.getElementById("imageBrowser").style.setProperty("display", "none");
		document.getElementById("bigImageViewer").style.removeProperty("display");

		let buttonBigImagePrev = document.getElementById("buttonBigImagePrev");
		let buttonBigImageNext = document.getElementById("buttonBigImageNext");
		let buttonImageViewerViewE6 = document.getElementById("buttonImageViewerViewE6");

		if (index <= 0)
		{
			buttonBigImagePrev.classList.add("disabled");
			buttonBigImagePrev.setAttribute("disabled", "disabled");
		}
		else
		{
			buttonBigImagePrev.classList.remove("disabled");
			buttonBigImagePrev.removeAttribute("disabled");
		}

		if (index >= ImageBrowser.imageURLsCache.length - 1)
		{
			buttonBigImageNext.classList.add("disabled");
			buttonBigImageNext.setAttribute("disabled", "disabled");
		}
		else
		{
			buttonBigImageNext.classList.remove("disabled");
			buttonBigImageNext.removeAttribute("disabled");
		}

		// Viewing images in e621 is only available for images loaded from e621.
		if (ImageBrowser.imageURLsCache[index].substr(0, E621_IMAGES_URL.length) !== E621_IMAGES_URL)
		{
			buttonImageViewerViewE6.classList.add("disabled");
			buttonImageViewerViewE6.setAttribute("disabled", "disabled");
		}
		else
		{
			buttonImageViewerViewE6.classList.remove("disabled");
			buttonImageViewerViewE6.removeAttribute("disabled");
		}

		// Remove the previous image and load the image to display.
		let bigImageSrcHolder = document.getElementById("bigImageSrcHolder");
		bigImageSrcHolder.src = "";
		bigImageSrcHolder.src = ImageBrowser.imageURLsCache[index];

		State.bigImageIndex = index;
		ImageBrowser.setDisabledButtons();

		if (State.bigImageIndex + 1 > (ImageBrowser.currentPage + 1) * ImageBrowser.imagesPerPage)
		{
			ImageBrowser.currentPage++;
		}
		else if (State.bigImageIndex < (ImageBrowser.currentPage) * ImageBrowser.imagesPerPage)
		{
			ImageBrowser.currentPage--;
		}

		ImageBrowser.updateImageGrid();
		ListManager.checkAddedBrowserLists();
	},

	displayPrev: function()
	{
		if (State.bigImageIndex > 0)
		{
			ImageBrowser.display(State.bigImageIndex - 1);
			document.getElementById("buttonBigImageRemoveList").value = "Remove From List";
			document.getElementById("buttonBigImageBlacklist").value = "Blacklist Picture";
			ImageBrowser.removeConfirmation = false;
			ImageBrowser.blacklistConfirmation = false;
		}
	},

	displayNext: function()
	{
		if (State.bigImageIndex < ImageBrowser.imageURLsCache.length)
		{
			ImageBrowser.display(State.bigImageIndex + 1);
			document.getElementById("buttonBigImageRemoveList").value = "Remove From List";
			document.getElementById("buttonBigImageBlacklist").value = "Blacklist Picture";
			ImageBrowser.removeConfirmation = false;
			ImageBrowser.blacklistConfirmation = false;
		}
	},

	removeList: function()
	{
		if (ImageBrowser.blacklistConfirmation)
		{
			document.getElementById("buttonBigImageRemoveList").value = "Remove From List"
			document.getElementById("buttonBigImageBlacklist").value = "Blacklist Picture"
			ImageBrowser.blacklistConfirmation = false;
			ImageBrowser.setDisabledButtons();
			return;
		}

		if (!ImageBrowser.removeConfirmation)
		{
			document.getElementById("buttonBigImageRemoveList").value = "Confirm";
			document.getElementById("buttonBigImageBlacklist").value = "Cancel";
			document.getElementById("buttonBigImageRemoveList").classList.remove("disabled");
			document.getElementById("buttonBigImageRemoveList").removeAttribute("disabled");
			document.getElementById("buttonBigImageBlacklist").classList.remove("disabled");
			document.getElementById("buttonBigImageBlacklist").removeAttribute("disabled");
			ImageBrowser.removeConfirmation = true;
		}
		else
		{
			let tempURL = ImageBrowser.imageURLsCache[State.bigImageIndex];

			//Remove it from the list that it is a part of
			delete State.imageBrowserCurrentList[tempURL];

			//Update the files in selected lists for the Browser gamemode
			if (gameListPictures[tempURL] == 1)
			{
				delete gameListPictures[tempURL];
				document.getElementById("labelInputList").textContent = "Total Unique Pics: " + (Object.keys(gameListPictures).length - 1);
			}
			else if (gameListPictures[tempURL] > 1)
			{
				--gameListPictures[tempURL];
			}

			//Remove it from the current arrays of images and sources and replace with a placeholder
			ImageBrowser.imageURLsCache[State.bigImageIndex] = ImageBrowser.strUrlDeleted;
			ImageBrowser.imageCache[State.bigImageIndex].src = ImageBrowser.strUrlDeleted;

			if (State.bigImageIndex < ImageBrowser.imageURLsCache.length - 1)
			{
				ImageBrowser.displayNext();
			}
			else if (State.bigImageIndex > 0)
			{
				ImageBrowser.displayPrev();
			}
			else
			{
				ImageBrowser.bigImageExit();
			}

			if (State.imageBrowserCurrentList == blacklistedPictures)
			{
				localStorage.setItem("blacklistedPicturesJSON", JSON.stringify(blacklistedPictures));
			}
			else
			{
				localStorage.setItem("listsLedger", JSON.stringify(ListManager.lists));
			}

			document.getElementById("buttonBigImageRemoveList").value = "Remove From List";
			document.getElementById("buttonBigImageBlacklist").value = "Blacklist Picture";
			ImageBrowser.removeConfirmation = false;
		}
	},

	bigImageExit: function()
	{
		document.getElementById("bigImageViewer").style.setProperty("display", "none");
		document.getElementById("imageBrowser").style.removeProperty("display");
		document.getElementById("buttonBigImageRemoveList").value = "Remove From List";
		document.getElementById("buttonBigImageBlacklist").value = "Blacklist Picture";
		ImageBrowser.removeConfirmation = false;
		ImageBrowser.blacklistConfirmation = false;
	},

	blacklist: function()
	{
		if (ImageBrowser.removeConfirmation)
		{
			document.getElementById("buttonBigImageRemoveList").value = "Remove From List";
			document.getElementById("buttonBigImageBlacklist").value = "Blacklist Picture";
			ImageBrowser.removeConfirmation = false;
			ImageBrowser.setDisabledButtons();
			return;
		}

		if (!ImageBrowser.blacklistConfirmation)
		{
			let url = ImageBrowser.imageURLsCache[State.bigImageIndex];
			let count = [];
			let keys = Object.keys(ListManager.lists);

			for (let i = 0; i < keys.length; ++i)
			{
				if (ListManager.lists[keys[i]][url] != null && ListManager.lists[keys[i]][url] != undefined)
				{
					count.push(keys[i]);
				}
			}

			// Regular message.
			let strMsg = "Confirm blacklisting?";

			// Warning if the picture is getting removed from a list.
			let targetListCount = 1;

			// If we're browsing a list, then only warning if the picture is in at least 2 lists:
			// This one and another.
			if (State.imageBrowserCurrentList && !State.bIsBookmarkView)
			{
				++targetListCount;
			}

			if (count.length >= targetListCount)
			{
				strMsg = "This image is present in lists. Blacklisting it will remove it from all lists.\n\nPresent in: " + count.join(", ");
			}

			let popup = new Popup(strMsg);
			popup.addOption("Confirm", () => { ImageBrowser.blacklist(); });
			popup.addOption("Cancel", () => { ImageBrowser.removeList(); });
			
			ImageBrowser.blacklistConfirmation = true;
		}
		else
		{
			//Blacklists an image. Unsure of what to do next
			let url = ImageBrowser.imageURLsCache[State.bigImageIndex];
			State.tempURL = url;

			if (State.imageBrowserListName == null)
			{
				blacklistedPictures[url] = {id: arrImgIdByUrl[url], url: url};
			}
			else if (State.imageBrowserListName != "bookmarked")
			{
				blacklistedPictures[url] = Object.assign(ListManager.lists[State.imageBrowserListName][url]);
			}
			else
			{
				blacklistedPictures[url] = {id: arrImgIdByUrl[url], url: ListManager.bookmarkedImages[url].url};
			}

			onClickBlacklist();

			let keys = Object.keys(ListManager.lists);
			for (let i = 0; i < keys.length; ++i)
			{
				if (ListManager.lists[keys[i]][url] != null && ListManager.lists[keys[i]][url] != undefined)
				{
					delete ListManager.lists[keys[i]][url];
				}
			}

			ImageBrowser.imageURLsCache[State.bigImageIndex] = ImageBrowser.strUrlDeleted;
			ImageBrowser.imageCache[State.bigImageIndex].src = ImageBrowser.strUrlDeleted;
			localStorage.setItem("blacklistedPicturesJSON", JSON.stringify(blacklistedPictures));
			localStorage.setItem("listsLedger", JSON.stringify(ListManager.lists));

			setTimeout(() => {
				if (State.bigImageIndex < ImageBrowser.imageURLsCache.length - 1)
				{
					ImageBrowser.displayNext();
				}
				else if (State.bigImageIndex > 0)
				{
					ImageBrowser.displayPrev();
				}
				else
				{
					ImageBrowser.bigImageExit();
				}
			}, 20);

			if (gameListPictures[url] >= 1)
			{
				delete gameListPictures[url];
				document.getElementById("labelInputList").textContent = "Total Unique Pics: " + (Object.keys(gameListPictures).length - 1);
			}

			document.getElementById("buttonBigImageRemoveList").value = "Remove From List";
			document.getElementById("buttonBigImageBlacklist").value = "Blacklist Picture";
			ImageBrowser.blacklistConfirmation = false;
		}
	},

	start: function()
	{
		ImageBrowser.imageCache = [];
		ImageBrowser.imageURLsCache = [];
		ImageBrowser.gameMode = true;
		State.imageBrowserCurrentList = null;
		State.bIsBookmarkView = false;

		ImageManager.finalImages.forEach(element => {
			ImageBrowser.imageURLsCache.push(element);
		});

		ImageBrowser.currentPage = 0;
		ImageBrowser.displayImageGrid();
	},

	openCurPicturePage: function()
	{
		ImageBrowser.openPicturePage(document.getElementById("bigImageSrcHolder").src);
	},

	openPicturePage: function(imgUrl) 
	{
		if (imgUrl.substr(0, E621_IMAGES_URL.length) !== E621_IMAGES_URL)
		{
			// This is not an e6 picture, don't do anything.
			return;
		}

		// If somehow an e6 picture doesn't have an ID stored for it, send a request to retrieve it.
		// This would be caused by an older version's list not upgraded to the new structure.
		if (!arrImgIdByUrl[imgUrl])
		{
			debugConsole.log("Attempting to retrieve ID from e621 for picture " + imgUrl);

			document.getElementById("buttonImageViewerViewE6").setAttribute("disabled", "disabled");
			document.getElementById("buttonImageViewerViewE6").classList.add("disabled");

			let strFileFullName = imgUrl.substring(imgUrl.lastIndexOf("/") + 1);
			let strMd5 = strFileFullName.split(".")[0];

			// Request to get the picture info.
			function sendRequest()
			{
				fetch(
					`${E621_URL}posts.json?_client=${USER_AGENT}&tags=md5:${strMd5}&callback=?`
				).then(getPicsFromRequest, displayError);
			};

			function displayError()
			{
				NotifMessage.displayError("Network error, check your internet connection.");
				
				// Reenable button.
				document.getElementById("buttonImageViewerViewE6").removeAttribute("disabled");
				document.getElementById("buttonImageViewerViewE6").classList.remove("disabled");
			};
			
			async function getPicsFromRequest(response)
			{
				// Reenable button.
				document.getElementById("buttonImageViewerViewE6").removeAttribute("disabled");
				document.getElementById("buttonImageViewerViewE6").classList.remove("disabled");

				// Handle error responses.
				if (response.status < 200 || response.status > 299)
				{
					if (response.status === 403)
					{
						NotifMessage.displayError("Unable to open the e621 page for this picture.");
					}
					else
					{
						NotifMessage.displayError("Error sending request to e621, website might be down, try again later.");
					}

					return;
				}
				
				let responseJson = await response.json();
				let posts = responseJson.posts;

				if (!posts.length)
				{
					// MD5 Not found.
					NotifMessage.displayError("Picture not found on e621.");
					return;
				}

				// Update the id in the cache and in any list the picture might be in.
				arrImgIdByUrl[imgUrl] = posts[0].id;

				// Update the ID field if the entry is in the Blacklist.
				// (Only the Blacklist is impacted by the structural change).
				if (typeof blacklistedPictures[imgUrl] !== "undefined")
				{
					blacklistedPictures[imgUrl] = {url: imgUrl, id: arrImgIdByUrl[imgUrl]};

					// Save the blacklist to the Local Storage.
					localStorage.setItem("blacklistedPicturesJSON", JSON.stringify(blacklistedPictures));
				}

				// Retry now that we have the ID.
				ImageBrowser.openPicturePage(imgUrl);
			}
			
			sendRequest();
			return;
		}

		var imgPageUrl = E621_URL + "posts/" + arrImgIdByUrl[imgUrl];
		
		window.open(imgPageUrl, "_blank");
	}
};
