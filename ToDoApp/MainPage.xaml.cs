#region Using
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core; 
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement; 
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#endregion

namespace ToDoApp
{
    public sealed partial class MainPage : Page
    {
        // ********** Fields **************************************************
        #region Fields
        MainPageData MPD = null;
        DataType insertDataType = DataType.Category;
        Object previouslySelected = null;
        ToDoItem selectedToDoItem = null;
        ToDoCategory selectedToDoCategory = null;
        ToDoItem overToDoItem = null;
        int insertIndex = 0;
        bool insertAtEnd = false;
        bool canUpdateWidths = true;
        bool manualSave = false;
        bool categoryClickHandled = false;
        StorageFile saveFile = null;
        string fileToken = null;
        string settingsFileName = "ToDoAppSettings.txt";
        StorageFolder settingsFolder = ApplicationData.Current.LocalFolder;
        DispatcherTimer saveTimer = new DispatcherTimer();

        // Properties
        public DataType InsertDataType
        {
            get => insertDataType;
            set
            {
                if (insertDataType != value)
                {
                    insertDataType = value;
                    TextBlockAddType.Text = (insertDataType == DataType.Category) ? "Add Category" : "Add Item";
                }
            }
        }
        #endregion

        // ********** Main Method **************************************************
        #region async Main Method

        public MainPage()
        {
            MPD = new MainPageData();
            this.InitializeComponent();
            this.DataContext = MPD;

            // Draw into the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            // Remove the solid-colored backgrounds from system buttons
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = Colors.DodgerBlue;
            titleBar.ButtonHoverForegroundColor = Colors.White;
            // Set TitleBar to BackgroundElement instead of the entire grid
            Window.Current.SetTitleBar(BackgroundElement);

            LoadSettingsAndModel();

            saveTimer.Interval = new TimeSpan(0, 0, 2);
            saveTimer.Tick += SaveModel;
        }
        #endregion

        // ********** Categories **************************************************
        #region Categories

        private void CategoryHeader_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            (sender as Grid).BorderThickness = new Thickness(5, 1, 5, 1);
            overToDoItem = null;
        }

