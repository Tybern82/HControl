using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using com.tybern.CMDProcessor;
using HControl.Devices;
using HControl.Game;
using HControl.ViewModels;

namespace HControl.Views;

public partial class MainView : UserControl {

    protected static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

    public MainViewModel ViewModel { get; } = new MainViewModel();

    private readonly Dictionary<string, HCGameModule> modules = [];

    public MainView()
    {
        InitializeComponent();
        this.DataContext = ViewModel;


        var imgIconCancel = new Avalonia.Media.Imaging.Bitmap("./Assets/img/iconCancel.png");
        iconCancel.Source = imgIconCancel;

        loadUIImage();

        btnMale.IsChecked = SettingsStore.Instance.isMaleUI;
        btnFemale.IsChecked = !SettingsStore.Instance.isMaleUI;
        btnMale.Click += (sender, args) => {
            SettingsStore.Instance.isMaleUI = btnMale.IsChecked ?? true;
            btnFemale.IsChecked = !btnMale.IsChecked;
            loadUIImage();
        };
        btnFemale.Click += (sender, args) => {
            SettingsStore.Instance.isMaleUI = !btnFemale.IsChecked ?? true;
            btnMale.IsChecked = !btnFemale.IsChecked;
            loadUIImage();
        };

        ViewModel.CurrentState = MainViewModel.StateIndicator.PAUSE;

        btnClose.Click += (sender, args) => ViewModel.triggerOnCancelClicked();

        btnRemoveFolder.Click += (sender, args) => {
            if (dataImages.SelectedItem != null) {
                if (dataImages.SelectedItem is ImageFolderListItem itemToRemove) ViewModel.CommonParameters.ImageList.Remove(itemToRemove);
            }
        };

        btnRemoveFFolder.Click += (sender, args) => {
            if (dataFinish.SelectedItem != null) {
                if (dataFinish.SelectedItem is ImageFolderListItem itemToRemove) ViewModel.CommonParameters.FinishList.Remove(itemToRemove);
            }
        };

        btnAddFolder.Click += async (sender, args) => {
            var topLevel = TopLevel.GetTopLevel(this);
            
            if (topLevel != null) {
                var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
                    Title = "Add Image Folder",
                    SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Pictures)
                });
                if (folders is not null) {
                    foreach (var item in folders) {
                        if (item is not null) {
                            string fName = item.TryGetLocalPath() ?? Path.Combine(item.Path.AbsolutePath, item.Name);
                            ViewModel.CommonParameters.ImageList.Add(new ImageFolderListItem(fName));
                        }
                    }
                }
            }
        };

        btnAddFFolder.Click += async (sender, args) => {
            var topLevel = TopLevel.GetTopLevel(this);

            if (topLevel != null) {
                var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
                    Title = "Add Finish Folder",
                    SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Pictures)
                });
                if (folders is not null) {
                    foreach (var item in folders) {
                        if (item is not null) {
                            string fName = item.TryGetLocalPath() ?? Path.Combine(item.Path.AbsolutePath, item.Name);
                            ViewModel.CommonParameters.FinishList.Add(new ImageFolderListItem(fName));
                        }
                    }
                }
            }
        };

        startSelected.Click += (sender, args) => {
            var selected = tcSettings.SelectedItem;
            if (selected != null && selected is TabItem tab) {
                if (tab.Header is string header) {
                    HCGameModule? module = modules[header];
                    module?.startGame(module.getSettingsPage(ViewModel.CommonParameters).GameParameters, ViewModel);
                }
            }
        };
    }

    private void loadUIImage() {
        var imgMainImage = SettingsStore.Instance.isMaleUI ? new Avalonia.Media.Imaging.Bitmap("./Assets/img/intro_Lucas.png") : new Avalonia.Media.Imaging.Bitmap("./Assets/img/intro_Lucia.png");
        imgMain.Source = imgMainImage;
    }

    public TabItem registerModule(HCGameModule module) {
        modules.Add(module.ModuleName, module);
        TabItem newSettingsTab = new() {
            Header = module.ModuleName,
            Content = module.getSettingsPage(ViewModel.CommonParameters)
        };
        tcSettings.Items.Add(newSettingsTab);
        tcSettings.SelectedItem = newSettingsTab;
        return newSettingsTab;
    }
}