        private void CategoryHeader_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            (sender as Grid).BorderThickness = new Thickness(1);
        }

        private void CategoryHeader_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            DeselectPrevious();
            previouslySelected = sender;
            selectedToDoCategory = (sender as Grid)?.DataContext as ToDoCategory;
            (sender as Grid).Background = SolidColorBrushFromHex("#99C9EF");
            InsertDataType = DataType.Item;
            insertAtEnd = true;
            categoryClickHandled = true;
            canUpdateWidths = false;
            MPD.WidthSliderValue = selectedToDoCategory.Width;
        }

        private async void CategoryHeader_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (selectedToDoCategory != null)
            {
                TextBoxEditCategory.Text = selectedToDoCategory.CategoryName;

                switch (selectedToDoCategory.DefaultItemType)
                {
                    case ItemType.Once:
                        RadioButtonOnceC.IsChecked = true;
                        break;
                    case ItemType.Common:
                        RadioButtonCommonC.IsChecked = true;
                        break;
                    case ItemType.Toggled:
                        RadioButtonToggledC.IsChecked = true;
                        break;
                    case ItemType.Constant:
                        RadioButtonConstantC.IsChecked = true;
                        break;
                }

                AnimateEditCategoryIn.Begin();
                await Task.Delay(100);
                EditCategoryOverlay.Visibility = Visibility.Visible;

                await Task.Delay(100);
                TextBoxEditCategory.Focus(FocusState.Programmatic);
            }
        }

        private void Category_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            DeselectPrevious();
            selectedToDoCategory = (sender as StackPanel).DataContext as ToDoCategory;
            InsertDataType = DataType.Category;
            categoryClickHandled = true;

            args.Data.SetText("_"); // for e.DataView.Contains(StandardDataFormats.Text)
            args.Data.RequestedOperation = DataPackageOperation.Move;
        }

        private void Category_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
                e.DragUIOverride.IsGlyphVisible = false;
            }
        }

        private void Category_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text) && selectedToDoCategory != null)
            {
                ToDoCategory toCategory = (sender as StackPanel).DataContext as ToDoCategory;

                // moving category
                if (insertDataType == DataType.Category)
                {
                    int index = MPD.ToDoCategories.IndexOf(toCategory);
                    MPD.ToDoCategories.Remove(selectedToDoCategory);
                    MPD.ToDoCategories.Insert(index, selectedToDoCategory);
                }
                // moving item
                else if (insertDataType == DataType.Item && selectedToDoItem != null && selectedToDoItem != overToDoItem)
                {
                    if (overToDoItem != null)
                    {
                        insertIndex = toCategory.ToDoItems.IndexOf(overToDoItem);
                        int fromIndex = selectedToDoCategory.ToDoItems.IndexOf(selectedToDoItem);

                        // correct behavior when dragging from lower to higher index in same list
                        if (selectedToDoCategory.ToDoItems == toCategory.ToDoItems && (insertIndex - fromIndex) > 1)
                        {
                            insertIndex--;
                        }
                    }
                    else
                    {
                        insertIndex = 0;
                    }

                    selectedToDoCategory.ToDoItems.Remove(selectedToDoItem);

                    if (insertAtEnd == true)
                    {
                        toCategory.ToDoItems.Add(selectedToDoItem);
                        insertAtEnd = false;
                    }
                    else
                    {
                        toCategory.ToDoItems.Insert(insertIndex, selectedToDoItem);
                    }
                }
                StartSave();
            }
        }

        private async void CategoryFooter_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            AnimateCommonItemsIn.Begin();
            await Task.Delay(100);
            CommonItemsOverlay.Visibility = Visibility.Visible;

            await Task.Delay(100);
            BtnCommonItemsClose.Focus(FocusState.Programmatic);
        }

        private void CategoryFooter_Drop(object sender, DragEventArgs e)
        {
            insertAtEnd = true;
            overToDoItem = null;
        }
        #endregion

        // ********** ToDoItems **************************************************
        #region ToDoItems

        private void ToDoItem_Click(object sender, ItemClickEventArgs e)
        {
            DeselectPrevious();
            previouslySelected = sender;
            selectedToDoItem = e.ClickedItem as ToDoItem;
            selectedToDoCategory = (sender as ListView).DataContext as ToDoCategory;
            insertIndex = selectedToDoCategory.ToDoItems.IndexOf(selectedToDoItem);
            insertAtEnd = false;
            InsertDataType = DataType.Item;
            canUpdateWidths = false;
            MPD.WidthSliderValue = selectedToDoCategory.Width;
        }

        private async void ToDoItem_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (selectedToDoItem != null)
            {
                TextBoxEditName.Text = selectedToDoItem.ItemName;
                TextBoxEditDescription.Text = selectedToDoItem.ItemDescription;

                switch (selectedToDoItem.ItemType)
                {
                    case ItemType.Once:
                        RadioButtonOnceI.IsChecked = true;
                        break;
                    case ItemType.Common:
                        RadioButtonCommonI.IsChecked = true;
                        break;
                    case ItemType.Toggled:
                        RadioButtonToggledI.IsChecked = true;
                        break;
                    case ItemType.Constant:
                        RadioButtonConstantI.IsChecked = true;
                        break;
                }

                AnimateEditItemIn.Begin();
                await Task.Delay(100);
                EditItemOverlay.Visibility = Visibility.Visible;

                await Task.Delay(100);
                TextBoxEditName.Focus(FocusState.Programmatic);
            }
        }

        private void ToDoItem_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            DeselectPrevious();
            previouslySelected = sender;
            selectedToDoItem = e.Items.First() as ToDoItem;
            selectedToDoCategory = (sender as ListView).DataContext as ToDoCategory;
            insertIndex = selectedToDoCategory.ToDoItems.IndexOf(selectedToDoItem);
            InsertDataType = DataType.Item;
            insertAtEnd = false;
            e.Data.SetText("_");
            e.Data.RequestedOperation = DataPackageOperation.Move;
        }

        private void ToDoItem_DragOver(object sender, DragEventArgs e)
        {
             overToDoItem = (sender as Grid).DataContext as ToDoItem;
        }

        private void ToDoItem_Checked(object sender, RoutedEventArgs e)
        {
            ToDoItem toDoItem = (sender as CheckBox).DataContext as ToDoItem;
            ToDoCategory toDoCategory = CategoryFromItem(toDoItem);

            if (toDoCategory != null && toDoCategory.ToDoItems.Contains(toDoItem))
            {
                if (toDoItem.ItemType == ItemType.Once)
                {
                    toDoCategory.Recycling.Add(toDoItem);
                    toDoCategory.ToDoItems.Remove(toDoItem);
                }
                else if (toDoItem.ItemType == ItemType.Common)
                {
                    toDoCategory.CommonItems.Add(toDoItem);
                    toDoCategory.ToDoItems.Remove(toDoItem);
                }
                else if (toDoItem.ItemType == ItemType.Toggled)
                {
                    toDoItem.ItemComplete = true;
                }
            }
            StartSave();
        }

        private void ToDoItem_Unchecked(object sender, RoutedEventArgs e)
        {
            ToDoItem toDoItem = (sender as CheckBox).DataContext as ToDoItem;

            if (toDoItem != null)
            {
                toDoItem.ItemComplete = false;
                StartSave();
            }
        }
        #endregion

        // ********** Overlay Screens **************************************************
        #region Overlay Screens

        private void OverlayScreen_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                OverlayScreen_Cancel(sender, e);
            }
            else if (e.Key == Windows.System.VirtualKey.Enter)
            {
                EditScreen_Save(sender, e);
            }
        }

        private async void OverlayScreen_Cancel(object sender, RoutedEventArgs e)
        {
            if (EditCategoryOverlay.Visibility == Visibility.Visible)
            {
                AnimateEditCategoryOut.Begin();
                await Task.Delay(200);
                EditCategoryOverlay.Visibility = Visibility.Collapsed;
            }

            if (EditItemOverlay.Visibility == Visibility.Visible)
            {
                AnimateEditItemOut.Begin();
                await Task.Delay(200);
                EditItemOverlay.Visibility = Visibility.Collapsed;
            }

            if (RecyclingBinOverlay.Visibility == Visibility.Visible)
            {
                AnimateRecyclingBinOut.Begin();
                await Task.Delay(200);
                RecyclingBinOverlay.Visibility = Visibility.Collapsed;
            }

            if (CommonItemsOverlay.Visibility == Visibility.Visible)
            {
                AnimateCommonItemsOut.Begin();
                await Task.Delay(200);
                CommonItemsOverlay.Visibility = Visibility.Collapsed;
            }

            FocusDump.Focus(FocusState.Programmatic);
        }

        private void EditScreen_Save(object sender, RoutedEventArgs e)
        {
            if (EditCategoryOverlay.Visibility == Visibility.Visible)
            {
                if (selectedToDoCategory != null)
                {
                    selectedToDoCategory.CategoryName = TextBoxEditCategory.Text;

                    if (RadioButtonOnceC.IsChecked == true)
                    {
                        selectedToDoCategory.DefaultItemType = ItemType.Once;
                    }
                    else if (RadioButtonCommonC.IsChecked == true)
                    {
                        selectedToDoCategory.DefaultItemType = ItemType.Common;
                    }
                    else if (RadioButtonToggledC.IsChecked == true)
                    {
                        selectedToDoCategory.DefaultItemType = ItemType.Toggled;
                    }
                    else if (RadioButtonConstantC.IsChecked == true)
                    {
                        selectedToDoCategory.DefaultItemType = ItemType.Constant;
                    }
                }
            }
            else if (EditItemOverlay.Visibility == Visibility.Visible)
            {
                if (selectedToDoItem != null)
                {
                    selectedToDoItem.ItemName = TextBoxEditName.Text;
                    selectedToDoItem.ItemDescription = TextBoxEditDescription.Text;

                    if (RadioButtonOnceI.IsChecked == true)
                    {
                        selectedToDoItem.ItemType = ItemType.Once;
                    }
                    else if (RadioButtonCommonI.IsChecked == true)
                    {
                        selectedToDoItem.ItemType = ItemType.Common;
                    }
                    else if (RadioButtonToggledI.IsChecked == true)
                    {
                        selectedToDoItem.ItemType = ItemType.Toggled;
                    }
                    else if (RadioButtonConstantI.IsChecked == true)
                    {
                        selectedToDoItem.ItemType = ItemType.Constant;
                    }
                }
            }
            OverlayScreen_Cancel(sender, e);
            StartSave();
        }

        private void RecyclingItem_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is TextBlock)
            {
                var toDoItem = (e.OriginalSource as TextBlock).DataContext as ToDoItem;
                var toDoCategory = (sender as ListView).DataContext as ToDoCategory;
                toDoItem.ItemComplete = false;
                toDoCategory.ToDoItems.Add(toDoItem);
                toDoCategory.Recycling.Remove(toDoItem);
                StartSave();
            }
        }

        private void CommonItem_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is TextBlock)
            {
                var toDoItem = (e.OriginalSource as TextBlock).DataContext as ToDoItem;
                var toDoCategory = (sender as ListView).DataContext as ToDoCategory;
                toDoItem.ItemComplete = false;
                toDoCategory.ToDoItems.Add(toDoItem);
                toDoCategory.CommonItems.Remove(toDoItem);
                StartSave();
            }
        }
        #endregion

        // ********** Menu/Toolbar **************************************************
        #region Menu/Toolbar

        private async void BtnNewFile_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await new ContentDialog
            {
                Title = "New file?",
                Content = (saveFile == null ? "Unsaved data will be lost" : "Current file will be closed."),
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            }.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                saveFile = null;
                fileToken = null;
                UpdateSettings();
                MPD.ToDoCategories = new ObservableCollection<ToDoCategory>();
                SaveModel();
            }
        }

        private async void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker(); // set up file picker
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".json");

            var newSaveFile = await picker.PickSingleFileAsync(); // get file/folder from user
            if (newSaveFile != null)
            {
                saveFile = newSaveFile;
                fileToken = StorageApplicationPermissions.FutureAccessList.Add(saveFile); // token to access the file later
                UpdateSettings(); // save the token to local settings file
                LoadModel(); // load data from save file to the model
            }
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            manualSave = true;
            SaveModel(); // save data from model to the save file
        }

        private async void BtnSaveAsFile_Click(object sender = null, RoutedEventArgs e = null)
        {
            var picker = new FileSavePicker(); // set up file picker
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("Json", new List<string>() { ".json" });

            var newSaveFile = await picker.PickSaveFileAsync(); // get file/folder from user
            if (newSaveFile != null)
            {
                saveFile = newSaveFile;
                fileToken = StorageApplicationPermissions.FutureAccessList.Add(saveFile); // token to access the file later
                UpdateSettings(); // save the token to local settings file
                StartSave(); // save data from model to the save file
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TextBoxAdd.Text))
            {
                string selectedType = (ComboBoxItemType.SelectedItem as ComboBoxItem).Content as string;
                ItemType itemType = ItemType.Once;
                switch (selectedType)
                {
                    case "Default":
                        itemType = (selectedToDoCategory == null ? ItemType.Common : selectedToDoCategory.DefaultItemType);
                        break;
                    case "Constant":
                        itemType = ItemType.Constant;
                        break;
                    case "Common":
                        itemType = ItemType.Common;
                        break;
                    case "Toggled":
                        itemType = ItemType.Toggled;
                        break;
                    case "Once":
                        itemType = ItemType.Once;
                        break;
                }

                // Add Category
                if (InsertDataType == DataType.Category)
                {
                    if (MPD.ToDoCategories.Count > 0)
                    {
                        MPD.ToDoCategories.Add(new ToDoCategory(TextBoxAdd.Text, MPD.ToDoCategories[0].Width, itemType));
                    }
                    else
                    {
                        MPD.ToDoCategories.Add(new ToDoCategory(TextBoxAdd.Text, (int)SliderWidth.Value, itemType));
                    }
                }
                // Add Item
                else // InsertType == InsertType.Item
                {
                    if (insertAtEnd == true)
                    {
                        selectedToDoCategory?.ToDoItems.Add(new ToDoItem(TextBoxAdd.Text, "", itemType));
                    }
                    else
                    {
                        selectedToDoCategory?.ToDoItems.Insert(insertIndex, new ToDoItem(TextBoxAdd.Text, "", itemType));

                        if (previouslySelected is ListView)
                        {
                            (previouslySelected as ListView).SelectedIndex = insertIndex;
                        }
                    }
                }
                TextBoxAdd.Text = "";
                TextBoxAdd.Focus(FocusState.Programmatic);
                StartSave();
            }    
        }

        private void TextBoxAdd_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                AddBtn_Click(sender, e);
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            DeselectPrevious();
            TextBoxAdd.Text = "";
            InsertDataType = DataType.Category;
        }

        private void WidthSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (canUpdateWidths)
            {
                if (selectedToDoCategory != null)
                {
                    selectedToDoCategory.Width = (int)e.NewValue;
                }
                else
                {
                    foreach (var toDoCategory in MPD.ToDoCategories)
                    {
                        toDoCategory.Width = (int)e.NewValue;
                    }
                }
                StartSave();
            }
            canUpdateWidths = true;
        }

        private void Bg_Deselect(object sender = null, PointerRoutedEventArgs e = null)
        {
            if (!categoryClickHandled)
            {
                DeselectPrevious();
                InsertDataType = DataType.Category;
                selectedToDoCategory = null;
                canUpdateWidths = false;
                MPD.WidthSliderValue = (MPD.ToDoCategories.Count() > 0 ? MPD.ToDoCategories[0].Width : 160);
            }
            categoryClickHandled = false;
        }
        #endregion

        // ********** Trash/Recycling **************************************************
        #region Trash/Recycling

        private void Trash_DragEnter(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
                e.DragUIOverride.Caption = "Delete";
                e.DragUIOverride.IsGlyphVisible = false;
                e.DragUIOverride.IsContentVisible = false;
                Trash.Background = new SolidColorBrush(Colors.Red);
                TrashIcon.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void Trash_DragLeave(object sender, DragEventArgs e)
        {
            Trash.Background = new SolidColorBrush(Colors.LightGray);
            TrashIcon.Foreground = SolidColorBrushFromHex("#555");
        }

        private async void Trash_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                Trash_DragLeave(sender, e);

                ContentDialogResult result = await new ContentDialog
                {
                    Title = "Continue?",
                    Content = "This will perminantly delete the selected item!",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No"
                }.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (insertDataType == DataType.Category && selectedToDoCategory != null)
                    {
                        MPD.ToDoCategories.Remove(selectedToDoCategory);
                    }
                    else if (selectedToDoItem != null && selectedToDoCategory != null)
                    {
                        selectedToDoCategory.ToDoItems.Remove(selectedToDoItem);
                    }
                    categoryClickHandled = false;
                    Bg_Deselect();
                    StartSave();
                }
            }
        }

        private async void TrashIcon_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            RecyclingBinOverlay.Visibility = Visibility.Visible;

            AnimateRecyclingBinIn.Begin();
            await Task.Delay(100);
            BtnRecyclingClose.Focus(FocusState.Programmatic);
        }
        #endregion

        // ********** Other **************************************************
        #region Other

        private void DeselectPrevious()
        {
            if (previouslySelected is Grid)
            {
                (previouslySelected as Grid).Background = SolidColorBrushFromHex("#F0F8FF");
            }
            else if (previouslySelected is ListView)
            {
                (previouslySelected as ListView).SelectedItem = null;
                insertIndex = 0;
            }
            previouslySelected = null;
        }

        private SolidColorBrush SolidColorBrushFromHex(string colorStr)
        {
            colorStr = colorStr.Replace("#", string.Empty);
            if (colorStr.Length == 3)
            {
                string stR = colorStr.Substring(0, 1);
                string stG = colorStr.Substring(1, 1);
                string stB = colorStr.Substring(2, 1);
                colorStr = $"{stR}{stR}{stG}{stG}{stB}{stB}";
            }
            if (colorStr.Length == 6)
            {
                var r = (byte)System.Convert.ToUInt32(colorStr.Substring(0, 2), 16);
                var g = (byte)System.Convert.ToUInt32(colorStr.Substring(2, 2), 16);
                var b = (byte)System.Convert.ToUInt32(colorStr.Substring(4, 2), 16);
                Color color = Color.FromArgb(255, r, g, b);
                return new SolidColorBrush(color);
            }
            return new SolidColorBrush(Colors.Black);
        }

        private ToDoCategory CategoryFromItem(ToDoItem toDoItem)
        {
            ToDoCategory result = null;
            foreach (var toDoCategory in MPD.ToDoCategories)
            {
                if (toDoCategory.ToDoItems.Contains(toDoItem))
                {
                    result = toDoCategory;
                    break;
                }
            }
            return result;
        }

        private async void LoadSettingsAndModel()
        {
            try // try using saved file token to load last used file
            {
                StorageFile settingsFile = await settingsFolder.GetFileAsync(settingsFileName); 
                fileToken = await FileIO.ReadTextAsync(settingsFile); 

                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(fileToken))
                {
                    saveFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(fileToken);
                    LoadModel();
                }
                else throw new Exception();
            }
            catch (Exception) // if no previous file could be loaded
            {
                saveFile = null;
                LoadModel();
            }
        }

        private async void UpdateSettings()
        {
            try // overwrite settings file with token pointing to current open file
            {
                StorageFile settingsFile = await settingsFolder.CreateFileAsync(settingsFileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(settingsFile, (fileToken ?? "null")); // write folder token to settings file
                MPD.TitlebarText = (saveFile?.DisplayName ?? "Unsaved File");
            }
            catch (Exception)
            {
                await new MessageDialog("Could not save settings.").ShowAsync();
                TxtFooter.Text = "Could not save settings.";
            }
        }

        private async void LoadModel()
        {
            
            if (saveFile != null)
            {
                try // load json from file and initialize model
                {
                    string jsonString = await FileIO.ReadTextAsync(saveFile);
                    MPD = JsonConvert.DeserializeObject<MainPageData>(jsonString);
                    this.DataContext = MPD;
                    MPD.TitlebarColor = "WhiteSmoke";
                }
                catch (Exception)
                {
                    await new MessageDialog("Error loading json from saved file.").ShowAsync();
                    TxtFooter.Text = "Error loading json from saved file.";
                    MPD.TitlebarColor = "Red";
                }
                MPD.TitlebarText = saveFile.DisplayName;
            }
            else // load UnsavedFile
            {
                try
                {
                    StorageFile unsavedFile = await settingsFolder.GetFileAsync("UnsavedFile.json");
                    string jsonString = await FileIO.ReadTextAsync(unsavedFile);
                    var newMPD = JsonConvert.DeserializeObject<MainPageData>(jsonString);
                    if (newMPD != null)
                    {
                        MPD = newMPD;
                        this.DataContext = MPD;
                    }
                }
                catch (Exception)
                {
                    // no need to do anything here, it just means nothing gets loaded. Ex: running the program for the first time
                }
                MPD.TitlebarColor = "#FDA";
            }
        }

        private async void SaveModel(object sender = null, object e = null)
        {
            string jsonString = JsonConvert.SerializeObject(MPD, Formatting.Indented);

            if (saveFile == null) // save to "UnsavedFile"
            {
                MPD.TitlebarColor = "#FDA";
                if (manualSave) // if manually saving, prompt to Save As
                {
                    manualSave = false;
                    BtnSaveAsFile_Click();
                }
                else // saving to "UnsavedFile" in case of accidental program termination before selecting a file to save
                {
                    try
                    {
                        StorageFile unsavedFile = await settingsFolder.CreateFileAsync("UnsavedFile.json", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(unsavedFile, jsonString);
                        TxtFooter.Text = "Saving...";
                        await Task.Delay(1000);
                        TxtFooter.Text = "...";
                    }
                    catch (Exception)
                    {
                        // failed save to UnsavedFile doesn't warrant alerting user, but could go in a debug log if one gets implimented
                    }
                }
            }
            else if (saveFile.DisplayName != "SampleData") // save to current open file if not using sample data
            {
                try
                {
                    await FileIO.WriteTextAsync(saveFile, jsonString); 
                    TxtFooter.Text = "Saving...";
                    await Task.Delay(1000);
                    TxtFooter.Text = "...";
                    MPD.TitlebarColor = "WhiteSmoke";
                }
                catch (Exception)
                {
                    TxtFooter.Text = "Error saving file.";
                    MPD.TitlebarColor = "Red";
                }
                manualSave = false;
            }
            
            saveTimer.Stop();
        }

        private void StartSave()
        {
            saveTimer.Stop();
            saveTimer.Start();
        }
        #endregion

    }
}